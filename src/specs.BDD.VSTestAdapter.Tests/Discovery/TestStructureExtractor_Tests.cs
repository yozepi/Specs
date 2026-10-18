using NFluent;
using NUnit.Framework;
using specs.BDD.Attributes;
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
        public void UsesDescribeAttributeDescriptionAsRootContextPath()
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

        // ── Grouped mode ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GroupedContext_YieldsSingleGroupedSpecTest()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(GroupedSpec)).ToList();

            Check.That(tests).HasSize(1);
            Check.That(tests[0].IsGrouped).IsTrue();
        }

        [Test]
        public void GroupedSpecTest_ContextPathIsRootContextDescription()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(GroupedSpec)).ToList();

            Check.That(tests[0].ContextPath).IsEqualTo("grouped feature");
            Check.That(tests[0].TestDescription).IsEmpty();
            Check.That(tests[0].ItAction).IsNull();
        }

        [Test]
        public void GroupedSpecTest_CarriesContextMethodForNavigation()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(GroupedSpec)).ToList();

            Check.That(tests[0].ContextMethod).IsEqualTo(GetContextMethod<GroupedSpec>("describe_grouped"));
        }

        [Test]
        public void MixedGrouping_GroupedAndFlatMethodsCoexist()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(MixedGroupingSpec)).ToList();

            var grouped = tests.Where(t => t.IsGrouped).ToList();
            var flat = tests.Where(t => !t.IsGrouped).ToList();

            Check.That(grouped).HasSize(1);
            Check.That(grouped[0].ContextPath).IsEqualTo("grouped part");
            Check.That(flat).HasSize(1);
            Check.That(flat[0].TestDescription).IsEqualTo("flat test");
        }

        [Test]
        public void RunSettingsGrouping_AppliesToContextsWithoutExplicitSetting()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(SimpleSpec), runSettingsGrouping: SpecReporting.GroupedByRootContext).ToList();

            Check.That(tests).HasSize(1);
            Check.That(tests[0].IsGrouped).IsTrue();
            Check.That(tests[0].ContextPath).IsEqualTo("simple context");
        }

        // ── Pending ───────────────────────────────────────────────────────────────────────────

        [Test]
        public void Pending_SpecsAreStillDiscovered()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(SpecWithPending)).ToList();

            Check.That(tests.Select(t => t.TestDescription)).ContainsExactly(
                "runnable", "todo", "todo async", "excluded", "inside pending context");
        }

        [Test]
        public void Pending_IsPendingSetForTodoXItAndPendingContext()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(SpecWithPending))
                .ToDictionary(t => t.TestDescription, t => t.IsPending);

            Check.That(tests["runnable"]).IsFalse();
            Check.That(tests["todo"]).IsTrue();
            Check.That(tests["todo async"]).IsTrue();
            Check.That(tests["excluded"]).IsTrue();
            Check.That(tests["inside pending context"]).IsTrue();
        }

        [Test]
        public void Pending_GroupedContext_IsPendingOnlyWhenAllSpecsArePending()
        {
            var tests = TestStructureExtractor.ExtractTests(typeof(GroupedSpecWithPending))
                .ToDictionary(t => t.ContextPath, t => t.IsPending);

            Check.That(tests["grouped with pending"]).IsFalse();
            Check.That(tests["grouped all pending"]).IsTrue();
        }
    }
}