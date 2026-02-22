using System;
using System.Reflection;

namespace specs.BDD.VSTestAdapter.Conversion
{
    /// <summary>
    /// Represents a single discovered test extracted from a <see cref="specs.BDD.Specifications"/> class.
    /// Carries the metadata needed to identify, display, and later execute the test.
    /// </summary>
    public class SpecTest
    {
        /// <summary>
        /// The concrete Specification class that contains this test.
        /// </summary>
        public Type SpecificationClass { get; set; } = null!;

        /// <summary>
        /// The [Context]-decorated method that, when invoked, builds the test structure containing this test.
        /// </summary>
        public MethodInfo ContextMethod { get; set; } = null!;

        /// <summary>
        /// The display path through the context hierarchy to this test's parent context.
        /// Built from the [Context] method name/description and any nested Context/Describe block names,
        /// separated by " → " (e.g., "describe_addition → when adding positive numbers").
        /// </summary>
        public string ContextPath { get; set; } = string.Empty;

        /// <summary>
        /// The description of the specific It[...] test assertion (e.g., "should return correct sum").
        /// </summary>
        public string TestDescription { get; set; } = string.Empty;
    }
}
