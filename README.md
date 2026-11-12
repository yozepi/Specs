# Specs

A .NET BDD testing framework. Write nested, readable specs with a full async lifecycle, and run them under VSTest (Visual Studio, `dotnet test`) or Microsoft.Testing.Platform (MTP).

```csharp
public class CalculatorSpecs : Specifications
{
    private Calculator _calc = null!;
    private int _result;

    [Feature("Addition")]
    public void addition()
    {
        BeforeEach(() => _calc = new Calculator());
        Act(() => _result = _calc.Add(2, 3));

        It["returns the sum"] = () => Check.That(_result).IsEqualTo(5);
    }
}
```

## Features

- Hierarchical `Describe` / `Context` / `It` specs inside `[Feature]` methods, with inherited setup and act
- Lifecycle: `BeforeAll`, `BeforeEach`, `Act`, `AfterEach`, `AfterAll`; `BeforeAll`/`AfterAll` run once per context, even for partial runs
- First-class async on every hook
- Pending specs (`TODO`, `xIt`, `xContext`) reported as Skipped and filterable
- Expected-exception specs with `ShouldThrow<T>()`
- Two runners that share one execution core: VSTest and Microsoft.Testing.Platform
- Framework-agnostic core with no test-framework dependencies

## Packages

| Package | Purpose |
|---|---|
| `specs.BDD.VSTest` | VSTest adapter (Test Explorer, `dotnet test`); includes `specs.BDD` |
| `specs.BDD.MTP` | Microsoft.Testing.Platform runner with the full spec tree; includes `specs.BDD` |
| `specs.core` | Framework-agnostic core (pulled in transitively) |
| `specs.BDD.Templates` | `dotnet new` templates |

## Quick start

Scaffold a project:

```
dotnet new install specs.BDD.Templates
dotnet new specs-vstest -n MyTests      # or specs-mtp
```

Or add a package to an existing test project:

```
dotnet add package specs.BDD.VSTest     # or specs.BDD.MTP
```

For MTP under `dotnet test` on .NET 10, add this to `global.json`:

```json
{ "test": { "runner": "Microsoft.Testing.Platform" } }
```

A `[Feature]`-decorated method is required for a suite to be discovered.

## Writing specs

### Pending specs

```csharp
It["is not written yet"] = TODO;
xIt["is excluded for now"] = () => { /* never run */ };
xContext["a pending scenario"] = () => { /* every spec inside is pending */ };
```

Select them with `dotnet test --filter IsPending=true`, or exclude with `IsPending=false`.

### Expected exceptions

```csharp
Act(() => calculator.Divide(1, 0));

It["throws"] = ShouldThrow<DivideByZeroException>();
It["explains why"] = ShouldThrow<DivideByZeroException>(ex =>
    Check.That(ex.Message.ToLowerInvariant()).Contains("zero"));
```

## Running on Microsoft.Testing.Platform

Each spec has a path that is its test ID and what `--treenode-filter` matches:

```
/MyTests/My.Specs/CartSpecs/Cart/when empty/shows a message
```

```
dotnet test --treenode-filter "/*/*/CartSpecs/Cart/**"
```

Runner settings go in `[appname].testconfig.json` or environment variables (`SpecsBDD__ReportingLevel`):

```json
{
  "SpecsBDD": {
    "ReportingLevel": "Feature",
    "ShowSourceLocations": true,
    "ColorOutput": false
  }
}
```

## Repository layout

| Path | Contents |
|---|---|
| `src/specs.core` | Core: builder, immutable test-case model, executor (.NET Standard 2.1) |
| `src/specs.specification` | `specs.BDD` authoring API (`Specifications`) |
| `src/specs.BDD.VSTestAdapter` | VSTest runner (package `specs.BDD.VSTest`) |
| `src/specs.BDD.TestingPlatform` | MTP runner (package `specs.BDD.MTP`) |
| `src/specs.BDD.Templates` | `dotnet new` templates |
| `src/shared/specs.BDD.Runner` | Discovery/execution code compiled into both runners |
| `tests/` | Test projects; `tests/mtp/` holds the MTP-hosted ones |

## Build and test

The root holds two solutions, so name one explicitly.

```
dotnet build Specs.sln
dotnet test Specs.sln
```

MTP projects live in `Specs.Mtp.slnx`; run them from `tests/mtp`, whose `global.json` enables MTP mode:

```
cd tests/mtp
dotnet test --solution ../../Specs.Mtp.slnx
```

Requires the .NET SDK pinned in [global.json](global.json).

## Documentation

- [Wiki](https://github.com/yozepi/Specs/wiki): [Getting Started](https://github.com/yozepi/Specs/wiki/Getting-Started), [Usage Guide](https://github.com/yozepi/Specs/wiki/Usage-Guide), [Architecture](https://github.com/yozepi/Specs/wiki/Architecture), [Testing](https://github.com/yozepi/Specs/wiki/Testing), [Troubleshooting](https://github.com/yozepi/Specs/wiki/Troubleshooting)
- [memory-bank/](memory-bank/projectbrief.md): project context for contributors

## License

See [LICENSE](LICENSE).
