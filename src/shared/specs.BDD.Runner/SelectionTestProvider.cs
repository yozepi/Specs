using System;
using System.Collections.Generic;
using System.Linq;
using specs.core.Framework.Adapters;

namespace specs.BDD.Runner
{
    /// <summary>
    /// Bridges a runner's selected test / test-action pairs into
    /// <see cref="ITestProviderAdapter"/> for use by the core
    /// <see cref="specs.core.Framework.Processors.TestExecutors.TestCaseExecutor"/>.
    /// </summary>
    /// <remarks>
    /// Each test action is wrapped so that <see cref="SelectionLoggingAdapter{TTest}.BeginTest"/>
    /// is called immediately before the real assertion executes, so the start is
    /// recorded and the per-test timer begins at the right moment.
    /// </remarks>
    internal class SelectionTestProvider<TTest> : ITestProviderAdapter
    {
        private readonly IReadOnlyCollection<TestInstance> instances;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionTestProvider{TTest}"/> class.
        /// </summary>
        /// <param name="tests">
        /// The resolved test pairs — each entry is a test handle with its
        /// matching description and test action from the built hierarchy.
        /// </param>
        /// <param name="logger">
        /// The <see cref="SelectionLoggingAdapter{TTest}"/> that will be notified when each test
        /// begins, so it can record the start and begin timing.
        /// </param>
        public SelectionTestProvider(
            IEnumerable<(TTest test, string description, Action testAction)> tests,
            SelectionLoggingAdapter<TTest> logger)
        {
            if (tests == null) throw new ArgumentNullException(nameof(tests));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            instances = tests.Select(t => new TestInstance(
                t.description,
                () =>
                {
                    logger.BeginTest(t.description);
                    t.testAction();
                }
            )).ToList();
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<TestInstance> GetTestInstances() => instances;
    }
}
