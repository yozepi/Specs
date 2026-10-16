using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Conversion;
using specs.BDD.VSTestAdapter.Execution;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Reflection;
using specs.BDD.VSTestAdapter.Discovery;

namespace specs.BDD.VSTestAdapter.Tests.Execution
{
    /// <summary>
    /// Integration-style unit tests for <see cref="TestInvoker"/>.
    /// Each test builds a real <see cref="TestCase"/> with metadata pointing to a fixture
    /// class in this assembly, then runs <c>TestInvoker.Invoke</c> and checks the
    /// <see cref="TestResult"/> outcome.
    /// </summary>
    [TestFixture]
    public class TestInvoker_Tests
    {
        // ── Helper ────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a <see cref="TestCase"/> with the metadata that <c>TestInvoker</c> requires.
        /// Uses the test assembly's location as the source so that <c>Assembly.LoadFrom</c>
        /// resolves to the currently-executing assembly.
        /// </summary>
        private static TestCase MakeTestCase<TSpec>(
            string methodName,
            string contextPath,
            string testDescription)
            where TSpec : specs.BDD.Specifications
        {
            var method = typeof(TSpec).GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;

            var specTest = new SpecTest
            {
                SpecificationClass = typeof(TSpec),
                ContextMethod = method,
                ContextPath = contextPath,
                TestDescription = testDescription
            };

            // Use the assembly that contains the spec fixture as the source DLL.
            var source = typeof(TSpec).Assembly.Location;
            return TestCaseConverter.ToVSTestCase(specTest, source);
        }

        // ── Passing tests ─────────────────────────────────────────────────────────────────────

        [Test]
        public void Invoke_PassingTest_ReturnsPassed()
        {
            var testCase = MakeTestCase<SimpleSpec>(
                "describe_something", "simple context", "test one");

            var result = TestInvoker.Invoke(testCase);

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Passed);
        }

        [Test]
        public void Invoke_PassingTest_HasNoErrorMessage()
        {
            var testCase = MakeTestCase<SimpleSpec>(
                "describe_something", "simple context", "test two");

            var result = TestInvoker.Invoke(testCase);

            Check.That(result.ErrorMessage).IsNullOrEmpty();
        }

        [Test]
        public void Invoke_PassingTest_HasNonNegativeDuration()
        {
            var testCase = MakeTestCase<SimpleSpec>(
                "describe_something", "simple context", "test one");

            var result = TestInvoker.Invoke(testCase);

            Check.That(result.Duration.TotalMilliseconds).IsGreaterOrEqualThan(0);
        }

        // ── Failing tests ─────────────────────────────────────────────────────────────────────

        [Test]
        public void Invoke_FailingTest_ReturnsFailed()
        {
            SpecWithMixedResults.TriggerFailure = true;
            try
            {
                var testCase = MakeTestCase<SpecWithMixedResults>(
                    "describe_mixed", "mixed results", "a failing test");

                var result = TestInvoker.Invoke(testCase);

                Check.That(result.Outcome).IsEqualTo(TestOutcome.Failed);
            }
            finally { SpecWithMixedResults.TriggerFailure = false; }
        }

        [Test]
        public void Invoke_FailingTest_ErrorMessageContainsExceptionMessage()
        {
            SpecWithMixedResults.TriggerFailure = true;
            try
            {
                var testCase = MakeTestCase<SpecWithMixedResults>(
                    "describe_mixed", "mixed results", "a failing test");

                var result = TestInvoker.Invoke(testCase);

                Check.That(result.ErrorMessage).Contains("intentional failure");
            }
            finally { SpecWithMixedResults.TriggerFailure = false; }
        }

        [Test]
        public void Invoke_FailingTest_PassingTestInSameContextIsUnaffected()
        {
            // The passing test should still pass whether or not TriggerFailure is set,
            // because TestInvoker runs each test in isolation.
            var testCase = MakeTestCase<SpecWithMixedResults>(
                "describe_mixed", "mixed results", "a passing test");

            var result = TestInvoker.Invoke(testCase);

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Passed);
        }

        // ── Nested context tests ──────────────────────────────────────────────────────────────

        [Test]
        public void Invoke_NestedTest_ReturnsPassed()
        {
            var testCase = MakeTestCase<NestedContextSpec>(
                "describe_something",
                $"root context{TestStructureExtractor.PathSeparator}when doing X",
                "should work for X");

            var result = TestInvoker.Invoke(testCase);

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Passed);
        }

        [Test]
        public void Invoke_DeeplyNestedTest_ReturnsPassed()
        {
            var testCase = MakeTestCase<DeeplyNestedContextSpec>(
                "describe_something",
                $"root{TestStructureExtractor.PathSeparator}level one{TestStructureExtractor.PathSeparator}level two",
                "deep test");

            var result = TestInvoker.Invoke(testCase);

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Passed);
        }

        // ── Lifecycle order ───────────────────────────────────────────────────────────────────

        [Test]
        public void Invoke_RunsLifecycleInCorrectOrder()
        {
            SpecWithLifecycle.ClearLog();

            var testCase = MakeTestCase<SpecWithLifecycle>(
                "describe_lifecycle",
                "lifecycle spec",
                "records execution order");

            TestInvoker.Invoke(testCase);

            // SetupOnce → SetupEach → Act → [test] → TeardownEach → TeardownOnce
            var log = SpecWithLifecycle.Log;
            Check.That(log).ContainsExactly("BeforeAll", "BeforeEach", "Act", "AfterEach", "AfterAll");
        }

        // ── Result metadata ───────────────────────────────────────────────────────────────────

        [Test]
        public void Invoke_Result_TestCaseMatchesInput()
        {
            var testCase = MakeTestCase<SimpleSpec>(
                "describe_something", "simple context", "test one");

            var result = TestInvoker.Invoke(testCase);

            Check.That(result.TestCase.FullyQualifiedName)
                .IsEqualTo(testCase.FullyQualifiedName);
        }
    }
}
