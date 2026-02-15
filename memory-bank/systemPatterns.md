# System Patterns: Specs

## Core Architectural Pattern

The Specs framework uses a **Builder Pattern** with **Hierarchical Contexts** to construct immutable test cases with sophisticated lifecycle management.

```
┌─────────────────────────────────────────────┐
│ Test Specification (User Code)              │
│   ↓ Uses                                    │
│ Fluent Builder API                          │
│   ↓ Builds                                  │
│ Immutable Test Case Model                   │
│   ↓ Executed by                             │
│ Test Case Executor                          │
│   ↓ Integrates via                          │
│ Adapter Layer                               │
│   ↓ Runs on                                 │
│ Test Framework (NSpec, etc.)                │
└─────────────────────────────────────────────┘
```

## Key Design Patterns

### 1. Builder Pattern (Core Pattern)

**Purpose**: Construct complex test cases with fluent API

**Implementation**:
```csharp
IContextualTestCaseBuilder builder = new ContextualTestCaseBuilder();

builder
    .AddSetupOnce(() => { /* setup */ })
    .AddSetupEach(() => { /* setup */ })
    .AddAct(() => { /* action */ })
    .AddTeardownEach(() => { /* cleanup */ })
    .AddTeardownOnce(() => { /* cleanup */ });

IContextualTestCase testCase = builder.Build();  // Immutable
```

**Benefits**:
- Fluent, readable API
- Clear separation between building and execution
- Validation at build time
- Immutability after build

### 2. Composite Pattern (Hierarchical Contexts)

**Purpose**: Support nested test contexts with parent-child relationships

**Structure**:
```
Parent Context
├── Setup: Database initialization
├── Child Context 1: "when user is authenticated"
│   ├── Setup: Login user
│   └── Test: "should access profile"
└── Child Context 2: "when user is anonymous"
    ├── Setup: No login
    └── Test: "should redirect to login"
```

**Key Characteristic**: Child contexts inherit parent actions

**Implementation**:
```csharp
// Parent context
private IContextualTestCaseBuilder parentBuilder;

// Create child context
IContextualTestCaseBuilder childBuilder = 
    parentBuilder.AddChildContext("child context");

// Child inherits all parent actions automatically
```

### 3. Adapter Pattern (Framework Independence)

**Purpose**: Decouple core logic from external test frameworks

**Key Interfaces**:
```csharp
public interface ITestProviderAdapter
{
    TestInstance RetrieveTestInstance();
}

public interface ITestContextAdapter
{
    void ExecuteSetupOnce(Action action);
    void ExecuteSetupEach(Action action);
    void ExecuteAct(Action action);
    void ExecuteTest(Action test);
    void ExecuteTeardownEach(Action action);
    void ExecuteTeardownOnce(Action action);
}

public interface ILoggingAdapter
{
    void Log(string message);
}
```

**Benefits**:
- Core framework remains framework-agnostic
- Easy to add new test runner integrations
- Testable in isolation
- Clear integration boundaries

### 4. Strategy Pattern (Execution Configuration)

**Purpose**: Allow different execution strategies without changing core logic

**Implementation**:
```csharp
public class TestCaseExecutorSettings
{
    public bool StopOnAssertFailure { get; set; }
    // Other configurable behaviors
}

// Usage
var executor = new TestCaseExecutor(settings);
```

**Benefits**:
- Configurable behavior
- No need to modify executor for new strategies
- Easy to test different execution modes

### 5. Template Method Pattern (Lifecycle Execution)

**Purpose**: Define skeleton of test execution with fixed steps

**Test Lifecycle Template**:
```
1. SetupOnce (parent → child)
2. For each test:
   a. SetupEach (parent → child)
   b. Act (parent → child)
   c. Test Assertion
   d. TeardownEach (child → parent)
3. TeardownOnce (child → parent)
```

**Key Invariants**:
- Order is fixed and predictable
- Setup flows top-down
- Teardown flows bottom-up
- Act always runs before test

## Critical Implementation Patterns

### Pattern: Action Collection Strategy

**Setup/Act Actions** (Top-Down Inheritance):
```csharp
private IEnumerable<Action> CollectSetupActions()
{
    // First collect parent actions
    if (parentContext != null)
    {
        foreach (var action in parentContext.CollectSetupActions())
            yield return action;
    }
    
    // Then add self actions
    foreach (var action in selfSetupActions)
        yield return action;
}
```

