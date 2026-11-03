using System;
using System.Collections.Generic;
using specs.BDD.Runner;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.TestingPlatform.Discovery
{
    /// <summary>The level a <see cref="SpecNode"/> represents in the spec tree.</summary>
    internal enum SpecNodeKind
    {
        Namespace,
        Class,
        Describe,
        Context,
        Spec,
        GroupedDescribe
    }

    /// <summary>
    /// A node of the discovered spec tree: a group (namespace, class, [Describe], context) or a
    /// runnable leaf (an It[...] spec, or a [Describe] that runs as one grouped unit).
    /// </summary>
    internal sealed class SpecNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecNode"/> class and attaches it to <paramref name="parent"/>.
        /// </summary>
        public SpecNode(SpecNodeKind kind, string text, string path, SpecNode? parent, SpecClass? specClass)
        {
            Kind = kind;
            Text = text;
            Path = path;
            Parent = parent;
            SpecClass = specClass;
            parent?.Children.Add(this);
        }

        /// <summary>The level this node represents.</summary>
        public SpecNodeKind Kind { get; }

        /// <summary>The unescaped name shown for this node (namespace, class name, or description).</summary>
        public string Text { get; }

        /// <summary>
        /// The '/'-separated tree path starting with the assembly name; segments are escaped with
        /// <see cref="SpecPath.Escape"/>. This is the path matched by <c>--treenode-filter</c>.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The stable test node UID: the path for leaves, the path plus a trailing '/' for groups,
        /// so a group and a spec with the same name can never collide.
        /// </summary>
        public string Uid => IsLeaf ? Path : Path + SpecPath.Separator;

        /// <summary>The parent node; <c>null</c> for namespace nodes.</summary>
        public SpecNode? Parent { get; }

        /// <summary>The child nodes in declaration order.</summary>
        public List<SpecNode> Children { get; } = new List<SpecNode>();

        /// <summary>The specification class this node belongs to; <c>null</c> for namespace nodes.</summary>
        public SpecClass? SpecClass { get; }

        /// <summary>Whether this node is a runnable test.</summary>
        public bool IsLeaf => Kind == SpecNodeKind.Spec || Kind == SpecNodeKind.GroupedDescribe;

        /// <summary>
        /// For leaves: the context descriptions from the [Describe] down, joined with
        /// <see cref="ContextPathFormat.Separator"/> (the [Describe] description only for grouped leaves).
        /// </summary>
        public string ContextPath { get; set; } = string.Empty;

        /// <summary>For spec leaves: the It[...] description; empty otherwise.</summary>
        public string TestDescription { get; set; } = string.Empty;

        /// <summary>For leaves: whether the spec (or every spec of a grouped [Describe]) is pending.</summary>
        public bool IsPending { get; set; }

        /// <summary>Source location for navigation; <c>null</c> when unknown.</summary>
        public SourceLocationReader.SourceLocation? Location { get; set; }

        /// <summary>Returns this node's ancestors from the nearest parent up to the namespace node.</summary>
        public IEnumerable<SpecNode> Ancestors()
        {
            for (var node = Parent; node != null; node = node.Parent)
                yield return node;
        }
    }

    /// <summary>
    /// A discovered specification class with its built suite tree.
    /// </summary>
    internal sealed class SpecClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecClass"/> class.
        /// </summary>
        public SpecClass(Type type, IContextualTestCase root, string assemblyPath)
        {
            Type = type;
            Root = root;
            AssemblyPath = assemblyPath;
        }

        /// <summary>The <see cref="Specifications"/> subclass.</summary>
        public Type Type { get; }

        /// <summary>The suite tree built from one instance of <see cref="Type"/>.</summary>
        public IContextualTestCase Root { get; }

        /// <summary>Path of the assembly that defines <see cref="Type"/>, used for PDB lookups.</summary>
        public string AssemblyPath { get; }
    }

    /// <summary>
    /// The complete spec tree of one assembly.
    /// </summary>
    internal sealed class SpecForest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecForest"/> class.
        /// </summary>
        public SpecForest(string assemblyName, IReadOnlyList<SpecNode> roots, IReadOnlyList<SpecNode> leaves, IReadOnlyList<string> warnings)
        {
            AssemblyName = assemblyName;
            Roots = roots;
            Leaves = leaves;
            Warnings = warnings;
        }

        /// <summary>The assembly name, which is the first path segment.</summary>
        public string AssemblyName { get; }

        /// <summary>The namespace nodes.</summary>
        public IReadOnlyList<SpecNode> Roots { get; }

        /// <summary>Every runnable leaf in tree order.</summary>
        public IReadOnlyList<SpecNode> Leaves { get; }

        /// <summary>Problems found while discovering (e.g. classes without [Describe] methods).</summary>
        public IReadOnlyList<string> Warnings { get; }
    }
}
