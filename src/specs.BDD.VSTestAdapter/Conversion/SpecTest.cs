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
        /// The [Context]-decorated method that builds this test's context scope.
        /// <c>null</c> for tests registered directly at the class root (outside any [Context] method).
        /// Used as a fallback for source-location lookup in Test Explorer via PDB.
        /// </summary>
        public System.Reflection.MethodInfo? ContextMethod { get; set; }

        /// <summary>
        /// The action delegate registered via <c>It["description"] = () => ...</c> for this test.
        /// Its <see cref="Delegate.Method"/> is the compiler-generated lambda method, whose PDB
        /// sequence points point to the <c>It[...]</c> definition line — enabling precise navigation
        /// in Test Explorer. <c>null</c> when the action could not be captured during discovery.
        /// </summary>
        public Action? ItAction { get; set; }

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
