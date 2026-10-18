using System;

namespace specs.core.Framework.Assertions
{
    /// <summary>
    /// Thrown when a test expects the Act phase to throw a specific exception type and it did not.
    /// </summary>
    public class ExpectedExceptionNotThrownException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedExceptionNotThrownException"/> class.
        /// </summary>
        /// <param name="message">The failure message.</param>
        public ExpectedExceptionNotThrownException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedExceptionNotThrownException"/> class.
        /// </summary>
        /// <param name="message">The failure message.</param>
        /// <param name="innerException">The exception that was actually thrown.</param>
        public ExpectedExceptionNotThrownException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
