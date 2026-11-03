using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Conversion;
using System;
using System.Reflection;

namespace specs.BDD.VSTestAdapter.Tests.Conversion
{
    [TestFixture]
    public class TestCaseConverter_Tests
    {
        private static readonly string FakeSource = @"C:\tests\MyTests.dll";

        private static readonly string RealSource =
            typeof(TestCaseConverter_Tests).Assembly.Location;

        // A real lambda defined in this class so its Method has a PDB entry in the test assembly.
        private static readonly Action RealItAction = () => { var x = 1 + 1; _ = x; };

        private SpecTest BuildSpecTest(
            string contextPath = "my context → sub context",
            string testDescription = "should behave correctly",
            string contextMethodName = "describe_something")
        {
            var method = typeof(FakeSpecClass).GetMethod(contextMethodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;

            return new SpecTest
            {
                SpecificationClass = typeof(FakeSpecClass),
                ContextMethod = method,
                ContextPath = contextPath,
                TestDescription = testDescription
            };
        }

        private SpecTest BuildSpecTestWithItAction(
            string contextPath = "my context → sub context",
            string testDescription = "should behave correctly",
            string contextMethodName = "describe_something")
        {
            var specTest = BuildSpecTest(contextPath, testDescription, contextMethodName);
            specTest.ItAction = RealItAction;
            return specTest;
        }

        // ── Fully-qualified name ──────────────────────────────────────────────────────────────

        [Test]
        public void FullyQualifiedName_ContainsClassName()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            Check.That(testCase.FullyQualifiedName).Contains(typeof(FakeSpecClass).FullName!);
        }

        [Test]
        public void FullyQualifiedName_ContainsMethodName()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            Check.That(testCase.FullyQualifiedName).Contains("describe_something");
        }

