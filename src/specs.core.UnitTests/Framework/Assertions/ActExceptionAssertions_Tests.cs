using NFluent;
using specs.BDD;
using specs.BDD.Attributes;
using specs.core.Framework.Assertions;
using specs.core.Framework.Processors.TestExecutors;

namespace specs.core.UnitTests.Framework.Assertions
{
    public class ActExceptionAssertions_Tests : Specifications
    {
        [Context]
        public void VerifyActException_Specs()
        {
            Action act = null!;
            ArgumentException? verified = null;
            Action<ArgumentException> verify = null!;
            Exception? thrown = null;

            BeforeEach(() =>
            {
                act = () => { };
                verified = null;
                verify = ex => verified = ex;
                thrown = null;
            });

            Act(() =>
            {
                try { ActExceptionContext.Run(act, verify.VerifyActException); }
                catch (Exception ex) { thrown = ex; }
            });

            Context["when Act throws the expected type..."] = () =>
            {
                var expected = new ArgumentException("bad argument");
                BeforeEach(() => act = () => throw expected);

                It["should pass the exception to the verifier"] = () => Check.That(verified).IsSameReferenceAs(expected);

                It["should pass"] = () => Check.That(thrown).IsNull();

                Context["when the verifier fails..."] = () =>
                {
                    var verifyFailure = new FormatException("verify failed");
                    BeforeEach(() => verify = _ => throw verifyFailure);

                    It["should report the verifier failure"] = () => Check.That(thrown).IsSameReferenceAs(verifyFailure);
                };
            };

            Context["when Act throws a derived type..."] = () =>
            {
                var expected = new ArgumentNullException("value");
                BeforeEach(() => act = () => throw expected);

                It["should pass the exception to the verifier"] = () => Check.That(verified).IsSameReferenceAs(expected);

                It["should pass"] = () => Check.That(thrown).IsNull();
            };

            Context["when Act does not throw..."] = () =>
            {
                It["should not call the verifier"] = () => Check.That(verified).IsNull();

                It["should fail with ExpectedExceptionNotThrownException"] = () =>
                {
                    Check.That(thrown).IsInstanceOf<ExpectedExceptionNotThrownException>();
                    Check.That(thrown!.Message)
                        .IsEqualTo("Expected ArgumentException to be thrown, but no exception was thrown.");
                };
            };

            Context["when Act throws an unrelated type..."] = () =>
            {
                var unexpected = new InvalidOperationException("wrong");
                BeforeEach(() => act = () => throw unexpected);

                It["should not call the verifier"] = () => Check.That(verified).IsNull();

                It["should fail with ExpectedExceptionNotThrownException wrapping the actual exception"] = () =>
                {
                    Check.That(thrown).IsInstanceOf<ExpectedExceptionNotThrownException>();
                    Check.That(thrown!.Message)
                        .IsEqualTo("Expected ArgumentException to be thrown, but InvalidOperationException was thrown.");
                    Check.That(thrown!.InnerException).IsSameReferenceAs(unexpected);
                };
            };

            It["should reject a null verifier"] = () =>
                Check.ThatCode(() => ((Action<ArgumentException>)null!).VerifyActException()).Throws<ArgumentNullException>();
        }
    }
}
