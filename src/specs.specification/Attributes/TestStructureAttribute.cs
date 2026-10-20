using System;

namespace specs.BDD.Attributes
{
    /// <summary>
    /// Sets the <see cref="TestStructure"/> for every [Describe] method in the test assembly
    /// or specification class.
    /// </summary>
    /// <remarks>
    /// Precedence: an explicit <see cref="DescribeAttribute.Structure"/> value on a [Describe]
    /// method overrides the class-level attribute; the class-level attribute overrides the
    /// assembly-level attribute; the assembly-level attribute overrides the
    /// <c>&lt;SpecsBDD&gt;&lt;TestStructure&gt;</c> .runsettings value.
    /// The class-level attribute is not inherited by derived specification classes.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TestStructureAttribute : Attribute
    {
        /// <summary>
        /// How specs map to test cases in the test runner.
        /// </summary>
        public TestStructure Structure { get; }

        /// <summary>
        /// Initializes the attribute.
        /// </summary>
        /// <param name="structure">How specs map to test cases in the test runner.</param>
        public TestStructureAttribute(TestStructure structure)
        {
            Structure = structure;
        }
    }
}
