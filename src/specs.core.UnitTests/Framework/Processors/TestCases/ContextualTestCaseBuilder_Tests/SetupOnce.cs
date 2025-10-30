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
            int callcount = 0;
            Action startAction = () => callOrder.Add(callcount++);

            beforeEach = () =>
            {
                subject = new ContextualTestCaseBuilder();
                subjectTestCase = null!;

                callcount = 1;
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
                    subject.AddSetupOnce(startAction);
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
                    subject.AddSetupOnce(startAction);
                    subject.AddSetupOnce(startAction);
                    subject.AddSetupOnce(startAction);
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
                    subject.AddSetupOnce(startAction);
                    subject.AddSetupOnce(() => Task.Run(startAction));
                    subject.AddSetupOnce(startAction);
                    subject.AddSetupOnce(() => Task.Run(startAction));
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
                    subject.AddSetupOnce(startAction);
                    subject.AddSetupOnce(startAction);
                    subject.AddSetupOnce(startAction);
                };

                act = () =>
                {
                    subjectTestCase.SetupOnce?.Invoke();
                    subjectTestCase.SetupOnce?.Invoke();
                    subjectTestCase.SetupOnce?.Invoke();
                };
                it["should not run the actions more than once"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["child test case handling "] = () =>
            {
                IContextualTestCaseBuilder childTestCaseBuilder = null!;
                IContextualTestCase childTestCase = null!;

                beforeEach = () =>
                {
                    subject.AddSetupOnce(startAction);

                    subject.ChildTestCase("child", child =>
                    {
                        child.AddSetupOnce(startAction);
                        child.AddSetupOnce(startAction);
                        child.AddSetupOnce(startAction);
                        childTestCaseBuilder = child;
                    });
                };

                context["when the parent context setupOnce has run it's actions..."] = () =>
                {
                    describe["the parent context..."] = () =>
                    {
                        it["should only invoke the parent context's SetupOnce instance and not the child's"] = () =>
                        {
                            int[] expected = { 1 };
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
                            int[] expected = { 2, 3, 4};
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
                            int[] expected = { 1 };
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
                            int[] expected = { 2, 3, 4 };
                            Check.That(callOrder).ContainsExactly(expected);
                        };
                    };

                };
            };
        }
    }
}
