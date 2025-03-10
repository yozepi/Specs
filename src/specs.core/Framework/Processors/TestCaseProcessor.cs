using specs.core.Framework.Adaptors;
using specs.core.Framework.Excensions;
using System;

namespace specs.core.Framework.Processors
{
    /// <summary>
    /// For internal use only. The TestCaseProcessor is responsible for processing test cases.
    /// </summary>
    public class TestCaseProcessor
    {
        private readonly TestCaseProcessorSettings settings;
        private readonly ILoggingAdapter logger;
        private readonly ISetupTestAdapter? setupAdapter;
        private readonly IActAdapter? actAdapter;
        private readonly IAssertAdapter assertAdapter;
        private readonly ITeardownAdapter? tearDownAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseProcessor"/> class.
        /// </summary>
        /// <param name="settings">Settings for the processor.</param>
        /// <param name="logger">The adapter used for logging."
        /// <param name="assert">The adapter that asserts the test passes.</param>
        /// <param name="setup">The adapter used for setting up the test environment. Optional.</param>
        /// <param name="act">The adapter used "act". Optional.</param>
        /// <param name="tearDown">The adapter used for tearing down the test environment. Optional.</param>
        public TestCaseProcessor(
            TestCaseProcessorSettings settings,
            ILoggingAdapter logger,
            IAssertAdapter assert,
            ISetupTestAdapter? setup = null,
            IActAdapter? act = null,
            ITeardownAdapter? tearDown = null)
        {
            this.settings = settings.OrThrowIfParameterIsNull(nameof(settings));
            this.logger = logger.OrThrowIfParameterIsNull(nameof(logger));
            this.assertAdapter = assert.OrThrowIfParameterIsNull($"The {nameof(assert)} prameter is required.");

            this.setupAdapter = setup;
            this.actAdapter = act;
            this.tearDownAdapter = tearDown;
        }

        public void ProcessTestCase()
        {
            try
            {
                this.setupAdapter?.Setup();
                this.actAdapter?.Act();

                try
                {
                    this.assertAdapter.Assert();
                    this.logger.LogAssertSuccess(this.settings.DescriptionBuilder());
                }
                catch (Exception ex)
                {
                    this.logger.LogAssertFailure(ex);
                    if(this.settings.StopOnAssertFailure)
                    {
                        throw;
                    }
                }

                this.tearDownAdapter?.Teardown();
            }
            catch (Exception ex)
            {
                this.logger.LogException(ex);
                throw;
            }
        }
    }
}
