using System;

namespace specs.BDD.Attributes
{
    /// <summary>
    /// Marks a method as a specification context builder.
    /// This attribute enables methods to define hierarchical test contexts with optional descriptions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ContextAttribute can be applied to methods to designate them as context builders
    /// within the Specs testing framework. Only one context attribute can be applied to a method.
    /// </para>
    /// <example>
    /// <code>
    /// [Context("User authentication")]
    /// public void describe_login()
    /// {
    ///     // Test context setup
    /// }
    /// 
    /// [Context("Valid credentials")]
    /// public void when_user_attempts_login()
    /// {
    ///     // Test context with description
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ContextAttribute : Attribute
    {
        // Tri-state backing: distinguishes "not specified" from an explicit true/false.
        private bool? _grouped;

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
        /// When <c>true</c>, all tests under this context run together as a single test result
        /// with the full contextual output published to the result's output.
        /// When set (either way), overrides the assembly-level
        /// <see cref="GroupSpecsByRootContextAttribute"/> and any .runsettings value.
        /// </summary>
        public bool Grouped
        {
            get => _grouped ?? false;
            set => _grouped = value;
        }

        /// <summary>
        /// The explicit <see cref="Grouped"/> setting, or <c>null</c> when not specified
        /// on the attribute (meaning lower-priority configuration applies).
        /// </summary>
        public bool? GroupedSetting => _grouped;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextAttribute"/> class
        /// with an optional description.
        /// </summary>
        /// <param name="description">
        /// Optional description for the context. If not provided, defaults to null.
        /// </param>
        public ContextAttribute(string? description = null)
        {
            Description = description;
        }
    }
}
