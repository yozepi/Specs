using specs.core.Framework.Adaptors;
using specs.core.Framework.Excensions;
using System;

namespace specs.core.Framework.Processors
{
    /// <summary>
    /// For internal use only. The TestCaseProcessor is responsible for processing test cases.
    /// </summary>
    internal class TestCaseProcessor
    {
        private readonly TestCaseProcessorSettings settings;
        private readonly ILoggingAdapter logger;
        private readonly ITestContext testContext;

        public TestCaseProcessor(
            TestCaseProcessorSettings settings,
            ILoggingAdapter logger,
            ITestContext testContext)
        {
            this.settings = settings.OrThrowIfParameterIsNull(nameof(settings));
            this.logger = logger.OrThrowIfParameterIsNull(nameof(logger));
            this.testContext = testContext.OrThrowIfParameterIsNull($"The {nameof(testContext)} prameter is required.");
        }

        public void ProcessTestCase()
        {
            try
            {
                this.testContext.Setup();
                this.testContext.Act();

                try
                {
                    this.testContext.Assert();
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

                this.testContext.Teardown();
            }
            catch (Exception ex)
            {
                this.logger.LogException(ex);
                throw;
            }
        }
    }
}
