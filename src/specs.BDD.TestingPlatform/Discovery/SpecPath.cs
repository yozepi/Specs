namespace specs.BDD.TestingPlatform.Discovery
{
    /// <summary>
    /// Builds the '/'-separated tree paths used as test node UIDs and matched by <c>--treenode-filter</c>.
    /// </summary>
    internal static class SpecPath
    {
        /// <summary>The path segment separator.</summary>
        public const string Separator = "/";

        /// <summary>
        /// Percent-encodes only '%' and '/', so a segment can't split the path while filters
        /// keep matching names with spaces and punctuation as written.
        /// </summary>
        public static string Escape(string segment) =>
            segment.Replace("%", "%25").Replace("/", "%2F");

        /// <summary>Appends an escaped segment to <paramref name="parentPath"/>.</summary>
        public static string Combine(string parentPath, string segment) =>
            parentPath + Separator + Escape(segment);
    }
}
