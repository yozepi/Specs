using System.Collections.Generic;

namespace specs.core.Framework.Adapters
{
    /// <summary>
    /// Lets a host decide which tests run, so selection (filters, discovery) stays outside the executor.
    /// </summary>
    public interface ITestProviderAdapter
    {
        /// <summary>
        /// Gets the tests the host has selected for this run.
        /// </summary>
        IReadOnlyCollection<TestInstance> GetTestInstances();
    }
}
