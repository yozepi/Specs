using System;
using System.Threading.Tasks;

namespace specs.core.Framework.Processors.TestCases
{
    internal interface ITestCaseBuilder
    {
        string? Description { get; }

        ITestCaseBuilder AddSetupOnce(Action action);
        ITestCaseBuilder AddSetupOnce(Func<Task> action);

        ITestCaseBuilder AddSetupEach(Action action);
        ITestCaseBuilder AddSetupEach(Func<Task> action);

        ITestCaseBuilder AddAct(Action action);
        ITestCaseBuilder AddAct(Func<Task> action);

        ITestCaseBuilder AddTest(string description, Action test);
        ITestCaseBuilder AddTest(string description, Func<Task> test);

        ITestCaseBuilder AddTeardownEach(Action action);
        ITestCaseBuilder AddTeardownEach(Func<Task> action);

        ITestCaseBuilder AddTeardownOnce(Action action);
        ITestCaseBuilder AddTeardownOnce(Func<Task> action);

        ITestCaseBuilder ChildTestCase(string description, Action<ITestCaseBuilder> configure);

        ITestCase Build();
    }
}