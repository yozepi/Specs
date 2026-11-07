# Specs

A .NET BDD testing framework with two coexisting API styles over a framework-agnostic core, plus a VSTest adapter for native IDE integration.

## 📚 Documentation

**Before working on this project**, review the documentation in the `memory-bank/` folder. Read the 6 core files in order:

1. **[Project Brief](memory-bank/projectbrief.md)** - Foundation: scope, goals, solution structure
2. **[Product Context](memory-bank/productContext.md)** - Why the project exists and what it solves
3. **[Active Context](memory-bank/activeContext.md)** - Current work focus and recent changes
4. **[System Patterns](memory-bank/systemPatterns.md)** - Architecture, design patterns, key decisions
5. **[Tech Context](memory-bank/techContext.md)** - Stack, dependencies, build and test commands
6. **[Progress](memory-bank/progress.md)** - What works, what's left, known limitations

Supplementary references: **[Core Components](memory-bank/core-components.md)**, **[Test Patterns](memory-bank/test-patterns.md)**, **[Glossary](memory-bank/glossary.md)**.

## Installation

Install the package for your test host; each brings in `specs.BDD` and `specs.core`.

| Test host | Package |
|---|---|
| VSTest | `dotnet add package specs.BDD.VSTest` |
| Microsoft.Testing.Platform | `dotnet add package specs.BDD.MTP` |

