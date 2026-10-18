using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using specs.BDD.VSTestAdapter.Conversion;
using specs.BDD.VSTestAdapter.Discovery;
using specs.core.Framework.Processors.TestCases;
using specs.core.Framework.Processors.TestExecutors;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// Loads the assembly for a <see cref="TestCase"/>, rebuilds the full specification
    /// suite tree via <see cref="SuiteTreeBuilder"/>, and runs the requested tests.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For single-test execution (<see cref="Invoke"/>), the suite tree is built and
    /// <see cref="TestFinder"/> navigates to the exact test action, which is then run with
    /// the cascaded lifecycle (SetupOnce through TeardownOnce).
    /// </para>
    /// <para>
    /// For suite execution (<see cref="InvokeSuite"/>), a <see cref="SelectionTreeWalker"/>
    /// fires each node's <c>BeforeAll</c> / <c>AfterAll</c> exactly once and delegates
    /// per-test loops to <see cref="specs.core.Framework.Processors.TestExecutors.TestCaseExecutor"/>.
    /// </para>
    /// </remarks>
    internal static class TestInvoker
    {
        // â”€â”€ Single-test execution â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// Executes a single <see cref="TestCase"/> end-to-end and returns a
        /// <see cref="TestResult"/> with the outcome.
        /// </summary>
        public static TestResult Invoke(TestCase testCase)
        {
            var startTime = DateTimeOffset.UtcNow;
            var timer = Stopwatch.StartNew();

            try
            {
                var ran = InvokeCore(testCase);
                timer.Stop();
                return ran
                    ? ResultReporter.Success(testCase, startTime, timer.Elapsed)
                    : ResultReporter.Skipped(testCase, startTime, timer.Elapsed);
            }
            catch (Exception ex)
            {
                timer.Stop();
                return ResultReporter.Failure(testCase, ex, startTime, timer.Elapsed);
            }
        }

        // â”€â”€ Suite execution â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// Executes all <paramref name="tests"/> from a single specification class.
        /// The suite tree is built once and a <see cref="SelectionTreeWalker"/> ensures
        /// each node's <c>BeforeAll</c> / <c>AfterAll</c> fires exactly once.
        /// </summary>
        internal static void InvokeSuite(
            string source,
            string className,
            IEnumerable<TestCase> tests,
            IFrameworkHandle frameworkHandle,
            bool showSourceLocations,
            MarkerStyle markerStyle,
            CancellationToken cancellationToken)
        {
            var testList = new List<TestCase>(tests);
            if (testList.Count == 0) return;

            Type specType;
            string rootPath;
            specs.core.Framework.Processors.TestCases.IContextualTestCase root;
            try
            {
                specType = ResolveSpecType(source, className);
                rootPath = ""; // root node has no path; [Context] children are the first level
                root = SuiteTreeBuilder.BuildSuite(specType);
            }
            catch (Exception buildEx)
            {
                ReportAllAsFailed(testList, frameworkHandle, buildEx);
                return;
            }

            // Grouped test cases run their whole context subtree as one result; the rest go
            // through the per-test selection walker. Both share the same suite tree instance.
            // Pending tests are reported up front and never touch any lifecycle.
            var groupedTests = new List<TestCase>();
            var regularTests = new List<TestCase>();
            var pendingTests = new List<TestCase>();
            foreach (var tc in testList)
            {
                if (tc.GetPropertyValue(TestProperties.IsGrouped, false))
                    groupedTests.Add(tc);
                else if (IsPending(root, tc))
                    pendingTests.Add(tc);
                else
                    regularTests.Add(tc);
            }

            var pendingStart = DateTimeOffset.UtcNow;
            foreach (var tc in pendingTests)
            {
                frameworkHandle.RecordStart(tc);
                frameworkHandle.RecordResult(ResultReporter.Skipped(tc, pendingStart, TimeSpan.Zero));
            }

            // Root BeforeAll/AfterAll come from the spec class constructor and run once per class,
            // but only when at least one selected spec will actually execute.
            var runRootLifecycle = regularTests.Count > 0
                || groupedTests.Any(tc => GroupedContextHasRunnableTests(root, tc));

            Exception? rootSetupFailure = null;
            if (runRootLifecycle)
            {
                try
                {
                    root.OwnSetupOnce();
                }
                catch (Exception ex)
                {
                    rootSetupFailure = ex;
                }
            }

            try
            {
                foreach (var groupedTest in groupedTests)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    GroupedContextRunner.Run(groupedTest, root, frameworkHandle, rootSetupFailure, showSourceLocations, markerStyle, cancellationToken);
                }

                if (regularTests.Count == 0) return;

                if (rootSetupFailure != null)
                {
                    ReportAllAsFailed(regularTests, frameworkHandle, rootSetupFailure);
                    return;
                }

                var walker = new SelectionTreeWalker(root, rootPath, regularTests, frameworkHandle, cancellationToken);
                walker.Walk();
            }
            finally
            {
                if (runRootLifecycle && rootSetupFailure == null)
                {
                    // Best-effort AfterAll — swallow so we don't mask test failures.
                    try { root.OwnTeardownOnce(); } catch { /* best-effort */ }
                }
            }
        }

        // â”€â”€ Private helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>Returns <c>false</c> without running anything when the test is pending.</summary>
        private static bool InvokeCore(TestCase testCase)
        {
            var className = testCase.GetPropertyValue<string>(TestProperties.ClassName, null)
                ?? throw new InvalidOperationException(
                    "TestCase is missing the ClassName metadata property.");

            var contextPath = testCase.GetPropertyValue<string>(TestProperties.ContextPath, null)
                ?? throw new InvalidOperationException(
                    "TestCase is missing the ContextPath metadata property.");

            var testDescription = testCase.GetPropertyValue<string>(TestProperties.TestDescription, null)
                ?? throw new InvalidOperationException(
                    "TestCase is missing the TestDescription metadata property.");

            var specType = ResolveSpecType(testCase.Source, className);
            var root = SuiteTreeBuilder.BuildSuite(specType);

            var found = TestFinder.Find(root, contextPath, testDescription)
                ?? throw new InvalidOperationException(
                    $"Cannot locate test '{testDescription}' at path '{contextPath}' " +
                    $"in '{specType.FullName}'.");

            if (PendingSpecs.IsPending(found.Context, testDescription))
                return false;

            RunLifecycle(found.Context, found.TestAction);
            return true;
        }

        private static bool IsPending(IContextualTestCase root, TestCase testCase)
        {
            var contextPath = testCase.GetPropertyValue<string>(TestProperties.ContextPath, null) ?? "";
            var description = testCase.GetPropertyValue<string>(TestProperties.TestDescription, null) ?? "";
            var found = TestFinder.Find(root, contextPath, description);
            return found != null && PendingSpecs.IsPending(found.Value.Context, description);
        }

        private static bool GroupedContextHasRunnableTests(IContextualTestCase root, TestCase testCase)
        {
            var contextName = testCase.GetPropertyValue<string>(TestProperties.ContextPath, null) ?? "";
            var node = root.ChildContexts
                .FirstOrDefault(c => string.Equals(c.Description, contextName, StringComparison.Ordinal));
            return node != null && PendingSpecs.HasRunnableTests(node);
        }

        /// <summary>
        /// Resolves the <see cref="Type"/> from the stored <paramref name="className"/> metadata.
        /// </summary>
        private static Type ResolveSpecType(string source, string className)
        {
            var assembly = Assembly.LoadFrom(source);
            var typeName = className.Contains(',') ? className.Split(',')[0].Trim() : className;
            return assembly.GetType(typeName)
                ?? throw new InvalidOperationException(
                    $"Cannot find type '{typeName}' in '{source}'.");
        }

        /// <summary>
        /// Runs the cascaded full lifecycle for a single test action.
        /// SetupOnce â†’ SetupEach â†’ Act â†’ [test] â†’ TeardownEach â†’ TeardownOnce.
        /// Teardown runs even when the test throws; the exception is re-thrown afterwards.
        /// </summary>
        private static void RunLifecycle(ITestCase context, Action testAction)
        {
            context.SetupOnce();
            context.SetupEach();

            Exception? failure = null;
            try
            {
                ActExceptionContext.Run(context.Act, testAction);
            }
            catch (Exception ex)
            {
                failure = ex;
            }

            context.TeardownEach();
            context.TeardownOnce();

            if (failure != null)
                ExceptionDispatchInfo.Capture(failure).Throw();
        }

        private static void ReportAllAsFailed(
            IEnumerable<TestCase> cases,
            IFrameworkHandle frameworkHandle,
            Exception ex)
        {
            var startTime = DateTimeOffset.UtcNow;
            foreach (var tc in cases)
            {
                frameworkHandle.RecordStart(tc);
                frameworkHandle.RecordResult(ResultReporter.Failure(tc, ex, startTime, TimeSpan.Zero));
            }
        }
    }
}

