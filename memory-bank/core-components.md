# Core Components: Specs Framework

## Component Overview

This document details the key components of the Specs framework, their responsibilities, and interactions.

---

## specs.BDD Layer (DSL Components)

The specs.BDD layer provides a user-friendly Domain Specific Language (DSL) for writing behavior-driven tests. This layer wraps the core `IContextualTestCaseBuilder` to provide an intuitive API.

### Test Hierarchy

The specs.BDD layer implements a **Suite → Feature Set → Specification** hierarchy:

1. **Suite** = A class inheriting from `Specifications` (one per subject under test)
2. **Feature Set** = A method decorated with `[Context]` (one per feature/aspect of the subject)
3. **Scenarios** = `Context[...]`/`Describe[...]` indexers (organize specs within a feature set)
4. **Specifications** = `It[...]` indexer (individual test assertions)

The `[Context]` attribute and `Context[...]` indexer are part of the same paradigm at different hierarchy levels — NOT separate approaches.

**Lifecycle operates at two levels**:
- **Suite-level**: Lifecycle methods called at the class level apply to ALL feature sets
- **Feature-level**: Lifecycle methods called inside a `[Context]` method apply only to that feature set's specs

### ContextAttribute

**Location**: `specs.specification/Attributes/ContextAttribute.cs`

**Purpose**: Marks a method as a specification context builder (feature set) within a suite. Each `[Context]` method represents one feature or aspect of the subject under test.

**Properties**:
- `Description` (string?, read-only) - Optional description for the feature set, used for reporting

**Usage**:
```csharp
public class Weather_Controller : Specifications  // SUITE
{
    // Suite-level lifecycle (applies to ALL feature sets)
    BeforeAll(() => { /* once for entire suite */ });

    [Context("Getting forecasts")]  // FEATURE SET
    public void getting_forecasts()
    {
        // Feature-level lifecycle (applies only to THIS feature set)
        BeforeEach(() => { /* per-spec setup for this feature */ });
        
        Context["when requesting today"] = () =>  // SCENARIO
        {
            It["should return data"] = () => { /* SPEC */ };
        };
    }

    [Context("Updating data")]  // ANOTHER FEATURE SET
    public void updating_data() { /* ... */ }
}
```

**Key Characteristics**:
- `[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]`
- Only one `[Context]` attribute per method
- Inherited by derived classes
- Description is optional (defaults to null)

### Specifications (Base Class)

**Location**: `specs.specification/Specifications.cs`

**Purpose**: Abstract base class for all test suites, providing the BDD-style API. Each class inheriting from `Specifications` represents a suite for one subject under test.

**Lifecycle Methods** (available at both suite and feature levels):
- `BeforeAll(Action action)` / `BeforeAll(Func<Task> taskFunction)` - Setup once before all tests in scope
- `BeforeEach(Action action)` / `BeforeEach(Func<Task> taskFunction)` - Setup before each test in scope
- `Act(Action action)` / `Act(Func<Task> taskFunction)` - Action under test
- `AfterEach(Action action)` / `AfterEach(Func<Task> taskFunction)` - Teardown after each test in scope
- `AfterAll(Action action)` / `AfterAll(Func<Task> taskFunction)` - Teardown once after all tests in scope

**DSL Properties** (for organizing scenarios and specs within feature sets):
- `Describe` - Define what is being tested (returns `ContextBuilder`)
- `Context` - Define when/given scenarios (returns `ContextBuilder`)
- `It` - Define test assertions (returns `AssertBuilder`)
- `ItAsync` - Define async test assertions (returns `AssertBuilderAsync`)

**Context Switching**: Uses a callback pattern to temporarily swap the internal `coreBuilder` when entering child contexts via `Context[...]`/`Describe[...]`.

### ContextBuilder

**Location**: `specs.specification/ContextBuilder.cs`

**Purpose**: Provides indexer syntax for defining test contexts using Describe/Context pattern.

**Key Features**:
- Uses a callback delegate (`ContextCallBack`) for context switching
- Enables nested context definitions with proper scope management
- Validates input using `ThrowIfNull`

**Usage**:
```csharp
Describe["Calculator"] = () =>
{
    Context["when adding numbers"] = () =>
    {
        It["should return correct sum"] = () => { /* assertion */ };
    };
};
```

### AssertBuilder

**Location**: `specs.specification/AssertBuilder.cs`

**Purpose**: Provides indexer syntax for defining synchronous test assertions.

**Usage**:
```csharp
It["should do something"] = () => { /* test assertion */ };
```

### AssertBuilderAsync

**Location**: `specs.specification/AssertBuilderAsync.cs`

**Purpose**: Provides indexer syntax for defining asynchronous test assertions.

**Usage**:
```csharp
ItAsync["should do something async"] = async () => { await /* async test */ };
```

### IAssertProvider<T>

**Location**: `specs.specification/IAssertProvider.cs`

