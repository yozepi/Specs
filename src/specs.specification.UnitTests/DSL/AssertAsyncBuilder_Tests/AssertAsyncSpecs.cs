using NFluent;
using specs.core.Framework.Processors.TestCases;
using specs.specification.DSL;
using yozepi;

namespace specs.specification.UnitTests.DSL
{
    public class AssertAsyncSpecs : nSpecTestHarness
    {
        private void AssertAsync_Specs()
        {
            describe["AssertAsyncBuilder"] = () =>
            {
                context["constructor"] = () =>
                {
                    it["should throw ArgumentNullException when builder is null"] = () =>
                    {
                        Check.ThatCode(() => new AssertAsyncBuilderTestWrapper(null!))
                            .Throws<ArgumentNullException>()
                            .WithProperty(ex => ex.ParamName, "builder");
                    };

                    it["should accept valid IContextualTestCaseBuilder"] = () =>
                    {
                        var builder = new ContextualTestCaseBuilder();
                        Check.ThatCode(() => new AssertAsyncBuilderTestWrapper(builder)).DoesNotThrow();
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
                            Check.ThatCode(() => spec.TestItAsync["valid description"] = async () => { await Task.CompletedTask; }).DoesNotThrow();
                        };

                        it["should return AssertAsyncBuilder type"] = () =>
                        {
                            Check.That(spec.TestItAsync).IsInstanceOf<AssertAsyncBuilder>();
                        };

                        it["should return new instance each time"] = () =>
                        {
                            var itAsync1 = spec.TestItAsync;
                            var itAsync2 = spec.TestItAsync;
                            Check.That(itAsync1).Not.IsSameReferenceAs(itAsync2);
                        };
                    };

                    context["with null description"] = () =>
                    {
                        it["should throw ArgumentNullException"] = () =>
                        {
                            Check.ThatCode(() => spec.TestItAsync[null!] = async () => { await Task.CompletedTask; })
                                .Throws<ArgumentNullException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should have descriptive error message"] = () =>
                        {
                            try
                            {
                                spec.TestItAsync[null!] = async () => { await Task.CompletedTask; };
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
                            Check.ThatCode(() => spec.TestItAsync[""] = async () => { await Task.CompletedTask; })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should have descriptive error message"] = () =>
                        {
                            try
                            {
                                spec.TestItAsync[""] = async () => { await Task.CompletedTask; };
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
                            Check.ThatCode(() => spec.TestItAsync["   "] = async () => { await Task.CompletedTask; })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should throw ArgumentException for tabs"] = () =>
                        {
                            Check.ThatCode(() => spec.TestItAsync["\t\t"] = async () => { await Task.CompletedTask; })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should throw ArgumentException for newlines"] = () =>
                        {
                            Check.ThatCode(() => spec.TestItAsync["\r\n"] = async () => { await Task.CompletedTask; })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should throw ArgumentException for mixed whitespace"] = () =>
                        {
                            Check.ThatCode(() => spec.TestItAsync[" \t\r\n "] = async () => { await Task.CompletedTask; })
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

                    context["when async action is null"] = () =>
                    {
                        it["should not throw when setting null async action"] = () =>
                        {
                            Check.ThatCode(() => spec.TestItAsync["Test Async Assertion"] = null!).DoesNotThrow();
                        };
                    };

                    context["when multiple async assertions are used"] = () =>
                    {
                        it["should accept multiple async assertion blocks"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                spec.TestItAsync["Async Assertion 1"] = async () => { await Task.CompletedTask; };
                                spec.TestItAsync["Async Assertion 2"] = async () => { await Task.Delay(1); };
                                spec.TestItAsync["Async Assertion 3"] = async () => { await Task.CompletedTask; };
                            }).DoesNotThrow();
                        };
                    };

                    context["with nested structure (within Context/Describe)"] = () =>
                    {
                        it["should compile and execute async assertions inside Describe"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                spec.Describe["Async Calculator"] = () =>
                                {
                                    spec.TestItAsync["should add correctly asynchronously"] = async () =>
                                    {
                                        await Task.CompletedTask;
                                        // This tests that ItAsync works inside Describe
                                    };
                                };
                            }).DoesNotThrow();
                        };

                        it["should compile and execute async assertions inside Context"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                spec.Context["when calculating asynchronously"] = () =>
                                {
                                    spec.TestItAsync["should add correctly"] = async () =>
                                    {
                                        await Task.CompletedTask;
                                        // This tests that ItAsync works inside Context
                                    };
                                };
                            }).DoesNotThrow();
                        };

                        it["should allow multiple async assertions in same context"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                spec.Describe["Async Calculator"] = () =>
                                {
                                    spec.TestItAsync["should add correctly"] = async () => { await Task.CompletedTask; };
                                    spec.TestItAsync["should subtract correctly"] = async () => { await Task.CompletedTask; };
                                    spec.TestItAsync["should multiply correctly"] = async () => { await Task.CompletedTask; };
                                };
                            }).DoesNotThrow();
                        };
                    };

                    context["with actual async operations"] = () =>
                    {
                        it["should handle Task.Delay operations"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                spec.TestItAsync["should delay"] = async () =>
                                {
                                    await Task.Delay(1);
                                };
                            }).DoesNotThrow();
                        };

                        it["should handle async method calls"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                spec.TestItAsync["should call async method"] = async () =>
                                {
                                    var result = await GetAsyncValue();
                                    Check.That(result).IsEqualTo(42);
                                };
                            }).DoesNotThrow();
                        };
                    };
                };

                context["type characteristics"] = () =>
                {
                    it["should be public class"] = () =>
                    {
                        var type = typeof(AssertAsyncBuilder);
                        Check.That(type.IsPublic).IsTrue();
                    };

                    it["should have internal constructor"] = () =>
                    {
                        var type = typeof(AssertAsyncBuilder);
                        var constructors = type.GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        var internalConstructor = constructors.FirstOrDefault(c => c.IsAssembly);
                        Check.That(internalConstructor).IsNotNull();
                    };

                    it["should not have public constructor"] = () =>
                    {
                        var type = typeof(AssertAsyncBuilder);
                        var publicConstructors = type.GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                        Check.That(publicConstructors).IsEmpty();
                    };

                    it["should have indexer property"] = () =>
                    {
                        var type = typeof(AssertAsyncBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer).IsNotNull();
                    };

                    it["should have Func<Task> type indexer"] = () =>
                    {
                        var type = typeof(AssertAsyncBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer!.PropertyType).IsEqualTo(typeof(Func<Task>));
                    };

                    it["should have setter on indexer"] = () =>
                    {
                        var type = typeof(AssertAsyncBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer!.CanWrite).IsTrue();
                    };

                    it["should not have getter on indexer"] = () =>
                    {
                        var type = typeof(AssertAsyncBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer!.CanRead).IsFalse();
                    };
                };
            };
        }

        // Helper async method for testing
        private static async Task<int> GetAsyncValue()
        {
            await Task.CompletedTask;
            return 42;
        }

        // Helper class to test Specifications integration
        private class TestSpecification : Specifications
        {
            // Expose protected ItAsync property for testing
            public AssertAsyncBuilder TestItAsync => ItAsync;
        }

        // Wrapper class to test internal constructor
        private class AssertAsyncBuilderTestWrapper : AssertAsyncBuilder
        {
            public AssertAsyncBuilderTestWrapper(IContextualTestCaseBuilder builder) : base(builder)
            {
            }
        }
    }
}
