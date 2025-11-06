using NFluent;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    class TeardownOnce : nSpecTestHarness
    {
        private void TeardownOnce_Specs()
        {
            ContextualTestCaseBuilder subject = null!;
            IContextualTestCase subjectTestCase = null!;

            List<int> callOrder = null!;
            Action teardownAction(int n) => () => callOrder.Add(n);

            beforeEach = () =>
            {
                subject = new ContextualTestCaseBuilder();
                subjectTestCase = null!;

                callOrder = [];
            };

            act = () =>
            {
                // Just build - don't invoke TeardownOnce yet
                // Let individual tests control when TeardownOnce runs
                subjectTestCase = subject.Build();
            };

            describe["invoking TeardownOnce after adding an action..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddTeardownOnce(teardownAction(1));
                };

                it["should invoke the action"] = () =>
                {
                    subjectTestCase.TeardownOnce?.Invoke();
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking TeardownOnce after adding multiple actions..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddTeardownOnce(teardownAction(1));
                    subject.AddTeardownOnce(teardownAction(2));
                    subject.AddTeardownOnce(teardownAction(3));
                };

                it["should invoke the actions in the order they were added"] = () =>
                {
                    subjectTestCase.TeardownOnce?.Invoke();
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking TeardownOnce after adding a Func<Task>..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddTeardownOnce(async () => await Task.Run(() => callOrder.Add(1)));
                };
                it["should invoke the Func<Task>"] = () =>
                {
                    subjectTestCase.TeardownOnce?.Invoke();
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking TeardownOnce after adding multiple actions and Func<Task>'s..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddTeardownOnce(teardownAction(1));
                    subject.AddTeardownOnce(() => Task.Run(teardownAction(2)));
                    subject.AddTeardownOnce(teardownAction(3));
                    subject.AddTeardownOnce(() => Task.Run(teardownAction(4)));
                };

                it["should invoke the actions and Func<Task>'s in the order they were added"] = () =>
                {
                    subjectTestCase.TeardownOnce?.Invoke();
                    int[] expected = { 1, 2, 3, 4 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking TeardownOnce multiple times..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddTeardownOnce(teardownAction(10));
                    subject.AddTeardownOnce(teardownAction(20));
                    subject.AddTeardownOnce(teardownAction(30));
                };

                act = () =>
                {
                    subjectTestCase.TeardownOnce?.Invoke();
                    subjectTestCase.TeardownOnce?.Invoke();
                    subjectTestCase.TeardownOnce?.Invoke();
                };

                it["should not run the actions more than once (idempotent)"] = () =>
                {
                    int[] expected = { 10, 20, 30 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["child test case handling"] = () =>
            {
                IContextualTestCaseBuilder childTestCaseBuilder = null!;
                IContextualTestCase childTestCase = null!;

                beforeEach = () =>
                {
                    subject.AddTeardownOnce(teardownAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddTeardownOnce(teardownAction(200));
                        child.AddTeardownOnce(teardownAction(300));
                        child.AddTeardownOnce(teardownAction(400));
                        childTestCaseBuilder = child;
                    });
                };

                context["when the parent context TeardownOnce has run..."] = () =>
                {
                    describe["the parent context..."] = () =>
                    {
                        it["should only invoke the parent context's TeardownOnce instance and not the child's"] = () =>
                        {
                            subjectTestCase.TeardownOnce?.Invoke();
                            int[] expected = { 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                    context["the child context..."] = () =>
                    {
                        act = () =>
                        {
                            callOrder.Clear();
                            childTestCase = childTestCaseBuilder.Build();
                            childTestCase.TeardownOnce?.Invoke();
                        };

                        it["should invoke child's TeardownOnce first, then parent's (REVERSE order - inherited)"] = () =>
                        {
                            // CRITICAL: TeardownOnce runs in REVERSE order (child first, then parent) - inherited like TeardownEach
                            int[] expected = { 200, 300, 400, 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };
                };

                context["when the parent context TeardownOnce has NOT run..."] = () =>
                {
                    describe["the parent context..."] = () =>
                    {
                        it["should only invoke the parent context's TeardownOnce instance and not the child's"] = () =>
                        {
                            subjectTestCase.TeardownOnce?.Invoke();
                            int[] expected = { 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                    context["the child context..."] = () =>
                    {
                        act = () =>
                        {
                            callOrder.Clear();
                            childTestCase = childTestCaseBuilder.Build();
                            childTestCase.TeardownOnce?.Invoke();
                        };

                        it["should invoke child's TeardownOnce first, then parent's (REVERSE order - inherited)"] = () =>
                        {
                            // CRITICAL: TeardownOnce runs in REVERSE order (child first, then parent) - inherited like TeardownEach
                            int[] expected = { 200, 300, 400, 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };
                };
            };

            describe["when no actions are added..."] = () =>
            {
                it["should have an empty TeardownOnce that does nothing"] = () =>
                {
                    // TeardownOnce exists but does nothing
                    subjectTestCase.TeardownOnce?.Invoke();
                    Check.ThatCode(() => subjectTestCase.TeardownOnce?.Invoke()).DoesNotThrow();
                    Check.That(callOrder).IsEmpty();
                };
            };

            describe["exception handling..."] = () =>
            {
                Exception thrownException = null!;
                IContextualTestCase exceptionTestCase = null!;

                beforeEach = () =>
                {
                    thrownException = new InvalidOperationException("Test exception");
                    var exceptionSubject = new ContextualTestCaseBuilder();
                    exceptionSubject.AddTeardownOnce(teardownAction(1));
                    exceptionSubject.AddTeardownOnce(() => throw thrownException);
                    exceptionSubject.AddTeardownOnce(teardownAction(3));
                    exceptionTestCase = exceptionSubject.Build();
                };

                it["should throw the exception when invoked"] = () =>
                {
                    Check.ThatCode(() => exceptionTestCase.TeardownOnce?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };

                it["should have executed actions before the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownOnce?.Invoke(); } catch { }
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["should not execute actions after the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownOnce?.Invoke(); } catch { }
                    Check.That(callOrder).Not.Contains(3);
                };

                it["should retry on subsequent invocations (hasRunToCompletion stays false)"] = () =>
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

            describe["multiple levels of nesting..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCaseBuilder grandchildBuilder = null!;
                IContextualTestCase grandchildTestCase = null!;

                beforeEach = () =>
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
                };

                act = () =>
                {
                    callOrder.Clear();
                    grandchildTestCase = grandchildBuilder.Build();
                    grandchildTestCase.TeardownOnce?.Invoke();
                };

                it["should invoke in REVERSE order: grandchild, parent, grandparent"] = () =>
                {
                    // TeardownOnce runs DEEPEST FIRST (like TeardownEach), then up the chain - REVERSE order
                    int[] expected = { 300, 400, 200, 100 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["parent with no actions, child with actions..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                beforeEach = () =>
                {
                    // Parent has no TeardownOnce actions
                    subject.ChildTestCase("child", child =>
                    {
                        child.AddTeardownOnce(teardownAction(1));
                        child.AddTeardownOnce(teardownAction(2));
                        childBuilder = child;
                    });
                };

                act = () =>
                {
                    callOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.TeardownOnce?.Invoke();
                };

                it["should invoke only the child's actions"] = () =>
                {
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["multiple sibling children..."] = () =>
            {
                IContextualTestCaseBuilder child1Builder = null!;
                IContextualTestCaseBuilder child2Builder = null!;
                IContextualTestCase child1TestCase = null!;
                IContextualTestCase child2TestCase = null!;

                beforeEach = () =>
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
                };

                context["first child..."] = () =>
                {
                    act = () =>
                    {
                        callOrder.Clear();
                        child1TestCase = child1Builder.Build();
                        child1TestCase.TeardownOnce?.Invoke();
                    };

                    it["should invoke child1 first, then parent (reverse order)"] = () =>
                    {
                        int[] expected = { 200, 100 };
                        Check.That(callOrder).ContainsExactly(expected);
                    };
                };

                context["second child..."] = () =>
                {
                    act = () =>
                    {
                        callOrder.Clear();
                        child2TestCase = child2Builder.Build();
                        child2TestCase.TeardownOnce?.Invoke();
                    };

                    it["should invoke child2 first, then parent (reverse order)"] = () =>
                    {
                        int[] expected = { 300, 100 };
                        Check.That(callOrder).ContainsExactly(expected);
                    };
                };
            };

            describe["building multiple times from the same builder..."] = () =>
            {
                IContextualTestCase firstBuild = null!;
                IContextualTestCase secondBuild = null!;

                beforeEach = () =>
                {
                    subject.AddTeardownOnce(teardownAction(1));
                    subject.AddTeardownOnce(teardownAction(2));
                };

                // Don't run in parent act - let this test manage invocation
                act = () =>
                {
                    // Just build, don't invoke yet
                    subjectTestCase = subject.Build();
                };

                it["should share idempotency state (only run once total across all builds)"] = () =>
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

            describe["invoking TeardownOnce after all tests complete..."] = () =>
            {
                List<string> executionOrder = null!;
                IContextualTestCase testCase = null!;

                beforeEach = () =>
                {
                    executionOrder = [];
                    subject.AddTeardownOnce(() => executionOrder.Add("teardown"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                    subject.AddTest("test3", () => executionOrder.Add("test3"));
                };

                // Don't invoke TeardownOnce in parent act
                act = () =>
                {
                    // Just build
                    subjectTestCase = subject.Build();
                };

                it["should run TeardownOnce once after all tests"] = () =>
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

            describe["TeardownOnce idempotency with multiple test runs..."] = () =>
            {
                List<string> executionOrder = null!;
                IContextualTestCase testCase = null!;

                beforeEach = () =>
                {
                    executionOrder = [];
                    subject.AddTeardownOnce(() => executionOrder.Add("teardown"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                };

                // Don't invoke TeardownOnce in parent act
                act = () =>
                {
                    // Just build
                    subjectTestCase = subject.Build();
                };

                it["should run TeardownOnce only once, even across multiple test runs"] = () =>
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

            describe["demonstrating SetupOnce/TeardownOnce symmetry..."] = () =>
            {
                List<string> executionOrder = null!;
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                beforeEach = () =>
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
                };

                act = () =>
                {
                    executionOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.SetupOnce?.Invoke();
                    executionOrder.Add("TEST");
                    childTestCase.TeardownOnce?.Invoke();
                };

                it["should show different patterns: SetupOnce runs parent then child (already ran in parent act), TeardownOnce inherited in REVERSE order"] = () =>
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
