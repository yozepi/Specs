using NFluent;
using specs.BDD.Attributes;
using specs.core.Framework.Assertions;
using specs.core.Framework.Processors.TestCases;
using specs.core.Framework.Processors.TestExecutors;
using System;
using System.Threading.Tasks;

namespace specs.BDD.UnitTests
{
    public class Specifications_Tests : Specifications
    {
        [Describe]
        public void Specifications()
        {
            Describe["Specifications base class"] = () =>
            {
                Context["when creating derived class instance"] = () =>
                {
                    TestSpecification subject = null!;

                    Act(() =>
                    {
                        subject = new TestSpecification();
                    });

                    It["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    It["should be assignable to Specifications"] = () =>
                    {
                        Check.That(subject is BDD.Specifications).IsTrue();
                    };
                };

                Context["constructor behavior"] = () =>
                {
                    It["should not throw exception when creating derived instance"] = () =>
                    {
                        Check.ThatCode(() => new TestSpecification()).DoesNotThrow();
                    };

                    It["should initialize successfully for multiple instances"] = () =>
                    {
                        var instance1 = new TestSpecification();
                        var instance2 = new TestSpecification();

                        Check.That(instance1).IsNotNull();
                        Check.That(instance2).IsNotNull();
                        Check.That(instance1).Not.IsSameReferenceAs(instance2);
                    };
                };

                Context["inheritance characteristics"] = () =>
                {
                    It["should be abstract class"] = () =>
                    {
                        var type = typeof(BDD.Specifications);
                        Check.That(type.IsAbstract).IsTrue();
                    };

                    It["should be public class"] = () =>
                    {
                        var type = typeof(BDD.Specifications);
                        Check.That(type.IsPublic).IsTrue();
                    };
                };

                Context["lifecycle methods"] = () =>
                {
                    TestSpecification subject = null!;

                    BeforeEach(() =>
                    {
                        subject = new TestSpecification();
                    });

                    Context["BeforeAll method"] = () =>
                    {
                        It["should accept synchronous action"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeAll(() => { })).DoesNotThrow();
                        };

                        It["should accept asynchronous task function"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeAll(() => Task.CompletedTask)).DoesNotThrow();
                        };

                        It["should accept async Task with actual async work"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeAll(async () => await Task.Delay(1))).DoesNotThrow();
                        };

                        It["should allow multiple BeforeAll registrations"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject.BeforeAll(() => { });
                                subject.BeforeAll(() => { });
                                subject.BeforeAll(() => Task.CompletedTask);
                            }).DoesNotThrow();
                        };
                    };

                    Context["BeforeEach method"] = () =>
                    {
                        It["should accept synchronous action"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeEach(() => { })).DoesNotThrow();
                        };

