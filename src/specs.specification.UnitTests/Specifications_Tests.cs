using NFluent;
using yozepi;

namespace specs.specification.UnitTests
{
    public class Specifications_Tests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        private void Specifications_Specs()
        {
            describe["Specifications base class"] = () =>
            {
                context["when creating derived class instance"] = () =>
                {
                    TestSpecification subject = null!;

                    act = () =>
                    {
                        subject = new TestSpecification();
                    };

                    it["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    it["should be assignable to Specifications"] = () =>
                    {
                        Check.That(subject is Specifications).IsTrue();
                    };

                    it["should be instance of derived type"] = () =>
                    {
                        Check.That(subject).IsInstanceOf<TestSpecification>();
                    };
                };

                context["constructor behavior"] = () =>
                {
                    it["should not throw exception when creating derived instance"] = () =>
                    {
                        Check.ThatCode(() => new TestSpecification()).DoesNotThrow();
                    };

                    it["should initialize successfully for multiple instances"] = () =>
                    {
                        var instance1 = new TestSpecification();
                        var instance2 = new TestSpecification();

                        Check.That(instance1).IsNotNull();
                        Check.That(instance2).IsNotNull();
                        Check.That(instance1).Not.IsSameReferenceAs(instance2);
                    };
                };

                context["inheritance characteristics"] = () =>
                {
                    it["should be abstract class"] = () =>
                    {
                        var type = typeof(Specifications);
                        Check.That(type.IsAbstract).IsTrue();
                    };

                    it["should be public class"] = () =>
                    {
                        var type = typeof(Specifications);
                        Check.That(type.IsPublic).IsTrue();
                    };

                    it["should allow inheritance by derived classes"] = () =>
                    {
                        var derivedType = typeof(TestSpecification);
                        Check.That(derivedType.IsSubclassOf(typeof(Specifications))).IsTrue();
                    };
                };
            };
        }

        // Helper class to test inheritance
        private class TestSpecification : Specifications
        {
        }
    }
}
