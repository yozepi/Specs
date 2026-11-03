namespace specs.BDD.Runner
{
    /// <summary>
    /// Pass/fail/pending markers for tree reports.
    /// </summary>
    internal static class ReportMarkers
    {
        /// <summary>Returns the markers for <paramref name="style"/>.</summary>
        public static (string Pass, string Fail, string Pending) For(MarkerStyle style) => style switch
        {
            MarkerStyle.Ansi => ("\u001b[32m✓\u001b[0m", "\u001b[31m✗\u001b[0m", "\u001b[33m○\u001b[0m"),
            MarkerStyle.Emoji => ("✅", "❌", "⏸️"),
            _ => ("✓", "✗", "○")
        };

        /// <summary>Two spaces per tree level.</summary>
        public static string Indent(int depth) => new string(' ', depth * 2);
    }
}
