using System;
using System.Threading.Tasks;
using Specs.Domain;
using Specs.Domain.Exceptions;

namespace Specs
{
    /// <summary>
    /// Base class for defining BDD-style specifications with fluent syntax.
    /// Provides properties and methods for organizing tests into contexts with setup, action, and assertion phases.
    /// </summary>
    public class Spec
    {

        internal IRunContext RunContext { get; set; } = null!;

        /// <summary>
        /// Gets an indexer for defining synchronous test assertions.
        /// Use this to specify individual test cases with the "It" BDD pattern.
        /// </summary>
        public IndexedRunner<Action> It => new IndexedRunner<Action>((name, action) => this.AddSpec(new ActionSpecDefinition(name, action, dontRun: action == TODO)));
        
        /// <summary>
        /// Gets an indexer for defining asynchronous test assertions.
        /// Use this to specify individual async test cases with the "It" BDD pattern.
        /// </summary>
        public IndexedRunner<Func<Task>> ItAsync => new IndexedRunner<Func<Task>>((name, func) => this.AddSpec(new TaskSpecDefinition(name, func, dontRun: func == TODOAsync)));

        /// <summary>
        /// Gets an indexer for defining test contexts.
        /// Use this to group related tests and specify preconditions or scenarios.
        /// </summary>
        public IndexedRunner<Action> Context => new IndexedRunner<Action>(this.SwitchContext);

        /// <summary>
        /// Gets an indexer for describing the subject under test.
        /// Use this to specify what component or feature is being tested.
        /// </summary>
        public IndexedRunner<Action> Describe => new IndexedRunner<Action>(this.SwitchContext);


        /// <summary>
        /// Sets a synchronous action to run before each test in the current context.
        /// </summary>
        public Action BeforeEach
        {
            set
            {
                this.AddBeforeEach(new ActionRunner(value));
            }
        }

        /// <summary>
        /// Sets an asynchronous action to run before each test in the current context.
        /// </summary>
        public Func<Task> BeforeEachAsync
        {
            set
            {
                this.AddBeforeEach(new TaskRunner(value));
            }
        }


        /// <summary>
        /// Sets the synchronous action under test.
        /// This action executes after setup and before assertions.
        /// </summary>
        public Action Act
        {
            set
            {
                this.AddAct(new ActionRunner(value));
            }
        }

        /// <summary>
        /// Sets the asynchronous action under test.
        /// This action executes after setup and before assertions.
        /// </summary>
        public Func<Task> ActAsync
        {
            set
            {
                this.AddAct(new TaskRunner(value));
            }
        }


        /// <summary>
        /// Sets a synchronous action to run after each test in the current context.
        /// </summary>
        public Action AfterEach
        {
            set
            {
                this.AddAfterEach(new ActionRunner(value));
            }
        }

        /// <summary>
        /// Sets an asynchronous action to run after each test in the current context.
        /// </summary>
        public Func<Task> AfterEachAsync
        {
            set
            {
                this.AddAfterEach(new TaskRunner(value));
            }
        }


        private void SwitchContext(string name, Action action)
        {
            var originalContext = this.RunContext;
            var childContext = this.RunContext.CreateChildContext(name);
            this.RunContext = childContext;
            action.Invoke();
            this.RunContext = originalContext;
            this.RunContext.ContextRestored(childContext);
        }

        private void AddSpec(ISpecDefinition spec)
        {
            this.RunContext.AddSpec(spec);
        }

        private void AddBeforeEach(Runner runner)
        {
            this.RunContext.AddBeforeEach(runner);
        }

        private void AddAct(Runner runner)
        {
            this.RunContext.AddAct(runner);
        }

        private void AddAfterEach(Runner runner)
        {
            this.RunContext.AddAfterEach(runner);
        }

        /// <summary>
        /// Gets a placeholder action for tests that are not yet implemented.
        /// Tests using TODO will be marked to skip execution.
        /// </summary>
        public Action TODO => () => { };

