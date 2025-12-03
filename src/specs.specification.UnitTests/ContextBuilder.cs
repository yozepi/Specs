using NFluent;
using specs.core.Framework.Processors.TestCases;
using yozepi;
using static specs.BDD.ContextBuilder;

namespace specs.BDD.UnitTests
{
    public class ContextBuilder : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        private void ContextBuilder_Specs()
        {
            describe["ContextBuilder"] = () =>
            {
                BDD.ContextBuilder subject = null!;
                ContextCallBack callBack = null!;

                base.act = () => subject = new BDD.ContextBuilder(new ContextualTestCaseBuilder(), callBack);

                context["when using indexer syntax"] = () =>
                {

                    beforeEach = () => callBack = (childBuilder, action) => { };

                    context["with valid description"] = () =>
                    {
                        it["should accept the description"] = () =>
                        {
                            Check.ThatCode(() => subject["valid description"] = () => { }).DoesNotThrow();
                        };
                    };

                    context["with null description"] = () =>
                    {
                        it["should throw ArgumentNullException"] = () =>
                        {
                            Check.ThatCode(() => subject[null!] = () => { })
                                .Throws<ArgumentNullException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };
                    };

                    context["with empty description"] = () =>
                    {
                        it["should throw ArgumentException"] = () =>
                        {
                            Check.ThatCode(() => subject[""] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };
                    };

                    context["with whitespace description"] = () =>
                    {
                        it["should throw ArgumentException for spaces"] = () =>
                        {
                            Check.ThatCode(() => subject["   "] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should throw ArgumentException for tabs"] = () =>
                        {
                            Check.ThatCode(() => subject["\t\t"] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };

                        it["should throw ArgumentException for newlines"] = () =>
                        {
                            Check.ThatCode(() => subject["\r\n"] = () => { })
                                .Throws<ArgumentException>()
                                .WithProperty(ex => ex.ParamName, "description");
                        };
                    };
                };

                context["usage patterns"] = () =>
                {
                    beforeEach = () => callBack = (childBuilder, action) => { };


                    it["should throw when setting null action"] = () =>
                    {
                        Check.ThatCode(() => subject["Test Context"] = null!).Throws<ArgumentNullException>();
                    };

                    context["when multiple contexts are set"] = () =>
                    {
                        it["should create multiple context blocks blocks"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject["Context 1"] = () => { };
                                subject["Context 2"] = () => { };
                                subject["Context 3"] = () => { };
                            }).DoesNotThrow();
                        };
                    };

                    context["with nested context structure"] = () =>
                    {
                        it["should compile and execute nested contexts"] = () =>
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

                context["type characteristics"] = () =>
                {
                    it["should be public class"] = () =>
                    {
                        var type = typeof(BDD.ContextBuilder);
                        Check.That(type.IsPublic).IsTrue();
                    };

                    it["should have indexer property"] = () =>
                    {
                        var type = typeof(BDD.ContextBuilder);
                        var indexer = type.GetProperty("Item");
                        Check.That(indexer).IsNotNull();
                    };

                    it["should have Action type indexer"] = () =>
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
