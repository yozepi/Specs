using System;
using System.Globalization;
using Specs.Domain;

namespace Specs
{
    public class Spec
    {

        internal IRunContext RunContext { get; set; }

        public ActionProcessor It => new ActionProcessor((name, action) => this.AddSpec(new SpecDefinition(name, action, dontRun: action == TODO)));

        public ActionProcessor Context => new ActionProcessor(this.AddChildContext);
        public ActionProcessor Describe => new ActionProcessor(this.AddChildContext);



        private void AddChildContext(string name, Action action)
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

        public static Action TODO => () => { };
    }
}
