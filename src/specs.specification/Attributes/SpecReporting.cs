namespace specs.BDD.Attributes
{
    /// <summary>
    /// How spec results are reported to the test runner.
    /// </summary>
    public enum SpecReporting
    {
        /// <summary>One test result per It[...].</summary>
        Individual,

        /// <summary>One test result per root [Context], with the full contextual output published to the result's output.</summary>
        GroupedByRootContext
    }
}
