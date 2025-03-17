namespace specs.core.Framework.Adapters
{
    internal interface ITestContextAdapter
    {
        void SetupOnce();
        void SetupEach();
        void Act();
        void TeardownEach();
        void TeardownOnce();
    }
}
