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

            BeforeEach = () =>
            {
                speed = 0;
                subject = new NumberCruncher();
            };


            Context["number cruncher"] = () =>
            {
                Context["syncronous chrunching..."] = () =>
                {
                    Func<int> act = () => subject.CrunchNumbers(speed);

                    It["will crunch numbers"] = () =>
                    {
                        subject.CrunchNumbers().Should().Be(0);
                    };

                    Context["when it runs faster..."] = () =>
                    {
                        BeforeEach = () => speed = 100;

                        It["will crunch numbers faster"] = () =>
                        {
                            act().Should().Be(100);
                        };
                    };

                    Describe["when it runs too fast..."] = () =>
                    {
                        BeforeEach = () => speed = 200;

                        It["will catch on fire"] = () =>
                        {
                            act().Should().Be(200);
                        };
                    };
                };

                Context["asyncronous chrunching..."] = () =>
                {

                    Func<Task<int>> act = async () => await subject.CrunchNumbersAsync(speed);

                    ItAsync["will crunch numbers"] = async () =>
                    {
                        (await subject.CrunchNumbersAsync()).Should().Be(0);
                    };

                    Context["when it runs faster..."] = () =>
                    {

                        BeforeEach = () => speed = 100;

                        ItAsync["will crunch numbers faster"] = async () =>
                        {
                            (await act()).Should().Be(speed);
                        };
                    };

                    Describe["when it runs too fast..."] = () =>
                    {
                        int actual = 0;

                        BeforeEach = () => speed = 200;

                        ItAsync["will catch on fire"] = async () =>
                        {
                            (await act()).Should().Be(speed);
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
