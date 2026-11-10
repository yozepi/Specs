# Architecture

> A layered .NET library: a framework-agnostic core, a BDD authoring API on top of it, and two thin runners (VSTest and Microsoft.Testing.Platform) that share one execution core.

## System Overview

Specs is consumed as NuGet packages by **test projects**. A test author derives a class from `Specifications`, declares `[Feature]` methods, and writes `Describe` / `Context` / `It` blocks inside them. A test host (VSTest or Microsoft.Testing.Platform) loads one of the Specs runners, which reflects over the test assembly, builds an immutable tree of contexts and specs, and executes the selected specs with the five-phase lifecycle.

The system has two layers and two runners:

- **`specs.core`** — the builder, the immutable test-case tree, and `TestCaseExecutor`, the single implementation of the lifecycle. No third-party runtime dependencies; its executor and adapter interfaces are public so other runners can host the same lifecycle.
- **`specs.BDD`** — the `Specifications` base class and the `[Feature]` / `[ReportingLevel]` attributes. It translates `BeforeEach(...)`, `It[...] = ...`, and friends into builder calls.
- **`specs.BDD.VSTestAdapter`** (package `specs.BDD.VSTest`) and **`specs.BDD.TestingPlatform`** (package `specs.BDD.MTP`) — host-specific discovery and result reporting. Everything runner-agnostic (suite-tree building, root lifecycle, grouped / pending routing, source locations) lives in `src/shared/specs.BDD.Runner/` and is compiled into both runners as linked source.

A fifth, content-only package, **`specs.BDD.Templates`**, ships the `dotnet new specs-vstest` / `specs-mtp` templates.

## C4 Level 1 — System Context

```mermaid
---
title: "C4 Level 1 — System Context: Specs"
---
%%{init: {'theme': 'neutral', 'flowchart': {'nodeSpacing': 30, 'rankSpacing': 45, 'wrappingWidth': 160}}}%%
flowchart TD
    Dev(["<b>Test author</b><br/>[Person]<br/>Writes and runs specs"]):::external

    subgraph TOOLS["IDEs and CLI"]
        direction LR
        IDE["<b>Visual Studio / VS Code</b><br/>[Software System: IDE]<br/>Test Explorer, Testing view"]:::external
        CLI["<b>dotnet CLI</b><br/>[Software System: .NET SDK]<br/>dotnet test / new / run"]:::external
    end

    subgraph HOSTS["Test hosts"]
        direction LR
        VST["<b>VSTest Platform</b><br/>[Software System: Microsoft]<br/>Loads test adapters"]:::external
        MTP["<b>Microsoft.Testing.Platform</b><br/>[Software System: Microsoft]<br/>Test app as executable"]:::external
    end

    SPECS["<b>Specs</b><br/>[Software System: NuGet packages]<br/>BDD API + runners"]:::app
    USR["<b>User test project</b><br/>[Software System: .NET assembly]<br/>Specifications suites"]:::external
    NUGET["<b>nuget.org</b><br/>[Software System: NuGet]<br/>Distributes packages"]:::external

    Dev -->|"Runs tests [UI / CLI]"| TOOLS
    Dev -->|"Authors specs [C#]"| USR
    TOOLS -->|"Starts runs [VSTest / MTP]"| HOSTS
    HOSTS -->|"Loads runner [host API]"| SPECS
    SPECS -->|"Discovers + runs [reflection]"| USR
    USR -.->|"Restores packages [NuGet]"| NUGET

    subgraph Legend
        direction TB
        L1["App-owned"]:::app
        L2["External / Person"]:::external
    end

    NUGET ~~~ Legend

    classDef app fill:#56B870,stroke:#3D8B50,color:#fff
    classDef external fill:#E8A838,stroke:#B07D20,color:#fff
```

| Actor / System | Relationship to Specs |
|----------------|----------------------|
| Test author | Writes `Specifications` suites; runs them from an IDE or the CLI |
| Visual Studio / VS Code | Show discovered specs; run and debug them through the host |
| VSTest Platform | Loads `specs.BDD.VSTestAdapter.dll` (`ITestDiscoverer` + `ITestExecutor`) |
| Microsoft.Testing.Platform | Runs the test project as an executable with the Specs test framework registered via `AddSpecsBDD()` |
| nuget.org | Distribution channel for all five packages (published by the release workflow) |

