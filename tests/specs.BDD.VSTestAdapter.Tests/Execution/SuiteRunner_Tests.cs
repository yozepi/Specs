using NFluent;
using NUnit.Framework;
using specs.BDD.Runner;
using specs.BDD.VSTestAdapter.Conversion;
using specs.BDD.VSTestAdapter.Discovery;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace specs.BDD.VSTestAdapter.Tests.Execution
{
    /// <summary>
    /// Tests for <see cref="SuiteRunner"/> root AfterAll failure reporting.
    /// </summary>
    [TestFixture]
    public class SuiteRunner_Tests
    {
        private sealed class RecordingListener : ISpecRunListener<string>
        {
            public List<(string Test, string Outcome, Exception? Error)> Results { get; } = new();
            public List<string> Errors { get; } = new();

            public void Started(string test) { }
            public void Passed(string test, DateTimeOffset startTime, TimeSpan duration) => Results.Add((test, "passed", null));
            public void Failed(string test, Exception exception, DateTimeOffset startTime, TimeSpan duration) => Results.Add((test, "failed", exception));
            public void Pending(string test, DateTimeOffset startTime, TimeSpan duration) => Results.Add((test, "pending", null));
            public void GroupedCompleted(string test, GroupedReport report, DateTimeOffset startTime, TimeSpan duration) =>
                Results.Add((test, report.Outcome.ToString().ToLowerInvariant(), null));
            public void LogInformation(string message) { }
            public void LogError(string message) => Errors.Add(message);
        }

        private static RecordingListener Run<TSpec>(bool failAfterAll, bool failFirstSpec = false)
            where TSpec : specs.BDD.Specifications
        {
            SpecWithFailingRootAfterAll.TriggerFailure = failAfterAll;
            SpecWithFailingRootAfterAll.FailFirstSpec = failFirstSpec;
            try
            {
                var items = SuiteTreeFlattener.Flatten(typeof(TSpec))
                    .Where(t => !t.IsGrouped)
                    .Select(t => new SpecRunItem<string>(t.TestDescription, t.ContextPath, t.TestDescription, false))
                    .ToList();

                var listener = new RecordingListener();
                SuiteRunner.Run(
                    SuiteTreeBuilder.BuildSuite(typeof(TSpec)),
                    items,
                    listener,
                    new SpecRunOptions(null, MarkerStyle.Plain),
                    CancellationToken.None);
                return listener;
            }
            finally
            {
                SpecWithFailingRootAfterAll.TriggerFailure = false;
                SpecWithFailingRootAfterAll.FailFirstSpec = false;
            }
        }

        [Test]
        public void AfterAllSucceeds_AllSpecsPass()
        {
            var listener = Run<SpecWithFailingRootAfterAll>(failAfterAll: false);

            Check.That(listener.Results.Select(r => r.Outcome)).ContainsExactly("passed", "passed");
            Check.That(listener.Errors).IsEmpty();
        }

        [Test]
        public void AfterAllThrows_LastPassingSpecIsReportedFailed()
        {
            var listener = Run<SpecWithFailingRootAfterAll>(failAfterAll: true);

            var last = listener.Results.Last();
            Check.That(last.Outcome).IsEqualTo("failed");
            Check.That(last.Error).IsInstanceOf<InvalidOperationException>();
            Check.That(last.Error!.Message).Contains("root cleanup failure");
            Check.That(listener.Errors.Single()).Contains("AfterAll failed");
        }

        [Test]
        public void AfterAllThrows_ExistingFailureIsPreserved()
        {
            var listener = Run<SpecWithFailingRootAfterAll>(failAfterAll: true, failFirstSpec: true);

            var first = listener.Results.First(r => r.Test == "first");
            Check.That(first.Outcome).IsEqualTo("failed");
            Check.That(first.Error!.Message).Contains("spec failure");
            Check.That(listener.Results.Last().Outcome).IsEqualTo("failed");
        }
    }
}
