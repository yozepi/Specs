# specs.core

[![NuGet](https://img.shields.io/nuget/v/specs.core.svg)](https://www.nuget.org/packages/specs.core)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/yozepi/Specs/blob/main/LICENSE)

The framework-agnostic core of [Specs](https://github.com/yozepi/Specs), an RSpec-style BDD framework for .NET. It gives you the hierarchical spec builder, the immutable test-case tree, and the `BeforeAll` / `BeforeEach` / `Act` / `AfterEach` / `AfterAll` lifecycle executor. Its adapter interfaces are public, so you can host that lifecycle in any test runner. It has no runtime dependencies.

## Which package do I need?

Most people writing specs don't need to reference this package directly. Install a runner package instead; it brings `specs.core` along.

| Package | Use it when |
|---|---|
| `specs.BDD.MTP` | You write specs and run them with Microsoft.Testing.Platform (full spec tree in the IDE, `--treenode-filter`, `dotnet run`) |
| `specs.BDD.VSTest` | You write specs and run them with VSTest (`dotnet test`, Test Explorer, `--filter`, `.runsettings`) |
| `specs.BDD.Templates` | You want `dotnet new specs-mtp` / `specs-vstest` project templates |
| `specs.core` | You're building your own runner or host and want the same lifecycle semantics as the built-in runners |

More: [Pick a Test Host](https://github.com/yozepi/Specs/wiki/Usage-Guide#pick-a-test-host)

## Installation

```shell
dotnet add package specs.core
```

## Quick Start

Build a test case, then run it through `TestCaseExecutor` with three small adapters: the lifecycle, the selected tests, and a logger.

```csharp
using specs.core.Framework.Adapters;
using specs.core.Framework.Processors.TestCases;
using specs.core.Framework.Processors.TestExecutors;

var items = new List<int>();
var count = 0;

IContextualTestCase root = new ContextualTestCaseBuilder()
    .AddSetupEach(() => items.Clear())
    .AddAct(() => { items.Add(42); count = items.Count; })
    .AddTest("has one item", () =>
    {
        if (count != 1) throw new Exception($"Expected 1 but was {count}");
    })
    .Build();

var executor = new TestCaseExecutor(
    new TestCaseExecutorSettings(),
    new ConsoleLogger(),
    TestContextAdapter.FromTestCase(root),
    new TestCaseProvider(root));

executor.ProcessTestCase(CancellationToken.None);

class TestCaseProvider : ITestProviderAdapter
{
    private readonly ITestCase testCase;

    public TestCaseProvider(ITestCase testCase) => this.testCase = testCase;

    public IReadOnlyCollection<TestInstance> GetTestInstances() =>
        testCase.Tests
            .Where(t => !testCase.PendingTests.Contains(t.description))
            .Select(t => new TestInstance(t.description, t.test))
            .ToList();
}

class ConsoleLogger : ILoggingAdapter
{
    public void Log(string message) => Console.WriteLine(message);
    public void LogAssertSuccess(string message) => Console.WriteLine($"PASS {message}");
    public void LogAssertFailure(Exception exception, string message) => Console.WriteLine($"FAIL {message}: {exception.Message}");
    public void LogException(Exception exception) => Console.WriteLine($"ERROR {exception}");
}
```

```shell
dotnet run
```

## Usage

### Build a spec tree

`ContextualTestCaseBuilder` has a fluent API for lifecycle actions and tests. Every `Add*` method has `Action` and `Func<Task>` overloads. Use `ChildTestCase` to add nested contexts that inherit their parent's lifecycle, and `AddPendingTest` or `ChildTestCase(..., isPending: true)` for specs that are reported but never run. `Build()` returns an immutable `IContextualTestCase`, and its nested contexts are in `ChildContexts`.

```csharp
IContextualTestCase root = new ContextualTestCaseBuilder()
    .AddSetupOnce(async () => await OpenConnectionAsync())
    .AddSetupEach(() => cart = new Cart())
    .ChildTestCase("when an item is added", child => child
        .AddAct(() => cart.Add("apple"))
        .AddTest("contains the item", () => { if (!cart.Contains("apple")) throw new Exception("missing"); })
        .AddPendingTest("applies a discount", () => { }))
    .AddTeardownOnce(() => CloseConnection())
    .Build();
```

On each node, `SetupOnce`, `SetupEach`, and `Act` already include the ancestors' actions (parent first), and `TeardownEach` / `TeardownOnce` include them in reverse order. `OwnSetupOnce` / `OwnTeardownOnce` hold only that node's own once-actions. Use them if you walk the tree and want each `BeforeAll` / `AfterAll` to fire exactly once.

More: [Lifecycle Hooks](https://github.com/yozepi/Specs/wiki/Usage-Guide#lifecycle-hooks)

### Host the lifecycle in your runner

`TestCaseExecutor.ProcessTestCase` runs `SetupOnce`, then `SetupEach` → `Act` + test → `TeardownEach` for each test, then `TeardownOnce`. You supply three pieces:

| Interface | You provide | Built-in helper |
|---|---|---|
| `ITestContextAdapter` | The five lifecycle phases | `TestContextAdapter` (five delegates, or `TestContextAdapter.FromTestCase(node)`) |
| `ITestProviderAdapter` | The `TestInstance`s selected for this run (apply your filters here) | — |
| `ILoggingAdapter` | Pass / fail / exception reporting to your host | — |

A failing test is logged and the run continues; the token passed to `ProcessTestCase` stops the run between tests. An exception thrown by `Act` fails only that test, and `AfterEach` / `AfterAll` still run.

More: [Design Patterns — Adapter](https://github.com/yozepi/Specs/wiki/Design-Patterns#adapter)

### Verify exceptions thrown by `Act`

`VerifyActException<T>()` turns a verifier into a test that passes when `Act` threw a `T` (or a subtype). If nothing was thrown, or the wrong type was, it throws `ExpectedExceptionNotThrownException`.

```csharp
using specs.core.Framework.Assertions;

Action<InvalidOperationException> verify = ex =>
{
    if (ex.Message != "Cart is empty") throw new Exception($"Unexpected message: {ex.Message}");
};

builder
    .AddAct(() => cart.Checkout())
    .AddTest("throws when the cart is empty", verify.VerifyActException);
```

More: [Expected Exceptions](https://github.com/yozepi/Specs/wiki/Usage-Guide#expected-exceptions)

## Configuration

| Setting | Where | Default | Description |
|---|---|---|---|
| `StopOnAssertFailure` | `TestCaseExecutorSettings` | `false` | When `true`, the first failing test is rethrown and the rest of the run stops |

## Compatibility

- Package target framework: `netstandard2.1` (.NET Core 3.0+, .NET 5+).
- No runtime package dependencies.
- Nullable annotations and XML docs included. Symbols are published as `.snupkg` with SourceLink.

## Learn More

- [Wiki home](https://github.com/yozepi/Specs/wiki)
- [Usage Guide](https://github.com/yozepi/Specs/wiki/Usage-Guide)
- [Decision Log](https://github.com/yozepi/Specs/wiki/Decision-Log)
- [Troubleshooting](https://github.com/yozepi/Specs/wiki/Troubleshooting)
- [Release notes](https://github.com/yozepi/Specs/releases)

## License

MIT — see [LICENSE](https://github.com/yozepi/Specs/blob/main/LICENSE).
