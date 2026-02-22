# specs.BDD.VSTestAdapter Implementation Plan

**Version**: 1.0  
**Date**: 2026-02-07  
**Status**: Planning Complete - Ready for Implementation

## Overview

This document outlines the implementation plan for integrating the Specs BDD testing framework with Visual Studio Test Platform (VSTest). This adapter will enable:
- Test discovery in Visual Studio Test Explorer
- Test execution via `dotnet test`
- Integration with Rider, VS Code, and other IDEs that support VSTest

## Background

### Why This Adapter is Needed

The Specs framework currently lacks integration with .NET's standard test infrastructure. Tests are written using:
- **specs.BDD API**: Method-based lifecycle (`BeforeAll()`, `BeforeEach()`, `Act()`, etc.)
- **Context Attribute**: `[Context]` attribute marks methods as test context builders
- **Fluent API**: Tests defined using indexers (`Describe[name]`, `Context[name]`, `It[name]`)

Without a VSTest adapter, tests cannot be:
- Discovered in Test Explorer
- Run via `dotnet test`
- Debugged through standard IDE tooling

### Design Principles

1. **Attribute-Based Discovery**: Use `[Context]` attribute for safe, fast test discovery
2. **Validation**: Enforce that all Specification classes have at least one `[Context]` method
3. **BDD-Specific**: Named `specs.BDD.VSTestAdapter` to support future paradigms (Gherkin, etc.)
4. **Hierarchical Support**: Preserve test hierarchy in display names and structure

## Architecture

### Discovery Algorithm

```
For each assembly in test run:
  1. Find all classes inheriting from Specifications or Spec
  2. For each specification class found:
     a. Find all methods decorated with [Context] attribute
     b. If NO [Context] methods found → THROW InvalidSpecificationException
     c. For each [Context] method:
        i.   Instantiate the specification class
        ii.  Invoke the [Context] method (builds test structure via fluent API)
        iii. Extract test hierarchy from the builder
        iv.  Flatten hierarchy into individual tests
        v.   Generate VSTest TestCase for each test
```

### Component Architecture

```
┌─────────────────────────────────────────┐
│   Visual Studio Test Explorer           │
│   / dotnet test / Rider / VS Code       │
└────────────────┬────────────────────────┘
                 │ VSTest Protocol
┌────────────────▼────────────────────────┐
│   specs.BDD.VSTestAdapter               │
│   ├─ Discovery/                         │
│   │   ├─ SpecsTestDiscoverer            │
│   │   ├─ SpecificationClassScanner      │
│   │   ├─ ContextMethodScanner           │
│   │   ├─ TestStructureExtractor         │
│   │   └─ InvalidSpecificationException  │
│   ├─ Execution/                         │
│   │   ├─ SpecsTestExecutor              │
│   │   ├─ TestInvoker                    │
│   │   └─ ResultReporter                 │
│   └─ Conversion/                        │
│       ├─ TestCaseConverter              │
│       └─ TestIdentifier                 │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│   specs.core / specs.BDD                │
│   (Existing Specs Framework)            │
└─────────────────────────────────────────┘
```

## Project Structure

```
src/
├── specs.core/                          # Core framework (existing)
├── specs.BDD/                           # BDD API (existing, formerly specs.specification)
├── specs.BDD.VSTestAdapter/             # NEW - VSTest adapter for BDD
│   ├── Discovery/
│   │   ├── SpecsTestDiscoverer.cs
│   │   ├── SpecificationClassScanner.cs
│   │   ├── ContextMethodScanner.cs
│   │   ├── TestStructureExtractor.cs
│   │   └── InvalidSpecificationException.cs
│   ├── Execution/
│   │   ├── SpecsTestExecutor.cs
│   │   ├── TestInvoker.cs
│   │   └─ ResultReporter.cs
│   ├── Conversion/
│   │   ├── TestCaseConverter.cs
│   │   └── TestIdentifier.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   └── specs.BDD.VSTestAdapter.csproj
└── specs.BDD.VSTestAdapter.Tests/       # NEW - Adapter tests
    └── specs.BDD.VSTestAdapter.Tests.csproj
```

## Component Specifications

### 1. SpecsTestDiscoverer

**Purpose**: Main entry point for test discovery in VSTest protocol

**Interface**: `ITestDiscoverer`

**Attributes**:
```csharp
[FileExtension(".dll")]
[FileExtension(".exe")]
[DefaultExecutorUri("executor://SpecsTestExecutor")]
[Category("managed")]
```

