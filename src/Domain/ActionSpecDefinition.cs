using System;

namespace Specs.Domain
{
    internal class ActionSpecDefinition : AbstractSpecDefinition<Action>
    {

        public ActionSpecDefinition(string name, Action action, bool dontRun) 
            : base(name, action, dontRun)
        { }

        public override void Run()
        {
            this.SpecAction.Invoke();
        }
    }
}
