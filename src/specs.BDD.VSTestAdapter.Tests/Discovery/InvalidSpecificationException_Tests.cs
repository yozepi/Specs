using NFluent;
using NUnit.Framework;
using specs.BDD.VSTestAdapter.Discovery;
using System;

namespace specs.BDD.VSTestAdapter.Tests.Discovery
{
    [TestFixture]
    public class InvalidSpecificationException_Tests
    {
        [Test]
        public void Constructor_WithMessage_SetsMessage()
        {
            var message = "test message";
            var ex = new InvalidSpecificationException(message);
            Check.That(ex.Message).IsEqualTo(message);
        }

        [Test]
        public void Constructor_WithMessageAndInner_SetsMessageAndInnerException()
        {
            var inner = new Exception("inner");
            var ex = new InvalidSpecificationException("outer", inner);

            Check.That(ex.Message).IsEqualTo("outer");
            Check.That(ex.InnerException).IsSameReferenceAs(inner);
        }

        [Test]
        public void IsAnException()
        {
            var ex = new InvalidSpecificationException("msg");
            // NFluent 3 IsInstanceOf<T> is exact-type only; use NUnit for inheritance check.
            Assert.IsInstanceOf<Exception>(ex);
        }
        [Test]
        public void CanBeCaughtAsException()
        {
            Exception? caught = null;
            try
            {
                throw new InvalidSpecificationException("catch me");
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            Check.That(caught).IsNotNull();
            Check.That(caught).IsInstanceOf<InvalidSpecificationException>();
        }
    }
}
