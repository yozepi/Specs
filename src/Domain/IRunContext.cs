namespace Specs.Domain
{
    internal interface IRunContext
    {
        void AddSpec(ISpecDefinition spec);

        IRunContext CreateChildContext(string name);

        void ContextRestored(IRunContext childContext);

        void AddBeforeEach(Runner runner);

        void AddAct(Runner runner);

        void AddAfterEach(Runner runner);
    }
}
