using NFluent;
using yozepi;

namespace specs.BDD.UnitTests
{
    public class Specifications : nSpecTestHarness
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
                        Check.That(subject is BDD.Specifications).IsTrue();
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
                        var type = typeof(BDD.Specifications);
                        Check.That(type.IsAbstract).IsTrue();
                    };

                    it["should be public class"] = () =>
                    {
                        var type = typeof(BDD.Specifications);
                        Check.That(type.IsPublic).IsTrue();
                    };
                };
            };
        }

        // Helper class to test inheritance
        private class TestSpecification : BDD.Specifications
        {
        }
    }
}
