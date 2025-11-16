using specs.core.Framework.ParameterValidation;
using System;

namespace specs.core.Framework.Adapters
{
    internal class TestInstance
    {
        public TestInstance(string description, Action test)
        {
            Description = description.OrThrowIfValueIsNullOrWhiteSpace(nameof(description));
            Test = test.OrThrowIfValueIsNull(nameof(test));
        }

        public string Description { get; }
        public Action Test { get; }
    }
}
