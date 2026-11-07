using specs.core.Framework.ParameterValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace specs.core.Framework.Processors.TestCases
{
    internal class ContextualTestCase : ITestCaseContext, IContextualTestCase
    {
        public static IContextualTestCase CreateTestCase(Action setupOnce, Action setupEach, Action act, Action teardownEach, Action teardownOnce, Action ownSetupOnce, Action ownTeardownOnce, List<(string description, Action test)> tests, List<ITestCaseContext> childContexts, IEnumerable<string>? pendingTests = null, IReadOnlyDictionary<string, (string filePath, int lineNumber)>? testLocations = null)
        {
            return new ContextualTestCase(setupOnce, setupEach, act, teardownEach, teardownOnce, ownSetupOnce, ownTeardownOnce, tests, childContexts, pendingTests, testLocations);
        }
        public static ITestCaseContext CreateTestCaseContext(string description, Action setupOnce, Action setupEach, Action act, Action teardownEach, Action teardownOnce, Action ownSetupOnce, Action ownTeardownOnce, List<(string description, Action test)> tests, List<ITestCaseContext> childContexts, IEnumerable<string>? pendingTests = null, IReadOnlyDictionary<string, (string filePath, int lineNumber)>? testLocations = null)
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
                pendingTests,
                testLocations,
                description.ThrowIfNullOrWhiteSpace(nameof(description)));
        }

        private ContextualTestCase(Action setupOnce, Action setupEach, Action act, Action teardownEach, Action teardownOnce, Action ownSetupOnce, Action ownTeardownOnce, List<(string description, Action test)> tests, List<ITestCaseContext> childContexts, IEnumerable<string>? pendingTests, IReadOnlyDictionary<string, (string filePath, int lineNumber)>? testLocations, string? description = null)
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
            this.PendingTests = new HashSet<string>(pendingTests ?? Array.Empty<string>(), StringComparer.Ordinal);
            this.TestLocations = (testLocations ?? new Dictionary<string, (string filePath, int lineNumber)>())
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);
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

        public IReadOnlyCollection<string> PendingTests { get; }

        public IReadOnlyDictionary<string, (string filePath, int lineNumber)> TestLocations { get; }

        private string? Description { get; } 

        string ITestCaseContext.Description => this.Description!;
    }
}
