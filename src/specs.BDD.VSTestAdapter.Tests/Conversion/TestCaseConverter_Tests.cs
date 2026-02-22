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
        public void DisplayName_CombinesContextPathAndTestDescriptionWithArrow()
        {
            var specTest = BuildSpecTest(
                contextPath: "my context → sub context",
                testDescription: "should behave correctly");

            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);

            Check.That(testCase.DisplayName).IsEqualTo("my context → sub context → should behave correctly");
        }

        [Test]
        public void DisplayName_WorksWithFlatContextPath()
        {
            var specTest = BuildSpecTest(contextPath: "root context", testDescription: "my test");
            var testCase = TestCaseConverter.ToVSTestCase(specTest, FakeSource);

            Check.That(testCase.DisplayName).IsEqualTo("root context → my test");
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

        // ── Test double ───────────────────────────────────────────────────────────────────────

        /// <summary>Minimal Specifications subclass used as a test double.</summary>
        private class FakeSpecClass : specs.BDD.Specifications
        {
            [specs.BDD.Attributes.Context("fake context")]
            public void describe_something()
            {
                It["a test"] = () => { };
            }
        }
    }
}
