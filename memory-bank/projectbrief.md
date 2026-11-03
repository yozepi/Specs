# Project Brief: Specs Testing Framework

## Project Identity

| Property | Value |
|----------|-------|
| **Name** | Specs |
| **Type** | .NET BDD testing framework + VSTest adapter + 📌 Microsoft.Testing.Platform runner |
| **Core Target** | .NET Standard 2.1 (deliverables) / net10.0 (tests) 📌 UPDATED |
| **Language** | C# 10 📌 UPDATED (deliverables; was C# 8; 📌 C# 12 on the MTP runner) with nullable reference types and implicit usings |
| **Repository** | <https://github.com/yozepi/Specs> |

## Core Purpose

Specs is a .NET testing framework that provides **two complementary BDD-style APIs** sitting on top of a framework-agnostic core, with **two runners**: a **VSTest adapter** that makes specs discoverable and runnable from Visual Studio Test Explorer, `dotnet test`, and any IDE that speaks the VSTest protocol, and 📌 a **Microsoft.Testing.Platform (MTP) runner** that shows the full spec tree and supports `--treenode-filter`.

## Primary Goals

1. **Structured BDD authoring** — intuitive `Describe` / `Context` / `It` indexers and `[Describe]` methods.
2. **Hierarchical organization** — nested contexts inherit setup/act behavior from their parents.
3. **Sophisticated lifecycle** — `SetupOnce`, `SetupEach`, `Act`, `TeardownEach`, `TeardownOnce` with correct ordering.
4. **First-class async** — every lifecycle hook has a `Func<Task>` overload.
5. **Single, modern authoring API** — method-based `Specifications` (📌 the legacy property-based `Spec` API was removed).
6. **Framework-agnostic core** — the core has zero test-framework dependencies; everything plugs in through adapters. 📌 Its executor and adapter interfaces are public, so third-party runners can host the same lifecycle.
7. **Native IDE integration** — VSTest adapter delivers Test Explorer discovery and execution with source navigation.
8. 📌 NEW **Runner choice** — the same specs run unchanged under VSTest or Microsoft.Testing.Platform, with identical lifecycle semantics (both runners share one execution core).

## Solution Structure

The solution (📌 `Specs.sln`, now at the repo root) contains 📌 **8 projects** — deliverables under `src/`, test projects under `tests/`. 📌 Two more MTP test projects are built by `Specs.Mtp.slnx` (📌 UPDATED — moved to the repo root; it lists `specs.core`, `specs.BDD`, `specs.BDD.TestingPlatform`, and the two MTP projects). The projects stay in `tests/mtp/`, whose `global.json` runs `dotnet test` in MTP mode. 📌 Runner-agnostic discovery/execution code lives in `src/shared/specs.BDD.Runner/` and is compiled into both runners (it is not a project). 📌 Shared build settings, package versions, and the SDK pin live in root-level `Directory.Build.props`, `Directory.Packages.props`, and `global.json`. CI/release workflows live in `.github/workflows/`.

### Deliverable Projects (📌 4)

| Project | Folder | Purpose |
|---------|--------|---------|
| **specs.core** | `src/specs.core/` | Framework-agnostic core: builder, immutable test case model, executor, adapter interfaces |
| **specs.BDD** (asm name) / `specs.specification` (folder) | `src/specs.specification/` | High-level BDD API built on specs.core (`Specifications` base class) |
| **specs.BDD.VSTestAdapter** | `src/specs.BDD.VSTestAdapter/` | VSTest adapter implementing `ITestDiscoverer` + `ITestExecutor` |
| 📌 NEW **specs.BDD.TestingPlatform** | `src/specs.BDD.TestingPlatform/` | Microsoft.Testing.Platform test framework (`AddSpecsBDD()`); full spec tree as test nodes, `--treenode-filter`, console tree report |

### Test Projects (4 + 📌 2 MTP)

