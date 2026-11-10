# Wiki Preferences — Specs (Project)

> Project-level preferences for the Specs BDD testing framework.
> Overrides matching sections in the global `wiki-preferences.md`.

## Global Constraints

<!-- Additive with global constraints; entries here win on conflict -->

- This project is a .NET BDD **testing framework** (a library consumed by test projects), not a service. Audience is developers writing tests with Specs.
- Do NOT generate `API-Reference.md` as an HTTP/endpoint reference. Replace it with `Usage-Guide.md` (see the `## Usage-Guide` section below) and link it from `Home.md` and `_Sidebar.md` in place of API Reference.
- Container engine, VPN, and internal-network guidance from global preferences does not apply — the framework has no container, network, or internal-registry dependencies. Omit Rancher Desktop and Zscaler from all pages.
- Do NOT generate `Configuration.md`, `Data-Layer.md`, `Background-Processing.md`, or `Authentication-&-Authorization.md`. Runner settings (`[appname].testconfig.json`, `SpecsBDD__*` environment variables) belong in `Usage-Guide.md`.
- Prefer the method-based `Specifications` API in all examples. Mention the legacy property-based `Spec` class only where needed for completeness.
- All code samples must be compilable C# drawn from or consistent with the repo (e.g. `tests/specs.BDD.IntegrationTests/`, `src/specs.BDD.Templates/templates/`). Do not invent APIs.

## Home

<!-- Applies to the introduction ("What Is This?") on Home.md -->

### Introduction Tone

- Use the same informal, developer-to-developer voice as the Usage Guide.
- Help the reader grasp the intent quickly by relating Specs to popular frameworks they likely know:
  - **RSpec** (Ruby) and **Jasmine / Jest / Mocha** (JavaScript) — the `describe` / `context` / `it` style Specs brings to .NET.
  - **NSpec** — the closest .NET ancestor; Specs keeps its grammar and adds a modern core, two runners, and first-class async.
  - **xUnit / NUnit / MSTest** — what Specs is *not*: flat test methods with a single setup/teardown.
  - **SpecFlow / Reqnroll** — Gherkin-based BDD; Specs is pure C#/VB with no `.feature` files or glue code.
- Keep comparisons factual and brief (a sentence or a short table); don't disparage other frameworks.
- Show the intro sample in both C# and VB.

## Getting-Started

<!-- Replaces global Getting-Started section; this page is for contributors building the repo -->

### Additional Prerequisites

None beyond the .NET SDK pinned in `global.json`.

### Audience Note

