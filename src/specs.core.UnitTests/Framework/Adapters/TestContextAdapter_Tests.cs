using NFluent;
using specs.core.Framework.Adapters;
using yozepi;

namespace specs.core.UnitTests.Framework.Adapters
{
    public class TestContextAdapter_Tests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        private void TestContextAdapter_Specs()
        {
            TestContextAdapter subject = null!;

            Action setupOnceAction = null!;
            Action setupEachAction = null!;
            Action actAction = null!;
            Action teardownEachAction = null!;
            Action teardownOnceAction = null!;
            Action testAction = null!;

            List<string> callOrder = null!;

            beforeEach = () =>
            {
                callOrder = new List<string>();
                setupOnceAction = () => callOrder.Add("SetupOnce");
                setupEachAction = () => callOrder.Add("SetupEach");
                actAction = () => callOrder.Add("Act");
                teardownEachAction = () => callOrder.Add("TeardownEach");
                teardownOnceAction = () => callOrder.Add("TeardownOnce");
                testAction = () => callOrder.Add("Test");
            };

            describe["TestContextAdapter constructor"] = () =>
            {
                context["when all parameters are valid"] = () =>
                {
                    act = () =>
                    {
                        subject = new TestContextAdapter(
                            setupOnceAction,
                            setupEachAction,
                            actAction,
                            teardownEachAction,
                            teardownOnceAction,
                            testAction);
                    };

                    it["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    it["should implement ITestContextAdapter interface"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                        Check.That(subject is ITestContextAdapter).IsTrue();
                    };
                };

                context["when setupOnceAction is null"] = () =>
                {
                    it["should throw ArgumentNullException"] = () =>
                    {
                        ArgumentNullException exception = null!;
                        try
                        {
                            new TestContextAdapter(
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

                context["when setupEachAction is null"] = () =>
                {
                    it["should throw ArgumentNullException"] = () =>
                    {
                        ArgumentNullException exception = null!;
                        try
                        {
                            new TestContextAdapter(
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

                context["when actAction is null"] = () =>
                {
                    it["should throw ArgumentNullException"] = () =>
                    {
                        ArgumentNullException exception = null!;
                        try
                        {
                            new TestContextAdapter(
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

                context["when teardownEachAction is null"] = () =>
                {
                    it["should throw ArgumentNullException"] = () =>
                    {
                        ArgumentNullException exception = null!;
                        try
                        {
                            new TestContextAdapter(
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

                context["when teardownOnceAction is null"] = () =>
                {
                    it["should throw ArgumentNullException"] = () =>
                    {
                        ArgumentNullException exception = null!;
                        try
                        {
                            new TestContextAdapter(
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

                context["when test parameter is null"] = () =>
                {
                    it["should create instance successfully (test parameter is not validated)"] = () =>
                    {
                        Check.ThatCode(() => new TestContextAdapter(
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

            describe["SetupOnce method"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                };

                act = () =>
                {
                    subject.SetupOnce();
                };

                it["should invoke the setupOnceAction"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("SetupOnce");
                };

                it["should invoke setupOnceAction each time SetupOnce is called"] = () =>
                {
                    subject.SetupOnce();
                    subject.SetupOnce();
                    Check.That(callOrder).ContainsExactly("SetupOnce", "SetupOnce", "SetupOnce");
                };

                context["when setupOnceAction throws exception"] = () =>
                {
                    it["should propagate the exception"] = () =>
                    {
                        setupOnceAction = () => throw new InvalidOperationException("Setup failed");
                        subject = new TestContextAdapter(
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

            describe["SetupEach method"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                };

                act = () =>
                {
                    subject.SetupEach();
                };

                it["should invoke the setupEachAction"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("SetupEach");
                };

                it["should invoke setupEachAction each time SetupEach is called"] = () =>
                {
                    subject.SetupEach();
                    subject.SetupEach();
                    Check.That(callOrder).ContainsExactly("SetupEach", "SetupEach", "SetupEach");
                };

                context["when setupEachAction throws exception"] = () =>
                {
                    it["should propagate the exception"] = () =>
                    {
                        setupEachAction = () => throw new ArgumentException("SetupEach failed");
                        subject = new TestContextAdapter(
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

            describe["Act method"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                };

                act = () =>
                {
                    subject.Act();
                };

                it["should invoke the actAction"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("Act");
                };

                it["should invoke actAction each time Act is called"] = () =>
                {
                    subject.Act();
                    subject.Act();
                    Check.That(callOrder).ContainsExactly("Act", "Act", "Act");
                };

                context["when actAction throws exception"] = () =>
                {
                    it["should propagate the exception"] = () =>
                    {
                        actAction = () => throw new NotImplementedException("Act not implemented");
                        subject = new TestContextAdapter(
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

            describe["TeardownEach method"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                };

                act = () =>
                {
                    subject.TeardownEach();
                };

                it["should invoke the teardownEachAction"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("TeardownEach");
                };

                it["should invoke teardownEachAction each time TeardownEach is called"] = () =>
                {
                    subject.TeardownEach();
                    subject.TeardownEach();
                    Check.That(callOrder).ContainsExactly("TeardownEach", "TeardownEach", "TeardownEach");
                };

                context["when teardownEachAction throws exception"] = () =>
                {
                    it["should propagate the exception"] = () =>
                    {
                        teardownEachAction = () => throw new InvalidCastException("TeardownEach failed");
                        subject = new TestContextAdapter(
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

            describe["TeardownOnce method"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                };

                act = () =>
                {
                    subject.TeardownOnce();
                };

                it["should invoke the teardownOnceAction"] = () =>
                {
                    Check.That(callOrder).ContainsExactly("TeardownOnce");
                };

                it["should invoke teardownOnceAction each time TeardownOnce is called"] = () =>
                {
                    subject.TeardownOnce();
                    subject.TeardownOnce();
                    Check.That(callOrder).ContainsExactly("TeardownOnce", "TeardownOnce", "TeardownOnce");
                };

                context["when teardownOnceAction throws exception"] = () =>
                {
                    it["should propagate the exception"] = () =>
                    {
                        teardownOnceAction = () => throw new ObjectDisposedException("TeardownOnce failed");
                        subject = new TestContextAdapter(
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

            describe["method interaction and independence"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new TestContextAdapter(
                        setupOnceAction,
                        setupEachAction,
                        actAction,
                        teardownEachAction,
                        teardownOnceAction,
                        testAction);
                };

                it["should allow calling methods in any order"] = () =>
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

                it["should not affect other methods when one throws"] = () =>
                {
                    setupEachAction = () => throw new Exception("SetupEach throws");
                    subject = new TestContextAdapter(
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

            describe["real-world usage scenario"] = () =>
            {
                it["should support typical test lifecycle execution"] = () =>
                {
                    subject = new TestContextAdapter(
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

                it["should support multiple test executions with same setup/teardown"] = () =>
                {
                    subject = new TestContextAdapter(
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
