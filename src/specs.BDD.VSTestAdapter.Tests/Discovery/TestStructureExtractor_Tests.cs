using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Discovery;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using System.Linq;
using System.Reflection;

namespace specs.BDD.VSTestAdapter.Tests.Discovery
{
    [TestFixture]
    public class TestStructureExtractor_Tests
    {
        private static MethodInfo GetContextMethod<T>(string methodName)
            => typeof(T).GetMethod(methodName,
                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
               ?? throw new System.Exception($"Method '{methodName}' not found on {typeof(T).Name}");

        [Test]
        public void SimpleSpec_ReturnsOneSpecTestPerItAssertion()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec)).ToList();
            Check.That(tests).HasSize(2);
        }

        [Test]
        public void SimpleSpec_TestDescriptionsMatchItLabels()
        {
            var descriptions = TestStructureExtractor.ExtractTests(typeof(SimpleSpec))
                .Select(t => t.TestDescription).ToList();
            Check.That(descriptions).Contains("test one");
            Check.That(descriptions).Contains("test two");
        }

        [Test]
        public void UsesContextAttributeDescriptionAsRootContextPath()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec)).ToList();
            foreach (var test in tests)
                Check.That(test.ContextPath).IsEqualTo("simple context");
        }

        [Test]
        public void UsesMethodNameAsContextPathWhenAttributeHasNoDescription()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(SpecWithNoContextDescription)).ToList();
            Check.That(tests).HasSize(1);
            Check.That(tests[0].ContextPath).IsEqualTo("describe_addition");
        }

        [Test]
        public void SpecTestCarriesCorrectSpecificationClass()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec)).ToList();
            foreach (var test in tests)
                Check.That(test.SpecificationClass).IsEqualTo(typeof(SimpleSpec));
        }

        [Test]
        public void SpecTestCarriesCorrectContextMethod()
        {
            var method = GetContextMethod<SimpleSpec>("describe_something");
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec)).ToList();
            foreach (var test in tests)
                Check.That(test.ContextMethod).IsEqualTo(method);
        }

        [Test]
        public void NestedContext_ContextPathIncludesArrowSeparator()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(NestedContextSpec)).ToList();
            Check.That(tests).HasSize(2);
            var paths = tests.Select(t => t.ContextPath).ToList();
            Check.That(paths).Contains($"root context{TestStructureExtractor.PathSeparator}when doing X");
            Check.That(paths).Contains($"root context{TestStructureExtractor.PathSeparator}when doing Y");
        }

        [Test]
        public void NestedContext_TestDescriptionsArePreserved()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(NestedContextSpec)).ToList();
            var descriptions = tests.Select(t => t.TestDescription).ToList();
            Check.That(descriptions).Contains("should work for X");
            Check.That(descriptions).Contains("should work for Y");
        }

        [Test]
        public void DeeplyNestedContext_BuildsFullPathWithMultipleArrows()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(DeeplyNestedContextSpec)).ToList();
            Check.That(tests).HasSize(1);
            Check.That(tests[0].ContextPath).IsEqualTo($"root{TestStructureExtractor.PathSeparator}level one{TestStructureExtractor.PathSeparator}level two");
            Check.That(tests[0].TestDescription).IsEqualTo("deep test");
        }

        [Test]
        public void MixedDepth_ReturnsTestsAtAllLevels()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(MixedDepthSpec)).ToList();
            Check.That(tests).HasSize(2);
            var paths = tests.Select(t => t.ContextPath).ToList();
            Check.That(paths).Contains("mixed depth");
            Check.That(paths).Contains($"mixed depth{TestStructureExtractor.PathSeparator}nested context");
        }

        [Test]
        public void MixedDepth_TestDescriptionsMatchItLabels()
        {
            var descriptions = TestStructureExtractor.ExtractTests(typeof(MixedDepthSpec))
                .Select(t => t.TestDescription).ToList();
            Check.That(descriptions).Contains("root level test");
            Check.That(descriptions).Contains("nested test");
        }

        [Test]
        public void MultipleContextMethods_EachMethodProducesItsOwnTests()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(MultiContextSpec)).ToList();
            var firstTests = tests.Where(t => t.ContextPath == "feature one").ToList();
            var secondTests = tests.Where(t => t.ContextPath == "feature two").ToList();
            Check.That(firstTests).HasSize(1);
            Check.That(secondTests).HasSize(2);
        }

        [Test]
        public void MultipleContextMethods_ContextPathsAreScopedToTheirOwnMethod()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(MultiContextSpec)).ToList();
            var paths = tests.Select(t => t.ContextPath).Distinct().ToList();
            Check.That(paths).Contains("feature one");
            Check.That(paths).Contains("feature two");
        }

        // ── ItAction ──────────────────────────────────────────────────────────────────────────

        [Test]
        public void EverySpecTest_HasItActionPopulated()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec)).ToList();
            foreach (var test in tests)
                Check.That(test.ItAction).IsNotNull();
        }

        [Test]
        public void ItAction_Method_IsDifferentFromContextMethod()
        {
            var contextMethod = GetContextMethod<SimpleSpec>("describe_something");
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec)).ToList();
            foreach (var test in tests)
                Check.That(test.ItAction!.Method).IsNotEqualTo(contextMethod);
        }

        [Test]
        public void ItAction_EachTestHasDistinctMethod()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec)).ToList();
            Check.That(tests).HasSize(2);
            Check.That(tests[0].ItAction!.Method).IsNotEqualTo(tests[1].ItAction!.Method);
        }
    }
}