using NFluent;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    partial class ContextualTestCaseBuilder_Tests
    {
        [Context("Pending Specs")]
        public void Pending_Specs()
        {
            ContextualTestCaseBuilder subject = null!;
            IContextualTestCase subjectTestCase = null!;
            bool bodyInvoked = false;

            BeforeEach(() =>
            {
                subject = new ContextualTestCaseBuilder();
                subjectTestCase = null!;
                bodyInvoked = false;
            });

            Act(() => subjectTestCase = subject.Build());

            Describe["AddPendingTest with an Action..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddTest("runnable", () => { });
                    subject.AddPendingTest("pending", () => bodyInvoked = true);
                });

                It["should include the pending test in Tests"] = () =>
                {
                    Check.That(subjectTestCase.Tests.Select(t => t.description)).ContainsExactly("runnable", "pending");
                };

                It["should list only the pending test in PendingTests"] = () =>
                {
                    Check.That(subjectTestCase.PendingTests).ContainsExactly("pending");
                };

                It["should not invoke the pending body"] = () =>
                {
                    Check.That(bodyInvoked).IsFalse();
                };
            };

            Describe["AddPendingTest with a Func<Task>..."] = () =>
            {
                Func<Task> body = null!;

                BeforeEach(() =>
                {
                    body = () => { bodyInvoked = true; return Task.CompletedTask; };
                    subject.AddPendingTest("pending async", body);
                });

                It["should list the test in PendingTests"] = () =>
                {
                    Check.That(subjectTestCase.PendingTests).ContainsExactly("pending async");
                };

                It["should keep the original Func<Task> as the wrapper's Target"] = () =>
                {
                    Check.That(subjectTestCase.Tests[0].test.Target).IsSameReferenceAs(body);
                };
            };

            Describe["AddPendingTest with invalid arguments..."] = () =>
            {
                It["should throw when the description is blank"] = () =>
                {
                    Check.ThatCode(() => subject.AddPendingTest(" ", () => { })).Throws<ArgumentException>();
                };

                It["should throw when the Action is null"] = () =>
                {
                    Check.ThatCode(() => subject.AddPendingTest("pending", (Action)null!)).Throws<ArgumentNullException>();
                };

                It["should throw when the Func<Task> is null"] = () =>
                {
                    Check.ThatCode(() => subject.AddPendingTest("pending", (Func<Task>)null!)).Throws<ArgumentNullException>();
                };
            };

            Describe["a pending child context..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.ChildTestCase("pending child", child =>
                    {
                        child.AddTest("child test", () => { });
                        child.ChildTestCase("grandchild", grandChild => grandChild.AddTest("grandchild test", () => { }));
                    }, isPending: true);
                    subject.ChildTestCase("normal child", child => child.AddTest("normal test", () => { }));
                });

                It["should mark its tests as pending"] = () =>
                {
                    Check.That(subjectTestCase.ChildContexts[0].PendingTests).ContainsExactly("child test");
                };

                It["should mark its descendants' tests as pending"] = () =>
                {
                    var pendingChild = (IContextualTestCase)subjectTestCase.ChildContexts[0];
                    Check.That(pendingChild.ChildContexts[0].PendingTests).ContainsExactly("grandchild test");
                };

                It["should not mark sibling contexts' tests as pending"] = () =>
                {
                    Check.That(subjectTestCase.ChildContexts[1].PendingTests).IsEmpty();
                };
            };

            Describe["a regular child context..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.ChildTestCase("child", child => child.AddTest("child test", () => { }));
                });

                It["should have no pending tests"] = () =>
                {
                    Check.That(subjectTestCase.ChildContexts[0].PendingTests).IsEmpty();
                };
            };
        }
    }
}
