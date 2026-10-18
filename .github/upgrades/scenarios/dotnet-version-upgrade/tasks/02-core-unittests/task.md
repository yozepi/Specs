# 02-core-unittests: Upgrade specs.core.UnitTests to net10.0

Change TFM to net10.0 and apply recommended package updates (System.Reflection.Metadata, System.Text.Encodings.Web → 10.0.x) where explicitly referenced. Depends on specs.core, specs.BDD, specs.BDD.VSTestAdapter, TestHarness (netstandard2.1).

**Done when**: Project targets net10.0, builds with no errors/new warnings, tests pass.

## Research
- File: src/specs.core.UnitTests/specs.core.UnitTests.csproj (net8.0, SDK-style).
- Assessment: NuGet.0002 (System.Text.Encodings.Web 9.0.11 → 10.0.x), Project.0002 (TFM). No API issues.
- System.Text.Encodings.Web ships in Microsoft.NETCore.App on net10.0 → explicit PackageReference is redundant (NU1510 on .NET 10 SDK). Plan: remove it rather than bump.
- System.Reflection.Metadata is transitive only (not referenced directly) — no action.
