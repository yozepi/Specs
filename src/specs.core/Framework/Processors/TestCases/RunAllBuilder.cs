using specs.core.Framework.ParameterValidation;
using System;
using System.Collections.Generic;

namespace specs.core.Framework.Processors.TestCases
{
    /// <summary>
    /// BeforeAll and AfterAll actions must only be allowed to run once.
    /// This class manages the collection of these actions and an action is built that will only run them once.
    /// </summary>
    internal class RunAllBuilder
    {
        private readonly List<Action> actions = new List<Action>();

        private Lazy<Action?>? actionFactory = null;

        private bool resetFactory = true;

        public void AddAction(Action action)
        {
            actions.Add(action.ThrowIfNull(nameof(action)));
            resetFactory = true;
        }

        public Action? Build()
        {
            if (resetFactory)
            {
                actionFactory = new Lazy<Action?>(() =>
                {
                    if (actions.Count == 0)
                    {
                        return null;
                    }
                    return new RunAllRunner(actions).RunAll;
                });

                resetFactory = false;
            }

            return actionFactory!.Value;
        }
    }
}
