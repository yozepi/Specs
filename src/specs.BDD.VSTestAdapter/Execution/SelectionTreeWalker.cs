using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using specs.BDD.VSTestAdapter.Conversion;
using specs.core.Framework.Adapters;
using specs.core.Framework.Processors.TestCases;
using specs.core.Framework.Processors.TestExecutors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// Walks a <see cref="IContextualTestCase"/> suite tree depth-first, firing each live
    /// node's <see cref="ITestCase.OwnSetupOnce"/> / <see cref="ITestCase.OwnTeardownOnce"/>
    /// exactly once, regardless of how many tests are selected within its subtree.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A node is <em>live</em> when at least one selected test lives at or beneath it.
    /// Only live nodes receive lifecycle calls; idle branches are skipped entirely.
    /// </para>
    /// <para>
    /// Failure contract: if <c>OwnSetupOnce</c> throws at node N, every selected test at or
    /// under N is reported as failed with that exception. N's own <c>OwnTeardownOnce</c> is
    /// NOT called. The parent continues processing sibling branches normally and calls its own
    /// <c>OwnTeardownOnce</c> on the way out.
    /// </para>
    /// <para>
    /// The cascaded <c>SetupEach</c>, <c>Act</c>, and <c>TeardownEach</c> on each leaf node
    /// still run around every individual test — that responsibility is delegated to
    /// <see cref="TestCaseExecutor"/> with no-op once-actions.
    /// </para>
    /// </remarks>
    internal sealed class SelectionTreeWalker
    {
        private readonly IContextualTestCase _root;
        private readonly string _rootPath;
        private readonly IFrameworkHandle _frameworkHandle;
        private readonly CancellationToken _cancellationToken;

        // contextPath → tests whose direct parent is exactly at that path.
        private readonly Dictionary<string, List<TestCase>> _testsByPath;

        // Every context path that is an ancestor-or-self of at least one selected test.
        private readonly HashSet<string> _livePaths;

        /// <summary>
        /// Initialises the walker and pre-indexes the selected tests.
        /// </summary>
        /// <param name="root">The suite-tree root built by <see cref="Discovery.SuiteTreeBuilder"/>.</param>
        /// <param name="rootPath">
        /// The path string that corresponds to <paramref name="root"/>, i.e. the simple class name
        /// used as the first segment of every <see cref="TestProperties.ContextPath"/> value.
        /// </param>
        /// <param name="tests">The selected <see cref="TestCase"/> objects to run.</param>
        /// <param name="frameworkHandle">VSTest handle for recording results.</param>
        /// <param name="cancellationToken">Token to honour cancel requests between tests.</param>
        public SelectionTreeWalker(
            IContextualTestCase root,
            string rootPath,
            IEnumerable<TestCase> tests,
            IFrameworkHandle frameworkHandle,
            CancellationToken cancellationToken)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
            _frameworkHandle = frameworkHandle ?? throw new ArgumentNullException(nameof(frameworkHandle));
            _cancellationToken = cancellationToken;

            _testsByPath = new Dictionary<string, List<TestCase>>(StringComparer.Ordinal);
            _livePaths = new HashSet<string>(StringComparer.Ordinal);

            foreach (var tc in tests)
            {
                var path = tc.GetPropertyValue<string>(TestProperties.ContextPath, null) ?? "";
                if (!_testsByPath.TryGetValue(path, out var bucket))
                {
                    bucket = new List<TestCase>();
                    _testsByPath[path] = bucket;
                }

                bucket.Add(tc);
                MarkAncestorsLive(path);
            }
        }

        /// <summary>
        /// Starts the depth-first walk from the root node.
        /// </summary>
        public void Walk() => WalkNode(_root, _rootPath);

        // ── Core DFS ──────────────────────────────────────────────────────────────────────────

        private void WalkNode(IContextualTestCase node, string nodePath)
        {
            if (_cancellationToken.IsCancellationRequested) return;
            if (!_livePaths.Contains(nodePath)) return;

            // The root's once-lifecycle is run by TestInvoker.InvokeSuite, not the walker.
            bool isRoot = string.IsNullOrEmpty(nodePath);

            if (!isRoot)
            {
                // Fire this node's own BeforeAll.  On failure: fail subtree, skip teardown, return.
                try
                {
                    node.OwnSetupOnce();
                }
                catch (Exception ex)
                {
                    FailSubtree(nodePath, ex);
                    return;
                }
            }

            try
            {
                // Run tests that live directly at this node's context level.
                if (_testsByPath.TryGetValue(nodePath, out var directTests))
                    RunDirectTests(node, directTests);

                // Recurse into child contexts that contain selected tests.
                foreach (var child in node.ChildContexts)
                {
                    if (_cancellationToken.IsCancellationRequested) break;

                    // When nodePath is empty (root), child paths are just the child description.
                    var childPath = string.IsNullOrEmpty(nodePath)
                        ? child.Description
                        : nodePath + ContextPathFormat.Separator + child.Description;

                    if (_livePaths.Contains(childPath) && child is IContextualTestCase contextualChild)
                        WalkNode(contextualChild, childPath);
                }
            }
            finally
            {
                if (!isRoot)
                {
                    // Best-effort AfterAll — swallow so we don't mask test failures.
                    try { node.OwnTeardownOnce(); } catch { /* best-effort */ }
                }
            }
        }

        // ── Per-node test loop ─────────────────────────────────────────────────────────────────

        private void RunDirectTests(ITestCase node, List<TestCase> directTests)
        {
            // Match each selected TestCase to its action from the built tree.
            var actionByDescription = node.Tests
                .ToDictionary(t => t.description, t => t.test, StringComparer.Ordinal);

            var triples = new List<(TestCase testCase, string description, Action testAction)>(directTests.Count);
            var testCaseMap = new Dictionary<string, TestCase>(directTests.Count, StringComparer.Ordinal);
            var startTime = DateTimeOffset.UtcNow;

            foreach (var tc in directTests)
            {
                var desc = tc.GetPropertyValue<string>(TestProperties.TestDescription, null) ?? "";
                if (actionByDescription.TryGetValue(desc, out var testAction))
                {
                    triples.Add((tc, desc, testAction));
                    testCaseMap[desc] = tc;
                }
                else
                {
                    // Test was removed since discovery — fail immediately without touching lifecycle.
                    _frameworkHandle.RecordStart(tc);
                    _frameworkHandle.RecordResult(ResultReporter.Failure(
                        tc,
                        new InvalidOperationException($"Test '{desc}' no longer exists in the current build."),
                        startTime,
                        TimeSpan.Zero));
                }
            }

            if (triples.Count == 0) return;

            var logger = new VSTestLoggingAdapter(_frameworkHandle, testCaseMap);
            var testProvider = new VSTestTestProvider(triples, logger);

            // OwnSetupOnce / OwnTeardownOnce are already handled by the walker.
            // Cascaded SetupEach, Act, TeardownEach still run correctly around every test.
            var contextAdapter = new TestContextAdapter(
                setupOnce:    () => { },
                setupEach:    node.SetupEach,
                act:          node.Act,
                teardownEach: node.TeardownEach,
                teardownOnce: () => { });

            var settings = new TestCaseExecutorSettings { StopOnAssertFailure = false };
            var executor = new TestCaseExecutor(settings, logger, contextAdapter, testProvider);

            Exception? executionFailure = null;
            try
            {
                executor.ProcessTestCase(_cancellationToken);
            }
            catch (Exception ex)
            {
                executionFailure = ex;
            }

            ReportUnreportedAsFailed(triples, logger, executionFailure);
        }

        // ── Failure helpers ────────────────────────────────────────────────────────────────────

        private void FailSubtree(string nodePath, Exception ex)
        {
            var startTime = DateTimeOffset.UtcNow;
            foreach (var (path, tests) in _testsByPath)
            {
                if (IsAtOrUnder(path, nodePath))
                {
                    foreach (var tc in tests)
                    {
                        _frameworkHandle.RecordStart(tc);
                        _frameworkHandle.RecordResult(
                            ResultReporter.Failure(tc, ex, startTime, TimeSpan.Zero));
                    }
                }
            }
        }

        private void ReportUnreportedAsFailed(
            List<(TestCase testCase, string description, Action testAction)> triples,
            VSTestLoggingAdapter logger,
            Exception? executionFailure)
        {
            var startTime = DateTimeOffset.UtcNow;
            var cause = executionFailure
                ?? new InvalidOperationException("Test was not reached due to a lifecycle failure.");

            foreach (var (testCase, description, _) in triples)
            {
                if (!logger.ReportedDescriptions.Contains(description))
                {
                    _frameworkHandle.RecordStart(testCase);
                    _frameworkHandle.RecordResult(
                        ResultReporter.Failure(testCase, cause, startTime, TimeSpan.Zero));
                }
            }
        }

        // ── Path utilities ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when <paramref name="path"/> equals <paramref name="prefix"/> or
        /// starts with "<paramref name="prefix"/> → ".
        /// </summary>
        private static bool IsAtOrUnder(string path, string prefix) =>
            string.IsNullOrEmpty(prefix) || // empty = root = every path is under it
            path == prefix ||
            path.StartsWith(prefix + ContextPathFormat.Separator, StringComparison.Ordinal);

        /// <summary>
        /// Marks every ancestor path of <paramref name="path"/> (including itself) as live.
        /// </summary>
        private void MarkAncestorsLive(string path)
        {
            // Always mark the root (empty string) as live so WalkNode can enter the tree.
            _livePaths.Add("");

            var parts = path.Split(ContextPathFormat.Separator, StringSplitOptions.None);
            var prefix = "";
            for (int i = 0; i < parts.Length; i++)
            {
                prefix = i == 0 ? parts[0] : prefix + ContextPathFormat.Separator + parts[i];
                _livePaths.Add(prefix);
            }
        }
    }
}
