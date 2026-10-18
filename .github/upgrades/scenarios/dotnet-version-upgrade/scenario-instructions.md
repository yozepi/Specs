# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0
- **Scope**: Upgrade .NET 8 projects only (leave .NET Standard 2.1 projects as-is)

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: Top-Down (user choice over recommended All-at-Once)

## Strategy
**Selected**: Top-Down
**Rationale**: User preference; netstandard2.1 libraries need no multi-targeting.

### Execution Constraints
- Upgrade test/entry projects one at a time; validate build + tests after each
- Do not change netstandard2.1 libraries
- No Phase 2 consolidation needed (no multi-targeting introduced)

## Source Control
- **Source Branch**: feature/feature-ID-#3
- **Working Branch**: feature/feature-ID-#3 (current branch, per user)
- **Commit Strategy**: After Each Task
- **Branch Sync**: Disabled (working on source branch)
