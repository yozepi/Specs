using specs.core.Framework.Extensions;
using System;

namespace specs.core.Framework.Adapters
{
    internal class TestInstance
    {
        public TestInstance(string description, Action test)
        {
            Description = description.OrThrowIfParameterIsNull(nameof(description));
            Test = test.OrThrowIfParameterIsNull(nameof(test));
        }

        public string Description { get; }
        public Action Test { get; }
    }
}
