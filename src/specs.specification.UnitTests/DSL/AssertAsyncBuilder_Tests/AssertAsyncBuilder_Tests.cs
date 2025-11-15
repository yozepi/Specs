// NFluent and specs imports
using NFluent;
using NUnit.Framework;
using specs.core.Framework.Processors.TestCases;
using specs.specification.DSL;
using yozepi;

namespace specs.specification.UnitTests.DSL
{
    public class AssertAsyncBuilder_Tests : nSpecTestHarness
    {
        [Test]
        public void AssertAsync_Specs()
        {
            RunSpecs(() => [typeof(AssertAsyncSpecs)]);
        }

        // Helper class to test Specifications integration
        private class TestSpecification : Specifications
        {
            // Expose protected ItAsync property for testing
            public AssertAsyncBuilder TestItAsync => ItAsync;
        }

        // Wrapper class to test internal constructor
        private class AssertAsyncBuilderTestWrapper : AssertAsyncBuilder
        {
            public AssertAsyncBuilderTestWrapper(IContextualTestCaseBuilder builder) : base(builder)
            {
            }
        }
    }
}
