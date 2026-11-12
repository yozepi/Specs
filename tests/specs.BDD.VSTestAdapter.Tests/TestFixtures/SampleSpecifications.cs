using specs.BDD;
using specs.BDD.Attributes;

namespace specs.BDD.VSTestAdapter.Tests.TestFixtures
{
    // ── Valid specification classes ───────────────────────────────────────────────────────────

    /// <summary>
    /// A minimal valid spec with one [Feature] method containing two direct tests.
    /// </summary>
    public class SimpleSpec : Specifications
    {
        [Feature("simple context")]
        public void describe_something()
        {
            It["test one"] = () => { };
            It["test two"] = () => { };
        }
    }

    /// <summary>
    /// A spec that uses the method name as the context path (no description on [Feature]).
    /// </summary>
    public class SpecWithNoContextDescription : Specifications
    {
        [Feature]
        public void describe_addition()
        {
            It["should work"] = () => { };
        }
    }

    /// <summary>
    /// A spec with multiple [Feature] methods — each method is an independent feature set.
    /// </summary>
    public class MultiContextSpec : Specifications
    {
        [Feature("feature one")]
        public void first_feature()
        {
            It["test A"] = () => { };
        }

        [Feature("feature two")]
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
        [Feature("root context")]
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
        [Feature("root")]
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
        [Feature("mixed depth")]
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
    /// INVALID: A Specifications subclass with no [Feature] methods.
    /// Discovery should throw <see cref="specs.BDD.Runner.InvalidSpecificationException"/>.
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

        [Feature("mixed results")]
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

        [Feature("lifecycle spec")]
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

    // ── Grouped-mode fixtures ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// A spec whose single [Feature] method opts into grouped execution — the whole subtree
    /// (one direct test + one nested context) runs as a single test result.
    /// </summary>
    public class GroupedSpec : Specifications
    {
        [Feature("grouped feature", Level = ReportingLevel.Feature)]
        public void describe_grouped()
        {
            It["direct test"] = () => { };

            Context["nested"] = () =>
            {
                It["nested test"] = () => { };
            };
        }
    }

    /// <summary>A grouped spec with an async It[...] test.</summary>
    public class GroupedAsyncSpec : Specifications
    {
        [Feature("grouped async", Level = ReportingLevel.Feature)]
        public void describe_grouped_async()
        {
            ItAsync["async test"] = async () =>
            {
                await System.Threading.Tasks.Task.Yield();
            };
        }
    }

    /// <summary>
    /// A spec that opts into grouped execution at the class level.
    /// </summary>
    [ReportingLevel(ReportingLevel.Feature)]
    public class ClassGroupedSpec : Specifications
    {
        [Feature("class grouped feature")]
        public void describe_class_grouped()
        {
            It["class grouped test"] = () => { };
        }

        [Feature("class grouped opt-out", Level = ReportingLevel.Test)]
        public void describe_opt_out()
        {
            It["opt-out test"] = () => { };
        }
    }

    /// <summary>
    /// Derives from a class-grouped spec; the class-level attribute is not inherited.
    /// </summary>
    public class DerivedFromClassGroupedSpec : ClassGroupedSpec
    {
    }

    /// <summary>
    /// A spec that mixes one grouped [Feature] method with one regular (per-It) method.
    /// </summary>
    public class MixedGroupingSpec : Specifications
    {
        [Feature("grouped part", Level = ReportingLevel.Feature)]
        public void grouped_part()
        {
            It["grouped test"] = () => { };
        }

        [Feature("flat part")]
        public void flat_part()
        {
            It["flat test"] = () => { };
        }
    }

    /// <summary>
    /// A grouped spec with a gated failing test (same TriggerFailure pattern as
    /// <see cref="SpecWithMixedResults"/> so the auto-run of this fixture stays green).
    /// </summary>
    public class GroupedSpecWithFailure : Specifications
    {
        /// <summary>When <c>true</c>, "a failing spec" throws. Reset after use.</summary>
        public static bool TriggerFailure { get; set; } = false;

        [Feature("grouped with failure", Level = ReportingLevel.Feature)]
        public void describe_grouped_failure()
        {
            It["a passing spec"] = () => { };

            Context["inner"] = () =>
            {
                It["a failing spec"] = () =>
                {
                    if (TriggerFailure)
                        throw new InvalidOperationException("grouped intentional failure");
                };
            };
        }
    }

    // ── Root-lifecycle fixtures ────────────────────────────────────────────────────────────────────────────────

    /// <summary>A spec that registers its whole lifecycle in the constructor (root level).</summary>
    public class SpecWithRootLifecycle : Specifications
    {
        public static readonly System.Collections.Generic.List<string> Log =
            new System.Collections.Generic.List<string>();

        public static void ClearLog() => Log.Clear();

