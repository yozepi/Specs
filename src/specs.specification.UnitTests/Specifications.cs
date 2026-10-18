using NFluent;
using specs.core.Framework.Processors.TestCases;
using System;
using System.Threading.Tasks;
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

                context["lifecycle methods"] = () =>
                {
                    TestSpecification subject = null!;

                    beforeEach = () =>
                    {
                        subject = new TestSpecification();
                    };

                    context["BeforeAll method"] = () =>
                    {
                        it["should accept synchronous action"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeAll(() => { })).DoesNotThrow();
                        };

                        it["should accept asynchronous task function"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeAll(() => Task.CompletedTask)).DoesNotThrow();
                        };

                        it["should accept async Task with actual async work"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeAll(async () => await Task.Delay(1))).DoesNotThrow();
                        };

                        it["should allow multiple BeforeAll registrations"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject.BeforeAll(() => { });
                                subject.BeforeAll(() => { });
                                subject.BeforeAll(() => Task.CompletedTask);
                            }).DoesNotThrow();
                        };
                    };

                    context["BeforeEach method"] = () =>
                    {
                        it["should accept synchronous action"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeEach(() => { })).DoesNotThrow();
                        };

                        it["should accept asynchronous task function"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeEach(() => Task.CompletedTask)).DoesNotThrow();
                        };

                        it["should accept async Task with actual async work"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeEach(async () => await Task.Delay(1))).DoesNotThrow();
                        };

                        it["should allow multiple BeforeEach registrations"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject.BeforeEach(() => { });
                                subject.BeforeEach(() => { });
                                subject.BeforeEach(() => Task.CompletedTask);
                            }).DoesNotThrow();
                        };
                    };

                    context["Act method"] = () =>
                    {
                        it["should accept synchronous action"] = () =>
                        {
                            Check.ThatCode(() => subject.Act(() => { })).DoesNotThrow();
                        };

                        it["should accept asynchronous task function"] = () =>
                        {
                            Check.ThatCode(() => subject.Act(() => Task.CompletedTask)).DoesNotThrow();
                        };

                        it["should accept async Task with actual async work"] = () =>
                        {
                            Check.ThatCode(() => subject.Act(async () => await Task.Delay(1))).DoesNotThrow();
                        };

                        it["should allow multiple Act registrations"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject.Act(() => { });
                                subject.Act(() => { });
                                subject.Act(() => Task.CompletedTask);
                            }).DoesNotThrow();
                        };
                    };

                    context["AfterEach method"] = () =>
                    {
                        it["should accept synchronous action"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterEach(() => { })).DoesNotThrow();
                        };

                        it["should accept asynchronous task function"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterEach(() => Task.CompletedTask)).DoesNotThrow();
                        };

                        it["should accept async Task with actual async work"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterEach(async () => await Task.Delay(1))).DoesNotThrow();
                        };

                        it["should allow multiple AfterEach registrations"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject.AfterEach(() => { });
                                subject.AfterEach(() => { });
                                subject.AfterEach(() => Task.CompletedTask);
                            }).DoesNotThrow();
                        };
                    };

                    context["AfterAll method"] = () =>
                    {
                        it["should accept synchronous action"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterAll(() => { })).DoesNotThrow();
                        };

                        it["should accept asynchronous task function"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterAll(() => Task.CompletedTask)).DoesNotThrow();
                        };

                        it["should accept async Task with actual async work"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterAll(async () => await Task.Delay(1))).DoesNotThrow();
                        };

                        it["should allow multiple AfterAll registrations"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject.AfterAll(() => { });
                                subject.AfterAll(() => { });
                                subject.AfterAll(() => Task.CompletedTask);
                            }).DoesNotThrow();
                        };
                    };
                };

                context["property accessors"] = () =>
                {
                    TestSpecification subject = null!;

                    beforeEach = () =>
                    {
                        subject = new TestSpecification();
                    };

                    context["Describe property"] = () =>
                    {
                it["should return ContextBuilder instance"] = () =>
                {
                    var spec = new TestSpecification();
                    var builder = spec.Describe;
                    Check.That(builder).IsInstanceOf<specs.BDD.ContextBuilder>();
                };

                        it["should return new instance each time"] = () =>
                        {
                            var first = subject.Describe;
                            var second = subject.Describe;
                            Check.That(first).Not.IsSameReferenceAs(second);
                        };

                        it["should support indexer syntax"] = () =>
                        {
                            Check.ThatCode(() => subject.Describe["test description"] = () => { }).DoesNotThrow();
                        };
                    };

                    context["Context property"] = () =>
                    {
                it["should return ContextBuilder instance"] = () =>
                {
                    var spec = new TestSpecification();
                    var builder = spec.Context;
                    Check.That(builder).IsInstanceOf<specs.BDD.ContextBuilder>();
                };

                        it["should return new instance each time"] = () =>
                        {
                            var first = subject.Context;
                            var second = subject.Context;
                            Check.That(first).Not.IsSameReferenceAs(second);
                        };

                        it["should support indexer syntax"] = () =>
                        {
                            Check.ThatCode(() => subject.Context["test context"] = () => { }).DoesNotThrow();
                        };
                    };

                    context["It property"] = () =>
                    {
                it["should return AssertBuilder instance"] = () =>
                {
                    var spec = new TestSpecification();
                    var builder = spec.It;
                    Check.That(builder).IsInstanceOf<specs.BDD.AssertBuilder>();
                };

                        it["should return new instance each time"] = () =>
                        {
                            var first = subject.It;
                            var second = subject.It;
                            Check.That(first).Not.IsSameReferenceAs(second);
                        };

                        it["should support indexer syntax"] = () =>
                        {
                            Check.ThatCode(() => subject.It["test assertion"] = () => { }).DoesNotThrow();
                        };
                    };

                    context["ItAsync property"] = () =>
                    {
                it["should return AssertBuilderAsync instance"] = () =>
                {
                    var spec = new TestSpecification();
                    var builder = spec.ItAsync;
                    Check.That(builder).IsInstanceOf<specs.BDD.AssertBuilderAsync>();
                };

                        it["should return new instance each time"] = () =>
                        {
                            var first = subject.ItAsync;
                            var second = subject.ItAsync;
                            Check.That(first).Not.IsSameReferenceAs(second);
                        };

                        it["should support indexer syntax"] = () =>
                        {
                            Check.ThatCode(() => subject.ItAsync["test async assertion"] = async () => await Task.CompletedTask).DoesNotThrow();
                        };
                    };
                };

                context["pending specs"] = () =>
                {
                    TestSpecification subject = null!;
                    IContextualTestCase built = null!;
                    bool bodyInvoked = false;

                    beforeEach = () =>
                    {
                        subject = new TestSpecification();
                        bodyInvoked = false;
                    };

                    act = () => built = subject.CoreBuilder.Build();

                    context["It assigned TODO"] = () =>
                    {
                        beforeEach = () =>
                        {
                            subject.It["runnable"] = () => { };
                            subject.It["todo"] = BDD.Specifications.TODO;
                        };

                        it["should mark only the TODO spec as pending"] = () =>
                            Check.That(built.PendingTests).ContainsExactly("todo");

                        it["should still register both specs"] = () =>
                            Check.That(built.Tests.Select(t => t.description)).ContainsExactly("runnable", "todo");
                    };

                    context["ItAsync assigned TODOAsync"] = () =>
                    {
                        beforeEach = () =>
                        {
                            subject.ItAsync["runnable"] = () => Task.CompletedTask;
                            subject.ItAsync["todo"] = BDD.Specifications.TODOAsync;
                        };

                        it["should mark only the TODOAsync spec as pending"] = () =>
                            Check.That(built.PendingTests).ContainsExactly("todo");
                    };

                    context["xIt"] = () =>
                    {
                        beforeEach = () => subject.xIt["excluded"] = () => bodyInvoked = true;

                        it["should mark the spec as pending"] = () =>
                            Check.That(built.PendingTests).ContainsExactly("excluded");

                        it["should keep the body without invoking it"] = () =>
                        {
                            Check.That(bodyInvoked).IsFalse();
                            built.Tests[0].test();
                            Check.That(bodyInvoked).IsTrue();
                        };
                    };

                    context["xItAsync"] = () =>
                    {
                        beforeEach = () => subject.xItAsync["excluded"] = () => Task.CompletedTask;

                        it["should mark the spec as pending"] = () =>
                            Check.That(built.PendingTests).ContainsExactly("excluded");
                    };

                    context["xContext"] = () =>
                    {
                        beforeEach = () =>
                        {
                            subject.xContext["pending context"] = () =>
                            {
                                subject.It["inner"] = () => { };
                                subject.Describe["nested"] = () => subject.It["nested inner"] = () => { };
                            };
                            subject.Context["normal context"] = () => subject.It["normal"] = () => { };
                        };

                        it["should mark its specs as pending"] = () =>
                            Check.That(built.ChildContexts[0].PendingTests).ContainsExactly("inner");

                        it["should mark nested specs as pending"] = () =>
                        {
                            var pendingContext = (IContextualTestCase)built.ChildContexts[0];
                            Check.That(pendingContext.ChildContexts[0].PendingTests).ContainsExactly("nested inner");
                        };

                        it["should not affect sibling contexts"] = () =>
                            Check.That(built.ChildContexts[1].PendingTests).IsEmpty();
                    };

                    context["xDescribe"] = () =>
                    {
                        beforeEach = () => subject.xDescribe["pending describe"] = () => subject.It["inner"] = () => { };

                        it["should mark its specs as pending"] = () =>
                            Check.That(built.ChildContexts[0].PendingTests).ContainsExactly("inner");
                    };
                };

                context["full lifecycle integration"] = () =>
                {
                    TestSpecification subject = null!;

                    beforeEach = () =>
                    {
                        subject = new TestSpecification();
                    };

                    it["should allow complete lifecycle registration"] = () =>
                    {
                        Check.ThatCode(() =>
                        {
                            subject.BeforeAll(() => { });
                            subject.BeforeEach(() => { });
                            subject.Act(() => { });
                            subject.AfterEach(() => { });
                            subject.AfterAll(() => { });
                        }).DoesNotThrow();
                    };

                    it["should allow async lifecycle registration"] = () =>
                    {
                        Check.ThatCode(() =>
                        {
                            subject.BeforeAll(() => Task.CompletedTask);
                            subject.BeforeEach(() => Task.CompletedTask);
                            subject.Act(() => Task.CompletedTask);
                            subject.AfterEach(() => Task.CompletedTask);
                            subject.AfterAll(() => Task.CompletedTask);
                        }).DoesNotThrow();
                    };

                    it["should allow mixed sync and async lifecycle"] = () =>
                    {
                        Check.ThatCode(() =>
                        {
                            subject.BeforeAll(() => { });
                            subject.BeforeEach(() => Task.CompletedTask);
                            subject.Act(() => { });
                            subject.AfterEach(() => Task.CompletedTask);
                            subject.AfterAll(() => { });
                        }).DoesNotThrow();
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
