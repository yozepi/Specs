namespace specs.core.Framework.Settings
{
    /// <summary>
    /// Global settings that control test framework behavior.
    /// </summary>
    public class TestSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether test execution should stop
        /// when an assertion failure occurs. When true, the framework will halt
        /// execution on the first assertion failure. When false (default),
        /// execution continues through all assertions.
        /// </summary>
        public bool StopOnAssertFailure { get; set; }
    }
}
