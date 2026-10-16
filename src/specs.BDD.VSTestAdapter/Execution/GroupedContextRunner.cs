using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using specs.BDD.VSTestAdapter.Conversion;
using specs.BDD.VSTestAdapter.Discovery;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// Executes an entire root [Context] subtree as a single grouped test result.
    /// All It[...] tests under the context run together; the full contextual tree
    /// (contexts, tests, pass/fail markers, failure details) is published to the
    /// result's standard output, in the style of NSpec's console formatter.
    /// </summary>
    /// <remarks>
    /// Lifecycle semantics mirror <see cref="SelectionTreeWalker"/>: each node's
    /// <c>OwnSetupOnce</c>/<c>OwnTeardownOnce</c> fires exactly once; the cascaded
    /// <c>SetupEach</c>, <c>Act</c>, and <c>TeardownEach</c> run around every test.
    /// Unlike per-test mode, a per-test lifecycle failure is counted as that test's
    /// failure and the run continues, so the grouped report is always complete.
    /// </remarks>
    internal static class GroupedContextRunner
    {
        /// <summary>
        /// Runs the grouped <paramref name="testCase"/> against the pre-built suite tree
        /// and records the single aggregated result.
        /// </summary>
        public static void Run(
            TestCase testCase,
            IContextualTestCase suiteRoot,
            IFrameworkHandle frameworkHandle,
            CancellationToken cancellationToken)
        {
            frameworkHandle.RecordStart(testCase);
            var result = Execute(testCase, suiteRoot, cancellationToken);
            frameworkHandle.RecordResult(result);
        }

        /// <summary>
        /// Executes the grouped context subtree and returns the aggregated
        /// <see cref="TestResult"/> without recording it (unit-testable core).
        /// </summary>
        internal static TestResult Execute(
            TestCase testCase,
            IContextualTestCase suiteRoot,
            CancellationToken cancellationToken)
        {
            var contextName = testCase.GetPropertyValue<string>(TestProperties.ContextPath, null) ?? "";
            var startTime = DateTimeOffset.UtcNow;
            var timer = Stopwatch.StartNew();

            var node = suiteRoot.ChildContexts
                .FirstOrDefault(c => string.Equals(c.Description, contextName, StringComparison.Ordinal));

            if (node == null)
            {
                timer.Stop();
                return ResultReporter.Failure(
                    testCase,
                    new InvalidOperationException(
                        $"Context '{contextName}' no longer exists in the current build."),
                    startTime,
                    timer.Elapsed);
            }

            var state = new RunState();
            RunNode(node, contextName, depth: 0, state, cancellationToken);
            timer.Stop();

            return BuildResult(testCase, state, startTime, timer.Elapsed);
        }

        // ── Subtree execution ──────────────────────────────────────────────────────────────────

        private sealed class RunState
        {
            public readonly StringBuilder Output = new StringBuilder();
            public readonly List<(string Path, Exception Cause)> Failures =
                new List<(string, Exception)>();
            public int Total;
            public int Passed;
        }

        private static void RunNode(
            ITestCaseContext node,
            string path,
            int depth,
            RunState state,
            CancellationToken cancellationToken)
        {
            state.Output.AppendLine($"{Indent(depth)}{node.Description}");

            try
            {
                node.OwnSetupOnce();
            }
            catch (Exception ex)
            {
                FailSubtree(node, path, depth + 1, ResultReporter.Unwrap(ex), state);
                return;
            }

            try
            {
                foreach (var (description, test) in node.Tests)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    RunTest(node, path, depth + 1, description, test, state);
                }

                // ChildContexts is exposed by the concrete node type via IContextualTestCase.
                if (node is IContextualTestCase contextualNode)
                {
                    foreach (var child in contextualNode.ChildContexts)
                    {
                        if (cancellationToken.IsCancellationRequested) return;
                        var childPath = path + TestStructureExtractor.PathSeparator + child.Description;
                        RunNode(child, childPath, depth + 1, state, cancellationToken);
                    }
                }
            }
            finally
            {
                // Best-effort AfterAll — swallow so we don't mask test failures.
                try { node.OwnTeardownOnce(); } catch { /* best-effort */ }
            }
        }

        private static void RunTest(
            ITestCase node,
            string path,
            int depth,
            string description,
            Action test,
            RunState state)
        {
            state.Total++;

            Exception? failure = null;
            try
            {
                node.SetupEach();
                node.Act();
                test();
            }
            catch (Exception ex)
            {
                failure = ex;
            }

            try { node.TeardownEach(); }
            catch (Exception ex) { failure ??= ex; }

            if (failure == null)
            {
                state.Passed++;
                state.Output.AppendLine($"{Indent(depth)}✓ {description}");
            }
            else
            {
                var cause = ResultReporter.Unwrap(failure);
                state.Failures.Add((path + TestStructureExtractor.PathSeparator + description, cause));
                state.Output.AppendLine($"{Indent(depth)}✗ {description} - FAILED:");
                state.Output.AppendLine(IndentMessage(cause.Message, depth + 1));
            }
        }

        /// <summary>
        /// Marks every test in the subtree as failed with the given cause (used when a
        /// context's BeforeAll throws, so its tests can never run).
        /// </summary>
        private static void FailSubtree(
            ITestCaseContext node,
            string path,
            int depth,
            Exception cause,
            RunState state)
        {
            foreach (var (description, _) in node.Tests)
            {
                state.Total++;
                state.Failures.Add((path + TestStructureExtractor.PathSeparator + description, cause));
                state.Output.AppendLine($"{Indent(depth)}✗ {description} - FAILED: BeforeAll failed:");
                state.Output.AppendLine(IndentMessage(cause.Message, depth + 1));
            }

            if (node is IContextualTestCase contextualNode)
            {
                foreach (var child in contextualNode.ChildContexts)
                {
                    state.Output.AppendLine($"{Indent(depth)}{child.Description}");
                    var childPath = path + TestStructureExtractor.PathSeparator + child.Description;
                    FailSubtree(child, childPath, depth + 1, cause, state);
                }
            }
        }

        // ── Result assembly ────────────────────────────────────────────────────────────────────

        private static TestResult BuildResult(
            TestCase testCase,
            RunState state,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var failed = state.Failures.Count;

            var output = state.Output;
            output.AppendLine();
            output.AppendLine($"{state.Passed} passed, {failed} failed, {state.Total} total");

            if (failed > 0)
            {
                output.AppendLine();
                output.AppendLine("Failures:");
                for (var i = 0; i < state.Failures.Count; i++)
                {
                    var (path, cause) = state.Failures[i];
                    output.AppendLine();
                    output.AppendLine($"{i + 1}) {path}");
                    output.AppendLine(cause.Message);
                    if (!string.IsNullOrEmpty(cause.StackTrace))
                        output.AppendLine(cause.StackTrace);
                }
            }

            var result = new TestResult(testCase)
            {
                StartTime = startTime,
                EndTime = startTime + duration,
                Duration = duration
            };

            if (state.Total == 0)
            {
                result.Outcome = TestOutcome.Skipped;
                result.ErrorMessage = "No specs found under this context.";
            }
            else if (failed == 0)
            {
                result.Outcome = TestOutcome.Passed;
            }
            else
            {
                result.Outcome = TestOutcome.Failed;
                var summary = new StringBuilder();
                summary.AppendLine($"{failed} of {state.Total} specs failed:");
                foreach (var (path, cause) in state.Failures)
                {
                    summary.AppendLine($"  ✗ {path}:");
                    summary.AppendLine(IndentMessage(cause.Message, 2));
                }
                result.ErrorMessage = summary.ToString().TrimEnd();
                result.ErrorStackTrace = string.Join(
                    Environment.NewLine + Environment.NewLine,
                    state.Failures.Select(f => f.Cause.StackTrace).Where(s => !string.IsNullOrEmpty(s)));
            }

            result.Messages.Add(new TestResultMessage(
                TestResultMessage.StandardOutCategory,
                output.ToString()));

            return result;
        }

        private static string Indent(int depth) => new string(' ', depth * 2);

        /// <summary>Indents every line of a (possibly multi-line) message under the given depth.</summary>
        private static string IndentMessage(string message, int depth)
        {
            var indent = Indent(depth);
            var lines = message.Trim('\r', '\n').Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            return string.Join(Environment.NewLine, lines.Select(line => indent + line));
        }
    }
}
