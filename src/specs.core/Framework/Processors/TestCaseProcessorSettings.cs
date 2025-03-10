using System;

namespace specs.core.Framework.Processors
{
    public class TestCaseProcessorSettings
    {
        public TestCaseProcessorSettings(Func<string> descriptionBuilder)
        {
            this.DescriptionBuilder = descriptionBuilder;
            this.StopOnAssertFailure = false;
        }

        public bool StopOnAssertFailure { get; set; }

        public Func<string> DescriptionBuilder { get;  }
    }
}
