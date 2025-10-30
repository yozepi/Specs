using specs.core.Framework.ParameterValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace specs.core.Framework.Processors.TestCases
{
    internal class ContextualTestCaseBuilder : IContextualTestCaseBuilder
    {
        private readonly ContextualTestCaseBuilder? parentContext = null;
        private readonly List<ContextualTestCaseBuilder> childBuilders = new List<ContextualTestCaseBuilder>();

        private readonly RunAllBuilder setupOnceBuilder = new RunAllBuilder();
        private readonly List<Action> setupEachActions = new List<Action>();

        private readonly List<Action> actActions = new List<Action>();
        private readonly List<(string description, Action test)> testActions = new List<(string description, Action test)>();

        private readonly List<Action> teardownEachActions = new List<Action>();
        private readonly RunAllBuilder teardownOnceBuilder = new RunAllBuilder();

        internal ContextualTestCaseBuilder(ContextualTestCaseBuilder? parentContext = null)
        {
            this.parentContext = parentContext;
        }

        public string? Description { get; private set; }

        /// <summary>
        /// Adds a setup action that will only be run once for all tests.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddSetupOnce(Action action)
        {
            setupOnceBuilder.AddAction(action);
            return this;
        }

        /// <summary>
        /// Adds a setup action that will onlly be run once for all tests.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddSetupOnce(Func<Task> action)
        {
            var newAction = new Action(() => action().GetAwaiter().GetResult());
            return AddSetupOnce(newAction);
        }

        /// <summary>
        /// Adds a setup action that will be run before each test.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddSetupEach(Action action)
        {
            setupEachActions.Add(action);
            return this;
        }

        /// <summary>
        /// Adds a setup action that will be run before each test.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddSetupEach(Func<Task> action)
        {
            var newAction = new Action(() => action().GetAwaiter().GetResult());
            return AddSetupEach(newAction);
        }

        /// <summary>
        /// Adds a test action.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddTest(string description, Action test)
        {
            testActions.Add((description, test));
            return this;
        }

        /// <summary>
        /// Adds a test action.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddTest(string description, Func<Task> test)
        {
            var newTest = new Action(() => test().GetAwaiter().GetResult());
            return AddTest(description, newTest);
        }

        /// <summary>
        /// Adds an action that will be run once before each test.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddAct(Action action)
        {
            actActions.Add(action);
            return this;
        }

        /// <summary>
        /// Adds an action that will be run once before each test.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddAct(Func<Task> action)
        {
            var newAction = new Action(() => action().GetAwaiter().GetResult());
            return AddAct(newAction);
        }


        /// <summary>
        /// Adds a teardown action that will be run once after each test.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddTeardownEach(Action action)
        {
            teardownEachActions.Add(action);
            return this;
        }

        /// <summary>
        /// Adds a teardown action that will be run once after each test.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddTeardownEach(Func<Task> action)
        {
            var newAction = new Action(() => action().GetAwaiter().GetResult());
            return AddTeardownEach(newAction);
        }

        /// <summary>
        /// Adds a teardown action that will only be run once after all tests.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddTeardownOnce(Action action)
        {
            teardownOnceBuilder.AddAction(action);
            return this;
        }

        /// <summary>
        /// Adds a teardown action that will only be run once after all tests.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder AddTeardownOnce(Func<Task> action)
        {
            var newAction = new Action(() => action().GetAwaiter().GetResult());
            return AddTeardownOnce(newAction);
        }

        /// <summary>
        /// Adds a child (context) test case to the test case.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public IContextualTestCaseBuilder ChildTestCase(string description, Action<IContextualTestCaseBuilder> configure)
        {
            description.OrThrowIfParameterIsNull(nameof(description));
            configure.OrThrowIfParameterIsNull(nameof(configure));

            var childBuilder = new ContextualTestCaseBuilder(this)
            {
                Description = description
            };

            childBuilders.Add(childBuilder);

            configure.Invoke(childBuilder);

            return this;
        }

        /// <summary>
        /// Build the test case.
        /// </summary>
        /// <returns></returns>
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
