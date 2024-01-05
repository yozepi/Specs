using FluentAssertions;
using Specs;

namespace ConsoleExample
{
    internal class DemoSpec : Spec
    {

        public void TryIt()
        {
            NumberCruncher subject = null!;
            int speed = 0;

            Func<int> act = () => subject.CrunchNumbers(speed);

            BeforeEach = () =>
            {
                speed = 0;
                subject = new NumberCruncher();
            };


            Context["number cruncher"] = () =>
            {
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


            Describe["when something else happens..."] = () =>
            {
                BeforeEach = () => { };

                It["will run differently"] = TODO;
            };
        }

    }
}
