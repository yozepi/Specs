using System;

namespace specs.BDD.VSTestAdapter.Discovery
{
    /// <summary>
    /// Thrown when a class inheriting from <see cref="specs.BDD.Specifications"/> has no methods
    /// decorated with the <see cref="specs.BDD.Attributes.DescribeAttribute"/>.
    /// All Specification classes must define at least one [Describe] method to be valid.
    /// </summary>
    public class InvalidSpecificationException : Exception
    {
        /// <summary>
        /// Initializes a new instance with the specified message.
        /// </summary>
        public InvalidSpecificationException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance with the specified message and inner exception.
        /// </summary>
        public InvalidSpecificationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
