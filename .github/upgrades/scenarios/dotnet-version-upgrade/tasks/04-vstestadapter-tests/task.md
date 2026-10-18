# 04-vstestadapter-tests: Upgrade VSTestAdapter test projects to net10.0

Upgrade specs.BDD.VSTestAdapter.Tests and specs.BDD.VSTestAdapter.IntegrationTests to net10.0. Assessment flags source-incompatible `TimeSpan.FromMilliseconds/FromSeconds(double)` overload ambiguity (new int/long overloads in .NET 10) and `System.Uri` behavioral changes in the Tests project.

**Done when**: Both projects target net10.0, build cleanly, tests pass.

## Research
- Tests project: Api.0002 in Execution/ResultReporter_Tests.cs (TimeSpan.FromMilliseconds/FromSeconds with int literals → bind to new .NET 10 int/long overloads, same semantics; no change needed). Api.0003 System.Uri behavioral (executor:// URIs compared for equality — unaffected).
- IntegrationTests: TFM only.
