using System;
using System.Collections.Generic;
using System.Text;

namespace specs.core.Framework.Adapters
{
    internal interface ITestProviderAdapter
    {
        IReadOnlyCollection<TestInstance> GetTestInstances();
    }
}
