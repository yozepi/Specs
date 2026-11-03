using NFluent;
using specs.core.Framework.Processors.TestCases;
using System.Reflection;

namespace specs.BDD.UnitTests
{
    public abstract class BaseAssertBuilderSpecs<TSubject, TDelegate> : Specifications
        where TDelegate : Delegate
        where TSubject : class
    {
        protected TSubject Subject { get; set; } = default!;

        protected virtual string ActionVerb => string.Empty;

        protected abstract TSubject Construct(IContextualTestCaseBuilder coreBuilder);

        protected abstract TDelegate EmptyAction();

        protected virtual void ExtendedUsePatterns() { }

        protected void SetSharedSpecs()
        {
            IAssertProvider<TDelegate> asIAssertProvider = null!;

            BeforeEach(() =>
            {
                Subject = Construct(new ContextualTestCaseBuilder());
                asIAssertProvider = (IAssertProvider<TDelegate>)Subject;
            });

            Context["constructor"] = () =>
            {
                It["should throw ArgumentNullException when builder is null"] = () =>
                {
                    Check.ThatCode(() => Construct(null!))
                        .Throws<ArgumentNullException>()
                        .WithProperty(ex => ex.ParamName, "builder");
                };

                It["should accept valid IContextualTestCaseBuilder"] = () =>
                {
                    Check.ThatCode(() => Construct(new ContextualTestCaseBuilder())).DoesNotThrow();
                };
            };


            Context["when using indexer syntax"] = () =>
            {
                Context["with valid description"] = () =>
                {
                    It["should accept the description"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider["valid description"] = EmptyAction()).DoesNotThrow();
                    };
                };

                Context["with null description"] = () =>
                {
                    It["should throw ArgumentNullException"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider[null!] = EmptyAction())
                            .Throws<ArgumentNullException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };

                    It["should have descriptive error message"] = () =>
                    {
                        try
                        {
                            asIAssertProvider[null!] = EmptyAction();
                        }
                        catch (ArgumentNullException ex)
                        {
                            Check.That(ex.Message).Contains("description cannot be null");
                        }
                    };
                };

                Context["with empty description"] = () =>
                {
                    It["should throw ArgumentException"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider[""] = EmptyAction())
                            .Throws<ArgumentException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };

                    It["should have descriptive error message"] = () =>
                    {
                        try
                        {
                            asIAssertProvider[""] = EmptyAction();
                        }
                        catch (ArgumentException ex)
                        {
                            Check.That(ex.Message).Contains("description cannot be empty or whitespace");
                        }
                    };
                };

                Context["with whitespace description"] = () =>
                {
                    It["should throw ArgumentException for spaces"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider["   "] = EmptyAction())
                            .Throws<ArgumentException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };

                    It["should throw ArgumentException for tabs"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider["\t\t"] = EmptyAction())
                            .Throws<ArgumentException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };

                    It["should throw ArgumentException for newlines"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider["\r\n"] = EmptyAction())
                            .Throws<ArgumentException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };

                    It["should throw ArgumentException for mixed whitespace"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider[" \t\r\n "] = EmptyAction())
                            .Throws<ArgumentException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };
                };
            };


            Context["usage patterns"] = () =>
            {
                Context[$"when {typeof(TDelegate).Name} is null"] = () =>
                {
                    It[$"should throw ArgumentNullException"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider[$"Test {ActionVerb} Assertion"] = null!)
                            .Throws<ArgumentNullException>()
                            .WithProperty(ex => ex.ParamName, "test");
                    };
                };

                Context[$"when multiple assertions are used"] = () =>
                {
                    It[$"should accept multiple assertion blocks"] = () =>
                    {
                        Check.ThatCode(() =>
                        {
                            asIAssertProvider["Assertion 1"] = EmptyAction();
                            asIAssertProvider["Assertion 2"] = EmptyAction();
                            asIAssertProvider["Assertion 3"] = EmptyAction();
                        }).DoesNotThrow();
                    };
                };

                ExtendedUsePatterns();
            };

            Context["type characteristics"] = () =>
            {
                It["should be public class"] = () =>
                {
                    var type = typeof(TSubject);
                    Check.That(type.IsPublic).IsTrue();
                };

                It["should have internal constructor"] = () =>
                {
                    var type = typeof(TSubject);
                    var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                    var internalConstructor = constructors.FirstOrDefault(c => c.IsAssembly);
                    Check.That(internalConstructor).IsNotNull();
                };

                It["should not have public constructor"] = () =>
                {
                    var type = typeof(TSubject);
                    var publicConstructors = type.GetConstructors(BindingFlags.Instance |   BindingFlags.Public);
                    Check.That(publicConstructors).IsEmpty();
                };

                It["should have indexer property"] = () =>
                {
                    var type = typeof(TSubject);
                    var indexer = type.GetProperty("Item");
                    Check.That(indexer).IsNotNull();
                };

                It[$"should have {typeof(TDelegate).Name} type indexer"] = () =>
                {
                    var type = typeof(TSubject);
                    var indexer = type.GetProperty("Item");
                    Check.That(indexer!.PropertyType).IsEqualTo(typeof(TDelegate));
                };

                It["should have setter on indexer"] = () =>
                {
                    var type = typeof(TSubject);
                    var indexer = type.GetProperty("Item");
                    Check.That(indexer!.CanWrite).IsTrue();
                };

                It["should not have getter on indexer"] = () =>
                {
                    var type = typeof(TSubject);
                    var indexer = type.GetProperty("Item");
                    Check.That(indexer!.CanRead).IsFalse();
                };
            };
        }
    }
}