**Teardown Actions** (Bottom-Up Cleanup):
```csharp
private IEnumerable<Action> CollectTeardownActions()
{
    // First process self actions
    foreach (var action in selfTeardownActions)
        yield return action;
    
    // Then parent actions
    if (parentContext != null)
    {
        foreach (var action in parentContext.CollectTeardownActions())
            yield return action;
    }
}
```

**Rationale**: 
- Setup flows parent → child (base environment first)
- Teardown flows child → parent (cleanup in reverse order)
- Matches natural initialization/disposal patterns

### Pattern: Context Switching Callback

**Purpose**: Enable nested context definitions with proper scope switching

**Implementation** (specs.BDD approach):
```csharp
private IContextualTestCaseBuilder coreBuilder;

private void contextCallBack(IContextualTestCaseBuilder childBuilder, Action action)
{
    var previousBuilder = this.coreBuilder;
    this.coreBuilder = childBuilder;  // Switch to child scope
    try
    {
        action.Invoke();  // Execute user's context definition
    }
    finally
    {
        this.coreBuilder = previousBuilder;  // Restore parent scope
    }
}
```

**Usage**:
```csharp
// When user writes:
Describe["Calculator"] = () => {
    BeforeEach(() => { /* setup */ });  // Targets child builder
    It["should work"] = () => { /* test */ };  // Targets child builder
};

// The callback ensures BeforeEach and It target the child context
```

### Pattern: Async-to-Sync Conversion

**Purpose**: Support async operations while maintaining simple Action-based interface

**Implementation**:
```csharp
public void AddSetupOnce(Func<Task> asyncAction)
{
    // Convert async to sync using GetAwaiter().GetResult()
    Action syncAction = () => asyncAction().GetAwaiter().GetResult();
    AddSetupOnce(syncAction);
}
```

**Benefits**:
- Unified interface (all lifecycle hooks use Action)
- Supports both sync and async transparently
- Simpler mental model for test authors
- No need for separate async test methods

**Trade-off**: Async operations block, but this is acceptable for test scenarios

### Pattern: Immutability After Build

**Purpose**: Ensure test cases cannot be modified after construction

**Implementation**:
```csharp
public interface IContextualTestCase
{
    // Read-only properties only
    IEnumerable<Action> SetupOnceActions { get; }
    IEnumerable<Action> SetupEachActions { get; }
    // ... no setters, no mutation methods
}
```

**Benefits**:
- Thread-safe
- Predictable execution
- No accidental modifications
- Can cache built test cases

## Component Relationships

### Core Framework Layer Relationships

```
┌─────────────────────────────────────────────────────┐
│                   User Test Code                    │
└────────────────────┬────────────────────────────────┘
                     │ Inherits from
┌────────────────────▼────────────────────────────────┐
│              Specifications (specs.BDD)             │
│  - BeforeAll, BeforeEach, Act, AfterEach, AfterAll  │
│  - Describe, Context, It builders                   │
└────────────────────┬────────────────────────────────┘
                     │ Uses
┌────────────────────▼────────────────────────────────┐
│        IContextualTestCaseBuilder (specs.core)      │
│  - AddSetupOnce, AddSetupEach, AddAct, etc.        │
│  - AddChildContext                                   │
│  - Build() → IContextualTestCase                    │
└────────────────────┬────────────────────────────────┘
                     │ Builds
┌────────────────────▼────────────────────────────────┐
│         IContextualTestCase (specs.core)            │
│  - Immutable test case model                        │
│  - Action collections for each lifecycle phase      │
└────────────────────┬────────────────────────────────┘
                     │ Executed by
┌────────────────────▼────────────────────────────────┐
│          TestCaseExecutor (specs.core)              │
│  - Orchestrates lifecycle execution                 │
│  - Respects configuration settings                  │
└────────────────────┬────────────────────────────────┘
                     │ Uses
┌────────────────────▼────────────────────────────────┐
│         ITestContextAdapter (specs.core)            │
│  - Abstracts test framework integration             │
└────────────────────┬────────────────────────────────┘
                     │ Integrates with
┌────────────────────▼────────────────────────────────┐
│              Test Framework (NSpec)                 │
└─────────────────────────────────────────────────────┘
```

### Data Flow: Building and Executing Tests

