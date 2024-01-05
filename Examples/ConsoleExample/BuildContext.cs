using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Specs.Domain;
using Specs.Domain.Exceptions;

namespace ConsoleExample
{
    internal class BuildContext : IRunContext
    {
        private readonly BuildContext? parent;
        private readonly List<Runner> beforeEach = new List<Runner>();

        public string Name { get; }

        public int SpecCount { get; private set; }
        public int SuccessCount { get; private set; }
        public int FailCount { get; private set; }
        public int SkippedCount { get; private set; }

        private int indentLevel = 0;

        private readonly StringBuilder descriptionBuilder = new StringBuilder();
        public string Description => descriptionBuilder.ToString();

        public BuildContext(string name, BuildContext? parent = null)
        {
            this.Name = name;
            this.parent = parent;

            if (parent != null)
            {
                this.indentLevel = parent.indentLevel + 1;
            }

            this.AppendLine(this.Name, this.indentLevel);
        }

        public void AddSpec(AbstractSpecDefinition spec)
        {
            this.SpecCount++;

            if (spec.DontRun)
            {
                this.SkipSpec(spec);
            }
            else
            {
                this.AppendLine(spec.Name, this.indentLevel + 1);
                try
                {
                    var action = ((SpecDefinition)spec).Action;

                    this.DoBeforeEach();

                    action.Invoke();
                    this.SuccessCount++;
                }
                catch (Exception ex)
                {
                    this.FailCount++;
                    WriteException(ex);
                }
            }
        }

        public IRunContext CreateChildContext(string name)
        {
            var child = new BuildContext(name, this);
            return child;
        }

        public void ContextRestored(IRunContext childContext)
        {
            var child = (BuildContext)childContext;

            this.SpecCount += child.SpecCount;
            this.SuccessCount += child.SuccessCount;
            this.FailCount += child.FailCount;
            this.SkippedCount += child.SkippedCount;

            this.descriptionBuilder.Append(child.Description);
        }


        public void AddBeforeEach(Runner runner)
        {
            this.beforeEach.Add(runner);
        }

        private void DoBeforeEach()
        {
            this.ProcessRunners(this.beforeEach, () => this.parent!.DoBeforeEach());
        }

        private void ProcessRunners(List<Runner> runners, Action parentInvocation)
        {
            try
            {
                if(this.parent != null)
                {
                    parentInvocation.Invoke();
                }

                foreach(Runner runner in runners)
                {
                    runner.Run();
                }
            }

            catch (ContextException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ContextException(ex);
            }
        }

        private void WriteException(Exception ex)
        {
            var exIndent = this.indentLevel + 2;

            this.AppendLine("EXCEPTION", exIndent);
            this.AppendLine(ex.GetType().FullName!, exIndent);
            this.AppendLine(ex.Message, exIndent);

            this.descriptionBuilder.AppendLine();
            this.AppendLine("CALL STACK", exIndent);

            if(ex is ContextException)
            {
                ex = ex.InnerException!;
            }

            var lines = ex.StackTrace!.Split('\n');
            foreach (var line in lines)
            {
                this.AppendLine(line, exIndent);
            }
            this.descriptionBuilder.AppendLine();
        }
        private void SkipSpec(AbstractSpecDefinition spec)
        {
            this.AppendLine($"SKIPPED - {spec.Name}", this.indentLevel + 1);
            this.SkippedCount++;
        }

        private void AppendLine(string text, int indentLevel)
        {
            this.descriptionBuilder.AppendLine($"{this.Tab(indentLevel)}{text}");
        }

        private string Tab(int indentLevel)
        {
            return new string(' ', indentLevel * 2);
        }
    }
}
