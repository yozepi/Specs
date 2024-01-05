using System;
using System.Collections.Generic;
using System.Text;

namespace Specs.Domain.Exceptions
{
    public class ContextException : Exception
    {
        internal ContextException(Exception innerException)
            : base($"Context exeption {innerException.GetType().Name}: {innerException.Message}", innerException)
        { }
    }
}
