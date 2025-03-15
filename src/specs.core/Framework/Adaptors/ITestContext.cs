namespace specs.core.Framework.Adaptors
{
    internal interface ITestContext
    {
        void Setup();
        void Act();
        void Assert();
        void Teardown();
    }
}
