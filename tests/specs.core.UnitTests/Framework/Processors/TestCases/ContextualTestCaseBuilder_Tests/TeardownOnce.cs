using NFluent;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public partial class ContextualTestCaseBuilder_Tests
    {
        [Describe("TeardownOnce Specs")]
        public void TeardownOnce_Specs()
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
                // Just build - don't invoke TeardownOnce yet
                // Let individual tests control when TeardownOnce runs
                subjectTestCase = subject.Build();
            });

            Describe["invoking TeardownOnce after adding an action..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTeardownOnce(teardownAction(1));
                });

                It["should invoke the action"] = () =>
                {
                    subjectTestCase.TeardownOnce?.Invoke();
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking TeardownOnce after adding multiple actions..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTeardownOnce(teardownAction(1));
                    subject.AddTeardownOnce(teardownAction(2));
                    subject.AddTeardownOnce(teardownAction(3));
                });

                It["should invoke the actions in the order they were added"] = () =>
                {
                    subjectTestCase.TeardownOnce?.Invoke();
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking TeardownOnce after adding a Func<Task>..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTeardownOnce(async () => await Task.Run(() => callOrder.Add(1)));
                });
                It["should invoke the Func<Task>"] = () =>
                {
                    subjectTestCase.TeardownOnce?.Invoke();
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking TeardownOnce after adding multiple actions and Func<Task>'s..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTeardownOnce(teardownAction(1));
                    subject.AddTeardownOnce(() => Task.Run(teardownAction(2)));
                    subject.AddTeardownOnce(teardownAction(3));
                    subject.AddTeardownOnce(() => Task.Run(teardownAction(4)));
                });

                It["should invoke the actions and Func<Task>'s in the order they were added"] = () =>
                {
                    subjectTestCase.TeardownOnce?.Invoke();
                    int[] expected = { 1, 2, 3, 4 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking TeardownOnce multiple times..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTeardownOnce(teardownAction(10));
                    subject.AddTeardownOnce(teardownAction(20));
                    subject.AddTeardownOnce(teardownAction(30));
                });

                Act(() =>
                {
                    subjectTestCase.TeardownOnce?.Invoke();
                    subjectTestCase.TeardownOnce?.Invoke();
                    subjectTestCase.TeardownOnce?.Invoke();
                });

                It["should not run the actions more than once (idempotent)"] = () =>
                {
                    int[] expected = { 10, 20, 30 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["child test case handling"] = () =>
            {
                IContextualTestCaseBuilder childTestCaseBuilder = null!;
                IContextualTestCase childTestCase = null!;

                BeforeEach(() =>
                {
                    subject.AddTeardownOnce(teardownAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddTeardownOnce(teardownAction(200));
                        child.AddTeardownOnce(teardownAction(300));
                        child.AddTeardownOnce(teardownAction(400));
                        childTestCaseBuilder = child;
                    });
                });

                Context["when the parent context TeardownOnce has run..."] = () =>
                {
                    Describe["the parent context..."] = () =>
                    {
                        It["should only invoke the parent context's TeardownOnce instance and not the child's"] = () =>
                        {
                            subjectTestCase.TeardownOnce?.Invoke();
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
                            childTestCase.TeardownOnce?.Invoke();
                        });

                        It["should invoke child's TeardownOnce first, then parent's (REVERSE order - inherited)"] = () =>
                        {
                            // CRITICAL: TeardownOnce runs in REVERSE order (child first, then parent) - inherited like TeardownEach
                            int[] expected = { 200, 300, 400, 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };
                };

                Context["when the parent context TeardownOnce has NOT run..."] = () =>
                {
                    Describe["the parent context..."] = () =>
                    {
                        It["should only invoke the parent context's TeardownOnce instance and not the child's"] = () =>
                        {
                            subjectTestCase.TeardownOnce?.Invoke();
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
                            childTestCase.TeardownOnce?.Invoke();
                        });

                        It["should invoke child's TeardownOnce first, then parent's (REVERSE order - inherited)"] = () =>
                        {
                            // CRITICAL: TeardownOnce runs in REVERSE order (child first, then parent) - inherited like TeardownEach
                            int[] expected = { 200, 300, 400, 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };
                };
            };

            Describe["when no actions are added..."] = () =>
            {
                It["should have an empty TeardownOnce that does nothing"] = () =>
                {
                    // TeardownOnce exists but does nothing
                    subjectTestCase.TeardownOnce?.Invoke();
                    Check.ThatCode(() => subjectTestCase.TeardownOnce?.Invoke()).DoesNotThrow();
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
                    exceptionSubject.AddTeardownOnce(teardownAction(1));
                    exceptionSubject.AddTeardownOnce(() => throw thrownException);
                    exceptionSubject.AddTeardownOnce(teardownAction(3));
                    exceptionTestCase = exceptionSubject.Build();
                });

                It["should throw the exception when invoked"] = () =>
                {
                    Check.ThatCode(() => exceptionTestCase.TeardownOnce?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };

                It["should have executed actions before the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownOnce?.Invoke(); } catch { }
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                It["should not execute actions after the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownOnce?.Invoke(); } catch { }
                    Check.That(callOrder).Not.Contains(3);
                };

                It["should retry on subsequent invocations (hasRunToCompletion stays false)"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownOnce?.Invoke(); } catch { }
                    callOrder.Clear();

                    // Should retry because exception prevented hasRunToCompletion from being set
                    Check.ThatCode(() => exceptionTestCase.TeardownOnce?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");

                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["multiple levels of nesting..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCaseBuilder grandchildBuilder = null!;
                IContextualTestCase grandchildTestCase = null!;

                BeforeEach(() =>
                {
                    subject.AddTeardownOnce(teardownAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddTeardownOnce(teardownAction(200));

                        child.ChildTestCase("grandchild", grandchild =>
                        {
                            grandchild.AddTeardownOnce(teardownAction(300));
                            grandchild.AddTeardownOnce(teardownAction(400));
                            grandchildBuilder = grandchild;
                        });

                        childBuilder = child;
                    });
                });

                Act(() =>
                {
                    callOrder.Clear();
                    grandchildTestCase = grandchildBuilder.Build();
                    grandchildTestCase.TeardownOnce?.Invoke();
                });

                It["should invoke in REVERSE order: grandchild, parent, grandparent"] = () =>
                {
                    // TeardownOnce runs DEEPEST FIRST (like TeardownEach), then up the chain - REVERSE order
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
                    // Parent has no TeardownOnce actions
                    subject.ChildTestCase("child", child =>
                    {
                        child.AddTeardownOnce(teardownAction(1));
                        child.AddTeardownOnce(teardownAction(2));
                        childBuilder = child;
                    });
                });

                Act(() =>
                {
                    callOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.TeardownOnce?.Invoke();
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
                    subject.AddTeardownOnce(teardownAction(100));

                    subject.ChildTestCase("child1", child =>
                    {
                        child.AddTeardownOnce(teardownAction(200));
                        child1Builder = child;
                    });

                    subject.ChildTestCase("child2", child =>
                    {
                        child.AddTeardownOnce(teardownAction(300));
                        child2Builder = child;
                    });
                });

                Context["first child..."] = () =>
                {
                    Act(() =>
                    {
                        callOrder.Clear();
                        child1TestCase = child1Builder.Build();
                        child1TestCase.TeardownOnce?.Invoke();
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
                        child2TestCase.TeardownOnce?.Invoke();
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
                    subject.AddTeardownOnce(teardownAction(1));
                    subject.AddTeardownOnce(teardownAction(2));
                });

                // Don't run in parent act - let this test manage invocation
                Act(() =>
                {
                    // Just build, don't invoke yet
                    subjectTestCase = subject.Build();
                });

                It["should share idempotency state (only run once total across all builds)"] = () =>
                {
                    callOrder.Clear();
                    firstBuild = subject.Build();
                    secondBuild = subject.Build();

                    firstBuild.TeardownOnce?.Invoke();
                    secondBuild.TeardownOnce?.Invoke();

                    // Multiple builds share the same Lazy<Action?>, so idempotency is shared
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["invoking TeardownOnce after all tests complete..."] = () =>
            {
                List<string> executionOrder = null!;
                IContextualTestCase testCase = null!;

                BeforeEach(() =>
                {
                    executionOrder = [];
                    subject.AddTeardownOnce(() => executionOrder.Add("teardown"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                    subject.AddTest("test3", () => executionOrder.Add("test3"));
                });

                // Don't invoke TeardownOnce in parent act
                Act(() =>
                {
                    // Just build
                    subjectTestCase = subject.Build();
                });

                It["should run TeardownOnce once after all tests"] = () =>
                {
                    executionOrder.Clear();
                    testCase = subject.Build();
                    // Simulate running all tests
                    foreach (var test in testCase.Tests)
                    {
                        test.test();
                    }
                    // TeardownOnce runs after all tests complete
                    testCase.TeardownOnce?.Invoke();

                    string[] expected = { "test1", "test2", "test3", "teardown" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            Describe["TeardownOnce idempotency with multiple test runs..."] = () =>
            {
                List<string> executionOrder = null!;
                IContextualTestCase testCase = null!;

                BeforeEach(() =>
                {
                    executionOrder = [];
                    subject.AddTeardownOnce(() => executionOrder.Add("teardown"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                });

                // Don't invoke TeardownOnce in parent act
                Act(() =>
                {
                    // Just build
                    subjectTestCase = subject.Build();
                });

                It["should run TeardownOnce only once, even across multiple test runs"] = () =>
                {
                    executionOrder.Clear();
                    testCase = subject.Build();

                    // First test run
                    foreach (var test in testCase.Tests)
                    {
                        test.test();
                    }
                    testCase.TeardownOnce?.Invoke();

                    // Second test run - TeardownOnce should not run again
                    foreach (var test in testCase.Tests)
                    {
                        test.test();
                    }
                    testCase.TeardownOnce?.Invoke();

                    string[] expected = { "test1", "test2", "teardown", "test1", "test2" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            Describe["demonstrating SetupOnce/TeardownOnce symmetry..."] = () =>
            {
                List<string> executionOrder = null!;
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                BeforeEach(() =>
                {
                    executionOrder = [];
                    subject.AddSetupOnce(() => executionOrder.Add("setup-parent"));
                    subject.AddTeardownOnce(() => executionOrder.Add("teardown-parent"));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupOnce(() => executionOrder.Add("setup-child"));
                        child.AddTeardownOnce(() => executionOrder.Add("teardown-child"));
                        childBuilder = child;
                    });
                });

                Act(() =>
                {
                    executionOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.SetupOnce?.Invoke();
                    executionOrder.Add("TEST");
                    childTestCase.TeardownOnce?.Invoke();
                });

                It["should show different patterns: SetupOnce runs parent then child (already ran in parent act), TeardownOnce inherited in REVERSE order"] = () =>
                {
                    // SetupOnce: parent already ran in parent act, then child runs (idempotent across shared builder)
                    // TeardownOnce: child then parent (REVERSE order - inherited like TeardownEach)
                    string[] expected = { "setup-parent", "setup-child", "TEST", "teardown-child", "teardown-parent" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };
        }
    }
}
