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

## Solution Structure

### Deliverables (3 projects)
- **specs.core** - Framework-agnostic core: builder, immutable test-case model, executor (.NET Standard 2.1)
- **specs.BDD** (`specs.specification`) - High-level BDD authoring API built on specs.core
- **specs.BDD.VSTestAdapter** - VSTest discovery + execution for Test Explorer and `dotnet test`

### Test projects (4)
- **specs.core.UnitTests** - Core framework tests, written in Specs itself
- **specs.BDD.UnitTests** - BDD API tests
- **specs.BDD.VSTestAdapter.Tests** - Adapter component tests
- **specs.BDD.VSTestAdapter.IntegrationTests** - End-to-end VSTest pipeline tests

### Legacy / internal (2)
- **Specs** - Legacy property-based `Spec` class and `Domain/` layer
- **TestHarness** - Internal-only nspec bridge; not a deliverable

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
    [Context("My feature")]
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

> A `[Context]`-decorated method is required for the VSTest adapter to discover tests in a suite.

## Key Features
- Hierarchical test organization with parent-child contexts
- Five-phase lifecycle (SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce)
- `BeforeAll` / `AfterAll` fire exactly once per context, even for partial test runs
- Full async/await support on every lifecycle hook
- Fluent API for test construction
- Framework-agnostic core with adapter pattern (zero external dependencies)
- Test Explorer integration with hierarchical nesting and source navigation
- Two coexisting API styles for gradual migration

## Technology Stack
- .NET Standard 2.1 (deliverables), net8.0 (tests)
- Microsoft.TestPlatform.ObjectModel (adapter only)
- NUnit, NFluent (tests)
- nspec (internal TestHarness only — not shipped)

## License
See [LICENSE](LICENSE) file for details.
