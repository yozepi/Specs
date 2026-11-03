using NFluent;
using specs.core.Framework.Processors.TestCases;
using specs.BDD;
using specs.BDD.Attributes;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public class RunAllRunner_Tests : Specifications
    {

        [Describe]
        public void RunAllRunner_Specs()
        {
            RunAllRunner subject = null!;
            List<Action> actionList = null!;

            List<int> callOrder = null!;
            Action trackAction(int n) => () => callOrder.Add(n);

            BeforeEach(() =>
            {
                actionList = [];
                callOrder = [];
            });

            Describe["RunAll() with empty action list..."] = () =>
            {
                BeforeEach(() =>
                {
                    subject = new RunAllRunner(actionList);
                });

                Act(() =>
                {
                    subject.RunAll();
                });

                It["should complete without error"] = () =>
                {
                    Check.That(callOrder).IsEmpty();
                };

                It["should allow multiple invocations without error"] = () =>
                {
                    subject.RunAll();
                    subject.RunAll();
                    Check.That(callOrder).IsEmpty();
                };
            };

            Describe["RunAll() with single action..."] = () =>
            {
                BeforeEach(() =>
                {
                    actionList.Add(trackAction(1));
                    subject = new RunAllRunner(actionList);
                });

                Act(() =>
                {
                    subject.RunAll();
                });

                It["should execute the action"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["RunAll() with multiple actions..."] = () =>
            {
                BeforeEach(() =>
                {
                    actionList.Add(trackAction(1));
                    actionList.Add(trackAction(2));
                    actionList.Add(trackAction(3));
                    subject = new RunAllRunner(actionList);
                });

                Act(() =>
                {
                    subject.RunAll();
                });

                It["should execute all actions in order"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["RunAll() called multiple times..."] = () =>
            {
                BeforeEach(() =>
                {
                    actionList.Add(trackAction(1));
                    actionList.Add(trackAction(2));
                    actionList.Add(trackAction(3));
                    subject = new RunAllRunner(actionList);
                });

                Act(() =>
                {
                    subject.RunAll();
                    subject.RunAll();
                    subject.RunAll();
                });

                It["should execute actions only once (idempotency guarantee)"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["RunAll() with exception in first action..."] = () =>
            {
                Exception? caughtException = null;

                BeforeEach(() =>
                {
                    actionList.Add(() => throw new InvalidOperationException("First action failed"));
                    actionList.Add(trackAction(2));
                    subject = new RunAllRunner(actionList);
                });

                Act(() =>
                {
                    try
                    {
                        subject.RunAll();
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

                It["should not set hasRuntoCompletion flag"] = () =>
                {
                    // Second invocation should retry
                    callOrder.Clear();
                    caughtException = null;

                    try
                    {
                        subject.RunAll();
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }

                    // Should throw again because hasRuntoCompletion was not set
                    Check.That(caughtException).IsNotNull();
                    Check.That(caughtException).IsInstanceOf<InvalidOperationException>();
                };
            };

            Describe["RunAll() with exception in middle action..."] = () =>
            {
                Exception? caughtException = null;

                BeforeEach(() =>
                {
                    actionList.Add(trackAction(1));
                    actionList.Add(() => throw new InvalidOperationException("Middle action failed"));
                    actionList.Add(trackAction(3));
                    subject = new RunAllRunner(actionList);
                });

                Act(() =>
                {
                    try
                    {
                        subject.RunAll();
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
                        subject.RunAll();
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

            Describe["RunAll() with exception in last action..."] = () =>
            {
                Exception? caughtException = null;

                BeforeEach(() =>
                {
                    actionList.Add(trackAction(1));
                    actionList.Add(trackAction(2));
                    actionList.Add(() => throw new InvalidOperationException("Last action failed"));
                    subject = new RunAllRunner(actionList);
                });

                Act(() =>
                {
                    try
                    {
                        subject.RunAll();
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }
                });

                It["should execute all actions before the exception"] = () =>
                {
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                It["should throw the exception"] = () =>
                {
                    Check.That(caughtException).IsNotNull();
                    Check.That(caughtException).IsInstanceOf<InvalidOperationException>();
                };

                It["should allow retry on second invocation"] = () =>
                {
                    callOrder.Clear();
                    caughtException = null;

                    try
                    {
                        subject.RunAll();
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }

                    // Should retry and execute actions 1 and 2 again
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["RunAll() with different exception types..."] = () =>
            {
                Context["ArgumentNullException"] = () =>
                {
                    Exception? caughtException = null;

                    BeforeEach(() =>
                    {
                        actionList.Add(() => throw new ArgumentNullException("param"));
                        subject = new RunAllRunner(actionList);
                    });

                    Act(() =>
                    {
                        try
                        {
                            subject.RunAll();
                        }
                        catch (Exception ex)
                        {
                            caughtException = ex;
                        }
                    });

                    It["should propagate ArgumentNullException"] = () =>
                    {
                        Check.That(caughtException).IsInstanceOf<ArgumentNullException>();
                    };
                };

                Context["NotImplementedException"] = () =>
                {
                    Exception? caughtException = null;

                    BeforeEach(() =>
                    {
                        actionList.Add(() => throw new NotImplementedException("Not done yet"));
                        subject = new RunAllRunner(actionList);
                    });

                    Act(() =>
                    {
                        try
                        {
                            subject.RunAll();
                        }
                        catch (Exception ex)
                        {
                            caughtException = ex;
                        }
                    });

                    It["should propagate NotImplementedException"] = () =>
                    {
                        Check.That(caughtException).IsInstanceOf<NotImplementedException>();
                    };
                };
            };

            Describe["Multiple independent RunAllRunner instances..."] = () =>
            {
                RunAllRunner runner1 = null!;
                RunAllRunner runner2 = null!;
                List<Action> list1 = null!;
                List<Action> list2 = null!;

                BeforeEach(() =>
                {
                    list1 = [trackAction(1)];
                    list2 = [trackAction(2)];

                    runner1 = new RunAllRunner(list1);
                    runner2 = new RunAllRunner(list2);
                });

                Act(() =>
                {
                    runner1.RunAll();
                    runner2.RunAll();
                });

                It["should not share state"] = () =>
                {
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                It["should maintain independent idempotency"] = () =>
                {
                    runner1.RunAll(); // Should not execute again
                    runner2.RunAll(); // Should not execute again

                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            Describe["RunAll() after successful completion..."] = () =>
            {
                BeforeEach(() =>
                {
                    actionList.Add(trackAction(1));
                    actionList.Add(trackAction(2));
                    subject = new RunAllRunner(actionList);

                    // First successful run
                    subject.RunAll();

                    // Clear call order to verify second run doesn't execute
                    callOrder.Clear();
                });

                Act(() =>
                {
                    subject.RunAll();
                });

                It["should not execute actions again"] = () =>
                {
                    Check.That(callOrder).IsEmpty();
                };
            };

            Describe["RunAll() with action modifying shared state..."] = () =>
            {
                int sharedCounter = 0;

                BeforeEach(() =>
                {
                    sharedCounter = 0;
                    actionList.Add(() => sharedCounter++);
                    actionList.Add(() => sharedCounter++);
                    subject = new RunAllRunner(actionList);
                });

                Act(() =>
                {
                    subject.RunAll();
                    subject.RunAll();
                    subject.RunAll();
                });

                It["should modify shared state only once"] = () =>
                {
                    Check.That(sharedCounter).IsEqualTo(2);
                };
            };
        }
    }
}
