using System;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// Creates VSTest <see cref="TestResult"/> instances from test execution outcomes.
    /// </summary>
    internal static class ResultReporter
    {
        /// <summary>
        /// Creates a <see cref="TestOutcome.Passed"/> result.
        /// </summary>
        public static TestResult Success(
            TestCase testCase,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            return new TestResult(testCase)
            {
                Outcome = TestOutcome.Passed,
                StartTime = startTime,
                EndTime = startTime + duration,
                Duration = duration
            };
        }

        /// <summary>
        /// Creates a <see cref="TestOutcome.Failed"/> result from an exception.
        /// Unwraps <see cref="TargetInvocationException"/> wrappers so that the error message
        /// and stack trace come from the actual cause rather than the reflection machinery.
        /// </summary>
        public static TestResult Failure(
            TestCase testCase,
            Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var cause = Unwrap(exception);

            return new TestResult(testCase)
            {
                Outcome = TestOutcome.Failed,
                StartTime = startTime,
                EndTime = startTime + duration,
                Duration = duration,
                ErrorMessage = cause.Message,
                ErrorStackTrace = cause.StackTrace ?? string.Empty
            };
        }

        // ── Private helpers ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Unwraps any layers of <see cref="TargetInvocationException"/> to expose the root cause.
        /// </summary>
        internal static Exception Unwrap(Exception exception)
        {
            while (exception is TargetInvocationException tie && tie.InnerException != null)
                exception = tie.InnerException;

            return exception;
        }
    }
}