        [Test]
        public void FullyQualifiedName_ContainsContextPath()
        {
            var specTest = BuildSpecTest(contextPath: "my context → sub context");
            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);
            Check.That(testCase.FullyQualifiedName).Contains("my context → sub context");
        }

        [Test]
        public void FullyQualifiedName_ContainsTestDescription()
        {
            var specTest = BuildSpecTest(testDescription: "should behave correctly");
            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);
            Check.That(testCase.FullyQualifiedName).Contains("should behave correctly");
        }

        [Test]
        public void FullyQualifiedName_UsesDoubleColonSeparator()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            // FQN format: ClassName::MethodName::ContextPath::TestDescription
            var parts = testCase.FullyQualifiedName.Split("::");
            Check.That(parts).HasSize(4);
        }

        // ── Display name ──────────────────────────────────────────────────────────────────────

        [Test]
        public void DisplayName_IsContextPathAndTestDescription()
        {
            var specTest = BuildSpecTest(
                contextPath: "my context → sub context",
                testDescription: "should behave correctly");

            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);

            Check.That(testCase.DisplayName).IsEqualTo("my context → sub context → should behave correctly");
        }

        [Test]
        public void DisplayName_IsContextPathAndTestDescription_WithFlatContextPath()
        {
            var specTest = BuildSpecTest(contextPath: "root context", testDescription: "my test");
            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);

            Check.That(testCase.DisplayName).IsEqualTo("root context → my test");
        }

        [Test]
        public void DisplayName_IsTestDescriptionOnly_WhenContextPathIsEmpty()
        {
            var specTest = BuildSpecTest(contextPath: "", testDescription: "my test");
            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);

            Check.That(testCase.DisplayName).IsEqualTo("my test");
        }

        // ── Grouped mode ──────────────────────────────────────────────────────────────────────

        private SpecTest BuildGroupedSpecTest(
            string contextPath = "my context",
            string contextMethodName = "describe_something")
        {
            var specTest = BuildSpecTest(contextPath, testDescription: "", contextMethodName: contextMethodName);
            specTest.IsGrouped = true;
            return specTest;
        }

        [Test]
        public void Grouped_DisplayName_IsContextPathOnly()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildGroupedSpecTest(), FakeSource);
            Check.That(testCase.DisplayName).IsEqualTo("my context");
        }

        [Test]
        public void Grouped_FullyQualifiedName_UsesGroupedMarker()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildGroupedSpecTest(), FakeSource);
            Check.That(testCase.FullyQualifiedName)
                .IsEqualTo($"{typeof(FakeSpecClass).FullName}::describe_something::[grouped]");
        }

        [Test]
        public void Grouped_IsGroupedProperty_IsTrue()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildGroupedSpecTest(), FakeSource);
            Check.That(testCase.GetPropertyValue(TestProperties.IsGrouped, false)).IsTrue();
        }

        [Test]
        public void NonGrouped_IsGroupedProperty_IsFalse()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            Check.That(testCase.GetPropertyValue(TestProperties.IsGrouped, false)).IsFalse();
        }

        // ── Pending ───────────────────────────────────────────────────────────────────────────

        [Test]
        public void Pending_IsPendingProperty_IsTrue()
        {
            var specTest = BuildSpecTest();
            specTest.IsPending = true;

            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);

            Check.That(testCase.GetPropertyValue(TestProperties.IsPending, false)).IsTrue();
        }

        [Test]
        public void NonPending_IsPendingProperty_IsFalse()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            Check.That(testCase.GetPropertyValue(TestProperties.IsPending, true)).IsFalse();
        }

        // ── Source ────────────────────────────────────────────────────────────────────────────

        [Test]
        public void Source_IsSetToProvidedSourcePath()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            Check.That(testCase.Source).IsEqualTo(FakeSource);
        }

        // ── ExecutorUri ───────────────────────────────────────────────────────────────────────

        [Test]
        public void ExecutorUri_MatchesExpectedUri()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            Check.That(testCase.ExecutorUri).IsEqualTo(TestCaseConverter.ExecutorUri);
        }

        [Test]
        public void ExecutorUri_MatchesSpecsTestExecutorExtensionUri()
        {
            // Must match the [ExtensionUri] attribute on SpecsTestExecutor.
            Check.That(TestCaseConverter.ExecutorUri)
                .IsEqualTo(new Uri("executor://SpecsTestExecutor/v1"));
        }

        // ── Metadata properties ───────────────────────────────────────────────────────────────

        [Test]
        public void Metadata_ClassNamePropertyIsSet()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            var className = testCase.GetPropertyValue<string>(TestProperties.ClassName, null);

            Check.That(className).IsNotNull();
            Check.That(className).Contains(nameof(FakeSpecClass));
        }

        [Test]
        public void Metadata_ContextMethodPropertyIsSet()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            var methodName = testCase.GetPropertyValue<string>(TestProperties.ContextMethod, null);

            Check.That(methodName).IsEqualTo("describe_something");
        }

        [Test]
        public void Metadata_ContextPathPropertyIsSet()
        {
            var specTest = BuildSpecTest(contextPath: "my context → sub context");
            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);
            var storedPath = testCase.GetPropertyValue<string>(TestProperties.ContextPath, null);

            Check.That(storedPath).IsEqualTo("my context → sub context");
        }

        [Test]
        public void Metadata_TestDescriptionPropertyIsSet()
        {
            var specTest = BuildSpecTest(testDescription: "should behave correctly");
            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);
            var storedDesc = testCase.GetPropertyValue<string>(TestProperties.TestDescription, null);

            Check.That(storedDesc).IsEqualTo("should behave correctly");
        }

        // ── Source location ───────────────────────────────────────────────────────────────────

        [Test]
        public void WhenItActionIsProvided_SourceLocationUsesLambdaMethod()
        {
            var specTest = BuildSpecTestWithItAction();

            var testCase = TestCaseConverter.ToVSTestCase(specTest, RealSource);

            // Lambda is in this same assembly/PDB so the location should resolve.
            Check.That(testCase.CodeFilePath).IsNotNull();
            Check.That(testCase.CodeFilePath).EndsWith(".cs");
        }

        [Test]
        public void WhenItActionIsProvided_SourceLocationFileContainsTestCaseConverterTests()
        {
            var specTest = BuildSpecTestWithItAction();

            var testCase = TestCaseConverter.ToVSTestCase(specTest, RealSource);

            Check.That(testCase.CodeFilePath).IsNotNull();
            Check.That(testCase.CodeFilePath).Contains("TestCaseConverter_Tests");
        }

        [Test]
        public void WhenItActionIsNullAndContextMethodExists_FallsBackToContextMethodLocation()
        {
            // ItAction intentionally left null; ContextMethod points into this assembly.
            var specTest = BuildSpecTest();
            specTest.ItAction = null;

            var testCase = TestCaseConverter.ToVSTestCase(specTest, RealSource);

            // The fallback should still resolve because the context method lives in this assembly.
            Check.That(testCase.CodeFilePath).IsNotNull();
        }

        [Test]
        public void WhenItActionYieldsNoLocation_DoesNotThrow()
        {
            // Use a fake DLL path so the PDB is not found → TryGetLocation returns null.
            // The converter must degrade gracefully and not throw.
            var specTest = BuildSpecTestWithItAction();

            Check.ThatCode(() => TestCaseConverter.ToVSTestCase(specTest, FakeSource))
                .DoesNotThrow();
        }

        // ── Hierarchy property ────────────────────────────────────────────────────────────────

        [Test]
        public void Hierarchy_HasExactlyFourElements()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            var hierarchy = testCase.GetPropertyValue<string[]>(
                TestProperty.Find("TestCase.Hierarchy")!, null);

            Check.That(hierarchy).IsNotNull();
            Check.That(hierarchy).HasSize(4);
        }

        [Test]
        public void Hierarchy_ContainerElementIsNullOrEmpty()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            var hierarchy = testCase.GetPropertyValue<string[]>(
                TestProperty.Find("TestCase.Hierarchy")!, null)!;

            Check.That(hierarchy[0]).IsNullOrEmpty();
        }

        [Test]
        public void Hierarchy_NamespaceElementMatchesSpecClassNamespace()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            var hierarchy = testCase.GetPropertyValue<string[]>(
                TestProperty.Find("TestCase.Hierarchy")!, null)!;

            Check.That(hierarchy[1]).IsEqualTo(typeof(FakeSpecClass).Namespace);
        }

        [Test]
        public void Hierarchy_ClassElementMatchesSpecClassSimpleName()
        {
            var testCase = TestCaseConverter.ToVSTestCase(BuildSpecTest(), FakeSource);
            var hierarchy = testCase.GetPropertyValue<string[]>(
                TestProperty.Find("TestCase.Hierarchy")!, null)!;

            Check.That(hierarchy[2]).IsEqualTo(nameof(FakeSpecClass));
        }

        [Test]
        public void Hierarchy_TestGroupElementMatchesContextPath()
        {
            var specTest = BuildSpecTest(contextPath: "my context → sub context");
            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);
            var hierarchy = testCase.GetPropertyValue<string[]>(
                TestProperty.Find("TestCase.Hierarchy")!, null)!;

            Check.That(hierarchy[3]).IsEqualTo("my context → sub context");
        }

        [Test]
        public void Hierarchy_WhenContextPathIsEmpty_StillHasFourElements()
        {
            var specTest = BuildSpecTest(contextPath: "");
            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);
            var hierarchy = testCase.GetPropertyValue<string[]>(
                TestProperty.Find("TestCase.Hierarchy")!, null);

            Check.That(hierarchy).IsNotNull();
            Check.That(hierarchy).HasSize(4);
        }

        // ── Test double ───────────────────────────────────────────────────────────────────────

        /// <summary>Minimal Specifications subclass used as a test double.</summary>
        private class FakeSpecClass : specs.BDD.Specifications
        {
            [specs.BDD.Attributes.Describe("fake context")]
            public void describe_something()
            {
                It["a test"] = () => { };
            }
        }
    }
}
