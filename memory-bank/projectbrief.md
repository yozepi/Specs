# Project Brief: Specs Testing Framework

## Project Identity

| Property | Value |
|----------|-------|
| **Name** | Specs |
| **Type** | .NET BDD testing framework + VSTest adapter |
| **Core Target** | .NET Standard 2.1 (deliverables) / net8.0 (tests) |
| **Language** | C# 8.0 with nullable reference types |
| **Repository** | <https://github.com/yozepi/Specs> |

## Core Purpose

Specs is a .NET testing framework that provides **two complementary BDD-style APIs** sitting on top of a framework-agnostic core, with a **VSTest adapter** that makes specs discoverable and runnable from Visual Studio Test Explorer, `dotnet test`, and any IDE that speaks the VSTest protocol.

## Primary Goalsl

1. **Structured BDD authoring** — intuitive `Describe` / `Context` / `It` indexers and `[Context]` methods.
2. **Hierarchical organization** — nested contexts inherit setup/act behavior from their parents.
3. **Sophisticated lifecycle** — `SetupOnce`, `SetupEach`, `Act`, `TeardownEach`, `TeardownOnce` with correct ordering.
4. **First-class async** — every lifecycle hook has a `Func<Task>` overload.
5. **Two coexisting API styles** — modern method-based `Specifications` and legacy property-based `Spec`.
6. **Framework-agnostic core** — the core has zero test-framework dependencies; everything plugs in through adapters.
7. **Native IDE integration** — VSTest adapter delivers Test Explorer discovery and execution with source navigation.

## Solution Structure

The solution contains **9 projects** plus a separate example.

### Deliverable Projects (3)

| Project | Folder | Purpose |
|---------|--------|---------|
| **specs.core** | `src/specs.core/` | Framework-agnostic core: builder, immutable test case model, executor, adapter interfaces |
| **specs.BDD** (asm name) / `specs.specification` (folder) | `src/specs.specification/` | High-level BDD API built on specs.core (`Specifications` base class) |
| **specs.BDD.VSTestAdapter** | `src/specs.BDD.VSTestAdapter/` | VSTest adapter implementing `ITestDiscoverer` + `ITestExecutor` |

### Test Projects (4)

| Project | Folder | Tests |
|---------|--------|-------|
| **specs.core.UnitTests** | `src/specs.core.UnitTests/` | 289 core framework test results — **written in Specs itself** and run by `specs.BDD.VSTestAdapter` (NFluent + Autofac.Extras.Moq); one suite runs in grouped mode |
| **specs.BDD.UnitTests** | `src/specs.specification.UnitTests/` | BDD API unit tests (NUnit) |
| **specs.BDD.VSTestAdapter.Tests** | `src/specs.BDD.VSTestAdapter.Tests/` | 149 adapter component unit tests (NUnit) |
| **specs.BDD.VSTestAdapter.IntegrationTests** | `src/specs.BDD.VSTestAdapter.IntegrationTests/` | 28 end-to-end VSTest pipeline tests (NFluent) |

### Legacy / Internal (2)

| Project | Folder | Purpose |
|---------|--------|---------|
| **Specs** (legacy) | `src/Specs.csproj` | Stub project holding the legacy `Spec` class and `Domain/` layer |
| **TestHarness** | `src/TestHarness/` | **Internal-only** nspec bridge from the era before the framework could test itself; not a deliverable, now a retirement candidate |

### Example (separate folder)

- **ConsoleExample** — `Examples/ConsoleExample/` — net6.0 sample using the legacy `Spec` API with `FluentAssertions`.

## Two Testing Approaches (Same Paradigm)

Both APIs target the **same hierarchical paradigm** — Suite → Feature Set → Scenario → Specification — and differ only in syntax.

### Modern: `Specifications` (specs.BDD)

