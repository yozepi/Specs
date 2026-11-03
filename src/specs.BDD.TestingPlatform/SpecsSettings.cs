using System;
using System.Linq;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Configurations;
using specs.BDD.Attributes;
using specs.BDD.Runner;

namespace specs.BDD.TestingPlatform
{
    /// <summary>
    /// Runner settings read from the platform configuration
    /// (<c>[appname].testconfig.json</c>, environment variables such as <c>SpecsBDD__TestStructure</c>).
    /// </summary>
    internal sealed class SpecsSettings
    {
        /// <summary>Configuration key for the runner-level <see cref="TestStructure"/> default.</summary>
        public const string TestStructureKey = "SpecsBDD:TestStructure";

        /// <summary>Configuration key that adds per-spec source locations to grouped reports.</summary>
        public const string ShowSourceLocationsKey = "SpecsBDD:ShowSourceLocations";

        /// <summary>Configuration key that disables colored pass/fail markers when <c>false</c>.</summary>
        public const string ColorOutputKey = "SpecsBDD:ColorOutput";

        private SpecsSettings(TestStructure? testStructure, bool showSourceLocations, bool colorOutput)
        {
            TestStructure = testStructure;
            ShowSourceLocations = showSourceLocations;
            ColorOutput = colorOutput;
        }

        /// <summary>The runner-level <see cref="TestStructure"/> default; <c>null</c> when not configured.</summary>
        public TestStructure? TestStructure { get; }

        /// <summary>Whether grouped reports include an <c>at file:line</c> line under each spec.</summary>
        public bool ShowSourceLocations { get; }

        /// <summary>Whether markers may use color.</summary>
        public bool ColorOutput { get; }

        /// <summary>Reads the settings from <paramref name="configuration"/>.</summary>
        public static SpecsSettings From(IConfiguration configuration)
        {
            var showSourceLocations = bool.TryParse(configuration[ShowSourceLocationsKey]?.Trim(), out var show) && show;
            var colorOutput = !(bool.TryParse(configuration[ColorOutputKey]?.Trim(), out var color) && !color);

            return new SpecsSettings(
                GroupingSettings.ParseTestStructure(configuration[TestStructureKey]),
                showSourceLocations,
                colorOutput);
        }
    }

    /// <summary>
    /// Identifies whether the test application is talking to an IDE test explorer.
    /// </summary>
    internal static class ClientMode
    {
        /// <summary>
        /// IDEs connect in JSON-RPC server mode; <c>dotnet test</c> uses server mode with the
        /// <c>dotnettestcli</c> protocol and, like a console run, lists tests without hierarchy.
        /// </summary>
        public static bool IsIde(ICommandLineOptions options)
        {
            if (!options.TryGetOptionArgumentList("server", out var arguments))
                return false;

            return arguments == null
                || arguments.Length == 0
                || arguments.Any(a => string.Equals(a, "jsonrpc", StringComparison.OrdinalIgnoreCase));
        }
    }
}
