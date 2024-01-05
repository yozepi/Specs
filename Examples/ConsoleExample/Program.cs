namespace ConsoleExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Discover();
            Run();
        }

        private static void Discover()
        {
            Console.WriteLine("== DISCOVERY ==============================");
            Console.WriteLine();

            var specs = new DemoSpec();

            var context = new DiscoveryContext(specs.GetType().Name);

            specs.RunContext = context;

            specs.TryIt();

            var description = context.GetSpecDescription();

            Console.WriteLine(description);
            Console.WriteLine();
            Console.WriteLine();

            var paths = context.GetSpecsWithPaths().ToList();
            foreach (var path in paths)
            {
                Console.WriteLine(path);
            }
        }

        private static void Run()
        {
            Console.WriteLine("== RUN ====================================");
            Console.WriteLine();

            var specs = new DemoSpec();

            var context = new BuildContext(specs.GetType().Name);

            specs.RunContext = context;

            specs.TryIt();

            Console.WriteLine($"TOTAL:   {context.SpecCount}");
            Console.WriteLine($"PASSED:  {context.SuccessCount}");
            Console.WriteLine($"FAILED:  {context.FailCount}");
            Console.WriteLine($"SKIPPED: {context.SkippedCount}");
            Console.WriteLine();
            Console.WriteLine(context.Description);
        }
    }
}