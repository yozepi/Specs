using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using specs.BDD.Runner;
using specs.BDD.VSTestAdapter.Conversion;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// Maps a grouped root [Describe] run (see <see cref="GroupedSubtreeRunner"/>) to a single
    /// VSTest <see cref="TestResult"/> whose standard output carries the NSpec-style tree report.
    /// </summary>
    internal static class GroupedContextRunner
    {
        /// <summary>
        /// Executes the grouped context subtree and returns the aggregated
        /// <see cref="TestResult"/> without recording it (unit-testable core).
        /// </summary>
        internal static TestResult Execute(
            TestCase testCase,
            IContextualTestCase suiteRoot,
            Exception? rootSetupFailure,
            bool showSourceLocations,
            MarkerStyle markerStyle,
            CancellationToken cancellationToken)
        {
            var contextName = testCase.GetPropertyValue<string>(TestProperties.ContextPath, null) ?? "";
            var startTime = DateTimeOffset.UtcNow;
            var timer = Stopwatch.StartNew();

            var options = new SpecRunOptions(showSourceLocations ? testCase.Source : null, markerStyle);
            var report = GroupedSubtreeRunner.Run(suiteRoot, contextName, rootSetupFailure, options, cancellationToken);
            timer.Stop();

            return report == null
                ? ResultReporter.Failure(testCase, GroupedSubtreeRunner.MissingContext(contextName), startTime, timer.Elapsed)
                : ToTestResult(testCase, report, startTime, timer.Elapsed);
        }

        /// <summary>
        /// Converts a grouped report into the single aggregated VSTest result.
        /// </summary>
        internal static TestResult ToTestResult(
            TestCase testCase,
            GroupedReport report,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var result = new TestResult(testCase)
            {
                StartTime = startTime,
                EndTime = startTime + duration,
                Duration = duration,
                Outcome = report.Outcome switch
                {
                    GroupedOutcome.Passed => TestOutcome.Passed,
                    GroupedOutcome.Failed => TestOutcome.Failed,
                    _ => TestOutcome.Skipped
                },
                ErrorMessage = report.ErrorMessage,
                ErrorStackTrace = report.ErrorStackTrace
            };

            result.Messages.Add(new TestResultMessage(
                TestResultMessage.StandardOutCategory,
                report.Output));

            return result;
        }
    }
}
