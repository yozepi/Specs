using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Discovery;
using specs.BDD.VSTestAdapter.Execution;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.VSTestAdapter.Tests.Execution
{
    /// <summary>
    /// Tests for <see cref="TestFinder"/>.
    /// Each test builds an <see cref="IContextualTestCase"/> hierarchy from a fixture spec
    /// via <see cref="SuiteTreeBuilder"/>, then asserts that <c>Find</c> navigates to â€”
    /// or correctly fails to locate â€” specific tests.
    /// </summary>
    [TestFixture]
    public class TestFinder_Tests
    {
        // â”€â”€ Helper â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static IContextualTestCase BuildRoot<T>()
            => SuiteTreeBuilder.BuildSuite(typeof(T));

        // â”€â”€ Flat context (root-level tests) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        [Test]
        public void FlatContext_FindsExistingTest()
        {
            var root = BuildRoot<SimpleSpec>();

            var found = TestFinder.Find(root, "simple context", "test one");

            Check.That(found).IsNotNull();
        }

        [Test]
        public void FlatContext_ReturnsCorrectTestAction()
        {
            var root = BuildRoot<SimpleSpec>();

            var found = TestFinder.Find(root, "simple context", "test one");

            // If the action is invocable (does not throw) the correct test was located.
            Check.That(found).IsNotNull();
            Assert.DoesNotThrow(() => found!.Value.TestAction.Invoke());
        }

        [Test]
        public void FlatContext_ReturnsNullForUnknownDescription()
        {
            var root = BuildRoot<SimpleSpec>();

            var found = TestFinder.Find(root, "simple context", "nonexistent test");

            Check.That(found).IsNull();
        }

        [Test]
        public void FlatContext_FindsSecondTest()
        {
            var root = BuildRoot<SimpleSpec>();

            var found = TestFinder.Find(root, "simple context", "test two");

            Check.That(found).IsNotNull();
        }


        [Test]
        public void NestedContext_FindsTestInChildContext()
        {
            var root = BuildRoot<NestedContextSpec>();

            var found = TestFinder.Find(root, $"root context{TestStructureExtractor.PathSeparator}when doing X", "should work for X");

            Check.That(found).IsNotNull();
        }

        [Test]
        public void NestedContext_FindsTestInDifferentChildContext()
        {
            var root = BuildRoot<NestedContextSpec>();

            var found = TestFinder.Find(root, $"root context{TestStructureExtractor.PathSeparator}when doing Y", "should work for Y");

            Check.That(found).IsNotNull();
        }

        [Test]
        public void NestedContext_ReturnsNullForUnknownChildName()
        {
            var root = BuildRoot<NestedContextSpec>();

            var found = TestFinder.Find(root, $"root context{TestStructureExtractor.PathSeparator}nonexistent child", "should work for X");

            Check.That(found).IsNull();
        }


        [Test]
        public void DeeplyNested_FindsTestAtFullPath()
        {
            var root = BuildRoot<DeeplyNestedContextSpec>();

            var found = TestFinder.Find(root, $"root{TestStructureExtractor.PathSeparator}level one{TestStructureExtractor.PathSeparator}level two", "deep test");

            Check.That(found).IsNotNull();
        }

        [Test]
        public void DeeplyNested_ReturnsNullWhenPathIsTooShort()
        {
            var root = BuildRoot<DeeplyNestedContextSpec>();

            // "deep test" lives 2 levels down; looking at level one should not find it.
            var found = TestFinder.Find(root, $"root{TestStructureExtractor.PathSeparator}level one", "deep test");

            Check.That(found).IsNull();
        }


        [Test]
        public void MixedDepth_FindsRootLevelTest()
        {
            var root = BuildRoot<MixedDepthSpec>();

            var found = TestFinder.Find(root, "mixed depth", "root level test");

            Check.That(found).IsNotNull();
        }

        [Test]
        public void MixedDepth_FindsNestedTest()
        {
            var root = BuildRoot<MixedDepthSpec>();

            var found = TestFinder.Find(root, $"mixed depth{TestStructureExtractor.PathSeparator}nested context", "nested test");

            Check.That(found).IsNotNull();
        }


        [Test]
        public void Find_ReturnedContext_IsTheNodeContainingTheTest()
        {
            var root = BuildRoot<SimpleSpec>();

            var found = TestFinder.Find(root, "simple context", "test one");

            // Root has 2 tests; if the returned context is the root, Tests.Count == 2.
            Check.That(found!.Value.Context.Tests).HasSize(2);
        }

        [Test]
        public void Find_ReturnedContext_ForNestedTest_IsTheChildNode()
        {
            var root = BuildRoot<NestedContextSpec>();

            var found = TestFinder.Find(root, $"root context{TestStructureExtractor.PathSeparator}when doing X", "should work for X");

            // Child context has 1 test.
            Check.That(found!.Value.Context.Tests).HasSize(1);
        }
    }
}
