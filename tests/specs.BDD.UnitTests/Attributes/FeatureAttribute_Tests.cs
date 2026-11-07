using NFluent;
using specs.BDD.Attributes;
using System.Reflection;

namespace specs.BDD.UnitTests.Attributes
{
    public class FeatureAttribute_Tests : Specifications
    {
        [Feature]
        public void FeatureAttribute()
        {
            FeatureAttribute subject = null!;

            Describe["constructor"] = () =>
            {
                Context["when created with no description"] = () =>
                {
                    Act(() =>
                    {
                        subject = new FeatureAttribute();
                    });

                    It["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    It["should set Description to null"] = () =>
                    {
                        Check.That(subject.Description).IsNull();
                    };
                };

                Context["when created with description"] = () =>
                {
                    string description = null!;

                    BeforeEach(() =>
                    {
                        description = "Test context description";
                    });

                    Act(() =>
                    {
                        subject = new FeatureAttribute(description);
                    });

                    It["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    It["should set Description property"] = () =>
                    {
                        Check.That(subject.Description).IsEqualTo(description);
                    };
                };

                Context["when created with null description explicitly"] = () =>
                {
                    Act(() =>
                    {
                        subject = new FeatureAttribute(null);
                    });

                    It["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    It["should set Description to null"] = () =>
                    {
                        Check.That(subject.Description).IsNull();
                    };
                };

                Context["when created with empty string"] = () =>
                {
                    Act(() =>
                    {
                        subject = new FeatureAttribute("");
                    });

                    It["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    It["should set Description to empty string"] = () =>
                    {
                        Check.That(subject.Description).IsEqualTo("");
                    };
                };

                Context["when created with whitespace"] = () =>
                {
                    Act(() =>
                    {
                        subject = new FeatureAttribute("   ");
                    });

                    It["should preserve whitespace in Description"] = () =>
                    {
                        Check.That(subject.Description).IsEqualTo("   ");
                    };
                };
            };

            Describe["Description property"] = () =>
            {
                BeforeEach(() =>
                {
                    subject = new FeatureAttribute("Test description");
                });

                It["should return the value provided in constructor"] = () =>
                {
                    Check.That(subject.Description).IsEqualTo("Test description");
                };

                It["should be read-only (no setter)"] = () =>
                {
                    var propertyInfo = typeof(FeatureAttribute).GetProperty("Description");
                    Check.That(propertyInfo).IsNotNull();
                    Check.That(propertyInfo!.CanWrite).IsFalse();
                };

                Context["with various description formats"] = () =>
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
                        It[$"should preserve this description: '{testDescription}'"] = () =>
                        {
                            var attr = new FeatureAttribute(testDescription);
                            Check.That(attr.Description).IsEqualTo(testDescription);
                        };
                    }
                };
            };

            Describe["AttributeUsage configuration"] = () =>
            {
                AttributeUsageAttribute attributeUsage = null!;

                BeforeEach(() =>
                {
                    var attributes = typeof(FeatureAttribute)
                        .GetCustomAttributes(typeof(AttributeUsageAttribute), false);
                    attributeUsage = (AttributeUsageAttribute)attributes[0];
                });

                It["should be retrievable via reflection"] = () =>
                {
                    Check.That(attributeUsage).IsNotNull();
                };

                It["should target Methods"] = () =>
                {
                    Check.That(attributeUsage.ValidOn).HasFlag(AttributeTargets.Method);
                };

                It["should not allow multiple attributes"] = () =>
                {
                    Check.That(attributeUsage.AllowMultiple).IsFalse();
                };

                It["should be inherited"] = () =>
                {
                    Check.That(attributeUsage.Inherited).IsTrue();
                };
            };

            Describe["inheritance from Attribute"] = () =>
            {
                It["should inherit from System.Attribute"] = () =>
                {
                    Check.That(typeof(FeatureAttribute).IsSubclassOf(typeof(Attribute))).IsTrue();
                };

                It["should be an Attribute type"] = () =>
                {
                    var attr = new FeatureAttribute("test");
                    Check.That(attr is Attribute).IsTrue();
                };
            };

            Describe["reflection-based retrieval"] = () =>
            {
                Context["from method with single attribute"] = () =>
                {
                    MethodInfo method = null!;

                    BeforeEach(() =>
                    {
                        method = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithSingleContext))!;
                    });

                    It["should retrieve attribute from method"] = () =>
                    {
                        var attr = method.GetCustomAttribute<FeatureAttribute>();
                        Check.That(attr).IsNotNull();
                    };

                    It["should have correct description"] = () =>
                    {
                        var attr = method.GetCustomAttribute<FeatureAttribute>();
                        Check.That(attr!.Description).IsEqualTo("Single context");
                    };
                };

                Context["from method without attribute"] = () =>
                {
                    MethodInfo method = null!;

                    BeforeEach(() =>
                    {
                        method = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithoutContext))!;
                    });

                    It["should return null when no attribute present"] = () =>
                    {
                        var attr = method.GetCustomAttribute<FeatureAttribute>();
                        Check.That(attr).IsNull();
                    };

                    It["should return empty collection when using GetCustomAttributes"] = () =>
                    {
                        var attrs = method.GetCustomAttributes<FeatureAttribute>();
                        Check.That(attrs).IsEmpty();
                    };
                };

