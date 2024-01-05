using System;
using System.Globalization;
using Specs.Domain;

namespace Specs
{
    public class Spec
    {

        internal IRunContext RunContext { get; set; }

        public IndexedRunner<Action> It => new IndexedRunner<Action>((name, action) => this.AddSpec(new SpecDefinition(name, action, dontRun: action == TODO)));

        public IndexedRunner<Action> Context => new IndexedRunner<Action>(this.SwitchContext);

        public IndexedRunner<Action> Describe => new IndexedRunner<Action>(this.SwitchContext);


        public Action BeforeEach
        {
            set
            {
                this.AddBeforeEach(new ActionRunner(value));
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

        private void AddSpec(AbstractSpecDefinition spec)
        {
            this.RunContext.AddSpec(spec);
        }

        private void AddBeforeEach(Runner runner)
        {
            this.RunContext.AddBeforeEach(runner);
        }

        public static Action TODO => () => { };
    }
}
