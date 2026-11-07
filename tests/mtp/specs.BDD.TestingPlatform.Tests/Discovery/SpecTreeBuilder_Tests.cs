using NFluent;
using NUnit.Framework;
using specs.BDD.Attributes;
using specs.BDD.Runner;
using specs.BDD.TestingPlatform.Discovery;
using specs.BDD.TestingPlatform.Tests.TestFixtures;

namespace specs.BDD.TestingPlatform.Tests.Discovery
{
    [TestFixture]
    public class SpecTreeBuilder_Tests
    {
        internal const string Ns = "/specs.BDD.TestingPlatform.Tests/specs.BDD.TestingPlatform.Tests.TestFixtures";

        private SpecForest _forest = null!;

        [SetUp]
        public void SetUp()
        {
            _forest = SpecTreeBuilder.Build(typeof(TreeSpec).Assembly, configuredStructure: null);
        }

        private SpecNode Leaf(string path) => _forest.Leaves.Single(l => l.Path == path);

        [Test]
        public void Build_SpecLeaf_HasFullTreePath()
        {
            var leaf = Leaf($"{Ns}/TreeSpec/Cart/when empty/shows a message");

            Check.That(leaf.Kind).IsEqualTo(SpecNodeKind.Spec);
            Check.That(leaf.Text).IsEqualTo("shows a message");
            Check.That(leaf.TestDescription).IsEqualTo("shows a message");
            Check.That(leaf.ContextPath).IsEqualTo($"Cart{ContextPathFormat.Separator}when empty");
        }

        [Test]
        public void Build_SpecLeaf_AncestorsAreContextDescribeClassNamespace()
        {
            var leaf = Leaf($"{Ns}/TreeSpec/Cart/when empty/shows a message");

            Check.That(leaf.Ancestors().Select(a => a.Kind)).ContainsExactly(
                SpecNodeKind.Context, SpecNodeKind.Describe, SpecNodeKind.Class, SpecNodeKind.Namespace);
            Check.That(leaf.Ancestors().Select(a => a.Text)).ContainsExactly(
                "when empty", "Cart", "TreeSpec", "specs.BDD.TestingPlatform.Tests.TestFixtures");
        }

        [Test]
        public void Build_Uids_GroupsEndWithSeparatorAndLeavesDoNot()
        {
            var leaf = Leaf($"{Ns}/TreeSpec/Cart/adds an item");

            Check.That(leaf.Uid).IsEqualTo(leaf.Path);
            Check.That(leaf.Parent!.Uid).IsEqualTo($"{Ns}/TreeSpec/Cart/");
        }

        [Test]
        public void Build_SpecsBeforeChildContexts_InDeclarationOrder()
        {
            var cart = Leaf($"{Ns}/TreeSpec/Cart/adds an item").Parent!;

            Check.That(cart.Children.Select(c => c.Text)).ContainsExactly("adds an item", "removes an item", "when empty");
        }

        [Test]
        public void Build_DescribeWithoutDescription_UsesMethodName()
        {
            Check.That(_forest.Leaves.Select(l => l.Path)).Contains($"{Ns}/TreeSpec/describe_checkout/charges the card");
        }

        [Test]
        public void Build_GroupedDescribe_IsSingleLeaf()
        {
            var grouped = Leaf($"{Ns}/GroupedTreeSpec/grouped");

            Check.That(grouped.Kind).IsEqualTo(SpecNodeKind.GroupedDescribe);
            Check.That(grouped.ContextPath).IsEqualTo("grouped");
            Check.That(grouped.TestDescription).IsEmpty();
            Check.That(grouped.IsPending).IsFalse();
            Check.That(_forest.Leaves.Any(l => l.Path.StartsWith($"{Ns}/GroupedTreeSpec/grouped/"))).IsFalse();
        }

        [Test]
        public void Build_GroupedDescribeWithOnlyPendingSpecs_IsPending()
        {
            Check.That(Leaf($"{Ns}/GroupedTreeSpec/all pending").IsPending).IsTrue();
        }

        [Test]
        public void Build_PendingSpec_IsMarkedPending()
        {
            Check.That(Leaf($"{Ns}/PendingTreeSpec/pending/waits").IsPending).IsTrue();
            Check.That(Leaf($"{Ns}/PendingTreeSpec/pending/runs").IsPending).IsFalse();
        }

        [Test]
        public void Build_ConfiguredFeatureLevel_GroupsFeaturesWithoutTheirOwnSetting()
        {
            var forest = SpecTreeBuilder.Build(typeof(TreeSpec).Assembly, ReportingLevel.Feature);

            var cart = forest.Leaves.Single(l => l.Path == $"{Ns}/TreeSpec/Cart");
            Check.That(cart.Kind).IsEqualTo(SpecNodeKind.GroupedDescribe);
        }

        [Test]
        public void Build_SegmentsWithSlashOrPercent_AreEscaped()
        {
            var leaf = Leaf($"{Ns}/SlashTreeSpec/input%2Foutput/100%25 done");

            Check.That(leaf.Text).IsEqualTo("100% done");
            Check.That(leaf.Parent!.Text).IsEqualTo("input/output");
        }

        [Test]
        public void Build_NestedClass_UsesPlusSeparatedName()
        {
            Check.That(_forest.Leaves.Select(l => l.Path)).Contains($"{Ns}/OuterSpec+InnerSpec/inner/works");
        }

        [Test]
        public void Build_ClassWithoutDescribe_ReportsWarningAndAddsNoNodes()
        {
            Check.That(_forest.Warnings.Any(w => w.Contains(nameof(NoDescribeSpec)))).IsTrue();
            Check.That(_forest.Leaves.Any(l => l.Path.Contains($"/{nameof(NoDescribeSpec)}/"))).IsFalse();
        }

        [Test]
        public void Build_SpecLeaf_PointsAtItsSourceFile()
        {
            var leaf = Leaf($"{Ns}/TreeSpec/Cart/adds an item");

            Check.That(leaf.Location).IsNotNull();
            Check.That(leaf.Location!.FilePath).EndsWith("TreeSpecs.cs");
        }

        [Test]
        public void Build_TodoSpecLeaf_PointsAtItsItDeclaration()
        {
            var leaf = Leaf($"{Ns}/PendingTreeSpec/pending/waits");

            Check.That(leaf.Location).IsNotNull();
            Check.That(leaf.Location!.FilePath.Replace('\\', '/').EndsWith("/TestFixtures/TreeSpecs.cs")).IsTrue();
            var sourcePath = Path.Combine(AppContext.BaseDirectory, "TestFixtures", "TreeSpecs.cs");
            var declaredLine = File.ReadAllLines(sourcePath)[leaf.Location.LineNumber - 1];
            Check.That(declaredLine).Contains("It[\"waits\"] = TODO;");
        }

        [Test]
        public void Build_Roots_AreNamespaceNodesUnderTheAssembly()
        {
            Check.That(_forest.AssemblyName).IsEqualTo("specs.BDD.TestingPlatform.Tests");
            Check.That(_forest.Roots.Select(r => r.Path)).ContainsExactly(Ns);
        }
    }
}
