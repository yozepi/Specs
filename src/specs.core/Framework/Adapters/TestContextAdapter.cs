using specs.core.Framework.ParameterValidation;
using System;

namespace specs.core.Framework.Adapters
{
    internal class TestContextAdapter : ITestContextAdapter
    {
        private readonly Action setupOnceAction;
        private readonly Action setupEachAction;
        private readonly Action actAction;
        private readonly Action teardownEachAction;
        private readonly Action teardownOnceAction;

        public TestContextAdapter(Action setupOnceAction, Action setupEachAction, Action actAction, Action teardownEachAction, Action teardownOnceAction, Action test)
        {
            this.setupOnceAction = setupOnceAction.OrThrowIfValueIsNull(nameof(setupOnceAction));
            this.setupEachAction = setupEachAction.OrThrowIfValueIsNull(nameof(setupEachAction));
            this.actAction = actAction.OrThrowIfValueIsNull(nameof(actAction));
            this.teardownEachAction = teardownEachAction.OrThrowIfValueIsNull(nameof(teardownEachAction));
            this.teardownOnceAction = teardownOnceAction.OrThrowIfValueIsNull(nameof(teardownOnceAction));
        }

        public void SetupOnce()
        {
            setupOnceAction.Invoke();
        }

        public void SetupEach()
        {
            setupEachAction.Invoke();
        }

        public void Act()
        {
            actAction.Invoke();
        }

        public void TeardownEach()
        {
            teardownEachAction.Invoke();
        }

        public void TeardownOnce()
        {
            teardownOnceAction.Invoke();
        }
    }
}
