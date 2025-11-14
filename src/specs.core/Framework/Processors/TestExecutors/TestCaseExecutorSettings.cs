using System;

namespace specs.core.Framework.Processors.TestExecutors
{
    /// <summary>
    /// Configuration settings for test case execution behavior.
    /// </summary>
    public class TestCaseExecutorSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseExecutorSettings"/> class
        /// with default values. Sets <see cref="StopOnAssertFailure"/> to false.
        /// </summary>
        public TestCaseExecutorSettings()
        {
            StopOnAssertFailure = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether test execution should stop
        /// when an assertion failure occurs. When true, the executor will halt
        /// execution on the first assertion failure. When false (default),
        /// execution continues through all assertions.
        /// </summary>
        public bool StopOnAssertFailure { get; set; }
    }
}
