using System;
using System.Runtime;

namespace specs.core.Framework.ParameterValidation
{
    internal static class ExceptionExtensions
    {
        internal static T OrThrowIfParameterIsNull<T>(this T instance, string parameterName)
            where T : notnull
        {
            return instance != null? instance: throw new NullReferenceException($"The {parameterName} parameter is required.");
        }
    }
}
