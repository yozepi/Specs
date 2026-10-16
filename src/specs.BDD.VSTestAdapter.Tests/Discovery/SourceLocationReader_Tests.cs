using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Discovery;
using System.Reflection;

namespace specs.BDD.VSTestAdapter.Tests.Discovery
{
    [TestFixture]
    public class SourceLocationReader_Tests
    {
        // ── Null / empty guard tests ───────────────────────────────────────────────────────────

        [Test]
        public void WhenAssemblyPathIsNull_ReturnsNull()
        {
            var method = GetSentinelMethod();

            var result = SourceLocationReader.TryGetLocation(null!, method);

            Check.That(result).IsNull();
        }

        [Test]
        public void WhenAssemblyPathIsEmpty_ReturnsNull()
        {
            var method = GetSentinelMethod();

            var result = SourceLocationReader.TryGetLocation(string.Empty, method);

            Check.That(result).IsNull();
        }

        [Test]
        public void WhenPdbFileDoesNotExist_ReturnsNull()
        {
            var method = GetSentinelMethod();

            var result = SourceLocationReader.TryGetLocation(@"C:\nonexistent\fake.dll", method);

            Check.That(result).IsNull();
        }

        // ── Happy-path tests (requires PDB alongside the running test assembly) ─────────────

        [Test]
        public void WhenPdbExists_ReturnsNonNullLocation()
        {
            var (assemblyPath, method) = GetTestAssemblyAndSentinelMethod();

            var result = SourceLocationReader.TryGetLocation(assemblyPath, method);

            Check.That(result).IsNotNull();
        }

        [Test]
        public void WhenPdbExists_FilePathEndsWithDotCs()
        {
            var (assemblyPath, method) = GetTestAssemblyAndSentinelMethod();

            var result = SourceLocationReader.TryGetLocation(assemblyPath, method);

            Check.That(result).IsNotNull();
            Check.That(result!.FilePath).EndsWith(".cs");
        }

        [Test]
        public void WhenPdbExists_FilePathContainsExpectedFileName()
        {
            var (assemblyPath, method) = GetTestAssemblyAndSentinelMethod();

            var result = SourceLocationReader.TryGetLocation(assemblyPath, method);

            Check.That(result).IsNotNull();
            Check.That(result!.FilePath).Contains("SourceLocationReader_Tests");
        }

        [Test]
        public void WhenPdbExists_LineNumberIsPositive()
        {
            var (assemblyPath, method) = GetTestAssemblyAndSentinelMethod();

            var result = SourceLocationReader.TryGetLocation(assemblyPath, method);

            Check.That(result).IsNotNull();
            Check.That(result!.LineNumber).IsStrictlyGreaterThan(0);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// A private method with a body, present purely so the PDB has a sequence point for it.
        /// Used as the target for happy-path TryGetLocation tests.
        /// </summary>
        private void SentinelMethod()
        {
            var x = 1 + 1;   // single real statement so the compiler emits a sequence point
            _ = x;
        }

        private static MethodInfo GetSentinelMethod()
            => typeof(SourceLocationReader_Tests)
                .GetMethod("SentinelMethod", BindingFlags.Instance | BindingFlags.NonPublic)!;

        private static (string assemblyPath, MethodInfo method) GetTestAssemblyAndSentinelMethod()
        {
            var assemblyPath = typeof(SourceLocationReader_Tests).Assembly.Location;
            var method = GetSentinelMethod();
            return (assemblyPath, method);
        }
    }
}