                Context["from method with null description"] = () =>
                {
                    MethodInfo method = null!;

                    BeforeEach(() =>
                    {
                        method = typeof(TestClass).GetMethod(nameof(TestClass.MethodWithNullDescription))!;
                    });

                    It["should retrieve attribute"] = () =>
                    {
                        var attr = method.GetCustomAttribute<FeatureAttribute>();
                        Check.That(attr).IsNotNull();
                    };

                    It["should have null description"] = () =>
                    {
                        var attr = method.GetCustomAttribute<FeatureAttribute>();
                        Check.That(attr!.Description).IsNull();
                    };
                };
            };

            Describe["real-world usage patterns"] = () =>
            {
                It["should work with NSpec-style context descriptions"] = () =>
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
                        var attr = new FeatureAttribute(desc);
                        Check.That(attr.Description).IsEqualTo(desc);
                    }
                };

                It["should allow discovery of all context methods in class"] = () =>
                {
                    var methods = typeof(TestClass).GetMethods()
                        .Where(m => m.GetCustomAttributes<FeatureAttribute>().Any())
                        .ToList();

                    Check.That(methods).HasSize(2);
                };
            };

            Describe["immutability"] = () =>
            {
                BeforeEach(() =>
                {
                    subject = new FeatureAttribute("Original description");
                });

                It["should not allow Description to be modified after construction"] = () =>
                {
                    var originalDescription = subject.Description;
                    // Property is read-only, verified in property tests
                    Check.That(subject.Description).IsEqualTo(originalDescription);
                };

                It["should maintain same Description value across multiple accesses"] = () =>
                {
                    var description1 = subject.Description;
                    var description2 = subject.Description;
                    Check.That(description1).IsSameReferenceAs(description2);
                };
            };

            Describe["instance independence"] = () =>
            {
                It["should create independent instances with same description"] = () =>
                {
                    var attr1 = new FeatureAttribute("same description");
                    var attr2 = new FeatureAttribute("same description");

                    Check.That(attr1).IsNotNull();
                    Check.That(attr2).IsNotNull();
                    Check.That(attr1).Not.IsSameReferenceAs(attr2);
                    Check.That(attr1.Description).IsEqualTo(attr2.Description);
                };

                It["should create independent instances with different descriptions"] = () =>
                {
                    var attr1 = new FeatureAttribute("first");
                    var attr2 = new FeatureAttribute("second");

                    Check.That(attr1.Description).IsEqualTo("first");
                    Check.That(attr2.Description).IsEqualTo("second");
                };
            };
        }

        // Test helper class with Context attributes for reflection tests
        private class TestClass
        {
            [Feature("Single context")]
            public void MethodWithSingleContext() { }

            public void MethodWithoutContext() { }

            [Feature]
            public void MethodWithNullDescription() { }
        }
    }
}
