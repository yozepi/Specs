using NFluent;
using specs.core.Framework.Processors.TestCases;
using specs.BDD;
using specs.BDD.Attributes;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public class ContextualTestCase_Tests : Specifications
    {

        string description = "description";
        Action setupOnce = () => { };
        Action setupEach = () => { };
        Action actAction = () => { };
        Action teardownEach = () => { };
        Action teardownOnce = () => { };

        List<(string description, Action test)> tests = [];
        List<ITestCaseContext> childContexts = [];


        [Describe]
        public void ContextualTestCase_Specs()
        {

            Describe["CreateTestCase static factory method"] = () =>
            {
                IContextualTestCase subject = null!;

                Act(() =>
                {
                    subject = ContextualTestCase.CreateTestCase(
                        setupOnce: setupOnce,
                        setupEach: setupEach,
                        act: actAction,
                        teardownEach: teardownEach,
                        teardownOnce: teardownOnce,
                        ownSetupOnce: () => { },
                        ownTeardownOnce: () => { },
                        tests: tests,
                        childContexts: childContexts);
                });

                It["should create a new instance of IContextualTestCase"] = () =>
                {
                    Check.That(subject).IsNotNull();
                    Check.That(subject).InheritsFrom<IContextualTestCase>();
                };

                ITestCaseTests(() => subject);

                It["should set the ChildContexts property"] = () =>
                {
                    Check.That(subject.ChildContexts).IsEqualTo(childContexts);
                };

                It["should default PendingTests to empty"] = () =>
                {
                    Check.That(subject.PendingTests).IsEmpty();
                };
            };

            Describe["CreateTestCase with pending tests"] = () =>
            {
                IContextualTestCase subject = null!;

                Act(() =>
                {
                    subject = ContextualTestCase.CreateTestCase(
                        setupOnce: setupOnce,
                        setupEach: setupEach,
                        act: actAction,
                        teardownEach: teardownEach,
                        teardownOnce: teardownOnce,
                        ownSetupOnce: () => { },
                        ownTeardownOnce: () => { },
                        tests: tests,
                        childContexts: childContexts,
                        pendingTests: ["pending", "pending"]);
                });

                It["should set the PendingTests property without duplicates"] = () =>
                {
                    Check.That(subject.PendingTests).ContainsExactly("pending");
                };
            };

            Describe["CreateTestCaseContext static factory method"] = () => 
            {
                ITestCaseContext subject = null!;
                Act(() =>
                {
                    subject = ContextualTestCase.CreateTestCaseContext(
                        description: description,
                        setupOnce: setupOnce,
                        setupEach: setupEach,
                        act: actAction,
                        teardownEach: teardownEach,
                        teardownOnce: teardownOnce,
                        ownSetupOnce: () => { },
                        ownTeardownOnce: () => { },
                        tests: tests,
                        childContexts: childContexts);
                });


                It["should create a new instance of IContextualTestCase"] = () =>
                {
                    Check.That(subject).IsNotNull();
                    Check.That(subject).InheritsFrom<ITestCaseContext>();
                };

                It["should set the Description property"] = () =>
                {
                    Check.That(subject.Description).IsEqualTo(description);
                };

                ITestCaseTests(() => subject);
            };
        }

        private void ITestCaseTests(Func<ITestCase> subjectFactory)
        {
            It["should set the SetupOnce property"] = () =>
            {
                Check.That(subjectFactory().SetupOnce).IsEqualTo(setupOnce);
            };

            It["should set the SetupEach property"] = () =>
            {
                Check.That(subjectFactory().SetupEach).IsEqualTo(setupEach);
            };

            It["should set the Act property"] = () =>
            {
                Check.That(subjectFactory().Act).IsEqualTo(actAction);
            };

            It["should set the TeardownEach property"] = () =>
            {
                Check.That(subjectFactory().TeardownEach).IsEqualTo(teardownEach);
            };

            It["should set the TeardownOnce property"] = () =>
            {
                Check.That(subjectFactory().TeardownOnce).IsEqualTo(teardownOnce);
            };

            It["should set the Tests property"] = () =>
            {
                Check.That(subjectFactory().Tests).IsEqualTo(tests);
            };
        }
    }
}