        public SpecWithRootLifecycle()
        {
            BeforeAll(() => Log.Add("root-BeforeAll"));
            BeforeEach(() => Log.Add("root-BeforeEach"));
            Act(() => Log.Add("root-Act"));
            AfterEach(() => Log.Add("root-AfterEach"));
            AfterAll(() => Log.Add("root-AfterAll"));
        }

        [Feature("grouped part", Level = ReportingLevel.Feature)]
        public void grouped_part()
        {
            It["grouped test"] = () => Log.Add("test:grouped");
        }

        [Feature("flat part")]
        public void flat_part()
        {
            BeforeEach(() => Log.Add("ctx-BeforeEach"));
            AfterEach(() => Log.Add("ctx-AfterEach"));

            It["flat one"] = () => Log.Add("test:flat one");
            It["flat two"] = () => Log.Add("test:flat two");
        }
    }

    /// <summary>A spec whose constructor-registered AfterAll throws when <see cref="TriggerFailure"/> is set.</summary>
    public class SpecWithFailingRootAfterAll : Specifications
    {
        /// <summary>When <c>true</c>, the root AfterAll throws. Reset after use.</summary>
        public static bool TriggerFailure { get; set; }

        /// <summary>When <c>true</c>, the "first" spec throws. Reset after use.</summary>
        public static bool FailFirstSpec { get; set; }

        public SpecWithFailingRootAfterAll()
        {
            AfterAll(() =>
            {
                if (TriggerFailure)
                    throw new InvalidOperationException("root cleanup failure");
            });
        }

        [Feature("cleanup")]
        public void cleanup()
        {
            It["first"] = () =>
            {
                if (FailFirstSpec)
                    throw new InvalidOperationException("spec failure");
            };
            It["second"] = () => { };
        }
    }

    /// <summary>A spec whose constructor-registered BeforeAll throws when <see cref="TriggerFailure"/> is set.</summary>
    public class SpecWithFailingRootBeforeAll : Specifications
    {
        /// <summary>When <c>true</c>, the root BeforeAll throws. Reset after use.</summary>
        public static bool TriggerFailure { get; set; } = false;

        public static bool AfterAllRan { get; set; }

        public SpecWithFailingRootBeforeAll()
        {
            BeforeAll(() =>
            {
                if (TriggerFailure)
                    throw new InvalidOperationException("root setup failure");
            });
            AfterAll(() => AfterAllRan = true);
        }

        [Feature("grouped part", Level = ReportingLevel.Feature)]
        public void grouped_part()
        {
            It["grouped test"] = () => { };

            Context["nested"] = () =>
            {
                It["nested test"] = () => { };
            };
        }

        [Feature("flat part")]
        public void flat_part()
        {
            It["flat one"] = () => { };
            It["flat two"] = () => { };
        }
    }

    // ── Pending fixtures ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// One runnable spec plus every flavour of pending spec. Pending bodies throw so that
    /// accidentally executing one is a visible failure.
    /// </summary>
    public class SpecWithPending : Specifications
    {
        public static readonly System.Collections.Generic.List<string> Log =
            new System.Collections.Generic.List<string>();

        public static void ClearLog() => Log.Clear();

        [Feature("pending feature")]
        public void describe_pending()
        {
            BeforeEach(() => Log.Add("BeforeEach"));
            Act(() => Log.Add("Act"));
            AfterEach(() => Log.Add("AfterEach"));

            It["runnable"] = () => Log.Add("test:runnable");
            It["todo"] = TODO;
            ItAsync["todo async"] = TODOAsync;
            xIt["excluded"] = () => throw new InvalidOperationException("pending body must not run");

            xContext["pending context"] = () =>
            {
                BeforeAll(() => Log.Add("pending-BeforeAll"));
                AfterAll(() => Log.Add("pending-AfterAll"));

                It["inside pending context"] = () => throw new InvalidOperationException("pending body must not run");
            };
        }
    }

    /// <summary>A spec whose only selected specs are pending; the root BeforeAll must not run.</summary>
    public class AllPendingSpec : Specifications
    {
        public static bool RootBeforeAllRan { get; set; }

        public AllPendingSpec()
        {
            BeforeAll(() => RootBeforeAllRan = true);
        }

        [Feature("all pending")]
        public void describe_all_pending()
        {
            It["todo one"] = TODO;

            xDescribe["later"] = () =>
            {
                It["todo two"] = () => throw new InvalidOperationException("pending body must not run");
            };
        }
    }

    /// <summary>A spec with a gated failing root BeforeAll alongside a pending spec.</summary>
    public class PendingWithFailingRootBeforeAll : Specifications
    {
        /// <summary>When <c>true</c>, the root BeforeAll throws. Reset after use.</summary>
        public static bool TriggerFailure { get; set; } = false;

