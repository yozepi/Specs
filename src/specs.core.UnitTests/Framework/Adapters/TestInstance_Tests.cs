using NFluent;
using specs.core.Framework.Adapters;
using yozepi;

namespace specs.core.UnitTests.Framework.Adapters
{
    public class TestInstance_Tests : nSpecTestHarness
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        private void TestInstance_Specs()
        {
            TestInstance subject = null!;
            string description = null!;
            Action test = null!;

            beforeEach = () =>
            {
                description = "Test description";
                test = () => { };
            };

            describe["TestInstance constructor"] = () =>
            {
                context["when both parameters are valid"] = () =>
                {
                    act = () =>
                    {
                        subject = new TestInstance(description, test);
                    };

                    it["should create instance successfully"] = () =>
                    {
                        Check.That(subject).IsNotNull();
                    };

                    it["should set Description property"] = () =>
                    {
                        Check.That(subject.Description).IsEqualTo(description);
                    };

                    it["should set Test property"] = () =>
                    {
                        Check.That(subject.Test).IsEqualTo(test);
                    };
                };

                context["when description is null"] = () =>
                {
                    it["should throw NullReferenceException"] = () =>
                    {
                        NullReferenceException exception = null!;
                        try
                        {
                            new TestInstance(null!, test);
                        }
                        catch (NullReferenceException ex)
                        {
                            exception = ex;
                        }
                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("description");
                    };
                };

                context["when test is null"] = () =>
                {
                    it["should throw NullReferenceException"] = () =>
                    {
                        NullReferenceException exception = null!;
                        try
                        {
                            new TestInstance(description, null!);
                        }
                        catch (NullReferenceException ex)
                        {
                            exception = ex;
                        }
                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("test");
                    };
                };

                context["when both parameters are null"] = () =>
                {
                    it["should throw NullReferenceException for description first"] = () =>
                    {
                        NullReferenceException exception = null!;
                        try
                        {
                            new TestInstance(null!, null!);
                        }
                        catch (NullReferenceException ex)
                        {
                            exception = ex;
                        }
                        Check.That(exception).IsNotNull();
                        Check.That(exception.Message).Contains("description");
                    };
                };

                context["when description is empty string"] = () =>
                {
                    it["should create instance successfully"] = () =>
                    {
                        Check.ThatCode(() => new TestInstance("", test))
                            .DoesNotThrow();
                    };

                    it["should set Description to empty string"] = () =>
                    {
                        subject = new TestInstance("", test);
                        Check.That(subject.Description).IsEqualTo("");
                    };
                };

                context["when description is whitespace"] = () =>
                {
                    it["should create instance successfully"] = () =>
                    {
                        Check.ThatCode(() => new TestInstance("   ", test))
                            .DoesNotThrow();
                    };

                    it["should preserve whitespace in Description"] = () =>
                    {
                        subject = new TestInstance("   ", test);
                        Check.That(subject.Description).IsEqualTo("   ");
                    };
                };
            };

            describe["Description property"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new TestInstance(description, test);
                };

                it["should return the value provided in constructor"] = () =>
                {
                    Check.That(subject.Description).IsEqualTo(description);
                };

                it["should be read-only (no setter)"] = () =>
                {
                    var propertyInfo = typeof(TestInstance).GetProperty("Description");
                    Check.That(propertyInfo).IsNotNull();
                    Check.That(propertyInfo!.CanWrite).IsFalse();
                };

                context["with various description formats"] = () =>
                {
                    var testDescriptions = new[]
                    {
                        "simple description",
                        "description with spaces",
                        "description_with_underscores",
                        "description-with-dashes",
                        "UPPERCASE DESCRIPTION",
                        "MixedCase Description",
                        "description with 123 numbers",
                        "description with special chars: !@#$%",
                    };

                    foreach (var testDescription in testDescriptions)
                    {
                        it[$"should preserve description: '{testDescription}'"] = () =>
                        {
                            var instance = new TestInstance(testDescription, test);
                            Check.That(instance.Description).IsEqualTo(testDescription);
                        };
                    }
                };
            };

            describe["Test property"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new TestInstance(description, test);
                };

                it["should return the value provided in constructor"] = () =>
                {
                    Check.That(subject.Test).IsEqualTo(test);
                };

                it["should be read-only (no setter)"] = () =>
                {
                    var propertyInfo = typeof(TestInstance).GetProperty("Test");
                    Check.That(propertyInfo).IsNotNull();
                    Check.That(propertyInfo!.CanWrite).IsFalse();
                };

                it["should be invokable"] = () =>
                {
                    bool invoked = false;
                    var testAction = new Action(() => invoked = true);
                    subject = new TestInstance(description, testAction);

                    subject.Test.Invoke();

                    Check.That(invoked).IsTrue();
                };

