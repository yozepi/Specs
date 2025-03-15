using Autofac.Extras.Moq;
using Moq;
using NFluent;
using NUnit.Framework.Internal;
using specs.core.Framework.Adaptors;
using specs.core.Framework.Processors;
using System.ComponentModel;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors
{
    public class TestCaseProcessorTests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            this.RunSpecs();
        }

        private void TestCaseProcessor_Specs()
        {
            TestCaseProcessor subject = null!;
            TestCaseProcessorSettings settings = null!;
            string testCaseDescription = "I'm a test case!";
            ITestContext testContext = null!;
            ILoggingAdapter logger = null!;

            List<CalledMethods> methodCallOrder = null!;

            AutoMock mocker = null!;

            beforeEach = () =>
            {
                subject = null!;
                settings = new TestCaseProcessorSettings(() => testCaseDescription);
                logger = null!;

                methodCallOrder = new List<CalledMethods>();

                mocker = AutoMock.GetLoose();
                mocker.Mock<ILoggingAdapter>();
                mocker.Mock<ITestContext>()
                    .Setup(m => m.Assert())
                    .Callback(() => methodCallOrder.Add(CalledMethods.Assert));
            };

            afterEach = () =>
            {
                mocker.Dispose();
            };

            act = () =>
            {
                logger = mocker.Create<ILoggingAdapter>();
                testContext = mocker.Create<ITestContext>();

                subject = new TestCaseProcessor(
                    settings,
                    logger,
                    testContext
                );

                subject.ProcessTestCase();
            };

            it["should assert the test case"] = () =>
            {
                mocker.Mock<ILoggingAdapter>()
                    .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Once());
            };

            context["when the assertion fails..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContext>()
                        .Setup(x => x.Assert())
                        .Throws(new AssertionException("Test Failed"));
                };

                it["should log the failure"] = () =>
                {
                    mocker.Mock<ILoggingAdapter>()
                        .Verify(x => x.LogAssertFailure(It.IsAny<AssertionException>()), Times.Once());
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

            context["running Setup..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContext>()
                        .Setup(x => x.Setup())
                        .Callback(() => methodCallOrder.Add(CalledMethods.Setup));
                };

                it["should execute Setup before asserting the test"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.Setup,
                            CalledMethods.Assert);
                };

                context["when the Setup method fails..."] = () =>
                {
                    beforeEach = () =>
                    {
                        mocker.Mock<ITestContext>().Setup(m => m.Setup())
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
            };

            context["running Act..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContext>()
                        .Setup(x => x.Act())
                        .Callback(() => methodCallOrder.Add(CalledMethods.Act));
                };
                it
                ["should execute Act before asserting the test"] = () =>
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
                        mocker.Mock<ITestContext>().Setup(m => m.Act())
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
            };

            context["running Teardown..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContext>()
                        .Setup(x => x.Teardown())
                        .Callback(() => methodCallOrder.Add(CalledMethods.Teardown));
                };

                it["should assert the test before running Teardown"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.Assert,
                            CalledMethods.Teardown);
                };

                context["when TearDown fails..."] = () =>
                {
                    beforeEach = () =>
                    {
                        mocker.Mock<ITestContext>().Setup(m => m.Teardown())
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
            };

            describe["Setup, Act, Assert, and Teardown call order..."] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ITestContext>()
                        .Setup(x => x.Setup())
                        .Callback(() => methodCallOrder.Add(CalledMethods.Setup));
                    mocker.Mock<ITestContext>()
                        .Setup(x => x.Act())
                        .Callback(() => methodCallOrder.Add(CalledMethods.Act));
                    mocker.Mock<ITestContext>()
                        .Setup(x => x.Teardown())
                        .Callback(() => methodCallOrder.Add(CalledMethods.Teardown));
                };
                it["should call call Setup, then Act, then Assert, then Teardown"] = () =>
                {
                    Check.That(methodCallOrder).ContainsExactly(
                        CalledMethods.Setup,
                        CalledMethods.Act,
                        CalledMethods.Assert,
                        CalledMethods.Teardown);
                };
            };
        }

    }

    enum CalledMethods
    {
        Setup,
        Act,
        Assert,
        Teardown
    }
}
