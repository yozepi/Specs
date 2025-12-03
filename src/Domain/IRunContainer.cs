namespace Specs.Domain
{
    /// <summary>
    /// Represents a container for test execution with metadata about its execution status.
    /// </summary>
    public interface IRunContainer
    {
        /// <summary>
        /// Gets the name of this test container.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets a value indicating whether this container should be skipped during test execution.
        /// </summary>
        bool DontRun { get; }
    }
}
