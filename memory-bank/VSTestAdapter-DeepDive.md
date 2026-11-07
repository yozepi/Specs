# Deep Dive: `specs.BDD.VSTestAdapter`

> **⚠️ PARTIALLY OUTDATED (October 2026).** This deep dive predates the suite-tree refactor.
> Where it conflicts with `systemPatterns.md`, **`systemPatterns.md` wins**. Specifically:
>
> | This document says | Current reality |
> |--------------------|-----------------|
> | `TestInvoker.InvokeGroup()` grouped by `(Source, ClassName, ContextMethod, ContextPath)` | `SpecsTestExecutor` groups by `(Source, ClassName)` and calls `TestInvoker.InvokeSuite()` |
> | `TestInvoker` rebuilds the hierarchy per group and runs the lifecycle | `SuiteTreeBuilder` builds the tree once per class; `SelectionTreeWalker` drives once-per-node lifecycle |
> | Reflection on the private `coreBuilder` field | `Specifications.CoreBuilder` accessed directly via `InternalsVisibleTo` |
> | Cancellation via a volatile `_cancelled` flag | `CancellationTokenSource` owned by `SpecsTestExecutor`; token passed to `TestCaseExecutor.ProcessTestCase` |
> | Source location resolved from the `[Feature]` method | Resolved from the `It[...]` lambda (`SpecTest.ItAction`), falling back to the `[Feature]` method |
> | ASCII diagrams below | Memory-bank standard is Mermaid; see `systemPatterns.md` |
>
> Retained for the per-class purpose descriptions and the round-trip narrative, which are still broadly accurate.

This project is a **VSTest adapter** — the bridge that lets Visual Studio's Test Explorer (and `dotnet test`) discover and run tests written with the `specs.BDD` framework. It has three distinct layers: **Discovery**, **Conversion**, and **Execution**.

---

## 1. Purpose of Each Class

### Conversion Layer (the "data model & translation" layer)

| Class | Purpose |
|---|---|
| **`SpecTest`** | A plain data object (POCO) that represents a single discovered test. It carries four pieces of metadata: the `Type` of the Specification class, the `MethodInfo` of the `[Feature]` method, a `ContextPath` string (the breadcrumb trail through nested contexts, separated by " → "), and the `TestDescription` (the `It[...]` assertion text). This is the adapter's **internal** representation of a test — framework-agnostic, not yet a VSTest object. |
| **`TestCaseConverter`** | A static translator that converts a `SpecTest` into a VSTest `TestCase`. It builds a 📌 method-level `FullyQualifiedName` (format: `ClassName.DescribeMethod`, shared by every spec of the `[Feature]`; per-spec uniqueness comes from a deterministic `Id` hashed from `ClassName::MethodName::ContextPath::TestDescription`, and RFC-0017 `ManagedType` / `ManagedMethod` let IDEs resolve the method symbol), a human-readable `DisplayName` (format: `ContextPath → TestDescription`), and embeds all four metadata fields as custom `TestProperty` values on the `TestCase`. It also calls `SourceLocationReader` to attach source-file navigation info. This is a **one-way mapper**: `SpecTest` → `TestCase`. |
| **`TestProperties`** | A static registry of four custom `TestProperty` objects (`ClassName`, `ContextMethod`, `ContextPath`, `TestDescription`). These are the "suitcase labels" — they define the named slots on a `TestCase` where the adapter stashes metadata during discovery, and reads it back during execution. Think of them as a custom schema for the adapter's data that survives the VSTest serialization/deserialization boundary between discovery and execution. |

### Discovery Layer (the "find all tests" layer)

