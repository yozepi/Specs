# 03-bdd-unittests: Upgrade specs.BDD.UnitTests to net10.0

Change TFM to net10.0 and apply recommended package updates. Depends on specs.BDD and TestHarness.

**Done when**: Project targets net10.0, builds cleanly, tests pass.

## Research
- File: src/specs.specification.UnitTests/specs.BDD.UnitTests.csproj (net8.0).
- Assessment: NuGet.0002 (System.Text.Encodings.Web 9.0.11) + Project.0002. No API issues.
- System.Text.Encodings.Web is in the net10.0 shared framework → remove explicit reference.
