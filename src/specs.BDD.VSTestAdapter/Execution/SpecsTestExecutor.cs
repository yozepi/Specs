using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using specs.BDD.VSTestAdapter.Conversion;
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
        private CancellationTokenSource? _cancellationTokenSource = default;

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
            if (tests == null || frameworkHandle == null)
                return;

#if DEBUG
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();
#endif

            // Group tests by specification class so that BeforeAll / AfterAll fire exactly once
            // per class per run, regardless of which context methods or paths are selected.
            var groups = tests.GroupBy(tc => (
                Source: tc.Source,
                ClassName: tc.GetPropertyValue<string>(TestProperties.ClassName, null) ?? ""
            ));

            var showSourceLocations = GroupingSettings.ReadShowSourceLocations(
                runContext?.RunSettings?.SettingsXml);
            var markerStyle = GroupingSettings.ResolveMarkerStyle(
                GroupingSettings.ReadColorOutput(runContext?.RunSettings?.SettingsXml),
                Environment.GetEnvironmentVariable("VSAPPIDNAME"));

            using (EnsureCancellationTokenSource())
            {
                foreach (var group in groups)
                {
                    if (_cancellationTokenSource!.IsCancellationRequested)
                    {
                        break;
                    }

                    TestInvoker.InvokeSuite(
                        group.Key.Source,
                        group.Key.ClassName,
                        group,
                        frameworkHandle,
                        showSourceLocations,
                        markerStyle,
                        _cancellationTokenSource.Token);
                }

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
            if (sources == null || frameworkHandle == null)
                return;

            var testCases = DiscoverFromSources(sources, runContext, frameworkHandle);
            RunTests(testCases, runContext, frameworkHandle);
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            EnsureCancellationTokenSource().Cancel();
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
            IRunContext? runContext,
            IFrameworkHandle frameworkHandle)
        {
            var collected = new List<TestCase>();
            var sink = new CollectingSink(collected);
            var discoverer = new SpecsTestDiscoverer();

            discoverer.DiscoverTests(
                sources,
                discoveryContext: runContext!,   // carries .runsettings into re-discovery
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

        private CancellationTokenSource EnsureCancellationTokenSource()
        {
            try
            {
                if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                }
            }
            catch (ObjectDisposedException)
            {

                _cancellationTokenSource = new CancellationTokenSource();
            }

            return _cancellationTokenSource;
        }
    }
}
