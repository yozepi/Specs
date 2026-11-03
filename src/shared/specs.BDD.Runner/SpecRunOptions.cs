namespace specs.BDD.Runner
{
    /// <summary>
    /// Settings that shape grouped reports.
    /// </summary>
    internal sealed class SpecRunOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecRunOptions"/> class.
        /// </summary>
        public SpecRunOptions(string? sourceForLocations, MarkerStyle markerStyle)
        {
            SourceForLocations = sourceForLocations;
            MarkerStyle = markerStyle;
        }

        /// <summary>Assembly path used for PDB lookups; <c>null</c> when source locations are disabled.</summary>
        public string? SourceForLocations { get; }

        /// <summary>How pass/fail/pending markers render.</summary>
        public MarkerStyle MarkerStyle { get; }
    }
}
