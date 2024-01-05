namespace Specs.Domain
{
    internal interface IRunContext
    {
        void AddSpec(AbstractSpecDefinition spec);

        IRunContext CreateChildContext(string name);

        void ContextRestored(IRunContext childContext);

        void AddBeforeEach(Runner runner);
    }
}
