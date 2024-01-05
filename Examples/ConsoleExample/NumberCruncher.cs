namespace ConsoleExample
{
    internal class NumberCruncher
    {
        public int CrunchNumbers(int speed = 0)
        {
            if(speed > 100)
            {
                throw new Exception("I'm running too fast!");
            }

            return speed;
        }

    }
}
