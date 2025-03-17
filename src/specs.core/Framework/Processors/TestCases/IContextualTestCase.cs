using System.Collections.Generic;

namespace specs.core.Framework.Processors.TestCases
{
    internal interface IContextualTestCase
    {
        IReadOnlyList<ITestCaseContext> ChildContexts { get; }
    }
}
