using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Discovery;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using System.Reflection;

namespace specs.BDD.VSTestAdapter.Tests.Discovery
{
    [TestFixture]
    public class GroupingSettings_Tests
    {
        // ── ReadRunSettings ───────────────────────────────────────────────────────────────────

        [Test]
        public void ReadRunSettings_True_ReturnsTrue()
        {
            var xml = "<RunSettings><SpecsBDD><GroupByRootContext>true</GroupByRootContext></SpecsBDD></RunSettings>";
            Check.That(GroupingSettings.ReadRunSettings(xml)).IsEqualTo(true);
        }

        [Test]
        public void ReadRunSettings_False_ReturnsFalse()
        {
            var xml = "<RunSettings><SpecsBDD><GroupByRootContext>false</GroupByRootContext></SpecsBDD></RunSettings>";
            Check.That(GroupingSettings.ReadRunSettings(xml)).IsEqualTo(false);
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
        public void ReadRunSettings_NonBooleanValue_ReturnsNull()
        {
            var xml = "<RunSettings><SpecsBDD><GroupByRootContext>yes</GroupByRootContext></SpecsBDD></RunSettings>";
            Check.That(GroupingSettings.ReadRunSettings(xml)).IsNull();
        }

        // ── Resolve precedence ────────────────────────────────────────────────────────────────

        [Test]
        public void Resolve_ContextSetting_OverridesAssemblyAndRunSettings()
        {
            Check.That(GroupingSettings.Resolve(contextSetting: false, assemblySetting: true, runSettingsSetting: true))
                .IsFalse();
        }

        [Test]
        public void Resolve_AssemblySetting_OverridesRunSettings()
        {
            Check.That(GroupingSettings.Resolve(contextSetting: null, assemblySetting: false, runSettingsSetting: true))
                .IsFalse();
        }

        [Test]
        public void Resolve_RunSettings_UsedWhenNoAttributes()
        {
            Check.That(GroupingSettings.Resolve(contextSetting: null, assemblySetting: null, runSettingsSetting: true))
                .IsTrue();
        }

        [Test]
        public void Resolve_DefaultsToFalse()
        {
            Check.That(GroupingSettings.Resolve(contextSetting: null, assemblySetting: null, runSettingsSetting: null))
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
            // This test assembly has no GroupSpecsByRootContextAttribute, so runsettings applies.
            var method = GetContextMethod<SimpleSpec>("describe_something");
            Check.That(GroupingSettings.Resolve(method, runSettingsSetting: true)).IsTrue();
            Check.That(GroupingSettings.Resolve(method, runSettingsSetting: null)).IsFalse();
        }

        [Test]
        public void Resolve_Method_GroupedTrue_IgnoresRunSettingsFalse()
        {
            var method = GetContextMethod<GroupedSpec>("describe_grouped");
            Check.That(GroupingSettings.Resolve(method, runSettingsSetting: false)).IsTrue();
        }
    }
}
