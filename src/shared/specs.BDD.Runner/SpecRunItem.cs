namespace specs.BDD.Runner
{
    /// <summary>
    /// A selected test in runner-neutral form, paired with the runner's own handle for it.
    /// </summary>
    /// <typeparam name="TTest">The runner's handle for the test.</typeparam>
    internal sealed class SpecRunItem<TTest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecRunItem{TTest}"/> class.
        /// </summary>
        public SpecRunItem(TTest test, string contextPath, string testDescription, bool isGrouped)
        {
            Test = test;
            ContextPath = contextPath;
            TestDescription = testDescription;
            IsGrouped = isGrouped;
        }

        /// <summary>The runner's handle for the test.</summary>
        public TTest Test { get; }

        /// <summary>
        /// Context descriptions from the [Describe] down, joined with <see cref="ContextPathFormat.Separator"/>;
        /// for grouped items, the [Describe] description only.
        /// </summary>
        public string ContextPath { get; }

        /// <summary>The It[...] description; empty for grouped items.</summary>
        public string TestDescription { get; }

        /// <summary>Whether the item runs a whole [Describe] subtree as one result.</summary>
        public bool IsGrouped { get; }
    }
}
