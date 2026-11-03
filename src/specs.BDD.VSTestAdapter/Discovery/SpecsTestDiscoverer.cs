using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using specs.BDD.Attributes;
using specs.BDD.Runner;
using specs.BDD.VSTestAdapter.Conversion;

namespace specs.BDD.VSTestAdapter.Discovery
{
    /// <summary>
    /// VSTest test discoverer for the Specs BDD framework.
    /// Finds all <see cref="specs.BDD.Specifications"/> subclasses in the scanned assemblies,
    /// validates that each has at least one [Describe] method, and reports a <see cref="TestCase"/>
    /// for every It[...] assertion found in the test hierarchy.
    /// </summary>
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [DefaultExecutorUri("executor://SpecsTestExecutor/v1")]
    public class SpecsTestDiscoverer : ITestDiscoverer
    {
        /// <inheritdoc/>
        public void DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//                Debugger.Launch();
//#endif

            var runSettingsGrouping = GroupingSettings.ReadRunSettings(
                discoveryContext?.RunSettings?.SettingsXml);

            foreach (var source in sources)
            {
                try
                {
                    DiscoverTestsInSource(source, logger, discoverySink, runSettingsGrouping);
                }
                catch (Exception ex)
                {
                    logger.SendMessage(
                        TestMessageLevel.Warning,
                        $"[specs.BDD.VSTestAdapter] Error scanning '{source}': {ex.Message}");
                }
            }
        }

        // ── Private helpers ────────────────────────────────────────────────────────────────────

        private static void DiscoverTestsInSource(
            string source,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink,
            TestStructure? runSettingsGrouping)
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(source);
            }
            catch (Exception ex)
            {
                logger.SendMessage(
                    TestMessageLevel.Informational,
                    $"[specs.BDD.VSTestAdapter] Skipping '{source}' (cannot load): {ex.Message}");
                return;
            }

            var specClasses = SpecificationClassScanner.FindSpecificationClasses(assembly);

            foreach (var specClass in specClasses)
            {
                try
                {
                    DiscoverTestsInClass(specClass, source, logger, discoverySink, runSettingsGrouping);
                }
                catch (InvalidSpecificationException ex)
                {
                    // Surface validation failures as warnings — fail loudly so authors know immediately.
                    logger.SendMessage(TestMessageLevel.Warning,
                        $"[specs.BDD.VSTestAdapter] {ex.Message}");
                }
                catch (Exception ex)
                {
                    logger.SendMessage(TestMessageLevel.Warning,
                        $"[specs.BDD.VSTestAdapter] Error processing '{specClass.FullName}': {ex.Message}");
                }
            }
        }

        private static void DiscoverTestsInClass(
            Type specClass,
            string source,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink,
            TestStructure? runSettingsGrouping)
        {
            // Validate: every Specification class must have at least one [Describe] method.
            ContextMethodScanner.ValidateHasContextMethods(specClass);

            IEnumerable<SpecTest> specTests;
            try
            {
                specTests = SuiteTreeFlattener.Flatten(specClass, runSettingsGrouping);
            }
            catch (Exception ex)
            {
                logger.SendMessage(TestMessageLevel.Warning,
                    $"[specs.BDD.VSTestAdapter] Error extracting tests from " +
                    $"'{specClass.FullName}': {ex.Message}");
                return;
            }

            foreach (var specTest in specTests)
            {
                var testCase = TestCaseConverter.ToVSTestCase(specTest, source);
                discoverySink.SendTestCase(testCase);
            }
        }
    }
}