```
User Code
    ↓ Calls BeforeEach, Act, It, etc.
specs.BDD Layer (Specifications)
    ↓ Delegates to
IContextualTestCaseBuilder
    ↓ Accumulates
Actions in Collections
    ↓ On Build()
IContextualTestCase (Immutable)
    ↓ Passed to
TestCaseExecutor
    ↓ Executes lifecycle using
ITestContextAdapter
    ↓ Runs on
Actual Test Framework
```

## Key Technical Decisions

### Decision 1: Immutable Test Cases

**Choice**: Test cases become immutable after `Build()`

**Rationale**:
- Prevents accidental modifications during execution
- Enables safe caching and reuse
- Clearer separation between definition and execution
- Thread-safe by design

**Impact**: Builder must be used for all test construction

### Decision 2: Action-Based Interface

**Choice**: All lifecycle hooks use `Action` (not `Func<T>` or Task)

**Rationale**:
- Simpler mental model
- Unified interface for sync and async
- Tests don't return values
- Async support via `GetAwaiter().GetResult()`

**Impact**: Framework handles async-to-sync conversion internally

### Decision 3: Top-Down Setup, Bottom-Up Teardown

**Choice**: Setup flows parent → child, Teardown flows child → parent

**Rationale**:
- Matches natural initialization order (base first, derived second)
- Matches natural cleanup order (derived first, base second)
- Ensures resources are available when needed
- Cleanup happens in proper dependency order

**Impact**: Framework must track and reverse action order for teardown

### Decision 4: Framework Agnostic Core

**Choice**: Core framework has no direct dependencies on test frameworks

**Rationale**:
- Testable in isolation
- Can integrate with multiple test runners
- Core logic remains focused and clean
- Easy to add new integrations

**Impact**: Adapter layer required for each test framework

### Decision 5: Support Two API Styles

**Choice**: Maintain both property-based (Spec) and method-based (Specifications)

**Rationale**:
- Legacy support for existing codebases
- Different teams prefer different styles
- Both approaches are valid
- Migration can be gradual

**Impact**: Documentation must cover both, some code duplication

### Decision 6: Hierarchical Context Model

**Choice**: Support nested contexts with inheritance

**Rationale**:
- Reflects natural test organization
- Enables DRY principle (shared setup in parent)
- More expressive than flat structure
- Matches BDD thinking (describe → context → it)

**Impact**: More complex builder/executor implementation

## Critical Implementation Paths

### Path 1: Test Case Construction

```
User calls BeforeEach(() => {})
    ↓
Specifications.BeforeEach delegates to coreBuilder.AddSetupEach
    ↓
ContextualTestCaseBuilder adds action to setupEachActions list
    ↓
User calls Build() (implicitly via framework)
    ↓
Builder creates ContextualTestCase with collected actions
    ↓
Returns IContextualTestCase (immutable)
```

### Path 2: Test Case Execution

```
TestCaseExecutor receives IContextualTestCase
    ↓
Collects SetupOnceActions (parent → child)
    ↓
Executes via adapter.ExecuteSetupOnce() for each action
    ↓
For each test:
    ├─ Collects SetupEachActions (parent → child)
    ├─ Executes via adapter.ExecuteSetupEach()
    ├─ Collects ActActions (parent → child)
    ├─ Executes via adapter.ExecuteAct()
    ├─ Executes test via adapter.ExecuteTest()
    ├─ Collects TeardownEachActions (child → parent)
    └─ Executes via adapter.ExecuteTeardownEach()
    ↓
Collects TeardownOnceActions (child → parent)
    ↓
Executes via adapter.ExecuteTeardownOnce() for each action
```

### Path 3: Child Context Creation

```
User writes: Describe["name"] = () => { ... }
    ↓
ContextBuilder indexer invoked with "name" and action
    ↓
Calls coreBuilder.AddChildContext("name")
    ↓
Creates new ContextualTestCaseBuilder with current as parent
    ↓
Invokes contextCallBack(childBuilder, action)
    ↓
Callback switches this.coreBuilder to childBuilder
    ↓
Executes user's action (defines child context contents)
    ↓
Callback restores this.coreBuilder to parent
    ↓
Child context now configured, parent context resumes
```

## Pattern Summary

| Pattern | Purpose | Benefit |
|---------|---------|---------|
| Builder | Construct test cases | Fluent API, immutability |
| Composite | Hierarchical contexts | Inheritance, organization |
| Adapter | Framework independence | Flexibility, testability |
| Strategy | Execution configuration | Extensibility |
| Template Method | Lifecycle execution | Consistency, predictability |

These patterns work together to create a flexible, maintainable, and expressive testing framework.
