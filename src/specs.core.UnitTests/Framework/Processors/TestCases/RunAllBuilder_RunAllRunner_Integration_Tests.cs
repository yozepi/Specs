using NFluent;
using NUnit.Framework;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public class RunAllBuilder_RunAllRunner_Integration_Tests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        private void RunAllBuilder_RunAllRunner_Integration_Specs()
        {
            RunAllBuilder builder = null!;
            Action? builtAction = null!;

            List<int> callOrder = null!;
            Action trackAction(int n) => () => callOrder.Add(n);

            beforeEach = () =>
            {
                builder = new RunAllBuilder();
                builtAction = null;
                callOrder = [];
            };

            describe["Complete workflow: Build and Execute..."] = () =>
            {
                beforeEach = () =>
                {
                    builder.AddAction(trackAction(1));
                    builder.AddAction(trackAction(2));
                    builder.AddAction(trackAction(3));
                };

                act = () =>
                {
                    builtAction = builder.Build();
                    builtAction?.Invoke();
                };

                it["should execute all actions in order"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["Complete workflow: Build, invoke, rebuild pattern..."] = () =>
            {
                int firstInvocationCount = 0;
                int secondInvocationCount = 0;
                Action? firstBuild = null;
                Action? secondBuild = null;

                beforeEach = () =>
                {
                    // First build with 1 action
                    builder.AddAction(() => firstInvocationCount++);
                    firstBuild = builder.Build();
                    firstBuild?.Invoke();

                    // Add more actions and build again
                    builder.AddAction(() => secondInvocationCount++);
                    secondBuild = builder.Build();
                };

                it["first build should have executed once after its invocation"] = () =>
                {
                    Check.That(firstInvocationCount).IsEqualTo(1);
                };

                it["second build should execute all accumulated actions"] = () =>
                {
                    int beforeInvoke = firstInvocationCount;
                    secondBuild?.Invoke();
                    Check.That(firstInvocationCount).IsEqualTo(beforeInvoke + 1); // First action runs again
                    Check.That(secondInvocationCount).IsEqualTo(1); // Second action runs
                };
            };

            describe["Exception handling: Partial completion and retry..."] = () =>
            {
                Exception? firstException = null;
                bool shouldThrow = true;
                List<int> retryCallOrder = null!;

                beforeEach = () =>
                {
                    retryCallOrder = [];

                    builder.AddAction(() => retryCallOrder.Add(1));
                    builder.AddAction(() =>
                    {
                        if (shouldThrow)
                        {
                            throw new InvalidOperationException("Simulated failure");
                        }
                        retryCallOrder.Add(2);
                    });
                    builder.AddAction(() => retryCallOrder.Add(3));

                    builtAction = builder.Build();

                    // First invocation - should throw
                    try
                    {
                        builtAction?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        firstException = ex;
                    }
                };

                it["first invocation should throw exception"] = () =>
                {
                    Check.That(firstException).IsNotNull();
                    Check.That(firstException).IsInstanceOf<InvalidOperationException>();
                };

                it["first invocation should have partial execution"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(retryCallOrder).ContainsExactly(expected);
                };

                it["second invocation after fix should retry and succeed"] = () =>
                {
                    // Fix the issue and retry
                    shouldThrow = false;
                    retryCallOrder.Clear();

                    Exception? secondException = null;
                    try
                    {
                        builtAction?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        secondException = ex;
                    }

                    Check.That(secondException).IsNull();
                    int[] expected = { 1, 2, 3 };
                    Check.That(retryCallOrder).ContainsExactly(expected);
                };
            };

            describe["Exception handling: Exception always thrown..."] = () =>
            {
                int invocationCount = 0;

                beforeEach = () =>
                {
                    builder.AddAction(trackAction(1));
                    builder.AddAction(() =>
                    {
                        invocationCount++;
                        throw new InvalidOperationException("Always fails");
                    });
                    builder.AddAction(trackAction(3));

                    builtAction = builder.Build();
                };

                act = () =>
                {
                    // Try multiple times
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            builtAction?.Invoke();
                        }
                        catch
                        {
                            // Ignore exception
                        }
                    }
                };

                it["should retry from beginning each time"] = () =>
                {
                    Check.That(invocationCount).IsEqualTo(3);
                };

                it["should execute first action each time"] = () =>
                {
                    // First action executes 3 times (once per retry)
                    int count1 = callOrder.Count(x => x == 1);
                    Check.That(count1).IsEqualTo(3);
                };

                it["should never execute action after exception"] = () =>
                {
                    Check.That(callOrder).Not.Contains(3);
                };
            };

            describe["Complex scenario: Multiple builders with shared state..."] = () =>
            {
                RunAllBuilder builder1 = null!;
                RunAllBuilder builder2 = null!;
                Action? action1 = null;
                Action? action2 = null;
                int sharedState = 0;

                beforeEach = () =>
                {
                    builder1 = new RunAllBuilder();
                    builder2 = new RunAllBuilder();
                    sharedState = 0;

                    builder1.AddAction(() => sharedState += 1);
                    builder1.AddAction(() => sharedState += 10);

                    builder2.AddAction(() => sharedState += 100);
                    builder2.AddAction(() => sharedState += 1000);

                    action1 = builder1.Build();
                    action2 = builder2.Build();
                };

                act = () =>
                {
                    action1?.Invoke();
                    action2?.Invoke();
                    action1?.Invoke(); // Should not execute again
                    action2?.Invoke(); // Should not execute again
                };

                it["should execute each builder's actions once"] = () =>
                {
                    // builder1: 1 + 10 = 11
                    // builder2: 100 + 1000 = 1100
                    // Total: 1111
                    Check.That(sharedState).IsEqualTo(1111);
                };
            };

            describe["Performance: Many actions..."] = () =>
            {
                int actionCount = 100;
                int executionCount = 0;

                beforeEach = () =>
                {
                    executionCount = 0;  // Reset for each test

                    for (int i = 0; i < actionCount; i++)
                    {
                        builder.AddAction(() => executionCount++);
                    }

                    builtAction = builder.Build();
                    builtAction?.Invoke();  // Execute once
                };

                it["should execute all actions exactly once"] = () =>
                {
                    Check.That(executionCount).IsEqualTo(actionCount);
                };

                it["should not execute on subsequent invocations"] = () =>
                {
                    builtAction?.Invoke();
                    builtAction?.Invoke();
                    Check.That(executionCount).IsEqualTo(actionCount);  // Should still be 100
                };
            };

            describe["Edge case: Build, execute, add more, build again..."] = () =>
            {
                Action? firstBuild = null;
                Action? secondBuild = null;

                beforeEach = () =>
                {
                    builder.AddAction(trackAction(1));
                    firstBuild = builder.Build();
                    firstBuild.Invoke(); // Execute first build

                    // Add more actions after executing
                    builder.AddAction(trackAction(2));
                    secondBuild = builder.Build();
                };

                it["first build should have executed"] = () =>
                {
                    Check.That(callOrder).Contains(1);
                };

                it["second build should execute all actions"] = () =>
                {
                    callOrder.Clear();
                    secondBuild?.Invoke();
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["first build should not execute again"] = () =>
                {
                    int count = callOrder.Count(x => x == 1);
                    firstBuild?.Invoke();
                    int newCount = callOrder.Count(x => x == 1);
                    Check.That(newCount).IsEqualTo(count); // No increase
                };
            };

            describe["Edge case: Empty builder evolved to non-empty..."] = () =>
            {
                Action? firstBuild = null;
                Action? secondBuild = null;

                beforeEach = () =>
                {
                    firstBuild = builder.Build(); // Build with no actions

                    builder.AddAction(trackAction(1));
                    secondBuild = builder.Build();
                };

                it["first build should be null"] = () =>
                {
                    Check.That(firstBuild).IsNull();
                };

                it["second build should be non-null"] = () =>
                {
                    Check.That(secondBuild).IsNotNull();
                };

                it["second build should execute action"] = () =>
                {
                    secondBuild?.Invoke();
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };
        }
    }
}
