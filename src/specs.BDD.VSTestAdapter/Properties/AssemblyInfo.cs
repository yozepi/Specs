using System.Runtime.CompilerServices;

// specs.BDD.VSTestAdapter
//
// VSTest discovers adapters in this assembly via the following attributes applied to the classes:
//   - [FileExtension]         on SpecsTestDiscoverer — file types this adapter handles
//   - [DefaultExecutorUri]    on SpecsTestDiscoverer — links discoverer to executor
//   - [ExtensionUri]          on SpecsTestExecutor   — identifies the executor

// Grant the unit-test project access to internal types (TestFinder, ResultReporter, TestInvoker).
[assembly: InternalsVisibleTo("specs.BDD.VSTestAdapter.Tests")]