```csharp
public class CalculatorSpecs : Specifications      // Suite
{
    private Calculator _calc = null!;
    private int _result;

    [Context("Addition")]                          // Feature set
    public void addition()
    {
        BeforeEach(() => _calc = new Calculator());
        Act(() => _result = _calc.Add(2, 3));
        It["returns the sum"] = () => Check.That(_result).IsEqualTo(5);
    }
}
```

### Legacy: `Spec` (property-based)

```csharp
public class CalculatorSpecs : Spec
{
    public void TryIt()
    {
        Calculator calc = null!;
        int result = 0;
        BeforeEach = () => calc = new Calculator();
        Act         = () => result = calc.Add(2, 3);
        It["returns the sum"] = () => result.Should().Be(5);
    }
}
```

## Hierarchy Vocabulary

| Term | Code element |
|------|--------------|
| **Suite** | Class inheriting `Specifications` or `Spec` (one subject) |
| **Feature Set** | `[Context("...")]`-decorated method (one feature of the subject) |
| **Scenario** | `Context[...]` / `Describe[...]` block (when/given) |
| **Specification** | `It[...]` (single assertion) |

## Target Audience

- .NET developers writing readable BDD tests.
- Teams needing nested contexts with inherited setup/teardown.
- Async-heavy codebases wanting first-class `async` support across the lifecycle.
- Teams migrating from older property-based test code to a modern fluent API.

## Technology Stack

- **Deliverables**: .NET Standard 2.1, C# 8.0, nullable enabled.
- **Tests**: net8.0. `specs.core.UnitTests` runs on **Specs itself**; the BDD and adapter test projects use NUnit 3.x. NFluent 3.x for assertions, Autofac.Extras.Moq 7.0 in core tests only.
- **Adapter dependencies**: Microsoft.TestPlatform.ObjectModel 17.8.0, System.Reflection.Metadata 6.0.0.
- **Legacy bridge**: nspec 3.1.0 (TestHarness only — not shipped).

## Scope

**In scope**:

- Hierarchical test construction, lifecycle, async support.
- Two coexisting BDD API styles.
- VSTest discovery + execution + source location resolution + Test Explorer tree nesting.
- Correct once-per-context `BeforeAll` / `AfterAll` for arbitrary test selections.
- 📌 Opt-in **grouped execution** — a root `[Context]` runs as one test result with an NSpec-style tree report in its output (`[Context(Grouped = true)]`, `[assembly: GroupSpecsByRootContext]`, or `.runsettings`).
- Adapter pattern for swappable test-framework integration.

**Out of scope**:

- Custom test runner / GUI (relies on VSTest hosts).
- A dedicated assertion library (NFluent / FluentAssertions are used by callers).
- Test data generation, mocking, snapshot testing.

## Key Constraints

1. Deliverables must remain `netstandard2.1`-compatible.
2. `specs.core` must have **zero** third-party package references.
3. Backward compatibility with the legacy `Spec` API must be preserved.
4. Both API styles must continue to coexist.
5. Discovery must be safe (cannot execute arbitrary spec code during scan).
6. Discovery and execution must build the suite tree through the same code path (`SuiteTreeBuilder`).

## Success Criteria

- ✅ Test Explorer discovery and execution work for any `Specifications`-derived class with `[Context]` methods.
- ✅ Test Explorer renders the full `Class → [Context] → Describe → It` hierarchy.
- ✅ Source location ("Go to source") navigates to the `It[...]` line for every discovered test.
- ✅ `BeforeAll` / `AfterAll` fire exactly once per context, even when only part of a suite is run.
- ✅ Both API styles produce identical lifecycle semantics.
- ✅ Execution is consolidated through `TestCaseExecutor` (no duplicated execution logic in adapter).
- ✅ **The framework tests its own core with itself** — 289 self-hosted test results, plus 149 adapter and 28 integration tests.
- ✅ A root `[Context]` can opt into grouped execution and still honour the full once-per-node lifecycle.
