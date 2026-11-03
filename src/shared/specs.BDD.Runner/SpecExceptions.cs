using System;
using System.Reflection;

namespace specs.BDD.Runner
{
    /// <summary>
    /// Exception helpers shared by runners.
    /// </summary>
    internal static class SpecExceptions
    {
        /// <summary>
        /// Unwraps any layers of <see cref="TargetInvocationException"/> to expose the root cause.
        /// </summary>
        public static Exception Unwrap(Exception exception)
        {
            while (exception is TargetInvocationException tie && tie.InnerException != null)
                exception = tie.InnerException;

            return exception;
        }
    }
}
