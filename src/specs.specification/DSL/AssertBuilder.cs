using System;
using specs.core.Framework.Processors.TestCases;

namespace specs.specification.DSL
{
    /// <summary>
    /// Provides indexer syntax for defining test assertions using the It pattern.
    /// This class wraps <see cref="IContextualTestCaseBuilder"/> to enable fluent DSL syntax.
    /// </summary>
    /// <example>
    /// <code>
    /// It["should calculate sum correctly"] = () =>
    /// {
    ///     // Test assertion
    /// };
    /// </code>
    /// </example>
    public class AssertBuilder
    {
        private readonly IContextualTestCaseBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertBuilder"/> class.
        /// </summary>
        /// <param name="builder">The underlying test case builder to delegate to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
        internal AssertBuilder(IContextualTestCaseBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Sets the test assertion definition using indexer syntax.
        /// The setter accepts an <see cref="Action"/> delegate that defines the test assertion.
        /// </summary>
        /// <param name="description">The description of the test assertion. Cannot be null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="description"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="description"/> is empty or whitespace.</exception>
        public Action this[string description]
        {
            set
            {
                if (description == null)
                {
                    throw new ArgumentNullException(nameof(description), "Description cannot be null.");
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    throw new ArgumentException("Description cannot be empty or whitespace.", nameof(description));
                }

                _builder.AddTest(description, value);
            }
        }
    }
}
