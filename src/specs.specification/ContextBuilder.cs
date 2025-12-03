using System;
using specs.core.Framework.ParameterValidation;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD
{
    /// <summary>
    /// Represents a BDD test context
    /// </summary>
    public class ContextBuilder
    {
        internal delegate void ContextCallBack(IContextualTestCaseBuilder childCoreBuilder, Action action);

        private readonly IContextualTestCaseBuilder baseCoreBuilder;
        private readonly ContextCallBack callBack;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="builder">The underlying test case builder to delegate to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
        internal ContextBuilder(IContextualTestCaseBuilder baseCoreBuilder, ContextCallBack callBack)
        {
            this.baseCoreBuilder = baseCoreBuilder ?? throw new ArgumentNullException(nameof(baseCoreBuilder));
            this.callBack = callBack ?? throw new ArgumentNullException(nameof(callBack));
        }

        /// <summary>
        /// Sets a child test context using indexer syntax.
        /// The setter creates a new nested context with the specified description.
        /// </summary>
        /// <param name="description">The description of the test context.</param>
        /// <value>An action that defines the context setup and nested tests.</value>
        /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
        public Action this[string description]
        {
            set
            {
                value.ThrowIfNull(nameof(value));

                baseCoreBuilder.ChildTestCase(description, childBuilder =>
                {
                    callBack.Invoke(childBuilder, value);
                });
            }
        }
    }
}
