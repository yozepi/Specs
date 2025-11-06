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

        public Task<int> CrunchNumbersAsync(int speed = 0)
        {
            return Task.FromResult(this.CrunchNumbers(speed));
        }
    }
}
