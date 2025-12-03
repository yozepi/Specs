using System;

namespace Specs.Domain
{
    internal delegate void IndexedRunnerDelegate<T>(string name, T action);

    /// <summary>
    /// Provides indexer syntax for defining test contexts, descriptions, and assertions.
    /// Enables fluent BDD-style test definition using bracket notation.
    /// </summary>
    /// <typeparam name="T">The type of delegate used for test definitions (Action or Func&lt;Task&gt;).</typeparam>
    /// <example>
    /// <code>
    /// It["should calculate correctly"] = () => 
    /// {
    ///     // Test assertion
    /// };
    /// </code>
    /// </example>
    public class IndexedRunner<T> 
    {
        private readonly IndexedRunnerDelegate<T> runnerDelegate = null;

        internal IndexedRunner(IndexedRunnerDelegate<T> runnerDelegate)
        {
            this.runnerDelegate = runnerDelegate;
        }

        /// <summary>
        /// Sets the test definition using indexer syntax.
        /// The setter accepts a delegate that defines the test action or context.
        /// </summary>
        /// <param name="name">The description or name of the test or context.</param>
        /// <value>The delegate that defines the test logic or context setup.</value>
        public T this[string name]
        {
            set
            {
                this.runnerDelegate.Invoke(name, value);
            }
        }
    }
}
