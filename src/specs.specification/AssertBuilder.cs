using System;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD
{
    /// <summary>
    /// Provides indexer syntax for defining test assertions using the It pattern.
    /// This class wraps <see cref="IContextualTestCaseBuilder"/> to enable fluent BDD syntax.
    /// </summary>
    /// <example>
    /// <code>
    /// It["should calculate sum correctly"] = () =>
    /// {
    ///     // Test assertion
    /// };
    /// </code>
    /// </example>
    public class AssertBuilder : IAssertProvider<Action>
    {
        private readonly IContextualTestCaseBuilder _builder;
        private readonly bool _isPending;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertBuilder"/> class.
        /// </summary>
        /// <param name="builder">The underlying test case builder to delegate to.</param>
        /// <param name="isPending">When <c>true</c>, every test defined through this builder is pending.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
        internal AssertBuilder(IContextualTestCaseBuilder builder, bool isPending = false)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _isPending = isPending;
        }

        /// <summary>
        /// Sets the test assertion definition using indexer syntax.
        /// The setter accepts an <see cref="Action"/> delegate that defines the test assertion.
        /// Assigning <see cref="Specifications.TODO"/> marks the test as pending.
        /// </summary>
        /// <param name="description">The description of the test assertion. Cannot be null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="description"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="description"/> is empty or whitespace.</exception>
        public Action this[string description]
        {
            set
            {
                if (_isPending || ReferenceEquals(value, Specifications.TODO))
                    _builder.AddPendingTest(description, value);
                else
                    _builder.AddTest(description, value);
            }
        }
    }
}
