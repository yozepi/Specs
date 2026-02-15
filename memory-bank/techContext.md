# Technical Context: Specs

## Technology Stack

### Target Framework
- **.NET Standard 2.1**
  - Chosen for broad compatibility across .NET Framework, .NET Core, and .NET 5+
  - Supports C# 8.0 features (nullable reference types, async streams)
  - Minimum runtime: .NET Core 3.0 or .NET Framework 4.8

### Language
- **C# 8.0+**
  - Nullable reference types enabled for better null safety
  - Async/await patterns throughout
  - Modern language features (pattern matching, switch expressions)

### Core Dependencies

#### Testing Frameworks
- **NSpec** (Primary integration target)
  - BDD-style testing framework for .NET
  - Provides context and specification structure
  - Used via TestHarness integration layer

- **NUnit** (For framework unit tests)
  - Unit testing framework
  - Used to test the Specs framework itself
  - Provides assertion library

#### Assertion Library
- **NFluent**
  - Fluent assertion library
  - Readable, expressive assertions
  - Used in unit tests

### Project Structure

```
Specs/
├── src/
│   ├── specs.core/                    # Core framework library (DELIVERABLE)
│   │   └── specs.core.csproj          # netstandard2.1
│   │
│   ├── specs.specification/           # specs.BDD high-level API (DELIVERABLE)
│   │   └── specs.BDD.csproj           # netstandard2.1
│   │
│   ├── specs.core.UnitTests/          # Core framework tests
│   │   └── specs.core.UnitTests.csproj
│   │
│   ├── specs.specification.UnitTests/ # specs.BDD tests
│   │   └── specs.BDD.UnitTests.csproj
│   │
│   ├── TestHarness/                   # Internal testing infrastructure (NOT DELIVERABLE)
│   │   └── TestHarness.csproj         # Temporary NSpec integration for testing Specs itself
│   │                                  # Will be phased out once framework is self-hosting
│   │
│   ├── Spec.cs                        # Legacy property-based API
│   ├── Global.cs                      # Global usings
│   └── Domain/                        # Legacy runtime layer
│       ├── IRunContext.cs
│       ├── IRunContainer.cs
│       ├── ActionSpecDefinition.cs
│       ├── TaskSpecDefinition.cs
│       └── ...
│
├── Examples/
│   └── ConsoleExample/                # Example usage
│
├── memory-bank/                       # Documentation
└── README.md
```

**Note on TestHarness**: This project exists solely to support internal unit testing of the Specs framework during development. It is NOT part of the project output and will eventually be replaced once Specs is mature enough to test itself.

## Development Environment Setup

### Prerequisites
- **.NET SDK 6.0 or later** (for building .NET Standard 2.1 projects)
- **Visual Studio 2019+** or **VS Code** with C# extension
- **Git** for version control

### Getting Started

#### 1. Clone Repository
```bash
git clone https://github.com/yozepi/Specs.git
cd Specs
```

#### 2. Restore Dependencies
```bash
dotnet restore src/Specs.sln
```

#### 3. Build Solution
```bash
dotnet build src/Specs.sln --configuration Release
```

#### 4. Run Tests
```bash
# Run all tests
dotnet test src/Specs.sln

# Run specific test project
dotnet test src/specs.core.UnitTests/specs.core.UnitTests.csproj

# Run with verbose output
dotnet test src/Specs.sln --verbosity detailed
```

### IDE Configuration

#### Visual Studio
- Solution file: `src/Specs.sln`
- Projects organized into solution folders:
  - `Deliverables` - specs.core, specs.specification
  - `Tests` - test projects (specs.core.UnitTests, specs.BDD.UnitTests)
  - `Internal` - TestHarness (temporary internal testing infrastructure)
  - `Examples` - ConsoleExample

#### VS Code
- Open folder: `Specs/`
- Recommended extensions:
  - C# (Microsoft)
  - NuGet Package Manager
  - .NET Core Test Explorer

### Build Configuration

#### Debug vs Release
- **Debug**: Full debug symbols, no optimizations
- **Release**: Optimized, minimal debug info

```bash
# Debug build
dotnet build src/Specs.sln --configuration Debug

# Release build
dotnet build src/Specs.sln --configuration Release
```

## Technical Constraints

