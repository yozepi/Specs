namespace specs.core.Framework.Processors.TestCases
{
    /// <summary>
    /// Represents a test case with an associated description.
    /// Extends <see cref="ITestCase"/> to include context information.
    /// </summary>
    public interface ITestCaseContext : ITestCase
    {
        /// <summary>
        /// Gets the description of this test context.
        /// </summary>
        string Description { get; }
    }
}
