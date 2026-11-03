using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace specs.BDD.Runner
{
    /// <summary>
    /// Scans an assembly for concrete classes that inherit from <see cref="specs.BDD.Specifications"/>.
    /// </summary>
    internal static class SpecificationClassScanner
    {
        /// <summary>
        /// Returns all non-abstract classes in the given assembly that inherit from
        /// <see cref="specs.BDD.Specifications"/>.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>An enumerable of matching specification types.</returns>
        public static IEnumerable<Type> FindSpecificationClasses(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Where(t => typeof(specs.BDD.Specifications).IsAssignableFrom(t));
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Some types may fail to load (e.g., missing dependencies).
                // Return the ones that did load and qualify.
                return ex.Types
                    .Where(t => t != null && t!.IsClass && !t!.IsAbstract)
                    .Where(t => typeof(specs.BDD.Specifications).IsAssignableFrom(t!))
                    .Select(t => t!);
            }
        }
    }
}
