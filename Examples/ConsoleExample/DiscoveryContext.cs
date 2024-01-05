using System.Runtime.CompilerServices;
using System.Text;
using Specs.Domain;

namespace ConsoleExample
{
    internal class DiscoveryContext : IRunContext
    {
        private readonly DiscoveryContext? parent;
        private readonly List<(string name, bool wontRun)> specMethods = new List<(string name, bool wontRun)>();
        private readonly List<DiscoveryContext> childContexts = new List<DiscoveryContext>();

        public DiscoveryContext(string name, DiscoveryContext? parent = null)
        {
            this.Name = name;
            this.parent = parent;
        }

        public string Name { get; }

        public void AddSpec(ISpecDefinition spec)
        {
            specMethods.Add((name: spec.Name, wontRun: spec.DontRun));
        }

        public IRunContext CreateChildContext(string name)
        {
            var child = new DiscoveryContext(name);
            this.childContexts.Add(child);
            return child;
        }

        public void ContextRestored(IRunContext childContext)
        {
        }

        public string GetSpecDescription()
        {
            return this.BuildDescription(0).ToString();
        }

        public IEnumerable<string> GetSpecsWithPaths()
        {
            foreach (var spec in this.specMethods)
            {
                yield return $"{{{this.Name}}} => [{spec.name}]";
            }

            foreach (var child in this.childContexts)
            {
                foreach (var path in child.GetSpecsWithPaths())
                {
                    yield return $"{{{this.Name}}} => {path}";
                }
            }
        }

        private StringBuilder BuildDescription(int indentLevel)
        {
            var sb = new StringBuilder();

            foreach (var spec in this.specMethods)
            {
                sb.Append(Tab(indentLevel + 1));
                var runState = spec.wontRun ? '-' : '+';
                sb.Append(runState);

                sb.AppendLine(spec.name);
            }

            foreach (var child in this.childContexts)
            {
                var childSb = child.BuildDescription(indentLevel + 1);
                if (childSb.Length > 0)
                {
                    //sb.AppendLine();
                    sb.Append(childSb);
                }
            }

            if (sb.Length > 0)
            {
                sb.Insert(0, $"{this.Name}\n");
                sb.Insert(0, Tab(indentLevel));
            }

            return sb;
        }

        private static string Tab(int indentLevel)
        {
            return new string(' ', indentLevel * 2);
        }

        public void AddBeforeEach(Runner runner)
        {
        }

        public void AddAfterEach(Runner runner)
        {
        }

        public void AddAct(Runner runner)
        {
        }
    }
}
