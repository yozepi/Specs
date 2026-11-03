namespace specs.BDD.Runner
{
    /// <summary>
    /// Formatting shared by discovery and execution for context path values.
    /// </summary>
    internal static class ContextPathFormat
    {
        /// <summary>
        /// Delimiter between context descriptions in a context path; must not appear in a description.
        /// </summary>
        public static string Separator { get; set; } = " \u2192 ";
    }
}
