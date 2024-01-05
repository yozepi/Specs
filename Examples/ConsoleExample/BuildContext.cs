using System.Text;
using Specs.Domain;
using Specs.Domain.Exceptions;

namespace ConsoleExample
{
    internal class BuildContext : IRunContext
    {
        private readonly BuildContext? parent;
        private readonly List<Runner> beforeEach = new List<Runner>();
        private readonly List<Runner> afterEach = new List<Runner>();

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

        public void AddSpec(ISpecDefinition spec)
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
                    this.DoBeforeEach();
                    spec.Run();
                    this.DoAfterEach();
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

        public void AddAfterEach(Runner runner)
        {
            this.afterEach.Add(runner);
        }

        private void DoBeforeEach()
        {
            try
            {
                if (this.parent != null)
                {
                    parent.DoBeforeEach();
                }

                foreach (Runner runner in this.beforeEach)
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

        private void DoAfterEach()
        {
            try
            {
                foreach (Runner runner in this.afterEach)
                {
                    runner.Run();
                }

                if (this.parent != null)
                {
                    parent.DoAfterEach();
                }
            }
            catch (Exception)
            {

                throw;
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
        private void SkipSpec(ISpecDefinition spec)
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
