using specs.BDD;
using specs.BDD.Attributes;

namespace specs.BDD.VSTestAdapter.Tests.TestFixtures
{
    // ── Valid specification classes ───────────────────────────────────────────────────────────

    /// <summary>
    /// A minimal valid spec with one [Context] method containing two direct tests.
    /// </summary>
    public class SimpleSpec : Specifications
    {
        [Context("simple context")]
        public void describe_something()
        {
            It["test one"] = () => { };
            It["test two"] = () => { };
        }
    }

    /// <summary>
    /// A spec that uses the method name as the context path (no description on [Context]).
    /// </summary>
    public class SpecWithNoContextDescription : Specifications
    {
        [Context]
        public void describe_addition()
        {
            It["should work"] = () => { };
        }
    }

    /// <summary>
    /// A spec with multiple [Context] methods — each method is an independent feature set.
    /// </summary>
    public class MultiContextSpec : Specifications
    {
        [Context("feature one")]
        public void first_feature()
        {
            It["test A"] = () => { };
        }

        [Context("feature two")]
        public void second_feature()
        {
            It["test B"] = () => { };
            It["test C"] = () => { };
        }
    }

    /// <summary>
    /// A spec with one level of nested Context blocks.
    /// </summary>
    public class NestedContextSpec : Specifications
    {
        [Context("root context")]
        public void describe_something()
        {
            Context["when doing X"] = () =>
            {
                It["should work for X"] = () => { };
            };

            Context["when doing Y"] = () =>
            {
                It["should work for Y"] = () => { };
            };
        }
    }

    /// <summary>
    /// A spec with deeply nested Context blocks (3 levels deep).
    /// </summary>
    public class DeeplyNestedContextSpec : Specifications
    {
        [Context("root")]
        public void describe_something()
        {
            Context["level one"] = () =>
            {
                Context["level two"] = () =>
                {
                    It["deep test"] = () => { };
                };
            };
        }
    }

    /// <summary>
    /// A spec with tests both at the root level and inside nested contexts.
    /// </summary>
    public class MixedDepthSpec : Specifications
    {
        [Context("mixed depth")]
        public void describe_something()
        {
            It["root level test"] = () => { };

            Context["nested context"] = () =>
            {
                It["nested test"] = () => { };
            };
        }
    }

    // ── Invalid specification classes ─────────────────────────────────────────────────────────

    /// <summary>
    /// INVALID: A Specifications subclass with no [Context] methods.
    /// Discovery should throw <see cref="Discovery.InvalidSpecificationException"/>.
    /// </summary>
    public class SpecWithNoContextMethods : Specifications
    {
        public void not_a_context()
        {
            It["should not be discovered"] = () => { };
        }
    }

    // ── Execution test fixtures ───────────────────────────────────────────────────────────────

    /// <summary>
    /// A spec with one passing and one always-passing test, plus one test that only fails when
    /// <see cref="TriggerFailure"/> is set to <c>true</c>.
    /// This gate prevents the intentional failure from surfacing when the adapter discovers and
    /// auto-runs this fixture during the normal test run; tests that verify failure behavior must
    /// set the flag before calling <c>TestInvoker.Invoke</c>.
    /// </summary>
    public class SpecWithMixedResults : Specifications
    {
        /// <summary>
        /// When <c>true</c>, "a failing test" throws. Defaults to <c>false</c>.
        /// Must be reset to <c>false</c> after each test that sets it.
        /// </summary>
        public static bool TriggerFailure { get; set; } = false;

        [Context("mixed results")]
        public void describe_mixed()
        {
            It["a passing test"] = () => { };
            It["a failing test"] = () =>
            {
                if (TriggerFailure)
                    throw new InvalidOperationException("intentional failure");
            };
        }
    }

    /// <summary>
    /// A spec that records lifecycle execution order into a shared log.
    /// Tests must call <see cref="ClearLog"/> before invoking, then inspect <see cref="Log"/>
    /// after. The static log is intentional here — it is the only way to observe what the
    /// framework did inside a fresh Specifications instance during a single test run.
    /// </summary>
    public class SpecWithLifecycle : Specifications
    {
        /// <summary>Execution log — populated by lifecycle hooks.</summary>
        public static readonly System.Collections.Generic.List<string> Log =
            new System.Collections.Generic.List<string>();

        /// <summary>Clears the log. Call before each test that uses this fixture.</summary>
        public static void ClearLog() => Log.Clear();

        [Context("lifecycle spec")]
        public void describe_lifecycle()
        {
            BeforeAll(() => Log.Add("BeforeAll"));
            BeforeEach(() => Log.Add("BeforeEach"));
            Act(() => Log.Add("Act"));
            AfterEach(() => Log.Add("AfterEach"));
            AfterAll(() => Log.Add("AfterAll"));

            It["records execution order"] = () => { };
        }
    }

    // ── Non-specification types ───────────────────────────────────────────────────────────────

    /// <summary>Non-abstract class that does NOT inherit from Specifications.</summary>
    public class NotASpec
    {
        public void SomeMethod() { }
    }

    /// <summary>Abstract subclass of Specifications — should be excluded from scanning.</summary>
    public abstract class AbstractSpec : Specifications
    {
        [Context]
        public void some_context() { }
    }
}