                context["with different test action behaviors"] = () =>
                {
                    it["should preserve action that does nothing"] = () =>
                    {
                        var emptyAction = new Action(() => { });
                        subject = new TestInstance(description, emptyAction);

                        Check.ThatCode(() => subject.Test.Invoke()).DoesNotThrow();
                    };

                    it["should preserve action that throws exception"] = () =>
                    {
                        var throwingAction = new Action(() => throw new InvalidOperationException("Test failed"));
                        subject = new TestInstance(description, throwingAction);

                        Check.ThatCode(() => subject.Test.Invoke())
                            .Throws<InvalidOperationException>()
                            .WithMessage("Test failed");
                    };

                    it["should preserve action with side effects"] = () =>
                    {
                        int counter = 0;
                        var countingAction = new Action(() => counter++);
                        subject = new TestInstance(description, countingAction);

                        subject.Test.Invoke();
                        subject.Test.Invoke();

                        Check.That(counter).IsEqualTo(2);
                    };
                };
            };

            describe["instance independence"] = () =>
            {
                it["should create independent instances with same description"] = () =>
                {
                    var instance1 = new TestInstance("same description", () => { });
                    var instance2 = new TestInstance("same description", () => { });

                    Check.That(instance1).IsNotNull();
                    Check.That(instance2).IsNotNull();
                    Check.That(instance1).Not.IsSameReferenceAs(instance2);
                    Check.That(instance1.Description).IsEqualTo(instance2.Description);
                };

                it["should create independent instances with same test action"] = () =>
                {
                    var sharedAction = new Action(() => { });
                    var instance1 = new TestInstance("description 1", sharedAction);
                    var instance2 = new TestInstance("description 2", sharedAction);

                    Check.That(instance1).IsNotNull();
                    Check.That(instance2).IsNotNull();
                    Check.That(instance1).Not.IsSameReferenceAs(instance2);
                    Check.That(instance1.Test).IsSameReferenceAs(instance2.Test);
                };

                it["should allow multiple instances to execute independently"] = () =>
                {
                    int counter1 = 0;
                    int counter2 = 0;
                    var instance1 = new TestInstance("test 1", () => counter1++);
                    var instance2 = new TestInstance("test 2", () => counter2++);

                    instance1.Test.Invoke();
                    instance1.Test.Invoke();
                    instance2.Test.Invoke();

                    Check.That(counter1).IsEqualTo(2);
                    Check.That(counter2).IsEqualTo(1);
                };
            };

            describe["real-world usage patterns"] = () =>
            {
                it["should work with NSpec-style test descriptions"] = () =>
                {
                    var testDescriptions = new[]
                    {
                        "should calculate sum correctly",
                        "when input is null then should throw exception",
                        "given valid data then should save to database",
                    };

                    foreach (var desc in testDescriptions)
                    {
                        var instance = new TestInstance(desc, () => { });
                        Check.That(instance.Description).IsEqualTo(desc);
                    }
                };

                it["should work with assertion-based tests"] = () =>
                {
                    var expectedResult = 42;
                    var actualResult = 0;

                    var instance = new TestInstance(
                        "should return 42",
                        () =>
                        {
                            actualResult = 40 + 2;
                            Check.That(actualResult).IsEqualTo(expectedResult);
                        });

                    subject = instance;
                    Check.That(subject.Test).IsNotNull();
                    Check.ThatCode(() => subject.Test.Invoke()).DoesNotThrow();
                    Check.That(actualResult).IsEqualTo(42);
                };

                it["should work with expect<TException> pattern"] = () =>
                {
                    var instance = new TestInstance(
                        "should throw ArgumentException",
                        () =>
                        {
                            throw new ArgumentException("Invalid argument");
                        });

                    Check.ThatCode(() => instance.Test.Invoke())
                        .Throws<ArgumentException>()
                        .WithMessage("Invalid argument");
                };

                it["should support tests that modify shared state"] = () =>
                {
                    var sharedState = new List<string>();

                    var instance1 = new TestInstance("adds item A", () => sharedState.Add("A"));
                    var instance2 = new TestInstance("adds item B", () => sharedState.Add("B"));

                    instance1.Test.Invoke();
                    Check.That(sharedState).ContainsExactly("A");

                    instance2.Test.Invoke();
                    Check.That(sharedState).ContainsExactly("A", "B");
                };
            };

            describe["immutability"] = () =>
            {
                beforeEach = () =>
                {
                    subject = new TestInstance(description, test);
                };

                it["should not allow Description to be modified after construction"] = () =>
                {
                    var originalDescription = subject.Description;
                    // Properties are read-only, verified in property tests
                    Check.That(subject.Description).IsEqualTo(originalDescription);
                };

                it["should not allow Test to be modified after construction"] = () =>
                {
                    var originalTest = subject.Test;
                    // Properties are read-only, verified in property tests
                    Check.That(subject.Test).IsEqualTo(originalTest);
                };

                it["should maintain same reference for Test action"] = () =>
                {
                    var testAction = new Action(() => { });
                    var instance = new TestInstance("test", testAction);

                    Check.That(instance.Test).IsSameReferenceAs(testAction);
                };
            };

            describe["type constraints and validation"] = () =>
            {
                it["should accept Action with no parameters"] = () =>
                {
                    Action validAction = () => Console.WriteLine("test");
                    Check.ThatCode(() => new TestInstance("test", validAction))
                        .DoesNotThrow();
                };

                it["should work with lambda expressions"] = () =>
                {
                    subject = new TestInstance("test", () => Console.WriteLine("lambda"));
                    Check.That(subject.Test).IsNotNull();
                };

                it["should work with method references"] = () =>
                {
                    void TestMethod() { }
                    subject = new TestInstance("test", TestMethod);
                    Check.That(subject.Test).IsNotNull();
                };

                it["should work with closures"] = () =>
                {
                    int capturedVariable = 0;
                    subject = new TestInstance("test", () => capturedVariable++);
                    
                    subject.Test.Invoke();
                    Check.That(capturedVariable).IsEqualTo(1);
                };
            };

            describe["ToString() behavior"] = () =>
            {
                it["should return meaningful string representation"] = () =>
                {
                    subject = new TestInstance("my test description", () => { });
                    var stringRepresentation = subject.ToString();
                    
                    Check.That(stringRepresentation).IsNotNull();
                    Check.That(stringRepresentation).Not.IsEmpty();
                };
            };
        }
    }
}
