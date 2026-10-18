using specs.core.Framework.Adapters;
using specs.core.Framework.ParameterValidation;
using System;
using System.Linq;
using System.Threading;

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
            this.settings = settings.ThrowIfNull(nameof(settings));
            this.logger = logger.ThrowIfNull(nameof(logger));
            this.testContext = testContext.ThrowIfNull(nameof(testContext));
            this.testProvider = tests;
        }

        public void ProcessTestCase(CancellationToken cancellationToken)
        {
            try
            {
                var tests = testProvider.GetTestInstances();
                if (tests.Any())
                {
                    testContext.SetupOnce();

                    foreach (var test in tests)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        testContext.SetupEach();

                        try
                        {
                            ActExceptionContext.Run(testContext.Act, test.Test);
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
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
                throw;
            }
        }
    }
}
