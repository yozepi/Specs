using System;
using System.Collections.Generic;

namespace specs.core.Framework.Processors.TestCases
{
    /// <summary>
    /// This class guarantees that the provided actions are run only once.
    /// </summary>
    internal class RunAllRunner
    {
        private bool hasRuntoCompletion = false;
        private readonly IEnumerable<Action> runList;

        public RunAllRunner(IEnumerable<Action> runList)
        {
            this.runList = runList;
        }

        public void RunAll()
        {
            if (!hasRuntoCompletion)
            {
                foreach (var action in runList)
                {
                    action.Invoke();
                }
                hasRuntoCompletion = true;
            }
        }
    }
}
