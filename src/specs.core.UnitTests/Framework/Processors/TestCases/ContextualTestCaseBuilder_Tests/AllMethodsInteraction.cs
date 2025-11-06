using NFluent;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    class AllMethodsInteraction : nSpecTestHarness
    {
        private void AllMethodsInteraction_Specs()
        {
            ContextualTestCaseBuilder subject = null!;
            IContextualTestCase subjectTestCase = null!;

            List<string> executionOrder = null!;
            Action labeledAction(string label) => () => executionOrder.Add(label);

            beforeEach = () =>
            {
                subject = new ContextualTestCaseBuilder();
                subjectTestCase = null!;
                executionOrder = [];
            };

            act = () =>
            {
                subjectTestCase = subject.Build();
                subjectTestCase.SetupOnce?.Invoke();
                subjectTestCase.SetupEach?.Invoke();
                subjectTestCase.Act?.Invoke();
                subjectTestCase.TeardownEach?.Invoke();
                subjectTestCase.TeardownOnce?.Invoke();
            };

            describe["when all methods (SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce) are defined..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupOnce(labeledAction("once-1"));
                    subject.AddSetupOnce(labeledAction("once-2"));
                    subject.AddSetupEach(labeledAction("each-1"));
                    subject.AddSetupEach(labeledAction("each-2"));
                    subject.AddAct(labeledAction("act-1"));
                    subject.AddAct(labeledAction("act-2"));
                    subject.AddTeardownEach(labeledAction("teardownEach-1"));
                    subject.AddTeardownEach(labeledAction("teardownEach-2"));
                    subject.AddTeardownOnce(labeledAction("teardownOnce-1"));
                    subject.AddTeardownOnce(labeledAction("teardownOnce-2"));
                };

                it["should execute in order: SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce"] = () =>
                {
                    string[] expected = { "once-1", "once-2", "each-1", "each-2", "act-1", "act-2", "teardownEach-1", "teardownEach-2", "teardownOnce-1", "teardownOnce-2" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            describe["invoking all methods multiple times..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupOnce(labeledAction("once"));
                    subject.AddSetupEach(labeledAction("each"));
                    subject.AddAct(labeledAction("act"));
                    subject.AddTeardownEach(labeledAction("teardownEach"));
                    subject.AddTeardownOnce(labeledAction("teardownOnce"));
                };

                act = () =>
                {
                    // Parent act already invoked once
                    // Invoke 2 more times
                    subjectTestCase.SetupOnce?.Invoke();
                    subjectTestCase.SetupEach?.Invoke();
                    subjectTestCase.Act?.Invoke();
                    subjectTestCase.TeardownEach?.Invoke();
                    subjectTestCase.TeardownOnce?.Invoke();
                    subjectTestCase.SetupOnce?.Invoke();
                    subjectTestCase.SetupEach?.Invoke();
                    subjectTestCase.Act?.Invoke();
                    subjectTestCase.TeardownEach?.Invoke();
                    subjectTestCase.TeardownOnce?.Invoke();
                };

                it["should run SetupOnce/TeardownOnce only once, but SetupEach/Act/TeardownEach every time"] = () =>
                {
                    // Parent act: once, each, act, teardownEach, teardownOnce
                    // This act: 2x (each, act, teardownEach) - once and teardownOnce don't run again
                    string[] expected = { "once", "each", "act", "teardownEach", "teardownOnce", "each", "act", "teardownEach", "each", "act", "teardownEach" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            describe["simulating realistic test execution flow..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupOnce(labeledAction("once"));
                    subject.AddSetupEach(labeledAction("each"));
                    subject.AddAct(labeledAction("act"));
                    subject.AddTeardownEach(labeledAction("teardownEach"));
                    subject.AddTeardownOnce(labeledAction("teardownOnce"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                    subject.AddTest("test3", () => executionOrder.Add("test3"));
                };

                act = () =>
                {
                    // Parent act already ran SetupOnce and TeardownOnce once
                    // Simulate realistic test execution
                    foreach (var test in subjectTestCase.Tests)
                    {
                        subjectTestCase.SetupEach?.Invoke();
                        subjectTestCase.Act?.Invoke();
                        test.test();
                        subjectTestCase.TeardownEach?.Invoke();
                    }
                    // TeardownOnce runs after all tests complete
                    subjectTestCase.TeardownOnce?.Invoke();
                };

                it["should run SetupOnce once at start, TeardownOnce once at end, and SetupEach+Act+Test+TeardownEach for each test"] = () =>
                {
                    // Parent act: once, each, act, teardownEach, teardownOnce
                    // This act: 3x (each, act, test, teardownEach), then nothing (teardownOnce already ran)
                    string[] expected = { "once", "each", "act", "teardownEach", "teardownOnce", "each", "act", "test1", "teardownEach", "each", "act", "test2", "teardownEach", "each", "act", "test3", "teardownEach" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            describe["parent-child inheritance with all methods..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                beforeEach = () =>
                {
                    subject.AddSetupOnce(labeledAction("setupOnce-parent"));
                    subject.AddSetupEach(labeledAction("setupEach-parent"));
                    subject.AddAct(labeledAction("act-parent"));
                    subject.AddTeardownEach(labeledAction("teardownEach-parent"));
                    subject.AddTeardownOnce(labeledAction("teardownOnce-parent"));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupOnce(labeledAction("setupOnce-child"));
                        child.AddSetupEach(labeledAction("setupEach-child"));
                        child.AddAct(labeledAction("act-child"));
                        child.AddTeardownEach(labeledAction("teardownEach-child"));
                        child.AddTeardownOnce(labeledAction("teardownOnce-child"));
                        childBuilder = child;
                    });
                };

                context["parent context..."] = () =>
                {
                    it["should only execute parent methods, not child"] = () =>
                    {
                        string[] expected = { "setupOnce-parent", "setupEach-parent", "act-parent", "teardownEach-parent", "teardownOnce-parent" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["child context..."] = () =>
                {
                    act = () =>
                    {
                        executionOrder.Clear();
                        childTestCase = childBuilder.Build();
                        childTestCase.SetupOnce?.Invoke();
                        childTestCase.SetupEach?.Invoke();
                        childTestCase.Act?.Invoke();
                        childTestCase.TeardownEach?.Invoke();
                        childTestCase.TeardownOnce?.Invoke();
                    };

                    it["should show different inheritance patterns: SetupOnce isolated, Each/Act/TeardownEach/TeardownOnce inherited (but idempotent), Teardowns in REVERSE"] = () =>
                    {
                        // SetupOnce: only child (isolated)
                        // SetupEach: parent then child (inherited)
                        // Act: parent then child (inherited)
                        // TeardownEach: child then parent (REVERSE order - inherited)
                        // TeardownOnce: only child (parent already ran in parent act - idempotent)
                        string[] expected = { "setupOnce-child", "setupEach-parent", "setupEach-child", "act-parent", "act-child", "teardownEach-child", "teardownEach-parent", "teardownOnce-child" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };
            };

            describe["multiple nesting levels with all methods..."] = () =>
            {
                IContextualTestCaseBuilder grandchildBuilder = null!;
                IContextualTestCase grandchildTestCase = null!;

                beforeEach = () =>
                {
                    subject.AddSetupOnce(labeledAction("setupOnce-grand"));
                    subject.AddSetupEach(labeledAction("setupEach-grand"));
                    subject.AddAct(labeledAction("act-grand"));
                    subject.AddTeardownEach(labeledAction("teardownEach-grand"));
                    subject.AddTeardownOnce(labeledAction("teardownOnce-grand"));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupOnce(labeledAction("setupOnce-parent"));
                        child.AddSetupEach(labeledAction("setupEach-parent"));
                        child.AddAct(labeledAction("act-parent"));
                        child.AddTeardownEach(labeledAction("teardownEach-parent"));
                        child.AddTeardownOnce(labeledAction("teardownOnce-parent"));

                        child.ChildTestCase("grandchild", grandchild =>
                        {
                            grandchild.AddSetupOnce(labeledAction("setupOnce-child"));
                            grandchild.AddSetupEach(labeledAction("setupEach-child"));
                            grandchild.AddAct(labeledAction("act-child"));
                            grandchild.AddTeardownEach(labeledAction("teardownEach-child"));
                            grandchild.AddTeardownOnce(labeledAction("teardownOnce-child"));
                            grandchildBuilder = grandchild;
                        });
                    });
                };

                act = () =>
                {
                    executionOrder.Clear();
                    grandchildTestCase = grandchildBuilder.Build();
                    grandchildTestCase.SetupOnce?.Invoke();
                    grandchildTestCase.SetupEach?.Invoke();
                    grandchildTestCase.Act?.Invoke();
                    grandchildTestCase.TeardownEach?.Invoke();
                    grandchildTestCase.TeardownOnce?.Invoke();
                };

                it["should show inheritance across 3 levels: SetupOnce isolated, Each/Act/TeardownEach/TeardownOnce inherited, Teardowns in REVERSE"] = () =>
                {
                    // SetupOnce: grandparent already ran in parent act, parent and child run now (isolated per level)
                    // SetupEach: grand → parent → child (all inherited)
                    // Act: grand → parent → child (all inherited)
                    // TeardownEach: child → parent → grand (REVERSE order - inherited)
                    // TeardownOnce: child → parent (REVERSE order - inherited like TeardownEach)
                    string[] expected = { "setupOnce-parent", "setupOnce-child", "setupEach-grand", "setupEach-parent", "setupEach-child", "act-grand", "act-parent", "act-child", "teardownEach-child", "teardownEach-parent", "teardownEach-grand", "teardownOnce-child", "teardownOnce-parent" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            describe["various combinations of methods..."] = () =>
            {
                context["only SetupOnce defined..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddSetupOnce(labeledAction("once"));
                    };

                    it["should execute SetupOnce only"] = () =>
                    {
                        string[] expected = { "once" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["only SetupEach defined..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddSetupEach(labeledAction("each"));
                    };

                    it["should execute SetupEach only"] = () =>
                    {
                        string[] expected = { "each" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["only Act defined..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddAct(labeledAction("act"));
                    };

                    it["should execute Act only"] = () =>
                    {
                        string[] expected = { "act" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["SetupOnce and SetupEach only..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddSetupOnce(labeledAction("once"));
                        subject.AddSetupEach(labeledAction("each"));
                    };

                    it["should execute in order: SetupOnce, SetupEach"] = () =>
                    {
                        string[] expected = { "once", "each" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["SetupOnce and Act only..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddSetupOnce(labeledAction("once"));
                        subject.AddAct(labeledAction("act"));
                    };

                    it["should execute in order: SetupOnce, Act"] = () =>
                    {
                        string[] expected = { "once", "act" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["SetupEach and Act only..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddSetupEach(labeledAction("each"));
                        subject.AddAct(labeledAction("act"));
                    };

                    it["should execute in order: SetupEach, Act"] = () =>
                    {
                        string[] expected = { "each", "act" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["only TeardownEach defined..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddTeardownEach(labeledAction("teardown"));
                    };

                    it["should execute TeardownEach only"] = () =>
                    {
                        string[] expected = { "teardown" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["SetupEach and TeardownEach only..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddSetupEach(labeledAction("each"));
                        subject.AddTeardownEach(labeledAction("teardown"));
                    };

                    it["should execute in order: SetupEach, TeardownEach"] = () =>
                    {
                        string[] expected = { "each", "teardown" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["Act and TeardownEach only..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddAct(labeledAction("act"));
                        subject.AddTeardownEach(labeledAction("teardownEach"));
                    };

                    it["should execute in order: Act, TeardownEach"] = () =>
                    {
                        string[] expected = { "act", "teardownEach" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["only TeardownOnce defined..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddTeardownOnce(labeledAction("teardownOnce"));
                    };

                    it["should execute TeardownOnce only"] = () =>
                    {
                        string[] expected = { "teardownOnce" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["SetupOnce and TeardownOnce only..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddSetupOnce(labeledAction("once"));
                        subject.AddTeardownOnce(labeledAction("teardownOnce"));
                    };

                    it["should execute in order: SetupOnce, TeardownOnce"] = () =>
                    {
                        string[] expected = { "once", "teardownOnce" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["Act and TeardownOnce only..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddAct(labeledAction("act"));
                        subject.AddTeardownOnce(labeledAction("teardownOnce"));
                    };

                    it["should execute in order: Act, TeardownOnce"] = () =>
                    {
                        string[] expected = { "act", "teardownOnce" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["TeardownEach and TeardownOnce only..."] = () =>
                {
                    beforeEach = () =>
                    {
                        subject.AddTeardownEach(labeledAction("teardownEach"));
                        subject.AddTeardownOnce(labeledAction("teardownOnce"));
                    };

                    it["should execute in order: TeardownEach, TeardownOnce"] = () =>
                    {
                        string[] expected = { "teardownEach", "teardownOnce" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["none defined..."] = () =>
                {
                    it["should execute nothing"] = () =>
                    {
                        Check.That(executionOrder).IsEmpty();
                    };
                };
            };

            describe["exception handling across methods..."] = () =>
            {
                IContextualTestCase exceptionTestCase = null!;

                context["when SetupOnce throws..."] = () =>
                {
                    beforeEach = () =>
                    {
                        var exceptionSubject = new ContextualTestCaseBuilder();
                        exceptionSubject.AddSetupOnce(labeledAction("once-1"));
                        exceptionSubject.AddSetupOnce(() => throw new InvalidOperationException("SetupOnce failed"));
                        exceptionSubject.AddSetupEach(labeledAction("each-1"));
                        exceptionSubject.AddAct(labeledAction("act-1"));
                        exceptionTestCase = exceptionSubject.Build();
                    };

                    it["should throw and not execute SetupEach or Act"] = () =>
                    {
                        executionOrder.Clear();
                        Check.ThatCode(() => exceptionTestCase.SetupOnce?.Invoke())
                            .Throws<InvalidOperationException>();

                        // SetupOnce threw, so only once-1 executed
                        string[] expected = { "once-1" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["when SetupEach throws..."] = () =>
                {
                    beforeEach = () =>
                    {
                        var exceptionSubject = new ContextualTestCaseBuilder();
                        exceptionSubject.AddSetupOnce(labeledAction("once-1"));
                        exceptionSubject.AddSetupEach(labeledAction("each-1"));
                        exceptionSubject.AddSetupEach(() => throw new InvalidOperationException("SetupEach failed"));
                        exceptionSubject.AddAct(labeledAction("act-1"));
                        exceptionTestCase = exceptionSubject.Build();
                    };

                    it["should execute SetupOnce, throw in SetupEach, not execute Act"] = () =>
                    {
                        executionOrder.Clear();
                        exceptionTestCase.SetupOnce?.Invoke();
                        Check.ThatCode(() => exceptionTestCase.SetupEach?.Invoke())
                            .Throws<InvalidOperationException>();

                        // SetupOnce succeeded, SetupEach threw
                        string[] expected = { "once-1", "each-1" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["when Act throws..."] = () =>
                {
                    beforeEach = () =>
                    {
                        var exceptionSubject = new ContextualTestCaseBuilder();
                        exceptionSubject.AddSetupOnce(labeledAction("once-1"));
                        exceptionSubject.AddSetupEach(labeledAction("each-1"));
                        exceptionSubject.AddAct(labeledAction("act-1"));
                        exceptionSubject.AddAct(() => throw new InvalidOperationException("Act failed"));
                        exceptionSubject.AddTeardownEach(labeledAction("teardown-1"));
                        exceptionTestCase = exceptionSubject.Build();
                    };

                    it["should execute SetupOnce and SetupEach, throw in Act, not execute TeardownEach"] = () =>
                    {
                        executionOrder.Clear();
                        exceptionTestCase.SetupOnce?.Invoke();
                        exceptionTestCase.SetupEach?.Invoke();
                        Check.ThatCode(() => exceptionTestCase.Act?.Invoke())
                            .Throws<InvalidOperationException>();

                        // SetupOnce and SetupEach succeeded, Act threw, TeardownEach not reached
                        string[] expected = { "once-1", "each-1", "act-1" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["when TeardownEach throws..."] = () =>
                {
                    beforeEach = () =>
                    {
                        var exceptionSubject = new ContextualTestCaseBuilder();
                        exceptionSubject.AddSetupOnce(labeledAction("once-1"));
                        exceptionSubject.AddSetupEach(labeledAction("each-1"));
                        exceptionSubject.AddAct(labeledAction("act-1"));
                        exceptionSubject.AddTeardownEach(labeledAction("teardownEach-1"));
                        exceptionSubject.AddTeardownEach(() => throw new InvalidOperationException("TeardownEach failed"));
                        exceptionSubject.AddTeardownOnce(labeledAction("teardownOnce-1"));
                        exceptionTestCase = exceptionSubject.Build();
                    };

                    it["should execute SetupOnce, SetupEach, Act, and partial TeardownEach, then throw (TeardownOnce not reached)"] = () =>
                    {
                        executionOrder.Clear();
                        exceptionTestCase.SetupOnce?.Invoke();
                        exceptionTestCase.SetupEach?.Invoke();
                        exceptionTestCase.Act?.Invoke();
                        Check.ThatCode(() => exceptionTestCase.TeardownEach?.Invoke())
                            .Throws<InvalidOperationException>();

                        // All succeeded except TeardownEach threw after teardownEach-1, TeardownOnce not reached
                        string[] expected = { "once-1", "each-1", "act-1", "teardownEach-1" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["when TeardownOnce throws..."] = () =>
                {
                    beforeEach = () =>
                    {
                        var exceptionSubject = new ContextualTestCaseBuilder();
                        exceptionSubject.AddSetupOnce(labeledAction("once-1"));
                        exceptionSubject.AddSetupEach(labeledAction("each-1"));
                        exceptionSubject.AddAct(labeledAction("act-1"));
                        exceptionSubject.AddTeardownEach(labeledAction("teardownEach-1"));
                        exceptionSubject.AddTeardownOnce(labeledAction("teardownOnce-1"));
                        exceptionSubject.AddTeardownOnce(() => throw new InvalidOperationException("TeardownOnce failed"));
                        exceptionTestCase = exceptionSubject.Build();
                    };

                    it["should execute all methods, then throw in TeardownOnce and allow retry"] = () =>
                    {
                        executionOrder.Clear();
                        exceptionTestCase.SetupOnce?.Invoke();
                        exceptionTestCase.SetupEach?.Invoke();
                        exceptionTestCase.Act?.Invoke();
                        exceptionTestCase.TeardownEach?.Invoke();
                        Check.ThatCode(() => exceptionTestCase.TeardownOnce?.Invoke())
                            .Throws<InvalidOperationException>();

                        // All succeeded except TeardownOnce threw after teardownOnce-1
                        string[] expected = { "once-1", "each-1", "act-1", "teardownEach-1", "teardownOnce-1" };
                        Check.That(executionOrder).ContainsExactly(expected);

                        // TeardownOnce should retry on next invocation (hasRunToCompletion stayed false)
                        executionOrder.Clear();
                        Check.ThatCode(() => exceptionTestCase.TeardownOnce?.Invoke())
                            .Throws<InvalidOperationException>();
                        string[] expectedRetry = { "teardownOnce-1" };
                        Check.That(executionOrder).ContainsExactly(expectedRetry);
                    };
                };
            };

            describe["sibling contexts with all methods..."] = () =>
            {
                IContextualTestCaseBuilder child1Builder = null!;
                IContextualTestCaseBuilder child2Builder = null!;
                IContextualTestCase child1TestCase = null!;
                IContextualTestCase child2TestCase = null!;

                beforeEach = () =>
                {
                    subject.AddSetupOnce(labeledAction("setupOnce-parent"));
                    subject.AddSetupEach(labeledAction("setupEach-parent"));
                    subject.AddAct(labeledAction("act-parent"));
                    subject.AddTeardownEach(labeledAction("teardownEach-parent"));
                    subject.AddTeardownOnce(labeledAction("teardownOnce-parent"));

                    subject.ChildTestCase("child1", child =>
                    {
                        child.AddSetupOnce(labeledAction("setupOnce-child1"));
                        child.AddSetupEach(labeledAction("setupEach-child1"));
                        child.AddAct(labeledAction("act-child1"));
                        child.AddTeardownEach(labeledAction("teardownEach-child1"));
                        child.AddTeardownOnce(labeledAction("teardownOnce-child1"));
                        child1Builder = child;
                    });

                    subject.ChildTestCase("child2", child =>
                    {
                        child.AddSetupOnce(labeledAction("setupOnce-child2"));
                        child.AddSetupEach(labeledAction("setupEach-child2"));
                        child.AddAct(labeledAction("act-child2"));
                        child.AddTeardownEach(labeledAction("teardownEach-child2"));
                        child.AddTeardownOnce(labeledAction("teardownOnce-child2"));
                        child2Builder = child;
                    });
                };

                context["first child..."] = () =>
                {
                    act = () =>
                    {
                        executionOrder.Clear();
                        child1TestCase = child1Builder.Build();
                        child1TestCase.SetupOnce?.Invoke();
                        child1TestCase.SetupEach?.Invoke();
                        child1TestCase.Act?.Invoke();
                        child1TestCase.TeardownEach?.Invoke();
                        child1TestCase.TeardownOnce?.Invoke();
                    };

                    it["should execute child1 methods: Once methods isolated, Each/Act inherited, TeardownEach in REVERSE, not child2"] = () =>
                    {
                        string[] expected = { "setupOnce-child1", "setupEach-parent", "setupEach-child1", "act-parent", "act-child1", "teardownEach-child1", "teardownEach-parent", "teardownOnce-child1" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                context["second child..."] = () =>
                {
                    act = () =>
                    {
                        executionOrder.Clear();
                        child2TestCase = child2Builder.Build();
                        child2TestCase.SetupOnce?.Invoke();
                        child2TestCase.SetupEach?.Invoke();
                        child2TestCase.Act?.Invoke();
                        child2TestCase.TeardownEach?.Invoke();
                        child2TestCase.TeardownOnce?.Invoke();
                    };

                    it["should execute child2 methods: Once methods isolated, Each/Act inherited, TeardownEach in REVERSE, not child1"] = () =>
                    {
                        string[] expected = { "setupOnce-child2", "setupEach-parent", "setupEach-child2", "act-parent", "act-child2", "teardownEach-child2", "teardownEach-parent", "teardownOnce-child2" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };
            };
        }
    }
}
