using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.UnitTests
{
    public class AssertBuilder_Tests : BaseAssertBuilderSpecs<BDD.AssertBuilder, Action>
    {
        protected override BDD.AssertBuilder Construct(IContextualTestCaseBuilder coreBuilder)
        {
            return new BDD.AssertBuilder(coreBuilder);
        }

        protected override Action EmptyAction() => () => { };

        [Feature]
        public void AssertBuilder()
        {
            SetSharedSpecs();
        }
    }
}