| Class | Purpose |
|---|---|
| **`SpecsTestDiscoverer`** | The **entry point for discovery**. Implements VSTest's `ITestDiscoverer` interface. When Test Explorer scans assemblies, it calls `DiscoverTests()`. This class iterates over source assemblies, delegates to `SpecificationClassScanner` to find spec classes, then to `ContextMethodScanner` to find `[Feature]` methods, then to `ReportingLevelExtractor` to extract individual tests, and finally to `TestCaseConverter` to produce `TestCase` objects that get pushed into the `ITestCaseDiscoverySink`. It's the **orchestrator** of the discovery pipeline. |
| **`SpecificationClassScanner`** | A static utility that scans a loaded `Assembly` for all concrete (non-abstract) classes that inherit from `specs.BDD.Specifications`. It handles `ReflectionTypeLoadException` gracefully (some types may fail to load due to missing dependencies). Returns a list of `Type` objects. This is **Step 1** of discovery: "which classes contain tests?" |
| **`ContextMethodScanner`** | A static utility that scans a single `Type` for methods decorated with `[FeatureAttribute]`. Also provides `ValidateHasContextMethods()` which throws `InvalidSpecificationException` if a Specification class has zero `[Feature]` methods (a configuration error the user needs to know about). This is **Step 2** of discovery: "which methods define test structures?" |
| **`ReportingLevelExtractor`** | The **heaviest lifter** in discovery. Given a spec class and a context method, it: (1) creates a fresh instance of the class via `Activator.CreateInstance`, (2) invokes the `[Feature]` method which runs the user's fluent DSL code and populates an internal builder, (3) reads the private `coreBuilder` field via reflection to get the `IContextualTestCaseBuilder`, (4) calls `Build()` to produce the immutable test-case tree, and (5) recursively flattens that tree into a flat list of `SpecTest` objects. This is **Step 3** of discovery: "what are the actual individual tests inside each context?" |
| **`SourceLocationReader`** | A static utility that reads **portable PDB** files (debug symbols) to find the source file path and line number for a given `MethodInfo`. It looks for a `.pdb` file next to the assembly DLL, opens it with `MetadataReaderProvider`, and reads sequence points. Returns a `SourceLocation` (file path + line number) or `null`. This enables the "Go to Source" feature in Test Explorer. Degrades gracefully — never crashes discovery. |
| **`InvalidSpecificationException`** | A custom exception thrown when a class inherits from `Specifications` but has no `[Feature]` methods. Surfaced as a warning in the Test Explorer output so the user knows they have an invalid spec class. |

### Execution Layer (the "run the tests" layer)

| Class | Purpose |
|---|---|
| **`SpecsTestExecutor`** | The **entry point for execution**. Implements VSTest's `ITestExecutor` interface. Has two `RunTests` overloads: one receives specific `TestCase` objects (for "Run Selected Tests"), the other receives source paths (for "Run All Tests" — it re-runs discovery internally first). For the `TestCase` overload, it **groups** tests by `(Source, ClassName, ContextMethod, ContextPath)` and delegates each group to `TestInvoker.InvokeGroup()`. Supports cancellation via a volatile `_cancelled` flag. Contains a private `CollectingSink` class for collecting test cases during the run-all-from-sources path. |
| **`TestInvoker`** | The **core execution engine**. Has two modes: **single-test** (`Invoke`) and **grouped** (`InvokeGroup`). In both modes it: loads the assembly, resolves the type and method from metadata, creates a fresh `Specifications` instance, re-invokes the `[Feature]` method to rebuild the test hierarchy, and uses `TestFinder` to navigate to the correct node. **Single mode** runs the full lifecycle (`SetupOnce → SetupEach → Act → test → TeardownEach → TeardownOnce`) for one test. **Group mode** runs `SetupOnce` once for all tests sharing a context, then per-test runs `SetupEach → Act → test → TeardownEach`, and finally `TeardownOnce` once. It also handles error cases: if hierarchy building fails, all tests in the group are reported as failed. |
| **`TestFinder`** | A static utility that navigates the `IContextualTestCase` tree to find a specific test. Takes the root node, a context path string, and a test description. Splits the path on " → ", walks down the tree by matching child context `Description` values segment-by-segment, then finds the matching `It[...]` test by description at the target node. Returns a `(ITestCase Context, Action TestAction)` tuple or `null`. |
| **`ResultReporter`** | A static factory for VSTest `TestResult` objects. Creates `Success` results (with timing) or `Failure` results (with exception message and stack trace). Unwraps `TargetInvocationException` layers so the reported error comes from the user's code, not reflection machinery. |

---

## 2. How Each Class Relates to the Others

Here's the dependency/relationship map:

