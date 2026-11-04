using specs.core.Framework.ParameterValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace specs.core.Framework.Processors.TestCases
{
    internal static class AsyncTestExtensions
    {
        public static void RunSync(this Func<Task> test) => test().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Builds hierarchical test cases with parent-child context relationships and lifecycle actions.
    /// Implements the fluent builder pattern for constructing immutable test cases.
    /// </summary>
    public class ContextualTestCaseBuilder : IContextualTestCaseBuilder
    {
        private readonly ContextualTestCaseBuilder? parentContext = null;
        private readonly List<ContextualTestCaseBuilder> childBuilders = new List<ContextualTestCaseBuilder>();

        private readonly RunAllBuilder setupOnceBuilder = new RunAllBuilder();
        private readonly List<Action> setupEachActions = new List<Action>();

        private readonly List<Action> actActions = new List<Action>();
        private readonly List<(string description, Action test)> testActions = new List<(string description, Action test)>();
        private readonly List<string> pendingTests = new List<string>();
        private readonly Dictionary<string, (string filePath, int lineNumber)> testLocations = new Dictionary<string, (string filePath, int lineNumber)>(StringComparer.Ordinal);
        private readonly bool isPending;

        private readonly List<Action> teardownEachActions = new List<Action>();
        private readonly RunAllBuilder teardownOnceBuilder = new RunAllBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualTestCaseBuilder"/> class.
        /// </summary>
        /// <param name="parentContext">The optional parent context from which this context inherits lifecycle actions.</param>
        public ContextualTestCaseBuilder(ContextualTestCaseBuilder? parentContext = null)
            : this(parentContext, isPending: false)
        {
        }

        private ContextualTestCaseBuilder(ContextualTestCaseBuilder? parentContext, bool isPending)
        {
            this.parentContext = parentContext;
            this.isPending = isPending || (parentContext?.isPending ?? false);
        }

        /// <inheritdoc/>
        public string? Description { get; private set; }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddSetupOnce(Action action)
        {
            setupOnceBuilder.AddAction(action.ThrowIfNull(nameof(action)));
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddSetupOnce(Func<Task> action)
        {
            var newAction = new Action(() =>
                action.ThrowIfNull(nameof(action))().GetAwaiter().GetResult());
            return AddSetupOnce(newAction);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddSetupEach(Action action)
        {
            setupEachActions.Add(action.ThrowIfNull(nameof(action)));
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddSetupEach(Func<Task> action)
        {
            var newAction = new Action(() => action.ThrowIfNull(nameof(action))().GetAwaiter().GetResult());
            return AddSetupEach(newAction);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTest(string description, Action test)
        {
            testActions.Add((
                description.ThrowIfNullOrWhiteSpace(nameof(description)),
                test.ThrowIfNull(nameof(test))));
            if (isPending)
                pendingTests.Add(description);
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTest(string description, Func<Task> test)
        {
            // Bound to the extension method so the wrapper's Target is the original Func<Task>.
            Action funcWrapper = null!;
            if (test != null)
                funcWrapper = test.RunSync;

            return AddTest(description, funcWrapper);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddPendingTest(string description, Action test)
        {
            AddTest(description, test);
            pendingTests.Add(description);
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddPendingTest(string description, Func<Task> test)
        {
            Action funcWrapper = null!;
            if (test != null)
                funcWrapper = test.RunSync;

            return AddPendingTest(description, funcWrapper);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTestLocation(string description, string filePath, int lineNumber)
        {
            testLocations[description.ThrowIfNullOrWhiteSpace(nameof(description))] =
                (filePath.ThrowIfNullOrWhiteSpace(nameof(filePath)), lineNumber);
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddAct(Action action)
        {
            actActions.Add(action.ThrowIfNull(nameof(action)));
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddAct(Func<Task> action)
        {
            var newAction = new Action(() => action.ThrowIfNull(nameof(action))().GetAwaiter().GetResult());
            return AddAct(newAction);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTeardownEach(Action action)
        {
            teardownEachActions.Add(action.ThrowIfNull(nameof(action)));
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTeardownEach(Func<Task> action)
        {
            var newAction = new Action(() => action.ThrowIfNull(nameof(action))().GetAwaiter().GetResult());
            return AddTeardownEach(newAction);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTeardownOnce(Action action)
        {
            teardownOnceBuilder.AddAction(action.ThrowIfNull(nameof(action)));
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTeardownOnce(Func<Task> action)
        {
            var newAction = new Action(() => action.ThrowIfNull(nameof(action))().GetAwaiter().GetResult());
            return AddTeardownOnce(newAction);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder ChildTestCase(string description, Action<IContextualTestCaseBuilder> configure)
            => ChildTestCase(description, configure, isPending: false);

        /// <inheritdoc/>
        public IContextualTestCaseBuilder ChildTestCase(string description, Action<IContextualTestCaseBuilder> configure, bool isPending)
        {
            description.ThrowIfNullOrWhiteSpace(nameof(description));
            configure.ThrowIfNull(nameof(configure));

            var childBuilder = new ContextualTestCaseBuilder(this, isPending)
            {
                Description = description
            };

            childBuilders.Add(childBuilder);
            configure.Invoke(childBuilder);

            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCase Build()
        {

            var setupOnce = new List<Action>();
            var setupEach = new List<Action>();
            var act = new List<Action>();
            var assert = new List<Action>();
            var teardownEach = new List<Action>();
            var teardownOnce = new List<Action>();

            setupOnce.AddRange(CollectAncestorAndSelfSetupOnceActions());
            setupEach.AddRange(CollectAncestorAndSelfSetupEachActions());

            act.AddRange(CollectAncestorAndSelfActActions());

            teardownEach.AddRange(CollectAncestorAndSelfTeardownEachActions());
            teardownOnce.AddRange(CollectAncestorAndSelfTeardownOnceActions());

            var ownSetupOnce = setupOnceBuilder.Build() ?? (() => { });
            var ownTeardownOnce = teardownOnceBuilder.Build() ?? (() => { });

            return ContextualTestCase.CreateTestCase(
                setupOnce: () => { foreach (var action in setupOnce) { action.Invoke(); } },
                setupEach: () => { foreach (var action in setupEach) { action.Invoke(); } },
                act: () => { foreach (var action in act) { action.Invoke(); } },
                teardownEach: () => { foreach (var action in teardownEach) { action.Invoke(); } },
                teardownOnce: () => { foreach (var action in teardownOnce) { action.Invoke(); } },
                ownSetupOnce: ownSetupOnce,
                ownTeardownOnce: ownTeardownOnce,
                tests: new List<(string description, Action test)>(this.testActions),
                childContexts: childBuilders.ConvertAll(builder => this.BuildAsContext(builder)),
                pendingTests: this.pendingTests,
                testLocations: this.testLocations
            );

        }

        private ITestCaseContext BuildAsContext(ContextualTestCaseBuilder builder)
        {
            var testCase = (ContextualTestCase)builder.Build();

            return ContextualTestCase.CreateTestCaseContext(
                description: builder.Description!,
                setupOnce: testCase.SetupOnce,
                setupEach: testCase.SetupEach,
                act: testCase.Act,
                teardownEach: testCase.TeardownEach,
                teardownOnce: testCase.TeardownOnce,
                ownSetupOnce: testCase.OwnSetupOnce,
                ownTeardownOnce: testCase.OwnTeardownOnce,
                tests: testCase.Tests.ToList(),
                childContexts: testCase.ChildContexts.ToList(),
                pendingTests: testCase.PendingTests,
                testLocations: testCase.TestLocations
            );
        }

        private IEnumerable<Action> CollectAncestorAndSelfSetupOnceActions()
        {
            // First the parents...
            if (parentContext != null)
            {
                foreach (var parentAction in parentContext.CollectAncestorAndSelfSetupOnceActions())
                {
                    yield return parentAction;
                }
            }

            // ...then self.            
            var action = setupOnceBuilder.Build();
            if (action != null)
            {
                yield return action;
            }
        }

        private IEnumerable<Action> CollectAncestorAndSelfSetupEachActions()
        {
            // First the parents...
            if (parentContext != null)
            {
                foreach (var action in parentContext.CollectAncestorAndSelfSetupEachActions())
                {
                    yield return action;
                }
            }

            // ...then self.
            foreach (var action in setupEachActions)
            {
                yield return action;
            }
        }

        private IEnumerable<Action> CollectAncestorAndSelfActActions()
        {
            // First the parents...
            if (parentContext != null)
            {
                foreach (var action in parentContext.CollectAncestorAndSelfActActions())
                {
                    yield return action;
                }
            }

            // ...then self.
            foreach (var action in actActions)
            {
                yield return action;
            }
        }

        private IEnumerable<Action> CollectAncestorAndSelfTeardownEachActions()
        {

            // First self...
            foreach (var action in teardownEachActions)
            {
                yield return action;
            }

            // ...then the parents.
            if (parentContext != null)
            {
                foreach (var action in parentContext.CollectAncestorAndSelfTeardownEachActions())
                {
                    yield return action;
                }
            }
        }

        private IEnumerable<Action> CollectAncestorAndSelfTeardownOnceActions()
        {
            // First self...
            var action = teardownOnceBuilder.Build();
            if (action != null)
            {
                yield return action;
            }

            // ...then the parents.
            if (parentContext != null)
            {
                foreach (var parentAction in parentContext.CollectAncestorAndSelfTeardownOnceActions())
                {
                    yield return parentAction;
                }
            }
        }
    }
}
