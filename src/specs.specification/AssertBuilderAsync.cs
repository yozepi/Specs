using System;
using System.Threading.Tasks;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD
{
    /// <summary>
    /// Provides indexer syntax for defining asynchronous test assertions using the ItAsync pattern.
    /// This class wraps <see cref="IContextualTestCaseBuilder"/> to enable fluent BDD syntax.
    /// </summary>
    /// <example>
    /// <code>
    /// ItAsync["should save data asynchronously"] = async () =>
    /// {
    ///     // Async test assertion
    ///     await repository.SaveAsync(data);
    /// };
    /// </code>
    /// </example>
    public class AssertBuilderAsync : IAssertProvider<Func<Task>>
    {
        private readonly IContextualTestCaseBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertBuilderAsync"/> class.
        /// </summary>
        /// <param name="builder">The underlying test case builder to delegate to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
        internal AssertBuilderAsync(IContextualTestCaseBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Sets the asynchronous test assertion definition using indexer syntax.
        /// The setter accepts a <see cref="Func{Task}"/> delegate that defines the async test assertion.
        /// </summary>
        /// <param name="description">The description of the test assertion. Cannot be null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="description"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="description"/> is empty or whitespace.</exception>
        public Func<Task> this[string description]
        {
            set
            {
                _builder.AddTest(description, value);
            }
        }
    }
}
