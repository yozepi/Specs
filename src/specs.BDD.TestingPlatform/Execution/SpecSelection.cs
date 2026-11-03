using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Requests;
using specs.BDD.TestingPlatform.Discovery;

namespace specs.BDD.TestingPlatform.Execution
{
    /// <summary>
    /// Selects the spec leaves that match a platform test execution filter.
    /// </summary>
    internal static class SpecSelection
    {
        private static readonly PropertyBag NoFilterableProperties = new PropertyBag();

        private const int MaxDescribedUids = 3;

        /// <summary>
        /// Returns the leaves of <paramref name="forest"/> selected by <paramref name="filter"/>, in tree order.
        /// </summary>
        /// <param name="forest">The discovered spec tree.</param>
        /// <param name="filter">The platform filter, or <c>null</c> to select every leaf.</param>
        /// <param name="emptyUidListSelectsAll">
        /// When <c>true</c>, a test node UID list with no UIDs selects every leaf. C# Dev Kit sends an
        /// empty list when it re-discovers a project just before running it.
        /// </param>
        /// <exception cref="NotSupportedException">The filter type is not supported.</exception>
        public static IReadOnlyList<SpecNode> Select(SpecForest forest, ITestExecutionFilter? filter, bool emptyUidListSelectsAll = false)
        {
            var matches = BuildPredicate(filter, emptyUidListSelectsAll);
            return forest.Leaves.Where(matches).ToList();
        }

        /// <summary>
        /// Returns a one-line summary of <paramref name="filter"/> for diagnostics.
        /// </summary>
        public static string Describe(ITestExecutionFilter? filter)
        {
            switch (filter)
            {
                case null:
                    return "none";

                case NopFilter:
                    return nameof(NopFilter);

                case TestNodeUidListFilter uidFilter:
                    var uids = uidFilter.TestNodeUids;
                    var shown = string.Join(", ", uids.Take(MaxDescribedUids).Select(u => $"'{u.Value}'"));
                    var more = uids.Length > MaxDescribedUids ? ", …" : string.Empty;
                    return $"{nameof(TestNodeUidListFilter)} ({uids.Length} uids: {shown}{more})";

                case TreeNodeFilter treeFilter:
                    return $"{nameof(TreeNodeFilter)} '{treeFilter.Filter}'";

                case CompositeTestExecutionFilter composite:
                    return $"{nameof(CompositeTestExecutionFilter)} ({string.Join("; ", composite.Filters.Select(Describe))})";

                default:
                    return filter.GetType().Name;
            }
        }

        private static Func<SpecNode, bool> BuildPredicate(ITestExecutionFilter? filter, bool emptyUidListSelectsAll)
        {
            switch (filter)
            {
                case null:
                case NopFilter:
                    return _ => true;

                case TestNodeUidListFilter uidFilter:
                    if (emptyUidListSelectsAll && uidFilter.TestNodeUids.Length == 0)
                        return _ => true;

                    var uids = new HashSet<string>(uidFilter.TestNodeUids.Select(u => u.Value), StringComparer.Ordinal);
                    // Selecting a group node selects every spec beneath it.
                    return leaf => uids.Contains(leaf.Uid) || leaf.Ancestors().Any(a => uids.Contains(a.Uid));

                case TreeNodeFilter treeFilter:
                    // MatchesFilter accepts any node whose path is a prefix of the filter (it is meant for
                    // walking down a tree), so a leaf must also be at least as deep as the filter.
                    var filterDepth = SegmentCount(treeFilter.Filter);
                    return leaf => SegmentCount(leaf.Path) >= filterDepth
                        && treeFilter.MatchesFilter(leaf.Path, NoFilterableProperties);

                case CompositeTestExecutionFilter composite:
                    var parts = composite.Filters.Select(f => BuildPredicate(f, emptyUidListSelectsAll)).ToList();
                    return leaf => parts.All(matches => matches(leaf));

                default:
                    throw new NotSupportedException(
                        $"Specs does not support the '{filter.GetType().Name}' test execution filter.");
            }
        }

        /// <summary>
        /// Counts path separators, skipping escaped characters and '/' inside property brackets.
        /// </summary>
        internal static int SegmentCount(string path)
        {
            var count = 0;
            var bracketDepth = 0;
            for (var i = 0; i < path.Length; i++)
            {
                switch (path[i])
                {
                    case '\\':
                        i++;
                        break;
                    case '[':
                        bracketDepth++;
                        break;
                    case ']':
                        bracketDepth--;
                        break;
                    case '/' when bracketDepth == 0:
                        count++;
                        break;
                }
            }

            return count;
        }
    }
}
