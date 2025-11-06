using System;
using System.Collections.Generic;
using System.Text;

namespace Specs.Domain
{
    internal abstract class AbstractSpecDefinition<T> : ISpecDefinition
    {

        public string Name { get; }
        public bool DontRun { get; }

        protected T SpecAction { get; }

        protected AbstractSpecDefinition(string name, T specAction, bool dontRun)
        {
            this.Name = name;
            this.DontRun = dontRun;
            this.SpecAction = specAction;
        }

        public abstract void Run();
    }
}
 