using System;
using System.Reflection;
using specs.BDD;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.Runner
{
    /// <summary>
    /// Builds a complete <see cref="IContextualTestCase"/> suite tree from a
    /// <see cref="Specifications"/> subclass.
    /// One instance of the class is created, all <c>[Feature]</c> methods are registered as
    /// named child contexts via <see cref="Specifications.Context"/>, and the resulting
    /// <see cref="IContextualTestCase"/> represents the whole class-level hierarchy.
    /// </summary>
    /// <remarks>
    /// This is the single source of truth for tree construction — used by every runner for both
    /// discovery and execution.
    /// </remarks>
    internal static class SuiteTreeBuilder
    {
        /// <summary>
        /// Instantiates <paramref name="specClass"/>, registers all <c>[Feature]</c> methods as
        /// named child contexts, and builds the immutable test-case hierarchy.
        /// </summary>
        /// <param name="specClass">A class derived from <see cref="Specifications"/>.</param>
        /// <returns>The root <see cref="IContextualTestCase"/> for the entire class.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="specClass"/> cannot be instantiated.
        /// </exception>
        public static IContextualTestCase BuildSuite(Type specClass)
        {
            var instance = Activator.CreateInstance(specClass) as Specifications
                ?? throw new InvalidOperationException(
                    $"Cannot instantiate specification class '{specClass.FullName}'.");

            foreach (var contextMethod in ContextMethodScanner.FindContextMethods(specClass))
            {
                var attr = contextMethod.GetCustomAttribute<FeatureAttribute>();
                var name = !string.IsNullOrWhiteSpace(attr?.Description)
                    ? attr!.Description!
                    : contextMethod.Name;

                // Capture the loop variable to avoid closure-capture bug.
                var capturedMethod = contextMethod;
                instance.Context[name] = () => capturedMethod.Invoke(instance, null);
            }

            return instance.CoreBuilder.Build();
        }
    }
}
