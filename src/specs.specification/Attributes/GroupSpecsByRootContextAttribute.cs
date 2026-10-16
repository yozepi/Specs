using System;

namespace specs.BDD.Attributes
{
    /// <summary>
    /// Assembly-level switch that enables grouped execution for every root [Context] method
    /// in the test assembly: each context runs as a single test result with the full
    /// contextual output published to the result's output.
    /// </summary>
    /// <remarks>
    /// Precedence: an explicit <see cref="ContextAttribute.Grouped"/> value on a [Context]
    /// method overrides this attribute; this attribute overrides the
    /// <c>&lt;SpecsBDD&gt;&lt;GroupByRootContext&gt;</c> .runsettings value.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class GroupSpecsByRootContextAttribute : Attribute
    {
        /// <summary>
        /// Whether grouped execution is enabled for the assembly.
        /// </summary>
        public bool Enabled { get; }

        /// <summary>
        /// Initializes the attribute.
        /// </summary>
        /// <param name="enabled">Whether grouped execution is enabled. Defaults to <c>true</c>.</param>
        public GroupSpecsByRootContextAttribute(bool enabled = true)
        {
            Enabled = enabled;
        }
    }
}
