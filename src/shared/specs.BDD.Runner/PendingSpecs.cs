using System.Linq;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.Runner
{
    /// <summary>
    /// Queries the pending state of specs in a built suite tree.
    /// </summary>
    internal static class PendingSpecs
    {
        /// <summary>Returns <c>true</c> when the test named <paramref name="description"/> at <paramref name="node"/> is pending.</summary>
        public static bool IsPending(ITestCase node, string description) =>
            node.PendingTests.Contains(description);

        /// <summary>Returns <c>true</c> when <paramref name="node"/> or any descendant has a non-pending test.</summary>
        public static bool HasRunnableTests(ITestCase node) =>
            node.Tests.Any(t => !IsPending(node, t.description))
            || (node is IContextualTestCase contextual && contextual.ChildContexts.Any(HasRunnableTests));

        /// <summary>Returns <c>true</c> when <paramref name="node"/> or any descendant has at least one test.</summary>
        public static bool HasAnyTests(ITestCase node) =>
            node.Tests.Count > 0
            || (node is IContextualTestCase contextual && contextual.ChildContexts.Any(HasAnyTests));
    }
}
