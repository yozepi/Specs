# Getting Started

> Clone, build, and test the Specs repository. This page is for **contributors** working on the framework itself.

> **Want to use Specs in your own tests?** Go to the [Usage Guide](Usage-Guide) instead — it covers installing the packages, scaffolding a project, and writing specs.

## Prerequisites

| # | Requirement | Version | Needed By | Install |
|---|-------------|---------|-----------|---------|
| 1 | Git | Any recent | Cloning; MinVer reads git tags to compute package versions | [git-scm.com](https://git-scm.com/) |
| 2 | .NET SDK | 10.0.400+ (`rollForward: latestFeature`, no prereleases) | Building every project; pinned in [global.json](../../global.json) | [dot.net](https://dotnet.microsoft.com/download/dotnet/10.0) |
| 3 | IDE (optional) | See [IDE Setup](#ide-setup) | Running and debugging specs interactively | — |

No other tooling is required: the framework has no database, container, network, or secret dependencies.

## Clone & Build

```powershell
git clone https://github.com/yozepi/Specs.git
cd Specs
dotnet restore Specs.sln
dotnet build Specs.sln
```

The repo root holds two solutions (`Specs.sln` and `Specs.Mtp.slnx`), so every root-level `dotnet` command must name one. A bare `dotnet build` fails with `MSB1011`.

| Solution | Contents | `dotnet test` mode |
|----------|----------|--------------------|
| `Specs.sln` | All five `src/` projects + the four VSTest-hosted test projects | VSTest |
| `Specs.Mtp.slnx` | `specs.core`, `specs.BDD`, `specs.BDD.TestingPlatform` + the two MTP test projects | Microsoft.Testing.Platform (via [tests/mtp/global.json](../../tests/mtp/global.json)) |

## Configuration

None. The build reads shared settings from files in the repo:

| File | Purpose |
|------|---------|
| [global.json](../../global.json) | SDK version pin |
| [Directory.Build.props](../../Directory.Build.props) | Nullable, implicit usings, XML docs, NuGet metadata, symbols, MinVer |
| [Directory.Packages.props](../../Directory.Packages.props) | Central package versions — do **not** put `Version=` on a `PackageReference` |
| [tests/Directory.Build.props](../../tests/Directory.Build.props) | Marks test projects non-packable and turns XML docs off |

## Running Tests

### VSTest-hosted suites

```powershell
dotnet test Specs.sln
```

### MTP-hosted suites

`tests/mtp/global.json` switches `dotnet test` to Microsoft.Testing.Platform mode, so run from that folder:

```powershell
cd tests/mtp
dotnet test --solution ../../Specs.Mtp.slnx
```

### Expected results

| Test Suite | Project | Host | Results |
|-----------|---------|------|---------|
| Core unit specs | `tests/specs.core.UnitTests` | Specs VSTest adapter | 340 |
| BDD API specs | `tests/specs.BDD.UnitTests` | Specs VSTest adapter | 149 |
| VSTest adapter tests | `tests/specs.BDD.VSTestAdapter.Tests` | NUnit | 277 (8 pending fixture specs reported as Skipped) |
| End-to-end VSTest | `tests/specs.BDD.IntegrationTests` | Specs VSTest adapter | 34 (5 pending) |
| MTP runner tests | `tests/mtp/specs.BDD.TestingPlatform.Tests` | NUnit on MTP | 70 |
| End-to-end MTP | `tests/mtp/specs.BDD.TestingPlatform.IntegrationTests` | Specs MTP runner | 34 (5 pending) |

`Specs.sln` reports 800 results (787 passed, 13 skipped); `Specs.Mtp.slnx` reports 104 (99 passed, 5 skipped). Skipped results are pending specs and are expected. See [Testing](Testing) for details.

## Packing Locally

```powershell
dotnet pack Specs.sln -c Release -o artifacts
```

Without a `v*` tag in history, MinVer produces versions like `0.0.0-alpha.0.N`. See [Deployment](Deployment) for versioning and releases.

## IDE Setup

### Visual Studio (Recommended)

1. Open `Specs.sln` from the repo root. <!-- TODO: verify the minimum Visual Studio version that supports the .NET 10 SDK and .slnx files -->
2. Build the solution (**Build › Build Solution**).
3. Open **Test › Test Explorer**. The self-hosted suites appear under `Namespace › Class › [Feature]`, with each spec listed as `Feature → context → spec`.
4. Right-click any node to run or debug it. "Go to Test" lands on the `It[...]` line.
5. To work on the MTP runner, open `Specs.Mtp.slnx` separately.

### VS Code

**Required extension:** C# Dev Kit (`ms-dotnettools.csdevkit`).

1. Open the repo folder. [.vscode/settings.json](../../.vscode/settings.json) sets `dotnet.defaultSolution` to `Specs.sln`.
2. Use the **Testing** view to run or debug specs. VS Code ignores Test Explorer hierarchy hints, so each spec shows its full `Feature → context → spec` path in its name.
3. Pre-configured tasks in [.vscode/tasks.json](../../.vscode/tasks.json):

| Task | Command |
|------|---------|
| `build` (default) | `dotnet build Specs.sln` |
| `build-tests` | Build `specs.core.UnitTests` |
| `test` (default test task) | `dotnet test` on `specs.core.UnitTests` with detailed console logging |
| `test-with-coverage` | Same, with coverage properties |
| `clean`, `restore`, `publish` | Solution-wide |

4. Launch configurations in [.vscode/launch.json](../../.vscode/launch.json): **Run Unit Tests**, **Debug Unit Tests** (prompts for a `FullyQualifiedName~` filter), **Debug Current Test File**, and **.NET Core Attach**.

## Common Setup Issues

| Symptom | Cause | Fix |
|---------|-------|-----|
| `MSB1011: Specify which project or solution file to use` | Two solutions at the repo root | Name one: `dotnet build Specs.sln` |
| `A compatible .NET SDK was not found` / `global.json` mismatch | SDK older than 10.0.400 | Install a .NET 10 SDK at feature band 400 or later |
| `NU1008` during restore | A `PackageReference` declares `Version=` while central package management is on | Remove the version and add/update a `PackageVersion` in `Directory.Packages.props` |
| `CS1591` warning in a deliverable | New public member without an XML doc comment (`GenerateDocumentationFile` is on) | Add a `///` comment; the build is expected to stay warning-free |
| MTP tests run through VSTest or are not found | `dotnet test` was run outside `tests/mtp` | `cd tests/mtp` then `dotnet test --solution ../../Specs.Mtp.slnx` |
| Package version is `0.0.0-alpha.0.N` | No `v*` tag reachable (or a shallow clone) | Expected for local builds; fetch tags with `git fetch --tags` if you need the real version |

## Related Pages

- [Usage Guide](Usage-Guide) — Using Specs in your own test projects
- [Architecture](Architecture) — How the framework is structured
- [Testing](Testing) — The repo's own test suites
- [Troubleshooting](Troubleshooting) — Consumer-facing issues
