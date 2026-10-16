using specs.BDD.VSTestAdapter.Discovery;
using specs.core.Framework.Processors.TestCases;
using System;
using System.Linq;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// Navigates an <see cref="IContextualTestCase"/> hierarchy to locate a specific
    /// test action by context path and description.
    /// </summary>
    /// <remarks>
    /// The context path is the same string that was embedded into the
    /// <see cref="Conversion.TestProperties.ContextPath"/> metadata property during discovery
    /// (e.g., "root context → when doing X → when doing Y").
    /// Segments are separated by the Unicode arrow " → " (U+2192).
    /// The first segment is the root context name; subsequent segments are child context descriptions.
    /// </remarks>
    internal static class TestFinder
    {

        /// <summary>
        /// Locates the <see cref="ITestCase"/> node and its matching <see cref="Action"/>
        /// for the given <paramref name="contextPath"/> and <paramref name="testDescription"/>
        /// within the built test hierarchy.
        /// </summary>
        /// <param name="root">The root of the hierarchy returned by the builder.</param>
        /// <param name="contextPath">
        /// The full context path as stored on the discovered test —
        /// e.g. "root context → when doing X".
        /// </param>
        /// <param name="testDescription">The <c>It[...]</c> description of the target test.</param>
        /// <returns>
        /// A tuple of (ITestCase context, Action testAction) if found; <c>null</c> otherwise.
        /// </returns>
        public static (ITestCase Context, Action TestAction)? Find(
            IContextualTestCase root,
            string contextPath,
            string testDescription)
        {
            var segments = contextPath.Split(TestStructureExtractor.PathSeparator, StringSplitOptions.None);

            // Navigate ALL segments from root.ChildContexts downward.
            // The tree root (built by SuiteTreeBuilder) has no description of its own;
            // each segment corresponds to a child context description.
            ITestCase current = root;
            foreach (var segment in segments)
            {
                if (current is IContextualTestCase parent)
                {
                    var child = parent.ChildContexts
                        .FirstOrDefault(c => c.Description == segment);

                    if (child == null)
                        return null;

                    current = child;
                }
                else
                {
                    // Reached a leaf node before exhausting path segments — not found.
                    return null;
                }
            }

            // Locate the specific test at this node by description.
            foreach (var (description, test) in current.Tests)
            {
                if (description == testDescription)
                    return (current, test);
            }

            return null;
        }
    }
}