## C4 Level 2 — Container

```mermaid
---
title: "C4 Level 2 — Container: Specs"
---
%%{init: {'theme': 'neutral', 'flowchart': {'nodeSpacing': 30, 'rankSpacing': 45, 'wrappingWidth': 160}}}%%
flowchart TD
    subgraph HOSTS["Test hosts"]
        direction LR
        VST["<b>VSTest Platform</b><br/>[Software System: Microsoft]<br/>Discovery + execution"]:::external
        MTP["<b>Microsoft.Testing.Platform</b><br/>[Software System: Microsoft]<br/>Self-hosted test app"]:::external
    end

    USR["<b>User test assembly</b><br/>[Software System: .NET assembly]<br/>Specifications subclasses"]:::external
    DNN(["<b>dotnet new</b><br/>[Software System: .NET SDK]<br/>Scaffolds projects"]):::external

    subgraph SPECS["System: Specs"]
        direction TB
        ADP["<b>specs.BDD.VSTestAdapter</b><br/>[Container: netstandard2.1]<br/>Package specs.BDD.VSTest"]:::app
        TP["<b>specs.BDD.TestingPlatform</b><br/>[Container: netstandard2.1]<br/>Package specs.BDD.MTP"]:::app
        TPL["<b>specs.BDD.Templates</b><br/>[Container: template package]<br/>specs-vstest, specs-mtp"]:::app
        BDD["<b>specs.BDD</b><br/>[Container: netstandard2.1]<br/>Describe / Context / It API"]:::app
        CORE["<b>specs.core</b><br/>[Container: netstandard2.1]<br/>Builder, tree, executor"]:::app
    end

    VST -->|"Discover + run [VSTest contracts]"| ADP
    MTP -->|"Discover + run [MTP API]"| TP
    USR -->|"Authors specs [C# API]"| BDD
    DNN -->|"Reads templates [nupkg]"| TPL
    ADP -->|"Builds suite trees [internal API]"| BDD
    TP -->|"Builds suite trees [internal API]"| BDD
    ADP -->|"Runs lifecycle [public API]"| CORE
    TP -->|"Runs lifecycle [public API]"| CORE
    BDD -->|"Records actions [builder]"| CORE

    subgraph Legend
        direction TB
        L1["App-owned"]:::app
        L2["External"]:::external
    end

    CORE ~~~ Legend

    classDef app fill:#56B870,stroke:#3D8B50,color:#fff
    classDef external fill:#E8A838,stroke:#B07D20,color:#fff
```

| Container | Folder | Package ID | Runtime dependencies |
|-----------|--------|------------|----------------------|
| `specs.core` | [src/specs.core](../../src/specs.core) | `specs.core` | None |
| `specs.BDD` | [src/specs.specification](../../src/specs.specification) | `specs.BDD` | `specs.core` |
| `specs.BDD.VSTestAdapter` | [src/specs.BDD.VSTestAdapter](../../src/specs.BDD.VSTestAdapter) | `specs.BDD.VSTest` | `specs.BDD`, Microsoft.TestPlatform.ObjectModel, System.Reflection.Metadata |
| `specs.BDD.TestingPlatform` | [src/specs.BDD.TestingPlatform](../../src/specs.BDD.TestingPlatform) | `specs.BDD.MTP` | `specs.BDD`, Microsoft.Testing.Platform (+ MSBuild), System.Reflection.Metadata |
| `specs.BDD.Templates` | [src/specs.BDD.Templates](../../src/specs.BDD.Templates) | `specs.BDD.Templates` | None (content only) |

> The VSTest assembly keeps the name `specs.BDD.VSTestAdapter.dll` because VSTest only loads assemblies ending in `TestAdapter.dll`; only the package ID differs.

## C4 Level 3 — Components

### specs.core

