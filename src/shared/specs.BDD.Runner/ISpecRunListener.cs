using System;

namespace specs.BDD.Runner
{
    /// <summary>
    /// Receives spec execution events so each runner (VSTest, Microsoft.Testing.Platform)
    /// can report them in its own result model.
    /// </summary>
    /// <typeparam name="TTest">The runner's handle for a selected test.</typeparam>
    internal interface ISpecRunListener<in TTest>
    {
        /// <summary>The test is about to run (or be reported without running).</summary>
        void Started(TTest test);

        /// <summary>The test passed.</summary>
        void Passed(TTest test, DateTimeOffset startTime, TimeSpan duration);

        /// <summary>The test failed; <paramref name="exception"/> may still be wrapped by reflection.</summary>
        void Failed(TTest test, Exception exception, DateTimeOffset startTime, TimeSpan duration);

        /// <summary>The test is pending and was not run.</summary>
        void Pending(TTest test, DateTimeOffset startTime, TimeSpan duration);

        /// <summary>A grouped [Feature] finished running as a single unit.</summary>
        void GroupedCompleted(TTest test, GroupedReport report, DateTimeOffset startTime, TimeSpan duration);

        /// <summary>An informational message from the core executor.</summary>
        void LogInformation(string message);

        /// <summary>An error message from the core executor.</summary>
        void LogError(string message);
    }
}
