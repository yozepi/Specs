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
    /// instantiating it, invoking a [Context] method, and recursively traversing the resulting
    /// test hierarchy.
    /// </summary>
    public static class TestStructureExtractor
    {
        /// <summary>
        /// Reflection handle to the private <c>coreBuilder</c> field on <see cref="specs.BDD.Specifications"/>.
        /// Accessed after the [Context] method has been invoked to retrieve the fully-built test structure.
        /// </summary>
        private static readonly FieldInfo? CoreBuilderField =
            typeof(specs.BDD.Specifications).GetField(
                "coreBuilder",
                BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Instantiates <paramref name="specClass"/>, invokes <paramref name="contextMethod"/> to build
        /// the test structure, and flattens all <see cref="SpecTest"/> items from the resulting hierarchy.
        /// A fresh instance is created for each invocation so context methods do not interfere with each other.
        /// </summary>
        /// <param name="specClass">The specification class to instantiate.</param>
        /// <param name="contextMethod">The [Context]-decorated method to invoke.</param>
        /// <returns>A flat enumerable of all discovered tests.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the internal builder field cannot be located or is null after invocation.
        /// </exception>
        public static IEnumerable<SpecTest> ExtractTests(Type specClass, MethodInfo contextMethod)
        {
            if (CoreBuilderField == null)
                throw new InvalidOperationException(
                    "Cannot locate internal 'coreBuilder' field on Specifications. " +
                    "The Specifications class implementation may have changed.");

            // 1. Create a fresh instance so this context method builds in isolation.
            var instance = Activator.CreateInstance(specClass) as specs.BDD.Specifications
                ?? throw new InvalidOperationException(
                    $"Cannot instantiate specification class '{specClass.FullName}'.");

            // 2. Invoke the [Context] method — this runs the user's fluent setup code and
            //    populates the internal builder with the full test hierarchy.
            contextMethod.Invoke(instance, null);

            // 3. Access the private builder field (IContextualTestCaseBuilder) via reflection.
            var builder = CoreBuilderField.GetValue(instance) as IContextualTestCaseBuilder
                ?? throw new InvalidOperationException(
                    "Internal 'coreBuilder' field is null after context method invocation.");

            // 4. Build the immutable test case tree.
            var testCase = builder.Build();

            // 5. Determine the root display name from the [Context] attribute description, or the method name.
            var contextAttr = contextMethod.GetCustomAttribute<ContextAttribute>();
            var rootContextName = !string.IsNullOrWhiteSpace(contextAttr?.Description)
                ? contextAttr!.Description!
                : contextMethod.Name;

            // 6. Flatten and return all tests.
            return FlattenTests(testCase, specClass, contextMethod, rootContextName);
        }

        // ── Private helpers ────────────────────────────────────────────────────────────────────

        private static IEnumerable<SpecTest> FlattenTests(
            IContextualTestCase testCase,
            Type specClass,
            MethodInfo contextMethod,
            string contextPath)
        {
            // Tests defined directly inside the [Context] method (not nested in a sub-context).
            foreach (var (description, _) in testCase.Tests)
            {
                yield return new SpecTest
                {
                    SpecificationClass = specClass,
                    ContextMethod = contextMethod,
                    ContextPath = contextPath,
                    TestDescription = description
                };
            }

            // Recurse into each child context (Context["..."] or Describe["..."] blocks).
            foreach (var child in testCase.ChildContexts)
            {
                var childPath = $"{contextPath} \u2192 {child.Description}";
                foreach (var test in FlattenChildContext(child, specClass, contextMethod, childPath))
                    yield return test;
            }
        }

        private static IEnumerable<SpecTest> FlattenChildContext(
            ITestCaseContext child,
            Type specClass,
            MethodInfo contextMethod,
            string contextPath)
        {
            // Tests defined directly at this context level.
            foreach (var (description, _) in child.Tests)
            {
                yield return new SpecTest
                {
                    SpecificationClass = specClass,
                    ContextMethod = contextMethod,
                    ContextPath = contextPath,
                    TestDescription = description
                };
            }

            // ITestCaseContext doesn't expose ChildContexts in its interface, but the concrete
            // ContextualTestCase class implements IContextualTestCase which does.
            // Cast and recurse when possible.
            if (child is IContextualTestCase contextualChild)
            {
                foreach (var grandChild in contextualChild.ChildContexts)
                {
                    var grandChildPath = $"{contextPath} \u2192 {grandChild.Description}";
                    foreach (var test in FlattenChildContext(grandChild, specClass, contextMethod, grandChildPath))
                        yield return test;
                }
            }
        }
    }
}
