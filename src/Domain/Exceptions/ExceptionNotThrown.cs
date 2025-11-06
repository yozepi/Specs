using System;

namespace Specs.Domain.Exceptions
{
    public class ExceptionNotThrown : Exception
    {
        internal ExceptionNotThrown(string message) : base(message)
        {
        }
    }
}
