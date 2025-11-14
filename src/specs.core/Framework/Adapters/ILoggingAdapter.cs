using System;

namespace specs.core.Framework.Adapters
{
    /// <summary>
    /// Provides logging functionality for test execution and assertions.
    /// </summary>
    public interface ILoggingAdapter
    {
        /// <summary>
        /// Logs a general message during test execution.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Log(string message);

        /// <summary>
        /// Logs an assertion failure with the associated exception and message.
        /// </summary>
        /// <param name="exception">The exception that caused the assertion failure.</param>
        /// <param name="message">A description of the assertion failure.</param>
        void LogAssertFailure(Exception exception, string message);

        /// <summary>
        /// Logs a successful assertion.
        /// </summary>
        /// <param name="message">A description of the successful assertion.</param>
        void LogAssertSuccess(string message);

        /// <summary>
        /// Logs an exception that occurred during test execution.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        void LogException(Exception exception);
    }
}
