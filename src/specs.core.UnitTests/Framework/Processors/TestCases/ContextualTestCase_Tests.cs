using NFluent;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public class ContextualTestCase_Tests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        string description = "description";
        Action setupOnce = () => { };
        Action setupEach = () => { };
        Action actAction = () => { };
        Action teardownEach = () => { };
        Action teardownOnce = () => { };

        List<(string description, Action test)> tests = [];
        List<ITestCaseContext> childContexts = [];


        private void ContextualTestCase_Specs()
        {

            describe["CreateTestCase static factory method"] = () =>
            {
                IContextualTestCase subject = null!;

                act = () =>
                {
                    subject = ContextualTestCase.CreateTestCase(
                        setupOnce: setupOnce,
                        setupEach: setupEach,
                        act: actAction,
                        teardownEach: teardownEach,
                        teardownOnce: teardownOnce,
                        tests: tests,
                        childContexts: childContexts);
                };

                it["should create a new instance of IContextualTestCase"] = () =>
                {
                    Check.That(subject).IsNotNull();
                    Check.That(subject).InheritsFrom<IContextualTestCase>();
                };

                ITestCaseTests(() => subject);

                it["should set the ChildContexts property"] = () =>
                {
                    Check.That(subject.ChildContexts).IsEqualTo(childContexts);
                };
            };

            describe["CreateTestCaseContext static factory method"] = () => 
            {
                ITestCaseContext subject = null!;
                act = () =>
                {
                    subject = ContextualTestCase.CreateTestCaseContext(
                        description: description,
                        setupOnce: setupOnce,
                        setupEach: setupEach,
                        act: actAction,
                        teardownEach: teardownEach,
                        teardownOnce: teardownOnce,
                        tests: tests,
                        childContexts: childContexts);
                };


                it["should create a new instance of IContextualTestCase"] = () =>
                {
                    Check.That(subject).IsNotNull();
                    Check.That(subject).InheritsFrom<ITestCaseContext>();
                };

                it["should set the Description property"] = () =>
                {
                    Check.That(subject.Description).IsEqualTo(description);
                };

                ITestCaseTests(() => subject);
            };
        }

        private void ITestCaseTests(Func<ITestCase> subjectFactory)
        {
            it["should set the SetupOnce property"] = () =>
            {
                Check.That(subjectFactory().SetupOnce).IsEqualTo(setupOnce);
            };

            it["should set the SetupEach property"] = () =>
            {
                Check.That(subjectFactory().SetupEach).IsEqualTo(setupEach);
            };

            it["should set the Act property"] = () =>
            {
                Check.That(subjectFactory().Act).IsEqualTo(actAction);
            };

            it["should set the TeardownEach property"] = () =>
            {
                Check.That(subjectFactory().TeardownEach).IsEqualTo(teardownEach);
            };

            it["should set the TeardownOnce property"] = () =>
            {
                Check.That(subjectFactory().TeardownOnce).IsEqualTo(teardownOnce);
            };

            it["should set the Tests property"] = () =>
            {
                Check.That(subjectFactory().Tests).IsEqualTo(tests);
            };
        }
    }
}
