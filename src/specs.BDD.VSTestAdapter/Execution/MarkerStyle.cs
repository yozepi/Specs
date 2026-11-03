namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>How grouped reports render pass/fail markers.</summary>
    internal enum MarkerStyle
    {
        Plain,
        Ansi,
        // Visual Studio's Test Explorer does not render ANSI escape codes.
        Emoji
    }
}
