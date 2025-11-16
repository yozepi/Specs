using specs.core.Framework.ParameterValidation;
using System;
using System.Collections.Generic;

namespace specs.core.Framework.Processors.TestCases
{
    internal class ContextualTestCase : ITestCaseContext, IContextualTestCase
    {
        public static IContextualTestCase CreateTestCase(Action setupOnce, Action setupEach, Action act, Action teardownEach, Action teardownOnce, List<(string description, Action test)> tests, List<ITestCaseContext> childContexts)
        {
            return new ContextualTestCase(setupOnce, setupEach, act, teardownEach, teardownOnce, tests, childContexts);
        }
        public static ITestCaseContext CreateTestCaseContext(string description, Action setupOnce, Action setupEach, Action act, Action teardownEach, Action teardownOnce, List<(string description, Action test)> tests, List<ITestCaseContext> childContexts)
        {
            return new ContextualTestCase(
                setupOnce,
                setupEach,
                act,
                teardownEach,
                teardownOnce,
                tests,
                childContexts,
                description.OrThrowIfValueIsNullOrWhiteSpace(nameof(description)));
        }

        private ContextualTestCase(Action setupOnce, Action setupEach, Action act, Action teardownEach, Action teardownOnce, List<(string description, Action test)> tests, List<ITestCaseContext> childContexts, string? description = null)
        {
            this.SetupOnce = setupOnce.OrThrowIfValueIsNull(nameof(setupOnce));
            this.SetupEach = setupEach.OrThrowIfValueIsNull(nameof(setupEach));
            this.Act = act.OrThrowIfValueIsNull(nameof(act));
            this.TeardownEach = teardownEach.OrThrowIfValueIsNull(nameof(teardownEach));
            this.TeardownOnce = teardownOnce.OrThrowIfValueIsNull(nameof(teardownOnce));
            this.Tests = tests.OrThrowIfValueIsNull(nameof(tests));
            this.ChildContexts = childContexts.OrThrowIfValueIsNull(nameof(childContexts));
            this.Description = description;
        }

        public Action SetupOnce { get; }

        public Action SetupEach { get; }

        public Action Act { get; }


        public Action TeardownEach { get; }

        public Action TeardownOnce { get; }

        public IReadOnlyList<(string description, Action test)> Tests { get; }

        public IReadOnlyList<ITestCaseContext> ChildContexts { get; }

        private string? Description { get; } 

        string ITestCaseContext.Description => this.Description!;
    }
}
