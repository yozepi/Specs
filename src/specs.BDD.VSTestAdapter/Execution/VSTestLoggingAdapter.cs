using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using specs.core.Framework.Adapters;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// Bridges the <see cref="ILoggingAdapter"/> interface to VSTest's
    /// <see cref="IFrameworkHandle"/> for result reporting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Maps test descriptions to <see cref="TestCase"/> objects so that
    /// <see cref="LogAssertSuccess"/> and <see cref="LogAssertFailure"/>
    /// can produce proper <see cref="TestResult"/> objects and report them
    /// via <see cref="ITestExecutionRecorder.RecordResult(TestResult)"/>.
    /// </para>
    /// <para>
    /// Per-test timing starts when <see cref="BeginTest"/> is called (from the
    /// wrapped test action in <see cref="VSTestTestProvider"/>) and ends when
    /// the corresponding <c>LogAssert*</c> callback fires.
    /// </para>
    /// </remarks>
    internal class VSTestLoggingAdapter : ILoggingAdapter
    {
        private readonly IFrameworkHandle frameworkHandle;
        private readonly Dictionary<string, TestCase> testCaseMap;

        private Stopwatch? currentTimer;
        private DateTimeOffset currentStartTime;

        /// <summary>
        /// Gets the set of test descriptions that have been reported (either success or failure).
        /// Used by the caller to identify tests that were never reached (e.g., due to
        /// a SetupOnce or SetupEach failure) and report them as failed.
        /// </summary>
        internal HashSet<string> ReportedDescriptions { get; } = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VSTestLoggingAdapter"/> class.
        /// </summary>
        /// <param name="frameworkHandle">The VSTest framework handle for result reporting.</param>
        /// <param name="testCaseMap">
        /// A dictionary mapping test descriptions to their corresponding <see cref="TestCase"/>
        /// objects. Descriptions must be unique within a single execution group.
        /// </param>
        public VSTestLoggingAdapter(
            IFrameworkHandle frameworkHandle,
            Dictionary<string, TestCase> testCaseMap)
        {
            this.frameworkHandle = frameworkHandle ?? throw new ArgumentNullException(nameof(frameworkHandle));
            this.testCaseMap = testCaseMap ?? throw new ArgumentNullException(nameof(testCaseMap));
        }

        /// <summary>
        /// Signals that a test is about to execute. Calls <see cref="ITestExecutionRecorder.RecordStart"/>
        /// and starts the per-test timer. Must be called before the test action runs.
        /// </summary>
        /// <param name="description">The test description used to look up the <see cref="TestCase"/>.</param>
        internal void BeginTest(string description)
        {
            currentStartTime = DateTimeOffset.UtcNow;
            currentTimer = Stopwatch.StartNew();

            if (testCaseMap.TryGetValue(description, out var testCase))
            {
                frameworkHandle.RecordStart(testCase);
            }
        }

        /// <inheritdoc/>
        public void LogAssertSuccess(string message)
        {
            currentTimer?.Stop();
            ReportedDescriptions.Add(message);

            if (testCaseMap.TryGetValue(message, out var testCase))
            {
                var result = ResultReporter.Success(
                    testCase,
                    currentStartTime,
                    currentTimer?.Elapsed ?? TimeSpan.Zero);

                frameworkHandle.RecordResult(result);
            }
        }

        /// <inheritdoc/>
        public void LogAssertFailure(Exception exception, string message)
        {
            currentTimer?.Stop();
            ReportedDescriptions.Add(message);

            if (testCaseMap.TryGetValue(message, out var testCase))
            {
                var result = ResultReporter.Failure(
                    testCase,
                    exception,
                    currentStartTime,
                    currentTimer?.Elapsed ?? TimeSpan.Zero);

                frameworkHandle.RecordResult(result);
            }
        }

        /// <inheritdoc/>
        public void Log(string message)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Informational, message);
        }

        /// <inheritdoc/>
        public void LogException(Exception exception)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Error, exception.ToString());
        }
    }
}
