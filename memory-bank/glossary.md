# Glossary: Specs Framework

## Core Concepts

### Act
An action that represents the operation being tested. Runs once before each test in a context. Acts are inherited from parent contexts and compose from parent to child. Part of the Arrange-Act-Assert pattern.

### Adapter
A design pattern used to abstract external dependencies. The framework uses adapters for logging, test context management, and test provider operations, allowing it to remain framework-agnostic.

### Assertion
A statement that verifies expected behavior in a test. In NSpec, assertions are typically written using the `should_be()` extension methods or NFluent's fluent assertion syntax.

### Async/Await
Asynchronous programming pattern supported throughout the framework. All lifecycle hooks support both synchronous (`Action`) and asynchronous (`Func<Task>`) operations.

---

## Lifecycle Hooks

### before / beforeEach
Synonym for `SetupEach`. An action that runs before each test in the context. Multiple `before` actions can be registered and will execute in registration order.

### beforeAll / setupOnce
Synonym for `SetupOnce`. An action that runs once before any tests in the context execute. Useful for expensive setup operations like database connections.

### after / afterEach  
Synonym for `TeardownEach`. An action that runs after each test in the context completes. Used for cleanup operations that should happen after every test.

### afterAll / teardownOnce
Synonym for `TeardownOnce`. An action that runs once after all tests in the context have completed. Used for cleanup of resources created in `beforeAll`.

### SetupOnce
Action that executes once before all tests in a context. Aggregated from parent contexts (parent → child order). Used for expensive initialization.

### SetupEach
Action that executes before each individual test. Aggregated from parent contexts (parent → child order). Used for per-test initialization.

### TeardownEach
Action that executes after each individual test. Aggregated from parent contexts (child → parent order). Used for per-test cleanup.

### TeardownOnce
Action that executes once after all tests in a context. Aggregated from parent contexts (child → parent order). Used for releasing resources.

---

## Test Organization

### Suite (Specification Class)
A class inheriting from `Specifications` that serves as a test suite for a single subject. Example: `Weather_Controller : Specifications` is a suite for the Weather Controller. Suite-level lifecycle methods (`BeforeAll`, `AfterAll`, `BeforeEach`, `AfterEach`) apply to ALL feature sets within the suite.

### Feature Set (`[Describe]` Decorated Method)
A method within a suite class decorated with the `[Describe]` attribute. Each feature set represents one aspect or feature of the subject under test. Feature sets have their own lifecycle control (`BeforeEach`, `AfterEach`) that applies only to specs within that feature set. The `[Describe]` attribute and `Context[...]` indexer are NOT separate paradigms — they work together at different levels of the hierarchy.

### Scenario (`Context[...]` / `Describe[...]` Indexer)
A named grouping of related specs within a feature set, created using the `Context[...]` or `Describe[...]` indexer syntax. Scenarios can be nested to create hierarchical test structures. Each scenario can have its own lifecycle hooks that are inherited by child scenarios.

### Specification (`It[...]` Indexer)
An individual test assertion defined using the `It[...]` indexer. Each specification tests one specific behavior or outcome.

### Child Context
A nested context within a parent context. Inherits all lifecycle hooks from its parent(s) and can add its own. Created using the `ChildTestCase()` method internally, or via nested `Context[...]`/`Describe[...]` syntax in user code.

### Contextual Test Case
A test case that supports hierarchical organization through parent-child context relationships. The core abstraction of the Specs framework.

### Focus Tag (fcontext, fit)
A special tag used to run only specific tests during development. When any test is focused, only focused tests will execute. Must be removed before committing code.

### Global Workflow
An automated Cline workflow stored in `~/OneDrive/Documents/Cline/Workflows/` that provides step-by-step instructions for common tasks across multiple projects. The track-changes workflow documents solution changes at project milestones, analyzes git history, detects new components, and guides memory bank updates. Global workflows are reusable across all projects and include memory bank detection to ensure compatibility.

### Test Instance
A single test to be executed, consisting of a description and an action. Represented by the `TestInstance` class.

---

## Framework Components

### Builder Pattern
Design pattern used for constructing complex test cases through a fluent API. The `ContextualTestCaseBuilder` accumulates actions and configuration, then produces an immutable test case via `Build()`.

### Composite Pattern
Design pattern used for hierarchical test organization. Parent contexts contain child contexts, and all contexts implement the same interface, allowing uniform treatment.

### IContextualTestCase
Interface representing a built test case with support for child contexts. Extends `ITestCase` with a collection of child contexts.

### IContextualTestCaseBuilder
Interface defining the fluent API for building hierarchical test cases. Provides methods for adding lifecycle hooks, tests, and child contexts.

### ITestCase
Core interface representing a test case with lifecycle hooks and a collection of tests to execute.

### ITestCaseContext
Interface representing a named context with its own lifecycle and tests. Used for child contexts within a parent test case.

### TestCaseExecutor
Component responsible for orchestrating test execution. Manages the lifecycle flow and error handling during test runs.

---

## Testing Patterns

### Arrange-Act-Assert (AAA)
Testing pattern where tests are structured in three phases:
- **Arrange**: Setup test data and dependencies (in `before`)
- **Act**: Perform the operation being tested (in `act`)
- **Assert**: Verify expected outcomes (in `it`)

### BDD (Behavior-Driven Development)
Testing methodology that emphasizes describing behavior in natural language. The Specs framework and NSpec follow BDD principles with contexts and specifications.

