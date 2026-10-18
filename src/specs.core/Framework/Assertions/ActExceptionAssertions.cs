using specs.core.Framework.ParameterValidation;
using specs.core.Framework.Processors.TestExecutors;
using System;

namespace specs.core.Framework.Assertions
{
    /// <summary>
    /// Assertions that verify the exception thrown by the Act phase of the currently running test.
    /// Extension methods so they can be bound as test delegates whose target is the caller's verifier.
    /// </summary>
    public static class ActExceptionAssertions
    {
        /// <summary>
        /// Verifies the Act phase threw an exception assignable to <typeparamref name="TException"/>,
        /// then passes it to <paramref name="verify"/>.
        /// </summary>
        /// <typeparam name="TException">The expected exception type.</typeparam>
        /// <param name="verify">Verifies the properties of the thrown exception.</param>
        /// <exception cref="ExpectedExceptionNotThrownException">No exception, or an exception of another type, was thrown.</exception>
        public static void VerifyActException<TException>(this Action<TException> verify)
            where TException : Exception
        {
            verify.ThrowIfNull(nameof(verify));
            verify(ExtractActException<TException>());
        }

        private static TException ExtractActException<TException>()
            where TException : Exception
        {
            var actual = ActExceptionContext.Current;
            ActExceptionContext.MarkHandled();

            if (actual == null)
            {
                throw new ExpectedExceptionNotThrownException(
                    $"Expected {typeof(TException).Name} to be thrown, but no exception was thrown.");
            }

            if (actual is TException expected)
                return expected;

            throw new ExpectedExceptionNotThrownException(
                $"Expected {typeof(TException).Name} to be thrown, but {actual.GetType().Name} was thrown.",
                actual);
        }
    }
}