**Responsibilities**:
1. Receive discovery requests from VSTest
2. Load test assemblies
3. Coordinate discovery process
4. Report discovered tests to VSTest

**Key Methods**:
```csharp
public void DiscoverTests(
    IEnumerable<string> sources,
    IDiscoveryContext discoveryContext,
    IMessageLogger logger,
    ITestCaseDiscoverySink discoverySink)
```

### 2. SpecificationClassScanner

**Purpose**: Find classes that inherit from `Specifications` or `Spec`

**Method**:
```csharp
public static IEnumerable<Type> FindSpecificationClasses(Assembly assembly)
{
    return assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract)
        .Where(t => typeof(Specifications).IsAssignableFrom(t) || 
                   typeof(Spec).IsAssignableFrom(t));
}
```

### 3. ContextMethodScanner

**Purpose**: Find methods decorated with `[Context]` attribute

**Method**:
```csharp
public static IEnumerable<MethodInfo> FindContextMethods(Type specificationClass)
{
    return specificationClass
        .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .Where(m => m.GetCustomAttribute<ContextAttribute>() != null);
}
```

**Validation**:
- If no `[Context]` methods found → throw `InvalidSpecificationException`

### 4. TestStructureExtractor

**Purpose**: Invoke `[Context]` methods and extract test structure

**Process**:
1. Instantiate specification class
2. Get internal builder reference
3. Invoke `[Context]` method (builds test structure using fluent API)
4. Call `builder.Build()` to get immutable test case
5. Flatten hierarchical structure into individual tests

**Key Method**:
```csharp
public static IEnumerable<SpecTest> ExtractTests(Type specClass, MethodInfo contextMethod)
{
    // 1. Instantiate specification class
    var instance = Activator.CreateInstance(specClass);
    
    // 2. Get builder (via reflection or internal access)
    var builder = GetBuilder(instance);
    
    // 3. Invoke [Context] method to build structure
    contextMethod.Invoke(instance, null);
    
    // 4. Build immutable test case
    var testCase = builder.Build();
    
    // 5. Flatten and return
    return FlattenTests(testCase, contextMethod);
}
```

### 5. TestCaseConverter

**Purpose**: Convert Specs test structure to VSTest `TestCase` format

**Test Identification**:
```
Format: {AssemblyName}::{ClassName}::{MethodName}::{ContextPath}::{TestDescription}

Example:
MyTests::CalculatorSpecs::describe_addition::Calculator → when adding → should return sum
```

**Display Name Format**:
```
{ContextPath} → {TestDescription}

Example:
Calculator → when adding → should return sum
```

**Key Method**:
```csharp
public static TestCase ToVSTestCase(SpecTest specTest, string source)
{
    var fullyQualifiedName = GenerateFullyQualifiedName(specTest);
    var displayName = GenerateDisplayName(specTest);
    
    var testCase = new TestCase(fullyQualifiedName, ExecutorUri, source)
    {
        DisplayName = displayName,
        CodeFilePath = GetSourceFilePath(specTest),
        LineNumber = GetSourceLineNumber(specTest)
    };
    
    // Store metadata for execution
    testCase.SetPropertyValue(TestProperties.ContextMethod, specTest.ContextMethod.Name);
    testCase.SetPropertyValue(TestProperties.ClassName, specTest.ContextMethod.DeclaringType.FullName);
    testCase.SetPropertyValue(TestProperties.TestDescription, specTest.TestDescription);
    
    return testCase;
}
```

### 6. SpecsTestExecutor

**Purpose**: Execute tests via VSTest protocol

**Interface**: `ITestExecutor`

**Attributes**:
```csharp
[ExtensionUri("executor://SpecsTestExecutor")]
```

**Key Methods**:
```csharp
public void RunTests(
    IEnumerable<TestCase> tests,
    IRunContext runContext,
    IFrameworkHandle frameworkHandle)
{
    foreach (var testCase in tests)
    {
        var testResult = TestInvoker.RunTest(testCase);
        frameworkHandle.RecordResult(testResult);
    }
}
```

### 7. InvalidSpecificationException

**Purpose**: Thrown when specification class has no `[Context]` methods

**Usage**:
```csharp
if (!contextMethods.Any())
{
    throw new InvalidSpecificationException(
        $"Specification class '{specClass.Name}' has no methods decorated with [Context] attribute. " +
        $"All Specification classes must have at least one [Context] method.");
}
```

## Data Models

### SpecTest

