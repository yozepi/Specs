namespace specs.BDD.VSTestAdapter.Conversion
{
    /// <summary>
    /// Formatting shared by discovery and execution for <see cref="SpecTest.ContextPath"/> values.
    /// </summary>
    public static class ContextPathFormat
    {
        /// <summary>
        /// Delimiter between context descriptions in a context path; must not appear in a description.
        /// </summary>
        public static string Separator { get; set; } = " \u2192 ";
    }
}
