using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using specs.core.Framework.Adapters;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// Bridges VSTest <see cref="TestCase"/> / test-action pairs into
    /// <see cref="ITestProviderAdapter"/> for use by the core
    /// <see cref="specs.core.Framework.Processors.TestExecutors.TestCaseExecutor"/>.
    /// </summary>
    /// <remarks>
    /// Each test action is wrapped so that <see cref="VSTestLoggingAdapter.BeginTest"/>
    /// is called immediately before the real assertion executes. This ensures
    /// <c>RecordStart</c> fires and the per-test timer begins at the right moment.
    /// </remarks>
    internal class VSTestTestProvider : ITestProviderAdapter
    {
        private readonly IReadOnlyCollection<TestInstance> instances;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSTestTestProvider"/> class.
        /// </summary>
        /// <param name="tests">
        /// The resolved test pairs — each entry is a <see cref="TestCase"/> with its
        /// matching description and test action from the built hierarchy.
        /// </param>
        /// <param name="logger">
        /// The <see cref="VSTestLoggingAdapter"/> that will be notified when each test
        /// begins, so it can call <c>RecordStart</c> and start timing.
        /// </param>
        public VSTestTestProvider(
            IEnumerable<(TestCase testCase, string description, Action testAction)> tests,
            VSTestLoggingAdapter logger)
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
