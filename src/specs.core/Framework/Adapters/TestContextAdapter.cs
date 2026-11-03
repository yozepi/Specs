using specs.core.Framework.ParameterValidation;
using specs.core.Framework.Processors.TestCases;
using System;

namespace specs.core.Framework.Adapters
{
    /// <summary>
    /// The simple way for a host to supply lifecycle phases when it already has them as delegates.
    /// </summary>
    public class TestContextAdapter : ITestContextAdapter
    {
        private readonly Action setupOnceAction;
        private readonly Action setupEachAction;
        private readonly Action actAction;
        private readonly Action teardownEachAction;
        private readonly Action teardownOnceAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestContextAdapter"/> class.
        /// </summary>
        /// <param name="setupOnce">One-time preparation shared by all tests.</param>
        /// <param name="setupEach">Per-test preparation.</param>
        /// <param name="act">The behavior under test.</param>
        /// <param name="teardownEach">Per-test cleanup.</param>
        /// <param name="teardownOnce">One-time cleanup.</param>
        public TestContextAdapter(Action setupOnce, Action setupEach, Action act, Action teardownEach, Action teardownOnce)
        {
            this.setupOnceAction = setupOnce.ThrowIfNull(nameof(setupOnce));
            this.setupEachAction = setupEach.ThrowIfNull(nameof(setupEach));
            this.actAction = act.ThrowIfNull(nameof(act));
            this.teardownEachAction = teardownEach.ThrowIfNull(nameof(teardownEach));
            this.teardownOnceAction = teardownOnce.ThrowIfNull(nameof(teardownOnce));
        }

        /// <summary>
        /// Adopts a built test case's lifecycle as-is, for hosts that run a whole tree through one executor.
        /// </summary>
        /// <param name="testCase">The built test case to take the lifecycle from.</param>
        public static TestContextAdapter FromTestCase(ITestCase testCase)
        {
            testCase.ThrowIfNull(nameof(testCase));
            return new TestContextAdapter(
                testCase.SetupOnce,
                testCase.SetupEach,
                testCase.Act,
                testCase.TeardownEach,
                testCase.TeardownOnce);
        }

        /// <inheritdoc/>
        public void SetupOnce()
        {
            setupOnceAction.Invoke();
        }

        /// <inheritdoc/>
        public void SetupEach()
        {
            setupEachAction.Invoke();
        }

        /// <inheritdoc/>
        public void Act()
        {
            actAction.Invoke();
        }

        /// <inheritdoc/>
        public void TeardownEach()
        {
            teardownEachAction.Invoke();
        }

        /// <inheritdoc/>
        public void TeardownOnce()
        {
            teardownOnceAction.Invoke();
        }
    }
}
