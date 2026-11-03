using NFluent;
using NUnit.Framework;
using specs.BDD.Runner;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using System.Linq;

namespace specs.BDD.VSTestAdapter.Tests.Discovery
{
    [TestFixture]
    public class ContextMethodScanner_Tests
    {
        // ── FindContextMethods ────────────────────────────────────────────────────────────────

        [Test]
        public void FindContextMethods_ReturnsMethodsDecoratedWithDescribeAttribute()
        {
            var methods = ContextMethodScanner.FindContextMethods(typeof(SimpleSpec)).ToList();

            Check.That(methods).HasSize(1);
            Check.That(methods[0].Name).IsEqualTo("describe_something");
        }

        [Test]
        public void FindContextMethods_ReturnsAllContextMethodsWhenMultipleExist()
        {
            var methods = ContextMethodScanner.FindContextMethods(typeof(MultiContextSpec))
                .Select(m => m.Name)
                .ToList();

            Check.That(methods).HasSize(2);
            Check.That(methods).Contains("first_feature");
            Check.That(methods).Contains("second_feature");
        }

        [Test]
        public void FindContextMethods_IgnoresMethodsWithoutDescribeAttribute()
        {
            var methods = ContextMethodScanner.FindContextMethods(typeof(SpecWithNoContextMethods)).ToList();

            Check.That(methods).IsEmpty();
        }

        [Test]
        public void FindContextMethods_ReturnsEmptyWhenNoContextMethodsExist()
        {
            var methods = ContextMethodScanner.FindContextMethods(typeof(SpecWithNoContextMethods)).ToList();

            Check.That(methods).IsEmpty();
        }

        // ── ValidateHasContextMethods ─────────────────────────────────────────────────────────

        [Test]
        public void ValidateHasContextMethods_DoesNotThrowWhenContextMethodsExist()
        {
            Check.ThatCode(() =>
                ContextMethodScanner.ValidateHasContextMethods(typeof(SimpleSpec)))
                .DoesNotThrow();
        }

        [Test]
        public void ValidateHasContextMethods_ThrowsInvalidSpecificationExceptionWhenNoContextMethods()
        {
            Check.ThatCode(() =>
                ContextMethodScanner.ValidateHasContextMethods(typeof(SpecWithNoContextMethods)))
                .Throws<InvalidSpecificationException>();
        }

        [Test]
        public void ValidateHasContextMethods_ExceptionMessageContainsClassName()
        {
            InvalidSpecificationException? caught = null;
            try
            {
                ContextMethodScanner.ValidateHasContextMethods(typeof(SpecWithNoContextMethods));
            }
            catch (InvalidSpecificationException ex)
            {
                caught = ex;
            }

            Check.That(caught).IsNotNull();
            Check.That(caught!.Message).Contains(nameof(SpecWithNoContextMethods));
        }

        [Test]
        public void ValidateHasContextMethods_ExceptionMessageMentionsDescribeAttribute()
        {
            InvalidSpecificationException? caught = null;
            try
            {
                ContextMethodScanner.ValidateHasContextMethods(typeof(SpecWithNoContextMethods));
            }
            catch (InvalidSpecificationException ex)
            {
                caught = ex;
            }

            Check.That(caught!.Message).Contains("[Describe]");
        }
    }
}
