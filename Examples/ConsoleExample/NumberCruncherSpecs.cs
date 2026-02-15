using FluentAssertions;
using Specs;

namespace ConsoleExample
{
    internal class NumberCruncherSpecs : Spec
    {
        public void TryIt()
        {
            NumberCruncher subject = null!;
            int speed = 0;
            int actual = 0;

            BeforeEach = () =>
            {
                speed = 0;
                actual = 0;
                subject = new NumberCruncher();
            };


            Context["number cruncher"] = () =>
            {
                Context["syncronous chrunching..."] = () =>
                {
                    Act = () => actual = subject.CrunchNumbers(speed);

                    It["will crunch numbers"] = () =>
                    {
                        actual.Should().Be(0);
                    };

                    Context["when it runs faster..."] = () =>
                    {
                        BeforeEach = () => speed = 100;

                        It["will crunch numbers faster"] = () =>
                        {
                            actual.Should().Be(100);
                        };
                    };

                    Describe["when it runs too fast..."] = () =>
                    {
                        BeforeEach = () => speed = 200;

                        It["will catch on fire"] = ShouldThrow<Exception>(ex => ex.Message == "I'm running too fast!", "the exception message is wrong");
                    };
                };

                Context["asyncronous chrunching..."] = () =>
                {
                    ActAsync = async () => actual = await subject.CrunchNumbersAsync(speed);

                    It["will crunch numbers"] = () =>
                    {
                        actual.Should().Be(0);
                    };

                    Context["when it runs faster..."] = () =>
                    {
                        BeforeEach = () => speed = 100;

                        It["will crunch numbers faster"] = () =>
                        {
                            actual.Should().Be(speed);
                        };
                    };

                    Describe["when it runs too fast..."] = () =>
                    {
                        BeforeEach = () => speed = 200;

                        It["will catch on fire"] = ShouldThrow<Exception>(ex => ex.Message == "I'm running too fast!", "the exception message is wrong");
                    };
                };
            };


            Describe["exception handling examples"] = () =>
            {
                Describe["Exceptions within \"It\""] = () =>
                {
                    Describe["Syncronous exception handling..."] = () =>
                    {
                        It["capture any exception"] = ShouldThrow(() => throw new Exception("Boom!"));

                        It["capture a specific exception type"] = ShouldThrow<InvalidCastException>(() => throw new InvalidCastException());

                        It["capture a specific exception type and predicate match"] = ShouldThrow<InvalidCastException>(() => throw new InvalidCastException("Crows"), ex => ex.Message == "Crows");
                    };

                    Describe["Async exception handling..."] = () =>
                    {
                        ItAsync["capture any exception"] = ShouldThrowAsync(() => Task.FromException(new Exception("Boom!")));

                        ItAsync["capture a specific exception type"] = ShouldThrowAsync<InvalidCastException>(() => Task.FromException(new InvalidCastException()));

                        ItAsync["capture a specific exception type and predicate match"] = ShouldThrowAsync<InvalidCastException>(() => Task.FromException(new InvalidCastException("Crows")), ex => ex.Message == "Crows");
                    };
                };

                Describe["Exceptions within \"Act\""] = () =>
                {
                    Describe["Syncronous exception handling..."] = () =>
                    {
                        Act = () => throw new InvalidCastException("Crows");

                        It["capture any exception"] = ShouldThrow();

                        It["capture a specific exception type"] = ShouldThrow<InvalidCastException>();

                        It["capture a specific exception type and predicate match"] = ShouldThrow<InvalidCastException>(ex => ex.Message == "Crows");
                    };

                    Describe["Async exception handling..."] = () =>
                    {
                        ActAsync = () => Task.FromException(new InvalidCastException("Crows"));

                        It["capture any exception"] = ShouldThrow();

                        It["capture a specific exception type"] = ShouldThrow<InvalidCastException>();

                        It["capture a specific exception type and predicate match"] = ShouldThrow<InvalidCastException>(ex => ex.Message == "Crows");
                    };
                };
            };
        }

    }
}
