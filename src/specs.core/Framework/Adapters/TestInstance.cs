using specs.core.Framework.ParameterValidation;
using System;

namespace specs.core.Framework.Adapters
{
    internal class TestInstance
    {
        public TestInstance(string description, Action test)
        {
            Description = description.ThrowIfNullOrWhiteSpace(nameof(description));
            Test = test.ThrowIfNull(nameof(test));
        }

        public string Description { get; }
        public Action Test { get; }
    }
}
