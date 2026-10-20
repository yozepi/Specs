# Technical Context: Specs

## Tech Stack Overview

```mermaid
---
title: "C4 Level 2 — Container: Specs tech stack"
---
%%{init: {'theme': 'neutral', 'flowchart': {'nodeSpacing': 30, 'rankSpacing': 45, 'wrappingWidth': 160}}}%%
flowchart TD
    subgraph TEST["Test stack — net10.0 · Microsoft.NET.Test.Sdk 17.8"]
        direction TB
        SELF["<b>Specs itself</b><br/>[Container: Specs-hosted tests]<br/>Core, BDD, integration"]:::app
        NU["<b>NUnit 3.14</b><br/>[Library: NUnit3TestAdapter]<br/>Adapter tests only"]:::external
        NF["<b>NFluent 3.x</b><br/>[Library: NuGet]<br/>Fluent assertions"]:::external
        MQ["<b>Moq 4.20</b><br/>[Library: NuGet]<br/>+ Autofac.Extras.Moq 7.0"]:::external
    end

    subgraph DEL["Deliverables — netstandard2.1 · C# 8 · nullable"]
        direction TB
        CORE["<b>specs.core</b><br/>[Container: .NET library]<br/>Zero external deps"]:::app
        BDD["<b>specs.BDD</b><br/>[Container: .NET library]<br/>Authoring API"]:::app
        ADP["<b>specs.BDD.VSTestAdapter</b><br/>[Container: VSTest adapter]<br/>Discovery + execution"]:::app
    end

    subgraph ADEPS["Adapter dependencies"]
        direction TB
        MTP["<b>TestPlatform.ObjectModel</b><br/>[Library: NuGet 17.8]<br/>VSTest contracts"]:::external
        SRM["<b>System.Reflection.Metadata</b><br/>[Library: NuGet 6.0]<br/>Portable PDB reader"]:::external
    end

    TEST -->|"Tests [project refs]"| DEL
    DEL -->|"Adapter references [NuGet]"| ADEPS

    subgraph LEGEND["Legend"]
        direction TB
        L1["App-owned"]:::app
        L2["Third-party"]:::external
    end

    ADEPS ~~~ LEGEND

    classDef app fill:#56B870,stroke:#3D8B50,color:#fff
    classDef external fill:#E8A838,stroke:#B07D20,color:#fff
```

> 📌 UPDATED — the "Internal bridge" layer (`TestHarness` + `nspec 3.1`) was removed; see `systemPatterns.md` section "Test Hosting".

## Target Frameworks

| Project type | TFM | Notes |
|--------------|-----|-------|
| Deliverables (core, BDD, VSTestAdapter) | `netstandard2.1` | Broad compatibility — runs on .NET Core 3.0+, .NET 5+, .NET Framework support via netstandard2.1 polyfills |
| All test projects | `net10.0` 📌 UPDATED (was `net8.0`) | NFluent + Microsoft.NET.Test.Sdk (NUnit in the adapter tests only); the redundant `System.Text.Encodings.Web` pin was dropped from `specs.core.UnitTests` and `specs.BDD.UnitTests` |

## Language

