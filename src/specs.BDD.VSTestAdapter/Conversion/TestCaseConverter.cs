using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace specs.BDD.VSTestAdapter.Conversion
{
    /// <summary>
    /// Converts a <see cref="SpecTest"/> to the VSTest <see cref="TestCase"/> format,
    /// embedding all metadata needed to re-locate and execute the test later.
    /// </summary>
    public static class TestCaseConverter
    {
        /// <summary>
        /// The executor URI that identifies the <c>SpecsTestExecutor</c>.
        /// Must match the <c>[ExtensionUri]</c> attribute on <c>SpecsTestExecutor</c>.
        /// </summary>
        public static readonly Uri ExecutorUri = new Uri("executor://SpecsTestExecutor/v1");

        /// <summary>
        /// Converts the given <see cref="SpecTest"/> into a VSTest <see cref="TestCase"/> object.
        /// </summary>
        /// <param name="specTest">The spec test to convert.</param>
        /// <param name="source">
        /// The path to the assembly DLL that contains this test (as provided by VSTest during discovery).
        /// </param>
        /// <returns>A fully-populated <see cref="TestCase"/> ready to be reported to VSTest.</returns>
        public static TestCase ToVSTestCase(SpecTest specTest, string source)
        {
            var fullyQualifiedName = BuildFullyQualifiedName(specTest);
            var displayName = BuildDisplayName(specTest);

            var testCase = new TestCase(fullyQualifiedName, ExecutorUri, source)
            {
                DisplayName = displayName
            };

            // Store metadata so the executor can reconstruct the test without re-scanning.
            testCase.SetPropertyValue(TestProperties.ClassName,
                specTest.SpecificationClass.AssemblyQualifiedName ?? specTest.SpecificationClass.FullName!);
            testCase.SetPropertyValue(TestProperties.ContextMethod,
                specTest.ContextMethod.Name);
            testCase.SetPropertyValue(TestProperties.ContextPath,
                specTest.ContextPath);
            testCase.SetPropertyValue(TestProperties.TestDescription,
                specTest.TestDescription);

            return testCase;
        }

        // ── Private helpers ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a unique fully-qualified name from the spec test's class, method, context path,
        /// and test description. This is the stable identifier VSTest uses to track results.
        /// Format: {ClassName}::{MethodName}::{ContextPath}::{TestDescription}
        /// </summary>
        private static string BuildFullyQualifiedName(SpecTest specTest)
        {
            return string.Join("::",
                specTest.SpecificationClass.FullName,
                specTest.ContextMethod.Name,
                specTest.ContextPath,
                specTest.TestDescription);
        }

        /// <summary>
        /// Builds the human-readable display name shown in Test Explorer.
        /// Format: {ContextPath} → {TestDescription}
        /// </summary>
        private static string BuildDisplayName(SpecTest specTest)
        {
            return $"{specTest.ContextPath} \u2192 {specTest.TestDescription}";
        }
    }
}
