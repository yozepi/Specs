using NFluent;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public class ContextualTestCaseBuilder_Tests : nSpecTestHarness
    {
        [Test]
        public void SetupOnce_Specs()
        {
            RunSpecs(() => [typeof(SetupOnce)]);
        }

    }
}