        /// <summary>
        /// Gets a placeholder async action for tests that are not yet implemented.
        /// Tests using TODOAsync will be marked to skip execution.
        /// </summary>
        public Func<Task> TODOAsync = () => Task.CompletedTask;

        /// <summary>
        /// Creates an assertion that verifies any exception was thrown by the Act phase.
        /// </summary>
        /// <returns>An action that asserts an exception occurred.</returns>
        /// <exception cref="ExceptionNotThrown">Thrown when no exception was thrown by the Act phase.</exception>
        public Action ShouldThrow()
        {
            return () =>
            {
                if (this.RunContext.LastActError == null)
                {
                    throw new ExceptionNotThrown(ExceptionHelpers.NoExceptionMessage());
                }

                this.RunContext.LastActErrorHandled();
            };
        }

        /// <summary>
        /// Creates an assertion that verifies any exception is thrown by the specified action.
        /// </summary>
        /// <param name="action">The action that should throw an exception.</param>
        /// <returns>An action that asserts an exception occurred.</returns>
        /// <exception cref="ExceptionNotThrown">Thrown when no exception was thrown by the action.</exception>
        public Action ShouldThrow(Action action)
        {
            return () =>
            {
                Exception? exSUT = null;

                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    exSUT = ex;
                }

                _ = exSUT ?? throw new ExceptionNotThrown(ExceptionHelpers.NoExceptionMessage());
            };
        }

        /// <summary>
        /// Creates an assertion that verifies a specific exception type was thrown by the Act phase.
        /// </summary>
        /// <typeparam name="TException">The expected exception type.</typeparam>
        /// <returns>An action that asserts the expected exception occurred.</returns>
        /// <exception cref="ExceptionNotThrown">Thrown when the expected exception was not thrown.</exception>
        public Action ShouldThrow<TException>()
            where TException : Exception
        {
            return () =>
            {
                _ = ExceptionHelpers.ExtractMatchOrThrow<TException>(this.RunContext.LastActError);

                this.RunContext.LastActErrorHandled();
            };
        }

