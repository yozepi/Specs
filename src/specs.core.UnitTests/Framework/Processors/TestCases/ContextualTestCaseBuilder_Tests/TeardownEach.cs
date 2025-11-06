using NFluent;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    class TeardownEach : nSpecTestHarness
    {
        private void TeardownEach_Specs()
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
                subjectTestCase = subject.Build();
                subjectTestCase.TeardownEach?.Invoke();
            };

            describe["invoking TeardownEach after adding an action..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddTeardownEach(teardownAction(1));
                };

                it["should invoke the action"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking TeardownEach after adding multiple actions..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddTeardownEach(teardownAction(1));
                    subject.AddTeardownEach(teardownAction(2));
                    subject.AddTeardownEach(teardownAction(3));
                };

                it["should invoke the actions in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking TeardownEach after adding a Func<Task>..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddTeardownEach(async () => await Task.Run(() => callOrder.Add(1)));
                };
                it["should invoke the Func<Task>"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking TeardownEach after adding multiple actions and Func<Task>'s..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddTeardownEach(teardownAction(1));
                    subject.AddTeardownEach(() => Task.Run(teardownAction(2)));
                    subject.AddTeardownEach(teardownAction(3));
                    subject.AddTeardownEach(() => Task.Run(teardownAction(4)));
                };

                it["should invoke the actions and Func<Task>'s in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3, 4 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking TeardownEach multiple times..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddTeardownEach(teardownAction(10));
                    subject.AddTeardownEach(teardownAction(20));
                    subject.AddTeardownEach(teardownAction(30));
                };

                act = () =>
                {
                    subjectTestCase.TeardownEach?.Invoke();
                    subjectTestCase.TeardownEach?.Invoke();
                    subjectTestCase.TeardownEach?.Invoke();
                };
                it["should run the actions each time it is invoked"] = () =>
                {
                    int[] expected = { 10, 20, 30, 10, 20, 30, 10, 20, 30, 10, 20, 30 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["child test case handling "] = () =>
            {
                IContextualTestCaseBuilder childTestCaseBuilder = null!;
                IContextualTestCase childTestCase = null!;

                beforeEach = () =>
                {
                    subject.AddTeardownEach(teardownAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddTeardownEach(teardownAction(200));
                        child.AddTeardownEach(teardownAction(300));
                        child.AddTeardownEach(teardownAction(400));
                        childTestCaseBuilder = child;
                    });
                };

                context["when the parent context TeardownEach has run it's actions..."] = () =>
                {
                    describe["the parent context..."] = () =>
                    {
                        it["should only invoke the parent context's TeardownEach instance and not the child's"] = () =>
                        {
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
                            childTestCase.TeardownEach?.Invoke();
                        };

                        it["should invoke child's TeardownEach first, then parent's (REVERSE order)"] = () =>
                        {
                            // CRITICAL: TeardownEach runs in REVERSE order (child first, then parent)
                            // This is opposite of SetupEach
                            int[] expected = { 200, 300, 400, 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };

                context["when the parent context TeardownEach has NOT run it's actions..."] = () =>
                {
                    describe["the parent context..."] = () =>
                    {
                        it["should only invoke the parent context's TeardownEach instance and not the child's"] = () =>
                        {
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
                            childTestCase.TeardownEach?.Invoke();
                        };

                        it["should invoke child's TeardownEach first, then parent's (REVERSE order)"] = () =>
                        {
                            // CRITICAL: TeardownEach runs in REVERSE order (child first, then parent)
                            int[] expected = { 200, 300, 400, 100 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };
            };

            describe["when no actions are added..."] = () =>
            {
                it["should have an empty TeardownEach that does nothing"] = () =>
                {
                    // TeardownEach exists but does nothing
                    Check.ThatCode(() => subjectTestCase.TeardownEach?.Invoke()).DoesNotThrow();
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
                    exceptionSubject.AddTeardownEach(teardownAction(1));
                    exceptionSubject.AddTeardownEach(() => throw thrownException);
                    exceptionSubject.AddTeardownEach(teardownAction(3));
                    exceptionTestCase = exceptionSubject.Build();
                };

                it["should throw the exception when invoked"] = () =>
                {
                    Check.ThatCode(() => exceptionTestCase.TeardownEach?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };

                it["should have executed actions before the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownEach?.Invoke(); } catch { }
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["should not execute actions after the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownEach?.Invoke(); } catch { }
                    Check.That(callOrder).Not.Contains(3);
                };

                it["should still throw on subsequent invocations"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.TeardownEach?.Invoke(); } catch { }

                    Check.ThatCode(() => exceptionTestCase.TeardownEach?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };
            };

            describe["multiple levels of nesting..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCaseBuilder grandchildBuilder = null!;
                IContextualTestCase grandchildTestCase = null!;

                beforeEach = () =>
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
                };

                act = () =>
                {
                    callOrder.Clear();
                    grandchildTestCase = grandchildBuilder.Build();
                    grandchildTestCase.TeardownEach?.Invoke();
                };

                it["should invoke in REVERSE order: grandchild, parent, grandparent"] = () =>
                {
                    // TeardownEach runs DEEPEST FIRST, then up the chain
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
                    // Parent has no TeardownEach actions
                    subject.ChildTestCase("child", child =>
                    {
                        child.AddTeardownEach(teardownAction(1));
                        child.AddTeardownEach(teardownAction(2));
                        childBuilder = child;
                    });
                };

                act = () =>
                {
                    callOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.TeardownEach?.Invoke();
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
                };

                context["first child..."] = () =>
                {
                    act = () =>
                    {
                        callOrder.Clear();
                        child1TestCase = child1Builder.Build();
                        child1TestCase.TeardownEach?.Invoke();
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
                        child2TestCase.TeardownEach?.Invoke();
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
                    subject.AddTeardownEach(teardownAction(1));
                    subject.AddTeardownEach(teardownAction(2));
                };

                act = () =>
                {
                    callOrder.Clear();
                    firstBuild = subject.Build();
                    secondBuild = subject.Build();

                    firstBuild.TeardownEach?.Invoke();
                    secondBuild.TeardownEach?.Invoke();
                };

                it["should execute independently for each build"] = () =>
                {
                    int[] expected = { 1, 2, 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["TeardownEach with multiple tests..."] = () =>
            {
                List<string> executionOrder = null!;

                beforeEach = () =>
                {
                    executionOrder = [];
                    subject.AddTeardownEach(() => executionOrder.Add("teardown"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                    subject.AddTest("test3", () => executionOrder.Add("test3"));
                };

                act = () =>
                {
                    executionOrder.Clear();
                    var testCase = subject.Build();
                    // Simulate running each test with TeardownEach after it
                    foreach (var test in testCase.Tests)
                    {
                        test.test();
                        testCase.TeardownEach?.Invoke();
                    }
                };

                it["should run TeardownEach after each test"] = () =>
                {
                    string[] expected = { "test1", "teardown", "test2", "teardown", "test3", "teardown" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };

            describe["demonstrating Setup/Teardown symmetry..."] = () =>
            {
                List<string> executionOrder = null!;
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                beforeEach = () =>
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
                };

                act = () =>
                {
                    executionOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.SetupEach?.Invoke();
                    executionOrder.Add("TEST");
                    childTestCase.TeardownEach?.Invoke();
                };

                it["should show perfect symmetry: parent setup, child setup, test, child teardown, parent teardown"] = () =>
                {
                    // Setup runs parent→child, Teardown runs child→parent (symmetric!)
                    string[] expected = { "setup-parent", "setup-child", "TEST", "teardown-child", "teardown-parent" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };
        }
    }
}
