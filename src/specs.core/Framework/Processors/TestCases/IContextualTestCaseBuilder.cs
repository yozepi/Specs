using System;
using System.Threading.Tasks;

namespace specs.core.Framework.Processors.TestCases
{
    internal interface IContextualTestCaseBuilder
    {
        string? Description { get; }

        IContextualTestCaseBuilder AddSetupOnce(Action action);
        IContextualTestCaseBuilder AddSetupOnce(Func<Task> action);

        IContextualTestCaseBuilder AddSetupEach(Action action);
        IContextualTestCaseBuilder AddSetupEach(Func<Task> action);

        IContextualTestCaseBuilder AddAct(Action action);
        IContextualTestCaseBuilder AddAct(Func<Task> action);

        IContextualTestCaseBuilder AddTest(string description, Action test);
        IContextualTestCaseBuilder AddTest(string description, Func<Task> test);

        IContextualTestCaseBuilder AddTeardownEach(Action action);
        IContextualTestCaseBuilder AddTeardownEach(Func<Task> action);

        IContextualTestCaseBuilder AddTeardownOnce(Action action);
        IContextualTestCaseBuilder AddTeardownOnce(Func<Task> action);

        IContextualTestCaseBuilder ChildTestCase(string description, Action<IContextualTestCaseBuilder> configure);

        IContextualTestCase Build();
    }
}