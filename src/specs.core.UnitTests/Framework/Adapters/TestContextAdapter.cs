using NFluent;
using specs.core.Framework.Adapters;
using specs.BDD;
using specs.BDD.Attributes;

namespace specs.core.UnitTests.Framework.Adapters
{
    public class TestContextAdapter : Specifications
    {

        [Context]
        public void Specs()
        {
            core.Framework.Adapters.TestContextAdapter subject = null!;

            Action setupOnceAction = null!;
            Action setupEachAction = null!;
            Action actAction = null!;
            Action teardownEachAction = null!;
            Action teardownOnceAction = null!;
            Action testAction = null!;

            List<string> callOrder = null!;

            BeforeEach(() =>
            {
                callOrder = new List<string>();
                setupOnceAction = () => callOrder.Add("SetupOnce");
                setupEachAction = () => callOrder.Add("SetupEach");
                actAction = () => callOrder.Add("Act");
                teardownEachAction = () => callOrder.Add("TeardownEach");
                teardownOnceAction = () => callOrder.Add("TeardownOnce");
                testAction = () => callOrder.Add("Test");
            });

            Describe["TestContextAdapter constructor"] = () =>
            {
                Context["when all parameters are valid"] = () =>
                {
                    Act(() =>
                    {
                        subject = new core.Framework.Adapters.TestContextAdapter(
                            setupOnceAction,
                            setupEachAction,
                            actAction,
                            teardownEachAction,
                            teardownOnceAction,
                            testAction);
                    });

                    It["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    It["should implement ITestContextAdapter interface"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                        Check.That(subject is ITestContextAdapter).IsTrue();
                    };
                };

                Context["when setupOnceAction is null"] = () =>
                {
                    It["should throw ArgumentNullException"] = () =>
                    {
                        ArgumentNullException exception = null!;
                        try
                        {
                            new core.Framework.Adapters.TestContextAdapter(
                                null!,
                                setupEachAction,
                                actAction,
                                teardownEachAction,
                                teardownOnceAction,
                                testAction);
                        }
                        catch (ArgumentNullException ex)
                        {
                            exception = ex;
                        }
                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("setupOnceAction");
                    };
                };

                Context["when setupEachAction is null"] = () =>
                {
                    It["should throw ArgumentNullException"] = () =>
                    {
                        ArgumentNullException exception = null!;
                        try
                        {
                            new core.Framework.Adapters.TestContextAdapter(
                                setupOnceAction,
                                null!,
                                actAction,
                                teardownEachAction,
                                teardownOnceAction,
                                testAction);
                        }
                        catch (ArgumentNullException ex)
                        {
                            exception = ex;
                        }
                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("setupEachAction");
                    };
                };

                Context["when actAction is null"] = () =>
                {
                    It["should throw ArgumentNullException"] = () =>
                    {
                        ArgumentNullException exception = null!;
                        try
                        {
                            new core.Framework.Adapters.TestContextAdapter(
                                setupOnceAction,
                                setupEachAction,
                                null!,
                                teardownEachAction,
                                teardownOnceAction,
                                testAction);
                        }
                        catch (ArgumentNullException ex)
                        {
                            exception = ex;
                        }
                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("actAction");
                    };
                };

                Context["when teardownEachAction is null"] = () =>
                {
                    It["should throw ArgumentNullException"] = () =>
                    {
                        ArgumentNullException exception = null!;
                        try
                        {
                            new core.Framework.Adapters.TestContextAdapter(
                                setupOnceAction,
                                setupEachAction,
                                actAction,
                                null!,
                                teardownOnceAction,
                                testAction);
                        }
                        catch (ArgumentNullException ex)
                        {
                            exception = ex;
                        }
                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("teardownEachAction");
                    };
                };

                Context["when teardownOnceAction is null"] = () =>
                {
                    It["should throw ArgumentNullException"] = () =>
                    {
                        ArgumentNullException exception = null!;
                        try
                        {
                            new core.Framework.Adapters.TestContextAdapter(
                                setupOnceAction,
                                setupEachAction,
                                actAction,
                                teardownEachAction,
                                null!,
                                testAction);
                        }
                        catch (ArgumentNullException ex)
                        {
                            exception = ex;
                        }
                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("teardownOnceAction");
                    };
                };

                Context["when test parameter is null"] = () =>
                {
                    It["should create instance successfully (test parameter is not validated)"] = () =>
                    {
                        Check.ThatCode(() => new specs.core.Framework.Adapters.TestContextAdapter(
                            setupOnceAction,
                            setupEachAction,
                            actAction,
                            teardownEachAction,
                            teardownOnceAction,
                            null!))
                        .DoesNotThrow();
                    };
                };
            };

            Describe["SetupOnce method"] = () =>
            {
                BeforeEach(() =>
                {
                    subject = new core.Framework.Adapters.TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                });

                Act(() =>
                {
                    subject.SetupOnce();
                });

                It["should invoke the setupOnceAction"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("SetupOnce");
                };

                It["should invoke setupOnceAction each time SetupOnce is called"] = () =>
                {
                    subject.SetupOnce();
                    subject.SetupOnce();
                    Check.That(callOrder).ContainsExactly("SetupOnce", "SetupOnce", "SetupOnce");
                };

                Context["when setupOnceAction throws exception"] = () =>
                {
                    It["should propagate the exception"] = () =>
                    {
                        setupOnceAction = () => throw new InvalidOperationException("Setup failed");
                        subject = new core.Framework.Adapters.TestContextAdapter(
                            setupOnceAction,
                            setupEachAction,
                            actAction,
                            teardownEachAction,
                            teardownOnceAction,
                            testAction);

                        Check.ThatCode(() => subject.SetupOnce())
                            .Throws<InvalidOperationException>()
                            .WithMessage("Setup failed");
                    };
                };
            };

            Describe["SetupEach method"] = () =>
            {
                BeforeEach(() =>
                {
                    subject = new core.Framework.Adapters.TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                });

                Act(() =>
                {
                    subject.SetupEach();
                });

                It["should invoke the setupEachAction"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("SetupEach");
                };

                It["should invoke setupEachAction each time SetupEach is called"] = () =>
                {
                    subject.SetupEach();
                    subject.SetupEach();
                    Check.That(callOrder).ContainsExactly("SetupEach", "SetupEach", "SetupEach");
                };

                Context["when setupEachAction throws exception"] = () =>
                {
                    It["should propagate the exception"] = () =>
                    {
                        setupEachAction = () => throw new ArgumentException("SetupEach failed");
                        subject = new core.Framework.Adapters.TestContextAdapter(
                            setupOnceAction,
                            setupEachAction,
                            actAction,
                            teardownEachAction,
                            teardownOnceAction,
                            testAction);

                        Check.ThatCode(() => subject.SetupEach())
                            .Throws<ArgumentException>()
                            .WithMessage("SetupEach failed");
                    };
                };
            };

            Describe["Act method"] = () =>
            {
                BeforeEach(() =>
                {
                    subject = new core.Framework.Adapters.TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                });

                Act(() =>
                {
                    subject.Act();
                });

                It["should invoke the actAction"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("Act");
                };

                It["should invoke actAction each time Act is called"] = () =>
                {
                    subject.Act();
                    subject.Act();
                    Check.That(callOrder).ContainsExactly("Act", "Act", "Act");
                };

                Context["when actAction throws exception"] = () =>
                {
                    It["should propagate the exception"] = () =>
                    {
                        actAction = () => throw new NotImplementedException("Act not implemented");
                        subject = new core.Framework.Adapters.TestContextAdapter(
                            setupOnceAction,
                            setupEachAction,
                            actAction,
                            teardownEachAction,
                            teardownOnceAction,
                            testAction);

                        Check.ThatCode(() => subject.Act())
                            .Throws<NotImplementedException>()
                            .WithMessage("Act not implemented");
                    };
                };
            };

            Describe["TeardownEach method"] = () =>
            {
                BeforeEach(() =>
                {
                    subject = new core.Framework.Adapters.TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                });

                Act(() =>
                {
                    subject.TeardownEach();
                });

                It["should invoke the teardownEachAction"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("TeardownEach");
                };

                It["should invoke teardownEachAction each time TeardownEach is called"] = () =>
                {
                    subject.TeardownEach();
                    subject.TeardownEach();
                    Check.That(callOrder).ContainsExactly("TeardownEach", "TeardownEach", "TeardownEach");
                };

                Context["when teardownEachAction throws exception"] = () =>
                {
                    It["should propagate the exception"] = () =>
                    {
                        teardownEachAction = () => throw new InvalidCastException("TeardownEach failed");
                        subject = new core.Framework.Adapters.TestContextAdapter(
                            setupOnceAction,
                            setupEachAction,
                            actAction,
                            teardownEachAction,
                            teardownOnceAction,
                            testAction);

                        Check.ThatCode(() => subject.TeardownEach())
                            .Throws<InvalidCastException>()
                            .WithMessage("TeardownEach failed");
                    };
                };
            };

            Describe["TeardownOnce method"] = () =>
            {
                BeforeEach(() =>
                {
                    subject = new core.Framework.Adapters.TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                });

                Act(() =>
                {
                    subject.TeardownOnce();
                });

                It["should invoke the teardownOnceAction"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("TeardownOnce");
                };

                It["should invoke teardownOnceAction each time TeardownOnce is called"] = () =>
                {
                    subject.TeardownOnce();
                    subject.TeardownOnce();
                    Check.That(callOrder).ContainsExactly("TeardownOnce", "TeardownOnce", "TeardownOnce");
                };

                Context["when teardownOnceAction throws exception"] = () =>
                {
                    It["should propagate the exception"] = () =>
                    {
                        teardownOnceAction = () => throw new ObjectDisposedException("TeardownOnce failed");
                        subject = new core.Framework.Adapters.TestContextAdapter(
                            setupOnceAction,
                            setupEachAction,
                            actAction,
                            teardownEachAction,
                            teardownOnceAction,
                            testAction);

                        Check.ThatCode(() => subject.TeardownOnce())
                            .Throws<ObjectDisposedException>();
                    };
                };
            };

            Describe["method interaction and independence"] = () =>
            {
                BeforeEach(() =>
                {
                    subject = new core.Framework.Adapters.TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                });

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
                    subject = new core.Framework.Adapters.TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);

                    subject.SetupOnce();
                    try { subject.SetupEach(); } catch { }
                    subject.Act();

                    Check.That(callOrder).ContainsExactly("SetupOnce", "Act");
                };
            };

            Describe["real-world usage scenario"] = () =>
            {
                It["should support typical test lifecycle execution"] = () =>
                {
                    subject = new core.Framework.Adapters.TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);

                    // Typical test lifecycle
                    subject.SetupOnce();
                    subject.SetupEach();
                    subject.Act();
                    testAction(); // Test would be invoked externally
                    subject.TeardownEach();
                    subject.TeardownOnce();

                    Check.That(callOrder).ContainsExactly(
                        "SetupOnce",
                        "SetupEach",
                        "Act",
                        "Test",
                        "TeardownEach",
                        "TeardownOnce");
                };

                It["should support multiple test executions with same setup/teardown"] = () =>
                {
                    subject = new core.Framework.Adapters.TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);

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
