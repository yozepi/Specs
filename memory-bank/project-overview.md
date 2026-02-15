# Project Overview: Specs

## Purpose
Specs is a .NET testing framework that provides TWO different BDD-style testing approaches built on different architectural foundations. It enables hierarchical test organization with sophisticated lifecycle management and async/await support.

## Solution Structure
The solution consists of **five projects** organized into two main testing approaches:

### Core Framework (3 projects)

#### 1. specs.core (Core Library)
- **Target Framework**: netstandard2.1
- **Purpose**: Core testing framework implementation using Builder pattern
- **Key Features**:
  - Contextual test case building with parent-child relationships
  - Comprehensive test lifecycle hooks (SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce)
  - Full async/await support for all lifecycle hooks
  - Fluent API for test construction
  - Configurable test execution engine
  - Framework-agnostic through adapter pattern

#### 2. specs.core.UnitTests
- **Purpose**: Unit tests for the specs.core library
- **Test Framework**: NUnit + NSpec integration
- **Coverage**: Tests for ContextualTestCaseBuilder, ContextualTestCase, and TestCaseExecutor
- **Location**: Tests solution folder

#### 3. TestHarness
- **Purpose**: Provides NSpec integration helper class (nSpecTestHarness)
- **Function**: Bridges NSpec and the Specs framework for easy test execution
- **Features**: Tag filtering, focus tag support, formatter configuration, fail-fast support
- **Location**: Tests solution folder

### BDD Specification Library (2 projects)

#### 4. specs.BDD (specs.specification)
- **Target Framework**: netstandard2.1
- **Purpose**: High-level BDD specification API built on specs.core
- **Base Class**: `Specifications` (abstract class for test specifications)
- **Key Features**:
  - Method-based lifecycle API: `BeforeAll()`, `BeforeEach()`, `Act()`, `AfterEach()`, `AfterAll()`
  - Builder properties: `Describe`, `Context`, `It`, `ItAsync`
  - Fluent indexer syntax for organizing tests
  - Built on top of `IContextualTestCaseBuilder` from specs.core
  - Both sync and async support throughout

#### 5. specs.BDD.UnitTests (specs.specification.UnitTests)
- **Purpose**: Unit tests for the specs.BDD library
- **Test Framework**: NUnit + specs.core integration
- **Coverage**: Tests for Specifications, AssertBuilder, ContextBuilder
- **Location**: Tests solution folder

## Two Testing Approaches

This solution provides TWO different BDD-style APIs, each with distinct characteristics:

### Approach 1: Property-Based API (Legacy `Spec` class)
Located in `src/Spec.cs`, this provides:
- **Property-based lifecycle**: `BeforeEach`, `Act`, `ActAsync`, `AfterEach` (set as properties)
- **Indexer syntax**: `Context["name"] = () => { };`
- **Built-in exception testing**: `ShouldThrow()`, `ShouldThrowAsync()` methods
- **Runtime integration**: Works with `IRunContext` and Domain layer
- **Example usage**:
```csharp
public class MySpecs : Spec
{
    BeforeEach = () => { /* setup */ };
    Act = () => { /* action */ };
    It["should work"] = () => { /* assertion */ };
}
```

### Approach 2: Method-Based API (specs.BDD `Specifications` class)
Located in `src/specs.specification/Specifications.cs`, this provides:
- **Method-based lifecycle**: `BeforeAll()`, `BeforeEach()`, `Act()` (called as methods)
- **Builder properties**: `Describe`, `Context`, `It` (return builders with indexer syntax)
- **Explicit lifecycle stages**: Including `BeforeAll` and `AfterAll` for once-per-context setup
- **Core integration**: Built on `IContextualTestCaseBuilder` from specs.core
- **Example usage**:
```csharp
public class MySpecs : Specifications
{
    BeforeEach(() => { /* setup */ });
    Act(() => { /* action */ });
    Describe["feature"] = () => {
        It["should work"] = () => { /* assertion */ };
    };
}
```

## Technology Stack
- .NET Standard 2.1
- NSpec (Behavior-Driven Development framework)
- NUnit (Unit testing framework)
- NFluent (Fluent assertions library)

## Project Goals
1. Provide structured approaches to BDD-style testing
2. Enable hierarchical test organization through contexts
3. Support both synchronous and asynchronous test operations
4. Maintain clean separation between test lifecycle phases
5. Offer intuitive APIs for test construction
6. Support multiple testing styles (property-based and method-based)
7. Keep core framework agnostic through adapter pattern

## Target Users
- .NET developers writing behavior-driven tests
- Teams seeking structured, hierarchical test organization
- Projects requiring sophisticated test setup/teardown patterns
- Developers working with async/await patterns in tests
- Teams preferring either property-based or method-based test APIs

## Migration Path
- **Legacy projects**: Can continue using the `Spec` class with property-based API
- **New projects**: Should prefer the `specs.BDD.Specifications` class with method-based API
- Both approaches can coexist in the same solution
