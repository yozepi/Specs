// NFluent and specs imports
using NFluent;
using NUnit.Framework;
using specs.core.Framework.Processors.TestCases;
using specs.specification.DSL;
using yozepi;

namespace specs.specification.UnitTests.DSL
{
    public class AssertBuilder_Tests : nSpecTestHarness
    {
        [Test]
        public void Assert_Specs()
        {
            RunSpecs(() => [typeof(AssertSpecs)]);
        }

        // Helper class to test Specifications integration
        private class TestSpecification : Specifications
        {
            // Expose protected It property for testing
            public AssertBuilder TestIt => It;
        }

        // Wrapper class to test internal constructor
        private class AssertBuilderTestWrapper : AssertBuilder
        {
            public AssertBuilderTestWrapper(IContextualTestCaseBuilder builder) : base(builder)
            {
            }
        }
    }
}
