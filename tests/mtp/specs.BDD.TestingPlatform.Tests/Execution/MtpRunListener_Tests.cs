using System.Reflection;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.Messages;
using Microsoft.Testing.Platform.TestHost;
using Moq;
using NFluent;
using NUnit.Framework;
using specs.BDD.Runner;
using specs.BDD.TestingPlatform.Discovery;
using specs.BDD.TestingPlatform.Execution;
using specs.BDD.TestingPlatform.Tests.TestFixtures;
using static specs.BDD.TestingPlatform.Tests.Discovery.SpecTreeBuilder_Tests;

namespace specs.BDD.TestingPlatform.Tests.Execution
{
    [TestFixture]
    public class MtpRunListener_Tests
    {
        private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(5);

        private List<TestNodeUpdateMessage> _published = null!;
        private MtpRunListener _listener = null!;
        private SpecNode _leaf = null!;

        [SetUp]
        public void SetUp()
        {
            _published = new List<TestNodeUpdateMessage>();
            var messageBus = new Mock<IMessageBus>();
            messageBus
                .Setup(b => b.PublishAsync(It.IsAny<IDataProducer>(), It.IsAny<IData>()))
                .Callback<IDataProducer, IData>((_, data) => _published.Add((TestNodeUpdateMessage)data))
                .Returns(Task.CompletedTask);

            var factory = new TestNodeFactory(new SessionUid("session"), isIdeClient: false);
            _listener = new MtpRunListener(messageBus.Object, new Mock<IDataProducer>().Object, factory, new Mock<ILogger>().Object);

            var forest = SpecTreeBuilder.Build(typeof(TreeSpec).Assembly, configuredStructure: null);
            _leaf = forest.Leaves.Single(l => l.Path == $"{Ns}/TreeSpec/Cart/adds an item");
        }

        private TestNodeStateProperty PublishedState => _published.Single().TestNode.Properties.Single<TestNodeStateProperty>();

        [Test]
        public void Started_PublishesInProgress()
        {
            _listener.Started(_leaf);

            Check.That(PublishedState).IsInstanceOf<InProgressTestNodeStateProperty>();
        }

        [Test]
        public void Passed_PublishesPassedWithTiming()
        {
            _listener.Passed(_leaf, Start, Duration);

            var properties = _published.Single().TestNode.Properties;
            Check.That(PublishedState).IsInstanceOf<PassedTestNodeStateProperty>();
            Check.That(properties.Single<TimingProperty>().GlobalTiming.Duration).IsEqualTo(Duration);
            Check.That(_listener.Outcomes[_leaf]).IsEqualTo(SpecOutcome.Passed);
        }

        [Test]
        public void Failed_PublishesTheUnwrappedCause()
        {
            _listener.Failed(_leaf, new TargetInvocationException(new InvalidOperationException("boom")), Start, Duration);

            var state = (FailedTestNodeStateProperty)PublishedState;
            Check.That(state.Exception).IsInstanceOf<InvalidOperationException>();
            Check.That(state.Explanation).IsEqualTo("boom");
            Check.That(_listener.Outcomes[_leaf]).IsEqualTo(SpecOutcome.Failed);
        }

        [Test]
        public void Pending_PublishesSkipped()
        {
            _listener.Pending(_leaf, Start, TimeSpan.Zero);

            Check.That(PublishedState).IsInstanceOf<SkippedTestNodeStateProperty>();
            Check.That(PublishedState.Explanation).IsEqualTo(MtpRunListener.PendingReason);
            Check.That(_listener.Outcomes[_leaf]).IsEqualTo(SpecOutcome.Pending);
        }

        [Test]
        public void GroupedCompleted_Failed_PublishesSummaryAndReport()
        {
            var report = new GroupedReport(GroupedOutcome.Failed, "tree report", "1 of 2 specs failed", "at Somewhere()");

            _listener.GroupedCompleted(_leaf, report, Start, Duration);

            var state = (FailedTestNodeStateProperty)PublishedState;
            Check.That(state.Explanation).IsEqualTo("1 of 2 specs failed");
            Check.That(state.Exception!.StackTrace).IsEqualTo("at Somewhere()");
            Check.That(_published.Single().TestNode.Properties.Single<StandardOutputProperty>().StandardOutput).IsEqualTo("tree report");
        }

        [Test]
        public void GroupedCompleted_Passed_PublishesPassedWithReport()
        {
            _listener.GroupedCompleted(_leaf, new GroupedReport(GroupedOutcome.Passed, "tree report", null, null), Start, Duration);

            Check.That(PublishedState).IsInstanceOf<PassedTestNodeStateProperty>();
            Check.That(_published.Single().TestNode.Properties.Single<StandardOutputProperty>().StandardOutput).IsEqualTo("tree report");
        }

        [Test]
        public void GroupedCompleted_Skipped_PublishesSkippedWithReason()
        {
            _listener.GroupedCompleted(_leaf, new GroupedReport(GroupedOutcome.Skipped, "tree report", "All specs are pending.", null), Start, Duration);

            Check.That(PublishedState).IsInstanceOf<SkippedTestNodeStateProperty>();
            Check.That(PublishedState.Explanation).IsEqualTo("All specs are pending.");
            Check.That(_listener.Outcomes[_leaf]).IsEqualTo(SpecOutcome.Pending);
        }
    }
}
