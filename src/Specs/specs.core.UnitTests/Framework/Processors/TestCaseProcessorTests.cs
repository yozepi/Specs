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
            IAssertAdapter assert = null!;
            ILoggingAdapter logger = null!;
            ISetupTestAdapter setup = null!;
            IActAdapter subjectAct = null!;
            ITeardownAdapter tearDown = null!;

            List<CalledAdapters> adapterCallOrder = null!;

            AutoMock mocker = null!;

            beforeEach = () =>
            {
                subject = null!;
                settings = new TestCaseProcessorSettings(() => testCaseDescription);
                assert = null!;
                logger = null!;
                setup = null!;
                subjectAct = null!;
                tearDown = null!;

                adapterCallOrder = new List<CalledAdapters>();

                mocker = AutoMock.GetLoose();
                mocker.Mock<ILoggingAdapter>();
                mocker.Mock<IAssertAdapter>()
                    .Setup(m => m.Assert())
                    .Callback(() => adapterCallOrder.Add(CalledAdapters.AssertAdapter));
            };

            afterEach = () =>
            {
                mocker.Dispose();
            };

            act = () =>
            {
                logger = mocker.Create<ILoggingAdapter>();
                assert = mocker.Create<IAssertAdapter>();

                subject = new TestCaseProcessor(
                    settings,
                    logger,
                    assert,
                    setup,
                    subjectAct,
                    tearDown
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
                    mocker.Mock<IAssertAdapter>()
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

            describe["when a setup adapter is included"] = () =>
            {
                beforeEach = () =>
                {
                    mocker.Mock<ISetupTestAdapter>()
                        .Setup(x => x.Setup())
                        .Callback(() => adapterCallOrder.Add(CalledAdapters.SetupAdapter));

                    setup = mocker.Create<ISetupTestAdapter>();

                };

                it["should execute the setup adapter before asserting the test."] = () =>
                {
                    Check
                        .That(adapterCallOrder)
                        .ContainsExactly(
                            CalledAdapters.SetupAdapter,
                            CalledAdapters.AssertAdapter);
                };

                context["when the setup adapter fails..."] = () =>
                {
                    beforeEach = () =>
                    {
                        mocker.Mock<ISetupTestAdapter>().Setup(m => m.Setup())
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

                describe["when an act adapter is included"] = () =>
                {
                    beforeEach = () =>
                    {
                        mocker.Mock<IActAdapter>()
                            .Setup(x => x.Act())
                            .Callback(() => adapterCallOrder.Add(CalledAdapters.ActAdapter));
                        subjectAct = mocker.Create<IActAdapter>();
                    };
                    it
                    ["should execute the adapters in the following order: Setup, Act, Assert"] = () =>
                    {
                        Check
                            .That(adapterCallOrder)
                            .ContainsExactly(
                                CalledAdapters.SetupAdapter,
                                CalledAdapters.ActAdapter,
                                CalledAdapters.AssertAdapter);
                    };
                    context["when the act adapter fails..."] = () =>
                    {
                        beforeEach = () =>
                        {
                            mocker.Mock<IActAdapter>().Setup(m => m.Act())
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

                    describe["when an teardown adapter is included"] = () =>
                    {
                        beforeEach = () =>
                        {
                            mocker.Mock<ITeardownAdapter>()
                                .Setup(x => x.Teardown())
                                .Callback(() => adapterCallOrder.Add(CalledAdapters.TeardownAdapter));
                            tearDown = mocker.Create<ITeardownAdapter>();
                        };

                        it["should execute the adapters in the following order: Setup, Act, Assert, Teardown"] = () =>
                        {
                            Check
                                .That(adapterCallOrder)
                                .ContainsExactly(
                                    CalledAdapters.SetupAdapter,
                                    CalledAdapters.ActAdapter,
                                    CalledAdapters.AssertAdapter,
                                    CalledAdapters.TeardownAdapter);
                        };

                        context["when the TearDown adapter fails..."] = () =>
                        {
                            beforeEach = () =>
                            {
                                mocker.Mock<ITeardownAdapter>().Setup(m => m.Teardown())
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
                };
            };
        }

    }

    enum CalledAdapters
    {
        SetupAdapter,
        ActAdapter,
        AssertAdapter,
        TeardownAdapter
    }
}
