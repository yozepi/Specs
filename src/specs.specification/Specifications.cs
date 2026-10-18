using specs.core.Framework.ParameterValidation;
using specs.core.Framework.Processors.TestCases;
using System;
using System.Threading.Tasks;

namespace specs.BDD
{
    /// <summary>
    /// Base class for BDD-style test specifications.
    /// Provides a fluent API for defining test contexts, setup, actions, and assertions.
    /// </summary>
    public abstract class Specifications
    {
        private IContextualTestCaseBuilder coreBuilder = new ContextualTestCaseBuilder();

        internal protected IContextualTestCaseBuilder CoreBuilder => coreBuilder;

        /// <summary>
        /// Defines a synchronous setup action that runs once before all tests in this context.
        /// </summary>
        /// <param name="action">The setup action to execute.</param>
        public void BeforeAll(Action action) => CoreBuilder.AddSetupOnce(action);

        /// <summary>
        /// Defines an asynchronous setup action that runs once before all tests in this context.
        /// </summary>
        /// <param name="taskFunction">The async setup action to execute.</param>
        public void BeforeAll(Func<Task> taskFunction) => CoreBuilder.AddSetupOnce(taskFunction);

        /// <summary>
        /// Defines a synchronous setup action that runs before each test.
        /// </summary>
        /// <param name="action">The setup action to execute.</param>
        public void BeforeEach(Action action) => CoreBuilder.AddSetupEach(action);

        /// <summary>
        /// Defines an asynchronous setup action that runs before each test.
        /// </summary>
        /// <param name="taskFunction">The async setup action to execute.</param>
        public void BeforeEach(Func<Task> taskFunction) => CoreBuilder.AddSetupEach(taskFunction);

        /// <summary>
        /// Defines the synchronous action under test.
        /// This action executes after setup and before assertions.
        /// </summary>
        /// <param name="action">The action to test.</param>
        public void Act(Action action) => CoreBuilder.AddAct(action);

        /// <summary>
        /// Defines the asynchronous action under test.
        /// This action executes after setup and before assertions.
        /// </summary>
        /// <param name="taskFunction">The async action to test.</param>
        public void Act(Func<Task> taskFunction) => CoreBuilder.AddAct(taskFunction);

        /// <summary>
        /// Defines a synchronous teardown action that runs after each test.
        /// </summary>
        /// <param name="action">The teardown action to execute.</param>
        public void AfterEach(Action action) => CoreBuilder.AddTeardownEach(action);

        /// <summary>
        /// Defines an asynchronous teardown action that runs after each test.
        /// </summary>
        /// <param name="taskFunction">The async teardown action to execute.</param>
        public void AfterEach(Func<Task> taskFunction) => CoreBuilder.AddTeardownEach(taskFunction);

        /// <summary>
        /// Defines a synchronous teardown action that runs once after all tests in this context.
        /// </summary>
        /// <param name="action">The teardown action to execute.</param>
        public void AfterAll(Action action) => CoreBuilder.AddTeardownOnce(action);

        /// <summary>
        /// Defines an asynchronous teardown action that runs once after all tests in this context.
        /// </summary>
        /// <param name="taskFunction">The async teardown action to execute.</param>
        public void AfterAll(Func<Task> taskFunction) => CoreBuilder.AddTeardownOnce(taskFunction);

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
        public ContextBuilder Describe => new ContextBuilder(CoreBuilder, contextCallBack);


        /// <summary>
        /// Defines "when" or "given" scenarios in behavior-driven design (BDD) style.
        /// Use Context to specify preconditions, scenarios, or situational contexts for tests.
        /// Supports hierarchical nesting with Describe for organizing test specifications.
        /// </summary>
        /// <returns>
        /// A builder instance enabling fluent indexer syntax for defining test contexts.
        /// </returns>
        /// <example>`
        /// <code>
        /// Context["when user is authenticated"] = () =>
        /// {
        ///     Describe["UserProfile"] = () =>
        ///     {
        ///     // Test cases for authenticated user profile
        ///     };
        /// };
        /// </code>
        /// </example>
        public ContextBuilder Context => new ContextBuilder(CoreBuilder, contextCallBack);

        private void contextCallBack(IContextualTestCaseBuilder childCoreBuilder, Action action)
        {
            var previousCoreBuilder = this.coreBuilder;
            this.coreBuilder = childCoreBuilder;
            try
            {
                action.ThrowIfNull(nameof(action)).Invoke();
            }
            finally
            {
                this.coreBuilder = previousCoreBuilder;
            }
        }

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
        ///       // Test assertion
        ///     };
        /// };
        /// </code>
        /// </example>
        public AssertBuilder It => new AssertBuilder(CoreBuilder);

        /// <summary>
        /// Defines asynchronous "then" test assertions in behavior-driven design (BDD) style.
        /// Use ItAsync to specify individual asynchronous test assertions within a context or describe block.
        /// </summary>
        /// <returns>
        /// A builder instance enabling fluent indexer syntax for defining asynchronous test assertions.
        /// </returns>
        /// <example>
        /// <code>
        /// Describe["Calculator"] = () =>
        /// {
        ///     ItAsync["should return data from the remote service"] = async () =>
        ///     {
        ///         // Async test assertion
        ///     };
        /// };
        /// </code>
        /// </example>
        public AssertBuilderAsync ItAsync => new AssertBuilderAsync(CoreBuilder);

        /// <summary>
        /// Placeholder body for a spec that is not yet implemented: <c>It["..."] = TODO;</c>.
        /// The spec is reported as pending and never executed.
        /// </summary>
        public static readonly Action TODO = () => { };

        /// <summary>
        /// Placeholder body for an async spec that is not yet implemented: <c>ItAsync["..."] = TODOAsync;</c>.
        /// The spec is reported as pending and never executed.
        /// </summary>
        public static readonly Func<Task> TODOAsync = () => Task.CompletedTask;

        /// <summary>
        /// Defines a pending (excluded) <see cref="It"/> spec. The body is kept but never executed.
        /// </summary>
        public AssertBuilder xIt => new AssertBuilder(CoreBuilder, isPending: true);

        /// <summary>
        /// Defines a pending (excluded) <see cref="ItAsync"/> spec. The body is kept but never executed.
        /// </summary>
        public AssertBuilderAsync xItAsync => new AssertBuilderAsync(CoreBuilder, isPending: true);

        /// <summary>
        /// Defines a pending <see cref="Context"/>. Every spec inside it, including nested contexts, is pending.
        /// </summary>
        public ContextBuilder xContext => new ContextBuilder(CoreBuilder, contextCallBack, isPending: true);

        /// <summary>
        /// Defines a pending <see cref="Describe"/>. Every spec inside it, including nested contexts, is pending.
        /// </summary>
        public ContextBuilder xDescribe => new ContextBuilder(CoreBuilder, contextCallBack, isPending: true);
    }
}
