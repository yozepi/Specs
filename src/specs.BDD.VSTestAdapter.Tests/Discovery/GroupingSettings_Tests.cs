using NFluent;
using NUnit.Framework;
using specs.BDD.Attributes;
using specs.BDD.VSTestAdapter.Discovery;
using specs.BDD.VSTestAdapter.Execution;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using System.Reflection;

namespace specs.BDD.VSTestAdapter.Tests.Discovery
{
    [TestFixture]
    public class GroupingSettings_Tests
    {
        // ── ReadRunSettings ───────────────────────────────────────────────────────────────────

        private const SpecReporting Grouped = SpecReporting.GroupedByRootContext;
        private const SpecReporting Individual = SpecReporting.Individual;

        private static string RunSettingsXml(string value) =>
            $"<RunSettings><SpecsBDD><SpecReporting>{value}</SpecReporting></SpecsBDD></RunSettings>";

        [Test]
        public void ReadRunSettings_GroupedByRootContext_ReturnsGrouped()
        {
            Check.That(GroupingSettings.ReadRunSettings(RunSettingsXml("GroupedByRootContext"))).IsEqualTo(Grouped);
        }

        [Test]
        public void ReadRunSettings_Individual_ReturnsIndividual()
        {
            Check.That(GroupingSettings.ReadRunSettings(RunSettingsXml("Individual"))).IsEqualTo(Individual);
        }

        [Test]
        public void ReadRunSettings_IsCaseInsensitive()
        {
            Check.That(GroupingSettings.ReadRunSettings(RunSettingsXml("groupedbyrootcontext"))).IsEqualTo(Grouped);
        }

        [Test]
        public void ReadRunSettings_NumericValue_ReturnsNull()
        {
            Check.That(GroupingSettings.ReadRunSettings(RunSettingsXml("1"))).IsNull();
        }

        [Test]
        public void ReadRunSettings_MissingElement_ReturnsNull()
        {
            var xml = "<RunSettings><SpecsBDD /></RunSettings>";
            Check.That(GroupingSettings.ReadRunSettings(xml)).IsNull();
        }

        [Test]
        public void ReadRunSettings_MissingSection_ReturnsNull()
        {
            var xml = "<RunSettings></RunSettings>";
            Check.That(GroupingSettings.ReadRunSettings(xml)).IsNull();
        }

        [Test]
        public void ReadRunSettings_NullXml_ReturnsNull()
        {
            Check.That(GroupingSettings.ReadRunSettings(null)).IsNull();
        }

        [Test]
        public void ReadRunSettings_MalformedXml_ReturnsNull()
        {
            Check.That(GroupingSettings.ReadRunSettings("<not-xml")).IsNull();
        }

        [Test]
        public void ReadRunSettings_UndefinedValue_ReturnsNull()
        {
            Check.That(GroupingSettings.ReadRunSettings(RunSettingsXml("true"))).IsNull();
        }
        // ── ReadShowSourceLocations ────────────────────────────────────────────────────────────────────

        private static string ShowSourceLocationsXml(string value) =>
            $"<RunSettings><SpecsBDD><ShowSourceLocations>{value}</ShowSourceLocations></SpecsBDD></RunSettings>";

        [TestCase("true")]
        [TestCase("True")]
        [TestCase(" TRUE ")]
        public void ReadShowSourceLocations_True_ReturnsTrue(string value)
        {
            Check.That(GroupingSettings.ReadShowSourceLocations(ShowSourceLocationsXml(value))).IsTrue();
        }

        [TestCase("false")]
        [TestCase("yes")]
        [TestCase("1")]
        [TestCase("")]
        public void ReadShowSourceLocations_NonTrueValue_ReturnsFalse(string value)
        {
            Check.That(GroupingSettings.ReadShowSourceLocations(ShowSourceLocationsXml(value))).IsFalse();
        }

        [TestCase(null)]
        [TestCase("<RunSettings></RunSettings>")]
        [TestCase("<RunSettings><SpecsBDD /></RunSettings>")]
        [TestCase("<not-xml")]
        public void ReadShowSourceLocations_MissingOrMalformed_ReturnsFalse(string? xml)
        {
            Check.That(GroupingSettings.ReadShowSourceLocations(xml)).IsFalse();
        }

        // ── ReadColorOutput ──

        private static string ColorOutputXml(string value) =>
            $"<RunSettings><SpecsBDD><ColorOutput>{value}</ColorOutput></SpecsBDD></RunSettings>";

        [TestCase("false")]
        [TestCase("False")]
        [TestCase(" FALSE ")]
        public void ReadColorOutput_False_ReturnsFalse(string value)
        {
            Check.That(GroupingSettings.ReadColorOutput(ColorOutputXml(value))).IsFalse();
        }

        [TestCase("true")]
        [TestCase("no")]
        [TestCase("0")]
        [TestCase("")]
        public void ReadColorOutput_NonFalseValue_ReturnsTrue(string value)
        {
            Check.That(GroupingSettings.ReadColorOutput(ColorOutputXml(value))).IsTrue();
        }

