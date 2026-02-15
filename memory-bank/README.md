# Specs Framework - Memory Bank

This memory bank follows the standard Cline Memory Bank structure, providing comprehensive documentation about the Specs testing framework for .NET.

## Understanding the Memory Bank

After each session, Cline's memory resets completely. The Memory Bank is Cline's only link to previous work, making accurate documentation essential for continued effectiveness.

## Memory Bank Structure

The memory bank consists of **6 core files** that build upon each other in a clear hierarchy, plus **additional context files** for deeper information.

### Core Files (Required Reading)

These files form the foundation and should be read in order when starting work:

#### 1. 📋 [projectbrief.md](projectbrief.md) - Foundation Document
**Purpose**: Shapes all other files, defines core requirements and goals

**Contains**:
- Project identity (Specs, .NET Standard 2.1 testing framework)
- Core purpose (BDD-style testing with lifecycle management)
- 5 projects structure
- Two testing approaches (Modern specs.BDD vs Legacy Spec)
- Technology stack, scope, constraints, success metrics

**Start here** - This is the source of truth for project scope.

#### 2. 🎯 [productContext.md](productContext.md) - Why This Exists
**Purpose**: Explains why the project exists and what problems it solves

**Contains**:
- Business/technical context
- Problems solved (test organization, lifecycle complexity, async testing, readability)
- How it should work from user perspective
- User experience goals
- Comparison with other frameworks

**Read this** to understand the project's value proposition and design philosophy.

#### 3. 🔄 [activeContext.md](activeContext.md) - Current Work Focus
**Purpose**: Tracks current work, recent changes, and next steps

**Contains**:
- Current work focus (memory bank restructuring)
- Recent changes
- Next steps
- Active decisions and considerations
- Important patterns and preferences
- Learnings and project insights

**Check this** to understand what's happening now and what comes next.

#### 4. 🏗️ [systemPatterns.md](systemPatterns.md) - Architecture & Design
**Purpose**: Documents system architecture and design patterns

**Contains**:
- Core architectural pattern (Builder + Hierarchical Contexts)
- Key design patterns (Builder, Composite, Adapter, Strategy, Template Method)
- Critical implementation patterns
- Component relationships
- Data flow diagrams
- Key technical decisions

**Reference this** to understand how the framework is structured and why.

#### 5. 🔧 [techContext.md](techContext.md) - Technologies & Setup
**Purpose**: Documents technologies, dependencies, and development setup

