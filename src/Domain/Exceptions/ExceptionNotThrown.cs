using System;

namespace Specs.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when an expected exception was not thrown during test execution.
    /// Used in assertion methods that verify exception throwing behavior.
    /// </summary>
    public class ExceptionNotThrown : Exception
    {
        internal ExceptionNotThrown(string message) : base(message)
        {
        }
    }
}
