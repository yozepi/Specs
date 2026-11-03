using System;
using System.Reflection;
using System.Xml.Linq;
using specs.BDD.Attributes;
using specs.BDD.VSTestAdapter.Execution;

namespace specs.BDD.VSTestAdapter.Discovery
{
    /// <summary>
    /// Resolves whether a root [Describe] method runs in grouped mode (one test result for the
    /// whole context subtree) from the layered configuration sources.
    /// </summary>
    /// <remarks>
    /// Precedence (highest wins):
    /// <list type="number">
    /// <item><see cref="DescribeAttribute.Structure"/> explicitly set on the [Describe] method</item>
    /// <item><see cref="TestStructureAttribute"/> on the specification class (not inherited)</item>
    /// <item><see cref="TestStructureAttribute"/> on the test assembly</item>
    /// <item><c>&lt;RunSettings&gt;&lt;SpecsBDD&gt;&lt;TestStructure&gt;</c> in .runsettings</item>
    /// <item>Default: <see cref="TestStructure.PerSpec"/> (one test result per It[...])</item>
    /// </list>
    /// </remarks>
    public static class GroupingSettings
    {
        /// <summary>
        /// Reads the <c>TestStructure</c> value from a .runsettings XML document.
        /// </summary>
        /// <param name="settingsXml">The raw runsettings XML, or <c>null</c> when unavailable.</param>
        /// <returns>
        /// The parsed value, or <c>null</c> when the XML is missing, malformed, or does not
        /// contain a valid <c>SpecsBDD/TestStructure</c> element.
        /// </returns>
        public static TestStructure? ReadRunSettings(string? settingsXml)
        {
            if (string.IsNullOrWhiteSpace(settingsXml))
                return null;

            try
            {
                var value = XDocument.Parse(settingsXml)
                    .Root?
                    .Element("SpecsBDD")?
                    .Element("TestStructure")?
                    .Value;

                // Enum.TryParse also accepts numeric strings; only names are valid here.
                if (string.IsNullOrWhiteSpace(value) || long.TryParse(value, out _))
                    return null;

                return Enum.TryParse<TestStructure>(value.Trim(), ignoreCase: true, out var parsed)
                    && Enum.IsDefined(typeof(TestStructure), parsed)
                    ? parsed
                    : (TestStructure?)null;
            }
            catch (System.Xml.XmlException)
            {
                return null;
            }
        }

        /// <summary>
        /// Reads <c>SpecsBDD/ShowSourceLocations</c> from .runsettings. Grouped reports include
        /// per-It source locations only when the value is <c>true</c>.
        /// </summary>
        public static bool ReadShowSourceLocations(string? settingsXml)
        {
            if (string.IsNullOrWhiteSpace(settingsXml))
                return false;

            try
            {
                var value = XDocument.Parse(settingsXml)
                    .Root?
                    .Element("SpecsBDD")?
                    .Element("ShowSourceLocations")?
                    .Value;

                return bool.TryParse(value?.Trim(), out var parsed) && parsed;
            }
            catch (System.Xml.XmlException)
            {
                return false;
            }
        }

        /// <summary>
        /// Reads <c>SpecsBDD/ColorOutput</c> from .runsettings. Grouped reports use ANSI-colored
        /// pass/fail markers unless the value is <c>false</c>.
        /// </summary>
        public static bool ReadColorOutput(string? settingsXml)
        {
            if (string.IsNullOrWhiteSpace(settingsXml))
                return true;

            try
            {
                var value = XDocument.Parse(settingsXml)
                    .Root?
                    .Element("SpecsBDD")?
                    .Element("ColorOutput")?
                    .Value;

                return !(bool.TryParse(value?.Trim(), out var parsed) && !parsed);
            }
            catch (System.Xml.XmlException)
            {
                return true;
            }
        }

        /// <summary>
        /// Picks grouped-report markers: plain when color is disabled, emoji when hosted by
        /// Visual Studio (<c>VSAPPIDNAME</c> is <c>devenv.exe</c>), otherwise ANSI.
        /// </summary>
        internal static MarkerStyle ResolveMarkerStyle(bool colorOutput, string? vsAppIdName)
        {
            if (!colorOutput)
                return MarkerStyle.Plain;

            return string.Equals(vsAppIdName, "devenv.exe", StringComparison.OrdinalIgnoreCase)
                ? MarkerStyle.Emoji
                : MarkerStyle.Ansi;
        }

        /// <summary>
        /// Resolves whether grouped execution applies from the four configuration layers.
        /// </summary>
        public static bool Resolve(TestStructure? contextSetting, TestStructure? classSetting, TestStructure? assemblySetting, TestStructure? runSettingsSetting)
            => (contextSetting ?? classSetting ?? assemblySetting ?? runSettingsSetting ?? TestStructure.PerSpec)
                == TestStructure.PerDescribe;

        /// <summary>
        /// Resolves whether grouped execution applies to a [Describe] method by reading its
        /// <see cref="DescribeAttribute"/> and the <see cref="TestStructureAttribute"/>
        /// on its reflected (concrete) specification class and assembly.
        /// </summary>
        public static bool Resolve(MethodInfo contextMethod, TestStructure? runSettingsSetting)
        {
            if (contextMethod == null) throw new ArgumentNullException(nameof(contextMethod));

            var contextSetting = contextMethod.GetCustomAttribute<DescribeAttribute>()?.StructureSetting;
            var classSetting = contextMethod.ReflectedType?
                .GetCustomAttribute<TestStructureAttribute>(inherit: false)?.Structure;
            var assemblySetting = contextMethod.DeclaringType?.Assembly
                .GetCustomAttribute<TestStructureAttribute>()?.Structure;

            return Resolve(contextSetting, classSetting, assemblySetting, runSettingsSetting);
        }
    }
}
