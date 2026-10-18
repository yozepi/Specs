# 04-vstestadapter-tests — Progress

## Changes
- src/specs.BDD.VSTestAdapter.Tests/specs.BDD.VSTestAdapter.Tests.csproj: net8.0 → net10.0
- src/specs.BDD.VSTestAdapter.IntegrationTests/specs.BDD.VSTestAdapter.IntegrationTests.csproj: net8.0 → net10.0
- No code changes required (TimeSpan int-literal calls bind to new overloads; Uri usage unaffected).

## Validation
- Build: 0 errors, 0 warnings.
- Tests: Tests 264 passed / 8 skipped; IntegrationTests 23 passed / 5 skipped; 0 failed (skips pre-existing).
