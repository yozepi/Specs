using System;

namespace Specs.Domain
{
    internal class ActionRunner : Runner
    {
        private readonly Action action;

        public ActionRunner(Action action)
        {
            this.action = action;
        }

        public override void Run()
        {
            this.action.Invoke();
        }
    }
}
