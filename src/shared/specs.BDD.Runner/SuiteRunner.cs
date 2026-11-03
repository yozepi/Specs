using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.Runner
{
    /// <summary>
    /// Runs the selected tests of one specification class against its built suite tree.
    /// </summary>
    /// <remarks>
    /// Grouped items run their whole [Describe] subtree as one result; pending items are
    /// reported up front and never touch any lifecycle; the rest go through the
    /// <see cref="SelectionTreeWalker{TTest}"/>. The root BeforeAll/AfterAll (spec constructor)
    /// run once per class, and only when at least one selected spec will actually execute.
    /// </remarks>
    internal static class SuiteRunner
    {
        /// <summary>
        /// Runs <paramref name="items"/> against <paramref name="root"/>, reporting through <paramref name="listener"/>.
        /// </summary>
        public static void Run<TTest>(
            IContextualTestCase root,
            IReadOnlyList<SpecRunItem<TTest>> items,
            ISpecRunListener<TTest> listener,
            SpecRunOptions options,
            CancellationToken cancellationToken)
        {
            if (items.Count == 0) return;

            var groupedItems = new List<SpecRunItem<TTest>>();
            var regularItems = new List<SpecRunItem<TTest>>();
            var pendingItems = new List<SpecRunItem<TTest>>();
            foreach (var item in items)
            {
                if (item.IsGrouped)
                    groupedItems.Add(item);
                else if (IsPending(root, item))
                    pendingItems.Add(item);
                else
                    regularItems.Add(item);
            }

            var pendingStart = DateTimeOffset.UtcNow;
            foreach (var item in pendingItems)
            {
                listener.Started(item.Test);
                listener.Pending(item.Test, pendingStart, TimeSpan.Zero);
            }

            var runRootLifecycle = regularItems.Count > 0
                || groupedItems.Any(item => GroupedContextHasRunnableTests(root, item));

            Exception? rootSetupFailure = null;
            if (runRootLifecycle)
            {
                try
                {
                    root.OwnSetupOnce();
                }
                catch (Exception ex)
                {
                    rootSetupFailure = ex;
                }
            }

            try
            {
                foreach (var item in groupedItems)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    RunGrouped(item, root, listener, rootSetupFailure, options, cancellationToken);
                }

                if (regularItems.Count == 0) return;

                if (rootSetupFailure != null)
                {
                    FailAll(regularItems, listener, rootSetupFailure);
                    return;
                }

                new SelectionTreeWalker<TTest>(root, "", regularItems, listener, cancellationToken).Walk();
            }
            finally
            {
                if (runRootLifecycle && rootSetupFailure == null)
                {
                    // Best-effort AfterAll — swallow so we don't mask test failures.
                    try { root.OwnTeardownOnce(); } catch { /* best-effort */ }
                }
            }
        }

        /// <summary>
        /// Reports every item as failed with <paramref name="exception"/> (e.g. when the suite cannot be built).
        /// </summary>
        public static void FailAll<TTest>(
            IEnumerable<SpecRunItem<TTest>> items,
            ISpecRunListener<TTest> listener,
            Exception exception)
        {
            var startTime = DateTimeOffset.UtcNow;
            foreach (var item in items)
            {
                listener.Started(item.Test);
                listener.Failed(item.Test, exception, startTime, TimeSpan.Zero);
            }
        }

        private static void RunGrouped<TTest>(
            SpecRunItem<TTest> item,
            IContextualTestCase root,
            ISpecRunListener<TTest> listener,
            Exception? rootSetupFailure,
            SpecRunOptions options,
            CancellationToken cancellationToken)
        {
            listener.Started(item.Test);
            var startTime = DateTimeOffset.UtcNow;
            var timer = Stopwatch.StartNew();

            var report = GroupedSubtreeRunner.Run(root, item.ContextPath, rootSetupFailure, options, cancellationToken);
            timer.Stop();

            if (report == null)
                listener.Failed(item.Test, GroupedSubtreeRunner.MissingContext(item.ContextPath), startTime, timer.Elapsed);
            else
                listener.GroupedCompleted(item.Test, report, startTime, timer.Elapsed);
        }

        private static bool IsPending<TTest>(IContextualTestCase root, SpecRunItem<TTest> item)
        {
            var found = TestFinder.Find(root, item.ContextPath, item.TestDescription);
            return found != null && PendingSpecs.IsPending(found.Value.Context, item.TestDescription);
        }

        private static bool GroupedContextHasRunnableTests<TTest>(IContextualTestCase root, SpecRunItem<TTest> item)
        {
            var node = root.ChildContexts
                .FirstOrDefault(c => string.Equals(c.Description, item.ContextPath, StringComparison.Ordinal));
            return node != null && PendingSpecs.HasRunnableTests(node);
        }
    }
}
