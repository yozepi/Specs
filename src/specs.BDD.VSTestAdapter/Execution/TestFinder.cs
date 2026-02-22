using System;
using System.Linq;
using specs.core.Framework.Processors.TestCases;

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
        private static readonly string[] PathSeparator = { " \u2192 " }; // " → "

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
            var segments = contextPath.Split(PathSeparator, StringSplitOptions.None);

            // Segment[0] is the root context name — we are already at the root, so skip it.
            // Each subsequent segment is a child context Description to navigate into.
            ITestCase current = root;
            for (int i = 1; i < segments.Length; i++)
            {
                if (current is IContextualTestCase parent)
                {
                    var child = parent.ChildContexts
                        .FirstOrDefault(c => c.Description == segments[i]);

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