        [TestCase(null)]
        [TestCase("<RunSettings></RunSettings>")]
        [TestCase("<RunSettings><SpecsBDD /></RunSettings>")]
        [TestCase("<not-xml")]
        public void ReadColorOutput_MissingOrMalformed_ReturnsTrue(string? xml)
        {
            Check.That(GroupingSettings.ReadColorOutput(xml)).IsTrue();
        }

        // ── ResolveMarkerStyle ──

        [TestCase(null)]
        [TestCase("devenv.exe")]
        public void ResolveMarkerStyle_ColorDisabled_ReturnsPlain(string? vsAppIdName)
        {
            Check.That(GroupingSettings.ResolveMarkerStyle(false, vsAppIdName)).IsEqualTo(MarkerStyle.Plain);
        }

        [TestCase("devenv.exe")]
        [TestCase("DEVENV.EXE")]
        public void ResolveMarkerStyle_VisualStudioHost_ReturnsEmoji(string vsAppIdName)
        {
            Check.That(GroupingSettings.ResolveMarkerStyle(true, vsAppIdName)).IsEqualTo(MarkerStyle.Emoji);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("code.exe")]
        public void ResolveMarkerStyle_OtherHost_ReturnsAnsi(string? vsAppIdName)
        {
            Check.That(GroupingSettings.ResolveMarkerStyle(true, vsAppIdName)).IsEqualTo(MarkerStyle.Ansi);
        }
        // ── Resolve precedence ────────────────────────────────────────────────────────────────

        [Test]
        public void Resolve_ContextSetting_OverridesClassAssemblyAndRunSettings()
        {
            Check.That(GroupingSettings.Resolve(contextSetting: Individual, classSetting: Grouped, assemblySetting: Grouped, runSettingsSetting: Grouped))
                .IsFalse();
        }

        [Test]
        public void Resolve_ClassSetting_OverridesAssemblyAndRunSettings()
        {
            Check.That(GroupingSettings.Resolve(contextSetting: null, classSetting: Grouped, assemblySetting: Individual, runSettingsSetting: Individual))
                .IsTrue();
        }

        [Test]
        public void Resolve_AssemblySetting_OverridesRunSettings()
        {
            Check.That(GroupingSettings.Resolve(contextSetting: null, classSetting: null, assemblySetting: Individual, runSettingsSetting: Grouped))
                .IsFalse();
        }

        [Test]
        public void Resolve_RunSettings_UsedWhenNoAttributes()
        {
            Check.That(GroupingSettings.Resolve(contextSetting: null, classSetting: null, assemblySetting: null, runSettingsSetting: Grouped))
                .IsTrue();
        }

        [Test]
        public void Resolve_DefaultsToFalse()
        {
            Check.That(GroupingSettings.Resolve(contextSetting: null, classSetting: null, assemblySetting: null, runSettingsSetting: null))
                .IsFalse();
        }

        // ── Resolve from MethodInfo ───────────────────────────────────────────────────────────

        private static MethodInfo GetContextMethod<TSpec>(string name) =>
            typeof(TSpec).GetMethod(name, BindingFlags.Public | BindingFlags.Instance)!;

        [Test]
        public void Resolve_Method_WithGroupedTrue_ReturnsTrue()
        {
            var method = GetContextMethod<GroupedSpec>("describe_grouped");
            Check.That(GroupingSettings.Resolve(method, runSettingsSetting: null)).IsTrue();
        }

        [Test]
        public void Resolve_Method_WithoutGroupedSetting_FallsBackToRunSettings()
        {
            // This test assembly has no assembly-level SpecReportingAttribute, so runsettings applies.
            var method = GetContextMethod<SimpleSpec>("describe_something");
            Check.That(GroupingSettings.Resolve(method, runSettingsSetting: Grouped)).IsTrue();
            Check.That(GroupingSettings.Resolve(method, runSettingsSetting: null)).IsFalse();
        }

        [Test]
        public void Resolve_Method_GroupedTrue_IgnoresRunSettingsIndividual()
        {
            var method = GetContextMethod<GroupedSpec>("describe_grouped");
            Check.That(GroupingSettings.Resolve(method, runSettingsSetting: Individual)).IsTrue();
        }

        [Test]
        public void Resolve_Method_ClassAttribute_ReturnsTrue()
        {
            var method = GetContextMethod<ClassGroupedSpec>("describe_class_grouped");
            Check.That(GroupingSettings.Resolve(method, runSettingsSetting: Individual)).IsTrue();
        }

        [Test]
        public void Resolve_Method_ContextIndividual_OverridesClassAttribute()
        {
            var method = GetContextMethod<ClassGroupedSpec>("describe_opt_out");
            Check.That(GroupingSettings.Resolve(method, runSettingsSetting: null)).IsFalse();
        }

        [Test]
        public void Resolve_Method_ClassAttribute_NotInheritedByDerivedClass()
        {
            var method = GetContextMethod<DerivedFromClassGroupedSpec>("describe_class_grouped");
            Check.That(GroupingSettings.Resolve(method, runSettingsSetting: null)).IsFalse();
        }
    }
}
