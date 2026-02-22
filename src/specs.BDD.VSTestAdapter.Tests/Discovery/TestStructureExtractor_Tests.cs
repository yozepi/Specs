using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Conversion;
using specs.BDD.VSTestAdapter.Discovery;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using System.Linq;
using System.Reflection;

namespace specs.BDD.VSTestAdapter.Tests.Discovery
{
    [TestFixture]
    public class TestStructureExtractor_Tests
    {
        // ── Helper ────────────────────────────────────────────────────────────────────────────

        private static MethodInfo GetContextMethod<T>(string methodName)
            => typeof(T).GetMethod(methodName,
                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
               ?? throw new System.Exception($"Method '{methodName}' not found on {typeof(T).Name}");

        // ── Simple direct tests ───────────────────────────────────────────────────────────────

        [Test]
        public void SimpleSpec_ReturnsOneSpecTestPerItAssertion()
        {
            var method = GetContextMethod<SimpleSpec>("describe_something");
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec), method).ToList();

            Check.That(tests).HasSize(2);
        }

        [Test]
        public void SimpleSpec_TestDescriptionsMatchItLabels()
        {
            var method = GetContextMethod<SimpleSpec>("describe_something");
            var descriptions = TestStructureExtractor
                .ExtractTests(typeof(SimpleSpec), method)
                .Select(t => t.TestDescription)
                .ToList();

            Check.That(descriptions).Contains("test one");
            Check.That(descriptions).Contains("test two");
        }

        // ── Context path from [Context] attribute description ─────────────────────────────────

        [Test]
        public void UsesContextAttributeDescriptionAsRootContextPath()
        {
            var method = GetContextMethod<SimpleSpec>("describe_something");
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec), method).ToList();

            foreach (var test in tests)
            {
                Check.That(test.ContextPath).IsEqualTo("simple context");
            }
        }

        [Test]
        public void UsesMethodNameAsContextPathWhenAttributeHasNoDescription()
        {
            var method = GetContextMethod<SpecWithNoContextDescription>("describe_addition");
            var tests = TestStructureExtractor.ExtractTests(typeof(SpecWithNoContextDescription), method).ToList();

            Check.That(tests).HasSize(1);
            Check.That(tests[0].ContextPath).IsEqualTo("describe_addition");
        }

        // ── SpecTest metadata ─────────────────────────────────────────────────────────────────

        [Test]
        public void SpecTestCarriesCorrectSpecificationClass()
        {
            var method = GetContextMethod<SimpleSpec>("describe_something");
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec), method).ToList();

            foreach (var test in tests)
            {
                Check.That(test.SpecificationClass).IsEqualTo(typeof(SimpleSpec));
            }
        }

        [Test]
        public void SpecTestCarriesCorrectContextMethod()
        {
            var method = GetContextMethod<SimpleSpec>("describe_something");
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec), method).ToList();

            foreach (var test in tests)
            {
                Check.That(test.ContextMethod).IsEqualTo(method);
            }
        }

        // ── Nested context paths ──────────────────────────────────────────────────────────────

        [Test]
        public void NestedContext_ContextPathIncludesArrowSeparator()
        {
            var method = GetContextMethod<NestedContextSpec>("describe_something");
            var tests = TestStructureExtractor.ExtractTests(typeof(NestedContextSpec), method).ToList();

            Check.That(tests).HasSize(2);

            var paths = tests.Select(t => t.ContextPath).ToList();
            Check.That(paths).Contains("root context → when doing X");
            Check.That(paths).Contains("root context → when doing Y");
        }

        [Test]
        public void NestedContext_TestDescriptionsArePreserved()
        {
            var method = GetContextMethod<NestedContextSpec>("describe_something");
            var tests = TestStructureExtractor.ExtractTests(typeof(NestedContextSpec), method).ToList();

            var descriptions = tests.Select(t => t.TestDescription).ToList();
            Check.That(descriptions).Contains("should work for X");
            Check.That(descriptions).Contains("should work for Y");
        }

        // ── Deeply nested context (recursive traversal) ───────────────────────────────────────

        [Test]
        public void DeeplyNestedContext_BuildsFullPathWithMultipleArrows()
        {
            var method = GetContextMethod<DeeplyNestedContextSpec>("describe_something");
            var tests = TestStructureExtractor.ExtractTests(typeof(DeeplyNestedContextSpec), method).ToList();

            Check.That(tests).HasSize(1);
            Check.That(tests[0].ContextPath).IsEqualTo("root → level one → level two");
            Check.That(tests[0].TestDescription).IsEqualTo("deep test");
        }

        // ── Mixed depth (root-level tests + nested tests) ─────────────────────────────────────

        [Test]
        public void MixedDepth_ReturnsTestsAtAllLevels()
        {
            var method = GetContextMethod<MixedDepthSpec>("describe_something");
            var tests = TestStructureExtractor.ExtractTests(typeof(MixedDepthSpec), method).ToList();

            Check.That(tests).HasSize(2);

            var paths = tests.Select(t => t.ContextPath).ToList();
            Check.That(paths).Contains("mixed depth");                       // root-level test
            Check.That(paths).Contains("mixed depth → nested context");      // nested test
        }

        [Test]
        public void MixedDepth_TestDescriptionsMatchItLabels()
        {
            var method = GetContextMethod<MixedDepthSpec>("describe_something");
            var descriptions = TestStructureExtractor
                .ExtractTests(typeof(MixedDepthSpec), method)
                .Select(t => t.TestDescription)
                .ToList();

            Check.That(descriptions).Contains("root level test");
            Check.That(descriptions).Contains("nested test");
        }

        // ── Multiple [Context] methods ────────────────────────────────────────────────────────

        [Test]
        public void MultipleContextMethods_EachMethodProducesItsOwnTests()
        {
            var firstMethod = GetContextMethod<MultiContextSpec>("first_feature");
            var secondMethod = GetContextMethod<MultiContextSpec>("second_feature");

            var firstTests = TestStructureExtractor.ExtractTests(typeof(MultiContextSpec), firstMethod).ToList();
            var secondTests = TestStructureExtractor.ExtractTests(typeof(MultiContextSpec), secondMethod).ToList();

            Check.That(firstTests).HasSize(1);
            Check.That(secondTests).HasSize(2);
        }

        [Test]
        public void MultipleContextMethods_ContextPathsAreScopedToTheirOwnMethod()
        {
            var firstMethod = GetContextMethod<MultiContextSpec>("first_feature");
            var secondMethod = GetContextMethod<MultiContextSpec>("second_feature");

            var firstTests = TestStructureExtractor.ExtractTests(typeof(MultiContextSpec), firstMethod).ToList();
            var secondTests = TestStructureExtractor.ExtractTests(typeof(MultiContextSpec), secondMethod).ToList();

            Check.That(firstTests[0].ContextPath).IsEqualTo("feature one");
            foreach (var t in secondTests)
                Check.That(t.ContextPath).IsEqualTo("feature two");
        }
    }
}
