using NFluent;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    partial class ContextualTestCaseBuilder_Tests
    {
        [Context("AllMethodsInteraction Specs")]
        public void AllMethodsInteraction_Specs()
        {
            ContextualTestCaseBuilder subject = null!;
            IContextualTestCase subjectTestCase = null!;

            List<string> executionOrder = null!;
            Action labeledAction(string label) => () => executionOrder.Add(label);

            BeforeEach(() =>
            {
                subject = new ContextualTestCaseBuilder();
                subjectTestCase = null!;
                executionOrder = [];
            });

            Act(() =>
            {
                subjectTestCase = subject.Build();
                subjectTestCase.SetupOnce?.Invoke();
                subjectTestCase.SetupEach?.Invoke();
                subjectTestCase.Act?.Invoke();
                subjectTestCase.TeardownEach?.Invoke();
                subjectTestCase.TeardownOnce?.Invoke();
            });

            Describe["when all methods (SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce) are defined..."] = () =>
            {
                BeforeEach(() =>
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
                });

                It["should execute in order: SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce"] = () =>
                {
                    string[] expected = { "once-1", "once-2", "each-1", "each-2", "act-1", "act-2", "teardownEach-1", "teardownEach-2", "teardownOnce-1", "teardownOnce-2" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking all methods multiple times..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddSetupOnce(labeledAction("once"));
                    subject.AddSetupEach(labeledAction("each"));
                    subject.AddAct(labeledAction("act"));
                    subject.AddTeardownEach(labeledAction("teardownEach"));
                    subject.AddTeardownOnce(labeledAction("teardownOnce"));
                });

                Act(() =>
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
                });

                It["should run SetupOnce/TeardownOnce only once, but SetupEach/Act/TeardownEach every time"] = () =>
                {
                    // Parent act: once, each, act, teardownEach, teardownOnce
                    // This act: 2x (each, act, teardownEach) - once and teardownOnce don't run again
                    string[] expected = { "once", "each", "act", "teardownEach", "teardownOnce", "each", "act", "teardownEach", "each", "act", "teardownEach" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            Describe["simulating realistic test execution flow..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddSetupOnce(labeledAction("once"));
                    subject.AddSetupEach(labeledAction("each"));
                    subject.AddAct(labeledAction("act"));
                    subject.AddTeardownEach(labeledAction("teardownEach"));
                    subject.AddTeardownOnce(labeledAction("teardownOnce"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                    subject.AddTest("test3", () => executionOrder.Add("test3"));
                });

                Act(() =>
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
                });

                It["should run SetupOnce once at start, TeardownOnce once at end, and SetupEach+Act+Test+TeardownEach for each test"] = () =>
                {
                    // Parent act: once, each, act, teardownEach, teardownOnce
                    // This act: 3x (each, act, test, teardownEach), then nothing (teardownOnce already ran)
                    string[] expected = { "once", "each", "act", "teardownEach", "teardownOnce", "each", "act", "test1", "teardownEach", "each", "act", "test2", "teardownEach", "each", "act", "test3", "teardownEach" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            Describe["parent-child inheritance with all methods..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                BeforeEach(() =>
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
                });

                Context["parent context..."] = () =>
                {
                    It["should only execute parent methods, not child"] = () =>
                    {
                        string[] expected = { "setupOnce-parent", "setupEach-parent", "act-parent", "teardownEach-parent", "teardownOnce-parent" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["child context..."] = () =>
                {
                    Act(() =>
                    {
                        executionOrder.Clear();
                        childTestCase = childBuilder.Build();
                        childTestCase.SetupOnce?.Invoke();
                        childTestCase.SetupEach?.Invoke();
                        childTestCase.Act?.Invoke();
                        childTestCase.TeardownEach?.Invoke();
                        childTestCase.TeardownOnce?.Invoke();
                    });

                    It["should show different inheritance patterns: SetupOnce isolated, Each/Act/TeardownEach/TeardownOnce inherited (but idempotent), Teardowns in REVERSE"] = () =>
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

            Describe["multiple nesting levels with all methods..."] = () =>
            {
                IContextualTestCaseBuilder grandchildBuilder = null!;
                IContextualTestCase grandchildTestCase = null!;

                BeforeEach(() =>
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
                });

                Act(() =>
                {
                    executionOrder.Clear();
                    grandchildTestCase = grandchildBuilder.Build();
                    grandchildTestCase.SetupOnce?.Invoke();
                    grandchildTestCase.SetupEach?.Invoke();
                    grandchildTestCase.Act?.Invoke();
                    grandchildTestCase.TeardownEach?.Invoke();
                    grandchildTestCase.TeardownOnce?.Invoke();
                });

                It["should show inheritance across 3 levels: SetupOnce isolated, Each/Act/TeardownEach/TeardownOnce inherited, Teardowns in REVERSE"] = () =>
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

            Describe["various combinations of methods..."] = () =>
            {
                Context["only SetupOnce defined..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddSetupOnce(labeledAction("once"));
                    });

                    It["should execute SetupOnce only"] = () =>
                    {
                        string[] expected = { "once" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["only SetupEach defined..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddSetupEach(labeledAction("each"));
                    });

                    It["should execute SetupEach only"] = () =>
                    {
                        string[] expected = { "each" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["only Act defined..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddAct(labeledAction("act"));
                    });

                    It["should execute Act only"] = () =>
                    {
                        string[] expected = { "act" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["SetupOnce and SetupEach only..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddSetupOnce(labeledAction("once"));
                        subject.AddSetupEach(labeledAction("each"));
                    });

                    It["should execute in order: SetupOnce, SetupEach"] = () =>
                    {
                        string[] expected = { "once", "each" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["SetupOnce and Act only..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddSetupOnce(labeledAction("once"));
                        subject.AddAct(labeledAction("act"));
                    });

                    It["should execute in order: SetupOnce, Act"] = () =>
                    {
                        string[] expected = { "once", "act" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["SetupEach and Act only..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddSetupEach(labeledAction("each"));
                        subject.AddAct(labeledAction("act"));
                    });

                    It["should execute in order: SetupEach, Act"] = () =>
                    {
                        string[] expected = { "each", "act" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["only TeardownEach defined..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddTeardownEach(labeledAction("teardown"));
                    });

                    It["should execute TeardownEach only"] = () =>
                    {
                        string[] expected = { "teardown" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["SetupEach and TeardownEach only..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddSetupEach(labeledAction("each"));
                        subject.AddTeardownEach(labeledAction("teardown"));
                    });

                    It["should execute in order: SetupEach, TeardownEach"] = () =>
                    {
                        string[] expected = { "each", "teardown" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["Act and TeardownEach only..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddAct(labeledAction("act"));
                        subject.AddTeardownEach(labeledAction("teardownEach"));
                    });

                    It["should execute in order: Act, TeardownEach"] = () =>
                    {
                        string[] expected = { "act", "teardownEach" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["only TeardownOnce defined..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddTeardownOnce(labeledAction("teardownOnce"));
                    });

                    It["should execute TeardownOnce only"] = () =>
                    {
                        string[] expected = { "teardownOnce" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["SetupOnce and TeardownOnce only..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddSetupOnce(labeledAction("once"));
                        subject.AddTeardownOnce(labeledAction("teardownOnce"));
                    });

                    It["should execute in order: SetupOnce, TeardownOnce"] = () =>
                    {
                        string[] expected = { "once", "teardownOnce" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["Act and TeardownOnce only..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddAct(labeledAction("act"));
                        subject.AddTeardownOnce(labeledAction("teardownOnce"));
                    });

                    It["should execute in order: Act, TeardownOnce"] = () =>
                    {
                        string[] expected = { "act", "teardownOnce" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["TeardownEach and TeardownOnce only..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        subject.AddTeardownEach(labeledAction("teardownEach"));
                        subject.AddTeardownOnce(labeledAction("teardownOnce"));
                    });

                    It["should execute in order: TeardownEach, TeardownOnce"] = () =>
                    {
                        string[] expected = { "teardownEach", "teardownOnce" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["none defined..."] = () =>
                {
                    It["should execute nothing"] = () =>
                    {
                        Check.That(executionOrder).IsEmpty();
                    };
                };
            };

            Describe["exception handling across methods..."] = () =>
            {
                IContextualTestCase exceptionTestCase = null!;

                Context["when SetupOnce throws..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        var exceptionSubject = new ContextualTestCaseBuilder();
                        exceptionSubject.AddSetupOnce(labeledAction("once-1"));
                        exceptionSubject.AddSetupOnce(() => throw new InvalidOperationException("SetupOnce failed"));
                        exceptionSubject.AddSetupEach(labeledAction("each-1"));
                        exceptionSubject.AddAct(labeledAction("act-1"));
                        exceptionTestCase = exceptionSubject.Build();
                    });

                    It["should throw and not execute SetupEach or Act"] = () =>
                    {
                        executionOrder.Clear();
                        Check.ThatCode(() => exceptionTestCase.SetupOnce?.Invoke())
                            .Throws<InvalidOperationException>();

                        // SetupOnce threw, so only once-1 executed
                        string[] expected = { "once-1" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["when SetupEach throws..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        var exceptionSubject = new ContextualTestCaseBuilder();
                        exceptionSubject.AddSetupOnce(labeledAction("once-1"));
                        exceptionSubject.AddSetupEach(labeledAction("each-1"));
                        exceptionSubject.AddSetupEach(() => throw new InvalidOperationException("SetupEach failed"));
                        exceptionSubject.AddAct(labeledAction("act-1"));
                        exceptionTestCase = exceptionSubject.Build();
                    });

                    It["should execute SetupOnce, throw in SetupEach, not execute Act"] = () =>
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

                Context["when Act throws..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        var exceptionSubject = new ContextualTestCaseBuilder();
                        exceptionSubject.AddSetupOnce(labeledAction("once-1"));
                        exceptionSubject.AddSetupEach(labeledAction("each-1"));
                        exceptionSubject.AddAct(labeledAction("act-1"));
                        exceptionSubject.AddAct(() => throw new InvalidOperationException("Act failed"));
                        exceptionSubject.AddTeardownEach(labeledAction("teardown-1"));
                        exceptionTestCase = exceptionSubject.Build();
                    });

                    It["should execute SetupOnce and SetupEach, throw in Act, not execute TeardownEach"] = () =>
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

                Context["when TeardownEach throws..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        var exceptionSubject = new ContextualTestCaseBuilder();
                        exceptionSubject.AddSetupOnce(labeledAction("once-1"));
                        exceptionSubject.AddSetupEach(labeledAction("each-1"));
                        exceptionSubject.AddAct(labeledAction("act-1"));
                        exceptionSubject.AddTeardownEach(labeledAction("teardownEach-1"));
                        exceptionSubject.AddTeardownEach(() => throw new InvalidOperationException("TeardownEach failed"));
                        exceptionSubject.AddTeardownOnce(labeledAction("teardownOnce-1"));
                        exceptionTestCase = exceptionSubject.Build();
                    });

                    It["should execute SetupOnce, SetupEach, Act, and partial TeardownEach, then throw (TeardownOnce not reached)"] = () =>
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

                Context["when TeardownOnce throws..."] = () =>
                {
                    BeforeEach(() =>
                    {
                        var exceptionSubject = new ContextualTestCaseBuilder();
                        exceptionSubject.AddSetupOnce(labeledAction("once-1"));
                        exceptionSubject.AddSetupEach(labeledAction("each-1"));
                        exceptionSubject.AddAct(labeledAction("act-1"));
                        exceptionSubject.AddTeardownEach(labeledAction("teardownEach-1"));
                        exceptionSubject.AddTeardownOnce(labeledAction("teardownOnce-1"));
                        exceptionSubject.AddTeardownOnce(() => throw new InvalidOperationException("TeardownOnce failed"));
                        exceptionTestCase = exceptionSubject.Build();
                    });

                    It["should execute all methods, then throw in TeardownOnce and allow retry"] = () =>
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

            Describe["sibling contexts with all methods..."] = () =>
            {
                IContextualTestCaseBuilder child1Builder = null!;
                IContextualTestCaseBuilder child2Builder = null!;
                IContextualTestCase child1TestCase = null!;
                IContextualTestCase child2TestCase = null!;

                BeforeEach(() =>
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
                });

                Context["first child..."] = () =>
                {
                    Act(() =>
                    {
                        executionOrder.Clear();
                        child1TestCase = child1Builder.Build();
                        child1TestCase.SetupOnce?.Invoke();
                        child1TestCase.SetupEach?.Invoke();
                        child1TestCase.Act?.Invoke();
                        child1TestCase.TeardownEach?.Invoke();
                        child1TestCase.TeardownOnce?.Invoke();
                    });

                    It["should execute child1 methods: Once methods isolated, Each/Act inherited, TeardownEach in REVERSE, not child2"] = () =>
                    {
                        string[] expected = { "setupOnce-child1", "setupEach-parent", "setupEach-child1", "act-parent", "act-child1", "teardownEach-child1", "teardownEach-parent", "teardownOnce-child1" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };

                Context["second child..."] = () =>
                {
                    Act(() =>
                    {
                        executionOrder.Clear();
                        child2TestCase = child2Builder.Build();
                        child2TestCase.SetupOnce?.Invoke();
                        child2TestCase.SetupEach?.Invoke();
                        child2TestCase.Act?.Invoke();
                        child2TestCase.TeardownEach?.Invoke();
                        child2TestCase.TeardownOnce?.Invoke();
                    });

                    It["should execute child2 methods: Once methods isolated, Each/Act inherited, TeardownEach in REVERSE, not child1"] = () =>
                    {
                        string[] expected = { "setupOnce-child2", "setupEach-parent", "setupEach-child2", "act-parent", "act-child2", "teardownEach-child2", "teardownEach-parent", "teardownOnce-child2" };
                        Check.That(executionOrder).ContainsExactly(expected);
                    };
                };
            };
        }
    }
}
