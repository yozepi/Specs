# Solution Changes Documentation

This directory contains documentation of all solution changes tracked at project milestones.

## Purpose

The changes directory maintains a historical record of:
- Git commit history
- File additions, modifications, and deletions
- New components (classes, interfaces, methods)
- Architectural impacts
- Memory bank documentation updates

This provides a comprehensive audit trail of the solution's evolution and helps maintain the memory bank documentation.

## File Structure

### `_current.md`
The work-in-progress change file that tracks ongoing changes since the last milestone. This file is:
- Continuously updated during development
- Generated/refreshed by running the change analysis script
- Archived at each milestone with a date-based filename

### `YYYY-MM-DD.md` (e.g., `2025-11-03.md`)
Archived milestone change files that document changes completed at specific dates. Files are:
- Named using the date format from the "Last Updated" field
- Sorted with most recent first (descending date order)
- Immutable historical records
- Referenced when reviewing past architectural decisions

## Workflow Usage

### Running the Analysis Script

This project uses a **global Cline workflow**. From your project root directory, run:

**Windows (PowerShell)**:
```powershell
~\OneDrive\Documents\Cline\Workflows\analyze-changes.ps1
```

**Linux/Mac (Bash)**:
```bash
~/OneDrive/Documents/Cline/Workflows/analyze-changes.sh
```

The script will:
1. Validate you're in the project root with a memory bank
2. Find the last milestone marker (git tag starting with "milestone-")
3. Analyze all commits since that marker
4. Archive the existing `_current.md` (if it exists)
5. Generate a new `_current.md` with detected changes

### Archiving Process

When you run the script:
1. The existing `_current.md` is read to extract its "Last Updated" date
2. The file is renamed to `YYYY-MM-DD.md` format
3. A fresh `_current.md` is created for new changes

### Manual Review Required

After running the script, you should:
1. Review the generated `_current.md` file
2. Add descriptions for new files and components
3. Document architectural impacts
4. Update relevant memory bank files
5. Check off completed memory bank updates
6. Update the milestone name if appropriate

### Committing Changes

After completing a milestone:
```bash
# Stage all changes
git add memory-bank/changes/
git add memory-bank/*.md

# Commit with milestone marker
git commit -m "docs: Archive changes and update memory bank [MILESTONE]"

# Tag the milestone
git tag -a "milestone-YYYY-MM-DD" -m "Milestone: [Description]"
```

## Change Document Format

Each change file follows this structure:

```markdown
# Changes: [Status]

**Last Updated**: YYYY-MM-DD HH:MM AM/PM TZ
**Milestone**: [Name or "In Progress"]

## Summary
[Quantitative overview of changes]

## Git Commits
[Commit history with hashes, messages, authors, dates]

## File Changes
### Added / Modified / Deleted
[Categorized file changes with descriptions]

## Component Changes
### New Components / Modified Components
[Classes, interfaces, methods with descriptions]

## Architectural Impact
[Design pattern changes, dependency updates, layer modifications]

## Memory Bank Updates
[Checklist of documentation updates needed/completed]
```

## Best Practices

### Frequency
- Run the workflow at natural breakpoints (feature completion, not mid-feature)
- Don't accumulate too many changes between milestones
- Use git tags to mark minor milestones if needed

### Descriptions
- Be specific about what changed and why
- Include context that would help future developers
- Link to related issues or design discussions if applicable

### Memory Bank Sync
- Update memory bank files immediately after archiving changes
- Don't postpone documentation updates
- Mark completed updates in the checklist

### Milestone Naming
- Use descriptive milestone names (not just dates)
- Examples: "Initial Setup", "Builder Pattern Refactor", "v1.0 Release"
- Include version numbers for releases

## Script Options

### PowerShell Script
```powershell
# Analyze from specific commit
~\OneDrive\Documents\Cline\Workflows\analyze-changes.ps1 -FromCommit <hash>

# Analyze from specific tag
~\OneDrive\Documents\Cline\Workflows\analyze-changes.ps1 -FromTag <tag>

# Show help
~\OneDrive\Documents\Cline\Workflows\analyze-changes.ps1 -Help
```

### Bash Script
```bash
# Analyze from specific commit
~/OneDrive/Documents/Cline/Workflows/analyze-changes.sh -c <hash>

# Analyze from specific tag
~/OneDrive/Documents/Cline/Workflows/analyze-changes.sh -t <tag>

# Show help
~/OneDrive/Documents/Cline/Workflows/analyze-changes.sh -h
```

## Troubleshooting

### Script can't find last milestone
- **Cause**: No git tags starting with "milestone-" exist
- **Solution**: Script will analyze from first commit; this is expected for initial setup

### Archive file already exists
- **Cause**: Multiple runs on same date or manual edits
- **Solution**: Script prompts for overwrite confirmation; choose based on your needs

### Changes seem incomplete
- **Cause**: Script analyzes from last milestone tag
- **Solution**: Verify tag exists and points to correct commit, or use `-FromCommit` option

### Component detection misses items
- **Cause**: Script uses simple regex patterns for C# code
- **Solution**: Manually review and add missed components to the change file

## Related Documentation

- **Workflow Guide**: `.claude/workflows/track-changes.md`
- **Memory Bank Index**: `memory-bank/README.md`
- **Glossary**: `memory-bank/glossary.md` (see "Global Workflow" entry)

---

**Created**: 2025-11-03
**Purpose**: This README helps maintain consistency in change tracking and documentation across the project lifecycle.