```csharp
public class SpecTest
{
    public MethodInfo ContextMethod { get; set; }
    public string ContextPath { get; set; }
    public string TestDescription { get; set; }
    public Action TestAction { get; set; }
    public Type SpecificationClass { get; set; }
}
```

### TestProperties

```csharp
public static class TestProperties
{
    public static readonly TestProperty ContextMethod = 
        TestProperty.Register("SpecsBDD.ContextMethod", "Context Method", typeof(string), typeof(SpecsTestDiscoverer));
    
    public static readonly TestProperty ClassName = 
        TestProperty.Register("SpecsBDD.ClassName", "Class Name", typeof(string), typeof(SpecsTestDiscoverer));
    
    public static readonly TestProperty TestDescription = 
        TestProperty.Register("SpecsBDD.TestDescription", "Test Description", typeof(string), typeof(SpecsTestDiscoverer));
}
```

## Project Configuration

### specs.BDD.VSTestAdapter.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <Nullable>enable</Nullable>
    <LangVersion>8.0</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.TestPlatform.ObjectModel" Version="17.8.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\specs.core\specs.core.csproj" />
    <ProjectReference Include="..\specs.BDD\specs.BDD.csproj" />
  </ItemGroup>
</Project>
```

## Implementation Checklist

### Phase 1: Project Setup ✅ COMPLETE
- [x] Create `specs.BDD.VSTestAdapter` project
- [x] Add Microsoft.TestPlatform.ObjectModel NuGet package
- [x] Reference specs.core and specs.BDD projects
- [x] Create directory structure (Discovery/, Execution/, Conversion/)
- [x] Create `specs.BDD.VSTestAdapter.Tests` project
- [x] Target framework: netstandard2.1 (matches specs.core and specs.BDD)
- [x] Add both projects to Specs.sln
- [x] Build and verify project structure

### Phase 2: Discovery Implementation ✅ COMPLETE
- [x] Create `InvalidSpecificationException`
- [x] Implement `SpecificationClassScanner`
- [x] Implement `ContextMethodScanner` (with ValidateHasContextMethods)
- [x] Create `SpecTest` data model
- [x] Implement `TestStructureExtractor`
  - [x] Instantiate specification classes (one per [Context] method)
  - [x] Access internal `coreBuilder` field via reflection
  - [x] Invoke [Context] methods to populate builder
  - [x] Flatten test hierarchy (handles arbitrary nesting depth)
- [x] Create `TestProperties` for metadata (ClassName, ContextMethod, ContextPath, TestDescription)
- [x] Implement `TestCaseConverter`
  - [x] Generate unique FullyQualifiedName (ClassName::Method::ContextPath::TestDescription)
  - [x] Create display names (ContextPath → TestDescription)
  - [x] Set all metadata properties on TestCase
- [x] Implement `SpecsTestDiscoverer`
  - [x] [FileExtension(".dll")], [FileExtension(".exe")], [DefaultExecutorUri] attributes
  - [x] Coordinate full discovery flow
  - [x] Handle exceptions and log warnings
- [x] Add `SpecsTestExecutor` stub (Phase 3 placeholder) with [ExtensionUri]
- [x] Add `AssemblyInfo.cs`
- [x] Remove stray Microsoft.TestPlatform package from specs.BDD.csproj

### Phase 3: Execution Implementation ✅ COMPLETE
- [x] Implement `TestFinder`
  - [x] Navigate IContextualTestCase hierarchy by context path segments
  - [x] Return (ITestCase, Action) tuple or null if not found
- [x] Implement `TestInvoker`
  - [x] Resolve test from TestCase metadata (ClassName, ContextMethod, ContextPath, TestDescription)
  - [x] Load assembly from TestCase.Source, strip AQN suffix to get type name
  - [x] Instantiate specification class and invoke [Context] method
  - [x] Call builder.Build() via coreBuilder reflection
  - [x] Use TestFinder to navigate to correct test
  - [x] Execute lifecycle: SetupOnce → SetupEach → Act → test → TeardownEach → TeardownOnce
  - [x] Teardown always runs even when test action throws (ExceptionDispatchInfo rethrow)
- [x] Implement `ResultReporter`
  - [x] Success() creates Passed TestResult with timing
  - [x] Failure() creates Failed TestResult with ErrorMessage + StackTrace
  - [x] Unwraps nested TargetInvocationException to surface root cause
- [x] Implement `SpecsTestExecutor` (full, replacing stub)
  - [x] RunTests(IEnumerable<TestCase>): iterates, calls RecordStart + TestInvoker.Invoke + RecordResult
  - [x] RunTests(IEnumerable<string>): re-discovers via SpecsTestDiscoverer then delegates
  - [x] Cancel() sets volatile _cancelled flag; loop checks it
  - [x] CollectingSink nested class for re-discovery
- [x] Add [assembly: InternalsVisibleTo("specs.BDD.VSTestAdapter.Tests")] to AssemblyInfo.cs

### Phase 4: Testing
- [ ] Create sample specification classes for testing
- [ ] Test discovery with valid specifications
- [ ] Test discovery with invalid specifications (no [Context] methods)
- [ ] Test execution of passing tests
- [ ] Test execution of failing tests
- [ ] Test hierarchical context display
- [ ] Test in Visual Studio Test Explorer
- [ ] Test with `dotnet test` CLI
- [ ] Test debugging support

### Phase 5: Example Updates
- [ ] Update `ConsoleExample` to use [Context] attributes
- [ ] Create additional examples demonstrating:
  - [ ] Simple single-context tests
  - [ ] Nested hierarchical contexts
  - [ ] Async test support
  - [ ] Exception testing

### Phase 6: Documentation
- [ ] Update memory-bank with adapter architecture
- [ ] Document [Context] attribute requirement
- [ ] Create usage guide for test authors
- [ ] Document migration path for existing tests
- [ ] Add troubleshooting section
- [ ] Update README.md

## Usage Example

### Writing Tests with [Context] Attribute

```csharp
using specs.BDD;
using specs.BDD.Attributes;