```
                    ┌─────────────────────────┐
                    │    VSTest Platform       │
                    │  (Test Explorer / CLI)   │
                    └──────┬──────────┬────────┘
                           │          │
                   Discovery│          │Execution
                           ▼          ▼
               ┌───────────────┐  ┌──────────────────┐
               │SpecsTest      │  │SpecsTestExecutor  │
               │Discoverer     │  │(ITestExecutor)     │
               │(ITestDiscoverer)│ │                    │
               └───┬───────────┘  └──┬──────────┬──────┘
                   │                 │          │
          ┌────────┼────────┐        │     (re-uses SpecsTestDiscoverer
          │        │        │        │      for run-all-from-sources)
          ▼        ▼        ▼        │
   Specification  Context  Test      │
   ClassScanner   Method   Structure │
                  Scanner  Extractor │
          │        │        │        │
          │        │        ▼        ▼
          │        │   ┌─────────┐  ┌──────────┐
          │        │   │SpecTest │  │TestInvoker│
          │        │   │ (POCO)  │  │           │
          │        │   └────┬────┘  └──┬──┬─────┘
          │        │        │          │  │
          │        │        ▼          │  │
          │        │   ┌──────────┐   │  │
          │        │   │TestCase  │◄──┘  │
          │        │   │Converter │      │
          │        │   └──┬───────┘      │
          │        │      │              │
          │        │      ▼              ▼
          │        │  ┌──────────┐  ┌──────────┐
          │        │  │TestProps  │  │TestFinder │
          │        │  └──────────┘  └──────────┘
          │        │      │              │
          │        │      │              ▼
          │        ▼      │         ┌──────────┐
          │  InvalidSpec  │         │Result    │
          │  Exception    │         │Reporter  │
          │               ▼         └──────────┘
          │         SourceLocation
          │         Reader
          ▼
     [Assembly reflection]
```

### Key relationships

- **`SpecsTestDiscoverer`** orchestrates `SpecificationClassScanner` → `ContextMethodScanner` → `TestStructureExtractor` → `TestCaseConverter` in a pipeline.
- **`ReportingLevelExtractor`** produces `SpecTest` objects. **`TestCaseConverter`** consumes them and produces VSTest `TestCase` objects.
- **`TestProperties`** is the shared contract between `TestCaseConverter` (writes metadata at discovery time) and `TestInvoker` (reads metadata at execution time). It's the glue that bridges the two phases.
- **`SpecsTestExecutor`** delegates to `TestInvoker` for the actual work, and re-uses `SpecsTestDiscoverer` when running from source paths.
- **`TestInvoker`** uses `TestFinder` to navigate the tree and `ResultReporter` to format outcomes.
- **`SourceLocationReader`** is only used by `TestCaseConverter` during discovery — execution doesn't need source locations.
- Both `ReportingLevelExtractor` (discovery) and `TestInvoker` (execution) access the private `coreBuilder` field via reflection — they both rebuild the test hierarchy, but for different purposes.

---

## 3. Input/Output Flow

### Phase 1: Discovery (Test Explorer scanning)

```
INPUT: List of assembly DLL paths (from VSTest)
                    │
                    ▼
        SpecsTestDiscoverer.DiscoverTests()
                    │
        ┌───────────┴───────────┐
        │  For each source DLL: │
        │                       │
        │  Assembly.LoadFrom()  │
        │         │             │
        │         ▼             │
        │  SpecificationClassScanner.FindSpecificationClasses(assembly)
        │    OUTPUT: IEnumerable<Type> — concrete Specifications subclasses
        │         │             │
        │         ▼             │
        │  For each spec class: │
        │                       │
        │  ContextMethodScanner.ValidateHasContextMethods(type)
        │    (throws InvalidSpecificationException if zero methods)
        │                       │
        │  ContextMethodScanner.FindContextMethods(type)
        │    OUTPUT: IEnumerable<MethodInfo> — [Feature] methods
        │         │             │
        │         ▼             │
        │  For each [Feature] method:
        │                       │
        │  ReportingLevelExtractor.ExtractTests(type, method)
        │    INTERNALLY:        │
        │    1. Activator.CreateInstance(type) → fresh Specifications instance
        │    2. method.Invoke(instance) → populates coreBuilder
        │    3. reflection read coreBuilder field → IContextualTestCaseBuilder
        │    4. builder.Build() → IContextualTestCase (immutable tree)
        │    5. Recursive flatten → SpecTest objects
        │    OUTPUT: IEnumerable<SpecTest>
        │         │             │
        │         ▼             │
        │  For each SpecTest:   │
        │                       │
        │  TestCaseConverter.ToVSTestCase(specTest, source)
        │    INTERNALLY:        │
        │    1. BuildFullyQualifiedName → "Class.DescribeMethod"  (📌 Id = SHA-256 of "Class::Method::Path::Desc")
        │    2. BuildDisplayName → "Path → Desc"
        │    3. Set TestProperties (ClassName, ContextMethod, ContextPath, TestDescription)
        │    4. SourceLocationReader.TryGetLocation → file path + line number
        │    OUTPUT: TestCase (VSTest object)
        │         │             │
        │         ▼             │
        │  discoverySink.SendTestCase(testCase) → reported to Test Explorer
        └───────────────────────┘

FINAL OUTPUT: TestCase objects visible in Test Explorer with display names,
              source navigation, and embedded metadata for execution.
```

