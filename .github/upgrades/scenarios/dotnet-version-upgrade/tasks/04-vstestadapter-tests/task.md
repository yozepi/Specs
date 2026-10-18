# 04-vstestadapter-tests: Upgrade VSTestAdapter test projects to net10.0

Upgrade specs.BDD.VSTestAdapter.Tests and specs.BDD.VSTestAdapter.IntegrationTests to net10.0. Assessment flags source-incompatible `TimeSpan.FromMilliseconds/FromSeconds(double)` overload ambiguity (new int/long overloads in .NET 10) and `System.Uri` behavioral changes in the Tests project.

**Done when**: Both projects target net10.0, build cleanly, tests pass.
