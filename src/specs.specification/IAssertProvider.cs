using System;
using System.Collections.Generic;
using System.Text;

namespace specs.BDD
{
    /// <summary>
    /// Defines a generic interface for assertion providers that use indexer syntax.
    /// Enables fluent BDD-style assertion definitions with bracket notation.
    /// </summary>
    /// <typeparam name="T">The delegate type used for assertions (Action or Func&lt;Task&gt;).</typeparam>
    public interface IAssertProvider<T> where T : Delegate
    {
        /// <summary>
        /// Sets the assertion definition using indexer syntax.
        /// </summary>
        /// <param name="description">The description of the assertion being tested.</param>
        /// <value>The delegate that contains the assertion logic.</value>
        T this[string description] { set; }
    }
}
