using NFluent;
using NUnit.Framework;
using specs.BDD.Runner;
using specs.BDD.VSTestAdapter.Tests.TestFixtures;
using System.Linq;
using System.Reflection;

namespace specs.BDD.VSTestAdapter.Tests.Discovery
{
    [TestFixture]
    public class SpecificationClassScanner_Tests
    {
        // The test assembly contains all the SampleSpecifications classes.
        private static readonly Assembly TestAssembly = Assembly.GetExecutingAssembly();

        [Test]
        public void FindsConcreteSpecificationsSubclasses()
        {
            var result = SpecificationClassScanner.FindSpecificationClasses(TestAssembly).ToList();

            Check.That(result).Contains(typeof(SimpleSpec));
            Check.That(result).Contains(typeof(MultiContextSpec));
            Check.That(result).Contains(typeof(NestedContextSpec));
            Check.That(result).Contains(typeof(SpecWithNoContextMethods));
            Check.That(result).Contains(typeof(DeeplyNestedContextSpec));
            Check.That(result).Contains(typeof(MixedDepthSpec));
        }

        [Test]
        public void ExcludesAbstractSpecificationsSubclasses()
        {
            var result = SpecificationClassScanner.FindSpecificationClasses(TestAssembly).ToList();

            Check.That(result).Not.Contains(typeof(AbstractSpec));
        }

        [Test]
        public void ExcludesClassesThatDoNotInheritFromSpecifications()
        {
            var result = SpecificationClassScanner.FindSpecificationClasses(TestAssembly).ToList();

            Check.That(result).Not.Contains(typeof(NotASpec));
        }

        [Test]
        public void ExcludesTheSpecificationsBaseClassItself()
        {
            var result = SpecificationClassScanner.FindSpecificationClasses(TestAssembly).ToList();

            Check.That(result).Not.Contains(typeof(specs.BDD.Specifications));
        }

        [Test]
        public void ReturnsEmptyForAssemblyWithNoSpecifications()
        {
            // The ObjectModel assembly certainly has no Specifications subclasses.
            var foreignAssembly = typeof(Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase).Assembly;
            var result = SpecificationClassScanner.FindSpecificationClasses(foreignAssembly).ToList();

            Check.That(result).IsEmpty();
        }
    }
}
