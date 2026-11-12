using Autofac.Extras.Moq;
using Moq;
using NFluent;
using specs.core.Framework.Adapters;
using specs.core.Framework.Processors.TestExecutors;
using System.ComponentModel;
using specs.BDD;
using specs.BDD.Attributes;

namespace specs.core.UnitTests.Framework.Processors.TestExecutors
{
    public class TestCaseExecutor_Tests : Specifications
    {

        [Feature]
        public void TestCaseExecutor_Specs()
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
            Exception? _caughtActException = null;

            BeforeEach(() =>
            {
                subject = null!;
                _caughtActException = null;
                settings = new TestCaseExecutorSettings();
                logger = null!;

                methodCallOrder = new List<CalledMethods>();

                mocker = AutoMock.GetLoose();
                mocker.Mock<ILoggingAdapter>();
                mocker.Mock<ITestContextAdapter >();

                tests = new List<TestInstance>();
                testCaseTest = () => methodCallOrder.Add(CalledMethods.Assert);
                tests.Add(new TestInstance(testCaseDescription, testCaseTest));
            });

            AfterEach(() =>
            {
                mocker.Dispose();
            });

            Act(() =>
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

                try { subject.ProcessTestCase(CancellationToken.None); }
                catch (Exception ex) { _caughtActException = ex; }
            });
            Describe["Test execution call order..."] = () =>
            {
                BeforeEach(() =>
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
                });

                Context["when there is one test in the test case..."] = () =>
                {
                    var oneTestCompleteCallOrder = new CalledMethods[] {
                    CalledMethods.SetupOnce,
                    CalledMethods.SetupEach,
                    CalledMethods.Act,
                    CalledMethods.Assert,
                    CalledMethods.TeardownEach,
                    CalledMethods.TeardownOnce
                };
                    It[$"should execute in the following order:\n\t\t\t{string.Join(",\n\t\t\t", oneTestCompleteCallOrder)}"] = () =>
                    {
                        Check.That(methodCallOrder).ContainsExactly(oneTestCompleteCallOrder);
                    };
                };

