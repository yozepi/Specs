namespace specs.core.Framework.Adapters
{
    /// <summary>
    /// Lets a host decide what each lifecycle phase does, so the executor can drive the phases
    /// without knowing how the host builds or scopes them.
    /// </summary>
    public interface ITestContextAdapter
    {
        /// <summary>One-time preparation shared by all tests (BeforeAll).</summary>
        void SetupOnce();

        /// <summary>Per-test preparation that gives each test a fresh starting state (BeforeEach).</summary>
        void SetupEach();

        /// <summary>The behavior under test, performed once per test before its assertion.</summary>
        void Act();

        /// <summary>Per-test cleanup so tests cannot affect each other (AfterEach).</summary>
        void TeardownEach();

        /// <summary>One-time cleanup of what <see cref="SetupOnce"/> created (AfterAll).</summary>
        void TeardownOnce();
    }
}