For MTP under `dotnet test` on .NET 10, also add `"test": { "runner": "Microsoft.Testing.Platform" }` to `global.json` (see [Running on Microsoft.Testing.Platform](#running-on-microsofttestingplatform)).

## Solution Structure

### Deliverables (4 projects)
- **specs.core** - Framework-agnostic core: builder, immutable test-case model, executor (.NET Standard 2.1)
- **specs.BDD** (`specs.specification`) - High-level BDD authoring API built on specs.core
- **specs.BDD.VSTestAdapter** (package `specs.BDD.VSTest`) - VSTest discovery + execution for Test Explorer and `dotnet test`
- **specs.BDD.TestingPlatform** (package `specs.BDD.MTP`) - Microsoft.Testing.Platform (MTP) runner that renders the full spec tree

Both runners compile the runner-agnostic discovery and execution code in `src/shared/specs.BDD.Runner/`.

### Test projects (4 + 2 MTP)
- **specs.core.UnitTests** - Core framework tests, written in Specs itself
- **specs.BDD.UnitTests** - BDD API tests
- **specs.BDD.VSTestAdapter.Tests** - Adapter component tests
- **specs.BDD.IntegrationTests** - End-to-end VSTest pipeline tests
- **tests/mtp/specs.BDD.TestingPlatform.Tests** - MTP runner tests (NUnit on MTP)
- **tests/mtp/specs.BDD.TestingPlatform.IntegrationTests** - The `specs.BDD.IntegrationTests` specs, run through MTP

The MTP projects are in `Specs.Mtp.slnx` at the repo root. `tests/mtp/global.json` puts `dotnet test` in MTP mode, so run them from that folder:
```
cd tests/mtp
dotnet test --solution ../../Specs.Mtp.slnx
```
The root holds two solutions, so other root commands must name one, e.g. `dotnet build Specs.sln`.

### Legacy / internal (1)
- **Specs** - Legacy property-based `Spec` class and `Domain/` layer

## Two API Styles, One Paradigm

Both styles express the same **Suite → Feature Set → Scenario → Specification** hierarchy and differ only in syntax.

### 1. Property-Based API (Legacy `Spec` class)
```csharp
public class MySpecs : Spec
{
    BeforeEach = () => { /* setup */ };
    Act = () => { /* action */ };
    It["should work"] = () => { /* assertion */ };
}
```

### 2. Method-Based API (`Specifications` class)
```csharp
public class MySpecs : Specifications
{
    [Feature("My feature")]
    public void my_feature()
    {
        BeforeEach(() => { /* setup */ });
        Act(() => { /* action */ });
        Describe["feature"] = () => {
            It["should work"] = () => { /* assertion */ };
        };
    }
}
```

> A `[Feature]`-decorated method is required for the VSTest adapter to discover tests in a suite.

### Pending specs
```csharp
It["is not written yet"] = TODO;               // ItAsync["..."] = TODOAsync;
xIt["is excluded for now"] = () => { /* kept, never run */ };   // also xItAsync
xContext["a pending scenario"] = () => { /* every spec inside is pending */ };   // also xDescribe
```
Pending specs are discovered but never executed (no `BeforeEach` / `Act` / `AfterEach`; a context whose selected specs are all pending skips its `BeforeAll` / `AfterAll`). The adapter reports them as **Skipped**, and grouped results list them with a `○ ... - PENDING` marker. Select them with `dotnet test --filter IsPending=true` (or exclude them with `IsPending=false`).

### Expected exceptions
```csharp
Act(() => calculator.Divide(1, 0));

It["throws"] = ShouldThrow<DivideByZeroException>();
It["explains why"] = ShouldThrow<DivideByZeroException>(ex =>
    Check.That(ex.Message.ToLowerInvariant()).Contains("zero"));
```
An exception thrown by `Act` is captured for each spec. `ShouldThrow<T>` passes when it is assignable to `T` (and the optional verifier succeeds), and fails if `Act` threw nothing or another type. Any other spec whose `Act` throws fails with the `Act` exception; `AfterEach` still runs.

## Running on Microsoft.Testing.Platform

Reference `specs.BDD.MTP` instead of `specs.BDD.VSTest`. The test project becomes a test application (it defaults to `<OutputType>Exe</OutputType>`), and the whole tree shows up as nested nodes: namespace → class → `[Feature]` → contexts → specs.

Every spec has a path, which is its test ID and what `--treenode-filter` matches:
```
/MyTests/My.Specs/CartSpecs/Cart/when empty/shows a message
```
```
dotnet test --treenode-filter "/*/*/CartSpecs/Cart/**"     # a [Feature] and everything under it
dotnet test --treenode-filter "/*/*/*/*/when empty/*"      # specs directly inside every 'when empty' context
```
`/` and `%` inside names are written as `%2F` and `%25`; filter operators such as `(`, `)`, `*`, `|`, `&`, `[` and `]` must be escaped with `\`.

IDEs receive the full tree, but Visual Studio's Test Explorer only nests four levels, so it shows namespace › class › `[Feature]` › `describe → context → spec` (VS Code shows only namespace › class › `describe → context → spec`). Console runs show each spec's full path and print an indented tree report. `[Feature(Level = ReportingLevel.Feature)]` and `[ReportingLevel]` still run a `[Feature]` as one result. Runner settings go in `[appname].testconfig.json` or environment variables (`SpecsBDD__ReportingLevel`):
```json
{
  "SpecsBDD": {
    "ReportingLevel": "Feature",
    "ShowSourceLocations": true,
    "ColorOutput": false
  }
}
```

## Key Features
- Hierarchical test organization with parent-child contexts
- Five-phase lifecycle (SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce)
- `BeforeAll` / `AfterAll` fire exactly once per context, even for partial test runs
- Full async/await support on every lifecycle hook
- Pending specs (`TODO`, `xIt`, `xContext`) reported as Skipped and filterable via `IsPending`
- Fluent API for test construction
- Framework-agnostic core with adapter pattern (zero external dependencies)
- Test Explorer integration with hierarchical nesting and source navigation
- Two coexisting API styles for gradual migration

## Technology Stack
- .NET Standard 2.1 (deliverables), net8.0 (tests)
- Microsoft.TestPlatform.ObjectModel (VSTest adapter only)
- Microsoft.Testing.Platform 2.x (MTP runner only)
- NUnit, NFluent (tests)

## License
See [LICENSE](LICENSE) file for details.
