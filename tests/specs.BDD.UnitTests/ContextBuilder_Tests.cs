using NFluent;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;
using static specs.BDD.ContextBuilder;

namespace specs.BDD.UnitTests
{
    public class ContextBuilder_Tests : Specifications
    {
        [Feature]
        public void ContextBuilder()
        {
            Describe["ContextBuilder"] = () =>
            {
                BDD.ContextBuilder subject = null!;
                ContextCallBack callBack = null!;

                BeforeEach(() => callBack = (childBuilder, action) => { });

                Act(() => subject = new BDD.ContextBuilder(new ContextualTestCaseBuilder(), callBack));

                Context["when using indexer syntax"] = () =>
                {
                    Context["with valid description"] = () =>
                    {
                        It["should accept the description"] = () =>
                        {
                            Check.ThatCode(() => subject["valid description"] = () => { }).DoesNotThrow();
                        };
                    };

                    Context["with null description"] = () =>
                    {
                        It["should throw ArgumentNullException"] = () =>
                        {
                            Check.ThatCode(() => subject[null!] = () => { })
                                .Throws<ArgumentNullException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };
                    };

                    Context["with empty description"] = () =>
                    {
                        It["should throw ArgumentException"] = () =>
                        {
                            Check.ThatCode(() => subject[""] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };
                    };

                    Context["with whitespace description"] = () =>
                    {
                        It["should throw ArgumentException for spaces"] = () =>
                        {
                            Check.ThatCode(() => subject["   "] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        It["should throw ArgumentException for tabs"] = () =>
                        {
                            Check.ThatCode(() => subject["\t\t"] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        It["should throw ArgumentException for newlines"] = () =>
                        {
                            Check.ThatCode(() => subject["\r\n"] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };
                    };
                };

                Context["usage patterns"] = () =>
                {
                    It["should throw when setting null action"] = () =>
                    {
                        Check.ThatCode(() => subject["Test Context"] = null!).Throws<ArgumentNullException>();
                    };

                    Context["when multiple contexts are set"] = () =>
                    {
                        It["should create multiple context blocks blocks"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject["Context 1"] = () => { };
                                subject["Context 2"] = () => { };
                                subject["Context 3"] = () => { };
                            }).DoesNotThrow();
                        };
                    };

                    Context["with nested context structure"] = () =>
                    {
                        It["should compile and execute nested contexts"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject["Outer"] = () =>
                                {
                                    subject["Inner"] = () => { };
                                };
                            }).DoesNotThrow();
                        };
                    };
                };

                Context["type characteristics"] = () =>
                {
                    It["should be public class"] = () =>
                    {
                        var type = typeof(BDD.ContextBuilder);
                        Check.That(type.IsPublic).IsTrue();
                    };

                    It["should have indexer property"] = () =>
                    {
                        var type = typeof(BDD.ContextBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer).IsNotNull();
                    };

                    It["should have Action type indexer"] = () =>
                    {
                        var type = typeof(BDD.ContextBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer!.PropertyType).IsEqualTo(typeof(Action));
                    };
                };
            };
        }
    }
}
