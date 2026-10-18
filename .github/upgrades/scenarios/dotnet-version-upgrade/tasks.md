# .NET 10 Upgrade Progress

## Overview

Upgrade the four net8.0 test projects to net10.0 using a Top-Down approach; netstandard2.1 libraries remain unchanged.

**Progress**: 4/5 tasks complete <progress value="80" max="100"></progress> 80%

## Tasks

- ✅ 01-prerequisites: Verify .NET 10 SDK and global.json ([Content](tasks/01-prerequisites/task.md), [Progress](tasks/01-prerequisites/progress-details.md))
- ✅ 02-core-unittests: Upgrade specs.core.UnitTests to net10.0 ([Content](tasks/02-core-unittests/task.md), [Progress](tasks/02-core-unittests/progress-details.md))
- ✅ 03-bdd-unittests: Upgrade specs.BDD.UnitTests to net10.0 ([Content](tasks/03-bdd-unittests/task.md), [Progress](tasks/03-bdd-unittests/progress-details.md))
- ✅ 04-vstestadapter-tests: Upgrade VSTestAdapter test projects to net10.0 ([Content](tasks/04-vstestadapter-tests/task.md), [Progress](tasks/04-vstestadapter-tests/progress-details.md))
- 🔲 05-final-validation: Full solution build and test
