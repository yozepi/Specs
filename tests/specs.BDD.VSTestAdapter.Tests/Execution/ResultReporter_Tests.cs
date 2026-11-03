using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Conversion;
using specs.BDD.VSTestAdapter.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Reflection;

namespace specs.BDD.VSTestAdapter.Tests.Execution
{
    [TestFixture]
    public class ResultReporter_Tests
    {
        private static readonly Uri FakeExecutorUri = new Uri("executor://SpecsTestExecutor/v1");
        private const string FakeSource = @"C:\tests\MyTests.dll";

        private static TestCase MakeTestCase()
            => new TestCase("SomeClass::SomeMethod::some path::some test", FakeExecutorUri, FakeSource);

        // ── Success ───────────────────────────────────────────────────────────────────────────

        [Test]
        public void Success_OutcomeIsPassed()
        {
            var result = ResultReporter.Success(MakeTestCase(), DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(42));

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Passed);
        }

        [Test]
        public void Success_DurationIsSet()
        {
            var duration = TimeSpan.FromMilliseconds(123);
            var result = ResultReporter.Success(MakeTestCase(), DateTimeOffset.UtcNow, duration);

            Check.That(result.Duration).IsEqualTo(duration);
        }

        [Test]
        public void Success_StartTimeAndEndTimeAreConsistent()
        {
            var start = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(2);
            var result = ResultReporter.Success(MakeTestCase(), start, duration);

            Check.That(result.StartTime).IsEqualTo(start);
            Check.That(result.EndTime).IsEqualTo(start + duration);
        }

        [Test]
        public void Success_TestCaseIsPreserved()
        {
            var testCase = MakeTestCase();
            var result = ResultReporter.Success(testCase, DateTimeOffset.UtcNow, TimeSpan.Zero);

            Check.That(result.TestCase).IsSameReferenceAs(testCase);
        }

        // ── Failure ───────────────────────────────────────────────────────────────────────────

        [Test]
        public void Failure_OutcomeIsFailed()
        {
            var ex = new Exception("something went wrong");
            var result = ResultReporter.Failure(MakeTestCase(), ex, DateTimeOffset.UtcNow, TimeSpan.Zero);

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Failed);
        }

        [Test]
        public void Failure_ErrorMessageFromException()
        {
            var ex = new InvalidOperationException("my assertion failed");
            var result = ResultReporter.Failure(MakeTestCase(), ex, DateTimeOffset.UtcNow, TimeSpan.Zero);

            Check.That(result.ErrorMessage).IsEqualTo("my assertion failed");
        }

        [Test]
        public void Failure_DurationIsSet()
        {
            var duration = TimeSpan.FromMilliseconds(99);
            var ex = new Exception("failure");
            var result = ResultReporter.Failure(MakeTestCase(), ex, DateTimeOffset.UtcNow, duration);

            Check.That(result.Duration).IsEqualTo(duration);
        }

        [Test]
        public void Failure_TestCaseIsPreserved()
        {
            var testCase = MakeTestCase();
            var result = ResultReporter.Failure(testCase, new Exception("x"), DateTimeOffset.UtcNow, TimeSpan.Zero);

            Check.That(result.TestCase).IsSameReferenceAs(testCase);
        }

        // ── Skipped ───────────────────────────────────────────────────────────────────────────

        [Test]
        public void Skipped_OutcomeIsSkipped_WithPendingMessage()
        {
            var result = ResultReporter.Skipped(MakeTestCase(), DateTimeOffset.UtcNow, TimeSpan.Zero);

            Check.That(result.Outcome).IsEqualTo(TestOutcome.Skipped);
            Check.That(result.ErrorMessage).IsEqualTo("Pending");
        }

        [Test]
        public void Skipped_UsesCustomMessage()
        {
            var result = ResultReporter.Skipped(MakeTestCase(), DateTimeOffset.UtcNow, TimeSpan.Zero, "not yet");

            Check.That(result.ErrorMessage).IsEqualTo("not yet");
        }

        [Test]
        public void Skipped_StartTimeAndEndTimeAreConsistent()
        {
            var start = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromMilliseconds(5);
            var result = ResultReporter.Skipped(MakeTestCase(), start, duration);

            Check.That(result.StartTime).IsEqualTo(start);
            Check.That(result.EndTime).IsEqualTo(start + duration);
            Check.That(result.Duration).IsEqualTo(duration);
        }

        // ── TargetInvocationException unwrapping ──────────────────────────────────────────────

        [Test]
        public void Failure_UnwrapsTargetInvocationException()
        {
            var inner = new InvalidOperationException("the real error");
            var wrapper = new TargetInvocationException(inner);

            var result = ResultReporter.Failure(MakeTestCase(), wrapper, DateTimeOffset.UtcNow, TimeSpan.Zero);

            // Error message should come from the inner exception, not the wrapper.
            Check.That(result.ErrorMessage).IsEqualTo("the real error");
        }

        [Test]
        public void Failure_UnwrapsNestedTargetInvocationExceptions()
        {
            var core = new ArgumentException("deeply nested cause");
            var middle = new TargetInvocationException(core);
            var outer = new TargetInvocationException(middle);

            var result = ResultReporter.Failure(MakeTestCase(), outer, DateTimeOffset.UtcNow, TimeSpan.Zero);

            Check.That(result.ErrorMessage).IsEqualTo("deeply nested cause");
        }

        [Test]
        public void Failure_DirectExceptionIsNotUnwrapped()
        {
            var ex = new Exception("direct error");
            var result = ResultReporter.Failure(MakeTestCase(), ex, DateTimeOffset.UtcNow, TimeSpan.Zero);

            Check.That(result.ErrorMessage).IsEqualTo("direct error");
        }
    }
}
