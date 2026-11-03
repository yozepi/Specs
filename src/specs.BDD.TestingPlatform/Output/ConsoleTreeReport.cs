using System.Collections.Generic;
using System.Linq;
using System.Text;
using specs.BDD.Runner;
using specs.BDD.TestingPlatform.Discovery;
using specs.BDD.TestingPlatform.Execution;

namespace specs.BDD.TestingPlatform.Output
{
    /// <summary>
    /// Renders the selected part of the spec tree as an indented, NSpec-style report for console output.
    /// </summary>
    internal static class ConsoleTreeReport
    {
        /// <summary>
        /// Renders every group and leaf that leads to a selected leaf.
        /// </summary>
        /// <param name="forest">The discovered tree.</param>
        /// <param name="selected">The selected leaves.</param>
        /// <param name="outcomes">Run outcomes per leaf; <c>null</c> for a discovery listing.</param>
        /// <param name="markerStyle">How pass/fail/pending markers render.</param>
        public static string Render(
            SpecForest forest,
            IReadOnlyCollection<SpecNode> selected,
            IReadOnlyDictionary<SpecNode, SpecOutcome>? outcomes,
            MarkerStyle markerStyle)
        {
            var visible = new HashSet<SpecNode>(selected);
            foreach (var leaf in selected)
                visible.UnionWith(leaf.Ancestors());

            var markers = ReportMarkers.For(markerStyle);
            var output = new StringBuilder();
            output.AppendLine(forest.AssemblyName);
            foreach (var root in forest.Roots.Where(visible.Contains))
                RenderNode(root, depth: 1, visible, outcomes, markers, output);

            return output.ToString().TrimEnd();
        }

        private static void RenderNode(
            SpecNode node,
            int depth,
            HashSet<SpecNode> visible,
            IReadOnlyDictionary<SpecNode, SpecOutcome>? outcomes,
            (string Pass, string Fail, string Pending) markers,
            StringBuilder output)
        {
            var indent = ReportMarkers.Indent(depth);
            if (!node.IsLeaf)
            {
                output.AppendLine(indent + node.Text);
                foreach (var child in node.Children.Where(visible.Contains))
                    RenderNode(child, depth + 1, visible, outcomes, markers, output);
                return;
            }

            if (outcomes == null)
            {
                var pending = node.IsPending ? " - PENDING" : "";
                output.AppendLine($"{indent}- {node.Text}{pending}");
                return;
            }

            if (!outcomes.TryGetValue(node, out var outcome))
            {
                output.AppendLine($"{indent}- {node.Text} - NOT RUN");
                return;
            }

            output.AppendLine(outcome switch
            {
                SpecOutcome.Passed => $"{indent}{markers.Pass} {node.Text}",
                SpecOutcome.Failed => $"{indent}{markers.Fail} {node.Text} - FAILED",
                _ => $"{indent}{markers.Pending} {node.Text} - PENDING"
            });
        }
    }
}