        public PendingWithFailingRootBeforeAll()
        {
            BeforeAll(() =>
            {
                if (TriggerFailure)
                    throw new InvalidOperationException("root setup failure");
            });
        }

        [Feature("flat part")]
        public void flat_part()
        {
            It["runnable"] = () => { };
            It["todo"] = TODO;
        }
    }

    /// <summary>Grouped contexts mixing runnable and pending specs, and one that is entirely pending.</summary>
    public class GroupedSpecWithPending : Specifications
    {
        public static readonly System.Collections.Generic.List<string> Log =
            new System.Collections.Generic.List<string>();

        public static void ClearLog() => Log.Clear();

        [Feature("grouped with pending", Level = ReportingLevel.Feature)]
        public void describe_grouped_pending()
        {
            It["a passing spec"] = () => { };
            It["a todo spec"] = TODO;

            xContext["pending inner"] = () =>
            {
                BeforeAll(() => Log.Add("pending-BeforeAll"));
                It["inner spec"] = () => throw new InvalidOperationException("pending body must not run");
            };
        }

        [Feature("grouped all pending", Level = ReportingLevel.Feature)]
        public void describe_grouped_all_pending()
        {
            BeforeAll(() => Log.Add("all-pending-BeforeAll"));

            It["a todo spec"] = TODO;
            xIt["an excluded spec"] = () => throw new InvalidOperationException("pending body must not run");
        }
    }

    // ── ShouldThrow fixtures ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Exercises ShouldThrow. Every spec passes unless <see cref="TriggerFailure"/> is set.
    /// </summary>
    public class ShouldThrowSpec : Specifications
    {
        /// <summary>When <c>true</c>, every spec outside "when Act throws" fails. Reset after use.</summary>
        public static bool TriggerFailure { get; set; } = false;

        public static readonly System.Collections.Generic.List<string> Log =
            new System.Collections.Generic.List<string>();

        public static void ClearLog() => Log.Clear();

        [Feature("should throw")]
        public void describe_should_throw()
        {
            AfterEach(() => Log.Add("AfterEach"));

            Context["when Act throws"] = () =>
            {
                Act(() => throw new DivideByZeroException("Attempted to divide by zero."));

                It["the thrown type"] = ShouldThrow<DivideByZeroException>();
                It["a base type"] = ShouldThrow<ArithmeticException>();
                It["a verified exception"] = ShouldThrow<DivideByZeroException>(ex =>
                {
                    if (!ex.Message.ToLowerInvariant().Contains("zero"))
                        throw new InvalidOperationException("message should mention zero");
                });
            };

            Context["when the verifier may fail"] = () =>
            {
                Act(() => throw new DivideByZeroException("Attempted to divide by zero."));

                It["a verified exception"] = ShouldThrow<DivideByZeroException>(_ =>
                {
                    if (TriggerFailure)
                        throw new InvalidOperationException("verify failed");
                });
            };

            Context["when Act may not throw"] = () =>
            {
                Act(() =>
                {
                    if (!TriggerFailure)
                        throw new DivideByZeroException();
                });

                It["an expected exception"] = ShouldThrow<DivideByZeroException>();
            };

            Context["when Act may throw another type"] = () =>
            {
                Act(() =>
                {
                    if (TriggerFailure)
                        throw new FormatException("unexpected");
                    throw new DivideByZeroException();
                });

                It["an expected exception"] = ShouldThrow<DivideByZeroException>();
            };

            Context["when Act may fail unexpectedly"] = () =>
            {
                Act(() =>
                {
                    if (TriggerFailure)
                        throw new InvalidOperationException("act failed");
                });

                It["a plain spec"] = () => Log.Add("plain spec ran");
            };
        }
    }

    /// <summary>Grouped variant of <see cref="ShouldThrowSpec"/>; fails only when <see cref="TriggerFailure"/> is set.</summary>
    public class GroupedShouldThrowSpec : Specifications
    {
        /// <summary>When <c>true</c>, the plain spec's Act throws. Reset after use.</summary>
        public static bool TriggerFailure { get; set; } = false;

        [Feature("grouped should throw", Level = ReportingLevel.Feature)]
        public void describe_grouped_should_throw()
        {
            Context["when Act throws"] = () =>
            {
                Act(() => throw new DivideByZeroException("Attempted to divide by zero."));

                It["the thrown type"] = ShouldThrow<DivideByZeroException>();
                It["a verified exception"] = ShouldThrow<DivideByZeroException>(ex =>
                {
                    if (!ex.Message.ToLowerInvariant().Contains("zero"))
                        throw new InvalidOperationException("message should mention zero");
                });
            };

            Context["when Act may fail unexpectedly"] = () =>
            {
                Act(() =>
                {
                    if (TriggerFailure)
                        throw new InvalidOperationException("grouped act failed");
                });

                It["a plain spec"] = () => { };
            };
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
        [Feature]
        public void some_context() { }
    }
}
