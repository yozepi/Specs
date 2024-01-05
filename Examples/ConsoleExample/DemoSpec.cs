using Specs;

namespace ConsoleExample
{
    internal class DemoSpec : Spec
    {

        public void TryIt()
        {
            Context["number cruncher"] = () =>
            {
                It["will crunch numbers"] = () => { };

                Context["when it runs faster..."] = () =>
                {
                    It["will crunch numbers faster"] = () => { };
                };

                Describe["when runs too fast..."] = () =>
                {
                    It["will catch on fire"] = () => throw new Exception("I went to fast!");
                };
            };


            Context["When something else happens..."] = () =>
            {
                It["Will Run differently"] = TODO;
            };
        }
    }
}