                        It["should accept asynchronous task function"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeEach(() => Task.CompletedTask)).DoesNotThrow();
                        };

                        It["should accept async Task with actual async work"] = () =>
                        {
                            Check.ThatCode(() => subject.BeforeEach(async () => await Task.Delay(1))).DoesNotThrow();
                        };

                        It["should allow multiple BeforeEach registrations"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject.BeforeEach(() => { });
                                subject.BeforeEach(() => { });
                                subject.BeforeEach(() => Task.CompletedTask);
                            }).DoesNotThrow();
                        };
                    };

                    Context["Act method"] = () =>
                    {
                        It["should accept synchronous action"] = () =>
                        {
                            Check.ThatCode(() => subject.Act(() => { })).DoesNotThrow();
                        };

                        It["should accept asynchronous task function"] = () =>
                        {
                            Check.ThatCode(() => subject.Act(() => Task.CompletedTask)).DoesNotThrow();
                        };

                        It["should accept async Task with actual async work"] = () =>
                        {
                            Check.ThatCode(() => subject.Act(async () => await Task.Delay(1))).DoesNotThrow();
                        };

                        It["should allow multiple Act registrations"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject.Act(() => { });
                                subject.Act(() => { });
                                subject.Act(() => Task.CompletedTask);
                            }).DoesNotThrow();
                        };
                    };

                    Context["AfterEach method"] = () =>
                    {
                        It["should accept synchronous action"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterEach(() => { })).DoesNotThrow();
                        };

                        It["should accept asynchronous task function"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterEach(() => Task.CompletedTask)).DoesNotThrow();
                        };

                        It["should accept async Task with actual async work"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterEach(async () => await Task.Delay(1))).DoesNotThrow();
                        };

                        It["should allow multiple AfterEach registrations"] = () =>
                        {
                            Check.ThatCode(() =>
                            {
                                subject.AfterEach(() => { });
                                subject.AfterEach(() => { });
                                subject.AfterEach(() => Task.CompletedTask);
                            }).DoesNotThrow();
                        };
                    };

                    Context["AfterAll method"] = () =>
                    {
                        It["should accept synchronous action"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterAll(() => { })).DoesNotThrow();
                        };

                        It["should accept asynchronous task function"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterAll(() => Task.CompletedTask)).DoesNotThrow();
                        };

                        It["should accept async Task with actual async work"] = () =>
                        {
                            Check.ThatCode(() => subject.AfterAll(async () => await Task.Delay(1))).DoesNotThrow();
                        };

                        It["should allow multiple AfterAll registrations"] = () =>
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

                Context["property accessors"] = () =>
                {
                    TestSpecification subject = null!;

                    BeforeEach(() =>
                    {
                        subject = new TestSpecification();
                    });

                    Context["Describe property"] = () =>
                    {
                        It["should return ContextBuilder instance"] = () =>
                        {
                            var spec = new TestSpecification();
                            var builder = spec.Describe;
                            Check.That(builder).IsInstanceOf<specs.BDD.ContextBuilder>();
                        };

                        It["should return new instance each time"] = () =>
                        {
                            var first = subject.Describe;
                            var second = subject.Describe;
                            Check.That(first).Not.IsSameReferenceAs(second);
                        };

                        It["should support indexer syntax"] = () =>
                        {
                            Check.ThatCode(() => subject.Describe["test description"] = () => { }).DoesNotThrow();
                        };
                    };

                    Context["Context property"] = () =>
                    {
                        It["should return ContextBuilder instance"] = () =>
                        {
                            var spec = new TestSpecification();
                            var builder = spec.Context;
                            Check.That(builder).IsInstanceOf<specs.BDD.ContextBuilder>();
                        };

                        It["should return new instance each time"] = () =>
                        {
                            var first = subject.Context;
                            var second = subject.Context;
                            Check.That(first).Not.IsSameReferenceAs(second);
                        };

                        It["should support indexer syntax"] = () =>
                        {
                            Check.ThatCode(() => subject.Context["test context"] = () => { }).DoesNotThrow();
                        };
                    };

                    Context["It property"] = () =>
                    {
                        It["should return AssertBuilder instance"] = () =>
                        {
                            var spec = new TestSpecification();
                            var builder = spec.It;
                            Check.That(builder).IsInstanceOf<specs.BDD.AssertBuilder>();
                        };

                        It["should return new instance each time"] = () =>
                        {
                            var first = subject.It;
                            var second = subject.It;
                            Check.That(first).Not.IsSameReferenceAs(second);
                        };

                        It["should support indexer syntax"] = () =>
                        {
                            Check.ThatCode(() => subject.It["test assertion"] = () => { }).DoesNotThrow();
                        };
                    };

                    Context["ItAsync property"] = () =>
                    {
                        It["should return AssertBuilderAsync instance"] = () =>
                        {
                            var spec = new TestSpecification();
                            var builder = spec.ItAsync;
                            Check.That(builder).IsInstanceOf<specs.BDD.AssertBuilderAsync>();
                        };

                        It["should return new instance each time"] = () =>
                        {
                            var first = subject.ItAsync;
                            var second = subject.ItAsync;
                            Check.That(first).Not.IsSameReferenceAs(second);
                        };

                        It["should support indexer syntax"] = () =>
                        {
                            Check.ThatCode(() => subject.ItAsync["test async assertion"] = async () => await Task.CompletedTask).DoesNotThrow();
                        };
                    };
                };

                Context["pending specs"] = () =>
                {
                    TestSpecification subject = null!;
                    IContextualTestCase built = null!;
                    bool bodyInvoked = false;

                    BeforeEach(() =>
                    {
                        subject = new TestSpecification();
                        bodyInvoked = false;
                    });

                    Act(() => built = subject.CoreBuilder.Build());

                    Context["It assigned TODO"] = () =>
                    {
                        BeforeEach(() =>
                        {
                            subject.It["runnable"] = () => { };
                            subject.It["todo"] = BDD.Specifications.TODO;
                        });

                        It["should mark only the TODO spec as pending"] = () =>
                            Check.That(built.PendingTests).ContainsExactly("todo");

                        It["should still register both specs"] = () =>
                            Check.That(built.Tests.Select(t => t.description)).ContainsExactly("runnable", "todo");
                    };

                    Context["ItAsync assigned TODOAsync"] = () =>
                    {
                        BeforeEach(() =>
                        {
                            subject.ItAsync["runnable"] = () => Task.CompletedTask;
                            subject.ItAsync["todo"] = BDD.Specifications.TODOAsync;
                        });

                        It["should mark only the TODOAsync spec as pending"] = () =>
                            Check.That(built.PendingTests).ContainsExactly("todo");
                    };

                    Context["xIt"] = () =>
                    {
                        BeforeEach(() => subject.xIt["excluded"] = () => bodyInvoked = true);

                        It["should mark the spec as pending"] = () =>
                            Check.That(built.PendingTests).ContainsExactly("excluded");

                        It["should keep the body without invoking it"] = () =>
                        {
                            Check.That(bodyInvoked).IsFalse();
                            built.Tests[0].test();
                            Check.That(bodyInvoked).IsTrue();
                        };
                    };

                    Context["xItAsync"] = () =>
                    {
                        BeforeEach(() => subject.xItAsync["excluded"] = () => Task.CompletedTask);

                        It["should mark the spec as pending"] = () =>
                            Check.That(built.PendingTests).ContainsExactly("excluded");
                    };

                    Context["xContext"] = () =>
                    {
                        BeforeEach(() =>
                        {
                            subject.xContext["pending context"] = () =>
                            {
                                subject.It["inner"] = () => { };
                                subject.Describe["nested"] = () => subject.It["nested inner"] = () => { };
                            };
                            subject.Context["normal context"] = () => subject.It["normal"] = () => { };
                        });

                        It["should mark its specs as pending"] = () =>
                            Check.That(built.ChildContexts[0].PendingTests).ContainsExactly("inner");

                        It["should mark nested specs as pending"] = () =>
                        {
                            var pendingContext = (IContextualTestCase)built.ChildContexts[0];
                            Check.That(pendingContext.ChildContexts[0].PendingTests).ContainsExactly("nested inner");
                        };

                        It["should not affect sibling contexts"] = () =>
                            Check.That(built.ChildContexts[1].PendingTests).IsEmpty();
                    };

                    Context["xDescribe"] = () =>
                    {
                        BeforeEach(() => subject.xDescribe["pending describe"] = () => subject.It["inner"] = () => { });

                        It["should mark its specs as pending"] = () =>
                            Check.That(built.ChildContexts[0].PendingTests).ContainsExactly("inner");
                    };
                };

                Context["ShouldThrow"] = () =>
                {
                    Action throwingAct = () => throw new DivideByZeroException("Attempted to divide by zero.");
                    Action quietAct = () => { };

                    It["should reject a null verifier"] = () =>
                        Check.ThatCode(() => BDD.Specifications.ShouldThrow<DivideByZeroException>(null!))
                            .Throws<ArgumentNullException>();

                    It["should bind the returned test to the verifier"] = () =>
                    {
                        Action<DivideByZeroException> verify = _ => { };
                        Check.That(BDD.Specifications.ShouldThrow(verify).Target).IsSameReferenceAs(verify);
                    };

                    It["should pass when Act throws an assignable exception"] = () =>
                        Check.ThatCode(() => ActExceptionContext.Run(throwingAct, BDD.Specifications.ShouldThrow<ArithmeticException>()))
                            .DoesNotThrow();

                    It["should pass the Act exception to the verifier"] = () =>
                    {
                        string? message = null;
                        ActExceptionContext.Run(throwingAct, BDD.Specifications.ShouldThrow<DivideByZeroException>(ex => message = ex.Message));
                        Check.That(message).IsEqualTo("Attempted to divide by zero.");
                    };

                    It["should fail when Act does not throw"] = () =>
                        Check.ThatCode(() => ActExceptionContext.Run(quietAct, BDD.Specifications.ShouldThrow<DivideByZeroException>()))
                            .Throws<ExpectedExceptionNotThrownException>();
                };

                Context["full lifecycle integration"] = () =>
                {
                    TestSpecification subject = null!;

                    BeforeEach(() =>
                    {
                        subject = new TestSpecification();
                    });

                    It["should allow complete lifecycle registration"] = () =>
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

                    It["should allow async lifecycle registration"] = () =>
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

                    It["should allow mixed sync and async lifecycle"] = () =>
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
            // Discovered by the Specs adapter; an empty [Describe] avoids the "no [Describe] method" warning.
            [Describe]
            public void Empty() { }
        }
    }
}
