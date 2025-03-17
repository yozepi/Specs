using specs.core.Framework.Extensions;
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
            this.setupOnceAction = setupOnceAction.OrThrowIfParameterIsNull(nameof(setupOnceAction));
            this.setupEachAction = setupEachAction.OrThrowIfParameterIsNull(nameof(setupEachAction));
            this.actAction = actAction.OrThrowIfParameterIsNull(nameof(actAction));
            this.teardownEachAction = teardownEachAction.OrThrowIfParameterIsNull(nameof(teardownEachAction));
            this.teardownOnceAction = teardownOnceAction.OrThrowIfParameterIsNull(nameof(teardownOnceAction));
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