### Framework Compatibility
- **Minimum .NET Standard**: 2.1
  - Cannot use .NET 5+ specific features
  - Cannot use C# 9.0+ features (records, init-only properties)
  - Async streams supported (C# 8.0)

### Nullable Reference Types
- **Enabled throughout**: `<Nullable>enable</Nullable>`
- All public APIs must handle nullability correctly
- Use `!` operator sparingly and only when guaranteed non-null

### Immutability Requirements
- Test cases must be immutable after `Build()`
- No public setters on `IContextualTestCase`
- Collections returned must be read-only

### Async Execution Model
- All async operations converted to synchronous via `GetAwaiter().GetResult()`
- Blocking is acceptable in test scenarios
- No async/await in public interface (all methods return `void`)

### Framework Independence
- **specs.core** must have zero dependencies on test frameworks
- Integration only through adapter interfaces
- Adapters implement framework-specific behavior

## Dependencies Management

### NuGet Packages

#### specs.core.csproj
```xml
<ItemGroup>
  <!-- No external dependencies -->
  <!-- Framework is self-contained -->
</ItemGroup>
```

#### specs.specification.csproj (specs.BDD)
```xml
<ItemGroup>
  <ProjectReference Include="..\specs.core\specs.core.csproj" />
</ItemGroup>
```

#### TestHarness.csproj (Internal Testing Only)
```xml
<ItemGroup>
  <PackageReference Include="NSpec" Version="[specific-version]" />
  <ProjectReference Include="..\specs.core\specs.core.csproj" />
</ItemGroup>
```

**Note**: TestHarness is internal infrastructure only, not a deliverable. Used temporarily to test Specs framework itself until the framework can be self-hosting.

#### Test Projects
```xml
<ItemGroup>
  <PackageReference Include="NUnit" Version="3.x" />
  <PackageReference Include="NUnit3TestAdapter" Version="4.x" />
  <PackageReference Include="NFluent" Version="2.x" />
  <ProjectReference Include="..\specs.core\specs.core.csproj" />
  <ProjectReference Include="..\specs.specification\specs.BDD.csproj" />
</ItemGroup>
```

### Dependency Guidelines
- **Minimize dependencies**: Keep core framework dependency-free
- **Version pinning**: Pin to specific major versions for stability
- **Compatibility**: Ensure all dependencies support .NET Standard 2.1

## Testing Strategy

### Unit Testing Approach
- **Framework tests itself using NUnit**
- Comprehensive coverage of builder, executor, and adapter components
- Test lifecycle order and action collection
- Test async-to-sync conversion

### Test Organization
```
specs.core.UnitTests/
├── Framework/
│   ├── Adapters/
│   │   ├── TestContextAdapter_Tests.cs
│   │   └── TestInstance_Tests.cs
│   ├── Processors/
│   │   ├── TestCases/
│   │   │   ├── ContextualTestCase_Tests.cs
│   │   │   ├── ContextualTestCaseBuilder_Tests/
│   │   │   │   ├── SetupOnce.cs
│   │   │   │   ├── SetupEach.cs
│   │   │   │   ├── Act.cs
│   │   │   │   ├── TeardownEach.cs
│   │   │   │   ├── TeardownOnce.cs
│   │   │   │   └── AllMethodsInteraction.cs
│   │   │   └── RunAllBuilder_Tests.cs
│   │   └── TestExecutors/
│   │       └── TestCaseExecutor_Tests.cs
│   └── ExceptionExtensions/
│       └── ExceptionExtensions_Tests.cs
```

### Running Tests
```bash
# Run all tests
dotnet test src/Specs.sln

# Run with code coverage (requires coverlet)
dotnet test src/Specs.sln /p:CollectCoverage=true

# Run specific test class
dotnet test --filter "FullyQualifiedName~ContextualTestCaseBuilder_Tests"

# Run tests matching pattern
dotnet test --filter "Name~SetupOnce"
```

## Build and CI/CD

### Local Build Commands
```bash
# Clean
dotnet clean src/Specs.sln

# Restore
dotnet restore src/Specs.sln

# Build
dotnet build src/Specs.sln --configuration Release

# Test
dotnet test src/Specs.sln --configuration Release

# Pack (if creating NuGet packages)
dotnet pack src/specs.core/specs.core.csproj --configuration Release
dotnet pack src/specs.specification/specs.BDD.csproj --configuration Release
```

### NuGet Package Creation
```bash
# Create package with specific version
dotnet pack src/specs.core/specs.core.csproj \
  --configuration Release \
  --output ./packages \
  /p:Version=1.0.0

# Include symbols
dotnet pack src/specs.core/specs.core.csproj \
  --configuration Release \
  --include-symbols \
  --include-source
```

## Tool Configuration

### Global.cs (Global Usings)
```csharp
// Commonly used namespaces across projects
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
```

### .gitignore
```
# Build outputs
bin/
obj/

# User-specific files
*.user
*.suo
*.userosscache

# Visual Studio
.vs/

# NuGet
*.nupkg
packages/
```

### editorconfig
- Standard C# formatting rules
- 4-space indentation
- UTF-8 encoding
- CRLF line endings (Windows)

## Performance Considerations

### Immutable Collections
- Test cases use read-only collections after build
- No performance penalty during execution
- Small memory overhead for immutability

### Action Collection
- Actions collected once at build time
- Cached for repeated execution
- Minimal overhead per test execution

### Async Overhead
- `GetAwaiter().GetResult()` blocks but acceptable for tests
- No async state machine overhead at test execution time
- Simpler than pure async execution model

## Debugging

### Debug Test Execution
```csharp
// Add breakpoints in user test code
Describe["Calculator"] = () => {
    BeforeEach(() => {
        calculator = new Calculator();  // Set breakpoint here
    });
    
    It["should add"] = () => {
        result = calculator.Add(2, 3);  // Set breakpoint here
        result.Should().Be(5);
    };
};
```

### Debug Framework Code
- Set breakpoints in `ContextualTestCaseBuilder`
- Watch action collections being built
- Step through `TestCaseExecutor` lifecycle
- Inspect `ITestContextAdapter` calls

### Logging
- Implement `ILoggingAdapter` for custom logging
- Log lifecycle phase transitions
- Track action execution order

## Common Issues and Solutions

### Issue: Async deadlocks
**Problem**: Async operations hang during test execution

**Solution**: Ensure async methods use `ConfigureAwait(false)` in libraries being tested

### Issue: Test isolation failures
**Problem**: Tests affect each other's state

**Solution**: Ensure proper teardown in `AfterEach` or `AfterAll`

### Issue: Lifecycle order confusion
**Problem**: Setup doesn't run in expected order

**Solution**: Remember parent → child for setup, child → parent for teardown

### Issue: NullReferenceException in tests
**Problem**: Object not initialized in setup

**Solution**: Verify `BeforeEach` runs before `Act` and test

## Version Compatibility Matrix

| Specs Version | .NET Standard | C# Version | NSpec | NUnit |
|---------------|---------------|------------|-------|-------|
| 1.x           | 2.1           | 8.0        | 3.x   | 3.x   |

## Future Technical Considerations

### Potential Upgrades
- **Target .NET 6+**: Would enable C# 10 features (records, global usings)
- **Pure async API**: Alternative to `GetAwaiter().GetResult()` approach
- **Source generators**: Could generate test registration code
- **Incremental compilation**: Better build performance

### Backward Compatibility
- Maintain .NET Standard 2.1 for broad compatibility
- Support legacy `Spec` class indefinitely
- Adapter pattern allows new integrations without breaking changes

### Performance Optimization
- Action collection caching (already implemented)
- Lazy evaluation of child contexts
- Parallel test execution (would require thread-safety improvements)

## Documentation and Resources

### Internal Documentation
- XML documentation comments on all public APIs
- Memory bank files in `memory-bank/`
- Examples in `Examples/ConsoleExample/`

### External Resources
- .NET Standard 2.1 specification
- C# 8.0 language reference
- NSpec documentation
- NUnit documentation

## Code Quality Tools

### Recommended
- **StyleCop**: C# style checking
- **FxCop / Roslyn Analyzers**: Code analysis
- **SonarQube**: Code quality and security
- **BenchmarkDotNet**: Performance benchmarking (if needed)

### Static Analysis
```bash
# Run Roslyn analyzers
dotnet build src/Specs.sln /p:TreatWarningsAsErrors=true

# Code metrics
dotnet build src/Specs.sln /p:CodeMetrics=true
```

## Summary

The Specs framework is built on solid technical foundations:
- **.NET Standard 2.1** for broad compatibility
- **Minimal dependencies** for maintainability
- **Clear separation of concerns** through layered architecture
- **Comprehensive test coverage** ensuring reliability
- **Modern C# features** while maintaining compatibility

The technical stack enables a flexible, maintainable testing framework that can evolve with the .NET ecosystem while maintaining backward compatibility.
