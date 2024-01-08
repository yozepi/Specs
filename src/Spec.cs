using System;
using System.Threading.Tasks;
using Specs.Domain;
using Specs.Domain.Exceptions;

namespace Specs
{
    public class Spec
    {

        internal IRunContext RunContext { get; set; } = null!;

        public IndexedRunner<Action> It => new IndexedRunner<Action>((name, action) => this.AddSpec(new ActionSpecDefinition(name, action, dontRun: action == TODO)));
        public IndexedRunner<Func<Task>> ItAsync => new IndexedRunner<Func<Task>>((name, func) => this.AddSpec(new TaskSpecDefinition(name, func, dontRun: func == TODOAsync)));

        public IndexedRunner<Action> Context => new IndexedRunner<Action>(this.SwitchContext);

        public IndexedRunner<Action> Describe => new IndexedRunner<Action>(this.SwitchContext);


        public Action BeforeEach
        {
            set
            {
                this.AddBeforeEach(new ActionRunner(value));
            }
        }

        public Func<Task> BeforeEachAsync
        {
            set
            {
                this.AddBeforeEach(new TaskRunner(value));
            }
        }


        public Action Act
        {
            set
            {
                this.AddAct(new ActionRunner(value));
            }
        }

        public Func<Task> ActAsync
        {
            set
            {
                this.AddAct(new TaskRunner(value));
            }
        }


        public Action AfterEach
        {
            set
            {
                this.AddAfterEach(new ActionRunner(value));
            }
        }

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

        public Action TODO => () => { };

        public Func<Task> TODOAsync = () => Task.CompletedTask;

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

        public Action ShouldThrow<TException>()
            where TException : Exception
        {
            return () =>
            {
                _ = ExceptionHelpers.ExtractMatchOrThrow<TException>(this.RunContext.LastActError);

                this.RunContext.LastActErrorHandled();
            };
        }

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