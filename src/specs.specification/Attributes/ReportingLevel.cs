namespace specs.BDD.Attributes
{
    /// <summary>
    /// How specs map to test cases in the test runner: one per It[...] or one per [Feature] method.
    /// </summary>
    public enum ReportingLevel
    {
        /// <summary>Each It[...] is a separate test case that can be run and reported on its own.</summary>
        Test,

        /// <summary>
        /// Each [Feature] method is one test case; its specs run together and the full context tree is written to the result's output.
        /// </summary>
        Feature
    }
}
