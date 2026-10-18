using System;

namespace specs.BDD.Attributes
{
    /// <summary>
    /// Marks a method as a specification context builder.
    /// This attribute enables methods to define hierarchical test contexts with optional descriptions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The DescribeAttribute can be applied to methods to designate them as context builders
    /// within the Specs testing framework. Only one context attribute can be applied to a method.
    /// </para>
    /// <example>
    /// <code>
    /// [Describe("User authentication")]
    /// public void describe_login()
    /// {
    ///     // Test context setup
    /// }
    /// 
    /// [Describe("Valid credentials")]
    /// public void when_user_attempts_login()
    /// {
    ///     // Test context with description
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DescribeAttribute : Attribute
    {
        // Nullable backing distinguishes "not specified" from an explicit value.
        private SpecReporting? _reporting;

        /// <summary>
        /// Gets the optional description for this context.
        /// </summary>
        /// <remarks>
        /// The description provides additional information about the context
        /// and can be used for enhanced test reporting and documentation.
        /// A null value indicates no specific description was provided.
        /// </remarks>
        public string? Description { get; }

        /// <summary>
        /// How the results of this context are reported.
        /// When set, overrides the class- and assembly-level
        /// <see cref="SpecReportingAttribute"/> and any .runsettings value.
        /// </summary>
        public SpecReporting Reporting
        {
            get => _reporting ?? SpecReporting.Individual;
            set => _reporting = value;
        }

        /// <summary>
        /// The explicit <see cref="Reporting"/> setting, or <c>null</c> when not specified
        /// on the attribute (meaning lower-priority configuration applies).
        /// </summary>
        public SpecReporting? ReportingSetting => _reporting;

        /// <summary>
        /// Initializes a new instance of the <see cref="DescribeAttribute"/> class
        /// with an optional description.
        /// </summary>
        /// <param name="description">
        /// Optional description for the context. If not provided, defaults to null.
        /// </param>
        public DescribeAttribute(string? description = null)
        {
            Description = description;
        }
    }
}
