using NFluent;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public partial class ContextualTestCaseBuilder_Tests
    {
        [Describe("SetUpEach Specs")]
        public void SetUpEach_Specs()
        {
            ContextualTestCaseBuilder subject = null!;
            IContextualTestCase subjectTestCase = null!;

            List<int> callOrder = null!;
            Action startAction(int n) => () => callOrder.Add(n);

            BeforeEach(() =>
            {
                subject = new ContextualTestCaseBuilder();
                subjectTestCase = null!;

                callOrder = [];
            });

            Act(() =>
            {
                subjectTestCase = subject.Build();
                subjectTestCase.SetupEach?.Invoke();
            });

            Describe["invoking SetupEach after adding an action..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddSetupEach(startAction(1));
                });

                It["should invoke the action"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking SetupEach after adding multiple actions..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddSetupEach(startAction(1));
                    subject.AddSetupEach(startAction(2));
                    subject.AddSetupEach(startAction(3));
                });

                It["should invoke the actions in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking SetupEach after adding a Func<Task>..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddSetupEach(async () => await Task.Run(() => callOrder.Add(1)));
                });
                It["should invoke the Func<Task>"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking SetupEach after adding multiple actions and Func<Task>'s..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddSetupEach(startAction(1));
                    subject.AddSetupEach(() => Task.Run(startAction(2)));
                    subject.AddSetupEach(startAction(3));
                    subject.AddSetupEach(() => Task.Run(startAction(4)));
                });

                It["should invoke the actions and Func<Task>'s in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3, 4 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking SetupEach multiple times..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddSetupEach(startAction(10));
                    subject.AddSetupEach(startAction(20));
                    subject.AddSetupEach(startAction(30));
                });

                Act(() =>
                {
                    subjectTestCase.SetupEach?.Invoke();
                    subjectTestCase.SetupEach?.Invoke();
                    subjectTestCase.SetupEach?.Invoke();
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
                    subject.AddSetupEach(startAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupEach(startAction(200));
                        child.AddSetupEach(startAction(300));
                        child.AddSetupEach(startAction(400));
                        childTestCaseBuilder = child;
                    });
                });

                Context["when the parent context setupEach has run it's actions..."] = () =>
                {
                    Describe["the parent context..."] = () =>
                    {
                        It["should only invoke the parent context's SetupEach instance and not the child's"] = () =>
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
                            childTestCase.SetupEach?.Invoke();
                        });

                        It["should invoke both the parent context's SetupEach and the child's SetupEach instances"] = () =>
                        {
                            int[] expected = { 100, 200, 300, 400};
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };

                Context["when the parent context setupEach has NOT run it's actions..."] = () =>
                {
                    Describe["the parent context..."] = () =>
                    {
                        It["should only invoke the parent context's SetupEach instance and not the child's"] = () =>
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
                            childTestCase.SetupEach?.Invoke();
                        });

                        It["should invoke both the parent context's SetupEach and the child's SetupEach instances"] = () =>
                        {
                            int[] expected = { 100, 200, 300, 400 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };
            };

            Describe["when no actions are added..."] = () =>
            {
                It["should have an empty SetupEach that does nothing"] = () =>
                {
                    // SetupEach exists but does nothing
                    Check.ThatCode(() => subjectTestCase.SetupEach?.Invoke()).DoesNotThrow();
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
                    exceptionSubject.AddSetupEach(startAction(1));
                    exceptionSubject.AddSetupEach(() => throw thrownException);
                    exceptionSubject.AddSetupEach(startAction(3));
                    exceptionTestCase = exceptionSubject.Build();
                });

                It["should throw the exception when invoked"] = () =>
                {
                    Check.ThatCode(() => exceptionTestCase.SetupEach?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };

                It["should have executed actions before the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.SetupEach?.Invoke(); } catch { }
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                It["should not execute actions after the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.SetupEach?.Invoke(); } catch { }
                    Check.That(callOrder).Not.Contains(3);
                };

                It["should still throw on subsequent invocations"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.SetupEach?.Invoke(); } catch { }

                    Check.ThatCode(() => exceptionTestCase.SetupEach?.Invoke())
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
                    subject.AddSetupEach(startAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupEach(startAction(200));

                        child.ChildTestCase("grandchild", grandchild =>
                        {
                            grandchild.AddSetupEach(startAction(300));
                            grandchild.AddSetupEach(startAction(400));
                            grandchildBuilder = grandchild;
                        });

                        childBuilder = child;
                    });
                });

                Act(() =>
                {
                    callOrder.Clear();
                    grandchildTestCase = grandchildBuilder.Build();
                    grandchildTestCase.SetupEach?.Invoke();
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
                    // Parent has no SetupEach actions
                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupEach(startAction(1));
                        child.AddSetupEach(startAction(2));
                        childBuilder = child;
                    });
                });

                Act(() =>
                {
                    callOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.SetupEach?.Invoke();
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
                    subject.AddSetupEach(startAction(100));

                    subject.ChildTestCase("child1", child =>
                    {
                        child.AddSetupEach(startAction(200));
                        child1Builder = child;
                    });

                    subject.ChildTestCase("child2", child =>
                    {
                        child.AddSetupEach(startAction(300));
                        child2Builder = child;
                    });
                });

                Context["first child..."] = () =>
                {
                    Act(() =>
                    {
                        callOrder.Clear();
                        child1TestCase = child1Builder.Build();
                        child1TestCase.SetupEach?.Invoke();
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
                        child2TestCase.SetupEach?.Invoke();
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
                    subject.AddSetupEach(startAction(1));
                    subject.AddSetupEach(startAction(2));
                });

                Act(() =>
                {
                    callOrder.Clear();
                    firstBuild = subject.Build();
                    secondBuild = subject.Build();

                    firstBuild.SetupEach?.Invoke();
                    secondBuild.SetupEach?.Invoke();
                });

                It["should execute independently for each build"] = () =>
                {
                    int[] expected = { 1, 2, 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["SetupEach with multiple tests..."] = () =>
            {
                List<string> executionOrder = null!;

                BeforeEach(() =>
                {
                    executionOrder = [];
                    subject.AddSetupEach(() => executionOrder.Add("setup"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                    subject.AddTest("test3", () => executionOrder.Add("test3"));
                });

                Act(() =>
                {
                    executionOrder.Clear();
                    var testCase = subject.Build();
                    // Simulate running each test with SetupEach
                    foreach (var test in testCase.Tests)
                    {
                        testCase.SetupEach?.Invoke();
                        test.test();
                    }
                });

                It["should run SetupEach before each test"] = () =>
                {
                    string[] expected = { "setup", "test1", "setup", "test2", "setup", "test3" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };
        }
    }
}
