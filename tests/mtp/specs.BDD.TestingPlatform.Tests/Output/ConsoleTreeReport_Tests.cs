using NFluent;
using NUnit.Framework;
using specs.BDD.Runner;
using specs.BDD.TestingPlatform.Discovery;
using specs.BDD.TestingPlatform.Execution;
using specs.BDD.TestingPlatform.Output;
using specs.BDD.TestingPlatform.Tests.TestFixtures;
using static specs.BDD.TestingPlatform.Tests.Discovery.SpecTreeBuilder_Tests;

namespace specs.BDD.TestingPlatform.Tests.Output
{
    [TestFixture]
    public class ConsoleTreeReport_Tests
    {
        private SpecForest _forest = null!;

        [SetUp]
        public void SetUp()
        {
            _forest = SpecTreeBuilder.Build(typeof(TreeSpec).Assembly, configuredStructure: null);
        }

        private List<SpecNode> LeavesUnder(string path) =>
            _forest.Leaves.Where(l => l.Path.StartsWith(path + "/")).ToList();

        private static string Lines(params string[] lines) => string.Join(Environment.NewLine, lines);

        [Test]
        public void Render_Discovery_ListsTheSelectedBranchIndented()
        {
            var report = ConsoleTreeReport.Render(_forest, LeavesUnder($"{Ns}/TreeSpec/Cart"), null, MarkerStyle.Plain);

            Check.That(report).IsEqualTo(Lines(
                "specs.BDD.TestingPlatform.Tests",
                "  specs.BDD.TestingPlatform.Tests.TestFixtures",
                "    TreeSpec",
                "      Cart",
                "        - adds an item",
                "        - removes an item",
                "        when empty",
                "          - shows a message"));
        }

        [Test]
        public void Render_Discovery_MarksPendingSpecs()
        {
            var report = ConsoleTreeReport.Render(_forest, LeavesUnder($"{Ns}/PendingTreeSpec"), null, MarkerStyle.Plain);

            Check.That(report).Contains("- waits - PENDING");
            Check.That(report).Contains("- runs" + Environment.NewLine);
        }

        [Test]
        public void Render_Run_UsesOutcomeMarkers()
        {
            var leaves = LeavesUnder($"{Ns}/TreeSpec/Cart");
            var outcomes = new Dictionary<SpecNode, SpecOutcome>
            {
                [leaves[0]] = SpecOutcome.Passed,
                [leaves[1]] = SpecOutcome.Failed
            };

            var report = ConsoleTreeReport.Render(_forest, leaves, outcomes, MarkerStyle.Plain);

            Check.That(report).Contains("        ✓ adds an item");
            Check.That(report).Contains("        ✗ removes an item - FAILED");
            Check.That(report).Contains("          - shows a message - NOT RUN");
        }
    }
}
