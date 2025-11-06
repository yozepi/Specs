using System;

namespace specs.core.Framework.Processors.TestExecutors
{
    public class TestCaseExecutorSettings
    {
        public TestCaseExecutorSettings()
        {
            StopOnAssertFailure = false;
        }

        public bool StopOnAssertFailure { get; set; }
    }
}
