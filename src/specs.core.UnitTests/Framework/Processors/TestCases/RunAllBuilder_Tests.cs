using NFluent;
using NUnit.Framework;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.core.UnitTests.Framework.Processors.TestCases
{
    public class RunAllBuilder_Tests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        private void RunAllBuilder_Specs()
        {
            RunAllBuilder subject = null!;
            Action? result = null!;

            List<int> callOrder = null!;
            Action trackAction(int n) => () => callOrder.Add(n);

            beforeEach = () =>
            {
                subject = new RunAllBuilder();
                result = null;
                callOrder = [];
            };

            describe["Build() with no actions added..."] = () =>
            {
                act = () =>
                {
                    result = subject.Build();
                };

                it["should return null"] = () =>
                {
                    Check.That(result).IsNull();
                };
            };

            describe["Build() with single action added..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddAction(trackAction(1));
                };

                act = () =>
                {
                    result = subject.Build();
                    result?.Invoke();
                };

                it["should return non-null Action"] = () =>
                {
                    Check.That(result).IsNotNull();
                };

                it["should execute the action when invoked"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["Build() with multiple actions added..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddAction(trackAction(1));
                    subject.AddAction(trackAction(2));
                    subject.AddAction(trackAction(3));
                };

                act = () =>
                {
                    result = subject.Build();
                    result?.Invoke();
                };

                it["should execute all actions in order"] = () =>
                {
                    int[] expected = { 1, 2, 3 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["Build() called multiple times without adding new actions..."] = () =>
            {
                Action? firstBuild = null;
                Action? secondBuild = null;

                beforeEach = () =>
                {
                    subject.AddAction(trackAction(1));
                };

                act = () =>
                {
                    firstBuild = subject.Build();
                    secondBuild = subject.Build();
                };

                it["should return the same Action instance (cached)"] = () =>
                {
                    Check.That(firstBuild).IsSameReferenceAs(secondBuild);
                };

                it["should execute action only once when both are invoked"] = () =>
                {
                    firstBuild?.Invoke();
                    secondBuild?.Invoke();

                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["AddAction() called after Build()..."] = () =>
            {
                Action? firstBuild = null;
                Action? secondBuild = null;

                beforeEach = () =>
                {
                    subject.AddAction(trackAction(1));
                    firstBuild = subject.Build();

                    // Invoke first build before adding more actions
                    firstBuild?.Invoke();

                    subject.AddAction(trackAction(2));
                    secondBuild = subject.Build();
                };

                it["should create a new Action instance"] = () =>
                {
                    Check.That(firstBuild).Not.IsSameReferenceAs(secondBuild);
                };

                it["first build should have executed with only original action"] = () =>
                {
                    int[] expected = { 1 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["second build should execute all accumulated actions"] = () =>
                {
                    callOrder.Clear();
                    secondBuild?.Invoke();
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["first build should not execute again (idempotency)"] = () =>
                {
                    int beforeCount = callOrder.Count;
                    firstBuild?.Invoke();
                    Check.That(callOrder.Count).IsEqualTo(beforeCount);
                };
            };

            describe["AddAction() with null parameter..."] = () =>
            {
                it["should throw NullReferenceException"] = () =>
                {
                    Check.ThatCode(() => subject.AddAction(null!))
                        .Throws<NullReferenceException>();
                };
            };

            describe["Built action invoked multiple times..."] = () =>
            {
                beforeEach = () =>
                {
                    subject.AddAction(trackAction(1));
                    subject.AddAction(trackAction(2));
                };

                act = () =>
                {
                    result = subject.Build();
                    result?.Invoke();
                    result?.Invoke();
                    result?.Invoke();
                };

                it["should execute actions only once (idempotency guarantee)"] = () =>
                {
                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };
            };

            describe["Built action with exception in first action..."] = () =>
            {
                Exception? caughtException = null;

                beforeEach = () =>
                {
                    subject.AddAction(() => throw new InvalidOperationException("First action failed"));
                    subject.AddAction(trackAction(2));
                };

                act = () =>
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

                it["should allow retry on second invocation"] = () =>
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

            describe["Built action with exception in middle action..."] = () =>
            {
                Exception? caughtException = null;

                beforeEach = () =>
                {
                    subject.AddAction(trackAction(1));
                    subject.AddAction(() => throw new InvalidOperationException("Middle action failed"));
                    subject.AddAction(trackAction(3));
                };

                act = () =>
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

            describe["Multiple independent RunAllBuilder instances..."] = () =>
            {
                RunAllBuilder builder1 = null!;
                RunAllBuilder builder2 = null!;
                Action? action1 = null;
                Action? action2 = null;

                beforeEach = () =>
                {
                    builder1 = new RunAllBuilder();
                    builder2 = new RunAllBuilder();

                    builder1.AddAction(trackAction(1));
                    builder2.AddAction(trackAction(2));

                    action1 = builder1.Build();
                    action2 = builder2.Build();
                };

                it["should not share state"] = () =>
                {
                    action1?.Invoke();
                    action2?.Invoke();

                    int[] expected = { 1, 2 };
                    Check.That(callOrder).ContainsExactly(expected);
                };

                it["should maintain independent idempotency"] = () =>
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
