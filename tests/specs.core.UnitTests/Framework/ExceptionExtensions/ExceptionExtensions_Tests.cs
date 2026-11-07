using NFluent;
using specs.core.Framework.Adapters;
using specs.core.Framework.ParameterValidation;
using specs.BDD;
using specs.BDD.Attributes;

namespace specs.core.UnitTests.Framework.ExceptionExtensions
{
    public class ExceptionExtensions_Tests : Specifications
    {

        [Feature]
        public void ExceptionExtensions_Specs()
        {
            Describe["SetValueOrThrowIfParameterIsNull extension method"] = () =>
            {
                Context["when parameter is null reference type"] = () =>
                {
                    It["should throw ArgumentNullException for null Action"] = () =>
                    {
                        Action nullAction = null!;
                        Check.ThatCode(() => nullAction.ThrowIfNull("testParam"))
                            .Throws<ArgumentNullException>();
                    };

                    It["should throw ArgumentNullException for null List"] = () =>
                    {
                        List<int> nullList = null!;
                        Check.ThatCode(() => nullList.ThrowIfNull("listParam"))
                            .Throws<ArgumentNullException>();
                    };

                    It["should include parameter name in exception message"] = () =>
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

                    It["should include different parameter names in message"] = () =>
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

                Context["when parameter is non-null reference type"] = () =>
                {
                    It["should return the same instance for Action"] = () =>
                    {
                        Action testAction = () => { };
                        var result = testAction.ThrowIfNull("actionParam");
                        Check.That(result).IsSameReferenceAs(testAction);
                    };

                    It["should return the same instance for string"] = () =>
                    {
                        string testString = "test value";
                        var result = testString.ThrowIfNullOrWhiteSpace("stringParam");
                        Check.That(result).IsSameReferenceAs(testString);
                    };

                    It["should return the same instance for List<T>"] = () =>
                    {
                        var testList = new List<int> { 1, 2, 3 };
                        var result = testList.ThrowIfNull("listParam");
                        Check.That(result).IsSameReferenceAs(testList);
                    };
                };

                Context["when used with value types"] = () =>
                {
                    It["should return the value for int"] = () =>
                    {
                        int testInt = 42;
                        var result = testInt.ThrowIfNull("intParam");
                        Check.That(result).IsEqualTo(42);
                    };

                    It["should return the value for bool"] = () =>
                    {
                        bool testBool = true;
                        var result = testBool.ThrowIfNull("boolParam");
                        Check.That(result).IsEqualTo(true);
                    };

                    It["should return zero value for int"] = () =>
                    {
                        int zeroInt = 0;
                        var result = zeroInt.ThrowIfNull("intParam");
                        Check.That(result).IsEqualTo(0);
                    };
                };

                Context["when used with framework types"] = () =>
                {
                    It["should work with TestInstance"] = () =>
                    {
                        var instance = new TestInstance("desc", () => { });
                        var result = instance.ThrowIfNull("testInstance");
                        Check.That(result).IsSameReferenceAs(instance);
                    };

                    It["should throw for null TestInstance"] = () =>
                    {
                        TestInstance nullInstance = null!;
                        Check.ThatCode(() => nullInstance.ThrowIfNull("testInstance"))
                            .Throws<ArgumentNullException>();
                    };

                    It["should work with Action parameter"] = () =>
                    {
                        Action action = () => Console.WriteLine("test");
                        var result = action.ThrowIfNull("action");
                        Check.That(result).IsSameReferenceAs(action);
                    };
                };

                Context["edge cases"] = () =>
                {
                    It["should handle empty string as parameter name"] = () =>
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

                    It["should work with various parameter name formats"] = () =>
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

                Context["real-world usage patterns from codebase"] = () =>
                {
                    It["should validate constructor parameters like TestContextAdapter"] = () =>
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

                    It["should throw when constructor parameter is null"] = () =>
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
