using System;
using System.Collections.Generic;
using System.Reflection;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;
using specs.BDD.VSTestAdapter.Conversion;

namespace specs.BDD.VSTestAdapter.Discovery
{
    /// <summary>
    /// Extracts the flat list of <see cref="SpecTest"/> items from a specification class by
    /// building the full suite tree via <see cref="SuiteTreeBuilder"/> and flattening it.
    /// </summary>
    public static class TestStructureExtractor
    {
        public static string PathSeparator { get; set; } = " \u2192 ";

        /// <summary>
        /// Builds the complete suite tree for <paramref name="specClass"/> and flattens all
        /// <see cref="SpecTest"/> items from the resulting hierarchy.
        /// Root contexts configured for grouped execution yield a single <see cref="SpecTest"/>
        /// (with <see cref="SpecTest.IsGrouped"/> set) instead of one item per It[...].
        /// </summary>
        /// <param name="specClass">The specification class to extract tests from.</param>
        /// <param name="runSettingsGrouping">
        /// The .runsettings <c>GroupByRootContext</c> value, or <c>null</c> when not configured.
        /// </param>
        /// <returns>A flat enumerable of all discovered tests.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specification class cannot be instantiated.
        /// </exception>
        public static IEnumerable<SpecTest> ExtractTests(Type specClass, bool? runSettingsGrouping = null)
        {
            // Build name → MethodInfo lookup so each SpecTest knows which [Context] method
            // it belongs to (used for source-location navigation in Test Explorer).
            var methodByContextName = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);
            foreach (var m in ContextMethodScanner.FindContextMethods(specClass))
            {
                var attr = m.GetCustomAttribute<ContextAttribute>();
                var name = !string.IsNullOrWhiteSpace(attr?.Description)
                    ? attr!.Description!
                    : m.Name;
                methodByContextName[name] = m;
            }

            var root = SuiteTreeBuilder.BuildSuite(specClass);

            // The root node represents the class level. Its immediate children are the
            // [Context] method contexts, so each child's description IS the root path.
            foreach (var child in root.ChildContexts)
            {
                methodByContextName.TryGetValue(child.Description, out var ctxMethod);

                if (ctxMethod != null && GroupingSettings.Resolve(ctxMethod, runSettingsGrouping))
                {
                    yield return new SpecTest
                    {
                        SpecificationClass = specClass,
                        ContextMethod = ctxMethod,
                        ContextPath = child.Description,
                        TestDescription = string.Empty,
                        IsGrouped = true
                    };
                    continue;
                }

                foreach (var test in FlattenChildContext(child, specClass, child.Description, ctxMethod))
                    yield return test;
            }
        }

        // ── Private helpers ────────────────────────────────────────────────────────────────────

        private static IEnumerable<SpecTest> FlattenTests(
            IContextualTestCase testCase,
            Type specClass,
            string rootPath,
            Dictionary<string, MethodInfo> methodByContextName)
        {
            // Tests defined directly at the class root (before any [Context] call).
            foreach (var (description, action) in testCase.Tests)
            {
                yield return new SpecTest
                {
                    SpecificationClass = specClass,
                    ContextPath = rootPath,
                    TestDescription = description,
                    ContextMethod = null,
                    ItAction = action
                };
            }

            // Recurse into each child context ([Context] method or nested Describe block).
            foreach (var child in testCase.ChildContexts)
            {
                var childPath = $"{rootPath}{PathSeparator}{child.Description}";
                methodByContextName.TryGetValue(child.Description, out var ctxMethod);
                foreach (var test in FlattenChildContext(child, specClass, childPath, ctxMethod))
                    yield return test;
            }
        }

        private static IEnumerable<SpecTest> FlattenChildContext(
            ITestCaseContext child,
            Type specClass,
            string contextPath,
            MethodInfo? contextMethod)
        {
            // Tests defined directly at this context level.
            foreach (var (description, action) in child.Tests)
            {
                yield return new SpecTest
                {
                    SpecificationClass = specClass,
                    ContextPath = contextPath,
                    TestDescription = description,
                    ContextMethod = contextMethod,
                    ItAction = action
                };
            }

            // ITestCaseContext doesn't expose ChildContexts in its interface, but the concrete
            // ContextualTestCase class implements IContextualTestCase which does.
            // Cast and recurse when possible.
            if (child is IContextualTestCase contextualChild)
            {
                foreach (var grandChild in contextualChild.ChildContexts)
                {
                    //var grandChildPath = $"{contextPath}.{grandChild.Description.Replace(".", "")}";
                    var grandChildPath = $"{contextPath}{PathSeparator}{grandChild.Description}";
                    foreach (var test in FlattenChildContext(grandChild, specClass, grandChildPath, contextMethod))
                        yield return test;
                }
            }
        }
    }
}