public class CalculatorSpecs : Specifications
{
    [Context("Calculator addition")]
    public void describe_addition()
    {
        Calculator calculator = null!;
        int result = 0;
        
        BeforeEach(() => 
        {
            calculator = new Calculator();
        });
        
        Context["when adding positive numbers"] = () =>
        {
            Act(() => 
            {
                result = calculator.Add(2, 3);
            });
            
            It["should return correct sum"] = () =>
            {
                result.Should().Be(5);
            };
        };
        
        Context["when adding negative numbers"] = () =>
        {
            Act(() => 
            {
                result = calculator.Add(-2, -3);
            });
            
            It["should return correct sum"] = () =>
            {
                result.Should().Be(-5);
            };
        };
    }
    
    [Context("Calculator multiplication")]
    public void describe_multiplication()
    {
        // Additional test context...
    }
}
```

### Invalid Specification (Will Throw Exception)

```csharp
// ❌ INVALID - No [Context] methods
public class InvalidSpecs : Specifications
{
    // Missing [Context] attribute!
    public void some_tests()
    {
        It["will not be discovered"] = () => { };
    }
}

// Exception thrown during discovery:
// InvalidSpecificationException: Specification class 'InvalidSpecs' has no methods 
// decorated with [Context] attribute. All Specification classes must have at least 
// one [Context] method.
```

## Future Considerations

### Support for Additional Paradigms

The naming `specs.BDD.VSTestAdapter` anticipates future adapters:
- `specs.Gherkin.VSTestAdapter` - For Gherkin/Cucumber-style tests
- `specs.PropertyBased.VSTestAdapter` - For property-based testing
- etc.

### Potential Enhancements

1. **Parallel Execution**: Support running tests in parallel (requires thread-safety analysis)
2. **Data-Driven Tests**: Support for parameterized tests
3. **Test Filtering**: Advanced filtering by tags, traits, or patterns
4. **Source Mapping**: Accurate source file/line number mapping
5. **Performance**: Optimize discovery for large test suites

## Status Tracking

- [x] Requirements analysis complete
- [x] Architecture design complete
- [x] Component specifications defined
- [x] Implementation started
- [x] Phase 1: Project Setup complete
- [x] Phase 2: Discovery implementation complete
- [x] Phase 3: Execution implemented
- [ ] Phase 4: Integration testing complete
- [ ] Phase 5: Documentation complete
- [ ] Ready for production

## Recent Changes

**2026-02-21 - Phase 3 Complete**

Full execution pipeline implemented and verified. All 112 tests across the full solution pass (0 errors, 0 warnings).

**Phase 3 — Execution:**
- `TestFinder` (internal) — navigates an `IContextualTestCase` hierarchy by splitting a context path
  on " → " and following `ChildContexts` by `Description`; returns `(ITestCase, Action)?` or null
- `TestInvoker` (internal) — reads metadata from `TestCase`, loads assembly, strips AQN to get type
  name, resolves method, builds hierarchy via reflection on `coreBuilder`, calls `TestFinder.Find`,
  executes lifecycle; teardown always runs (ExceptionDispatchInfo preserves stack on rethrow)
- `ResultReporter` (internal) — creates Passed/Failed `TestResult`; unwraps nested
  `TargetInvocationException` to surface root cause in `ErrorMessage` and `ErrorStackTrace`
- `SpecsTestExecutor` (public) — full implementation:
  - `RunTests(IEnumerable<TestCase>)`: iterates tests, `RecordStart` → `TestInvoker.Invoke` → `RecordResult`
  - `RunTests(IEnumerable<string>)`: re-discovers via `SpecsTestDiscoverer` using a `CollectingSink`,
    then delegates to the `TestCase` overload
  - `Cancel()`: sets `volatile bool _cancelled`; loop checks it before each test
- `AssemblyInfo.cs` updated with `[assembly: InternalsVisibleTo("specs.BDD.VSTestAdapter.Tests")]`
- New test fixtures: `SpecWithMixedResults` (gated failure via `TriggerFailure` static flag),
  `SpecWithLifecycle` (records BeforeAll/BeforeEach/Act/AfterEach/AfterAll to static log)
- New test files: `TestFinder_Tests` (13 tests), `ResultReporter_Tests` (10 tests),
  `TestInvoker_Tests` (11 tests) — 34 new tests, all passing

**Key Decisions:**
- `TriggerFailure` static flag on `SpecWithMixedResults`: prevents the intentional failure from
  surfacing when the adapter auto-discovers and runs it during `dotnet test`; only active when
  set explicitly by `TestInvoker_Tests`
- `ExceptionDispatchInfo.Capture().Throw()` preserves original stack trace on test failure rethrow
- `TargetInvocationException` unwrapping in `ResultReporter` applies only to reflection wrappers;
  the test action itself is invoked as a plain `Action`, so it never produces TIE
- `IFrameworkHandle` implements `IMessageLogger` directly — passed to `SpecsTestDiscoverer` for
  re-discovery logging without any adapter

**2026-02-21 - Phase 1 + Phase 2 Complete**

Phase 1 was re-done (prior session's work was not persisted). Phase 2 fully implemented.

**Phase 1 — Project Setup:**
- Created `specs.BDD.VSTestAdapter` project targeting netstandard2.1
- Created `specs.BDD.VSTestAdapter.Tests` test project targeting net8.0 with NUnit + NFluent 3.0.0
- Added `Microsoft.TestPlatform.ObjectModel` v17.8.0 to adapter project
- Created full directory structure (Discovery/, Execution/, Conversion/, Properties/)
- Added both projects to Specs.sln
- Removed stray `Microsoft.TestPlatform` v18 package from specs.BDD.csproj (wrong project)

**Phase 2 — Discovery:**
- `InvalidSpecificationException` — thrown when a Specifications class has no [Context] methods
- `SpecificationClassScanner` — finds all non-abstract `Specifications` subclasses in an assembly
- `ContextMethodScanner` — finds [Context]-decorated methods, validates at least one exists
- `SpecTest` — data model carrying SpecificationClass, ContextMethod, ContextPath, TestDescription
- `TestStructureExtractor` — instantiates spec class, invokes [Context] method, accesses private
  `coreBuilder` field via reflection, calls `Build()`, recursively flattens hierarchy into SpecTests
- `TestProperties` — registers VSTest TestProperty slots: ClassName, ContextMethod, ContextPath, TestDescription
- `TestCaseConverter` — converts SpecTest → VSTest TestCase with FQN, display name, all metadata
- `SpecsTestDiscoverer` — ITestDiscoverer with [FileExtension(".dll/.exe")], [DefaultExecutorUri] attributes;
  coordinates full discovery pipeline with exception-safe per-class and per-method error handling
- `SpecsTestExecutor` — Phase 3 stub with [ExtensionUri("executor://SpecsTestExecutor/v1")]
- `AssemblyInfo.cs` — documentation comments only (no special assembly attributes needed)
- Full solution build: **0 errors, 0 warnings**

**Key Decisions:**
- `coreBuilder` accessed via `FieldInfo` reflection — `IContextualTestCaseBuilder` is a public interface
  so `Build()` can be called without casting to the internal `ContextualTestCaseBuilder` type
- `ITestCaseContext` doesn't expose `ChildContexts`; recursion casts to `IContextualTestCase`
- Executor URI: `executor://SpecsTestExecutor/v1` (used on both discoverer and executor)
- One fresh `Specifications` instance per `[Context]` method invocation (isolation)

---

**Last Updated**: 2026-02-21  
**Next Review**: After Phase 3 completion
