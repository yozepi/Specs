using NFluent;
using specs.BDD.Attributes;
using System.Reflection;
using yozepi;

namespace specs.BDD.UnitTests.Attributes
{
    public class DescribeAttribute_Tests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        private void DescribeAttribute_Specs()
        {
            DescribeAttribute subject = null!;

            describe["DescribeAttribute constructor"] = () =>
            {
                context["when created with no description"] = () =>
                {
                    act = () =>
                    {
                        subject = new DescribeAttribute();
                    };

                    it["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    it["should set Description to null"] = () =>
                    {
                        Check.That(subject.Description).IsNull();
                    };
                };

                context["when created with description"] = () =>
                {
                    string description = null!;

                    beforeEach = () =>
                    {
                        description = "Test context description";
                    };

                    act = () =>
                    {
                        subject = new DescribeAttribute(description);
                    };

                    it["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    it["should set Description property"] = () =>
                    {
                        Check.That(subject.Description).IsEqualTo(description);
                    };
                };

                context["when created with null description explicitly"] = () =>
                {
                    act = () =>
                    {
                        subject = new DescribeAttribute(null);
                    };

                    it["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    it["should set Description to null"] = () =>
                    {
                        Check.That(subject.Description).IsNull();
                    };
                };

                context["when created with empty string"] = () =>
                {
                    act = () =>
                    {
                        subject = new DescribeAttribute("");
                    };

                    it["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    it["should set Description to empty string"] = () =>
                    {
                        Check.That(subject.Description).IsEqualTo("");
                    };
                };

                context["when created with whitespace"] = () =>
                {
                    act = () =>
                    {
                        subject = new DescribeAttribute("   ");
                    };

                    it["should preserve whitespace in Description"] = () =>
                    {
                        Check.That(subject.Description).IsEqualTo("   ");
                    };
                };
            };

            describe["Description property"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new DescribeAttribute("Test description");
                };

                it["should return the value provided in constructor"] = () =>
                {
                    Check.That(subject.Description).IsEqualTo("Test description");
                };

                it["should be read-only (no setter)"] = () =>
                {
                    var propertyInfo = typeof(DescribeAttribute).GetProperty("Description");
                    Check.That(propertyInfo).IsNotNull();
                    Check.That(propertyInfo!.CanWrite).IsFalse();
                };

                context["with various description formats"] = () =>
                {
                    var testDescriptions = new[]
                    {
                        "simple description",
                        "description with spaces",
                        "description_with_underscores",
                        "description-with-dashes",
                        "UPPERCASE DESCRIPTION",
                        "MixedCase Description",
                        "description with 123 numbers",
                        "description with special chars: !@#$%",
                    };

                    foreach (var testDescription in testDescriptions)
                    {
                        it[$"should preserve description: '{testDescription}'"] = () =>
                        {
                            var attr = new DescribeAttribute(testDescription);
                            Check.That(attr.Description).IsEqualTo(testDescription);
                        };
                    }
                };
            };

            describe["AttributeUsage configuration"] = () =>
            {
                AttributeUsageAttribute attributeUsage = null!;

                beforeEach = () =>
                {
                    var attributes = typeof(DescribeAttribute)
                        .GetCustomAttributes(typeof(AttributeUsageAttribute), false);
                    attributeUsage = (AttributeUsageAttribute)attributes[0];
                };

                it["should be retrievable via reflection"] = () =>
                {
                    Check.That(attributeUsage).IsNotNull();
                };

                it["should target Methods"] = () =>
                {
                    Check.That(attributeUsage.ValidOn).HasFlag(AttributeTargets.Method);
                };

                it["should not allow multiple attributes"] = () =>
                {
                    Check.That(attributeUsage.AllowMultiple).IsFalse();
                };

                it["should be inherited"] = () =>
                {
                    Check.That(attributeUsage.Inherited).IsTrue();
                };
            };

            describe["inheritance from Attribute"] = () =>
            {
                it["should inherit from System.Attribute"] = () =>
                {
                    Check.That(typeof(DescribeAttribute).IsSubclassOf(typeof(Attribute))).IsTrue();
                };

                it["should be an Attribute type"] = () =>
                {
                    var attr = new DescribeAttribute("test");
                    Check.That(attr is Attribute).IsTrue();
                };
            };

            describe["reflection-based retrieval"] = () =>
            {
                context["from method with single attribute"] = () =>
                {
                    MethodInfo method = null!;

                    beforeEach = () =>
                    {
                        method = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithSingleContext))!;
                    };

                    it["should retrieve attribute from method"] = () =>
                    {
                        var attr = method.GetCustomAttribute<DescribeAttribute>();
                        Check.That(attr).IsNotNull();
                    };

                    it["should have correct description"] = () =>
                    {
                        var attr = method.GetCustomAttribute<DescribeAttribute>();
                        Check.That(attr!.Description).IsEqualTo("Single context");
                    };
                };

                context["from method without attribute"] = () =>
                {
                    MethodInfo method = null!;

                    beforeEach = () =>
                    {
                        method = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithoutContext))!;
                    };

                    it["should return null when no attribute present"] = () =>
                    {
                        var attr = method.GetCustomAttribute<DescribeAttribute>();
                        Check.That(attr).IsNull();
                    };

                    it["should return empty collection when using GetCustomAttributes"] = () =>
                    {
                        var attrs = method.GetCustomAttributes<DescribeAttribute>();
                        Check.That(attrs).IsEmpty();
                    };
                };

