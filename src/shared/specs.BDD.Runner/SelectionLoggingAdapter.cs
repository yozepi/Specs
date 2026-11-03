using System;
using System.Diagnostics;
using System.Collections.Generic;
using specs.core.Framework.Adapters;

namespace specs.BDD.Runner
{
    /// <summary>
    /// Bridges the <see cref="ILoggingAdapter"/> interface to a runner's
    /// <see cref="ISpecRunListener{TTest}"/> for result reporting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Maps test descriptions to the runner's test handles so that
    /// <see cref="LogAssertSuccess"/> and <see cref="LogAssertFailure"/>
    /// can report the right test.
    /// </para>
    /// <para>
    /// Per-test timing starts when <see cref="BeginTest"/> is called (from the
    /// wrapped test action in <see cref="SelectionTestProvider{TTest}"/>) and ends when
    /// the corresponding <c>LogAssert*</c> callback fires.
    /// </para>
    /// </remarks>
    internal class SelectionLoggingAdapter<TTest> : ILoggingAdapter
    {
        private readonly ISpecRunListener<TTest> listener;
        private readonly Dictionary<string, TTest> testMap;

        private Stopwatch? currentTimer;
        private DateTimeOffset currentStartTime;

        /// <summary>
        /// Gets the set of test descriptions that have been reported (either success or failure).
        /// Used by the caller to identify tests that were never reached (e.g., due to
        /// a SetupOnce or SetupEach failure) and report them as failed.
        /// </summary>
        internal HashSet<string> ReportedDescriptions { get; } = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionLoggingAdapter{TTest}"/> class.
        /// </summary>
        /// <param name="listener">The runner listener that records results.</param>
        /// <param name="testMap">
        /// A dictionary mapping test descriptions to their test handles.
        /// Descriptions must be unique within a single execution group.
        /// </param>
        public SelectionLoggingAdapter(
            ISpecRunListener<TTest> listener,
            Dictionary<string, TTest> testMap)
        {
            this.listener = listener ?? throw new ArgumentNullException(nameof(listener));
            this.testMap = testMap ?? throw new ArgumentNullException(nameof(testMap));
        }

        /// <summary>
        /// Signals that a test is about to execute and starts the per-test timer.
        /// Must be called before the test action runs.
        /// </summary>
        /// <param name="description">The test description used to look up the test handle.</param>
        internal void BeginTest(string description)
        {
            currentStartTime = DateTimeOffset.UtcNow;
            currentTimer = Stopwatch.StartNew();

            if (testMap.TryGetValue(description, out var test))
            {
                listener.Started(test);
            }
        }

        /// <inheritdoc/>
        public void LogAssertSuccess(string message)
        {
            currentTimer?.Stop();
            ReportedDescriptions.Add(message);

            if (testMap.TryGetValue(message, out var test))
            {
                listener.Passed(test, currentStartTime, currentTimer?.Elapsed ?? TimeSpan.Zero);
            }
        }

        /// <inheritdoc/>
        public void LogAssertFailure(Exception exception, string message)
        {
            currentTimer?.Stop();
            ReportedDescriptions.Add(message);

            if (testMap.TryGetValue(message, out var test))
            {
                listener.Failed(test, exception, currentStartTime, currentTimer?.Elapsed ?? TimeSpan.Zero);
            }
        }

        /// <inheritdoc/>
        public void Log(string message)
        {
            listener.LogInformation(message);
        }

        /// <inheritdoc/>
        public void LogException(Exception exception)
        {
            listener.LogError(exception.ToString());
        }
    }
}
