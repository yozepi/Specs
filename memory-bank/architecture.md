# Architecture: Specs Framework

## Architectural Overview

The Specs framework organizes tests in a **Suite → Feature Set → Specification** hierarchy:

- **Suite** = A specification class (one per subject under test)
- **Feature Set** = A `[Feature]` decorated method (one per feature/aspect)  
- **Specification** = An `It[...]` assertion (one per expected behavior)

The `[Feature]` attribute and `Context[...]` indexer are NOT separate paradigms — they work together at different levels of the same hierarchy. `[Feature]` marks methods as feature sets within a suite, while `Context[...]`/`Describe[...]` organize scenarios within those feature sets.

The framework has ONE API style (📌 the legacy property-based `Spec` API and `Domain/` layer were removed; see `systemPatterns.md` decision #31):

1. **Method-Based API** (specs.core + specs.BDD)
   - `Specifications` base class with method-based lifecycle (`BeforeAll()`, `BeforeEach()`)
   - Builder-based approach, framework-agnostic through adapters
   - Immutable test case models

The API follows the Suite → Feature Set → Specification hierarchy.

## Design Patterns

### 1. Builder Pattern
The framework extensively uses the Builder pattern for constructing test cases:
- **IContextualTestCaseBuilder**: Interface defining the fluent API
- **ContextualTestCaseBuilder**: Concrete builder implementation
- **Benefits**: Provides a clean, fluent API for complex object construction

### 2. Composite Pattern
Hierarchical test organization uses the Composite pattern:
- Parent contexts can contain child contexts
- Each context maintains its own lifecycle hooks
- Actions are collected and executed in hierarchical order
- **Benefits**: Enables nested test contexts with inherited behavior

### 3. Adapter Pattern
Framework adapters provide abstraction over external dependencies:
- **ITestProviderAdapter**: Abstracts test instance retrieval
- **ITestContextAdapter**: Abstracts test lifecycle execution
- **ILoggingAdapter**: Abstracts logging operations
- **Benefits**: Decouples core logic from specific implementations

### 4. Strategy Pattern (Implicit)
The framework allows different execution strategies through:
- **TestCaseExecutorSettings**: Configurable execution behavior (e.g., StopOnAssertFailure)
- **IFormatter**: Pluggable output formatting

## Architectural Layers

### Layer 1: Adapters (Framework Integration)
```
Framework/Adapters/
├── ITestProviderAdapter.cs      - Test instance retrieval
├── ITestContextAdapter.cs       - Lifecycle execution
├── TestContextAdapter.cs        - Default adapter implementation
├── ILoggingAdapter.cs           - Logging abstraction
└── TestInstance.cs              - Test instance model
```

**Responsibility**: Provide abstraction over external test frameworks and dependencies.

### Layer 2: Test Case Models
```
Framework/Processors/TestCases/
├── ITestCase.cs                 - Core test case interface
├── IContextualTestCase.cs       - Contextual test case interface
├── ITestCaseContext.cs          - Test context interface
├── ContextualTestCase.cs        - Concrete implementation
├── IContextualTestCaseBuilder.cs - Builder interface
└── ContextualTestCaseBuilder.cs  - Builder implementation
```

**Responsibility**: Define and implement test case structures with lifecycle support.

### Layer 3: Test Execution
```
Framework/Processors/TestExecutors/
├── TestCaseExecutor.cs          - Orchestrates test execution
└── TestCaseExecutorSettings.cs  - Execution configuration
```

**Responsibility**: Execute test cases following the defined lifecycle.

### Layer 4: Settings & Configuration
```
Framework/Settings/
└── TestSettings.cs              - Global test settings
```

**Responsibility**: Manage framework-wide configuration.

### Layer 5: Utilities
```
Framework/ExceptionExtensions/
└── ExceptionExtensions.cs       - Exception handling utilities
```

**Responsibility**: Provide common utilities and helpers.

## Test Lifecycle Flow

```
1. SetupOnce (Parent → Child hierarchy)
   └─ Run once before all tests in context

2. For each test:
   ├─ SetupEach (Parent → Child hierarchy)
   │  └─ Run before each test
   │
   ├─ Act (Parent → Child hierarchy)
   │  └─ Perform the action being tested
   │
   ├─ Test Assertion
   │  └─ Execute the actual test
   │
   └─ TeardownEach (Child → Parent hierarchy)
      └─ Run after each test

3. TeardownOnce (Child → Parent hierarchy)
   └─ Run once after all tests in context
```

**Key Points**:
- Setup actions flow from parent to child (inheritance)
- Teardown actions flow from child to parent (cleanup order)
- Act phase aggregates all parent and self actions
- Each test inherits the full context chain

## Action Collection Strategy

### Setup/Act (Top-Down Inheritance)
```csharp
private IEnumerable<Action> CollectAncestorAndSelfActions()
{
    // First collect parent actions...
    if (parentContext != null)
    {
        foreach (var action in parentContext.CollectActions())
            yield return action;
    }
    
    // ...then add self actions
    foreach (var action in selfActions)
        yield return action;
}
```

### Teardown (Bottom-Up Cleanup)
```csharp
private IEnumerable<Action> CollectAncestorAndSelfTeardownActions()
{
    // First process self actions...
    foreach (var action in selfActions)
        yield return action;
    
    // ...then parent actions
    if (parentContext != null)
    {
        foreach (var action in parentContext.CollectActions())
            yield return action;
    }
}
```

## Async/Sync Abstraction

The framework provides a unified interface for both synchronous and asynchronous operations:

```csharp
// Synchronous
builder.AddSetupOnce(action: () => { /* sync code */ });

// Asynchronous
builder.AddSetupOnce(action: async () => { await /* async code */ });
```

**Implementation**: Async methods are converted to synchronous using `GetAwaiter().GetResult()` internally. This allows the framework to maintain a simple Action-based interface while supporting modern async patterns.

## Data Flow

### Approach 1: Core Framework Architecture (specs.core + specs.BDD)

```
User Code (Test Specification inheriting from Specifications)
    ↓
specs.BDD Layer (Method-based DSL: BeforeAll/BeforeEach/Act/It)
    ↓
IContextualTestCaseBuilder (Fluent API)
    ↓
ContextualTestCaseBuilder (Accumulates actions)
    ↓
Build() → IContextualTestCase (Immutable test case)
    ↓
TestCaseExecutor (Executes lifecycle)
    ↓
ITestContextAdapter (Framework integration)
    ↓
Actual Test Framework (NSpec, etc.)
```

### Approach 3: VSTest Adapter Architecture (specs.BDD.VSTestAdapter)

The VSTest adapter bridges specs.BDD specifications to the standard .NET test infrastructure
(`dotnet test`, Visual Studio Test Explorer, Rider, VS Code).

```
dotnet test / VS Test Explorer
    ↓ VSTest Protocol (ITestDiscoverer / ITestExecutor)
specs.BDD.VSTestAdapter
    ↓ Discovery
    ├─ SpecificationClassScanner  → finds Specifications subclasses
    ├─ ContextMethodScanner       → finds [Feature]-decorated methods, validates presence
    ├─ SuiteTreeBuilder           → registers each [Feature] method as a named child context,
    │                               reads Specifications.CoreBuilder (no reflection),
    │                               calls Build() → IContextualTestCase
    ├─ TestStructureExtractor     → recursively flattens the tree → SpecTest list
    ├─ SourceLocationReader       → portable PDB lookup for the It[...] lambda
    └─ TestCaseConverter          → converts SpecTest → VSTest TestCase (FQN + metadata
                                    + RFC-0033 Hierarchy for Test Explorer nesting)
    ↓ Execution
    ├─ SpecsTestExecutor          → groups selected tests by (Source, ClassName)
    ├─ TestInvoker                → resolves the type, rebuilds the suite via SuiteTreeBuilder
    ├─ SelectionTreeWalker        → depth-first walk firing OwnSetupOnce / OwnTeardownOnce
    │                               exactly once per live node; delegates per-test loops
    │                               to specs.core's TestCaseExecutor
    ├─ TestFinder                 → navigates IContextualTestCase hierarchy by context path
    └─ ResultReporter             → creates Passed/Failed TestResult with timing

Key constraint: every Specifications class MUST have at least one [Feature] method.
  → Discovery throws InvalidSpecificationException otherwise.
```

**FQN format**: `{ClassName}::{MethodName}::{ContextPath}::{TestDescription}`  
**Display name format**: `{MethodName} → {ContextPath} → {TestDescription}` (` → ` = space+U+2192+space)  
**Context path format**: `root context → child context → …`

### 📌 REMOVED — Legacy Runtime Architecture (Domain + Spec)

The `Spec` class, `IRunContext` runtime switching, and the `ActionRunner` / `TaskRunner` Domain layer were deleted. Only the builder-based architecture above remains.

### specs.BDD Context Switching Pattern

The `Specifications` class uses a callback pattern to manage context switching:

```csharp
// When entering a child context (Describe/Context):
private void contextCallBack(IContextualTestCaseBuilder childCoreBuilder, Action action)
{
    var previousCoreBuilder = this.coreBuilder;
    this.coreBuilder = childCoreBuilder;  // Switch to child builder
    try
    {
        action.Invoke();  // Execute user's context definition
    }
    finally
    {
        this.coreBuilder = previousCoreBuilder;  // Restore parent builder
    }
}
```

This enables nested context definitions where `BeforeAll`, `BeforeEach`, `Act`, `It`, etc. automatically target the correct builder scope.

## Key Design Decisions

### Core Framework (specs.core + specs.BDD)

#### 1. Immutability After Build
Once `Build()` is called, the test case becomes immutable. All modifications must be done through the builder.

#### 2. Action Aggregation
Actions are collected at build time, not execution time, allowing for optimization and validation.

#### 3. Hierarchical Contexts
Child contexts inherit and extend parent behavior, enabling powerful test organization patterns.

#### 4. Separation of Concerns
Clear separation between:
- Test definition (Builder)
- Test representation (TestCase)
- Test execution (Executor)
- Framework integration (Adapters)

#### 5. Framework Agnostic Core
The core is independent of specific test frameworks, with integration provided through adapters.

## Migration Strategy

Code written against the removed `Spec` class migrates to `Specifications` by:
1. Change base class from `Spec` to `Specifications`
2. Convert property assignments to method calls:
   - `BeforeEach = () => {}` → `BeforeEach(() => {})`
   - `Act = () => {}` → `Act(() => {})`
3. Move definitions into `[Feature("...")]`-decorated methods
4. Replace `ShouldThrow<T>()` helper calls with `It["..."] = ShouldThrow<T>()` (or an assertion library)
