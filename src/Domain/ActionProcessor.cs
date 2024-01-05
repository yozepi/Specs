using System;

namespace Specs.Domain
{
    internal delegate void ActionDelegate(string name, Action action);

    public  class ActionProcessor
    {
        private readonly ActionDelegate deleGate = null;

        internal ActionProcessor(ActionDelegate actionDelegate)
        {
            this.deleGate = actionDelegate;
        }

        public Action this[string name]
        {
            set
            {
                this.deleGate.Invoke(name, value);
            }
        }
    }
}
