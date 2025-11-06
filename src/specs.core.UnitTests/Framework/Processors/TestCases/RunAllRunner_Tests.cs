using NFluent;
using NUnit.Framework;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public class RunAllRunner_Tests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        private void RunAllRunner_Specs()
        {
            RunAllRunner subject = null!;
            List<Action> actionList = null!;

            List<int> callOrder = null!;
            Action trackAction(int n) => () => callOrder.Add(n);

            beforeEach = () =>
            {
                actionList = [];
                callOrder = [];
            };

            describe["RunAll() with empty action list..."] = () =>
            {
                beforeEach = () =>
                {
                    subject = new RunAllRunner(actionList);
                };

                act = () =>
                {
                    subject.RunAll();
                };

                it["should complete without error"] = () =>
                {
                    Check.That(callOrder).IsEmpty();
                };

                it["should allow multiple invocations without error"] = () =>
                {
                    subject.RunAll();
                    subject.RunAll();
                    Check.That(callOrder).IsEmpty();
                };
            };

            describe["RunAll() with single action..."] = () =>
            {
                beforeEach = () =>
                {
                    actionList.Add(trackAction(1));
                    subject = new RunAllRunner(actionList);
                };

                act = () =>
                {
                    subject.RunAll();
                };

                it["should execute the action"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["RunAll() with multiple actions..."] = () =>
            {
                beforeEach = () =>
                {
                    actionList.Add(trackAction(1));
                    actionList.Add(trackAction(2));
                    actionList.Add(trackAction(3));
                    subject = new RunAllRunner(actionList);
                };

                act = () =>
                {
                    subject.RunAll();
                };

                it["should execute all actions in order"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["RunAll() called multiple times..."] = () =>
            {
                beforeEach = () =>
                {
                    actionList.Add(trackAction(1));
                    actionList.Add(trackAction(2));
                    actionList.Add(trackAction(3));
                    subject = new RunAllRunner(actionList);
                };

                act = () =>
                {
                    subject.RunAll();
                    subject.RunAll();
                    subject.RunAll();
                };

                it["should execute actions only once (idempotency guarantee)"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["RunAll() with exception in first action..."] = () =>
            {
                Exception? caughtException = null;

                beforeEach = () =>
                {
                    actionList.Add(() => throw new InvalidOperationException("First action failed"));
                    actionList.Add(trackAction(2));
                    subject = new RunAllRunner(actionList);
                };

                act = () =>
                {
                    try
                    {
                        subject.RunAll();
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }
                };

                it["should throw the exception"] = () =>
                {
                    Check.That(caughtException).IsNotNull();
                    Check.That(caughtException).IsInstanceOf<InvalidOperationException>();
                    Check.That(caughtException!.Message).IsEqualTo("First action failed");
                };

                it["should not execute subsequent actions"] = () =>
                {
                    Check.That(callOrder).IsEmpty();
                };

                it["should not set hasRuntoCompletion flag"] = () =>
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

            describe["RunAll() with exception in middle action..."] = () =>
            {
                Exception? caughtException = null;

                beforeEach = () =>
                {
                    actionList.Add(trackAction(1));
                    actionList.Add(() => throw new InvalidOperationException("Middle action failed"));
                    actionList.Add(trackAction(3));
                    subject = new RunAllRunner(actionList);
                };

                act = () =>
                {
                    try
                    {
                        subject.RunAll();
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }
                };

                it["should execute actions up to the exception"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["should not execute actions after the exception"] = () =>
                {
                    Check.That(callOrder).Not.Contains(3);
                };

                it["should retry from beginning on second invocation"] = () =>
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

            describe["RunAll() with exception in last action..."] = () =>
            {
                Exception? caughtException = null;

                beforeEach = () =>
                {
                    actionList.Add(trackAction(1));
                    actionList.Add(trackAction(2));
                    actionList.Add(() => throw new InvalidOperationException("Last action failed"));
                    subject = new RunAllRunner(actionList);
                };

                act = () =>
                {
                    try
                    {
                        subject.RunAll();
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }
                };

                it["should execute all actions before the exception"] = () =>
                {
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["should throw the exception"] = () =>
                {
                    Check.That(caughtException).IsNotNull();
                    Check.That(caughtException).IsInstanceOf<InvalidOperationException>();
                };

                it["should allow retry on second invocation"] = () =>
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

            describe["RunAll() with different exception types..."] = () =>
            {
                context["ArgumentNullException"] = () =>
                {
                    Exception? caughtException = null;

                    beforeEach = () =>
                    {
                        actionList.Add(() => throw new ArgumentNullException("param"));
                        subject = new RunAllRunner(actionList);
                    };

                    act = () =>
                    {
                        try
                        {
                            subject.RunAll();
                        }
                        catch (Exception ex)
                        {
                            caughtException = ex;
                        }
                    };

                    it["should propagate ArgumentNullException"] = () =>
                    {
                        Check.That(caughtException).IsInstanceOf<ArgumentNullException>();
                    };
                };

                context["NotImplementedException"] = () =>
                {
                    Exception? caughtException = null;

                    beforeEach = () =>
                    {
                        actionList.Add(() => throw new NotImplementedException("Not done yet"));
                        subject = new RunAllRunner(actionList);
                    };

                    act = () =>
                    {
                        try
                        {
                            subject.RunAll();
                        }
                        catch (Exception ex)
                        {
                            caughtException = ex;
                        }
                    };

                    it["should propagate NotImplementedException"] = () =>
                    {
                        Check.That(caughtException).IsInstanceOf<NotImplementedException>();
                    };
                };
            };

            describe["Multiple independent RunAllRunner instances..."] = () =>
            {
                RunAllRunner runner1 = null!;
                RunAllRunner runner2 = null!;
                List<Action> list1 = null!;
                List<Action> list2 = null!;

                beforeEach = () =>
                {
                    list1 = [trackAction(1)];
                    list2 = [trackAction(2)];

                    runner1 = new RunAllRunner(list1);
                    runner2 = new RunAllRunner(list2);
                };

                act = () =>
                {
                    runner1.RunAll();
                    runner2.RunAll();
                };

                it["should not share state"] = () =>
                {
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["should maintain independent idempotency"] = () =>
                {
                    runner1.RunAll(); // Should not execute again
                    runner2.RunAll(); // Should not execute again

                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["RunAll() after successful completion..."] = () =>
            {
                beforeEach = () =>
                {
                    actionList.Add(trackAction(1));
                    actionList.Add(trackAction(2));
                    subject = new RunAllRunner(actionList);

                    // First successful run
                    subject.RunAll();

                    // Clear call order to verify second run doesn't execute
                    callOrder.Clear();
                };

                act = () =>
                {
                    subject.RunAll();
                };

                it["should not execute actions again"] = () =>
                {
                    Check.That(callOrder).IsEmpty();
                };
            };

            describe["RunAll() with action modifying shared state..."] = () =>
            {
                int sharedCounter = 0;

                beforeEach = () =>
                {
                    sharedCounter = 0;
                    actionList.Add(() => sharedCounter++);
                    actionList.Add(() => sharedCounter++);
                    subject = new RunAllRunner(actionList);
                };

                act = () =>
                {
                    subject.RunAll();
                    subject.RunAll();
                    subject.RunAll();
                };

                it["should modify shared state only once"] = () =>
                {
                    Check.That(sharedCounter).IsEqualTo(2);
                };
            };
        }
    }
}
