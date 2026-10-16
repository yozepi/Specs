using System;
using System.Reflection;
using System.Xml.Linq;
using specs.BDD.Attributes;

namespace specs.BDD.VSTestAdapter.Discovery
{
    /// <summary>
    /// Resolves whether a root [Context] method runs in grouped mode (one test result for the
    /// whole context subtree) from the layered configuration sources.
    /// </summary>
    /// <remarks>
    /// Precedence (highest wins):
    /// <list type="number">
    /// <item><see cref="ContextAttribute.Grouped"/> explicitly set on the [Context] method</item>
    /// <item><see cref="GroupSpecsByRootContextAttribute"/> on the test assembly</item>
    /// <item><c>&lt;RunSettings&gt;&lt;SpecsBDD&gt;&lt;GroupByRootContext&gt;</c> in .runsettings</item>
    /// <item>Default: <c>false</c> (one test result per It[...])</item>
    /// </list>
    /// </remarks>
    public static class GroupingSettings
    {
        /// <summary>
        /// Reads the <c>GroupByRootContext</c> value from a .runsettings XML document.
        /// </summary>
        /// <param name="settingsXml">The raw runsettings XML, or <c>null</c> when unavailable.</param>
        /// <returns>
        /// The parsed value, or <c>null</c> when the XML is missing, malformed, or does not
        /// contain a valid <c>SpecsBDD/GroupByRootContext</c> element.
        /// </returns>
        public static bool? ReadRunSettings(string? settingsXml)
        {
            if (string.IsNullOrWhiteSpace(settingsXml))
                return null;

            try
            {
                var value = XDocument.Parse(settingsXml)
                    .Root?
                    .Element("SpecsBDD")?
                    .Element("GroupByRootContext")?
                    .Value;

                return bool.TryParse(value, out var parsed) ? parsed : (bool?)null;
            }
            catch (System.Xml.XmlException)
            {
                return null;
            }
        }

        /// <summary>
        /// Resolves the effective grouped setting from the three configuration layers.
        /// </summary>
        public static bool Resolve(bool? contextSetting, bool? assemblySetting, bool? runSettingsSetting)
            => contextSetting ?? assemblySetting ?? runSettingsSetting ?? false;

        /// <summary>
        /// Resolves the effective grouped setting for a [Context] method by reading its
        /// <see cref="ContextAttribute"/> and its assembly's
        /// <see cref="GroupSpecsByRootContextAttribute"/>.
        /// </summary>
        public static bool Resolve(MethodInfo contextMethod, bool? runSettingsSetting)
        {
            if (contextMethod == null) throw new ArgumentNullException(nameof(contextMethod));

            var contextSetting = contextMethod.GetCustomAttribute<ContextAttribute>()?.GroupedSetting;
            var assemblySetting = contextMethod.DeclaringType?.Assembly
                .GetCustomAttribute<GroupSpecsByRootContextAttribute>()?.Enabled;

            return Resolve(contextSetting, assemblySetting, runSettingsSetting);
        }
    }
}
