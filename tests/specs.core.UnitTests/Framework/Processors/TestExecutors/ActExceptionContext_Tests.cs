using NFluent;
using specs.BDD;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestExecutors;

namespace specs.core.UnitTests.Framework.Processors.TestExecutors
{
    public class ActExceptionContext_Tests : Specifications
    {
        [Describe]
        public void ActExceptionContext_Specs()
        {
            Action act = null!;
            Action test = null!;
            bool testRan = false;
            Exception? observed = null;
            Exception? thrown = null;
            var actException = new InvalidOperationException("act failed");
            var testException = new FormatException("test failed");

            BeforeEach(() =>
            {
                act = () => { };
                test = () =>
                {
                    testRan = true;
                    observed = ActExceptionContext.Current;
                };
                testRan = false;
                observed = null;
                thrown = null;
            });

            Act(() =>
            {
                try { ActExceptionContext.Run(act, test); }
                catch (Exception ex) { thrown = ex; }
            });

            Context["when Act does not throw..."] = () =>
            {
                It["should run the test"] = () => Check.That(testRan).IsTrue();

                It["should expose no exception to the test"] = () => Check.That(observed).IsNull();

                It["should not throw"] = () => Check.That(thrown).IsNull();

                Context["when the test throws..."] = () =>
                {
                    BeforeEach(() => test = () => throw testException);

                    It["should throw the test exception"] = () => Check.That(thrown).IsSameReferenceAs(testException);
                };
            };

            Context["when Act throws..."] = () =>
            {
                BeforeEach(() => act = () => throw actException);

                It["should still run the test"] = () => Check.That(testRan).IsTrue();

                It["should expose the Act exception to the test"] = () => Check.That(observed).IsSameReferenceAs(actException);

                It["should throw the unhandled Act exception"] = () => Check.That(thrown).IsSameReferenceAs(actException);

                Context["when the test also throws without handling the Act exception..."] = () =>
                {
                    BeforeEach(() => test = () => throw testException);

                    It["should throw the Act exception instead"] = () => Check.That(thrown).IsSameReferenceAs(actException);
                };

                Context["when the test handles the Act exception..."] = () =>
                {
                    BeforeEach(() => test = ActExceptionContext.MarkHandled);

                    It["should not throw"] = () => Check.That(thrown).IsNull();
                };

                Context["when the test handles the Act exception and then throws..."] = () =>
                {
                    BeforeEach(() => test = () =>
                    {
                        ActExceptionContext.MarkHandled();
                        throw testException;
                    });

                    It["should throw the test exception"] = () => Check.That(thrown).IsSameReferenceAs(testException);
                };

                Context["when a nested run completes inside the test..."] = () =>
                {
                    Exception? nestedObserved = null;

                    BeforeEach(() =>
                    {
                        nestedObserved = actException;
                        test = () =>
                        {
                            ActExceptionContext.Run(() => { }, () => nestedObserved = ActExceptionContext.Current);
                            observed = ActExceptionContext.Current;
                            ActExceptionContext.MarkHandled();
                        };
                    });

                    It["should isolate the nested run"] = () => Check.That(nestedObserved).IsNull();

                    It["should restore the enclosing Act exception"] = () => Check.That(observed).IsSameReferenceAs(actException);

                    It["should not throw"] = () => Check.That(thrown).IsNull();
                };
            };

            Describe["parameter validation..."] = () =>
            {
                It["should reject a null act"] = () =>
                    Check.ThatCode(() => ActExceptionContext.Run(null!, () => { })).Throws<ArgumentNullException>();

                It["should reject a null test"] = () =>
                    Check.ThatCode(() => ActExceptionContext.Run(() => { }, null!)).Throws<ArgumentNullException>();
            };
        }
    }
}
