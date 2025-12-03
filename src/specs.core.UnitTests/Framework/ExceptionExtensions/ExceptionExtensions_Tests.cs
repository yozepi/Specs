using NFluent;
using specs.core.Framework.Adapters;
using specs.core.Framework.ParameterValidation;
using yozepi;

namespace specs.core.UnitTests.Framework.ExceptionExtensions
{
    public class ExceptionExtensions_Tests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        private void ExceptionExtensions_Specs()
        {
            describe["SetValueOrThrowIfParameterIsNull extension method"] = () =>
            {
                context["when parameter is null reference type"] = () =>
                {
                    it["should throw ArgumentNullException for null Action"] = () =>
                    {
                        Action nullAction = null!;
                        Check.ThatCode(() => nullAction.ThrowIfNull("testParam"))
                            .Throws<ArgumentNullException>();
                    };

                    it["should throw ArgumentNullException for null List"] = () =>
                    {
                        List<int> nullList = null!;
                        Check.ThatCode(() => nullList.ThrowIfNull("listParam"))
                            .Throws<ArgumentNullException>();
                    };

                    it["should include parameter name in exception message"] = () =>
                    {
                        string nullString = null!;
                        ArgumentNullException exception = null!;

                        try
                        {
                            nullString.ThrowIfNull("myParameter");
                        }
                        catch (ArgumentNullException ex)
                        {
                            exception = ex;
                        }

                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("myParameter cannot be null.");
                    };

                    it["should include different parameter names in message"] = () =>
                    {
                        Action nullAction = null!;
                        ArgumentNullException exception = null!;

                        try
                        {
                            nullAction.ThrowIfNull("setupOnce");
                        }
                        catch (ArgumentNullException ex)
                        {
                            exception = ex;
                        }

                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("setupOnce");
                    };
                };

                context["when parameter is non-null reference type"] = () =>
                {
                    it["should return the same instance for Action"] = () =>
                    {
                        Action testAction = () => { };
                        var result = testAction.ThrowIfNull("actionParam");
                        Check.That(result).IsSameReferenceAs(testAction);
                    };

                    it["should return the same instance for string"] = () =>
                    {
                        string testString = "test value";
                        var result = testString.ThrowIfNullOrWhiteSpace("stringParam");
                        Check.That(result).IsSameReferenceAs(testString);
                    };

                    it["should return the same instance for List<T>"] = () =>
                    {
                        var testList = new List<int> { 1, 2, 3 };
                        var result = testList.ThrowIfNull("listParam");
                        Check.That(result).IsSameReferenceAs(testList);
                    };
                };

                context["when used with value types"] = () =>
                {
                    it["should return the value for int"] = () =>
                    {
                        int testInt = 42;
                        var result = testInt.ThrowIfNull("intParam");
                        Check.That(result).IsEqualTo(42);
                    };

                    it["should return the value for bool"] = () =>
                    {
                        bool testBool = true;
                        var result = testBool.ThrowIfNull("boolParam");
                        Check.That(result).IsEqualTo(true);
                    };

                    it["should return zero value for int"] = () =>
                    {
                        int zeroInt = 0;
                        var result = zeroInt.ThrowIfNull("intParam");
                        Check.That(result).IsEqualTo(0);
                    };
                };

                context["when used with framework types"] = () =>
                {
                    it["should work with TestInstance"] = () =>
                    {
                        var instance = new TestInstance("desc", () => { });
                        var result = instance.ThrowIfNull("testInstance");
                        Check.That(result).IsSameReferenceAs(instance);
                    };

                    it["should throw for null TestInstance"] = () =>
                    {
                        TestInstance nullInstance = null!;
                        Check.ThatCode(() => nullInstance.ThrowIfNull("testInstance"))
                            .Throws<ArgumentNullException>();
                    };

                    it["should work with Action parameter"] = () =>
                    {
                        Action action = () => Console.WriteLine("test");
                        var result = action.ThrowIfNull("action");
                        Check.That(result).IsSameReferenceAs(action);
                    };
                };

                context["edge cases"] = () =>
                {
                    it["should handle empty string as parameter name"] = () =>
                    {
                        string nullString = null!;
                        ArgumentNullException exception = null!;

                        try
                        {
                            nullString.ThrowIfNull("");
                        }
                        catch (ArgumentNullException ex)
                        {
                            exception = ex;
                        }

                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains(" cannot be null.");
                    };

                    it["should work with various parameter name formats"] = () =>
                    {
                        var paramNames = new[] { "camelCase", "PascalCase", "snake_case", "UPPER_CASE" };

                        foreach (var paramName in paramNames)
                        {
                            string nullString = null!;
                            ArgumentNullException exception = null!;

                            try
                            {
                                nullString.ThrowIfNull(paramName);
                            }
                            catch (ArgumentNullException ex)
                            {
                                exception = ex;
                            }

                            Check.That(exception).IsNotNull();
                            Check.That(exception.Message).Contains(paramName);
                        }
                    };
                };

                context["real-world usage patterns from codebase"] = () =>
                {
                    it["should validate constructor parameters like TestContextAdapter"] = () =>
                    {
                        Action setupOnce = () => { };
                        Action setupEach = () => { };
                        Action act = () => { };
                        Action teardownEach = () => { };
                        Action teardownOnce = () => { };

                        // This mimics the pattern used in TestContextAdapter constructor
                        var validatedSetupOnce = setupOnce.ThrowIfNull(nameof(setupOnce));
                        var validatedSetupEach = setupEach.ThrowIfNull(nameof(setupEach));
                        var validatedAct = act.ThrowIfNull(nameof(act));
                        var validatedTeardownEach = teardownEach.ThrowIfNull(nameof(teardownEach));
                        var validatedTeardownOnce = teardownOnce.ThrowIfNull(nameof(teardownOnce));

                        Check.That(validatedSetupOnce).IsSameReferenceAs(setupOnce);
                        Check.That(validatedSetupEach).IsSameReferenceAs(setupEach);
                        Check.That(validatedAct).IsSameReferenceAs(act);
                        Check.That(validatedTeardownEach).IsSameReferenceAs(teardownEach);
                        Check.That(validatedTeardownOnce).IsSameReferenceAs(teardownOnce);
                    };

                    it["should throw when constructor parameter is null"] = () =>
                    {
                        Action nullSetupOnce = null!;
                        ArgumentNullException exception = null!;

                        try
                        {
                            nullSetupOnce.ThrowIfNull(nameof(nullSetupOnce));
                        }
                        catch (ArgumentNullException ex)
                        {
                            exception = ex;
                        }

                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("nullSetupOnce");
                        Check.That(exception.Message).Contains("nullSetupOnce cannot be null.");
                    };
                };
            };
        }
    }
}
