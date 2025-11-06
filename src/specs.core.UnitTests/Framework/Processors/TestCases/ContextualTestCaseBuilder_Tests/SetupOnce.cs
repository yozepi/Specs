using NFluent;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    class SetupOnce : nSpecTestHarness
    {
        private void SetupOnce_Specs()
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
                subjectTestCase.SetupOnce?.Invoke();
            };

            describe["invoking SetupOnce after adding an action..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupOnce(startAction(1));
                };

                it["should invoke the action"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking SetupOnce after adding multiple actions..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupOnce(startAction(1));
                    subject.AddSetupOnce(startAction(2));
                    subject.AddSetupOnce(startAction(3));
                };

                it["should invoke the actions in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking SetupOnce after adding a Func<Task>..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupOnce(async () => await Task.Run(() => callOrder.Add(1)));
                };
                it["should invoke the Func<Task>"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking SetupOnce after adding multiple actions and Func<Task>'s..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupOnce(startAction(1));
                    subject.AddSetupOnce(() => Task.Run(startAction(2)));
                    subject.AddSetupOnce(startAction(3));
                    subject.AddSetupOnce(() => Task.Run(startAction(4)));
                };

                it["should invoke the actions and Func<Task>'s in the order they were added"] = () =>
                {
                    int[] expected = { 1, 2, 3, 4 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["invoking SetupOnce multiple times..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddSetupOnce(startAction(10));
                    subject.AddSetupOnce(startAction(20));
                    subject.AddSetupOnce(startAction(30));
                };

                act = () =>
                {
                    subjectTestCase.SetupOnce?.Invoke();
                    subjectTestCase.SetupOnce?.Invoke();
                    subjectTestCase.SetupOnce?.Invoke();
                };
                it["should not run the actions more than once"] = () =>
                {
                    int[] expected = { 10, 20, 30 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["child test case handling "] = () =>
            {
                IContextualTestCaseBuilder childTestCaseBuilder = null!;
                IContextualTestCase childTestCase = null!;

                beforeEach = () =>
                {
                    subject.AddSetupOnce(startAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupOnce(startAction(200));
                        child.AddSetupOnce(startAction(300));
                        child.AddSetupOnce(startAction(400));
                        childTestCaseBuilder = child;
                    });
                };

                context["when the parent context setupOnce has run it's actions..."] = () =>
                {
                    describe["the parent context..."] = () =>
                    {
                        it["should only invoke the parent context's SetupOnce instance and not the child's"] = () =>
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
                             childTestCase.SetupOnce?.Invoke();
                        };

                        it["should only invoke the child context's SetupOnce instance and not the parent's"] = () =>
                        {
                            int[] expected = { 200, 300, 400};
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };

                context["when the parent context setupOnce has NOT run it's actions..."] = () =>
                {
                    describe["the parent context..."] = () =>
                    {
                        it["should only invoke the parent context's SetupOnce instance and not the child's"] = () =>
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
                            childTestCase.SetupOnce?.Invoke();
                        };

                        it["should only invoke the child context's SetupOnce instance and not the parent's"] = () =>
                        {
                            int[] expected = { 200, 300, 400 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };
            };

            describe["when no actions are added..."] = () =>
            {
                it["should have an empty SetupOnce that does nothing"] = () =>
                {
                    // SetupOnce exists but does nothing
                    Check.ThatCode(() => subjectTestCase.SetupOnce?.Invoke()).DoesNotThrow();
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
                    exceptionSubject.AddSetupOnce(startAction(1));
                    exceptionSubject.AddSetupOnce(() => throw thrownException);
                    exceptionSubject.AddSetupOnce(startAction(3));
                    exceptionTestCase = exceptionSubject.Build();
                };

                it["should throw the exception when invoked"] = () =>
                {
                    Check.ThatCode(() => exceptionTestCase.SetupOnce?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };

                it["should have executed actions before the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.SetupOnce?.Invoke(); } catch { }
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["should not execute actions after the exception"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.SetupOnce?.Invoke(); } catch { }
                    Check.That(callOrder).Not.Contains(3);
                };

                it["should still throw on subsequent invocations since it never completed successfully"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.SetupOnce?.Invoke(); } catch { }

                    Check.ThatCode(() => exceptionTestCase.SetupOnce?.Invoke())
                        .Throws<InvalidOperationException>()
                        .WithMessage("Test exception");
                };

                it["should retry from the beginning on subsequent calls"] = () =>
                {
                    callOrder.Clear();
                    try { exceptionTestCase.SetupOnce?.Invoke(); } catch { }
                    callOrder.Clear();
                    try { exceptionTestCase.SetupOnce?.Invoke(); } catch { }
                    // Should execute first action again before throwing
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
                    subject.AddSetupOnce(startAction(100));

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupOnce(startAction(200));

                        child.ChildTestCase("grandchild", grandchild =>
                        {
                            grandchild.AddSetupOnce(startAction(300));
                            grandchild.AddSetupOnce(startAction(400));
                            grandchildBuilder = grandchild;
                        });

                        childBuilder = child;
                    });
                };

                act = () =>
                {
                    callOrder.Clear();
                    grandchildTestCase = grandchildBuilder.Build();
                    grandchildTestCase.SetupOnce?.Invoke();
                };

                it["should invoke child and grandchild actions (grandparent already ran in parent act)"] = () =>
                {
                    // Grandparent SetupOnce already ran in parent act and is idempotent
                    // So only child and grandchild SetupOnce run here
                    int[] expected = { 200, 300, 400 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["parent with no actions, child with actions..."] = () =>
            {
                IContextualTestCaseBuilder childBuilder = null!;
                IContextualTestCase childTestCase = null!;

                beforeEach = () =>
                {
                    // Parent has no SetupOnce actions
                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupOnce(startAction(1));
                        child.AddSetupOnce(startAction(2));
                        childBuilder = child;
                    });
                };

                act = () =>
                {
                    callOrder.Clear();
                    childTestCase = childBuilder.Build();
                    childTestCase.SetupOnce?.Invoke();
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
                    subject.AddSetupOnce(startAction(100));

                    subject.ChildTestCase("child1", child =>
                    {
                        child.AddSetupOnce(startAction(200));
                        child1Builder = child;
                    });

                    subject.ChildTestCase("child2", child =>
                    {
                        child.AddSetupOnce(startAction(300));
                        child2Builder = child;
                    });
                };

                context["first child..."] = () =>
                {
                    act = () =>
                    {
                        callOrder.Clear();
                        child1TestCase = child1Builder.Build();
                        child1TestCase.SetupOnce?.Invoke();
                    };

                    it["should invoke only first child actions (isolated)"] = () =>
                    {
                        int[] expected = { 200 };
                        Check.That(callOrder).ContainsExactly(expected);
                    };
                };

                context["second child..."] = () =>
                {
                    act = () =>
                    {
                        callOrder.Clear();
                        child2TestCase = child2Builder.Build();
                        child2TestCase.SetupOnce?.Invoke();
                    };

                    it["should invoke only second child actions (isolated)"] = () =>
                    {
                        int[] expected = { 300 };
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
                    subject.AddSetupOnce(startAction(1));
                    subject.AddSetupOnce(startAction(2));
                };

                act = () =>
                {
                    // Parent act already built and invoked once
                    // Building again returns same cached RunAllRunner
                    firstBuild = subject.Build();
                    secondBuild = subject.Build();

                    firstBuild.SetupOnce?.Invoke();
                    secondBuild.SetupOnce?.Invoke();
                };

                it["should use the same RunAllRunner instance (cached) so only runs once total"] = () =>
                {
                    // Parent act: {1, 2}
                    // This act: two more invocations do nothing (hasRunToCompletion=true)
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["SetupOnce with multiple tests..."] = () =>
            {
                List<string> executionOrder = null!;

                beforeEach = () =>
                {
                    executionOrder = [];
                    subject.AddSetupOnce(() => executionOrder.Add("setup"));
                    subject.AddTest("test1", () => executionOrder.Add("test1"));
                    subject.AddTest("test2", () => executionOrder.Add("test2"));
                    subject.AddTest("test3", () => executionOrder.Add("test3"));
                };

                act = () =>
                {
                    // Parent act already ran SetupOnce
                    // Just run the tests here
                    foreach (var test in subjectTestCase.Tests)
                    {
                        test.test();
                    }
                };

                it["should run SetupOnce once before all tests"] = () =>
                {
                    // Parent act: "setup"
                    // This act: "test1", "test2", "test3"
                    string[] expected = { "setup", "test1", "test2", "test3" };
                    Check.That(executionOrder).ContainsExactly(expected);
                };
            };
        }
    }
}
