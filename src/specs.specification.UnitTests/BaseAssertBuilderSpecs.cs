using NFluent;
using specs.core.Framework.Processors.TestCases;
using System.Reflection;
using yozepi;

namespace specs.BDD.UnitTests
{
    public abstract class BaseAssertBuilderSpecs<TSubject, TDelegate> : nSpecTestHarness
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

            beforeEach = () =>
            {
                Subject = Construct(new ContextualTestCaseBuilder());
                asIAssertProvider = (IAssertProvider<TDelegate>)Subject;
            };

            context["constructor"] = () =>
            {
                it["should throw ArgumentNullException when builder is null"] = () =>
                {
                    Check.ThatCode(() => Construct(null!))
                        .Throws<ArgumentNullException>()
                        .WithProperty(ex => ex.ParamName, "builder");
                };

                it["should accept valid IContextualTestCaseBuilder"] = () =>
                {
                    Check.ThatCode(() => Construct(new ContextualTestCaseBuilder())).DoesNotThrow();
                };
            };


            context["when using indexer syntax"] = () =>
            {
                context["with valid description"] = () =>
                {
                    it["should accept the description"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider["valid description"] = EmptyAction()).DoesNotThrow();
                    };
                };

                context["with null description"] = () =>
                {
                    it["should throw ArgumentNullException"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider[null!] = EmptyAction())
                            .Throws<ArgumentNullException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };

                    it["should have descriptive error message"] = () =>
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

                context["with empty description"] = () =>
                {
                    it["should throw ArgumentException"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider[""] = EmptyAction())
                            .Throws<ArgumentException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };

                    it["should have descriptive error message"] = () =>
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

                context["with whitespace description"] = () =>
                {
                    it["should throw ArgumentException for spaces"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider["   "] = EmptyAction())
                            .Throws<ArgumentException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };

                    it["should throw ArgumentException for tabs"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider["\t\t"] = EmptyAction())
                            .Throws<ArgumentException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };

                    it["should throw ArgumentException for newlines"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider["\r\n"] = EmptyAction())
                            .Throws<ArgumentException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };

                    it["should throw ArgumentException for mixed whitespace"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider[" \t\r\n "] = EmptyAction())
                            .Throws<ArgumentException>()
                            .WithProperty(ex => ex.ParamName, "description");
                    };
                };
            };


            context["usage patterns"] = () =>
            {
                context[$"when {typeof(TDelegate).Name} is null"] = () =>
                {
                    it[$"should throw ArgumentNullException"] = () =>
                    {
                        Check.ThatCode(() => asIAssertProvider[$"Test {ActionVerb} Assertion"] = null!)
                            .Throws<ArgumentNullException>()
                            .WithProperty(ex => ex.ParamName, "test");
                    };
                };

                context[$"when multiple assertions are used"] = () =>
                {
                    it[$"should accept multiple assertion blocks"] = () =>
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

            context["type characteristics"] = () =>
            {
                it["should be public class"] = () =>
                {
                    var type = typeof(TSubject);
                    Check.That(type.IsPublic).IsTrue();
                };

                it["should have internal constructor"] = () =>
                {
                    var type = typeof(TSubject);
                    var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                    var internalConstructor = constructors.FirstOrDefault(c => c.IsAssembly);
                    Check.That(internalConstructor).IsNotNull();
                };

                it["should not have public constructor"] = () =>
                {
                    var type = typeof(TSubject);
                    var publicConstructors = type.GetConstructors(BindingFlags.Instance |   BindingFlags.Public);
                    Check.That(publicConstructors).IsEmpty();
                };

                it["should have indexer property"] = () =>
                {
                    var type = typeof(TSubject);
                    var indexer = type.GetProperty("Item");
                    Check.That(indexer).IsNotNull();
                };

                it[$"should have {typeof(TDelegate).Name} type indexer"] = () =>
                {
                    var type = typeof(TSubject);
                    var indexer = type.GetProperty("Item");
                    Check.That(indexer!.PropertyType).IsEqualTo(typeof(TDelegate));
                };

                it["should have setter on indexer"] = () =>
                {
                    var type = typeof(TSubject);
                    var indexer = type.GetProperty("Item");
                    Check.That(indexer!.CanWrite).IsTrue();
                };

                it["should not have getter on indexer"] = () =>
                {
                    var type = typeof(TSubject);
                    var indexer = type.GetProperty("Item");
                    Check.That(indexer!.CanRead).IsFalse();
                };
            };
        }
    }
}