```mermaid
---
title: "C4 Level 3 — Component: specs.core"
---
%%{init: {'theme': 'neutral', 'flowchart': {'nodeSpacing': 30, 'rankSpacing': 45, 'wrappingWidth': 160}}}%%
flowchart TD
    CALLER["<b>specs.BDD / custom front-end</b><br/>[Container: caller]<br/>Declares contexts and specs"]:::external
    HOST["<b>Runner</b><br/>[Container: VSTest / MTP / custom]<br/>Selects and runs specs"]:::external

    subgraph CORE["Container: specs.core"]
        direction TB
        CB["<b>ContextualTestCaseBuilder</b><br/>[Component: public class]<br/>Accumulates lifecycle + specs"]:::app
        TREE[("<b>ContextualTestCase</b><br/>[Component: immutable tree]<br/>Contexts, specs, pending")]:::data
        EXE["<b>TestCaseExecutor</b><br/>[Component: public class]<br/>Per-node lifecycle loop"]:::app
        AEC["<b>ActExceptionContext</b><br/>[Component: static class]<br/>Captures Act exception"]:::app
        ADPI["<b>Adapter interfaces</b><br/>[Component: public interfaces]<br/>Context, provider, logging"]:::app
    end

    CALLER -->|"Add* / ChildTestCase"| CB
    CB ==>|"Build()"| TREE
    HOST -->|"Reads nodes"| TREE
    HOST -->|"Implements"| ADPI
    HOST -->|"ProcessTestCase(token)"| EXE
    EXE -->|"Drives phases"| ADPI
    EXE -->|"Run(act, test)"| AEC

    subgraph Legend
        direction TB
        L1["App-owned"]:::app
        L2[("Immutable data")]:::data
        L3["Caller / host"]:::external
    end

    AEC ~~~ Legend

    classDef app fill:#56B870,stroke:#3D8B50,color:#fff
    classDef data fill:#9B6DC6,stroke:#6E4D8C,color:#fff
    classDef external fill:#E8A838,stroke:#B07D20,color:#fff
```

| Component | Responsibility | Source |
|-----------|---------------|--------|
| `ContextualTestCaseBuilder` / `IContextualTestCaseBuilder` | Fluent `AddSetupOnce`, `AddSetupEach`, `AddAct`, `AddTest`, `AddPendingTest`, `AddTestLocation`, `AddTeardownEach`, `AddTeardownOnce`, `ChildTestCase`, `Build` | [ContextualTestCaseBuilder.cs](../../src/specs.core/Framework/Processors/TestCases/ContextualTestCaseBuilder.cs) |
| `ContextualTestCase` / `ITestCase` | Immutable node: cascaded and own once-actions, `Tests`, `PendingTests`, `TestLocations`, `ChildContexts` | [ITestCase.cs](../../src/specs.core/Framework/Processors/TestCases/ITestCase.cs) |
| `TestCaseExecutor` | `SetupOnce` → per test (`SetupEach` → `Act` + test → `TeardownEach`) → `TeardownOnce`; honours `StopOnAssertFailure` and a `CancellationToken` | [TestCaseExecutor.cs](../../src/specs.core/Framework/Processors/TestExecutors/TestCaseExecutor.cs) |
| `ActExceptionContext` | Runs `Act` + test together; an `Act` exception fails only that spec unless a `ShouldThrow<T>` verifier handles it | [ActExceptionContext.cs](../../src/specs.core/Framework/Processors/TestExecutors/ActExceptionContext.cs) |
| `ITestContextAdapter`, `ITestProviderAdapter`, `ILoggingAdapter`, `TestContextAdapter`, `TestInstance` | Host-supplied lifecycle, selected tests, and result logging | [Adapters/](../../src/specs.core/Framework/Adapters) |

### specs.BDD

```mermaid
---
title: "C4 Level 3 — Component: specs.BDD"
---
%%{init: {'theme': 'neutral', 'flowchart': {'nodeSpacing': 30, 'rankSpacing': 45, 'wrappingWidth': 160}}}%%
flowchart TD
    USR["<b>User suite</b><br/>[Software System: user code]<br/>Derives from Specifications"]:::external

    subgraph BDDC["Container: specs.BDD"]
        direction TB
        SP["<b>Specifications</b><br/>[Component: abstract class]<br/>Lifecycle methods, indexers"]:::app
        CTX["<b>ContextBuilder</b><br/>[Component: indexer class]<br/>Describe / Context / x*"]:::app
        AB["<b>AssertBuilder / Async</b><br/>[Component: IAssertProvider]<br/>It / ItAsync / x*"]:::app
        ATTR["<b>FeatureAttribute, ReportingLevel</b><br/>[Component: metadata]<br/>Discovery + grouping"]:::app
    end

    CB["<b>IContextualTestCaseBuilder</b><br/>[Component: specs.core]<br/>Current scope builder"]:::app

    USR -->|"Calls API [C#]"| SP
    USR -->|"Decorates methods"| ATTR
    SP -->|"Opens scopes"| CTX
    SP -->|"Registers specs"| AB
    CTX -->|"ChildTestCase + swap scope"| CB
    AB -->|"AddTest / AddPendingTest"| CB
    SP -->|"Add* lifecycle"| CB

    subgraph Legend
        direction TB
        L1["App-owned"]:::app
        L2["User code"]:::external
    end

    CB ~~~ Legend

    classDef app fill:#56B870,stroke:#3D8B50,color:#fff
    classDef external fill:#E8A838,stroke:#B07D20,color:#fff
```

