using System;
using specs.core.Framework.Processors.TestCases;

namespace specs.specification.DSL
{
    /// <summary>
    /// Provides indexer syntax for defining top-level test contexts using the Describe pattern.
    /// This class wraps <see cref="IContextualTestCaseBuilder"/> to enable fluent DSL syntax.
    /// </summary>
    /// <example>
    /// <code>
    /// Describe["Calculator"] = () =>
    /// {
    ///     // Test context definition
    /// };
    /// </code>
    /// </example>
public class ContextBuilder
    {
        private readonly IContextualTestCaseBuilder _builder;

        /// <summary>
/// Initializes a new instance of the <see cref="ContextBuilder"/> class.
        /// </summary>
        /// <param name="builder">The underlying test case builder to delegate to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
internal ContextBuilder(IContextualTestCaseBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Gets or sets the test context definition using indexer syntax.
        /// The setter accepts an <see cref="Action"/> delegate that defines the test context.
        /// </summary>
        /// <param name="description">The description of the test context. Cannot be null or empty.</param>
        /// <returns>This property always throws <see cref="NotSupportedException"/> on get.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="description"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="description"/> is empty or whitespace.</exception>
        /// <exception cref="NotSupportedException">Thrown when attempting to get the value.</exception>
        public Action this[string description]
        {
            set
            {
                 _builder.ChildTestCase(description, childBuilder =>
                {
                    value?.Invoke();
                });
            }
        }
    }
}
