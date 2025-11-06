# Specs Framework - Memory Bank

This memory bank contains comprehensive documentation about the Specs testing framework for .NET.

## Documentation Index

### 📋 [Project Overview](project-overview.md)
High-level introduction to the Specs framework, including:
- Project purpose and goals
- Solution structure (3 projects: specs.core, specs.core.UnitTests, TestHarness)
- Technology stack (.NET Standard 2.1, NSpec, NUnit, NFluent)
- Target users and use cases

**Start here** if you're new to the project.

### 🏗️ [Architecture](architecture.md)
Detailed architectural documentation covering:
- Design patterns (Builder, Composite, Adapter, Strategy)
- Architectural layers and responsibilities
- Test lifecycle flow
- Action collection strategies
- Async/sync abstraction
- Data flow diagrams
- Key design decisions

**Reference this** to understand how the framework is structured and why.

### 🔧 [Core Components](core-components.md)
In-depth documentation of framework components:
- Builder interfaces and implementations
- Test case models (ITestCase, IContextualTestCase, ITestCaseContext)
- Test execution engine (TestCaseExecutor)
- Adapter interfaces (logging, test provider, test context)
- Helper classes (RunAllBuilder, TestInstance)
- Component interaction diagrams

**Use this** when working with specific framework classes and interfaces.

### 📝 [Test Patterns](test-patterns.md)
Practical examples and best practices:
- Basic test structure
- Hierarchical contexts
- Sharing state across tests
- Async test support
- Parameterized tests
- Exception testing
- Integration testing patterns
- Test isolation techniques
- Arrange-Act-Assert pattern
- Mocking strategies
- Builder pattern for test data
- Focus tags for debugging

**Reference this** when writing tests or learning framework usage patterns.

### 📖 [Glossary](glossary.md)
Comprehensive terminology reference:
- Core concepts (Act, Adapter, Assertion, Async/Await)
- Lifecycle hooks (SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce)
- Test organization (Context, Child Context, Focus Tags)
- Framework components
- Testing patterns (AAA, BDD, Integration Tests, Mocks)
- NSpec integration terms
- Technical terms and abbreviations
- Design principles

**Consult this** when you encounter unfamiliar terms or need clarification.

## Quick Reference

### Test Lifecycle Order
```
1. SetupOnce (Parent → Child)
2. For each test:
   - SetupEach (Parent → Child)
   - Act (Parent → Child)
   - Test Assertion
   - TeardownEach (Child → Parent)
3. TeardownOnce (Child → Parent)
```

### Key Classes
- `IContextualTestCaseBuilder` - Fluent API for building test cases
- `ContextualTestCaseBuilder` - Concrete builder implementation
- `IContextualTestCase` - Built test case with hierarchy support
- `TestCaseExecutor` - Orchestrates test execution
- `nSpecTestHarness` - Base class for NSpec integration

### Project Structure
```
src/
├── specs.core/              # Core framework library
│   └── Framework/
│       ├── Adapters/        # Framework integration abstractions
│       ├── Processors/      # Test case models and executors
│       └── Settings/        # Configuration
├── specs.core.UnitTests/    # Unit tests for the framework
└── TestHarness/             # NSpec integration helper
```

## Contributing to Documentation

When adding new features or making significant changes:

1. Update **project-overview.md** if the change affects high-level goals or structure
2. Update **architecture.md** if the change involves new patterns or architectural decisions
3. Update **core-components.md** if new components are added or existing ones change significantly
4. Add examples to **test-patterns.md** for new usage patterns
5. Add new terms to **glossary.md** for any new concepts introduced
6. Update this README.md if the documentation structure changes

## Version Information

- **.NET Target**: netstandard2.1
- **Primary Dependencies**: NSpec, NUnit, NFluent
- **Language**: C# with nullable reference types enabled

## Change Tracking

This project uses a global Cline workflow for tracking and documenting solution changes:

**Global Workflow**: `~/OneDrive/Documents/Cline/Workflows/track-changes.md`

**Purpose**: Automatically analyze and document:
- Git commit history since last milestone
- File additions, modifications, and deletions
- New components (classes, interfaces)
- Architectural impacts
- Memory bank updates needed

**Change Documentation**: `memory-bank/changes/`
- `_current.md` - Work-in-progress changes
- `YYYY-MM-DD.md` - Archived milestone changes

**Usage**: From project root, run:
```powershell
# Windows
~\OneDrive\Documents\Cline\Workflows\analyze-changes.ps1

# Linux/Mac
~/OneDrive/Documents/Cline/Workflows/analyze-changes.sh
```

**When to Use**: Run at project milestones such as feature completion, releases, or significant architectural changes.

See the global workflow documentation for detailed usage instructions.

## Related Resources

- [NSpec Documentation](http://nspec.org/)
- [NUnit Documentation](https://nunit.org/)
- [NFluent Documentation](http://www.n-fluent.net/)

---

**Last Updated**: 2025-11-03

For questions or clarifications about this documentation, please refer to the source code or create an issue in the project repository.
