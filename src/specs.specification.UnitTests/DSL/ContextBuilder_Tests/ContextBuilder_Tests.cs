using yozepi;

namespace specs.specification.UnitTests.DSL
{
    public class ContextBuilder_Tests : nSpecTestHarness
    {
        [Test]
        public void Describe_Specs()
        {
            RunSpecs(() => [typeof(DescribeSpecs)]);
        }

        [Test]
        public void Context_Specs()
        {
            RunSpecs(() => [typeof(ContextSpecs)]);
        }
    }
}
