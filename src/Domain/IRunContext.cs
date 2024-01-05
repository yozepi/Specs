using System;
using System.Collections.Generic;
using System.Text;

namespace Specs.Domain
{
    internal interface IRunContext
    {
        string Name { get; }

        void AddSpec(AbstractSpecDefinition spec);

        IRunContext CreateChildContext(string name);
        void ContextRestored(IRunContext childContext);
    }
}