`Specifications` keeps a pointer to the builder of the currently open scope. Opening `Describe[...]` / `Context[...]` swaps the pointer to a child builder while the block runs and restores it afterwards, so `BeforeEach(...)` or `It[...]` inside a block attach to that block. See [Specifications.cs](../../src/specs.specification/Specifications.cs).

### Shared runner core (`specs.BDD.Runner`)

Linked source in [src/shared/specs.BDD.Runner](../../src/shared/specs.BDD.Runner), compiled (as `internal` types) into both runners. It has no `.csproj`.

```mermaid
---
title: "C4 Level 3 — Component: shared runner core"
---
%%{init: {'theme': 'neutral', 'flowchart': {'nodeSpacing': 30, 'rankSpacing': 45, 'wrappingWidth': 160}}}%%
flowchart TD
    RUN["<b>Runner host</b><br/>[Container: VSTest / MTP]<br/>Maps selection to items"]:::external

    subgraph SHARED["Linked source: specs.BDD.Runner"]
        direction TB
        STB["<b>SuiteTreeBuilder</b><br/>[Component: static class]<br/>One tree per spec class"]:::app
        SR["<b>SuiteRunner</b><br/>[Component: static class]<br/>Root lifecycle + routing"]:::app
        SW["<b>SelectionTreeWalker&lt;T&gt;</b><br/>[Component: generic class]<br/>Once per live node"]:::app
        GSR["<b>GroupedSubtreeRunner</b><br/>[Component: static class]<br/>One report per Feature"]:::app
        LIS["<b>ISpecRunListener&lt;T&gt;</b><br/>[Component: interface]<br/>Start / result / log events"]:::app
    end

    EXE["<b>TestCaseExecutor</b><br/>[Component: specs.core]<br/>Per-test loop"]:::app

    RUN -->|"BuildSuite(type)"| STB
    RUN -->|"Run(root, items)"| SR
    SR -->|"Regular items"| SW
    SR -->|"Grouped items"| GSR
    SW -->|"Per-node batch"| EXE
    SW -.->|"Reports"| LIS
    GSR -.->|"GroupedReport"| LIS

    subgraph Legend
        direction TB
        L1["App-owned"]:::app
        L2["Runner host"]:::external
    end

    EXE ~~~ Legend

    classDef app fill:#56B870,stroke:#3D8B50,color:#fff
    classDef external fill:#E8A838,stroke:#B07D20,color:#fff
```

| Component | Responsibility |
|-----------|---------------|
| `SpecificationClassScanner`, `ContextMethodScanner` | Find non-abstract `Specifications` subclasses and their `[Feature]` methods; a class with none raises `InvalidSpecificationException` |
| `SuiteTreeBuilder` | Instantiates the suite once and registers each `[Feature]` method as a named child context (description, or method name when none). Used by both discovery and execution |
| `SuiteRunner` | Splits a selection into grouped / pending / regular items; runs the root (constructor-registered) `BeforeAll` / `AfterAll` once per class |
| `SelectionTreeWalker<TTest>` | Depth-first walk that fires each live node's own `BeforeAll` / `AfterAll` exactly once and delegates the per-test loop to `TestCaseExecutor` |
| `GroupedSubtreeRunner` | Runs a whole `[Feature]` subtree and returns a `GroupedReport` (NSpec-style tree, tallies, failures) |
| `GroupingSettings` | Resolves the effective `ReportingLevel` and marker style |
| `PendingSpecs` | `IsPending`, `HasRunnableTests`, `HasAnyTests` over a built tree |
| `SourceLocationReader` | Portable-PDB lookups for "Go to Source"; falls back to the declared `It[...]` line |
| `TestFinder`, `ContextPathFormat` | Re-locate a spec by its context path (separator `" → "`) |

