using System.Text;
using Specs.Domain;

namespace ConsoleExample
{
    internal class BuildContext : IRunContext
    {
        private readonly BuildContext? parent;
        private readonly List<BuildContext> children = new List<BuildContext>();

        private readonly List<Runner> beforeEach = new List<Runner>();
        private readonly List<Runner> act = new List<Runner>();
        private readonly List<Runner> afterEach = new List<Runner>();

        private readonly List<Exception> beforeEachErrors = new List<Exception>();
        private readonly List<Exception> actErrors = new List<Exception>();
        private readonly List<Exception> afterEachErrors = new List<Exception>();

        public string Name { get; }

        public int SpecCount { get; private set; }
        public int SuccessCount { get; private set; }
        public int FailCount { get; private set; }
        public int SkippedCount { get; private set; }

        private int indentLevel = 0;
        private bool actErrorHandled = false;

        private readonly StringBuilder descriptionBuilder = new StringBuilder();

        public string Description => descriptionBuilder.ToString();

        public IEnumerable<Exception> ContextErrors
        {
            get
            {
                foreach (var ex in this.AggregateBeforeEachErrors())
                {
                    yield return ex;
                }

                foreach (var ex in this.AggregateActErrors())
                {
                    yield return ex;
                }

                foreach (var ex in this.AggregateAfterEachErrors())
                {
                    yield return ex;
                }
            }
        }

        public Exception? LastActError { get; private set; }

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
                this.actErrorHandled = false;
                this.LastActError = null;

                this.AppendLine(spec.Name, this.indentLevel + 1);
                this.DoBeforeEach();
                this.DoAct();
                this.LastActError = this.AggregateParentAndSelfActErrors().LastOrDefault();
                try
                {
                    spec.Run();

                    if (this.LastActError != null && !this.actErrorHandled)
                    {
                        WriteException(this.LastActError);
                        this.FailCount++;
                    }
                    else
                    {
                        this.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    this.FailCount++;
                    WriteException(ex);
                }
                this.DoAfterEach();
            }
        }

        public IRunContext CreateChildContext(string name)
        {
            var child = new BuildContext(name, this);
            this.children.Add(child);
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

        public void AddAct(Runner runner)
        {
            this.act.Add(runner);
        }

        public void AddAfterEach(Runner runner)
        {
            this.afterEach.Add(runner);
        }

        public void WriteContextErrors()
        {
            var errors = this.ContextErrors.ToList();
            if (errors.Any())
            {
                this.descriptionBuilder.AppendLine();
                this.descriptionBuilder.AppendLine();

                this.AppendLine("CONTEXT ERRORS", this.indentLevel);
                foreach (var ex in ContextErrors)
                {
                    this.WriteException(ex);
                }
            }
        }

        public void LastActErrorHandled()
        {
            this.actErrorHandled = true;
        }

        private void DoBeforeEach()
        {
            this.beforeEachErrors.Clear();
            if (this.parent != null)
            {
                parent.DoBeforeEach();
            }

            foreach (Runner runner in this.beforeEach)
            {
                try
                {
                    runner.Run();
                }
                catch (Exception ex)
                {
                    this.beforeEachErrors.Add(ex);
                }
            }
        }

        private void DoAct()
        {
            this.actErrors.Clear();

            if (this.parent != null)
            {
                parent.DoAct();
            }

            foreach (Runner runner in this.act)
            {
                try
                {
                    runner.Run();
                }
                catch (Exception ex)
                {
                    this.actErrors.Add(ex);
                }
            }
        }

        private void DoAfterEach()
        {
            this.afterEachErrors.Clear();

            foreach (Runner runner in this.afterEach)
            {
                try
                {
                    runner.Run();
                }
                catch (Exception ex)
                {
                    this.afterEachErrors.Add(ex);
                }
            }

            if (this.parent != null)
            {
                parent.DoAfterEach();
            }
        }

        private IEnumerable<Exception> AggregateBeforeEachErrors()
        {
            foreach (var ex in this.beforeEachErrors)
            {
                yield return ex;
            }

            foreach (var child in this.children)
            {
                foreach (var ex in child.AggregateBeforeEachErrors())
                {
                    yield return ex;
                }
            }
        }

        private IEnumerable<Exception> AggregateParentAndSelfActErrors()
        {

            if(this.parent != null)
            {
                foreach (var ex in parent.AggregateParentAndSelfActErrors())
                {
                    yield return ex;
                }
            }

            foreach (var ex in this.actErrors)
            {
                yield return ex;
            }
        }

        private IEnumerable<Exception> AggregateActErrors()
        {
            foreach (var ex in this.actErrors)
            {
                yield return ex;
            }

            foreach (var child in this.children)
            {
                foreach (var ex in child.AggregateActErrors())
                {
                    yield return ex;
                }
            }
        }

        private IEnumerable<Exception> AggregateAfterEachErrors()
        {
            foreach (var child in this.children)
            {
                foreach (var ex in child.AggregateAfterEachErrors())
                {
                    yield return ex;
                }
            }

            foreach (var ex in this.afterEachErrors)
            {
                yield return ex;
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
