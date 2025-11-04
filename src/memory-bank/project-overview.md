# Project Overview: Specs

## Purpose
Specs is a .NET testing framework that provides a fluent, contextual test case builder pattern built on top of NSpec. It enables hierarchical test organization with sophisticated lifecycle management and async/await support.

## Solution Structure
The solution consists of three projects:

### 1. specs.core (Core Library)
- **Target Framework**: netstandard2.1
- **Purpose**: Core testing framework implementation
- **Key Features**:
  - Contextual test case building with parent-child relationships
  - Comprehensive test lifecycle hooks (SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce)
  - Full async/await support for all lifecycle hooks
  - Fluent API for test construction
  - Configurable test execution engine

### 2. specs.core.UnitTests
- **Purpose**: Unit tests for the core library
- **Test Framework**: NUnit + NSpec integration
- **Coverage**: Tests for ContextualTestCaseBuilder, ContextualTestCase, and TestCaseExecutor

### 3. TestHarness
- **Purpose**: Provides NSpec integration helper class (nSpecTestHarness)
- **Function**: Bridges NSpec and the Specs framework for easy test execution
- **Features**: Tag filtering, focus tag support, formatter configuration, fail-fast support

## Technology Stack
- .NET Standard 2.1
- NSpec (Behavior-Driven Development framework)
- NUnit (Unit testing framework)
- NFluent (Fluent assertions library)

## Project Goals
1. Provide a more structured approach to BDD-style testing
2. Enable hierarchical test organization through contexts
3. Support both synchronous and asynchronous test operations
4. Maintain clean separation between test lifecycle phases
5. Offer a fluent, intuitive API for test construction

## Target Users
- .NET developers writing behavior-driven tests
- Teams seeking structured, hierarchical test organization
- Projects requiring sophisticated test setup/teardown patterns
- Developers working with async/await patterns in tests
