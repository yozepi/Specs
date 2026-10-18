using Moq;
using NFluent;
using specs.core.Framework.Adapters;
using specs.core.Framework.Processors.TestCases;
using specs.BDD;
using specs.BDD.Attributes;

namespace specs.core.UnitTests.Framework.Adapters
{
    public class TestContextAdapter : Specifications
    {
        [Context(Reporting = SpecReporting.GroupedByRootContext)]
        public void Specs()
        {
            core.Framework.Adapters.TestContextAdapter subject = null!;

            Action setupOnceAction = null!;
            Action setupEachAction = null!;
            Action actAction = null!;
            Action teardownEachAction = null!;
            Action teardownOnceAction = null!;

            List<string> callOrder = null!;

            core.Framework.Adapters.TestContextAdapter Create() =>
                new core.Framework.Adapters.TestContextAdapter(
                    setupOnceAction,
                    setupEachAction,
                    actAction,
                    teardownEachAction,
                    teardownOnceAction);

            BeforeEach(() =>
            {
                callOrder = new List<string>();
                setupOnceAction = () => callOrder.Add("SetupOnce");
                setupEachAction = () => callOrder.Add("SetupEach");
                actAction = () => callOrder.Add("Act");
                teardownEachAction = () => callOrder.Add("TeardownEach");
                teardownOnceAction = () => callOrder.Add("TeardownOnce");
            });

            Describe["TestContextAdapter constructor"] = () =>
            {
                Context["when all parameters are valid"] = () =>
                {
                    Act(() =>
                    {
                        subject = Create();
                    });

                    It["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    It["should implement ITestContextAdapter interface"] = () =>
                    {
                        Check.That(subject is ITestContextAdapter).IsTrue();
                    };
                };

                Context["when setupOnce is null"] = () =>
                {
                    It["should throw ArgumentNullException naming the parameter"] = () =>
                    {
                        Check.ThatCode(() => new core.Framework.Adapters.TestContextAdapter(
                                null!, setupEachAction, actAction, teardownEachAction, teardownOnceAction))
                            .Throws<ArgumentNullException>()
                            .WithProperty("ParamName", "setupOnce");
                    };
                };

                Context["when setupEach is null"] = () =>
                {
                    It["should throw ArgumentNullException naming the parameter"] = () =>
                    {
                        Check.ThatCode(() => new core.Framework.Adapters.TestContextAdapter(
                                setupOnceAction, null!, actAction, teardownEachAction, teardownOnceAction))
                            .Throws<ArgumentNullException>()
                            .WithProperty("ParamName", "setupEach");
                    };
                };

                Context["when act is null"] = () =>
                {
                    It["should throw ArgumentNullException naming the parameter"] = () =>
                    {
                        Check.ThatCode(() => new core.Framework.Adapters.TestContextAdapter(
                                setupOnceAction, setupEachAction, null!, teardownEachAction, teardownOnceAction))
                            .Throws<ArgumentNullException>()
                            .WithProperty("ParamName", "act");
                    };
                };

                Context["when teardownEach is null"] = () =>
                {
                    It["should throw ArgumentNullException naming the parameter"] = () =>
                    {
                        Check.ThatCode(() => new core.Framework.Adapters.TestContextAdapter(
                                setupOnceAction, setupEachAction, actAction, null!, teardownOnceAction))
                            .Throws<ArgumentNullException>()
                            .WithProperty("ParamName", "teardownEach");
                    };
                };

                Context["when teardownOnce is null"] = () =>
                {
                    It["should throw ArgumentNullException naming the parameter"] = () =>
                    {
                        Check.ThatCode(() => new core.Framework.Adapters.TestContextAdapter(
                                setupOnceAction, setupEachAction, actAction, teardownEachAction, null!))
                            .Throws<ArgumentNullException>()
                            .WithProperty("ParamName", "teardownOnce");
                    };
                };
            };

            Describe["FromTestCase"] = () =>
            {
                Context["when the test case is null"] = () =>
                {
                    It["should throw ArgumentNullException"] = () =>
                    {
                        Check.ThatCode(() => core.Framework.Adapters.TestContextAdapter.FromTestCase(null!))
                            .Throws<ArgumentNullException>()
                            .WithProperty("ParamName", "testCase");
                    };
                };

                Context["when the test case is valid"] = () =>
                {
                    It["should run the test case's cascaded lifecycle actions"] = () =>
                    {
                        var testCase = new Mock<ITestCase>();
                        testCase.SetupGet(t => t.SetupOnce).Returns(setupOnceAction);
                        testCase.SetupGet(t => t.SetupEach).Returns(setupEachAction);
                        testCase.SetupGet(t => t.Act).Returns(actAction);
                        testCase.SetupGet(t => t.TeardownEach).Returns(teardownEachAction);
                        testCase.SetupGet(t => t.TeardownOnce).Returns(teardownOnceAction);

                        var adapter = core.Framework.Adapters.TestContextAdapter.FromTestCase(testCase.Object);
                        adapter.SetupOnce();
                        adapter.SetupEach();
                        adapter.Act();
                        adapter.TeardownEach();
                        adapter.TeardownOnce();

                        Check.That(callOrder).ContainsExactly(
                            "SetupOnce", "SetupEach", "Act", "TeardownEach", "TeardownOnce");
                    };
                };
            };

            Describe["SetupOnce method"] = () =>
            {
                BeforeEach(() => subject = Create());

                Act(() =>
                {
                    subject.SetupOnce();
                });

                It["should invoke the setupOnce action"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("SetupOnce");
                };

                It["should invoke the action each time SetupOnce is called"] = () =>
                {
                    subject.SetupOnce();
                    subject.SetupOnce();
                    Check.That(callOrder).ContainsExactly("SetupOnce", "SetupOnce", "SetupOnce");
                };

                Context["when the action throws"] = () =>
                {
                    It["should propagate the exception"] = () =>
                    {
                        setupOnceAction = () => throw new InvalidOperationException("Setup failed");
                        subject = Create();

                        Check.ThatCode(() => subject.SetupOnce())
                            .Throws<InvalidOperationException>()
                            .WithMessage("Setup failed");
                    };
                };
            };

            Describe["SetupEach method"] = () =>
            {
                BeforeEach(() => subject = Create());

                Act(() =>
                {
                    subject.SetupEach();
                });

                It["should invoke the setupEach action"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("SetupEach");
                };

                It["should invoke the action each time SetupEach is called"] = () =>
                {
                    subject.SetupEach();
                    subject.SetupEach();
                    Check.That(callOrder).ContainsExactly("SetupEach", "SetupEach", "SetupEach");
                };

                Context["when the action throws"] = () =>
                {
                    It["should propagate the exception"] = () =>
                    {
                        setupEachAction = () => throw new ArgumentException("SetupEach failed");
                        subject = Create();

                        Check.ThatCode(() => subject.SetupEach())
                            .Throws<ArgumentException>()
                            .WithMessage("SetupEach failed");
                    };
                };
            };

            Describe["Act method"] = () =>
            {
                BeforeEach(() => subject = Create());

                Act(() =>
                {
                    subject.Act();
                });

                It["should invoke the act action"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("Act");
                };

                It["should invoke the action each time Act is called"] = () =>
                {
                    subject.Act();
                    subject.Act();
                    Check.That(callOrder).ContainsExactly("Act", "Act", "Act");
                };

                Context["when the action throws"] = () =>
                {
                    It["should propagate the exception"] = () =>
                    {
                        actAction = () => throw new NotImplementedException("Act not implemented");
                        subject = Create();

                        Check.ThatCode(() => subject.Act())
                            .Throws<NotImplementedException>()
                            .WithMessage("Act not implemented");
                    };
                };
            };

            Describe["TeardownEach method"] = () =>
            {
                BeforeEach(() => subject = Create());

                Act(() =>
                {
                    subject.TeardownEach();
                });

                It["should invoke the teardownEach action"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("TeardownEach");
                };

                It["should invoke the action each time TeardownEach is called"] = () =>
                {
                    subject.TeardownEach();
                    subject.TeardownEach();
                    Check.That(callOrder).ContainsExactly("TeardownEach", "TeardownEach", "TeardownEach");
                };

                Context["when the action throws"] = () =>
                {
                    It["should propagate the exception"] = () =>
                    {
                        teardownEachAction = () => throw new InvalidCastException("TeardownEach failed");
                        subject = Create();

                        Check.ThatCode(() => subject.TeardownEach())
                            .Throws<InvalidCastException>()
                            .WithMessage("TeardownEach failed");
                    };
                };
            };

            Describe["TeardownOnce method"] = () =>
            {
                BeforeEach(() => subject = Create());

                Act(() =>
                {
                    subject.TeardownOnce();
                });

                It["should invoke the teardownOnce action"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("TeardownOnce");
                };

                It["should invoke the action each time TeardownOnce is called"] = () =>
                {
                    subject.TeardownOnce();
                    subject.TeardownOnce();
                    Check.That(callOrder).ContainsExactly("TeardownOnce", "TeardownOnce", "TeardownOnce");
                };

                Context["when the action throws"] = () =>
                {
                    It["should propagate the exception"] = () =>
                    {
                        teardownOnceAction = () => throw new ObjectDisposedException("TeardownOnce failed");
                        subject = Create();

                        Check.ThatCode(() => subject.TeardownOnce())
                            .Throws<ObjectDisposedException>();
                    };
                };
            };

            Describe["method interaction and independence"] = () =>
            {
                BeforeEach(() => subject = Create());

                It["should allow calling methods in any order"] = () =>
                {
                    subject.TeardownOnce();
                    subject.SetupOnce();
                    subject.Act();
                    subject.TeardownEach();
                    subject.SetupEach();

                    Check.That(callOrder).ContainsExactly(
                        "TeardownOnce",
                        "SetupOnce",
                        "Act",
                        "TeardownEach",
                        "SetupEach");
                };

                It["should not affect other methods when one throws"] = () =>
                {
                    setupEachAction = () => throw new Exception("SetupEach throws");
                    subject = Create();

                    subject.SetupOnce();
                    try { subject.SetupEach(); } catch { }
                    subject.Act();

                    Check.That(callOrder).ContainsExactly("SetupOnce", "Act");
                };
            };

            Describe["real-world usage scenario"] = () =>
            {
                It["should support multiple test executions with same setup/teardown"] = () =>
                {
                    subject = Create();

                    // First test
                    subject.SetupOnce();
                    subject.SetupEach();
                    subject.Act();
                    subject.TeardownEach();

                    // Second test (SetupOnce not called again)
                    subject.SetupEach();
                    subject.Act();
                    subject.TeardownEach();

                    subject.TeardownOnce();

                    Check.That(callOrder).ContainsExactly(
                        "SetupOnce",
                        "SetupEach",
                        "Act",
                        "TeardownEach",
                        "SetupEach",
                        "Act",
                        "TeardownEach",
                        "TeardownOnce");
                };
            };
        }
    }
}
