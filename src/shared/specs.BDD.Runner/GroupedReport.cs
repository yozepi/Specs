namespace specs.BDD.Runner
{
    /// <summary>Aggregated outcome of a grouped [Feature].</summary>
    internal enum GroupedOutcome
    {
        Passed,
        Failed,
        Skipped
    }

    /// <summary>
    /// The result of running a [Feature] subtree as one unit: an NSpec-style tree report
    /// plus the aggregated outcome.
    /// </summary>
    internal sealed class GroupedReport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupedReport"/> class.
        /// </summary>
        public GroupedReport(GroupedOutcome outcome, string output, string? errorMessage, string? errorStackTrace)
        {
            Outcome = outcome;
            Output = output;
            ErrorMessage = errorMessage;
            ErrorStackTrace = errorStackTrace;
        }

        /// <summary>The aggregated outcome.</summary>
        public GroupedOutcome Outcome { get; }

        /// <summary>The tree report, summary line, and failure details.</summary>
        public string Output { get; }

        /// <summary>Failure summary or skip reason; <c>null</c> when passed.</summary>
        public string? ErrorMessage { get; }

        /// <summary>Combined stack traces of the failed specs; <c>null</c> unless failed.</summary>
        public string? ErrorStackTrace { get; }
    }
}
