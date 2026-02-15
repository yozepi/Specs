# Specs

A .NET testing framework providing TWO different BDD-style approaches built on different architectural foundations.

## 📚 Documentation

**Before working on this project**, review the comprehensive documentation in the [Memory Bank](memory-bank/README.md):

- **[Project Overview](memory-bank/project-overview.md)** - Start here for project introduction
- **[Architecture](memory-bank/architecture.md)** - Design patterns and architectural decisions
- **[Core Components](memory-bank/core-components.md)** - Detailed component documentation
- **[Test Patterns](memory-bank/test-patterns.md)** - Usage examples and best practices
- **[Glossary](memory-bank/glossary.md)** - Complete terminology reference

## Solution Structure

### Core Framework (3 projects)
- **specs.core** - Core testing framework (.NET Standard 2.1)
- **specs.core.UnitTests** - Unit tests for the framework
- **TestHarness** - NSpec integration helper

### BDD Specification Library (2 projects)
- **specs.BDD** (specs.specification) - High-level BDD API built on specs.core
- **specs.BDD.UnitTests** - Tests for the BDD library

## Two Testing Approaches

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
    BeforeEach(() => { /* setup */ });
    Act(() => { /* action */ });
    Describe["feature"] = () => {
        It["should work"] = () => { /* assertion */ };
    };
}
```

## Key Features
- Hierarchical test organization with parent-child contexts
- Comprehensive test lifecycle hooks (SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce)
- Full async/await support
- Fluent API for test construction
- Framework-agnostic core with adapter pattern
- Two distinct API styles for different preferences

## Technology Stack
- .NET Standard 2.1
- NSpec (BDD framework)
- NUnit (Unit testing)
- NFluent (Fluent assertions)

## License
See [LICENSE](LICENSE) file for details.
