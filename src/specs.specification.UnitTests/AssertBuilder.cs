// NFluent and specs imports
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.UnitTests
{
    public class AssertBuilder : BaseAssertBuilderSpecs<BDD.AssertBuilder, Action>
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();   
        }

        protected override BDD.AssertBuilder Construct(IContextualTestCaseBuilder coreBuilder)
        {
            return new BDD.AssertBuilder(coreBuilder);
        }

        protected override Action EmptyAction() => () => { };

        private void AssertBuilder_Specs()
        {
            SetSharedSpecs();
        }

    }
}
