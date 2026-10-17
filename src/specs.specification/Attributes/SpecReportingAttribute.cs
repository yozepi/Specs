using System;

namespace specs.BDD.Attributes
{
    /// <summary>
    /// Assembly- or class-level setting for how the results of every root [Context] method
    /// in the test assembly or specification class are reported.
    /// </summary>
    /// <remarks>
    /// Precedence: an explicit <see cref="ContextAttribute.Reporting"/> value on a [Context]
    /// method overrides the class-level attribute; the class-level attribute overrides the
    /// assembly-level attribute; the assembly-level attribute overrides the
    /// <c>&lt;SpecsBDD&gt;&lt;SpecReporting&gt;</c> .runsettings value.
    /// The class-level attribute is not inherited by derived specification classes.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SpecReportingAttribute : Attribute
    {
        /// <summary>
        /// How spec results are reported.
        /// </summary>
        public SpecReporting Reporting { get; }

        /// <summary>
        /// Initializes the attribute.
        /// </summary>
        /// <param name="reporting">How spec results are reported.</param>
        public SpecReportingAttribute(SpecReporting reporting)
        {
            Reporting = reporting;
        }
    }
}
