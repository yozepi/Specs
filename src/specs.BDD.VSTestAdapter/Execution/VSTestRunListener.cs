using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using specs.BDD.Runner;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// Records shared-runner events as VSTest results on an <see cref="IFrameworkHandle"/>.
    /// </summary>
    internal sealed class VSTestRunListener : ISpecRunListener<TestCase>
    {
        private readonly IFrameworkHandle _frameworkHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSTestRunListener"/> class.
        /// </summary>
        public VSTestRunListener(IFrameworkHandle frameworkHandle)
        {
            _frameworkHandle = frameworkHandle ?? throw new ArgumentNullException(nameof(frameworkHandle));
        }

        /// <inheritdoc/>
        public void Started(TestCase test) => _frameworkHandle.RecordStart(test);

        /// <inheritdoc/>
        public void Passed(TestCase test, DateTimeOffset startTime, TimeSpan duration) =>
            _frameworkHandle.RecordResult(ResultReporter.Success(test, startTime, duration));

        /// <inheritdoc/>
        public void Failed(TestCase test, Exception exception, DateTimeOffset startTime, TimeSpan duration) =>
            _frameworkHandle.RecordResult(ResultReporter.Failure(test, exception, startTime, duration));

        /// <inheritdoc/>
        public void Pending(TestCase test, DateTimeOffset startTime, TimeSpan duration) =>
            _frameworkHandle.RecordResult(ResultReporter.Skipped(test, startTime, duration));

        /// <inheritdoc/>
        public void GroupedCompleted(TestCase test, GroupedReport report, DateTimeOffset startTime, TimeSpan duration) =>
            _frameworkHandle.RecordResult(GroupedContextRunner.ToTestResult(test, report, startTime, duration));

        /// <inheritdoc/>
        public void LogInformation(string message) =>
            _frameworkHandle.SendMessage(TestMessageLevel.Informational, message);

        /// <inheritdoc/>
        public void LogError(string message) =>
            _frameworkHandle.SendMessage(TestMessageLevel.Error, message);
    }
}
