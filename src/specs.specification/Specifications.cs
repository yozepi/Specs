using specs.core.Framework.Processors.TestCases;
using specs.specification.DSL;

namespace specs.specification
{
    /// <summary>
    /// Base class for test specifications providing access to the test case builder.
    /// Inherit from this class to create test specifications using the fluent API.
    /// </summary>
    public abstract class Specifications
    {
        private readonly IContextualTestCaseBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="Specifications"/> class.
        /// </summary>
        protected Specifications()
        {
            _builder = new ContextualTestCaseBuilder();
        }

        /// <summary>
        /// Gets the builder instance for constructing test cases.
        /// Use this property to access the fluent API for building test hierarchies.
        /// </summary>
        private protected IContextualTestCaseBuilder Builder => _builder;

        /// <summary>
        /// Defines "what" is being tested using behavior-driven design (BDD) style.
        /// Use Describe to specify the subject under test, such as a class name, feature, or component.
        /// Supports hierarchical nesting with Context for organizing test scenarios.
        /// </summary>
        /// <returns>
        /// A builder instance enabling fluent indexer syntax for defining test contexts.
        /// </returns>
        /// <example>
        /// <code>
        /// Describe["Calculator"] = () =>
        /// {
        ///     Context["when adding numbers"] = () =>
        ///     {
        ///         // Test cases for addition
        ///     };
        /// };
        /// </code>
        /// </example>
        public ContextBuilder Describe => new ContextBuilder(_builder);

        /// <summary>
        /// Defines "when" or "given" scenarios in behavior-driven design (BDD) style.
        /// Use Context to specify preconditions, scenarios, or situational contexts for tests.
        /// Supports hierarchical nesting with Describe for organizing test specifications.
        /// </summary>
        /// <returns>
        /// A builder instance enabling fluent indexer syntax for defining test contexts.
        /// </returns>
        /// <example>
        /// <code>
        /// Context["when user is authenticated"] = () =>
        /// {
        ///     Describe["UserProfile"] = () =>
        ///     {
        ///         // Test cases for authenticated user profile
        ///     };
        /// };
        /// </code>
        /// </example>
        public ContextBuilder Context => new ContextBuilder(_builder);

        /// <summary>
        /// Defines "then" test assertions in behavior-driven design (BDD) style.
        /// Use It to specify individual test assertions within a context or describe block.
        /// This property is protected and only accessible from derived test specification classes.
        /// </summary>
        /// <returns>
        /// A builder instance enabling fluent indexer syntax for defining test assertions.
        /// </returns>
        /// <example>
        /// <code>
        /// Describe["Calculator"] = () =>
        /// {
        ///     It["should add two numbers correctly"] = () =>
        ///     {
        ///         // Test assertion
        ///     };
        /// };
        /// </code>
        /// </example>
        protected AssertBuilder It => new AssertBuilder(_builder);

        /// <summary>
        /// Defines "then" asynchronous test assertions in behavior-driven design (BDD) style.
        /// Use ItAsync to specify individual async test assertions within a context or describe block.
        /// This property is protected and only accessible from derived test specification classes.
        /// </summary>
        /// <returns>
        /// A builder instance enabling fluent indexer syntax for defining async test assertions.
        /// </returns>
        /// <example>
        /// <code>
        /// Describe["DataRepository"] = () =>
        /// {
        ///     ItAsync["should save data asynchronously"] = async () =>
        ///     {
        ///         // Async test assertion
        ///         await repository.SaveAsync(data);
        ///     };
        /// };
        /// </code>
        /// </example>
        protected AssertAsyncBuilder ItAsync => new AssertAsyncBuilder(_builder);
    }
}
