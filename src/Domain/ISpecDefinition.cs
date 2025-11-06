namespace Specs.Domain
{
    internal interface ISpecDefinition
    {
        string Name { get; }
        bool DontRun { get; }

        void Run();
    }
}
