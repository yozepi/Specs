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

        private static TestResult ExecuteGrouped<TSpec>(string contextPath, bool showSourceLocations = true, MarkerStyle markerStyle = MarkerStyle.Plain)
            where TSpec : specs.BDD.Specifications
        {
            var testCase = MakeGroupedTestCase<TSpec>(contextPath);
            var root = SuiteTreeBuilder.BuildSuite(typeof(TSpec));
            return GroupedContextRunner.Execute(testCase, root, null, showSourceLocations, markerStyle, CancellationToken.None);
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

        // ── ShouldThrow ───────────────────────────────────────────────────────────────────────

        [Test]
        public void ShouldThrow_ExpectedActExceptions_OutcomeIsPassed()
        {
            var result = ExecuteGrouped<GroupedShouldThrowSpec>("grouped should throw");

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Passed);
            Check.That(OutputOf(result)).Contains("3 passed, 0 failed, 3 total");
        }

        [Test]
        public void ShouldThrow_UnexpectedActException_FailsOnlyThePlainSpec()
        {
            GroupedShouldThrowSpec.TriggerFailure = true;
            try
            {
                var result = ExecuteGrouped<GroupedShouldThrowSpec>("grouped should throw");
                var output = OutputOf(result);

                Check.That(result.Outcome).IsEqualTo(TestOutcome.Failed);
                Check.That(output).Contains("✗ a plain spec - FAILED:");
                Check.That(output).Contains("grouped act failed");
                Check.That(output).Contains("2 passed, 1 failed, 3 total");
            }
            finally { GroupedShouldThrowSpec.TriggerFailure = false; }
        }

        // ── Pending specs ─────────────────────────────────────────────────────────────────────

        [Test]
        public void MixedPending_OutcomeIsPassed()
        {
            var result = ExecuteGrouped<GroupedSpecWithPending>("grouped with pending");
            Check.That(result.Outcome).IsEqualTo(TestOutcome.Passed);
        }

        [Test]
        public void MixedPending_OutputMarksPendingSpecsAndCountsThem()
        {
            var output = OutputOf(ExecuteGrouped<GroupedSpecWithPending>("grouped with pending"));

            Check.That(output).Contains("✓ a passing spec");
            Check.That(output).Contains("○ a todo spec - PENDING");
            Check.That(output).Contains("pending inner");
            Check.That(output).Contains("○ inner spec - PENDING");
            Check.That(output).Contains("1 passed, 0 failed, 2 pending, 3 total");
        }

        [Test]
        public void PendingContext_BeforeAllDoesNotRun()
        {
            GroupedSpecWithPending.ClearLog();

            ExecuteGrouped<GroupedSpecWithPending>("grouped with pending");

            Check.That(GroupedSpecWithPending.Log).IsEmpty();
        }

        [Test]
        public void AllPending_OutcomeIsSkipped()
        {
            var result = ExecuteGrouped<GroupedSpecWithPending>("grouped all pending");

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Skipped);
            Check.That(result.ErrorMessage).IsEqualTo("All specs are pending.");
        }

        [Test]
        public void AllPending_ContextBeforeAllDoesNotRun()
        {
            GroupedSpecWithPending.ClearLog();

            ExecuteGrouped<GroupedSpecWithPending>("grouped all pending");

            Check.That(GroupedSpecWithPending.Log).IsEmpty();
        }

        [Test]
        public void RootSetupFailure_PendingSpecsStayPending()
        {
            var testCase = MakeGroupedTestCase<GroupedSpecWithPending>("grouped with pending");
            var root = SuiteTreeBuilder.BuildSuite(typeof(GroupedSpecWithPending));

            var result = GroupedContextRunner.Execute(
                testCase, root, new InvalidOperationException("root boom"), false, MarkerStyle.Plain, CancellationToken.None);
            var output = OutputOf(result);

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Failed);
            Check.That(output).Contains("✗ a passing spec - FAILED: BeforeAll failed:");
            Check.That(output).Contains("○ a todo spec - PENDING");
            Check.That(output).Contains("○ inner spec - PENDING");
            Check.That(result.ErrorMessage).Contains("1 of 3 specs failed");
        }

        [Test]
        public void AnsiMarkers_PendingMarkerIsYellow()
        {
            var output = OutputOf(ExecuteGrouped<GroupedSpecWithPending>("grouped with pending", markerStyle: MarkerStyle.Ansi));

            Check.That(output).Contains("\u001b[33m○\u001b[0m a todo spec - PENDING");
        }

        // ── Source locations ──

        [Test]
        public void AnsiMarkers_PassMarkerIsGreen()
        {
            var output = OutputOf(ExecuteGrouped<GroupedSpec>("grouped feature", markerStyle: MarkerStyle.Ansi));

            Check.That(output).Contains("\u001b[32m✓\u001b[0m direct test");
        }

        [Test]
        public void AnsiMarkers_FailMarkerIsRed()
        {
            GroupedSpecWithFailure.TriggerFailure = true;
            try
            {
                var output = OutputOf(ExecuteGrouped<GroupedSpecWithFailure>("grouped with failure", markerStyle: MarkerStyle.Ansi));

                Check.That(output).Contains("\u001b[31m✗\u001b[0m a failing spec - FAILED:");
            }
            finally { GroupedSpecWithFailure.TriggerFailure = false; }
        }

        [Test]
        public void PlainMarkers_HasNoEscapeCodes()
        {
            var output = OutputOf(ExecuteGrouped<GroupedSpec>("grouped feature", markerStyle: MarkerStyle.Plain));

            Check.That(output).Not.Contains("\u001b[");
        }

        [Test]
        public void EmojiMarkers_UsesColoredGlyphsWithoutEscapeCodes()
        {
            GroupedSpecWithFailure.TriggerFailure = true;
            try
            {
                var output = OutputOf(ExecuteGrouped<GroupedSpecWithFailure>("grouped with failure", markerStyle: MarkerStyle.Emoji));

                Check.That(output).Contains("✅ a passing spec");
                Check.That(output).Contains("❌ a failing spec - FAILED:");
                Check.That(output).Not.Contains("\u001b[");
            }
            finally { GroupedSpecWithFailure.TriggerFailure = false; }
        }

        // ── Source locations ──

        private static readonly System.Text.RegularExpressions.Regex LocationLine =
            new System.Text.RegularExpressions.Regex(@"^\s+at .+SampleSpecifications\.cs:line \d+$");

        private static System.Collections.Generic.List<string> LinesOf(TestResult result) =>
            OutputOf(result).Split('\n').Select(l => l.TrimEnd('\r')).ToList();

        private static string LineAfter(System.Collections.Generic.List<string> lines, string line) =>
            lines[lines.IndexOf(line) + 1];

        [Test]
        public void SourceLocations_AppearUnderEachPassingTest()
        {
            var lines = LinesOf(ExecuteGrouped<GroupedSpec>("grouped feature"));

            Check.That(LocationLine.IsMatch(LineAfter(lines, "  ✓ direct test"))).IsTrue();
            Check.That(LocationLine.IsMatch(LineAfter(lines, "    ✓ nested test"))).IsTrue();
            Check.That(LineAfter(lines, "  ✓ direct test")).StartsWith("    at ");
        }

        [Test]
        public void SourceLocations_AppearUnderFailingTest()
        {
            GroupedSpecWithFailure.TriggerFailure = true;
            try
            {
                var lines = LinesOf(ExecuteGrouped<GroupedSpecWithFailure>("grouped with failure"));

                Check.That(LocationLine.IsMatch(LineAfter(lines, "    ✗ a failing spec - FAILED:"))).IsTrue();
            }
            finally { GroupedSpecWithFailure.TriggerFailure = false; }
        }

        [Test]
        public void SourceLocations_Disabled_AreOmitted()
        {
            var lines = LinesOf(ExecuteGrouped<GroupedSpec>("grouped feature", showSourceLocations: false));

            Check.That(lines.Any(LocationLine.IsMatch)).IsFalse();
        }

        [Test]
        public void SourceLocations_AsyncTest_PointsToSpecFile()
        {
            var lines = LinesOf(ExecuteGrouped<GroupedAsyncSpec>("grouped async"));

            Check.That(LocationLine.IsMatch(LineAfter(lines, "  ✓ async test"))).IsTrue();
        }

        [Test]
        public void SourceLocations_AppearUnderRootSetupFailures()
        {
            var lines = LinesOf(ExecuteGroupedWithRootFailure());

            Check.That(LocationLine.IsMatch(LineAfter(lines, "  ✗ grouped test - FAILED: BeforeAll failed:"))).IsTrue();
        }

        // ── Missing context ───────────────────────────────────────────────────────────────────

        [Test]
        public void UnknownContext_OutcomeIsFailed()
        {
            var testCase = MakeGroupedTestCase<GroupedSpec>("grouped feature");
            testCase.SetPropertyValue(TestProperties.ContextPath, "no longer here");

            var root = SuiteTreeBuilder.BuildSuite(typeof(GroupedSpec));
            var result = GroupedContextRunner.Execute(testCase, root, null, showSourceLocations: true, MarkerStyle.Plain, CancellationToken.None);

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Failed);
            Check.That(result.ErrorMessage).Contains("no longer here");
        }

        // ── Root BeforeAll failure ────────────────────────────────────────────────────────────

        private static TestResult ExecuteGroupedWithRootFailure()
        {
            var testCase = MakeGroupedTestCase<SpecWithFailingRootBeforeAll>("grouped part");
            var root = SuiteTreeBuilder.BuildSuite(typeof(SpecWithFailingRootBeforeAll));
            return GroupedContextRunner.Execute(
                testCase, root, new InvalidOperationException("root setup failure"), showSourceLocations: true, MarkerStyle.Plain, CancellationToken.None);
        }

        [Test]
        public void RootSetupFailure_OutcomeIsFailed()
        {
            var result = ExecuteGroupedWithRootFailure();

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Failed);
            Check.That(result.ErrorMessage).Contains("2 of 2 specs failed", "root setup failure");
        }

        [Test]
        public void RootSetupFailure_OutputShowsFailedTree()
        {
            var output = OutputOf(ExecuteGroupedWithRootFailure());

            Check.That(output).Contains(
                "grouped part",
                "✗ grouped test - FAILED: BeforeAll failed:",
                "nested",
                "✗ nested test - FAILED: BeforeAll failed:",
                "0 passed, 2 failed, 2 total");
        }
    }
}