- **C# 8.0** (`<LangVersion>8.0</LangVersion>` on adapter; default on others)
- `<Nullable>enable</Nullable>` everywhere — all public APIs use nullable reference type annotations
- Async/await throughout
- No records / no `init` accessors (netstandard2.1 + C# 8 constraint)

## Project Dependency Graph

```mermaid
---
title: "C4 Level 2 — Container: Project references (📌 UPDATED)"
---
%%{init: {'theme': 'neutral', 'flowchart': {'nodeSpacing': 30, 'rankSpacing': 45, 'wrappingWidth': 160}}}%%
flowchart TD
    subgraph TESTS["Folder: tests/"]
        direction LR
        CUT["<b>specs.core.UnitTests</b><br/>[Container: Specs-hosted]<br/>Core specs"]:::app
        BUT["<b>specs.BDD.UnitTests</b><br/>[Container: Specs-hosted]<br/>Authoring API specs"]:::app
        AUT["<b>specs.BDD.VSTestAdapter.Tests</b><br/>[Container: NUnit tests]<br/>Adapter components"]:::app
        AIT["<b>...IntegrationTests</b><br/>[Container: Specs-hosted]<br/>End-to-end VSTest"]:::app
    end

    subgraph SRC["Folder: src/"]
        direction TB
        ADP["<b>specs.BDD.VSTestAdapter</b><br/>[Container: netstandard2.1]<br/>VSTest adapter"]:::app
        BDD["<b>specs.BDD</b><br/>[Container: netstandard2.1]<br/>Folder specs.specification"]:::app
        CORE["<b>specs.core</b><br/>[Container: netstandard2.1]<br/>Zero external deps"]:::app
    end

    TESTS -->|"Reference adapter + BDD [project ref]"| SRC
    CUT -->|"Also references [project ref]"| CORE
    ADP -->|"Builds suites [project ref]"| BDD
    BDD -->|"Builds on [project ref]"| CORE

    subgraph LEGEND["Legend"]
        direction LR
        L1["App-owned project"]:::app
    end

    CORE ~~~ LEGEND

    classDef app fill:#56B870,stroke:#3D8B50,color:#fff
```

Every test project references `specs.BDD.VSTestAdapter` and — except `IntegrationTests` — `specs.specification`; only `specs.core.UnitTests` references `specs.core` directly. 📌 No project references `TestHarness` (deleted).

## Package References by Project

### Deliverables

| Project | Package references | Project references |
|---------|-------------------|--------------------|
| `specs.core` | **None** | — |
| `specs.specification` (specs.BDD) | **None** | `specs.core` |
| `specs.BDD.VSTestAdapter` | `Microsoft.TestPlatform.ObjectModel 17.8.0`, `System.Reflection.Metadata 6.0.0` | `specs.core`, `specs.specification` |

📌 UPDATED — `InternalsVisibleTo("specs.BDD.VSTestAdapter")` is declared **only** on `specs.specification` (for `Specifications.CoreBuilder`). `specs.core` dropped it: the adapter uses the public `TestCaseExecutor` / adapter-interface surface, and `specs.core` grants internals only to its test assemblies (`specs.core.UnitTests`, `specs.BDD.UnitTests`, `DynamicProxyGenAssembly2`) via `GlobalAttributes.cs`.

### Test projects

| Project | Test framework | Assertions | Mocking | SDK |
|---------|---------------|------------|---------|-----|
| `specs.core.UnitTests` | 📌 **Specs itself** (`Specifications` + `[Describe]`, run by `specs.BDD.VSTestAdapter`) | NFluent 3.1 | Autofac.Extras.Moq 7.0 | Microsoft.NET.Test.Sdk 17.8 |
| `specs.BDD.UnitTests` | 📌 UPDATED **Specs itself** (was NUnit3TestAdapter 5.0 + `TestHarness`/nspec) | NFluent 3.1 | — | Microsoft.NET.Test.Sdk 17.8 |
| `specs.BDD.VSTestAdapter.Tests` | NUnit 3.14 / NUnit3TestAdapter 4.5 | NFluent 3.0 | 📌 Moq 4.20.72 (`Mock<IFrameworkHandle>` in `TestInvoker_Tests`; `Mock<IRunContext>` / `Mock<ITestCaseFilterExpression>` in `SpecsTestExecutor_Tests`) | Microsoft.NET.Test.Sdk 17.8 |
| `specs.BDD.VSTestAdapter.IntegrationTests` | — (uses the Specs VSTest adapter itself) | NFluent 3.1 📌 | — | Microsoft.NET.Test.Sdk 17.8 |

> 📌 **`specs.core.UnitTests` and 📌 `specs.BDD.UnitTests` are self-hosted.** Neither references `NUnit`, `NUnit3TestAdapter`, `NUnit.Analyzers`, or `TestHarness`; every test class derives from `Specifications` and declares `[Describe]` methods, and each project takes a `ProjectReference` on `specs.BDD.VSTestAdapter` so VSTest can discover and run them. 📌 The leftover global `<Using Include="NUnit.Framework" />` was removed from both.
>
> **📌 See**: `systemPatterns.md` section "Test Hosting".

## 📌 Adapter Configuration (`.runsettings`)

The adapter reads optional settings from the VSTest run settings, under a `SpecsBDD` element directly beneath `RunSettings`:

```xml
<RunSettings>
  <SpecsBDD>
    <TestStructure>PerDescribe</TestStructure>
    <ShowSourceLocations>true</ShowSourceLocations>
    <ColorOutput>false</ColorOutput>
  </SpecsBDD>
</RunSettings>
```

| Key | Type | Default | Effect |
|-----|------|---------|--------|
| `SpecsBDD/TestStructure` 📌 UPDATED | `TestStructure` name (`PerSpec` / `PerDescribe`, case-insensitive) | unset (→ `PerSpec`) | Lowest-priority test-structure layer; overridden by assembly `[TestStructure]`, class `[TestStructure]`, or `[Describe(Structure = ...)]`. Renamed from `SpecReporting` (which replaced the former `GroupByRootContext` bool) |
| `SpecsBDD/ShowSourceLocations` 📌 NEW | `bool` | `false` | Grouped reports add `at <file>:line <n>` under each `It` |
| `SpecsBDD/ColorOutput` 📌 NEW | `bool` | `true` | `false` forces plain `✓` / `✗` markers in grouped reports |

| Environment variable | Effect |
|----------------------|--------|
| `VSAPPIDNAME` 📌 NEW | When `devenv.exe` (Visual Studio host) and color is enabled, grouped reports use emoji markers (`✅` / `❌`) instead of ANSI escapes |

`GroupingSettings` parses the XML defensively — missing or malformed XML falls back to the defaults above. For `TestStructure`, numeric strings and undefined names yield `null` and fall through to the default. `TestStructure` is honoured both during discovery (`IDiscoveryContext.RunSettings`) and when `RunTests(sources, …)` re-discovers (`IRunContext` is forwarded); `ShowSourceLocations` / `ColorOutput` are read at execution time.

> **📌 See**: `systemPatterns.md` section "Grouped Root-Context Execution" for the full precedence chain.

## Solution Layout

📌 UPDATED — `Specs.sln` moved to the repo root; test projects moved from `src/` to `tests/`; `specs.specification.UnitTests/` folder renamed `specs.BDD.UnitTests/` (matching its assembly); `TestHarness/` deleted.

```
Specs/
├── Specs.sln                               # 📌 moved from src/
├── src/                                    # deliverables only
│   ├── specs.core/                         # DELIVERABLE
│   │   └── Framework/
│   │       ├── Adapters/                   # ITestContextAdapter, ILoggingAdapter, ITestProviderAdapter, TestInstance
│   │       ├── Assertions/                 # 📌 ActExceptionAssertions, ExpectedExceptionNotThrownException
│   │       ├── ExceptionExtensions/
│   │       ├── Processors/
│   │       │   ├── TestCases/              # IContextualTestCase, builders, RunAll*
│   │       │   └── TestExecutors/          # TestCaseExecutor, TestCaseExecutorSettings, 📌 ActExceptionContext
│   │
│   ├── specs.specification/                # DELIVERABLE (asm: specs.BDD)
│   │   ├── Specifications.cs
│   │   ├── ContextBuilder.cs
│   │   ├── AssertBuilder.cs
│   │   ├── AssertBuilderAsync.cs
│   │   ├── IAssertProvider.cs
│   │   └── Attributes/DescribeAttribute.cs, TestStructure.cs, TestStructureAttribute.cs
│   │
│   └── specs.BDD.VSTestAdapter/            # DELIVERABLE
│       ├── Discovery/                      # SpecsTestDiscoverer, scanners, SuiteTreeBuilder,
│       │                                   #   GroupingSettings, SuiteTreeFlattener, PendingSpecs, SourceLocationReader
│       ├── Conversion/                     # SpecTest, TestCaseConverter, TestProperties, ContextPathFormat
│       ├── Execution/                      # SpecsTestExecutor, TestInvoker, SelectionTreeWalker,
│       │                                   #   GroupedContextRunner, MarkerStyle, TestFinder, ResultReporter,
│       │                                   #   VSTestLoggingAdapter, VSTestTestProvider
│       └── Properties/AssemblyInfo.cs
│
├── tests/                                  # 📌 NEW — all test projects
│   ├── specs.core.UnitTests/               # Specs-hosted (Specifications + [Describe])
│   ├── specs.BDD.UnitTests/                # 📌 Specs-hosted (was src/specs.specification.UnitTests, NUnit + nspec)
│   ├── specs.BDD.VSTestAdapter.Tests/      # NUnit
│   └── specs.BDD.VSTestAdapter.IntegrationTests/  # VSTest via Specs adapter
│
├── memory-bank/                            # documentation (this folder)
├── scripts/
├── .vscode/settings.json                   # 📌 dotnet.defaultSolution = Specs.sln
└── README.md
```

## Development Environment

### Prerequisites

- **.NET SDK 10.0+** 📌 UPDATED (required for the net10.0 test projects; SDK can build netstandard2.1 deliverables)
- **Visual Studio 2022 17.8+**, **VS Code with C#/C# Dev Kit**, or **Rider 2023+**
- **Git**

### Common commands

```powershell
# Restore + build everything (📌 solution is at the repo root)
dotnet restore Specs.sln
dotnet build   Specs.sln -c Release

# Run all tests
dotnet test Specs.sln

# Run a single test project (📌 test projects live under tests\)
dotnet test tests\specs.core.UnitTests\specs.core.UnitTests.csproj
dotnet test tests\specs.BDD.UnitTests\specs.BDD.UnitTests.csproj
dotnet test tests\specs.BDD.VSTestAdapter.Tests\specs.BDD.VSTestAdapter.Tests.csproj

# Run integration tests (exercises the Specs VSTest adapter end-to-end)
dotnet test tests\specs.BDD.VSTestAdapter.IntegrationTests\specs.BDD.VSTestAdapter.IntegrationTests.csproj

# 📌 Run a subset of Specs tests (FullyQualifiedName, DisplayName, ContextMethod, ContextPath, TestDescription, IsPending)
dotnet test tests\specs.BDD.VSTestAdapter.IntegrationTests\specs.BDD.VSTestAdapter.IntegrationTests.csproj --filter "FullyQualifiedName~Calculator"

# 📌 List only pending specs (or exclude them with IsPending=false)
dotnet test tests\specs.BDD.VSTestAdapter.IntegrationTests\specs.BDD.VSTestAdapter.IntegrationTests.csproj --filter IsPending=true

# Pack deliverables
dotnet pack src\specs.core\specs.core.csproj -c Release
dotnet pack src\specs.specification\specs.BDD.csproj -c Release
dotnet pack src\specs.BDD.VSTestAdapter\specs.BDD.VSTestAdapter.csproj -c Release
```

### VS Code tasks

`build`, `build-tests`, `publish`, `clean`, `test`, `test-with-coverage`, `restore` are pre-defined in `.vscode/tasks.json` and point at the root `Specs.sln` / `tests\specs.core.UnitTests`. `test` is the default test task; `build` is the default build task. 📌 The `watch` task and the `.NET Core Launch (TestHarness)` launch configuration were removed with `TestHarness`.

## Technical Constraints

| Constraint | Reason |
|------------|--------|
| Deliverables must remain `netstandard2.1` | Broad consumer compatibility |
| `specs.core` must have **zero** external NuGet references | Keeps the core framework-agnostic and trivial to embed |
| Nullable reference types must be enabled | Better null safety for public API |
| `IContextualTestCase` must be immutable after `Build()` | Thread-safety, cacheability, predictability |
| All async hooks ultimately reduce to a synchronous `Action` | Uniform public surface; lifecycle ordering would otherwise be non-deterministic |
| Discovery must not execute user `BeforeAll`/`Act`/etc. | Safety; discovery may happen in IDE without setting up resources |
| Test FullyQualifiedName must be stable across runs | VSTest expects stable IDs for re-runs / filtering |
| 📌 `RunTests(sources)` must apply the VSTest filter itself | VSTest passes `--filter` to source-based runs only via `IRunContext.GetTestCaseFilter`; ignoring it runs the whole assembly |
| 📌 Discovery and execution must build the suite tree through `SuiteTreeBuilder` | Any divergence in node descriptions or ordering makes `TestFinder` / `SelectionTreeWalker` unable to match discovered `ContextPath` values |
| 📌 `ContextPathFormat.Separator` (`" → "`) must not appear in user context descriptions | It is the delimiter for `ContextPath` parsing on the execution side |
| 📌 Pending specs must never invoke any lifecycle action | A parked `xContext` may contain deliberately broken setup; execution derives pending state from the rebuilt tree, not the serialized `IsPending` flag |
| 📌 Pending state is keyed by spec description within a node | `ContextualTestCase.PendingTests` is an ordinal `HashSet<string>`; sibling specs must keep unique descriptions (already required by `TestFinder`) |
| 📌 Every execution path must run `Act` + test via `ActExceptionContext.Run(act, test)` | `ShouldThrow<T>` reads the ambient `Act` exception; a path that calls `Act()` directly makes exception specs fail and lets an `Act` throw skip teardown |
| 📌 Types that custom front-ends / runners need from `specs.core` must be `public` | `specs.core` is an extensibility library — `ActExceptionContext`, `ActExceptionAssertions`, 📌 `TestCaseExecutor`, `ITestContextAdapter`, `ITestProviderAdapter`, `TestContextAdapter`, and `TestInstance` are public rather than `internal` + `InternalsVisibleTo`; the adapter must not regain an IVT grant on `specs.core` |

## Testing Strategy

- 📌 **Specs tests `specs.core` and `specs.BDD` with Specs.** `specs.core.UnitTests` and 📌 `specs.BDD.UnitTests` derive from `Specifications` + `[Describe]` and are discovered/run by `specs.BDD.VSTestAdapter`.
- **NUnit covers only the adapter.** `specs.BDD.VSTestAdapter.Tests` remains NUnit fixtures; the adapter cannot credibly test itself.
- **`specs.BDD.VSTestAdapter.IntegrationTests`** exercises the real VSTest pipeline end to end (`CalculatorSpecs` — 📌 class-level `[TestStructure(PerSpec)]` with a `Subtraction (grouped)` `[Describe(Structure = PerDescribe)]` override and constructor-level `BeforeAll` — plus `StringSpecs`, `AsyncAndExceptionSpecs`, and 📌 `PendingSpecs`, whose pending bodies and pending-context `BeforeAll` throw to prove they never run). 📌 `CalculatorSpecs` (divide by zero) and `AsyncAndExceptionSpecs` (async `Act`) assert exceptions with `ShouldThrow<T>` instead of manual `try/catch`.
- 📌 **TestHarness retired** — the nspec bridge and its `nspec` / `NUnit` dependencies were deleted once `specs.BDD.UnitTests` self-hosted.
- Test layout mirrors source layout (`src/specs.core/Framework/Processors/TestCases/...` → `tests/specs.core.UnitTests/Framework/Processors/TestCases/...`); 📌 test files and classes are named `<TypeUnderTest>_Tests`.
- Assertion style: NFluent (`Check.That(...).IsEqualTo(...)`) in the self-hosted projects; NUnit constraint-based (`Assert.That(value, Is.EqualTo(...))`) in `specs.BDD.VSTestAdapter.Tests`.

## VSTest Adapter Deployment

The adapter behaves like any other VSTest extension:

1. A consuming test project adds a `PackageReference` (or `ProjectReference`) to `specs.BDD.VSTestAdapter`.
2. The adapter assembly is placed in the test project's output directory.
3. VSTest discovers the adapter via the `[FileExtension(".dll" / ".exe")]`, `[DefaultExecutorUri("executor://SpecsTestExecutor/v1")]`, and `[ExtensionUri("executor://SpecsTestExecutor/v1")]` attributes.
4. Test Explorer / `dotnet test` then call `DiscoverTests` and `RunTests` against it.

## Common Issues & Resolutions

| Issue | Cause | Fix |
|-------|-------|-----|
| `InvalidSpecificationException: Specifications subclass has no [Describe] methods` at discovery | The suite class doesn't declare any `[Describe]` method | Add at least one `[Describe("...")]`-decorated `public void` method |
| Async hangs during a test | User code calls `.Result` / `.Wait()` on captured contexts without `ConfigureAwait(false)` | Use `ConfigureAwait(false)` in libraries under test, or use the `Func<Task>` overloads |
| "Go to source" navigates only to class | Source PDB missing or non-portable | Build with `<DebugType>portable</DebugType>` (default for SDK-style projects) |
| Test result missing from Test Explorer | The adapter assembly wasn't copied to the output directory | Ensure the test project references `specs.BDD.VSTestAdapter` directly or via a package |
| 📌 Test Explorer shows a flat list instead of a tree | The `TestCase.Hierarchy` (RFC-0033) property is missing or not exactly 4 elements | `TestCaseConverter.BuildHierarchy` must emit `[null, Namespace, Class, ContextPath]` |
| 📌 `"Test '...' no longer exists in the current build."` | Stale `TestCase` objects from a previous discovery no longer match the rebuilt suite tree | Re-run discovery (rebuild / refresh Test Explorer) |
| 📌 `BeforeAll` runs more than once for a partial selection | A code path bypassed `SelectionTreeWalker` and used the cascaded single-test lifecycle | Route suite runs through `TestInvoker.InvokeSuite` |
| 📌 Grouped output shows raw `ESC[32m` escape sequences | Host does not render ANSI escapes and was not detected as Visual Studio | Set `<SpecsBDD><ColorOutput>false</ColorOutput>` in `.runsettings` |
| 📌 `ItAsync[...]` "Go to Source" lands on the `[Describe]` method | Async body sequence points live on the state machine `MoveNext`, and the wrapper hid the user lambda | Fixed: `AddTest(Func<Task>)` binds `RunSync` to the user delegate; `SourceLocationReader` follows `MoveNext` |
| 📌 `dotnet test --filter` runs every Specs test | Adapter versions before the filtering fix ignored `IRunContext.GetTestCaseFilter` in `RunTests(sources)` | Fixed: `SpecsTestExecutor` applies the filter; an invalid expression is logged as an error and runs nothing |
| 📌 Old `Grouped = true` / `[assembly: GroupSpecsByRootContext]` no longer compile | Replaced by the `TestStructure` API | Use `Structure = TestStructure.PerDescribe` / `[assembly: TestStructure(TestStructure.PerDescribe)]` |
| 📌 `SpecReporting` / `Reporting =` / `[SpecReporting]` no longer compile; `<SpecReporting>` in `.runsettings` is ignored | Renamed to `TestStructure` (decision #32) | `Individual` → `PerSpec`, `GroupedByRootContext` → `PerDescribe`; `Reporting =` → `Structure =`; use `<SpecsBDD><TestStructure>` |
| 📌 A spec fails with the `Act` exception instead of its own assertion message | `Act` threw and the `It` did not verify it; the unverified `Act` exception replaces any `It` failure | Assert it with `It[...] = ShouldThrow<T>(...)`, or fix the `Act` |
| 📌 `ShouldThrow<T>` fails with `ExpectedExceptionNotThrownException` | `Act` threw nothing, or a type not assignable to `T` (see `InnerException`) | Check the `Act` under that context and the expected type |
| 📌 `ItAsync[...] = ShouldThrow<T>()` does not compile | `ShouldThrow` returns `Action`; there is no async variant by design | Use `It[...]` — the `Act` exception (sync or async) is already captured |
