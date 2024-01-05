using System;
using System.Globalization;
using System.Threading.Tasks;
using Specs.Domain;

namespace Specs
{
    public class Spec
    {

        internal IRunContext RunContext { get; set; }

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

        private void AddAfterEach(Runner runner)
        {
            this.RunContext.AddAfterEach(runner);
        }

        public static Action TODO => () => { };

        public static Func<Task> TODOAsync = () => Task.CompletedTask;
    }
}
