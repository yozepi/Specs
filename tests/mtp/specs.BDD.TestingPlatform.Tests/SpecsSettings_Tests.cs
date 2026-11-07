using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Configurations;
using Moq;
using NFluent;
using NUnit.Framework;
using specs.BDD.Attributes;

namespace specs.BDD.TestingPlatform.Tests
{
    [TestFixture]
    public class SpecsSettings_Tests
    {
        private static SpecsSettings Read(string key, string? value)
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c[key]).Returns(value);
            return SpecsSettings.From(configuration.Object);
        }

        [TestCase("Feature", ReportingLevel.Feature)]
        [TestCase("Test", ReportingLevel.Test)]
        public void ReportingLevel_ValidName_IsParsed(string value, ReportingLevel expected)
        {
            Check.That(Read(SpecsSettings.ReportingLevelKey, value).ReportingLevel).IsEqualTo(expected);
        }

        [TestCase(null)]
        [TestCase("1")]
        [TestCase("bogus")]
        public void ReportingLevel_MissingOrInvalid_IsNull(string? value)
        {
            Check.That(Read(SpecsSettings.ReportingLevelKey, value).ReportingLevel).IsNull();
        }

        [Test]
        public void ShowSourceLocations_DefaultsToFalse()
        {
            Check.That(Read(SpecsSettings.ShowSourceLocationsKey, null).ShowSourceLocations).IsFalse();
            Check.That(Read(SpecsSettings.ShowSourceLocationsKey, "true").ShowSourceLocations).IsTrue();
        }

        [Test]
        public void ColorOutput_DefaultsToTrue()
        {
            Check.That(Read(SpecsSettings.ColorOutputKey, null).ColorOutput).IsTrue();
            Check.That(Read(SpecsSettings.ColorOutputKey, "false").ColorOutput).IsFalse();
        }
    }

    [TestFixture]
    public class ClientMode_Tests
    {
        private static bool IsIde(string[]? serverArguments)
        {
            var options = new Mock<ICommandLineOptions>();
            var arguments = serverArguments;
            options.Setup(o => o.TryGetOptionArgumentList("server", out arguments)).Returns(serverArguments != null);
            return ClientMode.IsIde(options.Object);
        }

        [Test]
        public void IsIde_ConsoleRun_IsFalse()
        {
            Check.That(IsIde(null)).IsFalse();
        }

        [Test]
        public void IsIde_JsonRpcServer_IsTrue()
        {
            Check.That(IsIde(new string[0])).IsTrue();
            Check.That(IsIde(new[] { "jsonrpc" })).IsTrue();
        }

        [Test]
        public void IsIde_DotnetTestPipe_IsFalse()
        {
            Check.That(IsIde(new[] { "dotnettestcli" })).IsFalse();
        }
    }
}