                context["from method with null description"] = () =>
                {
                    MethodInfo method = null!;

                    beforeEach = () =>
                    {
                        method = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithNullDescription))!;
                    };

                    it["should retrieve attribute"] = () =>
                    {
                        var attr = method.GetCustomAttribute<DescribeAttribute>();
                        Check.That(attr).IsNotNull();
                    };

                    it["should have null description"] = () =>
                    {
                        var attr = method.GetCustomAttribute<DescribeAttribute>();
                        Check.That(attr!.Description).IsNull();
                    };
                };
            };

            describe["real-world usage patterns"] = () =>
            {
                it["should work with NSpec-style context descriptions"] = () =>
                {
                    var testDescriptions = new[]
                    {
                        "when user logs in",
                        "given valid credentials",
                        "should calculate sum correctly",
                        "describe user authentication",
                    };

                    foreach (var desc in testDescriptions)
                    {
                        var attr = new DescribeAttribute(desc);
                        Check.That(attr.Description).IsEqualTo(desc);
                    }
                };

                it["should allow discovery of all context methods in class"] = () =>
                {
                    var methods = typeof(TestClass).GetMethods()
                        .Where(m => m.GetCustomAttributes<DescribeAttribute>().Any())
                        .ToList();

                    Check.That(methods).HasSize(2);
                };
            };

            describe["immutability"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new DescribeAttribute("Original description");
                };

                it["should not allow Description to be modified after construction"] = () =>
                {
                    var originalDescription = subject.Description;
                    // Property is read-only, verified in property tests
                    Check.That(subject.Description).IsEqualTo(originalDescription);
                };

                it["should maintain same Description value across multiple accesses"] = () =>
                {
                    var description1 = subject.Description;
                    var description2 = subject.Description;
                    Check.That(description1).IsSameReferenceAs(description2);
                };
            };

            describe["instance independence"] = () =>
            {
                it["should create independent instances with same description"] = () =>
                {
                    var attr1 = new DescribeAttribute("same description");
                    var attr2 = new DescribeAttribute("same description");

                    Check.That(attr1).IsNotNull();
                    Check.That(attr2).IsNotNull();
                    Check.That(attr1).Not.IsSameReferenceAs(attr2);
                    Check.That(attr1.Description).IsEqualTo(attr2.Description);
                };

                it["should create independent instances with different descriptions"] = () =>
                {
                    var attr1 = new DescribeAttribute("first");
                    var attr2 = new DescribeAttribute("second");

                    Check.That(attr1.Description).IsEqualTo("first");
                    Check.That(attr2.Description).IsEqualTo("second");
                };
            };
        }

        // Test helper class with Context attributes for reflection tests
        private class TestClass
        {
            [Describe("Single context")]
            public void MethodWithSingleContext() { }

            public void MethodWithoutContext() { }

            [Describe]
            public void MethodWithNullDescription() { }
        }
    }
}
