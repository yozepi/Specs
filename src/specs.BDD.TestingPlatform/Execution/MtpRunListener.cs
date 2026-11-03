using System;
using System.Collections.Generic;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.Messages;
using specs.BDD.Runner;
using specs.BDD.TestingPlatform.Discovery;

namespace specs.BDD.TestingPlatform.Execution
{
    /// <summary>The recorded outcome of a spec leaf in a run.</summary>
    internal enum SpecOutcome
    {
        Passed,
        Failed,
        Pending
    }

    /// <summary>
    /// Publishes shared-runner events to the platform message bus as test node updates.
    /// </summary>
    internal sealed class MtpRunListener : ISpecRunListener<SpecNode>
    {
        private readonly IMessageBus _messageBus;
        private readonly IDataProducer _producer;
        private readonly TestNodeFactory _factory;
        private readonly ILogger _logger;
        private readonly Dictionary<SpecNode, SpecOutcome> _outcomes = new Dictionary<SpecNode, SpecOutcome>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MtpRunListener"/> class.
        /// </summary>
        public MtpRunListener(IMessageBus messageBus, IDataProducer producer, TestNodeFactory factory, ILogger logger)
        {
            _messageBus = messageBus;
            _producer = producer;
            _factory = factory;
            _logger = logger;
        }

        /// <summary>The outcome of every leaf reported so far.</summary>
        public IReadOnlyDictionary<SpecNode, SpecOutcome> Outcomes => _outcomes;

        /// <inheritdoc/>
        public void Started(SpecNode test) =>
            Publish(_factory.Leaf(test, InProgressTestNodeStateProperty.CachedInstance));

        /// <inheritdoc/>
        public void Passed(SpecNode test, DateTimeOffset startTime, TimeSpan duration)
        {
            _outcomes[test] = SpecOutcome.Passed;
            Publish(_factory.Leaf(test, PassedTestNodeStateProperty.CachedInstance, Timing(startTime, duration)));
        }

        /// <inheritdoc/>
        public void Failed(SpecNode test, Exception exception, DateTimeOffset startTime, TimeSpan duration)
        {
            _outcomes[test] = SpecOutcome.Failed;
            var cause = SpecExceptions.Unwrap(exception);
            Publish(_factory.Leaf(test, new FailedTestNodeStateProperty(cause, cause.Message), Timing(startTime, duration)));
        }

        /// <inheritdoc/>
        public void Pending(SpecNode test, DateTimeOffset startTime, TimeSpan duration)
        {
            _outcomes[test] = SpecOutcome.Pending;
            Publish(_factory.Leaf(test, new SkippedTestNodeStateProperty(PendingReason), Timing(startTime, duration)));
        }

        /// <inheritdoc/>
        public void GroupedCompleted(SpecNode test, GroupedReport report, DateTimeOffset startTime, TimeSpan duration)
        {
            TestNodeStateProperty state;
            switch (report.Outcome)
            {
                case GroupedOutcome.Passed:
                    _outcomes[test] = SpecOutcome.Passed;
                    state = PassedTestNodeStateProperty.CachedInstance;
                    break;
                case GroupedOutcome.Failed:
                    _outcomes[test] = SpecOutcome.Failed;
                    var message = report.ErrorMessage ?? "Grouped specs failed.";
                    state = new FailedTestNodeStateProperty(new GroupedSpecsFailedException(message, report.ErrorStackTrace), message);
                    break;
                default:
                    _outcomes[test] = SpecOutcome.Pending;
                    state = new SkippedTestNodeStateProperty(report.ErrorMessage ?? PendingReason);
                    break;
            }

            Publish(_factory.Leaf(test, state, Timing(startTime, duration), new StandardOutputProperty(report.Output)));
        }

        /// <inheritdoc/>
        public void LogInformation(string message) => _logger.LogInformation(message);

        /// <inheritdoc/>
        public void LogError(string message) => _logger.LogError(message);

        /// <summary>The skip reason reported for pending specs.</summary>
        public const string PendingReason = "Pending";

        private static TimingProperty Timing(DateTimeOffset startTime, TimeSpan duration) =>
            new TimingProperty(new TimingInfo(startTime, startTime + duration, duration));

        // The shared runner is synchronous; it runs on a worker thread, so blocking here is safe.
        private void Publish(TestNodeUpdateMessage message) =>
            _messageBus.PublishAsync(_producer, message).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Carries a grouped [Describe]'s failure summary and combined stack traces to the platform.
    /// </summary>
    internal sealed class GroupedSpecsFailedException : Exception
    {
        private readonly string? _stackTrace;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupedSpecsFailedException"/> class.
        /// </summary>
        public GroupedSpecsFailedException(string message, string? stackTrace) : base(message)
        {
            _stackTrace = stackTrace;
        }

        /// <inheritdoc/>
        public override string? StackTrace => _stackTrace;
    }
}
