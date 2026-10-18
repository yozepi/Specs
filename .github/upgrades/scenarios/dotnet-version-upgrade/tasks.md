# .NET 10 Upgrade Progress

## Overview

Upgrade the four net8.0 test projects to net10.0 using a Top-Down approach; netstandard2.1 libraries remain unchanged.

**Progress**: 1/5 tasks complete <progress value="20" max="100"></progress> 20%

## Tasks

- ✅ 01-prerequisites: Verify .NET 10 SDK and global.json ([Content](tasks/01-prerequisites/task.md), [Progress](tasks/01-prerequisites/progress-details.md))
- 🔄 02-core-unittests: Upgrade specs.core.UnitTests to net10.0 ([Content](tasks/02-core-unittests/task.md))
- 🔲 03-bdd-unittests: Upgrade specs.BDD.UnitTests to net10.0
- 🔲 04-vstestadapter-tests: Upgrade VSTestAdapter test projects to net10.0
- 🔲 05-final-validation: Full solution build and test
