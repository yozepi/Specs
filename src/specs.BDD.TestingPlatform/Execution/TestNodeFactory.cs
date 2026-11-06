using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.TestHost;
using specs.BDD.Runner;
using specs.BDD.TestingPlatform.Discovery;

namespace specs.BDD.TestingPlatform.Execution
{
    /// <summary>
    /// Creates the <see cref="TestNodeUpdateMessage"/>s that describe spec nodes to the platform.
    /// </summary>
    /// <remarks>
    /// IDE clients get the full tree as group nodes with parent links, plus a method identifier on
    /// every leaf. Visual Studio ignores the group nodes and nests only Namespace › Class › Method › test,
    /// showing the method level only when two or more tests share it, so the [Feature] is the method.
    /// Each spec's name still carries its full context path, because VS Code shows only
    /// Namespace › Class › test. Console clients get flat leaves whose display
    /// name is the full tree path, since the console lists tests without hierarchy.
    /// </remarks>
    internal sealed class TestNodeFactory
    {
        private readonly SessionUid _sessionUid;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestNodeFactory"/> class.
        /// </summary>
        public TestNodeFactory(SessionUid sessionUid, bool isIdeClient)
        {
            _sessionUid = sessionUid;
            IsIdeClient = isIdeClient;
        }

        /// <summary>Whether nodes are being reported to an IDE test explorer.</summary>
        public bool IsIdeClient { get; }

        /// <summary>
        /// Returns the group nodes that lead to <paramref name="leaves"/>, parents first; empty for console clients.
        /// </summary>
        public IEnumerable<TestNodeUpdateMessage> Groups(IEnumerable<SpecNode> leaves)
        {
            if (!IsIdeClient) yield break;

            var published = new HashSet<SpecNode>();
            foreach (var leaf in leaves)
            {
                var chain = new Stack<SpecNode>(leaf.Ancestors());
                while (chain.Count > 0)
                {
                    var group = chain.Pop();
                    if (published.Add(group))
                        yield return Group(group);
                }
            }
        }

        /// <summary>Creates the update message for a group node (no state, so clients treat it as a group).</summary>
        public TestNodeUpdateMessage Group(SpecNode group)
        {
            var node = new TestNode
            {
                Uid = new TestNodeUid(group.Uid),
                DisplayName = group.Text,
                Properties = WithLocation(new PropertyBag(), group)
            };
            return new TestNodeUpdateMessage(_sessionUid, node, ParentOf(group));
        }

        /// <summary>Creates the update message for a leaf with the given state and extra properties.</summary>
        public TestNodeUpdateMessage Leaf(SpecNode leaf, params IProperty[] properties)
        {
            var bag = WithLocation(new PropertyBag(properties), leaf);
            bag.Add(MethodIdentifier(leaf));

            var node = new TestNode
            {
                Uid = new TestNodeUid(leaf.Uid),
                DisplayName = IsIdeClient ? IdeDisplayName(leaf) : leaf.Path,
                Properties = bag
            };
            return new TestNodeUpdateMessage(_sessionUid, node, ParentOf(leaf));
        }

        /// <summary>
        /// The spec's name from its [Feature] ("describe → context → … → spec"), so explorers that
        /// don't nest under the [Feature] (VS Code) still show it.
        /// </summary>
        private static string IdeDisplayName(SpecNode leaf)
        {
            if (leaf.Kind == SpecNodeKind.GroupedDescribe)
                return leaf.Text;

            var describe = DescribeOf(leaf);
            var segments = leaf.Ancestors()
                .TakeWhile(a => a != describe)
                .Reverse()
                .Select(a => a.Text)
                .Prepend(describe.Text)
                .Append(leaf.Text);

            return string.Join(ContextPathFormat.Separator, segments);
        }

        private static SpecNode DescribeOf(SpecNode leaf) =>
            leaf.Kind == SpecNodeKind.GroupedDescribe
                ? leaf
                : leaf.Ancestors().First(a => a.Kind == SpecNodeKind.Describe);

        /// <summary>
        /// Identifies the leaf as Namespace › Class › [Feature] method, so IDEs can resolve the
        /// method symbol (CodeLens, navigation).
        /// </summary>
        private static TestMethodIdentifierProperty MethodIdentifier(SpecNode leaf)
        {
            var type = leaf.SpecClass!.Type;
            var className = leaf.Ancestors().First(a => a.Kind == SpecNodeKind.Class).Text;
            var describe = DescribeOf(leaf);

            return new TestMethodIdentifierProperty(
                type.Assembly.FullName ?? string.Empty,
                type.Namespace ?? string.Empty,
                className,
                describe.Method?.Name ?? describe.Text,
                0,
                Array.Empty<string>(),
                "System.Void");
        }

        private TestNodeUid? ParentOf(SpecNode node) =>
            IsIdeClient && node.Parent != null ? new TestNodeUid(node.Parent.Uid) : null;

        private static PropertyBag WithLocation(PropertyBag bag, SpecNode node)
        {
            if (node.Location != null)
            {
                var position = new LinePosition(node.Location.LineNumber, -1);
                bag.Add(new TestFileLocationProperty(node.Location.FilePath, new LinePositionSpan(position, position)));
            }

            return bag;
        }
    }
}
