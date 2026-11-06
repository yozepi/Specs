using NFluent;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    partial class ContextualTestCaseBuilder_Tests
    {
        [Feature("Act Specs")]
        public void Act_Specs()
        {
            ContextualTestCaseBuilder subject = null!;
            IContextualTestCase subjectTestCase = null!;

            List<int> callOrder = null!;
            Action actAction(int n) => () => callOrder.Add(n);

            BeforeEach(() =>
            {
                subject = new ContextualTestCaseBuilder();
                subjectTestCase = null!;

                callOrder = [];
            });

            Act(() =>
            {
                subjectTestCase = subject.Build();
                subjectTestCase.Act?.Invoke();
            });

            Describe["invoking Act after adding an action..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddAct(actAction(1));
                });

                It["should invoke the action"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking Act after adding multiple actions..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddAct(actAction(1));
                    subject.AddAct(actAction(2));
                    subject.AddAct(actAction(3));
                });

                It["should invoke the actions in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking Act after adding a Func<Task>..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddAct(async () => await Task.Run(() => callOrder.Add(1)));
                });
                It["should invoke the Func<Task>"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking Act after adding multiple actions and Func<Task>'s..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddAct(actAction(1));
                    subject.AddAct(() => Task.Run(actAction(2)));
                    subject.AddAct(actAction(3));
                    subject.AddAct(() => Task.Run(actAction(4)));
                });

                It["should invoke the actions and Func<Task>'s in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3, 4 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking Act multiple times..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddAct(actAction(10));
                    subject.AddAct(actAction(20));
                    subject.AddAct(actAction(30));
                });

                Act(() =>
                {
                    subjectTestCase.Act?.Invoke();
                    subjectTestCase.Act?.Invoke();
                    subjectTestCase.Act?.Invoke();
                });
                It["should run the actions each time it is invoked"] = () =>
                {
                    int[] expected = { 10, 20, 30, 10, 20, 30, 10, 20, 30, 10, 20, 30 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["child test case handling "] = () =>
            {
                IContextualTestCaseBuilder childTestCaseBuilder = null!;
                IContextualTestCase childTestCase = null!;

                BeforeEach(() =>
                {
                    subject.AddAct(actAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddAct(actAction(200));
                        child.AddAct(actAction(300));
                        child.AddAct(actAction(400));
                        childTestCaseBuilder = child;
                    });
                });

                Context["when the parent context act has run it's actions..."] = () =>
                {
                    Describe["the parent context..."] = () =>
                    {
                        It["should only invoke the parent context's Act instance and not the child's"] = () =>
                        {
                            int[] expected = { 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                    Context["the child context..."] = () =>
                    {

                        Act(() =>
                        {
                            callOrder.Clear();
                            childTestCase = childTestCaseBuilder.Build();
                            childTestCase.Act?.Invoke();
                        });

                        It["should invoke both the parent context's Act and the child's Act instances"] = () =>
                        {
                            int[] expected = { 100, 200, 300, 400};
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };

                Context["when the parent context act has NOT run it's actions..."] = () =>
                {
                    Describe["the parent context..."] = () =>
                    {
                        It["should only invoke the parent context's Act instance and not the child's"] = () =>
                        {
                            int[] expected = { 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                    Context["the child context..."] = () =>
                    {

                        Act(() =>
                        {
                            callOrder.Clear();
                            childTestCase = childTestCaseBuilder.Build();
                            childTestCase.Act?.Invoke();
                        });

                        It["should invoke both the parent context's Act and the child's Act instances"] = () =>
                        {
                            int[] expected = { 100, 200, 300, 400 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };
            };

            Describe["when no actions are added..."] = () =>
            {
                It["should have an empty Act that does nothing"] = () =>
                {
                    // Act exists but does nothing
                    Check.ThatCode(() => subjectTestCase.Act?.Invoke()).DoesNotThrow();
                    Check.That(callOrder).IsEmpty();
                };
            };

            Describe["exception handling..."] = () =>
            {
                Exception thrownException = null!;
                IContextualTestCase exceptionTestCase = null!;

                BeforeEach(() =>
                {
                    thrownException = new InvalidOperationException("Test exception");
                    var exceptionSubject = new ContextualTestCaseBuilder();
                    exceptionSubject.AddAct(actAction(1));
                    exceptionSubject.AddAct(() => throw thrownException);
                    exceptionSubject.AddAct(actAction(3));
                    exceptionTestCase = exceptionSubject.Build();
                });

                It["should throw the exception when invoked"] = () =>
                {
                    Check.ThatCode(() => exceptionTestCase.Act?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };

                It["should have executed actions before the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.Act?.Invoke(); } catch { }
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                It["should not execute actions after the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.Act?.Invoke(); } catch { }
                    Check.That(callOrder).Not.Contains(3);
                };

                It["should still throw on subsequent invocations"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.Act?.Invoke(); } catch { }

                    Check.ThatCode(() => exceptionTestCase.Act?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };
            };

            Describe["multiple levels of nesting..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCaseBuilder grandchildBuilder = null!;
                IContextualTestCase grandchildTestCase = null!;

                BeforeEach(() =>
                {
                    subject.AddAct(actAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddAct(actAction(200));

                        child.ChildTestCase("grandchild", grandchild =>
                        {
                            grandchild.AddAct(actAction(300));
                            grandchild.AddAct(actAction(400));
                            grandchildBuilder = grandchild;
                        });

                        childBuilder = child;
                    });
                });

                Act(() =>
                {
                    callOrder.Clear();
                    grandchildTestCase = grandchildBuilder.Build();
                    grandchildTestCase.Act?.Invoke();
                });

                It["should invoke grandparent, parent, and grandchild actions in order"] = () =>
                {
                    int[] expected = { 100, 200, 300, 400 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["parent with no actions, child with actions..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                BeforeEach(() =>
                {
                    // Parent has no Act actions
                    subject.ChildTestCase("child", child =>
                    {
                        child.AddAct(actAction(1));
                        child.AddAct(actAction(2));
                        childBuilder = child;
                    });
                });

                Act(() =>
                {
                    callOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.Act?.Invoke();
                });

                It["should invoke only the child's actions"] = () =>
                {
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["multiple sibling children..."] = () =>
            {
                IContextualTestCaseBuilder child1Builder = null!;
                IContextualTestCaseBuilder child2Builder = null!;
                IContextualTestCase child1TestCase = null!;
                IContextualTestCase child2TestCase = null!;

                BeforeEach(() =>
                {
                    subject.AddAct(actAction(100));

                    subject.ChildTestCase("child1", child =>
                    {
                        child.AddAct(actAction(200));
                        child1Builder = child;
                    });

                    subject.ChildTestCase("child2", child =>
                    {
                        child.AddAct(actAction(300));
                        child2Builder = child;
                    });
                });

                Context["first child..."] = () =>
                {
                    Act(() =>
                    {
                        callOrder.Clear();
                        child1TestCase = child1Builder.Build();
                        child1TestCase.Act?.Invoke();
                    });

                    It["should invoke parent and first child actions only"] = () =>
                    {
                        int[] expected = { 100, 200 };
                        Check.That(callOrder).ContainsExactly(expected);
                    };
                };

                Context["second child..."] = () =>
                {
                    Act(() =>
                    {
                        callOrder.Clear();
                        child2TestCase = child2Builder.Build();
                        child2TestCase.Act?.Invoke();
                    });

                    It["should invoke parent and second child actions only"] = () =>
                    {
                        int[] expected = { 100, 300 };
                        Check.That(callOrder).ContainsExactly(expected);
                    };
                };
            };

            Describe["building multiple times from the same builder..."] = () =>
            {
                IContextualTestCase firstBuild = null!;
                IContextualTestCase secondBuild = null!;

                BeforeEach(() =>
                {
                    subject.AddAct(actAction(1));
                    subject.AddAct(actAction(2));
                });

                Act(() =>
                {
                    callOrder.Clear();
                    firstBuild = subject.Build();
                    secondBuild = subject.Build();

                    firstBuild.Act?.Invoke();
                    secondBuild.Act?.Invoke();
                });

                It["should execute independently for each build"] = () =>
                {
                    int[] expected = { 1, 2, 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };
        }
    }
}