                Context["when there are two tests in the test case..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        tests.Clear();
                        var test = new TestInstance(testCaseDescription, testCaseTest);
                        tests.AddRange(new TestInstance[] { test, test });
                    });
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
                    It[$"should execute in the following order:\n\t\t\t{string.Join(",\n\t\t\t", multiTestCompleteCallOrder)}"] = () =>
                    {
                        Check.That(methodCallOrder).ContainsExactly(multiTestCompleteCallOrder);
                    };
                };
            };

            Describe["Test assertion behavior..."] = () =>
            {
                Context["when the assertion passes..."] = () =>
                {
                    It["should log the passing test"] = () =>
                    {
                        Check
                             .That(methodCallOrder)
                             .ContainsExactly(CalledMethods.Assert);
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Once());
                    };
                };

                Context["when the assertion fails..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        testCaseTest = () => throw new InvalidOperationException("Test Failed");
                        var test = new TestInstance(testCaseDescription, testCaseTest);
                        tests.Clear();
                        tests.AddRange(new TestInstance[] { test });
                    });

                    It["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertFailure(Moq.It.IsAny<InvalidOperationException>(), testCaseDescription), Times.Once());
                    };

                    Context["when configured to stop on an assertion failure..."] = () =>
                    {
                        BeforeEach(() =>
                        {
                            settings.StopOnAssertFailure = true;
                        });

                        It["should throw the exception"] = () =>
                        {
                            Check.That(_caughtActException).IsNotNull();
                            Check.That(_caughtActException).IsInstanceOf<InvalidOperationException>();
                        };

                        It[$"should still run {nameof(ITestContextAdapter.TeardownEach)} and {nameof(ITestContextAdapter.TeardownOnce)}"] = () =>
                        {
                            mocker.Mock<ITestContextAdapter>().Verify(x => x.TeardownEach(), Times.Once());
                            mocker.Mock<ITestContextAdapter>().Verify(x => x.TeardownOnce(), Times.Once());
                        };

                        Context[$"when {nameof(ITestContextAdapter.TeardownOnce)} also throws..."] = () =>
                        {
                            BeforeEach(() =>
                            {
                                mocker.Mock<ITestContextAdapter>().Setup(m => m.TeardownOnce())
                                    .Throws<InvalidCastException>();
                            });

                            It["should preserve the original failure"] = () =>
                            {
                                Check.That(_caughtActException).IsInstanceOf<InvalidOperationException>();
                            };
                        };
                    };
                };
            };

            Describe[$"{nameof(ITestContextAdapter.SetupOnce)} behavior..."] = () =>
            {
                BeforeEach(() =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.SetupOnce())
                        .Callback(() => methodCallOrder.Add(CalledMethods.SetupOnce));
                });

                It[$"should execute {nameof(ITestContextAdapter.SetupOnce)} before asserting the test"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.SetupOnce,
                            CalledMethods.Assert);
                };

                Context[$"when the {nameof(ITestContextAdapter.SetupOnce)} method fails..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        mocker.Mock<ITestContextAdapter>().Setup(m => m.SetupOnce())
                            .Throws<InvalidEnumArgumentException>();
                    });

                    It["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogException(Moq.It.IsAny<InvalidEnumArgumentException>()), Times.Once());
                    };

                    It["should throw the exception"] = () =>
                    {
                        Check.That(_caughtActException).IsNotNull();
                        Check.That(_caughtActException).IsInstanceOf<InvalidEnumArgumentException>();
                    };

                    It["should not assert the test"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Never());
                    };
                };
                
                Context["when there are no tests to run..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        tests.Clear();
                    });

                    It[$"should not invoke {nameof(ITestContextAdapter.SetupOnce)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>()
                            .Verify(x => x.SetupOnce(), Times.Never());
                    };
                };
            };

            Describe[$"{nameof(ITestContextAdapter.SetupEach)} behavior..."] = () =>
            {
                BeforeEach(() =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.SetupEach())
                        .Callback(() => methodCallOrder.Add(CalledMethods.SetupEach));
                });

                It[$"should execute {nameof(ITestContextAdapter.SetupEach)} before asserting the test"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.SetupEach,
                            CalledMethods.Assert);
                };

                Context[$"when the {nameof(ITestContextAdapter.SetupEach)} method fails..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        mocker.Mock<ITestContextAdapter>().Setup(m => m.SetupEach())
                            .Throws<NullReferenceException>();
                    });

                    It["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogException(Moq.It.IsAny<NullReferenceException>()), Times.Once());
                    };

                    It["should throw the exception"] = () =>
                    {
                        Check.That(_caughtActException).IsNotNull();
                        Check.That(_caughtActException).IsInstanceOf<NullReferenceException>();
                    };

                    It[$"should still run {nameof(ITestContextAdapter.TeardownEach)} and {nameof(ITestContextAdapter.TeardownOnce)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>().Verify(x => x.TeardownEach(), Times.Once());
                        mocker.Mock<ITestContextAdapter>().Verify(x => x.TeardownOnce(), Times.Once());
                    };

                    It["should not assert the test"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Never());
                    };
                };

                Context["when there are no tests to run..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        tests.Clear();
                    });

                    It[$"should not invoke {nameof(ITestContextAdapter.SetupEach)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>()
                            .Verify(x => x.SetupEach(), Times.Never());
                    };
                };
            };

            Describe[$"{nameof(ITestContextAdapter.Act)} behavior..."] = () =>
            {
                BeforeEach(() =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.Act())
                        .Callback(() => methodCallOrder.Add(CalledMethods.Act));
                });

                It[$"should execute {nameof(ITestContextAdapter.Act)} before asserting the test"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.Act,
                            CalledMethods.Assert);
                };

                Context["when the Act method fails..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        mocker.Mock<ITestContextAdapter>().Setup(m => m.Act())
                            .Throws<NullReferenceException>();
                    });

                    It["should log the Act exception as the test's failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertFailure(Moq.It.IsAny<NullReferenceException>(), testCaseDescription), Times.Once());
                    };

                    It["should not throw the exception"] = () =>
                    {
                        Check.That(_caughtActException).IsNull();
                    };

                    It["should not log the test as passing"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Never());
                    };

                    It[$"should still run {nameof(ITestContextAdapter.TeardownEach)} and {nameof(ITestContextAdapter.TeardownOnce)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>().Verify(x => x.TeardownEach(), Times.Once());
                        mocker.Mock<ITestContextAdapter>().Verify(x => x.TeardownOnce(), Times.Once());
                    };

                    Context["when configured to stop on an assertion failure..."] = () =>
                    {
                        BeforeEach(() =>
                        {
                            settings.StopOnAssertFailure = true;
                        });

                        It["should throw the Act exception"] = () =>
                        {
                            Check.That(_caughtActException).IsInstanceOf<NullReferenceException>();
                        };
                    };

                    Context["when the test handles the Act exception..."] = () =>
                    {
                        Exception? observedActException = null;

                        BeforeEach(() =>
                        {
                            observedActException = null;
                            tests.Clear();
                            tests.Add(new TestInstance(testCaseDescription, () =>
                            {
                                observedActException = ActExceptionContext.Current;
                                ActExceptionContext.MarkHandled();
                            }));
                        });

                        It["should expose the Act exception to the test"] = () =>
                        {
                            Check.That(observedActException).IsInstanceOf<NullReferenceException>();
                        };

                        It["should log the test as passing"] = () =>
                        {
                            mocker.Mock<ILoggingAdapter>()
                                .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Once());
                        };
                    };
                };

                Context["when there are no tests to run..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        tests.Clear();
                    });

                    It[$"should not invoke {nameof(ITestContextAdapter.Act)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>()
                            .Verify(x => x.Act(), Times.Never());
                    };
                };
            };

            Describe[$"{nameof(ITestContextAdapter.TeardownEach)} behavior..."] = () =>
            {
                BeforeEach(() =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.TeardownEach())
                        .Callback(() => methodCallOrder.Add(CalledMethods.TeardownEach));
                });

                It[$"should assert the test before running {nameof(ITestContextAdapter.TeardownEach)}"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.Assert,
                            CalledMethods.TeardownEach);
                };

                Context[$"when {nameof(ITestContextAdapter.TeardownEach)} fails..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        mocker.Mock<ITestContextAdapter>().Setup(m => m.TeardownEach())
                            .Throws<InvalidCastException>();
                    });

                    It["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogException(Moq.It.IsAny<InvalidCastException>()), Times.Once());
                    };

                    It["should throw the exception"] = () =>
                    {
                        Check.That(_caughtActException).IsNotNull();
                        Check.That(_caughtActException).IsInstanceOf<InvalidCastException>();
                    };

                    It[$"should still run {nameof(ITestContextAdapter.TeardownOnce)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>().Verify(x => x.TeardownOnce(), Times.Once());
                    };

                    It["should report the current spec as failed, not passed"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertFailure(Moq.It.IsAny<InvalidCastException>(), testCaseDescription), Times.Once());
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Never());
                    };

                    Context["when the assertion also failed..."] = () =>
                    {
                        BeforeEach(() =>
                        {
                            tests.Clear();
                            tests.Add(new TestInstance(testCaseDescription, () => throw new InvalidOperationException("Test Failed")));
                        });

                        It["should keep the assertion failure"] = () =>
                        {
                            mocker.Mock<ILoggingAdapter>()
                                .Verify(x => x.LogAssertFailure(Moq.It.IsAny<InvalidOperationException>(), testCaseDescription), Times.Once());
                        };
                    };
                };

                Context["when there are no tests to run..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        tests.Clear();
                    });

                    It[$"should not invoke {nameof(ITestContextAdapter.TeardownEach)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>()
                            .Verify(x => x.TeardownEach(), Times.Never());
                    };
                };
            };

            Describe[$"{nameof(ITestContextAdapter.TeardownOnce)} behavior..."] = () =>
            {
                BeforeEach(() =>
                {
                    mocker.Mock<ITestContextAdapter>()
                        .Setup(x => x.TeardownOnce())
                        .Callback(() => methodCallOrder.Add(CalledMethods.TeardownOnce));
                });

                It[$"should assert the test before running {nameof(ITestContextAdapter.TeardownOnce)}"] = () =>
                {
                    Check
                        .That(methodCallOrder)
                        .ContainsExactly(
                            CalledMethods.Assert,
                            CalledMethods.TeardownOnce);
                };

                Context[$"when {nameof(ITestContextAdapter.TeardownOnce)} fails..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        mocker.Mock<ITestContextAdapter>().Setup(m => m.TeardownOnce())
                            .Throws<InvalidCastException>();
                    });

                    It["should log the failure"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogException(Moq.It.IsAny<InvalidCastException>()), Times.Once());
                    };

                    It["should throw the exception"] = () =>
                    {
                        Check.That(_caughtActException).IsNotNull();
                        Check.That(_caughtActException).IsInstanceOf<InvalidCastException>();
                    };

                    It["should have already asserted the test"] = () =>
                    {
                        mocker.Mock<ILoggingAdapter>()
                            .Verify(x => x.LogAssertSuccess(testCaseDescription), Times.Once());
                    };
                };

                Context["when there are no tests to run..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        tests.Clear();
                    });

                    It[$"should not invoke {nameof(ITestContextAdapter.TeardownOnce)}"] = () =>
                    {
                        mocker.Mock<ITestContextAdapter>()
                            .Verify(x => x.TeardownOnce(), Times.Never());
                    };
                };
            };

            Describe["Constructor validation..."] = () =>
            {
                It["should throw when the test provider is null"] = () =>
                {
                    Check.ThatCode(() => new TestCaseExecutor(settings, logger, testContext, null!))
                        .Throws<ArgumentNullException>()
                        .WithProperty("ParamName", "testProvider");
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
