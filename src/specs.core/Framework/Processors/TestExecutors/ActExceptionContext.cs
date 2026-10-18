using specs.core.Framework.ParameterValidation;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace specs.core.Framework.Processors.TestExecutors
{
    /// <summary>
    /// Captures the exception thrown by a test's Act phase so the test's assertion can verify it.
    /// </summary>
    public static class ActExceptionContext
    {
        private sealed class Outcome
        {
            public Outcome(Exception? exception) => Exception = exception;

            public Exception? Exception { get; }

            public bool Handled { get; set; }

            public bool IsUnhandledFailure => Exception != null && !Handled;
        }

        private static readonly AsyncLocal<Outcome?> current = new AsyncLocal<Outcome?>();

        /// <summary>
        /// Gets the exception thrown by the Act phase of the currently running test, or <c>null</c> if none was thrown.
        /// </summary>
        public static Exception? Current => current.Value?.Exception;

        /// <summary>
        /// Marks the current Act exception as verified so the test is not failed with it.
        /// </summary>
        public static void MarkHandled()
        {
            var outcome = current.Value;
            if (outcome != null)
                outcome.Handled = true;
        }

        /// <summary>
        /// Runs <paramref name="act"/> followed by <paramref name="test"/>.
        /// An exception thrown by <paramref name="act"/> is exposed through <see cref="Current"/> while
        /// <paramref name="test"/> runs; unless the test calls <see cref="MarkHandled"/>, it is rethrown
        /// in place of any exception thrown by <paramref name="test"/>.
        /// </summary>
        /// <param name="act">The Act phase of the test.</param>
        /// <param name="test">The test assertion.</param>
        public static void Run(Action act, Action test)
        {
            act.ThrowIfNull(nameof(act));
            test.ThrowIfNull(nameof(test));

            Exception? actException = null;
            try
            {
                act();
            }
            catch (Exception ex)
            {
                actException = ex;
            }

            var outcome = new Outcome(actException);
            var previous = current.Value;
            current.Value = outcome;
            try
            {
                test();
            }
            catch (Exception) when (outcome.IsUnhandledFailure)
            {
                // The unverified Act exception is the root cause, so it is reported instead.
            }
            finally
            {
                current.Value = previous;
            }

            if (outcome.IsUnhandledFailure)
                ExceptionDispatchInfo.Capture(actException!).Throw();
        }
    }
}
