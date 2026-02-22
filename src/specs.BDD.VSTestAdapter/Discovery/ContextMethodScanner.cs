using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using specs.BDD.Attributes;

namespace specs.BDD.VSTestAdapter.Discovery
{
    /// <summary>
    /// Scans a specification class for methods decorated with <see cref="ContextAttribute"/>.
    /// </summary>
    public static class ContextMethodScanner
    {
        /// <summary>
        /// Returns all methods on the given type that are decorated with <see cref="ContextAttribute"/>.
        /// Searches public, non-public, and instance methods.
        /// </summary>
        /// <param name="specificationClass">The specification class to scan.</param>
        /// <returns>An enumerable of matching methods.</returns>
        public static IEnumerable<MethodInfo> FindContextMethods(Type specificationClass)
        {
            return specificationClass
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<ContextAttribute>() != null);
        }

        /// <summary>
        /// Validates that the given specification class has at least one <see cref="ContextAttribute"/>-decorated
        /// method. Throws <see cref="InvalidSpecificationException"/> if none are found.
        /// </summary>
        /// <param name="specificationClass">The class to validate.</param>
        /// <exception cref="InvalidSpecificationException">
        /// Thrown when no [Context] methods are found on the class.
        /// </exception>
        public static void ValidateHasContextMethods(Type specificationClass)
        {
            if (!FindContextMethods(specificationClass).Any())
            {
                throw new InvalidSpecificationException(
                    $"Specification class '{specificationClass.FullName}' has no methods decorated with " +
                    $"[Context] attribute. All Specification classes must have at least one [Context] method.");
            }
        }
    }
}
