using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Conversion;
using specs.BDD.VSTestAdapter.Discovery;
using specs.BDD.VSTestAdapter.Execution;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using System.Linq;
using System.Threading;

namespace specs.BDD.VSTestAdapter.Tests.Execution
{
    /// <summary>
    /// Tests for <see cref="GroupedContextRunner"/> — builds a real suite tree from grouped
    /// fixtures, executes the whole root-context subtree, and checks the single aggregated
    /// <see cref="TestResult"/> and its published output.
    /// </summary>
    [TestFixture]
    public class GroupedContextRunner_Tests
    {
        private static TestCase MakeGroupedTestCase<TSpec>(string contextPath)
            where TSpec : specs.BDD.Specifications
        {
            var specTest = TestStructureExtractor
                .ExtractTests(typeof(TSpec))
                .Single(t => t.IsGrouped && t.ContextPath == contextPath);

            return TestCaseConverter.ToVSTestCase(specTest, typeof(TSpec).Assembly.Location);
        }

        private static TestResult ExecuteGrouped<TSpec>(string contextPath)
            where TSpec : specs.BDD.Specifications
        {
            var testCase = MakeGroupedTestCase<TSpec>(contextPath);
            var root = SuiteTreeBuilder.BuildSuite(typeof(TSpec));
            return GroupedContextRunner.Execute(testCase, root, CancellationToken.None);
        }

        private static string OutputOf(TestResult result) =>
            result.Messages.Single(m => m.Category == TestResultMessage.StandardOutCategory).Text ?? "";

        // ── Passing subtree ───────────────────────────────────────────────────────────────────

        [Test]
        public void AllSpecsPass_OutcomeIsPassed()
        {
            var result = ExecuteGrouped<GroupedSpec>("grouped feature");
            Check.That(result.Outcome).IsEqualTo(TestOutcome.Passed);
        }

        [Test]
        public void Output_ContainsContextTreeAndTests()
        {
            var output = OutputOf(ExecuteGrouped<GroupedSpec>("grouped feature"));

            Check.That(output).Contains("grouped feature");
            Check.That(output).Contains("✓ direct test");
            Check.That(output).Contains("nested");
            Check.That(output).Contains("✓ nested test");
        }

        [Test]
        public void Output_ContainsSummaryLine()
        {
            var output = OutputOf(ExecuteGrouped<GroupedSpec>("grouped feature"));
            Check.That(output).Contains("2 passed, 0 failed, 2 total");
        }

        [Test]
        public void NestedTests_AreIndentedDeeperThanContext()
        {
            var output = OutputOf(ExecuteGrouped<GroupedSpec>("grouped feature"));
            var lines = output.Split('\n').Select(l => l.TrimEnd()).ToList();

            Check.That(lines).Contains("grouped feature");
            Check.That(lines).Contains("  ✓ direct test");
            Check.That(lines).Contains("  nested");
            Check.That(lines).Contains("    ✓ nested test");
        }

        // ── Failing subtree ───────────────────────────────────────────────────────────────────

        [Test]
        public void FailingSpec_OutcomeIsFailed_WithAggregatedErrorMessage()
        {
            GroupedSpecWithFailure.TriggerFailure = true;
            try
            {
                var result = ExecuteGrouped<GroupedSpecWithFailure>("grouped with failure");

                Check.That(result.Outcome).IsEqualTo(TestOutcome.Failed);
                Check.That(result.ErrorMessage).Contains("1 of 2 specs failed");
                Check.That(result.ErrorMessage).Contains("grouped intentional failure");
                Check.That(result.ErrorMessage).Contains("a failing spec");
            }
            finally { GroupedSpecWithFailure.TriggerFailure = false; }
        }

        [Test]
        public void FailingSpec_OutputMarksFailureAndListsDetails()
        {
            GroupedSpecWithFailure.TriggerFailure = true;
            try
            {
                var output = OutputOf(ExecuteGrouped<GroupedSpecWithFailure>("grouped with failure"));

                Check.That(output).Contains("✓ a passing spec");
                Check.That(output).Contains("✗ a failing spec - FAILED:");
                Check.That(output).Contains("grouped intentional failure");
                Check.That(output).Contains("1 passed, 1 failed, 2 total");
                Check.That(output).Contains("Failures:");
            }
            finally { GroupedSpecWithFailure.TriggerFailure = false; }
        }

        [Test]
        public void FailingSpec_PassingSpecsStillRun()
        {
            GroupedSpecWithFailure.TriggerFailure = true;
            try
            {
                var output = OutputOf(ExecuteGrouped<GroupedSpecWithFailure>("grouped with failure"));
                Check.That(output).Contains("✓ a passing spec");
            }
            finally { GroupedSpecWithFailure.TriggerFailure = false; }
        }

        // ── Missing context ───────────────────────────────────────────────────────────────────

        [Test]
        public void UnknownContext_OutcomeIsFailed()
        {
            var testCase = MakeGroupedTestCase<GroupedSpec>("grouped feature");
            testCase.SetPropertyValue(TestProperties.ContextPath, "no longer here");

            var root = SuiteTreeBuilder.BuildSuite(typeof(GroupedSpec));
            var result = GroupedContextRunner.Execute(testCase, root, CancellationToken.None);

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Failed);
            Check.That(result.ErrorMessage).Contains("no longer here");
        }
    }
}