### specs.BDD.VSTestAdapter

```mermaid
---
title: "C4 Level 3 — Component: specs.BDD.VSTestAdapter"
---
%%{init: {'theme': 'neutral', 'flowchart': {'nodeSpacing': 30, 'rankSpacing': 45, 'wrappingWidth': 160}}}%%
flowchart TD
    VST["<b>VSTest Platform</b><br/>[Software System: Microsoft]<br/>Discovery and run requests"]:::external

    subgraph ADP["Container: specs.BDD.VSTestAdapter"]
        direction TB
        DISC["<b>SpecsTestDiscoverer</b><br/>[Component: ITestDiscoverer]<br/>Scans .dll / .exe sources"]:::app
        FLAT["<b>SuiteTreeFlattener</b><br/>[Component: static class]<br/>Tree to SpecTest records"]:::app
        CONV["<b>TestCaseConverter</b><br/>[Component: static class]<br/>SpecTest to TestCase"]:::app
        EXEC["<b>SpecsTestExecutor</b><br/>[Component: ITestExecutor]<br/>Filter, group by class"]:::app
        INV["<b>TestInvoker</b><br/>[Component: static class]<br/>Builds suite, maps items"]:::app
        VRL["<b>VSTestRunListener</b><br/>[Component: ISpecRunListener]<br/>Events to TestResults"]:::app
    end

    SHR["<b>Shared runner core</b><br/>[Component: linked source]<br/>SuiteRunner and friends"]:::app

    VST -->|"DiscoverTests"| DISC
    DISC -->|"Flatten(type)"| FLAT
    FLAT -->|"SpecTest list"| CONV
    CONV -->|"SendTestCase"| VST
    VST -->|"RunTests"| EXEC
    EXEC -->|"InvokeSuite per class"| INV
    INV -->|"Run(root, items)"| SHR
    SHR -.->|"Events"| VRL
    VRL -->|"RecordResult [IFrameworkHandle]"| VST

    subgraph Legend
        direction TB
        L1["App-owned"]:::app
        L2["External"]:::external
    end

    SHR ~~~ Legend

    classDef app fill:#56B870,stroke:#3D8B50,color:#fff
    classDef external fill:#E8A838,stroke:#B07D20,color:#fff
```

- Each spec becomes a VSTest `TestCase` whose `FullyQualifiedName` is `Class.FeatureMethod` (shared by all specs of a `[Feature]`, so IDEs attach CodeLens to the method). Uniqueness comes from a deterministic `TestCase.Id` (SHA-256 of source + spec address).
- `TestCase.Hierarchy` is `[assembly, namespace, class, [Feature]]`; the display name carries the rest of the path (`Feature → context → spec`).
- Custom `TestProperties` (`ClassName`, `ContextMethod`, `ContextPath`, `TestDescription`, `IsGrouped`, `IsPending`) survive VSTest serialization and let execution re-find each spec. They are also the `--filter` properties.

### specs.BDD.TestingPlatform

