using NFluent;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public partial class ContextualTestCaseBuilder_Tests
    {
        [Context("TeardownEach Specs")]
        public void TeardownEach_Specs()
        {
            ContextualTestCaseBuilder subject = null!;
            IContextualTestCase subjectTestCase = null!;

            List<int> callOrder = null!;
            Action teardownAction(int n) => () => callOrder.Add(n);

            BeforeEach(() =>
            {
                subject = new ContextualTestCaseBuilder();
                subjectTestCase = null!;

                callOrder = [];
            });

            Act(() =>
            {
                subjectTestCase = subject.Build();
                subjectTestCase.TeardownEach?.Invoke();
            });

            Describe["invoking TeardownEach after adding an action..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTeardownEach(teardownAction(1));
                });

                It["should invoke the action"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking TeardownEach after adding multiple actions..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTeardownEach(teardownAction(1));
                    subject.AddTeardownEach(teardownAction(2));
                    subject.AddTeardownEach(teardownAction(3));
                });

                It["should invoke the actions in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking TeardownEach after adding a Func<Task>..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTeardownEach(async () => await Task.Run(() => callOrder.Add(1)));
                });
                It["should invoke the Func<Task>"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking TeardownEach after adding multiple actions and Func<Task>'s..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTeardownEach(teardownAction(1));
                    subject.AddTeardownEach(() => Task.Run(teardownAction(2)));
                    subject.AddTeardownEach(teardownAction(3));
                    subject.AddTeardownEach(() => Task.Run(teardownAction(4)));
                });

                It["should invoke the actions and Func<Task>'s in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3, 4 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking TeardownEach multiple times..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTeardownEach(teardownAction(10));
                    subject.AddTeardownEach(teardownAction(20));
                    subject.AddTeardownEach(teardownAction(30));
                });

                Act(() =>
                {
                    subjectTestCase.TeardownEach?.Invoke();
                    subjectTestCase.TeardownEach?.Invoke();
                    subjectTestCase.TeardownEach?.Invoke();
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
                    subject.AddTeardownEach(teardownAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddTeardownEach(teardownAction(200));
                        child.AddTeardownEach(teardownAction(300));
                        child.AddTeardownEach(teardownAction(400));
                        childTestCaseBuilder = child;
                    });
                });

                Context["when the parent context TeardownEach has run it's actions..."] = () =>
                {
                    Describe["the parent context..."] = () =>
                    {
                        It["should only invoke the parent context's TeardownEach instance and not the child's"] = () =>
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
                            childTestCase.TeardownEach?.Invoke();
                        });

                        It["should invoke child's TeardownEach first, then parent's (REVERSE order)"] = () =>
                        {
                            // CRITICAL: TeardownEach runs in REVERSE order (child first, then parent)
                            // This is opposite of SetupEach
                            int[] expected = { 200, 300, 400, 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };

                Context["when the parent context TeardownEach has NOT run it's actions..."] = () =>
                {
                    Describe["the parent context..."] = () =>
                    {
                        It["should only invoke the parent context's TeardownEach instance and not the child's"] = () =>
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
                            childTestCase.TeardownEach?.Invoke();
                        });

                        It["should invoke child's TeardownEach first, then parent's (REVERSE order)"] = () =>
                        {
                            // CRITICAL: TeardownEach runs in REVERSE order (child first, then parent)
                            int[] expected = { 200, 300, 400, 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };
            };

            Describe["when no actions are added..."] = () =>
            {
                It["should have an empty TeardownEach that does nothing"] = () =>
                {
                    // TeardownEach exists but does nothing
                    Check.ThatCode(() => subjectTestCase.TeardownEach?.Invoke()).DoesNotThrow();
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
                    exceptionSubject.AddTeardownEach(teardownAction(1));
                    exceptionSubject.AddTeardownEach(() => throw thrownException);
                    exceptionSubject.AddTeardownEach(teardownAction(3));
                    exceptionTestCase = exceptionSubject.Build();
                });

                It["should throw the exception when invoked"] = () =>
                {
                    Check.ThatCode(() => exceptionTestCase.TeardownEach?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };

                It["should have executed actions before the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownEach?.Invoke(); } catch { }
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                It["should not execute actions after the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownEach?.Invoke(); } catch { }
                    Check.That(callOrder).Not.Contains(3);
                };

                It["should still throw on subsequent invocations"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownEach?.Invoke(); } catch { }

                    Check.ThatCode(() => exceptionTestCase.TeardownEach?.Invoke())
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
                    subject.AddTeardownEach(teardownAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddTeardownEach(teardownAction(200));

                        child.ChildTestCase("grandchild", grandchild =>
                        {
                            grandchild.AddTeardownEach(teardownAction(300));
                            grandchild.AddTeardownEach(teardownAction(400));
                            grandchildBuilder = grandchild;
                        });

                        childBuilder = child;
                    });
                });

                Act(() =>
                {
                    callOrder.Clear();
                    grandchildTestCase = grandchildBuilder.Build();
                    grandchildTestCase.TeardownEach?.Invoke();
                });

                It["should invoke in REVERSE order: grandchild, parent, grandparent"] = () =>
                {
                    // TeardownEach runs DEEPEST FIRST, then up the chain
                    int[] expected = { 300, 400, 200, 100 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["parent with no actions, child with actions..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                BeforeEach(() =>
                {
                    // Parent has no TeardownEach actions
                    subject.ChildTestCase("child", child =>
                    {
                        child.AddTeardownEach(teardownAction(1));
                        child.AddTeardownEach(teardownAction(2));
                        childBuilder = child;
                    });
                });

                Act(() =>
                {
                    callOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.TeardownEach?.Invoke();
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
                    subject.AddTeardownEach(teardownAction(100));

                    subject.ChildTestCase("child1", child =>
                    {
                        child.AddTeardownEach(teardownAction(200));
                        child1Builder = child;
                    });

                    subject.ChildTestCase("child2", child =>
                    {
                        child.AddTeardownEach(teardownAction(300));
                        child2Builder = child;
                    });
                });

                Context["first child..."] = () =>
                {
                    Act(() =>
                    {
                        callOrder.Clear();
                        child1TestCase = child1Builder.Build();
                        child1TestCase.TeardownEach?.Invoke();
                    });

                    It["should invoke child1 first, then parent (reverse order)"] = () =>
                    {
                        int[] expected = { 200, 100 };
                        Check.That(callOrder).ContainsExactly(expected);
                    };
                };

                Context["second child..."] = () =>
                {
                    Act(() =>
                    {
                        callOrder.Clear();
                        child2TestCase = child2Builder.Build();
                        child2TestCase.TeardownEach?.Invoke();
                    });

                    It["should invoke child2 first, then parent (reverse order)"] = () =>
                    {
                        int[] expected = { 300, 100 };
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
                    subject.AddTeardownEach(teardownAction(1));
                    subject.AddTeardownEach(teardownAction(2));
                });

                Act(() =>
                {
                    callOrder.Clear();
                    firstBuild = subject.Build();
                    secondBuild = subject.Build();

                    firstBuild.TeardownEach?.Invoke();
                    secondBuild.TeardownEach?.Invoke();
                });

                It["should execute independently for each build"] = () =>
                {
                    int[] expected = { 1, 2, 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["TeardownEach with multiple tests..."] = () =>
            {
                List<string> executionOrder = null!;

                BeforeEach(() =>
                {
                    executionOrder = [];
                    subject.AddTeardownEach(() => executionOrder.Add("teardown"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                    subject.AddTest("test3", () => executionOrder.Add("test3"));
                });

                Act(() =>
                {
                    executionOrder.Clear();
                    var testCase = subject.Build();
                    // Simulate running each test with TeardownEach after it
                    foreach (var test in testCase.Tests)
                    {
                        test.test();
                        testCase.TeardownEach?.Invoke();
                    }
                });

                It["should run TeardownEach after each test"] = () =>
                {
                    string[] expected = { "test1", "teardown", "test2", "teardown", "test3", "teardown" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            Describe["demonstrating Setup/Teardown symmetry..."] = () =>
            {
                List<string> executionOrder = null!;
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                BeforeEach(() =>
                {
                    executionOrder = [];
                    subject.AddSetupEach(() => executionOrder.Add("setup-parent"));
                    subject.AddTeardownEach(() => executionOrder.Add("teardown-parent"));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupEach(() => executionOrder.Add("setup-child"));
                        child.AddTeardownEach(() => executionOrder.Add("teardown-child"));
                        childBuilder = child;
                    });
                });

                Act(() =>
                {
                    executionOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.SetupEach?.Invoke();
                    executionOrder.Add("TEST");
                    childTestCase.TeardownEach?.Invoke();
                });

                It["should show perfect symmetry: parent setup, child setup, test, child teardown, parent teardown"] = () =>
                {
                    // Setup runs parent→child, Teardown runs child→parent (symmetric!)
                    string[] expected = { "setup-parent", "setup-child", "TEST", "teardown-child", "teardown-parent" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };
        }
    }
}
