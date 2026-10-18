           using specs.core.Framework.Adapters;
using specs.core.Framework.ParameterValidation;
using System;
using System.Linq;
using System.Threading;

namespace specs.core.Framework.Processors.TestExecutors
{
    /// <summary>
    /// The single, canonical implementation of the spec lifecycle, so every front-end
    /// (VSTest adapter, custom runners) gets identical semantics by supplying only its own tests, context and logging.
    /// </summary>
    public class TestCaseExecutor
    {
        private readonly TestCaseExecutorSettings settings;
        private readonly ILoggingAdapter logger;
        private readonly ITestContextAdapter testContext;
        private readonly ITestProviderAdapter testProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseExecutor"/> class.
        /// </summary>
        /// <param name="settings">Tunes how the executor reacts to failures.</param>
        /// <param name="logger">Where results and failures are reported, keeping the executor independent of any host.</param>
        /// <param name="testContext">The host's lifecycle hooks, kept separate from the tests themselves.</param>
        /// <param name="testProvider">The tests to run, so the host decides which specs are selected.</param>
        public TestCaseExecutor(
            TestCaseExecutorSettings settings,
            ILoggingAdapter logger,
            ITestContextAdapter testContext,
            ITestProviderAdapter testProvider)
        {
            this.settings = settings.ThrowIfNull(nameof(settings));
            this.logger = logger.ThrowIfNull(nameof(logger));
            this.testContext = testContext.ThrowIfNull(nameof(testContext));
            this.testProvider = testProvider.ThrowIfNull(nameof(testProvider));
        }

        /// <summary>
        /// Executes the selected tests of one context as a complete spec lifecycle.
        /// </summary>
        /// <param name="cancellationToken">Lets the host stop the run between tests.</param>
        /// <remarks>
        /// One failing spec should not hide the others, so by default failures are reported and the run continues;
        /// <see cref="TestCaseExecutorSettings.StopOnAssertFailure"/> opts into fail-fast.
        /// </remarks>
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