### Phase 2: Execution — "Run Selected Tests"

```
INPUT: IEnumerable<TestCase> (from Test Explorer selection)
                    │
                    ▼
        SpecsTestExecutor.RunTests(tests, ...)
                    │
        Group tests by (Source, ClassName, ContextMethod, ContextPath)
                    │
        ┌───────────┴───────────┐
        │  For each group:      │
        │                       │
        │  TestInvoker.InvokeGroup(source, class, method, path, tests, ...)
        │    INTERNALLY:        │
        │                       │
        │    1. ResolveTypeAndMethod(source, className, methodName)
        │       → Assembly.LoadFrom, GetType, GetMethod
        │       OUTPUT: (Type, MethodInfo)
        │                       │
        │    2. BuildHierarchy(specType, contextMethod)
        │       → Activator.CreateInstance, method.Invoke, read coreBuilder, Build()
        │       OUTPUT: IContextualTestCase (root of tree)
        │                       │
        │    3. TestFinder.Find(root, contextPath, desc)
        │       → Split path by " → ", walk tree segment by segment,
        │         match test by description
        │       OUTPUT: (ITestCase sharedContext, Action testAction)
        │                       │
        │    4. sharedContext.SetupOnce()     ←── runs ONCE for entire group
        │                       │
        │    ┌──────────────────┴──────────────────┐
        │    │  For each (testCase, testAction):   │
        │    │                                      │
        │    │  frameworkHandle.RecordStart(tc)     │
        │    │  sharedContext.SetupEach()           │
        │    │  sharedContext.Act()                 │
        │    │  testAction.Invoke()   ← the It[...] assertion
        │    │  sharedContext.TeardownEach()        │
        │    │  frameworkHandle.RecordResult(       │
        │    │    ResultReporter.Success/Failure)   │
        │    └─────────────────────────────────────┘
        │                       │
        │    5. sharedContext.TeardownOnce()  ←── runs ONCE for entire group
        └───────────────────────┘

FINAL OUTPUT: TestResult objects (Pass/Fail with timing & error info)
              reported to Test Explorer via frameworkHandle.
```

### Phase 2 (alternate): Execution — "Run All Tests from Sources"

```
INPUT: IEnumerable<string> source DLL paths
                    │
                    ▼
        SpecsTestExecutor.RunTests(sources, ...)
                    │
        DiscoverFromSources(sources)
          → creates a SpecsTestDiscoverer internally
          → runs full discovery pipeline (Phase 1)
          → collects TestCase objects via CollectingSink
                    │
                    ▼
        RunTests(collectedTestCases, ...) → same as "Run Selected Tests" above
```

### Summary of the Full Round-Trip

1. **Discovery** walks the assembly → classes → methods → builds tree → flattens to `SpecTest` → converts to `TestCase` with embedded metadata → reports to VSTest.
2. **Execution** reads the metadata back from `TestCase` → reloads assembly → re-instantiates class → re-invokes `[Feature]` method → rebuilds the same tree → navigates to the specific test node → runs the lifecycle → reports results.

The key insight is that **the test hierarchy is built twice** — once during discovery (to enumerate tests) and once during execution (to run them). The `TestProperties` metadata stored on each `TestCase` is the "address" that lets execution find the same test that discovery found, without having to pass the live object graph across the VSTest process boundary.
