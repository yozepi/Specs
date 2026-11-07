using System;
using System.Collections.Generic;
using System.Reflection;
using specs.BDD.Attributes;
using specs.BDD.Runner;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.TestingPlatform.Discovery
{
    /// <summary>
    /// Builds the full spec tree (namespace → class → [Feature] → contexts → specs) of an assembly.
    /// </summary>
    internal static class SpecTreeBuilder
    {
        /// <summary>The namespace segment used for specification classes declared outside any namespace.</summary>
        public const string GlobalNamespace = "global";

        /// <summary>
        /// Discovers every specification class in <paramref name="assembly"/> and builds its tree.
        /// </summary>
        /// <param name="assembly">The test assembly.</param>
        /// <param name="configuredStructure">
        /// The runner-level <c>ReportingLevel</c> default, or <c>null</c> when not configured.
        /// </param>
        public static SpecForest Build(Assembly assembly, ReportingLevel? configuredStructure)
        {
            var assemblyName = assembly.GetName().Name ?? string.Empty;
            var assemblyPath = assembly.Location;
            var rootPath = SpecPath.Separator + SpecPath.Escape(assemblyName);

            var roots = new List<SpecNode>();
            var namespaces = new Dictionary<string, SpecNode>(StringComparer.Ordinal);
            var leaves = new List<SpecNode>();
            var warnings = new List<string>();

            foreach (var type in SpecificationClassScanner.FindSpecificationClasses(assembly))
            {
                IContextualTestCase root;
                try
                {
                    ContextMethodScanner.ValidateHasContextMethods(type);
                    root = SuiteTreeBuilder.BuildSuite(type);
                }
                catch (InvalidSpecificationException ex)
                {
                    warnings.Add(ex.Message);
                    continue;
                }
                catch (Exception ex)
                {
                    warnings.Add($"Error processing '{type.FullName}': {SpecExceptions.Unwrap(ex).Message}");
                    continue;
                }

                var namespaceName = type.Namespace ?? GlobalNamespace;
                if (!namespaces.TryGetValue(namespaceName, out var namespaceNode))
                {
                    namespaceNode = new SpecNode(SpecNodeKind.Namespace, namespaceName,
                        SpecPath.Combine(rootPath, namespaceName), parent: null, specClass: null);
                    namespaces[namespaceName] = namespaceNode;
                    roots.Add(namespaceNode);
                }

                var specClass = new SpecClass(type, root, assemblyPath);
                var className = NestedName(type);
                var classNode = new SpecNode(SpecNodeKind.Class, className,
                    SpecPath.Combine(namespaceNode.Path, className), namespaceNode, specClass);

                AddDescribes(classNode, specClass, configuredStructure, leaves);
            }

            return new SpecForest(assemblyName, roots, leaves, warnings);
        }

        // ── Private helpers ────────────────────────────────────────────────────────────────────

        private static void AddDescribes(
            SpecNode classNode,
            SpecClass specClass,
            ReportingLevel? configuredStructure,
            List<SpecNode> leaves)
        {
            var methodByContextName = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);
            foreach (var method in ContextMethodScanner.FindContextMethods(specClass.Type))
            {
                var attr = method.GetCustomAttribute<FeatureAttribute>();
                var name = !string.IsNullOrWhiteSpace(attr?.Description) ? attr!.Description! : method.Name;
                methodByContextName[name] = method;
            }

            // The suite root is the class level; its children are the [Feature] contexts.
            foreach (var describe in specClass.Root.ChildContexts)
            {
                methodByContextName.TryGetValue(describe.Description, out var method);
                var methodLocation = method != null
                    ? SourceLocationReader.TryGetLocation(specClass.AssemblyPath, method)
                    : null;
                var path = SpecPath.Combine(classNode.Path, describe.Description);

                if (method != null && GroupingSettings.Resolve(method, configuredStructure))
                {
                    var grouped = new SpecNode(SpecNodeKind.GroupedDescribe, describe.Description, path, classNode, specClass)
                    {
                        ContextPath = describe.Description,
                        IsPending = PendingSpecs.HasAnyTests(describe) && !PendingSpecs.HasRunnableTests(describe),
                        Location = methodLocation,
                        Method = method
                    };
                    leaves.Add(grouped);
                    continue;
                }

                var describeNode = new SpecNode(SpecNodeKind.Describe, describe.Description, path, classNode, specClass)
                {
                    Location = methodLocation,
                    Method = method
                };
                AddContextChildren(describeNode, describe, describe.Description, specClass, methodLocation, leaves);
            }
        }

        private static void AddContextChildren(
            SpecNode node,
            ITestCaseContext context,
            string contextPath,
            SpecClass specClass,
            SourceLocationReader.SourceLocation? fallbackLocation,
            List<SpecNode> leaves)
        {
            foreach (var (description, action) in context.Tests)
            {
                var leaf = new SpecNode(SpecNodeKind.Spec, description, SpecPath.Combine(node.Path, description), node, specClass)
                {
                    ContextPath = contextPath,
                    TestDescription = description,
                    IsPending = PendingSpecs.IsPending(context, description),
                    // Prefer the It[...] lambda so navigation lands on the spec, not the [Feature] method.
                    Location = SourceLocationReader.TryGetTestLocation(specClass.AssemblyPath, context, description, action)
                        ?? fallbackLocation
                };
                leaves.Add(leaf);
            }

            if (context is IContextualTestCase contextual)
            {
                foreach (var child in contextual.ChildContexts)
                {
                    var childNode = new SpecNode(SpecNodeKind.Context, child.Description,
                        SpecPath.Combine(node.Path, child.Description), node, specClass);
                    AddContextChildren(childNode, child, contextPath + ContextPathFormat.Separator + child.Description,
                        specClass, fallbackLocation, leaves);
                }
            }
        }

        /// <summary>Returns the class name, prefixed by its declaring classes joined with '+'.</summary>
        private static string NestedName(Type type) =>
            type.DeclaringType == null ? type.Name : NestedName(type.DeclaringType) + "+" + type.Name;
    }
}