```mermaid
---
title: "C4 Level 3 — Component: specs.BDD.TestingPlatform"
---
%%{init: {'theme': 'neutral', 'flowchart': {'nodeSpacing': 30, 'rankSpacing': 45, 'wrappingWidth': 160}}}%%
flowchart TD
    MTP["<b>Microsoft.Testing.Platform</b><br/>[Software System: Microsoft]<br/>Discover / run requests"]:::external

    subgraph TP["Container: specs.BDD.TestingPlatform"]
        direction TB
        HOOK["<b>AddSpecsBDD / BuilderHook</b><br/>[Component: extension methods]<br/>Registers framework"]:::app
        STF["<b>SpecsTestFramework</b><br/>[Component: ITestFramework]<br/>Handles requests"]:::app
        TREE["<b>SpecTreeBuilder</b><br/>[Component: SpecNode tree]<br/>Tree paths as UIDs"]:::app
        SEL["<b>SpecSelection</b><br/>[Component: C# class]<br/>Tree-node filter, UIDs"]:::app
        MRL["<b>MtpRunListener</b><br/>[Component: ISpecRunListener]<br/>TestNode updates"]:::app
        CTR["<b>ConsoleTreeReport</b><br/>[Component: output]<br/>Indented console tree"]:::app
    end

    SHR["<b>Shared runner core</b><br/>[Component: linked source]<br/>SuiteRunner and friends"]:::app

    MTP -->|"Calls at startup"| HOOK
    MTP -->|"ExecuteRequestAsync"| STF
    STF -->|"Builds nodes"| TREE
    STF -->|"Filters"| SEL
    STF -->|"Run(root, items)"| SHR
    SHR -.->|"Events"| MRL
    MRL -->|"Publishes [message bus]"| MTP
    STF -->|"CLI mode only"| CTR

    subgraph Legend
        direction TB
        L1["App-owned"]:::app
        L2["External"]:::external
    end

    SHR ~~~ Legend

    classDef app fill:#56B870,stroke:#3D8B50,color:#fff
    classDef external fill:#E8A838,stroke:#B07D20,color:#fff
```

- Every node's UID is its tree path: `/<assembly>/<namespace>/<class>/<Feature>/<context>/…/<spec>`. Group nodes end in `/` so a group and a spec with the same name never collide. Segments escape `%` → `%25` and `/` → `%2F`.
- IDE clients (`--server` / `--server jsonrpc`) receive group nodes with parent UIDs; `dotnet test` and console runs get flat full-path names plus a `ConsoleTreeReport`.
- `build/specs.BDD.MTP.props` (packed into the NuGet `build/` folder) sets `OutputType=Exe`, `IsTestProject`, `IsTestingPlatformApplication`, and registers `TestingPlatformBuilderHook`, so the MTP MSBuild package generates an entry point that calls `AddSpecsBDD()`.

## Dynamic View — Running a Selection

The same flow applies to both runners; only the entry point and the listener differ.

```mermaid
---
title: "Dynamic — Running selected specs (VSTest)"
---
%%{init: {'theme': 'neutral'}}%%
sequenceDiagram
    autonumber
    participant VST as VSTest Platform [Software System]
    participant EXR as SpecsTestExecutor [Component]
    participant INV as TestInvoker [Component]
    participant SB as SuiteTreeBuilder [Component]
    participant SR as SuiteRunner [Component]
    participant SW as SelectionTreeWalker [Component]
    participant EXE as TestCaseExecutor [Component]
    Note over VST,EXE: Legend — [Software System]=external host, [Component]=in-process class
    VST->>EXR: RunTests(testCases or sources + filter)
    EXR->>EXR: Apply --filter, group by (source, class)
    loop per spec class
        EXR->>INV: InvokeSuite(class, tests)
        INV->>SB: BuildSuite(specType)
        SB-->>INV: immutable root tree
        INV->>SR: Run(root, items, VSTestRunListener)
        SR->>SR: Record pending items as Skipped
        SR->>SR: root BeforeAll (once)
        SR->>SW: Walk regular items
        loop per live node (depth-first)
            SW->>SW: node BeforeAll (once)
            SW->>EXE: ProcessTestCase(token)
            EXE-->>VST: results via listener
            SW->>SW: node AfterAll (once)
        end
        SR->>SR: root AfterAll (best-effort)
    end
```

Discovery and execution each build the tree independently (VSTest serializes `TestCase` objects between the two phases). Because both go through `SuiteTreeBuilder`, the context path recorded at discovery always resolves at execution.

## Packaging View