**Purpose**: Generic interface for assertion builders.

**Definition**:
```csharp
public interface IAssertProvider<T> where T : Delegate
{
    T this[string description] { set; }
}
```

---

## 1. IContextualTestCaseBuilder

**Location**: `Framework/Processors/TestCases/IContextualTestCaseBuilder.cs`

**Purpose**: Defines the fluent API for building hierarchical test cases with lifecycle hooks.

**Key Methods**:

### Lifecycle Hook Methods
- `AddSetupOnce(Action action)` / `AddSetupOnce(Func<Task> action)`
  - Registers actions to run once before all tests
  - Supports both sync and async operations

- `AddSetupEach(Action action)` / `AddSetupEach(Func<Task> action)`
  - Registers actions to run before each test
  - Supports both sync and async operations

- `AddAct(Action action)` / `AddAct(Func<Task> action)`
  - Registers the action being tested
  - Runs once before each test
  - Supports both sync and async operations

- `AddTeardownEach(Action action)` / `AddTeardownEach(Func<Task> action)`
  - Registers actions to run after each test
  - Supports both sync and async operations

- `AddTeardownOnce(Action action)` / `AddTeardownOnce(Func<Task> action)`
  - Registers actions to run once after all tests
  - Supports both sync and async operations

### Test Methods
- `AddTest(string description, Action test)` / `AddTest(string description, Func<Task> test)`
  - Adds a test with description
  - Supports both sync and async test execution

### Hierarchy Methods
- `ChildTestCase(string description, Action<IContextualTestCaseBuilder> configure)`
  - Creates nested test contexts
  - Child contexts inherit parent behavior
  - Enables hierarchical test organization

### Builder Methods
- `Build()` → `IContextualTestCase`
  - Finalizes the builder and creates an immutable test case
  - Collects all actions from parent hierarchy

**Usage Pattern**:
```csharp
var builder = new ContextualTestCaseBuilder()
    .AddSetupOnce(() => InitializeDatabase())
    .AddSetupEach(() => CreateTestData())
    .AddAct(() => ExecuteOperation())
    .AddTest("should succeed", () => AssertSuccess())
    .AddTeardownEach(() => CleanupTestData())
    .AddTeardownOnce(() => CloseDatabase());

var testCase = builder.Build();
```

---

## 2. ContextualTestCaseBuilder

**Location**: `Framework/Processors/TestCases/ContextualTestCaseBuilder.cs`

**Purpose**: Concrete implementation of the builder with support for hierarchical contexts.

**Key Internal Components**:

### Private Fields
- `parentContext`: Reference to parent builder for hierarchy
- `childBuilders`: List of child context builders
- `setupOnceBuilder`: RunAllBuilder for setup-once actions
- `setupEachActions`: List of setup-each actions
- `actActions`: List of act actions
- `testActions`: List of test descriptions and actions
- `teardownEachActions`: List of teardown-each actions
- `teardownOnceBuilder`: RunAllBuilder for teardown-once actions

### Action Collection Methods
- `CollectAncestorAndSelfSetupOnceActions()`: Top-down collection
- `CollectAncestorAndSelfSetupEachActions()`: Top-down collection
- `CollectAncestorAndSelfActActions()`: Top-down collection
- `CollectAncestorAndSelfTeardownEachActions()`: Bottom-up collection
- `CollectAncestorAndSelfTeardownOnceActions()`: Bottom-up collection

### Key Behaviors
1. **Hierarchical Action Collection**: Actions are collected from parent contexts
2. **Order Preservation**: Setup actions flow parent→child, teardown actions flow child→parent
3. **Async Wrapping**: Async methods are wrapped to maintain Action-based interface
4. **Child Context Management**: Children are built as `ITestCaseContext` instances

---

## 3. ITestCase

**Location**: `Framework/Processors/TestCases/ITestCase.cs`

**Purpose**: Core interface representing a built test case.

**Properties**:
- `Action SetupOnce`: Aggregated setup-once action
- `Action SetupEach`: Aggregated setup-each action
- `Action Act`: Aggregated act action
- `Action TeardownEach`: Aggregated teardown-each action
- `Action TeardownOnce`: Aggregated teardown-once action
- `IReadOnlyList<(string description, Action test)> Tests`: List of tests to execute

**Key Characteristics**:
- Immutable after creation
- All actions are composed and ready to execute
- No builder methods, only data properties

---

## 4. IContextualTestCase

**Location**: `Framework/Processors/TestCases/IContextualTestCase.cs`

**Purpose**: Extension of ITestCase with support for child contexts.

**Additional Properties**:
- `IReadOnlyList<ITestCaseContext> ChildContexts`: Nested test contexts

**Purpose**: Enables hierarchical test organization where contexts can contain sub-contexts.

---

## 5. ITestCaseContext

**Location**: `Framework/Processors/TestCases/ITestCaseContext.cs`

**Purpose**: Represents a named context with its own lifecycle and tests.

