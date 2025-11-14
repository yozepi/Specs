using System;
using System.Threading.Tasks;

namespace specs.core.Framework.Processors.TestCases
{
    /// <summary>
    /// Provides a fluent API for building hierarchical test cases with lifecycle actions.
    /// Supports both synchronous (Action) and asynchronous (Func&lt;Task&gt;) operations.
    /// </summary>
    public interface IContextualTestCaseBuilder
    {
        /// <summary>
        /// Gets the description of this test context.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Adds a synchronous setup action that runs once before all tests.
        /// </summary>
        /// <param name="action">The setup action to execute.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddSetupOnce(Action action);

        /// <summary>
        /// Adds an asynchronous setup action that runs once before all tests.
        /// </summary>
        /// <param name="action">The async setup action to execute.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddSetupOnce(Func<Task> action);

        /// <summary>
        /// Adds a synchronous setup action that runs before each test.
        /// </summary>
        /// <param name="action">The setup action to execute.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddSetupEach(Action action);

        /// <summary>
        /// Adds an asynchronous setup action that runs before each test.
        /// </summary>
        /// <param name="action">The async setup action to execute.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddSetupEach(Func<Task> action);

        /// <summary>
        /// Adds a synchronous action under test.
        /// </summary>
        /// <param name="action">The action to test.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddAct(Action action);

        /// <summary>
        /// Adds an asynchronous action under test.
        /// </summary>
        /// <param name="action">The async action to test.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddAct(Func<Task> action);

        /// <summary>
        /// Adds a synchronous test with assertions.
        /// </summary>
        /// <param name="description">A description of what the test verifies.</param>
        /// <param name="test">The test action containing assertions.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddTest(string description, Action test);

        /// <summary>
        /// Adds an asynchronous test with assertions.
        /// </summary>
        /// <param name="description">A description of what the test verifies.</param>
        /// <param name="test">The async test action containing assertions.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddTest(string description, Func<Task> test);

        /// <summary>
        /// Adds a synchronous teardown action that runs after each test.
        /// </summary>
        /// <param name="action">The teardown action to execute.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddTeardownEach(Action action);

        /// <summary>
        /// Adds an asynchronous teardown action that runs after each test.
        /// </summary>
        /// <param name="action">The async teardown action to execute.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddTeardownEach(Func<Task> action);

        /// <summary>
        /// Adds a synchronous teardown action that runs once after all tests.
        /// </summary>
        /// <param name="action">The teardown action to execute.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddTeardownOnce(Action action);

        /// <summary>
        /// Adds an asynchronous teardown action that runs once after all tests.
        /// </summary>
        /// <param name="action">The async teardown action to execute.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder AddTeardownOnce(Func<Task> action);

        /// <summary>
        /// Creates a child test context that inherits lifecycle actions from this parent.
        /// </summary>
        /// <param name="description">A description of the child context.</param>
        /// <param name="configure">An action to configure the child builder.</param>
        /// <returns>The builder instance for method chaining.</returns>
        IContextualTestCaseBuilder ChildTestCase(string description, Action<IContextualTestCaseBuilder> configure);

        /// <summary>
        /// Builds the immutable test case from the configured actions.
        /// Once built, the test case cannot be modified.
        /// </summary>
        /// <returns>The constructed test case ready for execution.</returns>
        IContextualTestCase Build();
    }
}