### Integration Test
Test that verifies interactions between multiple components or with external systems. Often uses real dependencies rather than mocks.

### Mock Object
A test double that simulates the behavior of real objects. Used to isolate the system under test from its dependencies.

### Parameterized Test
A test pattern where the same test logic is executed with different input values. In NSpec, typically implemented using array iteration with `.Do()`.

### Test Isolation
The principle that each test should be independent and not affect other tests. Achieved through proper setup/teardown and avoiding shared mutable state.

### Unit Test
Test that verifies a single unit of code (typically a method or class) in isolation from its dependencies.

---

## NSpec Integration

### nSpec
Behavior-Driven Development framework for .NET. The Specs framework builds on top of NSpec to provide enhanced test case building capabilities.

### nSpecTestHarness
Base class provided by the Specs framework that integrates with NSpec and NUnit. Provides methods for loading and running specs with tag filtering and formatting support.

### Specification (Spec)
A test class that describes the behavior of a system. In NSpec, specs are classes that inherit from `nspec` or `nSpecTestHarness`.

### it
NSpec syntax for defining a test assertion. Example: `it["should return true"] = () => result.should_be_true();`

### context
NSpec syntax for defining a test context or grouping. Example: `context["when user is logged in"] = () => { /* tests */ };`

---

## Technical Terms

### Action Aggregation
The process of collecting lifecycle actions from parent and child contexts into a single composite action. Setup actions aggregate parent → child, teardown actions aggregate child → parent.

### Fluent API
An interface design that provides a readable, flow-like syntax by returning the object itself from methods, enabling method chaining.

### Immutability
Property of objects that cannot be modified after creation. Test cases become immutable after `Build()` is called, ensuring consistency during execution.

### Lazy Evaluation
Deferring execution of code until it's needed. The framework uses lambda expressions and `Action` delegates to enable lazy evaluation of test setup and execution.

### RunAllBuilder
Internal helper class that aggregates multiple actions into a single composite action. Used for SetupOnce and TeardownOnce operations.

---

## Configuration & Settings

### StopOnAssertFailure
Configuration setting in `TestCaseExecutorSettings` that controls whether test execution stops after the first assertion failure or continues running remaining tests.

### Tags
Metadata attached to tests that can be used for filtering which tests to run. NSpec supports tag-based test selection.

### Formatter
Component responsible for formatting and displaying test output. Can be customized via the `IFormatter` interface.

---

## Assertion Libraries

### NFluent
Fluent assertion library used in the Specs framework tests. Provides readable assertion syntax like `Check.That(value).IsEqualTo(expected)`.

### should_be / should_not_be
NSpec's fluent assertion extensions. Example: `result.should_be(42)` or `value.should_not_be_null()`.

---

## Project-Specific Terms

### specs.core
Core library project containing the Specs framework implementation. Targets .NET Standard 2.1. Provides the builder pattern-based architecture with `IContextualTestCaseBuilder` and related components.

### specs.core.UnitTests
Unit test project that tests the functionality of the specs.core library using NUnit and NSpec.

### specs.BDD (specs.specification)
High-level BDD specification library built on top of specs.core. Provides the method-based API through the `Specifications` base class with methods like `BeforeAll()`, `BeforeEach()`, `Act()`, and properties like `Describe`, `Context`, `It`.

### specs.BDD.UnitTests
Unit test project that tests the functionality of the specs.BDD library.

### Specifications (specs.BDD)
Abstract base class in the specs.BDD library providing a method-based DSL for writing BDD-style tests. Modern approach that uses method calls for lifecycle hooks and builder properties for test organization.

### Spec (Legacy) — 📌 REMOVED
Former property-based base class (`src/Spec.cs`). Deleted; use `Specifications`.

### Domain Layer — 📌 REMOVED
Former legacy runtime layer in `src/Domain/` (`IRunContext`, spec definitions, `ActionRunner` / `TaskRunner`). Deleted together with `Spec`.

### TestHarness
Project containing the `nSpecTestHarness` base class for integrating the Specs framework with NSpec and NUnit.

---

## Common Abbreviations

- **AAA**: Arrange-Act-Assert
- **BDD**: Behavior-Driven Development
- **SUT**: System Under Test
- **DI**: Dependency Injection
- **IoC**: Inversion of Control
- **TDD**: Test-Driven Development

---

## Related Concepts

### Test Double
Generic term for any object that stands in for a real object in tests. Includes mocks, stubs, fakes, and spies.

### Test Fixture
The set of preconditions and resources needed to run a test. In the Specs framework, fixtures are established through lifecycle hooks.

### Test Suite
A collection of related tests, often organized within a test class or assembly.

### Regression Test
A test that verifies previously working functionality hasn't been broken by recent changes.

### Smoke Test
A basic test to verify that the most critical functionality works before running more comprehensive tests.

---

## Design Principles

### DRY (Don't Repeat Yourself)
Principle of avoiding code duplication. The framework supports DRY through context inheritance and shared lifecycle hooks.

### Single Responsibility Principle
Each component should have one reason to change. The framework separates concerns: building, representation, execution, and integration.

### Open/Closed Principle
Software should be open for extension but closed for modification. The framework uses interfaces and adapters to allow extension.

### Dependency Inversion Principle  
High-level modules should not depend on low-level modules. Both should depend on abstractions (interfaces). The framework uses adapter interfaces to achieve this.

---

This glossary provides a comprehensive reference
