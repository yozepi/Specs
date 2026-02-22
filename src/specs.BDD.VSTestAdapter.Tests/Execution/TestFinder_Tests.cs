using NFluent;
using NUnit.Framework;
using specs.BDD;
using specs.BDD.VSTestAdapter.Execution;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using specs.core.Framework.Processors.TestCases;
using System;
using System.Reflection;

namespace specs.BDD.VSTestAdapter.Tests.Execution
{
    /// <summary>
    /// Tests for <see cref="TestFinder"/>.
    /// Each test builds an <see cref="IContextualTestCase"/> hierarchy from a fixture spec
    /// (using the same reflection technique as <c>TestStructureExtractor</c>), then asserts
    /// that <c>Find</c> navigates to — or correctly fails to locate — specific tests.
    /// </summary>
    [TestFixture]
    public class TestFinder_Tests
    {
        // ── Helper ────────────────────────────────────────────────────────────────────────────

        private static readonly FieldInfo CoreBuilderField =
            typeof(Specifications).GetField(
                "coreBuilder",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new Exception("Cannot locate 'coreBuilder' field on Specifications.");

        /// <summary>
        /// Instantiates <typeparamref name="T"/>, invokes <paramref name="methodName"/>,
        /// and returns the built <see cref="IContextualTestCase"/> root.
        /// </summary>
        private static IContextualTestCase BuildRoot<T>(string methodName)
            where T : Specifications, new()
        {
            var instance = new T();
            var method = typeof(T).GetMethod(
                methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(instance, null);
            var builder = (IContextualTestCaseBuilder)CoreBuilderField.GetValue(instance)!;
            return builder.Build();
        }

        // ── Flat context (root-level tests) ───────────────────────────────────────────────────

        [Test]
        public void FlatContext_FindsExistingTest()
        {
            var root = BuildRoot<SimpleSpec>("describe_something");

            var found = TestFinder.Find(root, "simple context", "test one");

            Check.That(found).IsNotNull();
        }

        [Test]
        public void FlatContext_ReturnsCorrectTestAction()
        {
            var root = BuildRoot<SimpleSpec>("describe_something");

            var found = TestFinder.Find(root, "simple context", "test one");

            // If the action is invocable (does not throw) the correct test was located.
            Check.That(found).IsNotNull();
            Assert.DoesNotThrow(() => found!.Value.TestAction.Invoke());
        }

        [Test]
        public void FlatContext_ReturnsNullForUnknownDescription()
        {
            var root = BuildRoot<SimpleSpec>("describe_something");

            var found = TestFinder.Find(root, "simple context", "nonexistent test");

            Check.That(found).IsNull();
        }

        [Test]
        public void FlatContext_FindsSecondTest()
        {
            var root = BuildRoot<SimpleSpec>("describe_something");

            var found = TestFinder.Find(root, "simple context", "test two");

            Check.That(found).IsNotNull();
        }

        // ── Nested context ────────────────────────────────────────────────────────────────────

        [Test]
        public void NestedContext_FindsTestInChildContext()
        {
            var root = BuildRoot<NestedContextSpec>("describe_something");

            var found = TestFinder.Find(root, "root context \u2192 when doing X", "should work for X");

            Check.That(found).IsNotNull();
        }

        [Test]
        public void NestedContext_FindsTestInDifferentChildContext()
        {
            var root = BuildRoot<NestedContextSpec>("describe_something");

            var found = TestFinder.Find(root, "root context \u2192 when doing Y", "should work for Y");

            Check.That(found).IsNotNull();
        }

        [Test]
        public void NestedContext_ReturnsNullForUnknownChildName()
        {
            var root = BuildRoot<NestedContextSpec>("describe_something");

            var found = TestFinder.Find(root, "root context \u2192 nonexistent child", "should work for X");

            Check.That(found).IsNull();
        }

        // ── Deeply nested context (3 levels) ─────────────────────────────────────────────────

        [Test]
        public void DeeplyNested_FindsTestAtFullPath()
        {
            var root = BuildRoot<DeeplyNestedContextSpec>("describe_something");

            var found = TestFinder.Find(root, "root \u2192 level one \u2192 level two", "deep test");

            Check.That(found).IsNotNull();
        }

        [Test]
        public void DeeplyNested_ReturnsNullWhenPathIsTooShort()
        {
            var root = BuildRoot<DeeplyNestedContextSpec>("describe_something");

            // "deep test" lives 2 levels down; looking at level one should not find it.
            var found = TestFinder.Find(root, "root \u2192 level one", "deep test");

            Check.That(found).IsNull();
        }

        // ── Mixed depth ───────────────────────────────────────────────────────────────────────

        [Test]
        public void MixedDepth_FindsRootLevelTest()
        {
            var root = BuildRoot<MixedDepthSpec>("describe_something");

            var found = TestFinder.Find(root, "mixed depth", "root level test");

            Check.That(found).IsNotNull();
        }

        [Test]
        public void MixedDepth_FindsNestedTest()
        {
            var root = BuildRoot<MixedDepthSpec>("describe_something");

            var found = TestFinder.Find(root, "mixed depth \u2192 nested context", "nested test");

            Check.That(found).IsNotNull();
        }

        // ── Context node on found result ──────────────────────────────────────────────────────

        [Test]
        public void Find_ReturnedContext_IsTheNodeContainingTheTest()
        {
            var root = BuildRoot<SimpleSpec>("describe_something");

            var found = TestFinder.Find(root, "simple context", "test one");

            // Root has 2 tests; if the returned context is the root, Tests.Count == 2.
            Check.That(found!.Value.Context.Tests).HasSize(2);
        }

        [Test]
        public void Find_ReturnedContext_ForNestedTest_IsTheChildNode()
        {
            var root = BuildRoot<NestedContextSpec>("describe_something");

            var found = TestFinder.Find(root, "root context \u2192 when doing X", "should work for X");

            // Child context has 1 test.
            Check.That(found!.Value.Context.Tests).HasSize(1);
        }
    }
}
