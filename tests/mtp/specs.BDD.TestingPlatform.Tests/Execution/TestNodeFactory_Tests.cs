using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.TestHost;
using NFluent;
using NUnit.Framework;
using specs.BDD.Runner;
using specs.BDD.TestingPlatform.Discovery;
using specs.BDD.TestingPlatform.Execution;
using specs.BDD.TestingPlatform.Tests.TestFixtures;
using static specs.BDD.TestingPlatform.Tests.Discovery.SpecTreeBuilder_Tests;

namespace specs.BDD.TestingPlatform.Tests.Execution
{
    [TestFixture]
    public class TestNodeFactory_Tests
    {
        private static readonly SessionUid Session = new SessionUid("session");

        private SpecForest _forest = null!;

        [SetUp]
        public void SetUp()
        {
            _forest = SpecTreeBuilder.Build(typeof(TreeSpec).Assembly, configuredStructure: null);
        }

        private SpecNode Leaf(string path) => _forest.Leaves.Single(l => l.Path == path);

        [Test]
        public void Groups_IdeClient_PublishesAncestorsParentsFirst()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: true);
            var leaf = Leaf($"{Ns}/TreeSpec/Cart/when empty/shows a message");

            var groups = factory.Groups(new[] { leaf }).ToList();

            Check.That(groups.Select(g => g.TestNode.Uid.Value)).ContainsExactly(
                $"{Ns}/", $"{Ns}/TreeSpec/", $"{Ns}/TreeSpec/Cart/", $"{Ns}/TreeSpec/Cart/when empty/");
            Check.That(groups[0].ParentTestNodeUid).IsNull();
            Check.That(groups[1].ParentTestNodeUid!.Value).IsEqualTo($"{Ns}/");
        }

        [Test]
        public void Groups_SharedAncestors_ArePublishedOnce()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: true);
            var leaves = new[] { Leaf($"{Ns}/TreeSpec/Cart/adds an item"), Leaf($"{Ns}/TreeSpec/Cart/removes an item") };

            Check.That(factory.Groups(leaves)).HasSize(3);
        }

        [Test]
        public void Group_HasNoStateSoClientsTreatItAsAGroup()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: true);
            var group = factory.Groups(new[] { Leaf($"{Ns}/TreeSpec/Cart/adds an item") }).Last();

            Check.That(group.TestNode.DisplayName).IsEqualTo("Cart");
            Check.That(group.TestNode.Properties.Any<TestNodeStateProperty>()).IsFalse();
        }

        [Test]
        public void Leaf_IdeClient_ShowsContextPathFromTheDescribe()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: true);
            var leaf = Leaf($"{Ns}/TreeSpec/Cart/when empty/shows a message");

            var message = factory.Leaf(leaf, DiscoveredTestNodeStateProperty.CachedInstance);

            Check.That(message.TestNode.Uid.Value).IsEqualTo(leaf.Path);
            Check.That(message.TestNode.DisplayName).IsEqualTo($"Cart{ContextPathFormat.Separator}when empty{ContextPathFormat.Separator}shows a message");
            Check.That(message.ParentTestNodeUid!.Value).IsEqualTo($"{Ns}/TreeSpec/Cart/when empty/");
        }

        [Test]
        public void Leaf_IdeClient_SpecDirectlyInDescribe_IncludesTheDescribe()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: true);

            var message = factory.Leaf(Leaf($"{Ns}/TreeSpec/Cart/adds an item"), DiscoveredTestNodeStateProperty.CachedInstance);

            Check.That(message.TestNode.DisplayName).IsEqualTo($"Cart{ContextPathFormat.Separator}adds an item");
        }

        [Test]
        public void Leaf_IdeClient_GroupedDescribe_ShowsTheDescribe()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: true);

            var message = factory.Leaf(Leaf($"{Ns}/GroupedTreeSpec/grouped"), DiscoveredTestNodeStateProperty.CachedInstance);

            Check.That(message.TestNode.DisplayName).IsEqualTo("grouped");
        }

        [Test]
        public void Groups_ConsoleClient_PublishesNothing()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: false);

            Check.That(factory.Groups(_forest.Leaves)).IsEmpty();
        }

        [Test]
        public void Leaf_ConsoleClient_ShowsFullPathWithoutParent()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: false);
            var leaf = Leaf($"{Ns}/TreeSpec/Cart/when empty/shows a message");

            var message = factory.Leaf(leaf, DiscoveredTestNodeStateProperty.CachedInstance);

            Check.That(message.TestNode.DisplayName).IsEqualTo(leaf.Path);
            Check.That(message.ParentTestNodeUid).IsNull();
        }

        [Test]
        public void Leaf_CarriesStateAndSourceLocation()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: false);
            var leaf = Leaf($"{Ns}/TreeSpec/Cart/adds an item");

            var properties = factory.Leaf(leaf, DiscoveredTestNodeStateProperty.CachedInstance).TestNode.Properties;

            Check.That(properties.Single<TestNodeStateProperty>()).IsInstanceOf<DiscoveredTestNodeStateProperty>();
            Check.That(properties.Single<TestFileLocationProperty>().FilePath).EndsWith("TreeSpecs.cs");
            Check.That(properties.Single<TestFileLocationProperty>().LineSpan.Start.Line).IsEqualTo(leaf.Location!.LineNumber);
        }

        [Test]
        public void Leaf_MethodIdentifier_IsNamespaceClassAndDescribeMethod()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: true);
            var leaf = Leaf($"{Ns}/TreeSpec/Cart/when empty/shows a message");

            var identifier = factory.Leaf(leaf, DiscoveredTestNodeStateProperty.CachedInstance)
                .TestNode.Properties.Single<TestMethodIdentifierProperty>();

            Check.That(identifier.Namespace).IsEqualTo("specs.BDD.TestingPlatform.Tests.TestFixtures");
            Check.That(identifier.TypeName).IsEqualTo("TreeSpec");
            Check.That(identifier.MethodName).IsEqualTo("describe_cart");
            Check.That(identifier.AssemblyFullName).IsEqualTo(typeof(TreeSpec).Assembly.FullName);
        }

        [Test]
        public void Leaf_GroupedDescribe_MethodIdentifierIsTheDescribeMethod()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: true);
            var leaf = Leaf($"{Ns}/GroupedTreeSpec/grouped");

            var identifier = factory.Leaf(leaf, DiscoveredTestNodeStateProperty.CachedInstance)
                .TestNode.Properties.Single<TestMethodIdentifierProperty>();

            Check.That(identifier.TypeName).IsEqualTo("GroupedTreeSpec");
            Check.That(identifier.MethodName).IsEqualTo("describe_grouped");
        }

        [Test]
        public void Leaf_NestedClass_MethodIdentifierUsesPlusSeparatedName()
        {
            var factory = new TestNodeFactory(Session, isIdeClient: true);
            var leaf = Leaf($"{Ns}/OuterSpec+InnerSpec/inner/works");

            var identifier = factory.Leaf(leaf, DiscoveredTestNodeStateProperty.CachedInstance)
                .TestNode.Properties.Single<TestMethodIdentifierProperty>();

            Check.That(identifier.TypeName).IsEqualTo("OuterSpec+InnerSpec");
        }
    }
}
