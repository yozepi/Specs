using System.Diagnostics;
using NFluent;
using NUnit.Framework;

namespace specs.BDD.TestingPlatform.Tests
{
    /// <summary>
    /// Runs the specs.BDD.TestingPlatform.IntegrationTests app end to end. It compiles the same specs
    /// as specs.BDD.IntegrationTests, so its totals must match the VSTest run.
    /// </summary>
    [TestFixture]
    public class IntegrationApp_Tests
    {
        private const string AppName = "specs.BDD.TestingPlatform.IntegrationTests";
        private const string Root = "/" + AppName + "/specs.BDD.IntegrationTests";

        private static (int ExitCode, string Output) RunApp(params string[] arguments)
        {
            // This assembly runs from <tests>/specs.BDD.TestingPlatform.Tests/bin/<Configuration>/<TFM>/.
            var outputDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            var configuration = outputDir.Parent!.Name;
            var testsDir = outputDir.Parent.Parent!.Parent!.Parent!.FullName;
            var appDll = Path.Combine(testsDir, AppName, "bin", configuration, outputDir.Name, AppName + ".dll");

            var startInfo = new ProcessStartInfo("dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add(appDll);
            foreach (var argument in new[] { "--no-ansi", "--progress", "off" }.Concat(arguments))
                startInfo.ArgumentList.Add(argument);

            using var process = Process.Start(startInfo)!;
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(120_000))
            {
                process.Kill(entireProcessTree: true);
                Assert.Fail($"{AppName} did not finish.");
            }

            return (process.ExitCode, stdout.Result + stderr.Result);
        }

        [Test]
        public void Run_AllSpecs_MatchesTheVSTestTotals()
        {
            var (exitCode, output) = RunApp();

            Check.That(exitCode).IsEqualTo(0);
            Check.That(output).Contains("total: 34");
            Check.That(output).Contains("succeeded: 29");
            Check.That(output).Contains("skipped: 5");
        }

        [Test]
        public void ListTests_ShowsFullPathsAndTheTree()
        {
            var (exitCode, output) = RunApp("--list-tests");

            Check.That(exitCode).IsEqualTo(0);
            Check.That(output).Contains("found 34 test(s)");
            Check.That(output).Contains($"{Root}/CalculatorSpecs/Multiplication/positive operands/small numbers/returns the correct product");
            Check.That(output).Contains(string.Join(Environment.NewLine,
                "      Multiplication",
                "        positive operands",
                "          small numbers",
                "            - returns the correct product"));
        }

        [Test]
        public void TreeNodeFilter_RunsOnlyTheMatchingSubtree()
        {
            var (exitCode, output) = RunApp("--treenode-filter", "/*/*/CalculatorSpecs/Addition/**");

            Check.That(exitCode).IsEqualTo(0);
            Check.That(output).Contains("total: 3");
        }

        [Test]
        public void TreeNodeFilter_GroupedDescribe_RunsAsOneTest()
        {
            // Parentheses are filter operators, so the literal ones in the name are escaped.
            var (exitCode, output) = RunApp("--treenode-filter", @"/*/*/CalculatorSpecs/Subtraction \(grouped\)");

            Check.That(exitCode).IsEqualTo(0);
            Check.That(output).Contains("total: 1");
            Check.That(output).Contains("succeeded: 1");
        }
    }
}