**Contains**:
- Technology stack (.NET Standard 2.1, C# 8.0)
- Core dependencies (NSpec, NUnit, NFluent)
- Project structure
- Development environment setup
- Build and test commands
- Technical constraints

**Use this** for environment setup and technical reference.

#### 6. ✅ [progress.md](progress.md) - Current Status
**Purpose**: Tracks what works and what's left to build

**Contains**:
- What works (all core features ✅)
- What's left to build (optional enhancements 📋)
- Known issues and limitations
- Evolution timeline
- Success metrics
- Current status: **STABLE AND FEATURE-COMPLETE**

**Check this** to understand project health and completion status.

### File Hierarchy Flow

```
projectbrief.md (Foundation)
    ↓
productContext.md (Why)
    ↓
activeContext.md (Now)
    ↓
systemPatterns.md (How)
    ↓
techContext.md (With what)
    ↓
progress.md (Status)
```

## Additional Context Files

These files provide supplementary information and deeper dives into specific topics:

### 📖 [glossary.md](glossary.md) - Terminology Reference
Comprehensive definitions of terms, concepts, and abbreviations used throughout the project.

**Use when**: You encounter unfamiliar terms or need clarification on framework concepts.

### 🧪 [test-patterns.md](test-patterns.md) - Usage Examples
Practical examples and best practices for writing tests with the framework.

**Use when**: Writing tests or learning framework usage patterns.

### 🔌 [core-components.md](core-components.md) - Component Details
In-depth documentation of specific framework components and their interactions.

**Use when**: Working with specific framework classes and interfaces.

### 🏢 [devops-formatting.md](devops-formatting.md) - DevOps Guidelines
Azure DevOps work item formatting and workflow guidelines.

**Use when**: Creating or managing Azure DevOps work items.

### 👥 [roles.md](roles.md) - Role-Based Workflows
Specialized workflows for different development roles (Solution Architect, Developer Lead, etc.).

**Use when**: Following role-specific processes.

### 📁 [changes/](changes/)
Directory tracking project changes over time.

- `_current.md` - Work-in-progress changes
- `YYYY-MM-DD.md` - Archived milestone changes

## Quick Reference

### Test Lifecycle Order
```
1. SetupOnce (Parent → Child)
2. For each test:
   - SetupEach (Parent → Child)
   - Act (Parent → Child)
   - Test Assertion
   - TeardownEach (Child → Parent)
3. TeardownOnce (Child → Parent)
```

### Key Architectural Patterns
- **Builder Pattern**: Construct test cases with fluent API
- **Composite Pattern**: Hierarchical contexts with inheritance
- **Adapter Pattern**: Framework-agnostic core
- **Strategy Pattern**: Configurable execution behavior
- **Template Method**: Fixed lifecycle execution order

### Project Structure
```
src/
├── specs.core/              # Core framework (builder pattern, adapters) - DELIVERABLE
├── specs.specification/     # specs.BDD (modern method-based API) - DELIVERABLE
├── specs.core.UnitTests/    # Framework unit tests
├── specs.specification.UnitTests/  # specs.BDD tests
├── TestHarness/             # Internal testing infrastructure (NOT DELIVERABLE)
│                            # Temporary NSpec integration for testing Specs itself
├── Spec.cs                  # Legacy property-based API
└── Domain/                  # Legacy runtime layer
```

## Reading Recommendations

### First Time with Project
1. Read [projectbrief.md](projectbrief.md) - Understand what Specs is
2. Read [productContext.md](productContext.md) - Understand why it exists
3. Skim [systemPatterns.md](systemPatterns.md) - Understand architectural approach
4. Check [progress.md](progress.md) - Understand current status

### Before Starting Work
1. Check [activeContext.md](activeContext.md) - What's happening now?
2. Check [progress.md](progress.md) - What's the status?
3. Review relevant sections in core files as needed

### When Writing Code
1. Reference [systemPatterns.md](systemPatterns.md) - Follow established patterns
2. Reference [techContext.md](techContext.md) - Use correct dependencies
3. Reference [test-patterns.md](test-patterns.md) - Write tests correctly
4. Reference [glossary.md](glossary.md) - Use correct terminology

### When Troubleshooting
1. Check [progress.md](progress.md) - Known issues and limitations
2. Check [techContext.md](techContext.md) - Common issues and solutions
3. Check [test-patterns.md](test-patterns.md) - Test isolation and debugging

## Maintaining Documentation

After significant work, update the relevant core files:

1. **activeContext.md** - Always update with recent changes and next steps
2. **progress.md** - Update status, mark completed items
3. **systemPatterns.md** - If architectural patterns change
4. **techContext.md** - If dependencies or setup changes
5. **productContext.md** - If problems solved or use cases change
6. **projectbrief.md** - Rarely, only if fundamental scope changes

## Version Information

- **.NET Target**: netstandard2.1
- **Language**: C# 8.0 with nullable reference types
- **Primary Dependencies**: NSpec, NUnit, NFluent
- **Deliverable Projects**: 2 (specs.core, specs.BDD)
- **Test Projects**: 2 (specs.core.UnitTests, specs.BDD.UnitTests)
- **Internal Infrastructure**: 1 (TestHarness - temporary testing tool, not deliverable)
- **API Approaches**: 2 (Modern method-based, Legacy property-based)
- **Current Status**: Stable and feature-complete

## Related Resources

- [.NET Standard 2.1 Documentation](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
- [NSpec Documentation](http://nspec.org/)
- [NUnit Documentation](https://nunit.org/)
- [NFluent Documentation](http://www.n-fluent.net/)

---

**Last Updated**: 2026-02-07

**Memory Bank Format**: Standard Cline Memory Bank (6 core files)

For questions about this documentation structure, refer to the Cline Memory Bank guidelines.
