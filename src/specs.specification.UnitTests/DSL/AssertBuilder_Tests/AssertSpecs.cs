using NFluent;
using specs.core.Framework.Processors.TestCases;
using specs.specification.DSL;
using yozepi;

namespace specs.specification.UnitTests.DSL
{
    public class AssertSpecs : nSpecTestHarness
    {
        private void Assert_Specs()
        {
            describe["AssertBuilder"] = () =>
            {
                context["constructor"] = () =>
                {
                    it["should throw ArgumentNullException when builder is null"] = () =>
                    {
                        Check.ThatCode(() => new AssertBuilderTestWrapper(null!))
                            .Throws<ArgumentNullException>()
                            .WithProperty(ex => ex.ParamName, "builder");
                    };

                    it["should accept valid IContextualTestCaseBuilder"] = () =>
                    {
                        var builder = new ContextualTestCaseBuilder();
                        Check.ThatCode(() => new AssertBuilderTestWrapper(builder)).DoesNotThrow();
                    };
                };

                context["when using indexer syntax"] = () =>
                {
                    TestSpecification spec = null!;

                    before = () =>
                    {
                        spec = new TestSpecification();
                    };

                    context["with valid description"] = () =>
                    {
                        it["should accept the description"] = () =>
                        {
                            Check.ThatCode(() => spec.TestIt["valid description"] = () => { }).DoesNotThrow();
                        };

                        it["should return AssertBuilder type"] = () =>
                        {
                            Check.That(spec.TestIt).IsInstanceOf<AssertBuilder>();
                        };

                        it["should return new instance each time"] = () =>
                        {
                            var it1 = spec.TestIt;
                            var it2 = spec.TestIt;
                            Check.That(it1).Not.IsSameReferenceAs(it2);
                        };
                    };

                    context["with null description"] = () =>
                    {
                        it["should throw ArgumentNullException"] = () =>
                        {
                            Check.ThatCode(() => spec.TestIt[null!] = () => { })
                                .Throws<ArgumentNullException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should have descriptive error message"] = () =>
                        {
                            try
                            {
                                spec.TestIt[null!] = () => { };
                            }
                            catch (ArgumentNullException ex)
                            {
                                Check.That(ex.Message).Contains("Description cannot be null");
                            }
                        };
                    };

                    context["with empty description"] = () =>
                    {
                        it["should throw ArgumentException"] = () =>
                        {
                            Check.ThatCode(() => spec.TestIt[""] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should have descriptive error message"] = () =>
                        {
                            try
                            {
                                spec.TestIt[""] = () => { };
                            }
                            catch (ArgumentException ex)
                            {
                                Check.That(ex.Message).Contains("Description cannot be empty or whitespace");
                            }
                        };
                    };

                    context["with whitespace description"] = () =>
                    {
                        it["should throw ArgumentException for spaces"] = () =>
                        {
                            Check.ThatCode(() => spec.TestIt["   "] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should throw ArgumentException for tabs"] = () =>
                        {
                            Check.ThatCode(() => spec.TestIt["\t\t"] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should throw ArgumentException for newlines"] = () =>
                        {
                            Check.ThatCode(() => spec.TestIt["\r\n"] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should throw ArgumentException for mixed whitespace"] = () =>
                        {
                            Check.ThatCode(() => spec.TestIt[" \t\r\n "] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };
                    };
                };

                context["usage patterns"] = () =>
                {
                    TestSpecification spec = null!;

                    before = () =>
                    {
                        spec = new TestSpecification();
                    };

                    context["when action is null"] = () =>
                    {
                        it["should not throw when setting null action"] = () =>
                        {
                            Check.ThatCode(() => spec.TestIt["Test Assertion"] = null!).DoesNotThrow();
                        };
                    };

                    context["when multiple assertions are used"] = () =>
                    {
                        it["should accept multiple assertion blocks"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                spec.TestIt["Assertion 1"] = () => { };
                                spec.TestIt["Assertion 2"] = () => { };
                                spec.TestIt["Assertion 3"] = () => { };
                            }).DoesNotThrow();
                        };
                    };

                    context["with nested structure (within Context/Describe)"] = () =>
                    {
                        it["should compile and execute assertions inside Describe"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                spec.Describe["Calculator"] = () =>
                                {
                                    spec.TestIt["should add correctly"] = () =>
                                    {
                                        // This tests that It works inside Describe
                                    };
                                };
                            }).DoesNotThrow();
                        };

                        it["should compile and execute assertions inside Context"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                spec.Context["when calculating"] = () =>
                                {
                                    spec.TestIt["should add correctly"] = () =>
                                    {
                                        // This tests that It works inside Context
                                    };
                                };
                            }).DoesNotThrow();
                        };

                        it["should allow multiple assertions in same context"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                spec.Describe["Calculator"] = () =>
                                {
                                    spec.TestIt["should add correctly"] = () => { };
                                    spec.TestIt["should subtract correctly"] = () => { };
                                    spec.TestIt["should multiply correctly"] = () => { };
                                };
                            }).DoesNotThrow();
                        };
                    };
                };

                context["type characteristics"] = () =>
                {
                    it["should be public class"] = () =>
                    {
                        var type = typeof(AssertBuilder);
                        Check.That(type.IsPublic).IsTrue();
                    };

                    it["should have internal constructor"] = () =>
                    {
                        var type = typeof(AssertBuilder);
                        var constructors = type.GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        var internalConstructor = constructors.FirstOrDefault(c => c.IsAssembly);
                        Check.That(internalConstructor).IsNotNull();
                    };

                    it["should not have public constructor"] = () =>
                    {
                        var type = typeof(AssertBuilder);
                        var publicConstructors = type.GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                        Check.That(publicConstructors).IsEmpty();
                    };

                    it["should have indexer property"] = () =>
                    {
                        var type = typeof(AssertBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer).IsNotNull();
                    };

                    it["should have Action type indexer"] = () =>
                    {
                        var type = typeof(AssertBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer!.PropertyType).IsEqualTo(typeof(Action));
                    };

                    it["should have setter on indexer"] = () =>
                    {
                        var type = typeof(AssertBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer!.CanWrite).IsTrue();
                    };

                    it["should not have getter on indexer"] = () =>
                    {
                        var type = typeof(AssertBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer!.CanRead).IsFalse();
                    };
                };
            };
        }

        // Helper class to test Specifications integration
        private class TestSpecification : Specifications
        {
            // Expose protected It property for testing
            public AssertBuilder TestIt => It;
        }

        // Wrapper class to test internal constructor
        private class AssertBuilderTestWrapper : AssertBuilder
        {
            public AssertBuilderTestWrapper(IContextualTestCaseBuilder builder) : base(builder)
            {
            }
        }
    }
}
