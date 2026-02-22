using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using specs.BDD.VSTestAdapter.Discovery;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// VSTest test executor for the Specs BDD framework.
    /// Executes tests discovered by <see cref="SpecsTestDiscoverer"/> by re-instantiating
    /// the specification class, re-invoking the [Context] method to rebuild the test hierarchy,
    /// navigating to the specific test, and running the full lifecycle.
    /// </summary>
    [ExtensionUri("executor://SpecsTestExecutor/v1")]
    public class SpecsTestExecutor : ITestExecutor
    {
        private volatile bool _cancelled;

        /// <inheritdoc/>
        /// <remarks>
        /// This overload is called by VSTest when specific <see cref="TestCase"/> objects are
        /// selected for execution (e.g., "Run Selected Tests" in Test Explorer).
        /// </remarks>
        public void RunTests(
            IEnumerable<TestCase>? tests,
            IRunContext? runContext,
            IFrameworkHandle? frameworkHandle)
        {
            _cancelled = false;

            if (tests == null || frameworkHandle == null)
                return;

            foreach (var testCase in tests)
            {
                if (_cancelled)
                    break;

                frameworkHandle.RecordStart(testCase);
                var result = TestInvoker.Invoke(testCase);
                frameworkHandle.RecordResult(result);
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This overload is called when VSTest is asked to run all tests from a set of source
        /// assemblies (e.g., <c>dotnet test</c> without a filter).
        /// It re-runs discovery internally to obtain the <see cref="TestCase"/> list, then
        /// delegates to the <see cref="RunTests(IEnumerable{TestCase},IRunContext,IFrameworkHandle)"/>
        /// overload.
        /// </remarks>
        public void RunTests(
            IEnumerable<string>? sources,
            IRunContext? runContext,
            IFrameworkHandle? frameworkHandle)
        {
            _cancelled = false;

            if (sources == null || frameworkHandle == null)
                return;

            var testCases = DiscoverFromSources(sources, frameworkHandle);
            RunTests(testCases, runContext, frameworkHandle);
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            _cancelled = true;
        }

        // ── Private helpers ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Re-runs the discoverer over <paramref name="sources"/> and collects the resulting
        /// <see cref="TestCase"/> objects into a list.
        /// <para>
        /// <see cref="IFrameworkHandle"/> implements <see cref="IMessageLogger"/>, so it is
        /// passed directly for any discovery warnings.
        /// </para>
        /// </summary>
        private static IEnumerable<TestCase> DiscoverFromSources(
            IEnumerable<string> sources,
            IFrameworkHandle frameworkHandle)
        {
            var collected = new List<TestCase>();
            var sink = new CollectingSink(collected);
            var discoverer = new SpecsTestDiscoverer();

            discoverer.DiscoverTests(
                sources,
                discoveryContext: null!,    // SpecsTestDiscoverer does not use this parameter
                logger: frameworkHandle,
                discoverySink: sink);

            return collected;
        }

        // ── Nested private types ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Minimal <see cref="ITestCaseDiscoverySink"/> that accumulates discovered
        /// <see cref="TestCase"/> objects into the provided list.
        /// </summary>
        private sealed class CollectingSink : ITestCaseDiscoverySink
        {
            private readonly List<TestCase> _tests;

            public CollectingSink(List<TestCase> tests) => _tests = tests;

            public void SendTestCase(TestCase discoveredTest) => _tests.Add(discoveredTest);
        }
    }
}
