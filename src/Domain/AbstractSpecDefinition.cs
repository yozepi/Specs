using System;
using System.Collections.Generic;
using System.Text;

namespace Specs.Domain
{
    internal abstract class AbstractSpecDefinition
    {

        public string Name { get; }
        public bool DontRun { get; }

        protected AbstractSpecDefinition(string name, bool dontRun)
        {
            this.Name = name;
            this.DontRun = dontRun;
        }
    }
}
