# .NET 10 Upgrade Plan

## Overview

**Target**: Upgrade the net8.0 projects in Specs.sln to net10.0
**Scope**: 4 net8.0 test projects (~small); 4 netstandard2.1 libraries stay as-is

### Selected Strategy
**Top-Down (Application-First)** — Entry-point (test) projects upgraded first, one at a time.
**Rationale**: User preference. All entry projects are net8.0; shared libraries target netstandard2.1, which net10.0 consumes directly — no library multi-targeting is required, so Phase 2 consolidation is a no-op.

**Applications (priority order)**: specs.core.UnitTests → specs.BDD.UnitTests → specs.BDD.VSTestAdapter.Tests + IntegrationTests
**Libraries (unchanged, netstandard2.1)**: specs.core, specs.BDD, specs.BDD.VSTestAdapter, TestHarness

## Tasks

### 01-prerequisites: Verify .NET 10 SDK and global.json

Confirm the .NET 10 SDK is installed and that any global.json does not pin an incompatible SDK.

**Done when**: .NET 10 SDK available; global.json (if any) compatible.

---

### 02-core-unittests: Upgrade specs.core.UnitTests to net10.0

Change TFM to net10.0 and apply recommended package updates (System.Reflection.Metadata, System.Text.Encodings.Web → 10.0.x) where explicitly referenced. Depends on specs.core, specs.BDD, specs.BDD.VSTestAdapter, TestHarness (netstandard2.1).

**Done when**: Project targets net10.0, builds with no errors/new warnings, tests pass.

---

### 03-bdd-unittests: Upgrade specs.BDD.UnitTests to net10.0

Change TFM to net10.0 and apply recommended package updates. Depends on specs.BDD and TestHarness.

**Done when**: Project targets net10.0, builds cleanly, tests pass.

---

### 04-vstestadapter-tests: Upgrade VSTestAdapter test projects to net10.0

Upgrade specs.BDD.VSTestAdapter.Tests and specs.BDD.VSTestAdapter.IntegrationTests to net10.0. Assessment flags source-incompatible `TimeSpan.FromMilliseconds/FromSeconds(double)` overload ambiguity (new int/long overloads in .NET 10) and `System.Uri` behavioral changes in the Tests project.

**Done when**: Both projects target net10.0, build cleanly, tests pass.

---

### 05-final-validation: Full solution build and test

Build the entire solution and run all tests. Library consolidation is not needed (no multi-targeting introduced).

**Done when**: Solution builds with 0 errors and all tests pass.
