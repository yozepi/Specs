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

       [Test]
        public void SetupEach_Specs()
        {
            RunSpecs(() => [typeof(SetUpEach)]);
        }

        [Test]
        public void Act_Specs()
        {
            RunSpecs(() => [typeof(Act)]);
        }

        [Test]
        public void AllMethodsInteraction_Specs()
        {
            RunSpecs(() => [typeof(AllMethodsInteraction)]);
        }

        [Test]
        public void TeardownEach_Specs()
        {
            RunSpecs(() => [typeof(TeardownEach)]);
        }

        [Test]
        public void TeardownOnce_Specs()
        {
            RunSpecs(() => [typeof(TeardownOnce)]);
        }
    }
}
