using System;

namespace specs.BDD.Attributes
{
    /// <summary>
    /// Sets the <see cref="ReportingLevel"/> for every [Feature] method in the test assembly
    /// or specification class.
    /// </summary>
    /// <remarks>
    /// Precedence: an explicit <see cref="FeatureAttribute.Level"/> value on a [Feature]
    /// method overrides the class-level attribute; the class-level attribute overrides the
    /// assembly-level attribute; the assembly-level attribute overrides the
    /// <c>&lt;SpecsBDD&gt;&lt;ReportingLevel&gt;</c> .runsettings value.
    /// The class-level attribute is not inherited by derived specification classes.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ReportingLevelAttribute : Attribute
    {
        /// <summary>
        /// How specs map to test cases in the test runner.
        /// </summary>
        public ReportingLevel Level { get; }

        /// <summary>
        /// Initializes the attribute.
        /// </summary>
        /// <param name="level">How specs map to test cases in the test runner.</param>
        public ReportingLevelAttribute(ReportingLevel level)
        {
            Level = level;
        }
    }
}
