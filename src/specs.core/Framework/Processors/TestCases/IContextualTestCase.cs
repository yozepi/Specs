using System.Collections.Generic;

namespace specs.core.Framework.Processors.TestCases
{
    internal interface IContextualTestCase : ITestCase
    {
        IReadOnlyList<ITestCaseContext> ChildContexts { get; }
    }
}
