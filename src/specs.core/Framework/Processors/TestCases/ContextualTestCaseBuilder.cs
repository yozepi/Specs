using specs.core.Framework.ParameterValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace specs.core.Framework.Processors.TestCases
{
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

        private readonly List<Action> teardownEachActions = new List<Action>();
        private readonly RunAllBuilder teardownOnceBuilder = new RunAllBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualTestCaseBuilder"/> class.
        /// </summary>
        /// <param name="parentContext">The optional parent context from which this context inherits lifecycle actions.</param>
        public ContextualTestCaseBuilder(ContextualTestCaseBuilder? parentContext = null)
        {
            this.parentContext = parentContext;
        }

        /// <inheritdoc/>
        public string? Description { get; private set; }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddSetupOnce(Action action)
        {
            setupOnceBuilder.AddAction(action);
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddSetupOnce(Func<Task> action)
        {
            var newAction = new Action(() => action().GetAwaiter().GetResult());
            return AddSetupOnce(newAction);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddSetupEach(Action action)
        {
            setupEachActions.Add(action);
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddSetupEach(Func<Task> action)
        {
            var newAction = new Action(() => action().GetAwaiter().GetResult());
            return AddSetupEach(newAction);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTest(string description, Action test)
        {
            testActions.Add((description, test));
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTest(string description, Func<Task> test)
        {
            var newTest = new Action(() => test().GetAwaiter().GetResult());
            return AddTest(description, newTest);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddAct(Action action)
        {
            actActions.Add(action);
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddAct(Func<Task> action)
        {
            var newAction = new Action(() => action().GetAwaiter().GetResult());
            return AddAct(newAction);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTeardownEach(Action action)
        {
            teardownEachActions.Add(action);
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTeardownEach(Func<Task> action)
        {
            var newAction = new Action(() => action().GetAwaiter().GetResult());
            return AddTeardownEach(newAction);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTeardownOnce(Action action)
        {
            teardownOnceBuilder.AddAction(action);
            return this;
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder AddTeardownOnce(Func<Task> action)
        {
            var newAction = new Action(() => action().GetAwaiter().GetResult());
            return AddTeardownOnce(newAction);
        }

        /// <inheritdoc/>
        public IContextualTestCaseBuilder ChildTestCase(string description, Action<IContextualTestCaseBuilder> configure)
        {
 
            var childBuilder = new ContextualTestCaseBuilder(this)
            {
                Description = description.OrThrowIfValueIsNullOrWhiteSpace(nameof(description))
            };

            childBuilders.Add(childBuilder);

            configure
                .OrThrowIfValueIsNull(nameof(configure))
                .Invoke(childBuilder);

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

            return ContextualTestCase.CreateTestCase(
                setupOnce: () => { foreach (var action in setupOnce) { action.Invoke(); } },
                setupEach: () => { foreach (var action in setupEach) { action.Invoke(); } },
                act: () => { foreach (var action in act) { action.Invoke(); } },
                teardownEach: () => { foreach (var action in teardownEach) { action.Invoke(); } },
                teardownOnce: () => { foreach (var action in teardownOnce) { action.Invoke(); } },
                tests: new List<(string description, Action test)>(this.testActions),
                childContexts: childBuilders.ConvertAll(builder => this.BuildAsContext(builder))
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
                tests: testCase.Tests.ToList(),
                childContexts: testCase.ChildContexts.ToList()
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
