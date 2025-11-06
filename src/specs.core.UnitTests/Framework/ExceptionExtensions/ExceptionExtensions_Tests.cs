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
            describe["OrThrowIfParameterIsNull extension method"] = () =>
            {
                context["when parameter is null reference type"] = () =>
                {
                    it["should throw NullReferenceException for null Action"] = () =>
                    {
                        Action nullAction = null!;
                        Check.ThatCode(() => nullAction.OrThrowIfParameterIsNull("testParam"))
                            .Throws<NullReferenceException>();
                    };

                    it["should throw NullReferenceException for null string"] = () =>
                    {
                        string nullString = null!;
                        Check.ThatCode(() => nullString.OrThrowIfParameterIsNull("stringParam"))
                            .Throws<NullReferenceException>();
                    };

                    it["should throw NullReferenceException for null List"] = () =>
                    {
                        List<int> nullList = null!;
                        Check.ThatCode(() => nullList.OrThrowIfParameterIsNull("listParam"))
                            .Throws<NullReferenceException>();
                    };

                    it["should include parameter name in exception message"] = () =>
                    {
                        string nullString = null!;
                        NullReferenceException exception = null!;

                        try
                        {
                            nullString.OrThrowIfParameterIsNull("myParameter");
                        }
                        catch (NullReferenceException ex)
                        {
                            exception = ex;
                        }

                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("myParameter");
                        Check.That(exception.Message).Contains("parameter is required");
                    };

                    it["should include different parameter names in message"] = () =>
                    {
                        Action nullAction = null!;
                        NullReferenceException exception = null!;

                        try
                        {
                            nullAction.OrThrowIfParameterIsNull("setupOnce");
                        }
                        catch (NullReferenceException ex)
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
                        var result = testAction.OrThrowIfParameterIsNull("actionParam");
                        Check.That(result).IsSameReferenceAs(testAction);
                    };

                    it["should return the same instance for string"] = () =>
                    {
                        string testString = "test value";
                        var result = testString.OrThrowIfParameterIsNull("stringParam");
                        Check.That(result).IsSameReferenceAs(testString);
                    };

                    it["should return the same instance for List<T>"] = () =>
                    {
                        var testList = new List<int> { 1, 2, 3 };
                        var result = testList.OrThrowIfParameterIsNull("listParam");
                        Check.That(result).IsSameReferenceAs(testList);
                    };

                    it["should return empty string without throwing"] = () =>
                    {
                        string emptyString = "";
                        var result = emptyString.OrThrowIfParameterIsNull("stringParam");
                        Check.That(result).IsEqualTo("");
                    };
                };

                context["when used with value types"] = () =>
                {
                    it["should return the value for int"] = () =>
                    {
                        int testInt = 42;
                        var result = testInt.OrThrowIfParameterIsNull("intParam");
                        Check.That(result).IsEqualTo(42);
                    };

                    it["should return the value for bool"] = () =>
                    {
                        bool testBool = true;
                        var result = testBool.OrThrowIfParameterIsNull("boolParam");
                        Check.That(result).IsEqualTo(true);
                    };

                    it["should return zero value for int"] = () =>
                    {
                        int zeroInt = 0;
                        var result = zeroInt.OrThrowIfParameterIsNull("intParam");
                        Check.That(result).IsEqualTo(0);
                    };
                };

                context["when used with framework types"] = () =>
                {
                    it["should work with TestInstance"] = () =>
                    {
                        var instance = new TestInstance("desc", () => { });
                        var result = instance.OrThrowIfParameterIsNull("testInstance");
                        Check.That(result).IsSameReferenceAs(instance);
                    };

                    it["should throw for null TestInstance"] = () =>
                    {
                        TestInstance nullInstance = null!;
                        Check.ThatCode(() => nullInstance.OrThrowIfParameterIsNull("testInstance"))
                            .Throws<NullReferenceException>();
                    };

                    it["should work with Action parameter"] = () =>
                    {
                        Action action = () => Console.WriteLine("test");
                        var result = action.OrThrowIfParameterIsNull("action");
                        Check.That(result).IsSameReferenceAs(action);
                    };
                };

                context["edge cases"] = () =>
                {
                    it["should handle empty string as parameter name"] = () =>
                    {
                        string nullString = null!;
                        NullReferenceException exception = null!;

                        try
                        {
                            nullString.OrThrowIfParameterIsNull("");
                        }
                        catch (NullReferenceException ex)
                        {
                            exception = ex;
                        }

                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("parameter is required");
                    };

                    it["should work with various parameter name formats"] = () =>
                    {
                        var paramNames = new[] { "camelCase", "PascalCase", "snake_case", "UPPER_CASE" };

                        foreach (var paramName in paramNames)
                        {
                            string nullString = null!;
                            NullReferenceException exception = null!;

                            try
                            {
                                nullString.OrThrowIfParameterIsNull(paramName);
                            }
                            catch (NullReferenceException ex)
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
                        var validatedSetupOnce = setupOnce.OrThrowIfParameterIsNull(nameof(setupOnce));
                        var validatedSetupEach = setupEach.OrThrowIfParameterIsNull(nameof(setupEach));
                        var validatedAct = act.OrThrowIfParameterIsNull(nameof(act));
                        var validatedTeardownEach = teardownEach.OrThrowIfParameterIsNull(nameof(teardownEach));
                        var validatedTeardownOnce = teardownOnce.OrThrowIfParameterIsNull(nameof(teardownOnce));

                        Check.That(validatedSetupOnce).IsSameReferenceAs(setupOnce);
                        Check.That(validatedSetupEach).IsSameReferenceAs(setupEach);
                        Check.That(validatedAct).IsSameReferenceAs(act);
                        Check.That(validatedTeardownEach).IsSameReferenceAs(teardownEach);
                        Check.That(validatedTeardownOnce).IsSameReferenceAs(teardownOnce);
                    };

                    it["should throw when constructor parameter is null"] = () =>
                    {
                        Action nullSetupOnce = null!;
                        NullReferenceException exception = null!;

                        try
                        {
                            nullSetupOnce.OrThrowIfParameterIsNull(nameof(nullSetupOnce));
                        }
                        catch (NullReferenceException ex)
                        {
                            exception = ex;
                        }

                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("nullSetupOnce");
                        Check.That(exception.Message).Contains("parameter is required");
                    };
                };
            };
        }
    }
}
