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
    /// Grouped items run their whole [Feature] subtree as one result; pending items are
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

            var tracker = new PassTracker<TTest>(listener);

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
                    RunGrouped(item, root, tracker, rootSetupFailure, options, cancellationToken);
                }

                if (regularItems.Count == 0) return;

                if (rootSetupFailure != null)
                {
                    FailAll(regularItems, listener, rootSetupFailure);
                    return;
                }

                new SelectionTreeWalker<TTest>(root, "", regularItems, tracker, cancellationToken).Walk();
            }
            finally
            {
                if (runRootLifecycle && rootSetupFailure == null)
                {
                    try { root.OwnTeardownOnce(); }
                    catch (Exception ex) { ReportTeardownFailure(tracker, listener, ex); }
                }
            }
        }

        private static void ReportTeardownFailure<TTest>(
            PassTracker<TTest> tracker,
            ISpecRunListener<TTest> listener,
            Exception exception)
        {
            // A later result for the same test supersedes its earlier pass; earlier failures are kept.
            if (tracker.LastPassed is { } last)
                listener.Failed(last.Test, exception, last.Start, last.Duration);

            listener.LogError("AfterAll failed: " + exception.Message);
        }

        private sealed class PassTracker<TTest> : ISpecRunListener<TTest>
        {
            private readonly ISpecRunListener<TTest> _inner;

            public PassTracker(ISpecRunListener<TTest> inner) => _inner = inner;

            public (TTest Test, DateTimeOffset Start, TimeSpan Duration)? LastPassed { get; private set; }

            public void Started(TTest test) => _inner.Started(test);

            public void Passed(TTest test, DateTimeOffset startTime, TimeSpan duration)
            {
                LastPassed = (test, startTime, duration);
                _inner.Passed(test, startTime, duration);
            }

            public void Failed(TTest test, Exception exception, DateTimeOffset startTime, TimeSpan duration) =>
                _inner.Failed(test, exception, startTime, duration);

            public void Pending(TTest test, DateTimeOffset startTime, TimeSpan duration) =>
                _inner.Pending(test, startTime, duration);

            public void GroupedCompleted(TTest test, GroupedReport report, DateTimeOffset startTime, TimeSpan duration)
            {
                if (report.Outcome == GroupedOutcome.Passed)
                    LastPassed = (test, startTime, duration);
                _inner.GroupedCompleted(test, report, startTime, duration);
            }

            public void LogInformation(string message) => _inner.LogInformation(message);

            public void LogError(string message) => _inner.LogError(message);
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
