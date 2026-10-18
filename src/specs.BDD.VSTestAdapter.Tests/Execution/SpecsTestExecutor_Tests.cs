using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Moq;
using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Conversion;
using specs.BDD.VSTestAdapter.Execution;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;

namespace specs.BDD.VSTestAdapter.Tests.Execution
{
    [TestFixture]
    public class SpecsTestExecutor_Tests
    {
        private static readonly string Source = typeof(MultiContextSpec).Assembly.Location;
        private static readonly string MultiContextPrefix = typeof(MultiContextSpec).FullName + "::";

        private Mock<IRunContext> _runContext = null!;
        private Mock<IFrameworkHandle> _frameworkHandle = null!;
        private Mock<ITestCaseFilterExpression> _filter = null!;
        private List<TestResult> _results = null!;

        [SetUp]
        public void SetUp()
        {
            _runContext = new Mock<IRunContext>();
            _frameworkHandle = new Mock<IFrameworkHandle>();
            _filter = new Mock<ITestCaseFilterExpression>();
            _results = new List<TestResult>();

            _frameworkHandle
                .Setup(h => h.RecordResult(It.IsAny<TestResult>()))
                .Callback<TestResult>(r => _results.Add(r));

            _runContext
                .Setup(c => c.GetTestCaseFilter(It.IsAny<IEnumerable<string>>(), It.IsAny<Func<string, TestProperty>>()))
                .Returns(_filter.Object);
        }

        private void RunSources() =>
            new SpecsTestExecutor().RunTests(new[] { Source }, _runContext.Object, _frameworkHandle.Object);

        [Test]
        public void RunTests_WithFilter_RunsOnlyMatchingTests()
        {
            _filter
                .Setup(f => f.MatchTestCase(It.IsAny<TestCase>(), It.IsAny<Func<string, object>>()))
                .Returns<TestCase, Func<string, object>>((tc, _) => tc.FullyQualifiedName.StartsWith(MultiContextPrefix));

            RunSources();

            var names = _results.Select(r => r.TestCase.FullyQualifiedName).ToList();
            Check.That(names).HasSize(3);
            Check.That(names.All(n => n.StartsWith(MultiContextPrefix))).IsTrue();
        }

        [Test]
        public void RunTests_WithFilter_ProvidesPropertyValuesByName()
        {
            var mismatches = new List<string>();
            _filter
                .Setup(f => f.MatchTestCase(It.IsAny<TestCase>(), It.IsAny<Func<string, object>>()))
                .Returns<TestCase, Func<string, object>>((tc, provider) =>
                {
                    if (!tc.FullyQualifiedName.StartsWith(MultiContextPrefix))
                        return false;

                    if (!Equals(provider("ContextMethod"), tc.GetPropertyValue(TestProperties.ContextMethod)))
                        mismatches.Add("ContextMethod");
                    if (!Equals(provider("testdescription"), tc.GetPropertyValue(TestProperties.TestDescription)))
                        mismatches.Add("TestDescription");
                    if (!Equals(provider("FullyQualifiedName"), tc.FullyQualifiedName))
                        mismatches.Add("FullyQualifiedName");
                    if (provider("Unknown") != null)
                        mismatches.Add("Unknown");

                    return false;
                });

            RunSources();

            Check.That(mismatches).IsEmpty();
            Check.That(_results).IsEmpty();
        }

        [Test]
        public void RunTests_WithFilter_PassesSupportedPropertyNames()
        {
            IEnumerable<string>? supported = null;
            _runContext
                .Setup(c => c.GetTestCaseFilter(It.IsAny<IEnumerable<string>>(), It.IsAny<Func<string, TestProperty>>()))
                .Callback<IEnumerable<string>, Func<string, TestProperty>>((names, _) => supported = names.ToList())
                .Returns(_filter.Object);

            RunSources();

            Check.That(supported).Contains(
                "FullyQualifiedName", "DisplayName", "ContextMethod", "ContextPath", "TestDescription", "IsPending");
        }

        [TestCase(true, 4, TestOutcome.Skipped)]
        [TestCase(false, 1, TestOutcome.Passed)]
        public void RunTests_FilterOnIsPending_SelectsByPendingState(bool isPending, int expectedCount, TestOutcome expectedOutcome)
        {
            var prefix = typeof(SpecWithPending).FullName + "::";
            _filter
                .Setup(f => f.MatchTestCase(It.IsAny<TestCase>(), It.IsAny<Func<string, object>>()))
                .Returns<TestCase, Func<string, object>>((tc, provider) =>
                    tc.FullyQualifiedName.StartsWith(prefix) && Equals(provider("IsPending"), isPending));

            RunSources();

            Check.That(_results).HasSize(expectedCount);
            Check.That(_results.Select(r => r.Outcome)).ContainsOnlyElementsThatMatch(o => o == expectedOutcome);
        }

        [Test]
        public void RunTests_InvalidFilter_ReportsErrorAndRunsNothing()
        {
            _runContext
                .Setup(c => c.GetTestCaseFilter(It.IsAny<IEnumerable<string>>(), It.IsAny<Func<string, TestProperty>>()))
                .Throws(new TestPlatformFormatException("bad filter"));

            RunSources();

            _frameworkHandle.Verify(h => h.SendMessage(TestMessageLevel.Error, "bad filter"), Times.Once);
            Check.That(_results).IsEmpty();
        }
    }
}
