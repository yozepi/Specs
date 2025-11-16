using System;
using System.Runtime;
using System.Runtime.ExceptionServices;

namespace specs.core.Framework.ParameterValidation
{
    internal static class ExceptionExtensions
    {
        internal static T OrThrowIfValueIsNull<T>(this T instance, string parameterName)
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

        internal static string OrThrowIfValueIsNullOrWhiteSpace(this string instance, string parameterName)
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
