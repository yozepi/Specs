namespace specs.BDD.Attributes
{
    /// <summary>
    /// How specs map to test cases in the test runner: one per It[...] or one per [Describe] method.
    /// </summary>
    public enum TestStructure
    {
        /// <summary>Each It[...] is a separate test case that can be run and reported on its own.</summary>
        PerSpec,

        /// <summary>
        /// Each [Describe] method is one test case; its specs run together and the full context tree is written to the result's output.
        /// </summary>
        PerDescribe
    }
}
