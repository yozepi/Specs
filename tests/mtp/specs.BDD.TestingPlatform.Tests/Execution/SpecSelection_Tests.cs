using System.Reflection;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Requests;
using Moq;
using NFluent;
using NUnit.Framework;
using specs.BDD.TestingPlatform.Discovery;
using specs.BDD.TestingPlatform.Execution;
using specs.BDD.TestingPlatform.Tests.TestFixtures;
using static specs.BDD.TestingPlatform.Tests.Discovery.SpecTreeBuilder_Tests;

namespace specs.BDD.TestingPlatform.Tests.Execution
{
    [TestFixture]
    public class SpecSelection_Tests
    {
        private SpecForest _forest = null!;

        [SetUp]
        public void SetUp()
        {
            _forest = SpecTreeBuilder.Build(typeof(TreeSpec).Assembly, configuredStructure: null);
        }

        // The platform builds TreeNodeFilter from --treenode-filter; its constructor is internal.
        private static TreeNodeFilter TreeFilter(string filter) =>
            (TreeNodeFilter)typeof(TreeNodeFilter)
                .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string) }, null)!
                .Invoke(new object[] { filter });

        private static TestNodeUidListFilter UidFilter(params string[] uids) =>
            new TestNodeUidListFilter(uids.Select(u => new TestNodeUid(u)).ToArray());

        private IEnumerable<string> Paths(ITestExecutionFilter? filter) =>
            SpecSelection.Select(_forest, filter).Select(l => l.Path);

        [Test]
        public void Select_NoFilter_SelectsEveryLeaf()
        {
            Check.That(SpecSelection.Select(_forest, null)).HasSize(_forest.Leaves.Count);
            Check.That(SpecSelection.Select(_forest, new NopFilter())).HasSize(_forest.Leaves.Count);
        }

        [Test]
        public void Select_LeafUid_SelectsOnlyThatLeaf()
        {
            var uid = $"{Ns}/TreeSpec/Cart/adds an item";

            Check.That(Paths(UidFilter(uid))).ContainsExactly(uid);
        }

        [Test]
        public void Select_GroupUid_SelectsEveryLeafBeneathIt()
        {
            Check.That(Paths(UidFilter($"{Ns}/TreeSpec/Cart/"))).ContainsExactly(
                $"{Ns}/TreeSpec/Cart/adds an item",
                $"{Ns}/TreeSpec/Cart/removes an item",
                $"{Ns}/TreeSpec/Cart/when empty/shows a message");
        }

        [Test]
        public void Select_TreeNodeFilterWithAllBelow_SelectsTheSubtree()
        {
            Check.That(Paths(TreeFilter("/*/*/TreeSpec/Cart/**"))).HasSize(3);
        }

        [Test]
        public void Select_TreeNodeFilter_MatchesOnlyLeavesAtTheFilterDepth()
        {
            Check.That(Paths(TreeFilter("/*/*/TreeSpec/Cart/*"))).ContainsExactly(
                $"{Ns}/TreeSpec/Cart/adds an item",
                $"{Ns}/TreeSpec/Cart/removes an item");
        }

        [Test]
        public void Select_TreeNodeFilter_DoesNotMatchAShallowerGroupedLeaf()
        {
            Check.That(Paths(TreeFilter("/*/*/GroupedTreeSpec/grouped/*"))).IsEmpty();
            Check.That(Paths(TreeFilter("/*/*/GroupedTreeSpec/grouped"))).ContainsExactly($"{Ns}/GroupedTreeSpec/grouped");
        }

        [Test]
        public void Select_TreeNodeFilter_MatchesEscapedSegments()
        {
            Check.That(Paths(TreeFilter("/*/*/SlashTreeSpec/input%2Foutput/*"))).HasSize(1);
        }

        [Test]
        public void Select_EmptyUidList_SelectsNothing()
        {
            Check.That(Paths(UidFilter())).IsEmpty();
        }

        [Test]
        public void Select_EmptyUidList_WhenEmptyListSelectsAll_SelectsEveryLeaf()
        {
            Check.That(SpecSelection.Select(_forest, UidFilter(), emptyUidListSelectsAll: true)).HasSize(_forest.Leaves.Count);
        }

        [Test]
        public void Select_NonEmptyUidList_WhenEmptyListSelectsAll_SelectsOnlyThoseUids()
        {
            var uid = $"{Ns}/TreeSpec/Cart/adds an item";

            Check.That(SpecSelection.Select(_forest, UidFilter(uid), emptyUidListSelectsAll: true).Select(l => l.Path))
                .ContainsExactly(uid);
        }

        [Test]
        public void Describe_SummarizesEachFilterKind()
        {
            Check.That(SpecSelection.Describe(null)).IsEqualTo("none");
            Check.That(SpecSelection.Describe(new NopFilter())).IsEqualTo("NopFilter");
            Check.That(SpecSelection.Describe(TreeFilter("/*/*/TreeSpec/**"))).IsEqualTo("TreeNodeFilter '/*/*/TreeSpec/**'");
            Check.That(SpecSelection.Describe(UidFilter("a", "b"))).IsEqualTo("TestNodeUidListFilter (2 uids: 'a', 'b')");
        }

        [Test]
        public void Describe_UidList_ShowsOnlyTheFirstThreeUids()
        {
            Check.That(SpecSelection.Describe(UidFilter("a", "b", "c", "d")))
                .IsEqualTo("TestNodeUidListFilter (4 uids: 'a', 'b', 'c', …)");
        }

        [Test]
        public void Describe_Composite_ListsItsParts()
        {
            var filter = new CompositeTestExecutionFilter(
                TestExecutionFilterOperator.And,
                new ITestExecutionFilter[] { new NopFilter(), UidFilter("a") });

            Check.That(SpecSelection.Describe(filter))
                .IsEqualTo("CompositeTestExecutionFilter (NopFilter; TestNodeUidListFilter (1 uids: 'a'))");
        }

        [Test]
        public void Select_CompositeFilter_RequiresEveryFilter()
        {
            var uid = $"{Ns}/TreeSpec/Cart/adds an item";
            var filter = new CompositeTestExecutionFilter(
                TestExecutionFilterOperator.And,
                new ITestExecutionFilter[] { TreeFilter("/*/*/TreeSpec/**"), UidFilter(uid, $"{Ns}/PendingTreeSpec/pending/runs") });

            Check.That(Paths(filter)).ContainsExactly(uid);
        }

        [Test]
        public void Select_UnknownFilter_Throws()
        {
            var filter = new Mock<ITestExecutionFilter>().Object;

            Check.ThatCode(() => SpecSelection.Select(_forest, filter)).Throws<NotSupportedException>();
        }

        [TestCase("/a/b", 2)]
        [TestCase("/a/b/**", 3)]
        [TestCase("/a/b[Key=x/y]", 2)]
        [TestCase("/a/b\\/c", 2)]
        public void SegmentCount_CountsOnlyPathSeparators(string path, int expected)
        {
            Check.That(SpecSelection.SegmentCount(path)).IsEqualTo(expected);
        }
    }
}
