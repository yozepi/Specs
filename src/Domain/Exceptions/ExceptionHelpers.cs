using System;
using System.Linq;
using System.Text;

namespace Specs.Domain.Exceptions
{
    internal static class ExceptionHelpers
    {
        public static string NoExceptionMessage()
        {
            return $"No exception was thrown.";
        }
        public static string IncorrectExceptionTypeMessage<TException>() where TException : Exception
        {
            return $"Exception of type \"{typeof(TException).Name}\" was not thrown.";
        }

        public static string ExceptionMismatchMessage<TExpected>(Exception actual) where TExpected : Exception
        {
            if (actual.GetType().Equals(typeof(AggregateException)))
            {
                return $"Exeption of type \"{typeof(TExpected).Name}\" was not found in the \"{typeof(AggregateException)}\" instance.";
            }

            return $"Expected exception of type \"{typeof(TExpected).Name}\" but \"{actual.GetType().Name}\" was thrown.";
        }

        public static string PredicateExceptionMessage(string? message)
        {
            var sb = new StringBuilder("Exception predicate failure");
            if(message != null)
            {
                sb.Append($": {message}");
            }
            else
            {
                sb.Append('.');
            }

            return sb.ToString();
        }

        public static TExpected ExtractMatchOrThrow<TExpected>(Exception? actual) where TExpected : Exception
        {
            if (actual == null)
            {
                throw new ExceptionNotThrown(IncorrectExceptionTypeMessage<TExpected>());
            }

            var expectedType = typeof(TExpected);

            if (expectedType.Equals(actual.GetType()))
            {
                return (TExpected)actual;
            }

            TExpected? match = null;
            var asAggregate = actual as AggregateException;
            if (asAggregate != null)
            {
                match = (TExpected)asAggregate.InnerExceptions.FirstOrDefault(ex => ex.GetType().Equals(expectedType));
            }

            if (match == null)
            {
                throw new ExceptionNotThrown(ExceptionMismatchMessage<TExpected>(actual));
            }

            return match;
        }
    }
}
