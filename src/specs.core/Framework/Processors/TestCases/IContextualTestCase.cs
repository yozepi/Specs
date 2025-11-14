using System.Collections.Generic;

namespace specs.core.Framework.Processors.TestCases
{
    /// <summary>
    /// Represents a test case that can contain child test contexts, forming a hierarchical structure.
    /// Child contexts inherit lifecycle actions from their parents.
    /// </summary>
    public interface IContextualTestCase : ITestCase
    {
        /// <summary>
        /// Gets the collection of child test contexts.
        /// Child contexts inherit and extend parent lifecycle actions.
        /// </summary>
        IReadOnlyList<IContextualTestCase> ChildContexts { get; }
    }
}
