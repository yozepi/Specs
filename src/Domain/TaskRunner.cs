using System;
using System.Threading.Tasks;

namespace Specs.Domain
{
    internal class TaskRunner : Runner
    {
        private readonly Func<Task> task;
        private readonly int timeout;

        private const int defaultTimeout = 60 * 1000;

        public TaskRunner(Func<Task> task, int timeout = defaultTimeout)
        {
            this.task = task;
            this.timeout = timeout;
        }

        public override void Run()
        {
            this.task.Invoke().Wait(this.timeout);
        }
    }
}
