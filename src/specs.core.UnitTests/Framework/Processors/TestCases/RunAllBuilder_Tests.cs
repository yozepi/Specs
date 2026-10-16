using NFluent;
using NUnit.Framework;
using specs.core.Framework.Processors.TestCases;
using specs.BDD;
using specs.BDD.Attributes;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public class RunAllBuilder_Tests : Specifications
    {

        [Context]
        public void RunAllBuilder_Specs()
        {
            RunAllBuilder subject = null!;
            Action? result = null!;

            List<int> callOrder = null!;
            Action trackAction(int n) => () => callOrder.Add(n);

            BeforeEach(() =>
            {
                subject = new RunAllBuilder();
                result = null;
                callOrder = [];
            });

            Describe["Build() with no actions added..."] = () =>
            {
                Act(() =>
                {
                    result = subject.Build();
                });

                It["should return null"] = () =>
                {
                    Check.That(result).IsNull();
                };
            };

            Describe["Build() with single action added..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddAction(trackAction(1));
                });

                Act(() =>
                {
                    result = subject.Build();
                    result?.Invoke();
                });

                It["should return non-null Action"] = () =>
                {
                    Check.That(result).IsNotNull();
                };

                It["should execute the action when invoked"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["Build() with multiple actions added..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddAction(trackAction(1));
                    subject.AddAction(trackAction(2));
                    subject.AddAction(trackAction(3));
                });

                Act(() =>
                {
                    result = subject.Build();
                    result?.Invoke();
                });

                It["should execute all actions in order"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["Build() called multiple times without adding new actions..."] = () =>
            {
                Action? firstBuild = null;
                Action? secondBuild = null;

                BeforeEach(() =>
                {
                    subject.AddAction(trackAction(1));
                });

                Act(() =>
                {
                    firstBuild = subject.Build();
                    secondBuild = subject.Build();
                });

                It["should return the same Action instance (cached)"] = () =>
                {
                    Check.That(firstBuild).IsSameReferenceAs(secondBuild);
                };

                It["should execute action only once when both are invoked"] = () =>
                {
                    firstBuild?.Invoke();
                    secondBuild?.Invoke();

                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["AddAction() called after Build()..."] = () =>
            {
                Action? firstBuild = null;
                Action? secondBuild = null;

                BeforeEach(() =>
                {
                    subject.AddAction(trackAction(1));
                    firstBuild = subject.Build();

                    // Invoke first build before adding more actions
                    firstBuild?.Invoke();

                    subject.AddAction(trackAction(2));
                    secondBuild = subject.Build();
                });

                It["should create a new Action instance"] = () =>
                {
                    Check.That(firstBuild).Not.IsSameReferenceAs(secondBuild);
                };

                It["first build should have executed with only original action"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                It["second build should execute all accumulated actions"] = () =>
                {
                    callOrder.Clear();
                    secondBuild?.Invoke();
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                It["first build should not execute again (idempotency)"] = () =>
                {
                    int beforeCount = callOrder.Count;
                    firstBuild?.Invoke();
                    Check.That(callOrder.Count).IsEqualTo(beforeCount);
                };
            };

            Describe["AddAction() with null parameter..."] = () =>
            {
                It["should throw ArgumentNullException"] = () =>
                {
                    Check.ThatCode(() => subject.AddAction(null!))
                        .Throws<ArgumentNullException>();
                };
            };

            Describe["Built action invoked multiple times..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject.AddAction(trackAction(1));
                    subject.AddAction(trackAction(2));
                });

                Act(() =>
                {
                    result = subject.Build();
                    result?.Invoke();
                    result?.Invoke();
                    result?.Invoke();
                });

                It["should execute actions only once (idempotency guarantee)"] = () =>
                {
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["Built action with exception in first action..."] = () =>
            {
                Exception? caughtException = null;

                BeforeEach(() =>
                {
                    subject.AddAction(() => throw new InvalidOperationException("First action failed"));
                    subject.AddAction(trackAction(2));
                });

                Act(() =>
                {
                    result = subject.Build();
                    try
                    {
                        result?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }
                });

                It["should throw the exception"] = () =>
                {
                    Check.That(caughtException).IsNotNull();
                    Check.That(caughtException).IsInstanceOf<InvalidOperationException>();
                    Check.That(caughtException!.Message).IsEqualTo("First action failed");
                };

                It["should not execute subsequent actions"] = () =>
                {
                    Check.That(callOrder).IsEmpty();
                };

                It["should allow retry on second invocation"] = () =>
                {
                    // Clear previous exception
                    caughtException = null;

                    // Second invocation should retry from beginning
                    try
                    {
                        result?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }

                    Check.That(caughtException).IsNotNull();
                    Check.That(caughtException).IsInstanceOf<InvalidOperationException>();
                };
            };

            Describe["Built action with exception in middle action..."] = () =>
            {
                Exception? caughtException = null;

                BeforeEach(() =>
                {
                    subject.AddAction(trackAction(1));
                    subject.AddAction(() => throw new InvalidOperationException("Middle action failed"));
                    subject.AddAction(trackAction(3));
                });

                Act(() =>
                {
                    result = subject.Build();
                    try
                    {
                        result?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }
                });

                It["should execute actions up to the exception"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                It["should not execute actions after the exception"] = () =>
                {
                    Check.That(callOrder).Not.Contains(3);
                };

                It["should retry from beginning on second invocation"] = () =>
                {
                    callOrder.Clear();
                    caughtException = null;

                    try
                    {
                        result?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }

                    // Should start over from action 1
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["Multiple independent RunAllBuilder instances..."] = () =>
            {
                RunAllBuilder builder1 = null!;
                RunAllBuilder builder2 = null!;
                Action? action1 = null;
                Action? action2 = null;

                BeforeEach(() =>
                {
                    builder1 = new RunAllBuilder();
                    builder2 = new RunAllBuilder();

                    builder1.AddAction(trackAction(1));
                    builder2.AddAction(trackAction(2));

                    action1 = builder1.Build();
                    action2 = builder2.Build();
                });

                It["should not share state"] = () =>
                {
                    action1?.Invoke();
                    action2?.Invoke();

                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                It["should maintain independent idempotency"] = () =>
                {
                    action1?.Invoke();
                    action1?.Invoke(); // Should not execute again

                    action2?.Invoke();
                    action2?.Invoke(); // Should not execute again

                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };
        }
    }
}
