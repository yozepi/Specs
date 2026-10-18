using specs.core.Framework.ParameterValidation;
using System;

namespace specs.core.Framework.Adapters
{
    /// <summary>
    /// Pairs a spec's reported name with its assertion, so results can be attributed to the right spec.
    /// </summary>
    public class TestInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestInstance"/> class.
        /// </summary>
        /// <param name="description">The spec's name as it should appear in results.</param>
        /// <param name="test">The assertion that decides whether the spec passes.</param>
        public TestInstance(string description, Action test)
        {
            Description = description.ThrowIfNullOrWhiteSpace(nameof(description));
            Test = test.ThrowIfNull(nameof(test));
        }

        /// <summary>Gets the spec's name as it should appear in results.</summary>
        public string Description { get; }

        /// <summary>Gets the assertion that decides whether the spec passes.</summary>
        public Action Test { get; }
    }
}
