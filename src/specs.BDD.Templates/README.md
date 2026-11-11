# specs.BDD.Templates

[![NuGet](https://img.shields.io/nuget/v/specs.BDD.Templates.svg)](https://www.nuget.org/packages/specs.BDD.Templates)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/yozepi/Specs/blob/main/LICENSE)

`dotnet new` templates for [Specs](https://github.com/yozepi/Specs), an RSpec-style BDD framework for .NET. One command gives you a spec project in C# or VB that already builds and passes, on either VSTest or Microsoft.Testing.Platform (MTP).

## Which template do I need?

| Template | Runner package | Use it when |
|---|---|---|
| `specs-vstest` | `specs.BDD.VSTest` | You want the classic `dotnet test` / Test Explorer setup, or you rely on `--filter` and `.runsettings` |
| `specs-mtp` | `specs.BDD.MTP` | You want the whole spec tree as nodes in your IDE, filtering by path, a console tree report, or a test project you can `dotnet run` |

Both come in C# (default) and VB. Your specs behave the same on both hosts, so switching later only means swapping the package.

More: [Pick a Test Host](https://github.com/yozepi/Specs/wiki/Usage-Guide#pick-a-test-host)

## Installation

```shell
dotnet new install specs.BDD.Templates
```

To update or remove the templates later:

```shell
dotnet new update
dotnet new uninstall specs.BDD.Templates
```

## Quick Start

```shell
dotnet new specs-vstest -n MyApp.Specs              # VSTest, C#
dotnet new specs-vstest -n MyApp.Specs -lang VB     # VSTest, VB
dotnet new specs-mtp -n MyApp.Specs                 # MTP, C#
dotnet new specs-mtp -n MyApp.Specs -lang VB        # MTP, VB
```

Then run it:

```shell
cd MyApp.Specs
dotnet test        # specs-vstest
dotnet run         # specs-mtp
```

Each project contains a `CalculatorSpecs` class to start from:

```csharp
using NFluent;
using specs.BDD;
using specs.BDD.Attributes;

namespace MyApp.Specs;

public class CalculatorSpecs : Specifications
{
    int num1;
    int num2;
    int result;

    public CalculatorSpecs()
    {
        BeforeEach(() => num1 = num2 = result = 0);
    }

    [Feature("Addition")]
    public void describe_addition()
    {
        Act(() => result = num1 + num2);

        Context["when adding two positive numbers"] = () =>
        {
            BeforeEach(() => { num1 = 2; num2 = 3; });

            It["returns the sum"] = () => Check.That(result).IsEqualTo(5);
        };
    }
}
```

```vb
Imports NFluent
Imports specs.BDD
Imports specs.BDD.Attributes

Public Class CalculatorSpecs
    Inherits Specifications

    Private num1 As Integer
    Private num2 As Integer
    Private result As Integer

    Public Sub New()
        BeforeEach(
            Sub()
                num1 = 0
                num2 = 0
                result = 0
            End Sub)
    End Sub

    <Feature("Addition")>
    Public Sub describe_addition()
        Act(Sub() result = num1 + num2)

        Context("when adding two positive numbers") =
            Sub()
                BeforeEach(
                    Sub()
                        num1 = 2
                        num2 = 3
                    End Sub)

                It("returns the sum") = Sub() Check.That(result).IsEqualTo(5)
            End Sub
    End Sub
End Class
```

More: [Quick Launch](https://github.com/yozepi/Specs/wiki/Usage-Guide#quick-launch)

## Usage

### Running an MTP project with `dotnet test`

The `specs-mtp` project is a test application, so `dotnet run` always works. To use `dotnet test` on the .NET 10 SDK, add this to the `global.json` that covers your project:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

More: [Microsoft.Testing.Platform troubleshooting](https://github.com/yozepi/Specs/wiki/Troubleshooting#microsofttestingplatform)

### Pinning the Specs version

By default the generated project references the Specs packages at the same version as the templates. Pass `--specsVersion` to pick another:

```shell
dotnet new specs-vstest -n MyApp.Specs --specsVersion 1.2.0
```

## Configuration

| Option | Default | Description |
|---|---|---|
| `-n`, `--name` | Current folder name | Project name, folder, and root namespace |
| `-lang`, `--language` | `C#` | `C#` or `VB` |
| `--specsVersion` | The templates' package version | Version of `specs.BDD.VSTest` / `specs.BDD.MTP` to reference |

Run `dotnet new specs-vstest --help` or `dotnet new specs-mtp --help` for the full list.

## Compatibility

- Generated projects target `net10.0`.
- Requires the .NET 10 SDK.
- Generated projects reference `NFluent` for assertions; swap in any assertion library you like.

## Learn More

- [Usage Guide](https://github.com/yozepi/Specs/wiki/Usage-Guide)
- [Writing Specs](https://github.com/yozepi/Specs/wiki/Usage-Guide#writing-specs)
- [Running and Filtering](https://github.com/yozepi/Specs/wiki/Usage-Guide#running-and-filtering)
- [Troubleshooting](https://github.com/yozepi/Specs/wiki/Troubleshooting)
- [Release notes](https://github.com/yozepi/Specs/releases)

## License

MIT — see [LICENSE](https://github.com/yozepi/Specs/blob/main/LICENSE).
