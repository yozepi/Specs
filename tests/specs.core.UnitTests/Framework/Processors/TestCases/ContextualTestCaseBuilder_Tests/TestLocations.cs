using NFluent;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    partial class ContextualTestCaseBuilder_Tests
    {
        [Describe("Test Locations")]
        public void Test_Locations()
        {
            ContextualTestCaseBuilder subject = null!;
            IContextualTestCase subjectTestCase = null!;

            BeforeEach(() =>
            {
                subject = new ContextualTestCaseBuilder();
                subjectTestCase = null!;
            });

            Act(() => subjectTestCase = subject.Build());

            Describe["a test without a recorded location..."] = () =>
            {
                BeforeEach(() => subject.AddTest("test", () => { }));

                It["should have no location"] = () =>
                {
                    Check.That(subjectTestCase.TestLocations).IsEmpty();
                };
            };

            Describe["AddTestLocation..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddPendingTest("pending", () => { });
                    subject.AddTestLocation("pending", "Specs.cs", 42);
                });

                It["should expose the location by description"] = () =>
                {
                    Check.That(subjectTestCase.TestLocations["pending"]).IsEqualTo(("Specs.cs", 42));
                };

                It["should replace a previously recorded location"] = () =>
                {
                    subject.AddTestLocation("pending", "Other.cs", 7);
                    Check.That(subject.Build().TestLocations["pending"]).IsEqualTo(("Other.cs", 7));
                };
            };

            Describe["AddTestLocation in a child context..."] = () =>
            {
                BeforeEach(() => subject.ChildTestCase("child", child =>
                {
                    child.AddTest("child test", () => { });
                    child.AddTestLocation("child test", "Child.cs", 3);
                }));

                It["should expose the location on the child context"] = () =>
                {
                    Check.That(subjectTestCase.ChildContexts[0].TestLocations["child test"]).IsEqualTo(("Child.cs", 3));
                };

                It["should not expose the location on the parent"] = () =>
                {
                    Check.That(subjectTestCase.TestLocations).IsEmpty();
                };
            };

            Describe["AddTestLocation with invalid arguments..."] = () =>
            {
                It["should throw when the description is blank"] = () =>
                {
                    Check.ThatCode(() => subject.AddTestLocation(" ", "Specs.cs", 1)).Throws<ArgumentException>();
                };

                It["should throw when the file path is blank"] = () =>
                {
                    Check.ThatCode(() => subject.AddTestLocation("test", " ", 1)).Throws<ArgumentException>();
                };
            };
        }
    }
}
