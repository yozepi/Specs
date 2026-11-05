using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using specs.BDD.Runner;

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
        /// VSTest RFC-0033 hierarchy property. A string[4] of [Container, Namespace, Class, TestGroup]
        /// that drives the nested tree in VS Test Explorer.
        /// </summary>
        private static readonly TestProperty HierarchyProperty =
            TestProperty.Register(
                id: "TestCase.Hierarchy",
                label: "Hierarchy",
                category: string.Empty,
                description: string.Empty,
                valueType: typeof(string[]),
                validateValueCallback: null,
                attributes: TestPropertyAttributes.Immutable,
                owner: typeof(TestCase));

        // RFC-0017 managed names; IDEs resolve these to the [Describe] method symbol (CodeLens, navigation).
        private static readonly TestProperty ManagedTypeProperty =
            TestProperty.Register("TestCase.ManagedType", "ManagedType", string.Empty, string.Empty,
                typeof(string), null, TestPropertyAttributes.Hidden, typeof(TestCase));

        private static readonly TestProperty ManagedMethodProperty =
            TestProperty.Register("TestCase.ManagedMethod", "ManagedMethod", string.Empty, string.Empty,
                typeof(string), null, TestPropertyAttributes.Hidden, typeof(TestCase));

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
                Id = BuildId(specTest, source),
                DisplayName = displayName
            };

            // Store metadata so the executor can reconstruct the test without re-scanning.
            testCase.SetPropertyValue(TestProperties.ClassName,
                specTest.SpecificationClass.AssemblyQualifiedName ?? specTest.SpecificationClass.FullName!);
            testCase.SetPropertyValue(TestProperties.ContextMethod,
                specTest.ContextMethod?.Name ?? "");
            testCase.SetPropertyValue(TestProperties.ContextPath,
                specTest.ContextPath);
            testCase.SetPropertyValue(TestProperties.TestDescription,
                specTest.TestDescription);
            testCase.SetPropertyValue(TestProperties.IsGrouped,
                specTest.IsGrouped);
            testCase.SetPropertyValue(TestProperties.IsPending,
                specTest.IsPending);

            // Set the VSTest RFC-0033 Hierarchy property so Test Explorer nests the test under
            // Namespace › Class › [Describe].
            testCase.SetPropertyValue(HierarchyProperty, BuildHierarchy(specTest));

            if (specTest.ContextMethod != null)
            {
                testCase.SetPropertyValue(ManagedTypeProperty, specTest.SpecificationClass.FullName);
                testCase.SetPropertyValue(ManagedMethodProperty, specTest.ContextMethod.Name);
            }

            // Prefer the It[...] lambda's source line so Test Explorer navigates to the test
            // definition rather than the top of the [Describe] method.  Fall back to the
            // [Describe] method when the lambda has no PDB sequence points (e.g. empty body).
            var location =
                (specTest.ItAction != null
                    ? SourceLocationReader.TryGetLocation(source, SourceLocationReader.GetTestMethod(specTest.ItAction))
                    : null)
                ?? (specTest.DeclaredLocation is { } declared
                    ? new SourceLocationReader.SourceLocation(declared.filePath, declared.lineNumber)
                    : null)
                ?? (specTest.ContextMethod != null
                    ? SourceLocationReader.TryGetLocation(source, specTest.ContextMethod)
                    : null);
            if (location != null)
            {
                testCase.CodeFilePath = location.FilePath;
                testCase.LineNumber = location.LineNumber;
            }

            return testCase;
        }

        // ── Private helpers ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the fully-qualified name {ClassName}.{MethodName}, shared by every spec of a
        /// [Describe] so Visual Studio treats them as data-driven rows of the method.
        /// </summary>
        private static string BuildFullyQualifiedName(SpecTest specTest) =>
            specTest.ContextMethod != null
                ? $"{specTest.SpecificationClass.FullName}.{specTest.ContextMethod.Name}"
                : BuildUniqueName(specTest);

        /// <summary>Deterministic per-spec Id, since the fully-qualified name is not unique.</summary>
        private static Guid BuildId(SpecTest specTest, string source)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(source + "|" + BuildUniqueName(specTest)));
            var bytes = new byte[16];
            Array.Copy(hash, bytes, bytes.Length);
            return new Guid(bytes);
        }

        /// <summary>
        /// Builds a name unique to the spec.
        /// Format: {ClassName}::{MethodName}::{ContextPath}::{TestDescription}
        /// Grouped format: {ClassName}::{MethodName}::[grouped]
        /// </summary>
        private static string BuildUniqueName(SpecTest specTest)
        {
            if (specTest.IsGrouped)
            {
                return string.Join("::",
                    specTest.SpecificationClass.FullName,
                    specTest.ContextMethod?.Name ?? specTest.ContextPath,
                    "[grouped]");
            }

            return string.Join("::",
                specTest.SpecificationClass.FullName,
                specTest.ContextMethod?.Name ?? "",
                specTest.ContextPath,
                specTest.TestDescription);
        }

        /// <summary>
        /// Builds the human-readable display name shown in Test Explorer.
        /// Format: {ContextPath} → {TestDescription}, so explorers that don't nest under the
        /// [Describe] (VS Code) still show it.
        /// Grouped format: {ContextPath} (root context name only)
        /// </summary>
        private static string BuildDisplayName(SpecTest specTest)
        {
            if (specTest.IsGrouped)
                return specTest.ContextPath;

            return string.Join(ContextPathFormat.Separator,
                SplitContextPath(specTest.ContextPath).Append(specTest.TestDescription));
        }

        /// <summary>
        /// Builds the VSTest RFC-0033 Hierarchy array (exactly 4 elements) that controls how
        /// Test Explorer nests this test in its tree:
        ///   [0] Container  — null, so VS uses the project/assembly name
        ///   [1] Namespace  — the namespace of the Specifications class
        ///   [2] Class      — the simple (unqualified) class name
        ///   [3] TestGroup  — the [Describe] (first segment of the context path)
        /// </summary>
        private static string[] BuildHierarchy(SpecTest specTest)
        {
            return new string[]
            {
                null!,                                             // [0] Container  — VS fills from assembly
                specTest.SpecificationClass.Namespace ?? "",      // [1] Namespace
                specTest.SpecificationClass.Name,                 // [2] Class
                SplitContextPath(specTest.ContextPath).FirstOrDefault() ?? "" // [3] TestGroup — [Describe]
            };
        }

        private static string[] SplitContextPath(string contextPath) =>
            string.IsNullOrEmpty(contextPath)
                ? Array.Empty<string>()
                : contextPath.Split(new[] { ContextPathFormat.Separator }, StringSplitOptions.None);
    }
}