        /// <summary>
        /// Creates an assertion that verifies a specific exception type is thrown by the specified action.
        /// </summary>
        /// <typeparam name="TException">The expected exception type.</typeparam>
        /// <param name="action">The action that should throw an exception.</param>
        /// <returns>An action that asserts the expected exception occurred.</returns>
        /// <exception cref="ExceptionNotThrown">Thrown when the expected exception was not thrown.</exception>
        public Action ShouldThrow<TException>(Action action)
            where TException : Exception
        {
            return () =>
            {
                Exception? exSUT = null;

                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    exSUT = ex;
                }

                _ = ExceptionHelpers.ExtractMatchOrThrow<TException>(exSUT);
            };
        }

        /// <summary>
        /// Creates an assertion that verifies an exception matching the predicate was thrown by the Act phase.
        /// </summary>
        /// <typeparam name="TException">The expected exception type.</typeparam>
        /// <param name="predicate">A predicate to verify exception properties.</param>
        /// <param name="description">Optional description of the predicate condition.</param>
        /// <returns>An action that asserts the expected exception with matching properties occurred.</returns>
        /// <exception cref="ExceptionNotThrown">Thrown when the expected exception was not thrown.</exception>
        /// <exception cref="PredicateException">Thrown when the exception doesn't match the predicate.</exception>
        public Action ShouldThrow<TException>(Predicate<TException> predicate, string? description = null)
        where TException : Exception
        {
            return () =>
            {
                var ex = ExceptionHelpers.ExtractMatchOrThrow<TException>(this.RunContext.LastActError);
                if (!predicate.Invoke(ex))
                {
                    throw new PredicateException(ExceptionHelpers.PredicateExceptionMessage(description));
                }
                this.RunContext.LastActErrorHandled();
            };
        }

        /// <summary>
        /// Creates an assertion that verifies an exception matching the predicate is thrown by the specified action.
        /// </summary>
        /// <typeparam name="TException">The expected exception type.</typeparam>
        /// <param name="action">The action that should throw an exception.</param>
        /// <param name="predicate">A predicate to verify exception properties.</param>
        /// <param name="description">Optional description of the predicate condition.</param>
        /// <returns>An action that asserts the expected exception with matching properties occurred.</returns>
        /// <exception cref="ExceptionNotThrown">Thrown when the expected exception was not thrown.</exception>
        /// <exception cref="PredicateException">Thrown when the exception doesn't match the predicate.</exception>
        public Action ShouldThrow<TException>(Action action, Predicate<TException> predicate, string? description = null)
            where TException : Exception
        {
            return () =>
            {
                TException? exSUT = null;

                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    exSUT = ex as TException;
                }

                exSUT = ExceptionHelpers.ExtractMatchOrThrow<TException>(exSUT);
                if (!predicate.Invoke(exSUT))
                {
                    throw new PredicateException(ExceptionHelpers.PredicateExceptionMessage(description));
                }
            };
        }


        /// <summary>
        /// Creates an async assertion that verifies any exception is thrown by the specified async task.
        /// </summary>
        /// <param name="task">The async task that should throw an exception.</param>
        /// <returns>An async action that asserts an exception occurred.</returns>
        /// <exception cref="ExceptionNotThrown">Thrown when no exception was thrown by the task.</exception>
        public Func<Task> ShouldThrowAsync(Func<Task> task)
        {
            return async () =>
            {
                Exception? exSUT = null;

                try
                {
                    await task.Invoke();
                }
                catch (Exception ex)
                {
                    exSUT = ex;
                }

                _ = exSUT ?? throw new ExceptionNotThrown(ExceptionHelpers.NoExceptionMessage());
            };
        }

        /// <summary>
        /// Creates an async assertion that verifies a specific exception type is thrown by the specified async task.
        /// </summary>
        /// <typeparam name="TException">The expected exception type.</typeparam>
        /// <param name="task">The async task that should throw an exception.</param>
        /// <returns>An async action that asserts the expected exception occurred.</returns>
        /// <exception cref="ExceptionNotThrown">Thrown when the expected exception was not thrown.</exception>
        public Func<Task> ShouldThrowAsync<TException>(Func<Task> task)
            where TException : Exception
        {
            return async () =>
            {
                Exception? exSUT = null;

                try
                {
                    await task.Invoke();
                }
                catch (Exception ex)
                {
                    exSUT = ex;
                }

                _ = ExceptionHelpers.ExtractMatchOrThrow<TException>(exSUT);
            };
        }

        /// <summary>
        /// Creates an async assertion that verifies an exception matching the predicate is thrown by the specified async task.
        /// </summary>
        /// <typeparam name="TException">The expected exception type.</typeparam>
        /// <param name="task">The async task that should throw an exception.</param>
        /// <param name="predicate">A predicate to verify exception properties.</param>
        /// <param name="description">Optional description of the predicate condition.</param>
        /// <returns>An async action that asserts the expected exception with matching properties occurred.</returns>
        /// <exception cref="ExceptionNotThrown">Thrown when the expected exception was not thrown.</exception>
        /// <exception cref="PredicateException">Thrown when the exception doesn't match the predicate.</exception>
        public Func<Task> ShouldThrowAsync<TException>(Func<Task> task, Predicate<TException> predicate, string? description = null)
        where TException : Exception
        {
            return async () =>
            {
                TException? exSUT = null;

                try
                {
                    await task.Invoke();
                }
                catch (Exception ex)
                {
                    exSUT = ex as TException;
                }

                exSUT = ExceptionHelpers.ExtractMatchOrThrow<TException>(exSUT);
                if (!predicate.Invoke(exSUT))
                {
                    throw new PredicateException(ExceptionHelpers.PredicateExceptionMessage(description));
                }
            };
        }
    }
}
