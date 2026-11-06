using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.Services;
using specs.BDD.Runner;
using specs.BDD.TestingPlatform.Discovery;
using specs.BDD.TestingPlatform.Execution;
using specs.BDD.TestingPlatform.Output;

namespace specs.BDD.TestingPlatform
{
    /// <summary>
    /// Microsoft.Testing.Platform test framework that discovers and runs the
    /// <see cref="Specifications"/> classes of the test application as a nested tree.
    /// </summary>
    internal sealed class SpecsTestFramework : SpecsExtension, ITestFramework, IDataProducer, IOutputDeviceDataProducer
    {
        private readonly IOutputDevice _outputDevice;
        private readonly ILogger _logger;
        private readonly SpecsSettings _settings;
        private readonly bool _isIdeClient;
        private readonly string _clientId;
        private readonly Func<Assembly?> _testAssembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecsTestFramework"/> class for the entry assembly.
        /// </summary>
        public SpecsTestFramework(IServiceProvider serviceProvider)
            : this(serviceProvider, Assembly.GetEntryAssembly)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecsTestFramework"/> class for a given assembly.
        /// </summary>
        internal SpecsTestFramework(IServiceProvider serviceProvider, Func<Assembly?> testAssembly)
        {
            _outputDevice = serviceProvider.GetOutputDevice();
            _logger = serviceProvider.GetLoggerFactory().CreateLogger<SpecsTestFramework>();
            _settings = SpecsSettings.From(serviceProvider.GetConfiguration());
            _isIdeClient = ClientMode.IsIde(serviceProvider.GetCommandLineOptions());
            _clientId = serviceProvider.GetClientInfo().Id;
            _testAssembly = testAssembly;

            _logger.LogDebug($"Client '{_clientId}', IDE tree mode: {_isIdeClient}");
        }

        /// <inheritdoc/>
        public Type[] DataTypesProduced => new[] { typeof(TestNodeUpdateMessage) };

        /// <inheritdoc/>
        public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context) =>
            Task.FromResult(new CreateTestSessionResult { IsSuccess = true });

        /// <inheritdoc/>
        public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context) =>
            Task.FromResult(new CloseTestSessionResult { IsSuccess = true });

        /// <inheritdoc/>
        public async Task ExecuteRequestAsync(ExecuteRequestContext context)
        {
            try
            {
                switch (context.Request)
                {
                    case DiscoverTestExecutionRequest discover:
                        await DiscoverAsync(discover, context).ConfigureAwait(false);
                        break;
                    case RunTestExecutionRequest run:
                        await RunAsync(run, context).ConfigureAwait(false);
                        break;
                    default:
                        throw new NotSupportedException(
                            $"Specs does not support the '{context.Request.GetType().Name}' request.");
                }
            }
            finally
            {
                context.Complete();
            }
        }

        // ── Requests ───────────────────────────────────────────────────────────────────────────

        private async Task DiscoverAsync(DiscoverTestExecutionRequest request, ExecuteRequestContext context)
        {
            var forest = await BuildForestAsync(context.CancellationToken).ConfigureAwait(false);
            var selected = SelectSpecs("Discover", forest, request.Filter);
            var factory = new TestNodeFactory(request.Session.SessionUid, _isIdeClient);

            foreach (var group in factory.Groups(selected))
                await context.MessageBus.PublishAsync(this, group).ConfigureAwait(false);

            foreach (var leaf in selected)
                await context.MessageBus.PublishAsync(this, factory.Leaf(leaf, DiscoveredTestNodeStateProperty.CachedInstance)).ConfigureAwait(false);

            if (!_isIdeClient && selected.Count > 0)
                await DisplayAsync(ConsoleTreeReport.Render(forest, selected, null, ResolveMarkerStyle()), context.CancellationToken).ConfigureAwait(false);
        }

        private async Task RunAsync(RunTestExecutionRequest request, ExecuteRequestContext context)
        {
            var forest = await BuildForestAsync(context.CancellationToken).ConfigureAwait(false);
            var selected = SelectSpecs("Run", forest, request.Filter);
            var factory = new TestNodeFactory(request.Session.SessionUid, _isIdeClient);

            if (_isIdeClient && selected.Count == 0 && forest.Leaves.Count > 0)
            {
                await _outputDevice.DisplayAsync(this, new WarningMessageOutputDeviceData(
                    $"[Specs] The run request matched none of the {forest.Leaves.Count} discovered specs. Filter: {SpecSelection.Describe(request.Filter)}"),
                    context.CancellationToken).ConfigureAwait(false);
            }

            foreach (var group in factory.Groups(selected))
                await context.MessageBus.PublishAsync(this, group).ConfigureAwait(false);

            var listener = new MtpRunListener(context.MessageBus, this, factory, _logger);
            var markerStyle = ResolveMarkerStyle();
            var cancellationToken = context.CancellationToken;

            await Task.Run(() =>
            {
                foreach (var classLeaves in selected.GroupBy(leaf => leaf.SpecClass!))
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var specClass = classLeaves.Key;
                    var items = classLeaves
                        .Select(leaf => new SpecRunItem<SpecNode>(
                            leaf, leaf.ContextPath, leaf.TestDescription, leaf.Kind == SpecNodeKind.GroupedDescribe))
                        .ToList();
                    var options = new SpecRunOptions(
                        _settings.ShowSourceLocations ? specClass.AssemblyPath : null, markerStyle);

                    SuiteRunner.Run(specClass.Root, items, listener, options, cancellationToken);
                }
            }, cancellationToken).ConfigureAwait(false);

            if (!_isIdeClient && selected.Count > 0)
                await DisplayAsync(ConsoleTreeReport.Render(forest, selected, listener.Outcomes, markerStyle), cancellationToken).ConfigureAwait(false);
        }

        // ── Helpers ────────────────────────────────────────────────────────────────────────────

        private IReadOnlyList<SpecNode> SelectSpecs(string requestKind, SpecForest forest, ITestExecutionFilter? filter)
        {
            // Logged before selecting so an unsupported filter is still recorded.
            _logger.LogInformation($"[Specs] {requestKind} request from client '{_clientId}': filter {SpecSelection.Describe(filter)}");

            var selected = SpecSelection.Select(forest, filter, emptyUidListSelectsAll: _isIdeClient);
            _logger.LogInformation($"[Specs] {requestKind} request selected {selected.Count} of {forest.Leaves.Count} specs.");
            return selected;
        }

        private async Task<SpecForest> BuildForestAsync(CancellationToken cancellationToken)
        {
            var assembly = _testAssembly()
                ?? throw new InvalidOperationException("Specs could not determine the test assembly.");

            var forest = SpecTreeBuilder.Build(assembly, _settings.ReportingLevel);
            foreach (var warning in forest.Warnings)
                await _outputDevice.DisplayAsync(this, new WarningMessageOutputDeviceData($"[Specs] {warning}"), cancellationToken).ConfigureAwait(false);

            return forest;
        }

        private Task DisplayAsync(string text, CancellationToken cancellationToken) =>
            _outputDevice.DisplayAsync(this, new TextOutputDeviceData(text), cancellationToken);

        private MarkerStyle ResolveMarkerStyle() =>
            GroupingSettings.ResolveMarkerStyle(_settings.ColorOutput, Environment.GetEnvironmentVariable("VSAPPIDNAME"));
    }
}