```mermaid
---
title: "Deployment — NuGet package dependencies"
---
%%{init: {'theme': 'neutral', 'flowchart': {'nodeSpacing': 30, 'rankSpacing': 45, 'wrappingWidth': 160}}}%%
flowchart TD
    subgraph RUNNERS["Runner packages (pick one)"]
        direction LR
        PV["<b>specs.BDD.VSTest</b><br/>[Container: NuGet package]<br/>VSTest adapter"]:::app
        PM["<b>specs.BDD.MTP</b><br/>[Container: NuGet package]<br/>MTP framework + props"]:::app
    end

    PB["<b>specs.BDD</b><br/>[Container: NuGet package]<br/>Authoring API"]:::app
    PC["<b>specs.core</b><br/>[Container: NuGet package]<br/>Zero dependencies"]:::app
    PT["<b>specs.BDD.Templates</b><br/>[Container: template package]<br/>dotnet new templates"]:::app

    subgraph MS["Microsoft packages"]
        direction LR
        OM["<b>TestPlatform.ObjectModel</b><br/>[Software System: NuGet 17.8]<br/>VSTest contracts"]:::external
        MP["<b>Microsoft.Testing.Platform</b><br/>[Software System: NuGet 2.5.1]<br/>MTP API + MSBuild"]:::external
    end

    PV -->|"Depends on"| PB
    PM -->|"Depends on"| PB
    PB -->|"Depends on"| PC
    PV -->|"Depends on"| OM
    PM -->|"Depends on"| MP
    PT -.->|"Scaffolds a reference to"| RUNNERS

    subgraph Legend
        direction TB
        L1["Specs package"]:::app
        L2["Third-party package"]:::external
    end

    PC ~~~ Legend

    classDef app fill:#56B870,stroke:#3D8B50,color:#fff
    classDef external fill:#E8A838,stroke:#B07D20,color:#fff
```

All packages are versioned together from git tags and published by the release workflow. See [Deployment](Deployment).

## Project Structure

```
Specs/
├── Specs.sln                        — main solution (VSTest mode): 5 deliverables + 4 test projects
├── Specs.Mtp.slnx                   — MTP solution: core, BDD, MTP runner + 2 MTP test projects
├── global.json                      — SDK 10.0.400 pin
├── Directory.Build.props            — shared MSBuild settings, package metadata, MinVer
├── Directory.Packages.props         — central package versions
├── .github/workflows/               — ci.yml, release.yml
├── src/
│   ├── specs.core/                  — framework-agnostic core
│   ├── specs.specification/         — specs.BDD authoring API (folder name differs from assembly)
│   ├── specs.BDD.VSTestAdapter/     — VSTest runner (Discovery/, Conversion/, Execution/)
│   ├── specs.BDD.TestingPlatform/   — MTP runner (Discovery/, Execution/, Output/, build/)
│   ├── specs.BDD.Templates/         — dotnet new templates (content only)
│   └── shared/specs.BDD.Runner/     — linked source compiled into both runners
└── tests/
    ├── Directory.Build.props        — IsPackable=false for test projects
    ├── specs.core.UnitTests/        — self-hosted (Specs)
    ├── specs.BDD.UnitTests/         — self-hosted (Specs)
    ├── specs.BDD.VSTestAdapter.Tests/ — NUnit + Moq
    ├── specs.BDD.IntegrationTests/  — end-to-end through VSTest
    └── mtp/
        ├── global.json              — switches dotnet test to MTP mode
        ├── specs.BDD.TestingPlatform.Tests/            — NUnit on MTP
        └── specs.BDD.TestingPlatform.IntegrationTests/ — same specs as the VSTest integration project
```

## Key Design Decisions

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Immutable test-case tree after `Build()` | Predictable, shareable, safe to execute repeatedly | Mutable runtime state (the NSpec model) |
| `Action`-based public surface; async hooks run via `GetAwaiter().GetResult()` | One uniform API for sync and async | A parallel async-only pipeline |
| `TestCaseExecutor` is the single lifecycle implementation and is public | Every runner, including third-party ones, gets identical semantics | Re-implementing the lifecycle in each adapter |
| `[Feature]` methods are registered as named child contexts | They become tree nodes that can own `BeforeAll` / `AfterAll` | Invoking `[Feature]` methods directly |
| Runner-agnostic code shared as linked source, not a separate assembly | Avoids a sixth package and a public API for implementation details | A shared `specs.BDD.Runner` package |
| MTP identifies nodes by tree path | Stable across discovery/execution, readable, and what `--treenode-filter` matches | Opaque generated IDs |
| Pending is tree metadata in the core; skipping is runner policy | The builder knows `xContext` inheritance; reporting is host-specific | Teaching `TestCaseExecutor` about pending |

The full history is in the [Decision Log](Decision-Log).

## Related Pages

- [Design Patterns](Design-Patterns)
- [Usage Guide](Usage-Guide)
- [Testing](Testing)
- [Deployment](Deployment)
