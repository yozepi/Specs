using Autofac.Extras.Moq;
using Moq;
using NFluent;
using NUnit.Framework.Internal;
using specs.core.Framework.Adapters;
using specs.core.Framework.Processors.TestExecutors;
using System.ComponentModel;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestExecutors
{
    public class TestCaseExecutor_Tests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        private void TestCaseExecutor_Specs()
        {
            TestCaseExecutor subject = null!;
            TestCaseExecutorSettings settings = null!;
            ITestContextAdapter testContext = null!;
            ITestProviderAdapter testProvider = null!;
            ILoggingAdapter logger = null!;

            string testCaseDescription = "I'm a test case!";
            Action testCaseTest = null!;
            List<TestInstance> tests = null!;
            List<CalledMethods> methodCallOrder = null!;

            AutoMock mocker = null!;

            beforeEach = () =>
            {
                subject = null!;
                settings = new TestCaseExecutorSettings();
                logger = null!;

                methodCallOrder = new List<CalledMethods>();

                mocker = AutoMock.GetLoose();
                mocker.Mock<ILoggingAdapter>();
                mocker.Mock<ITestContextAdapter >();

                tests = new List<TestInstance>();
                testCaseTest = () => methodCallOrder.Add(CalledMethods.Assert);
                tests.Add(new TestInstance(testCaseDescription, testCaseTest));
            };

            afterEach = () =>
            {
                mocker.Dispose();
            };

            act = () =>
            {
                mocker.Mock<ITestProviderAdapter>()
                    .Setup(x => x.GetTestInstances())
                    .Returns(tests);

                logger = mocker.Create<ILoggingAdapter>();
                testContext = mocker.Create<ITestContextAdapter>();
                testProvider = mocker.Create<ITestProviderAdapter>();
                subject = new TestCaseExecutor(
                    settings,
                    logger,
                    testContext,
                    testProvider
                );

                subject.ProcessTestCase();
            };
            describe["Test execution call order..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.SetupOnce())
                        .Callback(() => methodCallOrder.Add(CalledMethods.SetupOnce));
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.SetupEach())
                        .Callback(() => methodCallOrder.Add(CalledMethods.SetupEach));
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.Act())
                        .Callback(() => methodCallOrder.Add(CalledMethods.Act));
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.TeardownEach())
                        .Callback(() => methodCallOrder.Add(CalledMethods.TeardownEach));
                    mocker.Mock<ITestContextAdapter>()
                          .Setup(x => x.TeardownOnce())
                          .Callback(() => methodCallOrder.Add(CalledMethods.TeardownOnce));
                };

                context["when there is one test in the test case..."] = () =>
                {
                    var oneTestCompleteCallOrder = new CalledMethods[] {
                    CalledMethods.SetupOnce,
                    CalledMethods.SetupEach,
                    CalledMethods.Act,
                    CalledMethods.Assert,
                    CalledMethods.TeardownEach,
                    CalledMethods.TeardownOnce
                };
                    it[$"should execute in the following order:\n\t\t\t{string.Join(",\n\t\t\t", oneTestCompleteCallOrder)}"] = () =>
                    {
                        Check.That(methodCallOrder).ContainsExactly(oneTestCompleteCallOrder);
                    };
                };

                context["when there are two tests in the test case..."] = () =>
                {
                    beforeEach = () =>
                    {
                        tests.Clear();
                        var test = new TestInstance(testCaseDescription, testCaseTest);
                        tests.AddRange(new TestInstance[] { test, test });
                    };
                    var multiTestCompleteCallOrder = new CalledMethods[] {
                        CalledMethods.SetupOnce,
                        CalledMethods.SetupEach,
                        CalledMethods.Act,
                        CalledMethods.Assert,
                        CalledMethods.TeardownEach,
                        CalledMethods.SetupEach,
                        CalledMethods.Act,
                        CalledMethods.Assert,
                        CalledMethods.TeardownEach,
                        CalledMethods.TeardownOnce
                    };
                    it[$"should execute in the following order:\n\t\t\t{string.Join(",\n\t\t\t", multiTestCompleteCallOrder)}"] = () =>
                    {
                        Check.That(methodCallOrder).ContainsExactly(multiTestCompleteCallOrder);
                    };
                };
            };

            describe["Test assertion behavior..."] = () =>
            {
                context["when the assertion passes..."] = () =>
                {
                    it["should log the passing test"] = () =>
                    {
                        Check
                             .That(methodCallOrder)
                             .ContainsExactly(CalledMethods.Assert);
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Once());
                    };
                };

                context["when the assertion fails..."] = () =>
                {
                    beforeEach = () =>
                    {
                        testCaseTest = () => throw new AssertionException("Test Failed");
                        var test = new TestInstance(testCaseDescription, testCaseTest);
                        tests.Clear();
                        tests.AddRange(new TestInstance[] { test });
                    };

                    it["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertFailure(It.IsAny<AssertionException>(), testCaseDescription), Times.Once());
                    };

                    context["when configured to stop on an assertion failure..."] = () =>
                    {
                        beforeEach = () =>
                        {
                            settings.StopOnAssertFailure = true;
                        };

                        it["should throw the exception"] = expect<AssertionException>();
                    };
                };
            };

            describe[$"{nameof(ITestContextAdapter.SetupOnce)} behavior..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.SetupOnce())
                        .Callback(() => methodCallOrder.Add(CalledMethods.SetupOnce));
                };

                it[$"should execute {nameof(ITestContextAdapter.SetupOnce)} before asserting the test"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.SetupOnce,
                            CalledMethods.Assert);
                };

                context[$"when the {nameof(ITestContextAdapter.SetupOnce)} method fails..."] = () =>
                {
                    beforeEach = () =>
                    {
                        mocker.Mock<ITestContextAdapter>().Setup(m => m.SetupOnce())
                            .Throws<InvalidEnumArgumentException>();
                    };

                    it["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogException(It.IsAny<InvalidEnumArgumentException>()), Times.Once());
                    };

                    it["should throw the exception"] = expect<InvalidEnumArgumentException>();

                    it["should not assert the test"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Never());
                    };
                };
                
                context["when there are no tests to run..."] = () =>
                {
                    beforeEach = () =>
                    {
                        tests.Clear();
                    };

                    it[$"should not invoke {nameof(ITestContextAdapter.SetupOnce)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>()
                            .Verify(x => x.SetupOnce(), Times.Never());
                    };
                };
            };

            describe[$"{nameof(ITestContextAdapter.SetupEach)} behavior..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.SetupEach())
                        .Callback(() => methodCallOrder.Add(CalledMethods.SetupEach));
                };

                it[$"should execute {nameof(ITestContextAdapter.SetupEach)} before asserting the test"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.SetupEach,
                            CalledMethods.Assert);
                };

                context[$"when the {nameof(ITestContextAdapter.SetupEach)} method fails..."] = () =>
                {
                    beforeEach = () =>
                    {
                        mocker.Mock<ITestContextAdapter>().Setup(m => m.SetupEach())
                            .Throws<NullReferenceException>();
                    };

                    it["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogException(It.IsAny<NullReferenceException>()), Times.Once());
                    };

                    it["should throw the exception"] = expect<NullReferenceException>();

                    it["should not assert the test"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Never());
                    };
                };

                context["when there are no tests to run..."] = () =>
                {
                    beforeEach = () =>
                    {
                        tests.Clear();
                    };

                    it[$"should not invoke {nameof(ITestContextAdapter.SetupEach)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>()
                            .Verify(x => x.SetupEach(), Times.Never());
                    };
                };
            };

            describe[$"{nameof(ITestContextAdapter.Act)} behavior..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.Act())
                        .Callback(() => methodCallOrder.Add(CalledMethods.Act));
                };

                it[$"should execute {nameof(ITestContextAdapter.Act)} before asserting the test"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.Act,
                            CalledMethods.Assert);
                };

                context["when the Act method fails..."] = () =>
                {
                    beforeEach = () =>
                    {
                        mocker.Mock<ITestContextAdapter>().Setup(m => m.Act())
                            .Throws<NullReferenceException>();
                    };

                    it["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogException(It.IsAny<NullReferenceException>()), Times.Once());
                    };

                    it["should throw the exception"] = expect<NullReferenceException>();

                    it["should not assert the test"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Never());
                    };
                };

                context["when there are no tests to run..."] = () =>
                {
                    beforeEach = () =>
                    {
                        tests.Clear();
                    };

                    it[$"should not invoke {nameof(ITestContextAdapter.Act)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>()
                            .Verify(x => x.Act(), Times.Never());
                    };
                };
            };

            describe[$"{nameof(ITestContextAdapter.TeardownEach)} behavior..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.TeardownEach())
                        .Callback(() => methodCallOrder.Add(CalledMethods.TeardownOnce));
                };

                it[$"should assert the test before running {nameof(ITestContextAdapter.TeardownEach)}"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.Assert,
                            CalledMethods.TeardownOnce);
                };

                context[$"when {nameof(ITestContextAdapter.TeardownEach)} fails..."] = () =>
                {
                    beforeEach = () =>
                    {
                        mocker.Mock<ITestContextAdapter>().Setup(m => m.TeardownOnce())
                            .Throws<InvalidCastException>();
                    };

                    it["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogException(It.IsAny<InvalidCastException>()), Times.Once());
                    };

                    it["should throw the exception"] = expect<InvalidCastException>();

                    it["should have already asserted the test"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Once());
                    };
                };

                context["when there are no tests to run..."] = () =>
                {
                    beforeEach = () =>
                    {
                        tests.Clear();
                    };

                    it[$"should not invoke {nameof(ITestContextAdapter.TeardownEach)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>()
                            .Verify(x => x.TeardownEach(), Times.Never());
                    };
                };
            };

            describe[$"{nameof(ITestContextAdapter.TeardownOnce)} behavior..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.TeardownOnce())
                        .Callback(() => methodCallOrder.Add(CalledMethods.TeardownOnce));
                };

                it[$"should assert the test before running {nameof(ITestContextAdapter.TeardownOnce)}"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.Assert,
                            CalledMethods.TeardownOnce);
                };

                context[$"when {nameof(ITestContextAdapter.TeardownOnce)} fails..."] = () =>
                {
                    beforeEach = () =>
                    {
                        mocker.Mock<ITestContextAdapter>().Setup(m => m.TeardownOnce())
                            .Throws<InvalidCastException>();
                    };

                    it["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogException(It.IsAny<InvalidCastException>()), Times.Once());
                    };

                    it["should throw the exception"] = expect<InvalidCastException>();

                    it["should have already asserted the test"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Once());
                    };
                };

                context["when there are no tests to run..."] = () =>
                {
                    beforeEach = () =>
                    {
                        tests.Clear();
                    };

                    it[$"should not invoke {nameof(ITestContextAdapter.TeardownOnce)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>()
                            .Verify(x => x.TeardownOnce(), Times.Never());
                    };
                };
            };
        }
    }

    enum CalledMethods
    {
        SetupOnce,
        SetupEach,
        Act,
        Assert,
        TeardownEach,
        TeardownOnce
    }
}
