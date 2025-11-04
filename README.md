# Specs

A .NET testing framework providing a fluent, contextual test case builder pattern built on top of NSpec.

## 📚 Documentation

**Before working on this project**, review the comprehensive documentation in the [Memory Bank](src/memory-bank/README.md):

- **[Project Overview](src/memory-bank/project-overview.md)** - Start here for project introduction
- **[Architecture](src/memory-bank/architecture.md)** - Design patterns and architectural decisions
- **[Core Components](src/memory-bank/core-components.md)** - Detailed component documentation
- **[Test Patterns](src/memory-bank/test-patterns.md)** - Usage examples and best practices
- **[Glossary](src/memory-bank/glossary.md)** - Complete terminology reference

## Quick Start

### Solution Structure
- `specs.core` - Core testing framework (.NET Standard 2.1)
- `specs.core.UnitTests` - Unit tests for the framework
- `TestHarness` - NSpec integration helper

### Key Features
- Hierarchical test organization with parent-child contexts
- Comprehensive test lifecycle hooks (SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce)
- Full async/await support
- Fluent API for test construction
- Framework-agnostic core with adapter pattern

## Technology Stack
- .NET Standard 2.1
- NSpec (BDD framework)
- NUnit (Unit testing)
- NFluent (Fluent assertions)

## License
See [LICENSE](LICENSE) file for details.
