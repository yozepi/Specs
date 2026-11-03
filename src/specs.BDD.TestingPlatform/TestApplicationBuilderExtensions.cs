using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Helpers;

namespace specs.BDD.TestingPlatform
{
    /// <summary>
    /// Registers the Specs test framework with a Microsoft.Testing.Platform application.
    /// </summary>
    public static class TestApplicationBuilderExtensions
    {
        /// <summary>
        /// Registers the Specs test framework, which discovers and runs the
        /// <see cref="specs.BDD.Specifications"/> classes of the test application's entry assembly,
        /// and enables <c>--treenode-filter</c>.
        /// </summary>
        /// <param name="builder">The test application builder.</param>
        /// <returns>The same builder, for chaining.</returns>
        public static ITestApplicationBuilder AddSpecsBDD(this ITestApplicationBuilder builder)
        {
            builder.RegisterTestFramework(
                _ => new TestFrameworkCapabilities(),
                (_, serviceProvider) => new SpecsTestFramework(serviceProvider));
            builder.AddTreeNodeFilterService(new SpecsExtension());
            return builder;
        }
    }

    /// <summary>
    /// Identity of the Specs extension as reported to the platform.
    /// </summary>
    internal class SpecsExtension : IExtension
    {
        /// <inheritdoc/>
        public string Uid => "SpecsBDD";

        /// <inheritdoc/>
        public string Version => typeof(SpecsExtension).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

        /// <inheritdoc/>
        public string DisplayName => "Specs BDD";

        /// <inheritdoc/>
        public string Description => "Discovers and runs Specs BDD specifications.";

        /// <inheritdoc/>
        public Task<bool> IsEnabledAsync() => Task.FromResult(true);
    }

    /// <summary>
    /// Entry point used by the generated test application to auto-register the Specs test framework.
    /// </summary>
    public static class TestingPlatformBuilderHook
    {
        /// <summary>
        /// Registers the Specs test framework; invoked by the MSBuild-generated entry point.
        /// </summary>
        /// <param name="testApplicationBuilder">The test application builder.</param>
        /// <param name="arguments">The command-line arguments passed to the test application.</param>
        public static void AddExtensions(ITestApplicationBuilder testApplicationBuilder, string[] arguments)
            => testApplicationBuilder.AddSpecsBDD();
    }
}
