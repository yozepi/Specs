using specs.core.Framework.ParameterValidation;
using System;
using System.Collections.Generic;

namespace specs.core.Framework.Processors.TestCases
{
    internal class ContextualTestCase : ITestCaseContext, IContextualTestCase
    {
        public static IContextualTestCase CreateTestCase(Action setupOnce, Action setupEach, Action act, Action teardownEach, Action teardownOnce, Action ownSetupOnce, Action ownTeardownOnce, List<(string description, Action test)> tests, List<ITestCaseContext> childContexts)
        {
            return new ContextualTestCase(setupOnce, setupEach, act, teardownEach, teardownOnce, ownSetupOnce, ownTeardownOnce, tests, childContexts);
        }
        public static ITestCaseContext CreateTestCaseContext(string description, Action setupOnce, Action setupEach, Action act, Action teardownEach, Action teardownOnce, Action ownSetupOnce, Action ownTeardownOnce, List<(string description, Action test)> tests, List<ITestCaseContext> childContexts)
        {
            return new ContextualTestCase(
                setupOnce,
                setupEach,
                act,
                teardownEach,
                teardownOnce,
                ownSetupOnce,
                ownTeardownOnce,
                tests,
                childContexts,
                description.ThrowIfNullOrWhiteSpace(nameof(description)));
        }

        private ContextualTestCase(Action setupOnce, Action setupEach, Action act, Action teardownEach, Action teardownOnce, Action ownSetupOnce, Action ownTeardownOnce, List<(string description, Action test)> tests, List<ITestCaseContext> childContexts, string? description = null)
        {
            this.SetupOnce = setupOnce.ThrowIfNull(nameof(setupOnce));
            this.SetupEach = setupEach.ThrowIfNull(nameof(setupEach));
            this.Act = act.ThrowIfNull(nameof(act));
            this.TeardownEach = teardownEach.ThrowIfNull(nameof(teardownEach));
            this.TeardownOnce = teardownOnce.ThrowIfNull(nameof(teardownOnce));
            this.OwnSetupOnce = ownSetupOnce.ThrowIfNull(nameof(ownSetupOnce));
            this.OwnTeardownOnce = ownTeardownOnce.ThrowIfNull(nameof(ownTeardownOnce));
            this.Tests = tests.ThrowIfNull(nameof(tests));
            this.ChildContexts = childContexts.ThrowIfNull(nameof(childContexts));
            this.Description = description;
        }

        public Action SetupOnce { get; }

        public Action SetupEach { get; }

        public Action Act { get; }

        public Action TeardownEach { get; }

        public Action TeardownOnce { get; }

        public Action OwnSetupOnce { get; }

        public Action OwnTeardownOnce { get; }

        public IReadOnlyList<(string description, Action test)> Tests { get; }

        public IReadOnlyList<ITestCaseContext> ChildContexts { get; }

        private string? Description { get; } 

        string ITestCaseContext.Description => this.Description!;
    }
}
