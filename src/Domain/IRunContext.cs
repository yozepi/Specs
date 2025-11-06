using System;
using System.Collections.Generic;

namespace Specs.Domain
{
    internal interface IRunContext
    {
        IEnumerable<Exception> ContextErrors { get; }

        Exception? LastActError { get; }

        void LastActErrorHandled();

        void AddSpec(ISpecDefinition spec);

        IRunContext CreateChildContext(string name);

        void ContextRestored(IRunContext childContext);

        void AddBeforeEach(Runner runner);

        void AddAct(Runner runner);

        void AddAfterEach(Runner runner);

        void WriteContextErrors();
    }
}
