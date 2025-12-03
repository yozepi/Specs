using System;

namespace Specs.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a predicate condition fails during exception verification.
    /// Used in assertion methods that verify specific exception properties or conditions.
    /// </summary>
    public class PredicateException : Exception
    {
        internal PredicateException(string message) : base(message)
        {
        }
    }
}
