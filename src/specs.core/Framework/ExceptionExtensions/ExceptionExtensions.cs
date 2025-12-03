using System;
using System.Runtime;
using System.Runtime.ExceptionServices;

namespace specs.core.Framework.ParameterValidation
{
    /// <summary>
    /// Provides extension methods for parameter validation with exception handling.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the instance is null.
        /// Preserves the original stack trace when throwing.
        /// </summary>
        /// <typeparam name="T">The type of the instance to validate.</typeparam>
        /// <param name="instance">The instance to check for null.</param>
        /// <param name="parameterName">The name of the parameter being validated.</param>
        /// <returns>The original instance if not null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        public static T ThrowIfNull<T>(this T instance, string parameterName)
            where T : notnull
        {
            Exception? exception = null;
            if (instance == null)
            {
                exception = new ArgumentNullException(parameterName, $"{parameterName} cannot be null.");
            }

            if (exception != null)
            {
                // Capture and throw to preserve original stack trace
                ExceptionDispatchInfo.Capture(exception).Throw();
            }

            return instance!;
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the string is null,
        /// or an <see cref="ArgumentException"/> if the string is empty or whitespace.
        /// Preserves the original stack trace when throwing.
        /// </summary>
        /// <param name="instance">The string to check.</param>
        /// <param name="parameterName">The name of the parameter being validated.</param>
        /// <returns>The original string if not null, empty, or whitespace.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="instance"/> is empty or whitespace.</exception>
        public static string ThrowIfNullOrWhiteSpace(this string instance, string parameterName)
        {
            Exception? exception = null;
            if (instance == null)
            {
                exception = new ArgumentNullException(parameterName, $"{parameterName} cannot be null.");
            }
            else if (string.IsNullOrWhiteSpace(instance))
            {
                exception = new ArgumentException($"{parameterName} cannot be empty or whitespace.", parameterName);
            }

            if (exception != null)
            {
                // Capture and throw to preserve original stack trace
                ExceptionDispatchInfo.Capture(exception).Throw();
            }

            return instance!;
        }
    }
}
