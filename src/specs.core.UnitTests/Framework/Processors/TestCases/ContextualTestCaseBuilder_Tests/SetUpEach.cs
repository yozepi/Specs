using NFluent;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    class SetUpEach : nSpecTestHarness
    {
        private void SetUpEach_Specs()
        {
            ContextualTestCaseBuilder subject = null!;
            IContextualTestCase subjectTestCase = null!;

            List<int> callOrder = null!;
            Action startAction(int n) => () => callOrder.Add(n);

            beforeEach = () =>
            {
                subject = new ContextualTestCaseBuilder();
                subjectTestCase = null!;

                callOrder = [];
            };

            act = () =>
            {
                subjectTestCase = subject.Build();
                subjectTestCase.SetupEach?.Invoke();
            };

            describe["invoking SetupEach after adding an action..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupEach(startAction(1));
                };

                it["should invoke the action"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking SetupEach after adding multiple actions..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupEach(startAction(1));
                    subject.AddSetupEach(startAction(2));
                    subject.AddSetupEach(startAction(3));
                };

                it["should invoke the actions in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking SetupEach after adding a Func<Task>..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupEach(async () => await Task.Run(() => callOrder.Add(1)));
                };
                it["should invoke the Func<Task>"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking SetupEach after adding multiple actions and Func<Task>'s..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupEach(startAction(1));
                    subject.AddSetupEach(() => Task.Run(startAction(2)));
                    subject.AddSetupEach(startAction(3));
                    subject.AddSetupEach(() => Task.Run(startAction(4)));
                };

                it["should invoke the actions and Func<Task>'s in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3, 4 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking SetupEach multiple times..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupEach(startAction(10));
                    subject.AddSetupEach(startAction(20));
                    subject.AddSetupEach(startAction(30));
                };

                act = () =>
                {
                    subjectTestCase.SetupEach?.Invoke();
                    subjectTestCase.SetupEach?.Invoke();
                    subjectTestCase.SetupEach?.Invoke();
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
                    subject.AddSetupEach(startAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupEach(startAction(200));
                        child.AddSetupEach(startAction(300));
                        child.AddSetupEach(startAction(400));
                        childTestCaseBuilder = child;
                    });
                };

                context["when the parent context setupEach has run it's actions..."] = () =>
                {
                    describe["the parent context..."] = () =>
                    {
                        it["should only invoke the parent context's SetupEach instance and not the child's"] = () =>
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
                            childTestCase.SetupEach?.Invoke();
                        };

                        it["should invoke both the parent context's SetupEach and the child's SetupEach instances"] = () =>
                        {
                            int[] expected = { 100, 200, 300, 400};
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };

                context["when the parent context setupEach has NOT run it's actions..."] = () =>
                {
                    describe["the parent context..."] = () =>
                    {
                        it["should only invoke the parent context's SetupEach instance and not the child's"] = () =>
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
                            childTestCase.SetupEach?.Invoke();
                        };

                        it["should invoke both the parent context's SetupEach and the child's SetupEach instances"] = () =>
                        {
                            int[] expected = { 100, 200, 300, 400 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };
            };

            describe["when no actions are added..."] = () =>
            {
                it["should have an empty SetupEach that does nothing"] = () =>
                {
                    // SetupEach exists but does nothing
                    Check.ThatCode(() => subjectTestCase.SetupEach?.Invoke()).DoesNotThrow();
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
                    exceptionSubject.AddSetupEach(startAction(1));
                    exceptionSubject.AddSetupEach(() => throw thrownException);
                    exceptionSubject.AddSetupEach(startAction(3));
                    exceptionTestCase = exceptionSubject.Build();
                };

                it["should throw the exception when invoked"] = () =>
                {
                    Check.ThatCode(() => exceptionTestCase.SetupEach?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };

                it["should have executed actions before the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.SetupEach?.Invoke(); } catch { }
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["should not execute actions after the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.SetupEach?.Invoke(); } catch { }
                    Check.That(callOrder).Not.Contains(3);
                };

                it["should still throw on subsequent invocations"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.SetupEach?.Invoke(); } catch { }

                    Check.ThatCode(() => exceptionTestCase.SetupEach?.Invoke())
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
                };

                act = () =>
                {
                    callOrder.Clear();
                    grandchildTestCase = grandchildBuilder.Build();
                    grandchildTestCase.SetupEach?.Invoke();
                };

                it["should invoke grandparent, parent, and grandchild actions in order"] = () =>
                {
                    int[] expected = { 100, 200, 300, 400 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["parent with no actions, child with actions..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                beforeEach = () =>
                {
                    // Parent has no SetupEach actions
                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupEach(startAction(1));
                        child.AddSetupEach(startAction(2));
                        childBuilder = child;
                    });
                };

                act = () =>
                {
                    callOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.SetupEach?.Invoke();
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
                };

                context["first child..."] = () =>
                {
                    act = () =>
                    {
                        callOrder.Clear();
                        child1TestCase = child1Builder.Build();
                        child1TestCase.SetupEach?.Invoke();
                    };

                    it["should invoke parent and first child actions only"] = () =>
                    {
                        int[] expected = { 100, 200 };
                        Check.That(callOrder).ContainsExactly(expected);
                    };
                };

                context["second child..."] = () =>
                {
                    act = () =>
                    {
                        callOrder.Clear();
                        child2TestCase = child2Builder.Build();
                        child2TestCase.SetupEach?.Invoke();
                    };

                    it["should invoke parent and second child actions only"] = () =>
                    {
                        int[] expected = { 100, 300 };
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
                    subject.AddSetupEach(startAction(1));
                    subject.AddSetupEach(startAction(2));
                };

                act = () =>
                {
                    callOrder.Clear();
                    firstBuild = subject.Build();
                    secondBuild = subject.Build();

                    firstBuild.SetupEach?.Invoke();
                    secondBuild.SetupEach?.Invoke();
                };

                it["should execute independently for each build"] = () =>
                {
                    int[] expected = { 1, 2, 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["SetupEach with multiple tests..."] = () =>
            {
                List<string> executionOrder = null!;

                beforeEach = () =>
                {
                    executionOrder = [];
                    subject.AddSetupEach(() => executionOrder.Add("setup"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                    subject.AddTest("test3", () => executionOrder.Add("test3"));
                };

                act = () =>
                {
                    executionOrder.Clear();
                    var testCase = subject.Build();
                    // Simulate running each test with SetupEach
                    foreach (var test in testCase.Tests)
                    {
                        testCase.SetupEach?.Invoke();
                        test.test();
                    }
                };

                it["should run SetupEach before each test"] = () =>
                {
                    string[] expected = { "setup", "test1", "setup", "test2", "setup", "test3" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };
        }
    }
}
