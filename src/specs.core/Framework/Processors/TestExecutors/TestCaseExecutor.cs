using specs.core.Framework.Adapters;
using specs.core.Framework.ParameterValidation;
using System;
using System.Linq;
using System.Runtime.ExceptionServices;
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

                    Exception? runFailure = null;
                    try
                    {
                        RunTests(tests, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        runFailure = ex;
                        throw;
                    }
                    finally
                    {
                        // Context cleanup always runs; it must not mask the original failure.
                        try
                        {
                            testContext.TeardownOnce();
                        }
                        catch when (runFailure != null)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
                throw;
            }
        }

        private void RunTests(System.Collections.Generic.IEnumerable<TestInstance> tests, CancellationToken cancellationToken)
        {
            foreach (var test in tests)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                Exception? failure = null;
                Exception? fatal = null;

                try
                {
                    testContext.SetupEach();
                    try
                    {
                        ActExceptionContext.Run(testContext.Act, test.Test);
                    }
                    catch (Exception ex)
                    {
                        failure = ex;
                    }
                }
                catch (Exception setupEx)
                {
                    fatal = setupEx;
                }

                // Teardown runs even if setup threw, so partially built resources are released.
                try
                {
                    testContext.TeardownEach();
                }
                catch (Exception teardownEx)
                {
                    // Outcome is reported after teardown so a throwing AfterEach fails this spec.
                    fatal ??= teardownEx;
                    failure ??= teardownEx;
                }

                if (fatal != null)
                {
                    logger.LogAssertFailure(failure ?? fatal, test.Description);
                    ExceptionDispatchInfo.Capture(fatal).Throw();
                }

                if (failure == null)
                {
                    logger.LogAssertSuccess(test.Description);
                }
                else
                {
                    logger.LogAssertFailure(failure, test.Description);
                    if (settings.StopOnAssertFailure)
                    {
                        ExceptionDispatchInfo.Capture(failure).Throw();
                    }
                }
            }
        }
    }
}
