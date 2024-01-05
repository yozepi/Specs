using System;

namespace Specs.Domain
{
    internal delegate void IndexedRunnerDelegate<T>(string name, T action);

    public class IndexedRunner<T> 
    {
        private readonly IndexedRunnerDelegate<T> runnerDelegate = null;

        internal IndexedRunner(IndexedRunnerDelegate<T> runnerDelegate)
        {
            this.runnerDelegate = runnerDelegate;
        }

        public T this[string name]
        {
            set
            {
                this.runnerDelegate.Invoke(name, value);
            }
        }
    }
}