| Project | Folder | Tests |
|---------|--------|-------|
| **specs.core.UnitTests** | 📌 `tests/specs.core.UnitTests/` | 📌 333 core framework test results — **written in Specs itself** and run by `specs.BDD.VSTestAdapter` (NFluent + Autofac.Extras.Moq); one suite runs in grouped mode |
| **specs.BDD.UnitTests** | 📌 `tests/specs.BDD.UnitTests/` | 📌 147 BDD API spec results — **written in Specs itself** (was 5 NUnit wrappers over nspec) |
| **specs.BDD.VSTestAdapter.Tests** | 📌 `tests/specs.BDD.VSTestAdapter.Tests/` | 📌 273 results — adapter component unit tests (NUnit + Moq) plus 8 pending fixture specs reported as Skipped |
| **specs.BDD.IntegrationTests** 📌 RENAMED (was `specs.BDD.VSTestAdapter.IntegrationTests`) | 📌 `tests/specs.BDD.IntegrationTests/` | 📌 34 end-to-end VSTest pipeline results (NFluent), 5 of them pending/Skipped; `CalculatorSpecs` runs per spec except its `Subtraction (grouped)` describe |
| 📌 NEW **specs.BDD.TestingPlatform.Tests** | `tests/mtp/specs.BDD.TestingPlatform.Tests/` | 69 MTP runner tests (NUnit on MTP + Moq), including end-to-end runs of the MTP integration app |
| 📌 NEW **specs.BDD.TestingPlatform.IntegrationTests** | `tests/mtp/specs.BDD.TestingPlatform.IntegrationTests/` | MTP test app compiling the `specs.BDD.IntegrationTests` specs; 34 results (5 Skipped), matching VSTest |

> 📌 REMOVED: the internal-only `TestHarness` project (nspec bridge). See `systemPatterns.md` decision #33.

> 📌 REMOVED: the legacy `Specs` stub project (`src/Specs.csproj`, `Spec` class, `Domain/` layer) and the `Examples/ConsoleExample` sample. See `systemPatterns.md` decision #31.

## Authoring API

The API follows a single hierarchical paradigm — Suite → Feature Set → Scenario → Specification.

### `Specifications` (specs.BDD)

```csharp
public class CalculatorSpecs : Specifications      // Suite
{
    private Calculator _calc = null!;
    private int _result;

    [Describe("Addition")]                          // Feature set
    public void addition()
    {
        BeforeEach(() => _calc = new Calculator());
        Act(() => _result = _calc.Add(2, 3));
        It["returns the sum"] = () => Check.That(_result).IsEqualTo(5);
    }
}
```

## Hierarchy Vocabulary

| Term | Code element |
|------|--------------|
| **Suite** | Class inheriting `Specifications` (one subject) |
| **Feature Set** | `[Describe("...")]`-decorated method (one feature of the subject) |
| **Scenario** | `Context[...]` / `Describe[...]` block (when/given) |
| **Specification** | `It[...]` (single assertion) |
| 📌 **Pending specification** | `It[...] = TODO`, `xIt[...]`, or any spec inside `xContext[...]` / `xDescribe[...]` (discovered, never executed) |

## Target Audience

- .NET developers writing readable BDD tests.
- Teams needing nested contexts with inherited setup/teardown.
- Async-heavy codebases wanting first-class `async` support across the lifecycle.
- Teams migrating from NSpec-style or older test code to a modern fluent API.

## Technology Stack

- **Deliverables**: .NET Standard 2.1, C# 10 📌, nullable enabled.
- **Tests**: net10.0 📌 UPDATED. 📌 `specs.core.UnitTests`, `specs.BDD.UnitTests`, and the integration tests run on **Specs itself**; only the adapter test project uses NUnit 3.x. NFluent 3.1 for assertions, Autofac.Extras.Moq 7.0 in core tests, Moq 4.20 in adapter tests.
- **Adapter dependencies**: Microsoft.TestPlatform.ObjectModel 17.8.0, System.Reflection.Metadata 6.0.0.
- 📌 NEW — **MTP runner dependencies**: Microsoft.Testing.Platform 2.5.1 + Microsoft.Testing.Platform.MSBuild 2.5.1 (flowed to consumers), System.Reflection.Metadata 6.0.0.
- 📌 NEW — **Build & release**: .NET SDK 10.0.400 (`global.json`), central package management, MinVer (git-tag versioning, build-time only), GitHub Actions CI + tag-triggered release to nuget.org.

## Scope

**In scope**:

- Hierarchical test construction, lifecycle, async support.
- BDD authoring API with async support.
- VSTest discovery + execution + source location resolution + Test Explorer tree nesting + 📌 `dotnet test --filter` support.
- 📌 NEW — **Microsoft.Testing.Platform runner**: discovery + execution as a nested node tree (namespace → class → `[Describe]` → contexts → specs), `--treenode-filter` on tree paths, an indented console tree report, and `SpecsBDD:*` settings in `testconfig.json` / environment variables.
- Correct once-per-context `BeforeAll` / `AfterAll` for arbitrary test selections.
- 📌 Opt-in **grouped execution** — a root `[Describe]` runs as one test result with an NSpec-style tree report in its output, selected via the 📌 `TestStructure` enum (`PerSpec` / `PerDescribe`) at method (`[Describe(Structure = ...)]`), class or assembly (`[TestStructure(...)]`), or `.runsettings` level.
- 📌 Class-level (constructor-registered) `BeforeAll` / `AfterAll` running once per suite.
- 📌 **Pending specs** — `TODO` / `TODOAsync` placeholders and `xIt` / `xItAsync` / `xContext` / `xDescribe` exclusions, reported as Skipped and filterable via `IsPending`.
- 📌 **Expected-exception specs** — `It[...] = ShouldThrow<T>()` / `ShouldThrow<T>(verify)` assert on the exception thrown by `Act`; an unverified `Act` exception fails only its own spec.
- Adapter pattern for swappable test-framework integration.
- 📌 NEW — **NuGet distribution**: all 📌 four deliverables are packed with shared metadata, XML docs, and `.snupkg` symbols. They are versioned from `v*` git tags and published together to nuget.org by a GitHub Actions release workflow.

**Out of scope**:

- Custom test runner / GUI (relies on VSTest and 📌 Microsoft.Testing.Platform hosts).
- A dedicated assertion library (NFluent / FluentAssertions are used by callers).
- Test data generation, mocking, snapshot testing.

## Key Constraints

1. Deliverables must remain `netstandard2.1`-compatible.
2. `specs.core` must have **zero** third-party package references at runtime (📌 build-time-only `PrivateAssets=all` tools such as MinVer are allowed).
3. ~~Backward compatibility with the legacy `Spec` API~~ — 📌 dropped; the legacy API was removed.
4. `Specifications` is the single supported authoring API.
5. Discovery must be safe (cannot execute arbitrary spec code during scan).
6. Discovery and execution must build the suite tree through the same code path (`SuiteTreeBuilder`).
7. 📌 NEW — Both runners must share one execution core (`src/shared/specs.BDD.Runner/`) so lifecycle, pending, grouped, and `ShouldThrow` semantics never diverge.

## Success Criteria

- ✅ Test Explorer discovery and execution work for any `Specifications`-derived class with `[Describe]` methods.
- ✅ Test Explorer renders the full `Class → [Describe] → Describe → It` hierarchy.
- ✅ Source location ("Go to source") navigates to the `It[...]` line for every discovered test.
- ✅ `BeforeAll` / `AfterAll` fire exactly once per context, even when only part of a suite is run.
- ✅ Sync and async hooks produce identical lifecycle semantics.
- ✅ Execution is consolidated through `TestCaseExecutor` (no duplicated execution logic in adapter) — 📌 and the adapter reaches it through the same public API available to third-party runners.
- ✅ **The framework tests its own core with itself** — 📌 333 self-hosted core results and 147 self-hosted BDD results, plus 273 adapter and 34 integration results.
- ✅ 📌 NEW — The same integration specs give identical totals under VSTest and Microsoft.Testing.Platform (34 results, 29 passed, 5 pending).
- ✅ 📌 NEW — Under MTP, IDEs receive the full `namespace → class → [Describe] → context → spec` tree, and `--treenode-filter` selects any subtree by path.
- ✅ A root `[Describe]` can opt into grouped execution and still honour the full once-per-node lifecycle.
- ✅ 📌 Constructor-registered `BeforeAll` / `AfterAll` fire exactly once per suite class in both per-`It` and grouped modes.
- ✅ 📌 Pending specs are discovered and reported as Skipped without running any lifecycle hook (including the `BeforeAll` / `AfterAll` of contexts whose selected specs are all pending).
- ✅ 📌 An exception thrown by `Act` is verifiable from `It` via `ShouldThrow<T>`, and otherwise fails only that spec while `AfterEach` / `AfterAll` still run — identically in `TestCaseExecutor`, per-`It`, and grouped execution.
