using System;
using System.Threading.Tasks;

namespace Specs.Domain
{
    internal class TaskSpecDefinition : AbstractSpecDefinition<Func<Task>>
    {
        private readonly int timeout;

        private const int defaultTimeout = 60 * 1000;

        public TaskSpecDefinition(string name, Func<Task> task, bool dontRun, int timeout = defaultTimeout) 
            : base(name, task, dontRun)
        {
            this.timeout = timeout;
        }

        public override void Run()
        {
            this.SpecAction.Invoke().Wait(this.timeout);
        }
    }
}
