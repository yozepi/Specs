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

                        It["will catch on fire"] = () =>
                        {
                            actual.Should().Be(200);
                        };
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

                        It["will catch on fire"] = () =>
                        {
                            actual.Should().Be(speed);
                        };
                    };
                };
            };


            Describe["when something else happens..."] = () =>
            {
                BeforeEach = () => { };

                It["will run differently"] = TODO;
            };
        }

    }
}
