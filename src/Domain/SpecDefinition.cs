using System;
using System.Collections.Generic;
using System.Text;

namespace Specs.Domain
{
    internal class SpecDefinition : AbstractSpecDefinition
    {
        public Action Action { get; }

        public SpecDefinition(string name, Action action, bool dontRun) : base(name, dontRun)
        {
            this.Action = action;
        }
    }
}