**Properties**:
- `string Description`: Context name/description
- All properties from `IContextualTestCase`

**Usage**: Represents child contexts within a parent test case.

---

## 6. ContextualTestCase

**Location**: `Framework/Processors/TestCases/ContextualTestCase.cs`

**Purpose**: Concrete implementation of contextual test cases.

**Static Factory Methods**:
- `CreateTestCase(...)`: Creates an `IContextualTestCase` instance
- `CreateTestCaseContext(...)`: Creates an `ITestCaseContext` instance

**Implementation Note**: Uses static factory methods to enforce immutability and proper construction.

---

## 7. TestCaseExecutor

**Location**: `Framework/Processors/TestExecutors/TestCaseExecutor.cs`

**Purpose**: Orchestrates test execution following the defined lifecycle.

**Dependencies**:
- `TestCaseExecutorSettings`: Configuration for execution behavior
- `ILoggingAdapter`: Logging abstraction
- `ITestContextAdapter`: Test lifecycle adapter
- `ITestProviderAdapter`: Test instance provider

**Key Method**:
```csharp
public void ProcessTestCase()
```

**Execution Flow**:
1. Retrieve test instances from provider
2. Execute SetupOnce
3. For each test:
   - Execute SetupEach
   - Execute Act
   - Execute Test (with error handling)
   - Execute TeardownEach
4. Execute TeardownOnce
5. Log success or failure

**Error Handling**:
- Catches test failures and logs them
- Can stop execution on first failure (via StopOnAssertFailure setting)
- Ensures teardown executes even on failure

---

## 8. TestCaseExecutorSettings

**Location**: `Framework/Processors/TestExecutors/TestCaseExecutorSettings.cs`

**Purpose**: Configuration object for test execution.

**Properties**:
- `bool StopOnAssertFailure`: Controls whether to stop on first failure

---

## 9. Adapter Interfaces

### ITestProviderAdapter

**Purpose**: Provides test instances to execute
**Method**: `IReadOnlyCollection<TestInstance> GetTestInstances()`

### ITestContextAdapter

**Purpose**: Provides lifecycle hook execution
**Methods**:
- `void SetupOnce()`
- `void SetupEach()`
- `void Act()`
- `void TeardownEach()`
- `void TeardownOnce()`

### ILoggingAdapter

**Purpose**: Abstracts logging operations
**Methods**:
- `void LogAssertSuccess(string description)`
- `void LogAssertFailure(Exception ex, string description)`
- `void LogException(Exception ex)`

---

## 10. TestInstance

**Location**: `Framework/Adapters/TestInstance.cs`

**Purpose**: Represents a single test to execute.

**Properties**:
- `string Description`: Test description
- `Action Test`: Test action to execute

---

## 11. RunAllBuilder

**Location**: `Framework/Processors/TestCases/RunAllBuilder.cs`

**Purpose**: Helper for building composite actions from multiple actions.

**Usage**: Used internally for SetupOnce and TeardownOnce to aggregate multiple actions into a single action.

**Method**: `Action? Build()` - Returns null if no actions, otherwise returns a composite action.

---

## 12. nSpecTestHarness

**Location**: `TestHarness/nSpecTestHarness.cs`

**Purpose**: Base class for integrating Specs framework with NSpec and NUnit.

**Key Features**:
- **Tag Support**: Filter tests by tags
- **Focus Tags**: Run only focused tests
- **Formatter**: Configurable output formatting
- **Fail Fast**: Stop on first failure
- **NUnit Integration**: Asserts test results through NUnit

**Key Methods**:
- `LoadSpecs(Func<Type[]> specFinder)`: Load test specifications
- `RunSpecs()`: Execute loaded specs
- `RunSpecs(Func<Type[]> specFinder)`: Load and run in one call
- `RunMySpecs()`: Run specs from current test class

**Workflow**:
1. Find spec types
2. Build contexts (with tag filtering)
3. Run contexts through NSpec runner
4. Assert results through NUnit

---

## Component Interaction Diagram

```
User Test Code
    ↓
ContextualTestCaseBuilder (Build test structure)
    ↓
Build() → ContextualTestCase (Immutable representation)
    ↓
TestCaseExecutor (Execute lifecycle)
    ↓
├─→ ITestProviderAdapter (Get tests)
├─→ ITestContextAdapter (Run lifecycle hooks)
└─→ ILoggingAdapter (Log results)
    ↓
nSpecTestHarness (Integration layer)
    ↓
NSpec Runner → NUnit Assertions
```

---

## Summary

The framework is built on three core pillars:

1. **Builder Layer**: Fluent API for constructing hierarchical test cases
2. **Model Layer**: Immutable representations of test cases and contexts
3. **Execution Layer**: Orchestrated execution with proper lifecycle management

These components work together to provide a powerful, flexible testing framework that supports complex test hierarchies while maintaining a clean, intuitive API.