Getting-Started targets **contributors** (clone, build, run the repo's own tests). Consumers wanting to use the framework should be directed to [Usage Guide](Usage-Guide) from the top of the page.

## Usage-Guide

<!-- Replaces API-Reference.md. Generate as documents/wiki/Usage-Guide.md -->

Structure the page as progressive disclosure: a quick launch first, then increasing depth. Every section pairs a short explanation with a code sample.

### Audience & Tone

- The audience is strictly **developers writing tests** with Specs. Do NOT include anything about code internals: no framework class names beyond the authoring API (`Specifications`, `[Feature]`, `[ReportingLevel]`, `ReportingLevel`, the lifecycle methods and indexers, `TODO` / `TODOAsync`, `ShouldThrow<T>`), no source-file links into `src/`, no builder/executor/adapter/runner-component discussion, and no "consumed by" columns.
- Write informally, like one developer explaining things to another over coffee: second person ("you"), contractions, short sentences, the occasional light aside. Stay accurate and concise — friendly, not jokey or padded.
- Assume every Specs NuGet package (`specs.BDD.VSTest`, `specs.BDD.MTP`, `specs.BDD.Templates`, and their dependencies) is published and installable from nuget.org. Do not include local-pack, local-feed, or "before the first release" workarounds.

### Visual Basic

- Show every authoring code sample in both C# and VB.NET, C# first, as adjacent fenced blocks (` ```csharp ` then ` ```vb `) under the same explanation.
- VB samples must be idiomatic and compilable, and consistent with the VB templates in `src/specs.BDD.Templates/templates/specs-vstest-vb/` and `specs-mtp-vb/` (e.g. `Inherits Specifications`, `<Feature("...")>`, `Sub()` / `Async Function()` lambdas, indexer assignment syntax).
- Command-line samples cover VB where it differs (e.g. `dotnet new specs-vstest -lang VB`).
- Project-file samples show both `.csproj` and `.vbproj` only where they differ.

### 1. Quick Launch

- Fastest path from zero to a passing spec in under a minute.
- Use the `dotnet new` templates from `specs.BDD.Templates` (`specs-vstest`, `specs-mtp`; C# and VB variants), then `dotnet test`.
- Show one minimal `Specifications` class with a single `[Feature]`, `Act`, and `It`.

### 2. Installation

Cover choosing a test host first (table: VSTest via `specs.BDD.VSTest` vs Microsoft.Testing.Platform via `specs.BDD.MTP`, with when to pick each), then:

- **CLI** — `dotnet new install` for templates, `dotnet add package` for existing projects, and the MTP `global.json` `test.runner` setting for `dotnet test` on .NET 10.
- **Visual Studio** — create/open the test project, add the package via NuGet Package Manager, build, run and debug from Test Explorer, and describe how the spec tree nests (namespace › class › `[Feature]` › `describe → context → spec`).
- **VS Code** — required extension (C# Dev Kit), add the package, run and debug from the Testing view, and note how its tree differs from Visual Studio's.

### 3. Core Concepts

- The **Suite → Feature Set → Scenario → Specification** hierarchy and how it maps to classes, `[Feature]` methods, `Describe`/`Context`, and `It`.
- Why a `[Feature]` method is required for discovery.

### 4. Writing Specs (progressive)

Each subsection builds on the previous example:

1. `Act` + `It` assertions
2. Nested `Describe` / `Context` scenarios
3. Lifecycle hooks — `BeforeAll`, `BeforeEach`, `Act`, `AfterEach`, `AfterAll`; execution order (include a sequence diagram) and the once-per-context guarantee for partial runs
4. Async specs and hooks (`ItAsync`, async lifecycle hooks)
5. Expected exceptions — `ShouldThrow<T>()` with and without a verifier
6. Pending specs — `TODO`, `TODOAsync`, `xIt`, `xItAsync`, `xContext`, `xDescribe`, and how they report
7. Assertions — Specs has no built-in assertion library; any exception fails a spec, so use your favorite library (e.g. NFluent) or plain exceptions

### 5. Running and Filtering

- `dotnet test` basics for each host
- VSTest filtering (`--filter IsPending=true|false`)
- MTP tree-node paths and `--treenode-filter` examples, including escaping rules
- Reporting levels — `[Feature(Level = ReportingLevel.Feature)]` and `[ReportingLevel]`

### 6. Runner Settings (MTP)

- `[appname].testconfig.json` `SpecsBDD` section and `SpecsBDD__*` environment variables
- Table of each setting, its effect, and default (no consuming-class column)
- The VSTest `.runsettings` equivalents

### 7. Cheat Sheet

- One-page table of every authoring keyword/method with a one-line description and link back to its section.

## Deployment

<!-- Replaces global Deployment section -->

### Access Requirements

Not applicable — deliverables are NuGet packages (`specs.core`, `specs.BDD`, `specs.BDD.VSTest`, `specs.BDD.MTP`, templates). Document packaging/versioning from `Directory.Build.props`, `Directory.Packages.props`, and any CI workflows found; omit Azure, container registry, DNS, and certificate content.

## Troubleshooting

<!-- Replaces global Troubleshooting section -->

### Additional Issues

Focus on consumer-facing issues: specs not discovered (missing `[Feature]`, wrong package for the host), MTP not used by `dotnet test` (missing `global.json` runner setting), multiple solutions at repo root requiring an explicit solution argument, and Test Explorer nesting differences between Visual Studio and VS Code.
