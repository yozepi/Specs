using System;

namespace Specs.Domain.Exceptions
{
    public class PredicateException : Exception
    {
        internal PredicateException(string message) : base(message)
        {
        }
    }
}
