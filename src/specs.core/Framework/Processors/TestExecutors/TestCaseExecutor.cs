using specs.core.Framework.Adapters;
using specs.core.Framework.Extensions;
using System;

namespace specs.core.Framework.Processors.TestExecutors
{
    /// <summary>
    /// For internal use only. The TestCaseProcessor is responsible for processing test cases.
    /// </summary>
    internal class TestCaseExecutor
    {
        private readonly TestCaseExecutorSettings settings;
        private readonly ILoggingAdapter logger;
        private readonly ITestContextAdapter testContext;
        private readonly ITestProviderAdapter testProvider;

        public TestCaseExecutor(
            TestCaseExecutorSettings settings,
            ILoggingAdapter logger,
            ITestContextAdapter testContext,
            ITestProviderAdapter tests)
        {
            this.settings = settings.OrThrowIfParameterIsNull(nameof(settings));
            this.logger = logger.OrThrowIfParameterIsNull(nameof(logger));
            this.testContext = testContext.OrThrowIfParameterIsNull($"The {nameof(testContext)} prameter is required.");
            this.testProvider = tests;
        }

        public void ProcessTestCase()
        {
            try
            {
                testContext.SetupOnce();

                var tests = testProvider.GetTestInstances();
                foreach(var test in tests)
                {
                    testContext.SetupEach();
                    testContext.Act();

                    try
                    {
                        test.Test.Invoke();
                        logger.LogAssertSuccess(test.Description);
                    }
                    catch (Exception ex)
                    {
                        logger.LogAssertFailure(ex, test.Description);
                        if (settings.StopOnAssertFailure)
                        {
                            throw;
                        }
                    }

                    testContext.TeardownEach();
                }

                testContext.TeardownOnce();
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
                throw;
            }
        }
    }
}
