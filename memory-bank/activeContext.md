# Active Context: Specs

## Current Work Focus

### Memory Bank Restructuring (In Progress)
Converting memory bank from custom structure to standard Cline Memory Bank format:
- ✅ Created `projectbrief.md` - Foundation document
- ✅ Created `productContext.md` - Why the project exists
- 🔄 Creating remaining core files (activeContext, systemPatterns, techContext, progress)
- 📋 Need to update `README.md` to reflect new structure
- 📋 Need to decide what to do with existing custom files

### Project Status
- Framework is **feature-complete** and **stable**
- Two API approaches are fully implemented and tested
- Comprehensive test coverage exists for core framework
- Documentation is being restructured for better maintainability

## Recent Changes

### Suite → Feature Set → Specification Hierarchy Correction (February 2026)
**Critical correction based on user feedback**: The `[Context]` attribute and `Context[...]` indexer were incorrectly documented as two separate test paradigms. They are actually parts of the **same paradigm** at different levels of the hierarchy.

**Correct understanding**:
- A **Specification class** (inheriting from `Specifications` or `Spec`) is a **test suite** for a single subject (e.g., `Weather_Controller`)
- **`[Context]` decorated methods** within the class are **feature sets** — each method covers one feature/aspect of the subject, with its own lifecycle control
- **`Context[...]`/`Describe[...]` indexers** inside those methods organize **scenarios** within a feature set
- **`It[...]`** indexer defines individual **specifications**
- **Class-level lifecycle** (`BeforeAll`/`AfterAll`/`BeforeEach`/`AfterEach`) applies to the **entire suite** (all feature sets)
- **Feature-level lifecycle** (inside a `[Context]` method) applies only to **that feature set's specs**

**Files Updated**:
1. **test-patterns.md**: Rewrote to show Suite → Feature Set → Scenario → Specification hierarchy as primary pattern
2. **glossary.md**: Added Suite, Feature Set, Scenario, Specification definitions; clarified Context vs [Context]
3. **architecture.md**: Corrected overview to show single paradigm with two API styles, not two paradigms
4. **core-components.md**: Added ContextAttribute documentation, clarified Specifications as suite base class with two lifecycle levels
5. **productContext.md**: Updated examples to show full hierarchy with [Context] methods
6. **activeContext.md**: Documented this correction (current file)

### TestHarness Clarification (February 2026)
**Critical correction based on user feedback**: TestHarness is internal testing infrastructure only, NOT a project deliverable.

Updated documentation across all memory bank files to clarify:
- TestHarness exists solely to support internal unit testing during development
- It is NOT part of project output or deliverables
- It is a temporary solution until Specs framework is mature enough to test itself
- Will eventually be phased out once framework is self-hosting

**Files Updated**:
1. **projectbrief.md**: Restructured solution structure to distinguish deliverable vs internal projects
2. **techContext.md**: Added notes clarifying TestHarness is not deliverable, updated project structure
3. **progress.md**: Clarified TestHarness role in integration and testing sections
4. **README.md**: Updated project structure and version information sections
5. **activeContext.md**: Documented this correction (current file)

### Memory Bank Documentation (February 2026)
1. **Created projectbrief.md**: Established foundation document defining project identity, goals, and structure
2. **Created productContext.md**: Documented why the framework exists and what problems it solves
3. **Expanded project-overview.md**: Updated from 3 to 5 projects, documented both API approaches
4. **Enhanced architecture.md**: Added comparison between Modern (specs.BDD) and Legacy (Spec) approaches
5. **Updated glossary.md**: Added key terminology for both API styles

### Documentation Improvements
- Clarified the existence of TWO distinct API approaches (not just one)
- Documented five projects (not three) in the solution
- Added architectural comparisons and migration guidance
- Improved understanding of framework layers and data flow

## Next Steps

### Immediate (This Session)
1. ✅ Create `activeContext.md` (current file)
2. Create `systemPatterns.md` - Migrate architecture content from `architecture.md`
3. Create `techContext.md` - Document technologies, dependencies, setup
4. Create `progress.md` - Document what works and what's left
5. Update `README.md` - Reflect new standard structure
6. Decide on existing files:
   - `project-overview.md` - Keep as additional context or merge?
   - `architecture.md` - Keep as additional context or replace with systemPatterns?
   - `core-components.md` - Keep or integrate?
   - `test-patterns.md` - Keep or integrate?
   - `glossary.md` - Keep as additional context
   - `devops-formatting.md` - Keep as additional context
   - `roles.md` - Keep as additional context

### Short-term
1. Validate memory bank completeness
2. Ensure all documentation is accurate and up-to-date
3. Consider adding examples directory to memory bank
4. Document common usage patterns

### Long-term
1. Keep memory bank synchronized with any code changes
2. Document any new patterns or components as they emerge
3. Maintain both API approaches equally in documentation

## Active Decisions & Considerations

### Decision: Support Two API Approaches
**Why**: 
- Legacy `Spec` class has existing users and codebases
- Modern `Specifications` class represents better architectural patterns
- Both can coexist without conflict

**Impact**:
- Documentation must cover both approaches equally
- Examples should show both styles where appropriate
- Migration guidance helps teams transition gradually

### Consideration: Memory Bank Structure
**Current**: Transitioning from custom structure to standard Cline format

**Rationale**:
- Standard format improves AI assistant comprehension
- Clear hierarchy (projectbrief → productContext → activeContext → etc.)
- Better separation of concerns (why vs. how vs. what vs. status)

**Trade-offs**:
- More files to maintain
- Need to reorganize existing content
- Some duplication during transition

### Consideration: Framework Agnosticism
**Pattern**: Core framework uses adapter pattern for external dependencies

**Benefits**:
- Can integrate with different test runners
- Core logic remains clean and testable
- Easy to add new integrations

**Current State**: 
- Primary integration is NSpec
- Adapter interfaces defined
- Future integrations possible without core changes

## Important Patterns & Preferences

### Code Organization Pattern
```
specs.core (framework-agnostic core)
    ↓
specs.BDD (high-level API)
    ↓
User Test Code
```

### Naming Conventions
- **Test Lifecycle Methods**: `BeforeAll`, `BeforeEach`, `Act`, `AfterEach`, `AfterAll`
- **Test Organization**: `Describe` (what), `Context` (when/given), `It` (should/then)
- **Async Methods**: Suffix with `Async` where appropriate (`BeforeEachAsync`, `ItAsync`)

### Testing Philosophy
1. **Hierarchical Organization**: Use nested contexts to reflect conceptual relationships
2. **Clear Lifecycle**: Explicit phases for setup, action, assertion, teardown
3. **DRY Principle**: Common setup in parent contexts, inherited by children
4. **Readability First**: Tests should read like specifications
5. **Async Support**: First-class async support throughout all lifecycle phases

### Code Style Preferences
- Immutable test cases after `Build()` (Modern approach)
- Fluent API with indexer syntax for readability
- Adapter pattern for external dependencies
- Comprehensive XML documentation comments

## Learnings & Project Insights

### Insight 1: Two Architectural Philosophies
The framework evolved to support two distinct approaches:
- **Legacy Runtime**: Property-based API with mutable runtime state
- **Modern Builder**: Method-based API with immutable built test cases

Both are valid and serve different needs. The legacy approach is simpler for basic cases, while the modern approach offers better testability and separation of concerns.

### Insight 2: Async Abstraction Strategy
The framework maintains a simple `Action`-based interface while supporting async operations by converting `Task` to synchronous execution using `GetAwaiter().GetResult()`. This keeps the API simple while enabling modern async patterns.

### Insight 3: Lifecycle Order Matters
- **Setup/Act**: Top-down (parent → child) ensures base setup happens first
- **Teardown**: Bottom-up (child → parent) ensures proper cleanup order
- This matches natural initialization/disposal patterns

### Insight 4: Context Switching Pattern
The `Specifications` class uses a callback pattern to manage context switching:
```csharp
private void contextCallBack(IContextualTestCaseBuilder childBuilder, Action action)
{
    var previousBuilder = this.coreBuilder;
    this.coreBuilder = childBuilder;  // Switch context
    try { action.Invoke(); }          // Run user code
    finally { this.coreBuilder = previousBuilder; }  // Restore
}
```
This enables nested context definitions where lifecycle methods automatically target the correct scope.

### Insight 5: Framework Agnosticism Through Adapters
The adapter pattern successfully decouples the core framework from specific test runners:
- `ITestProviderAdapter` - Test instance retrieval
- `ITestContextAdapter` - Lifecycle execution
- `ILoggingAdapter` - Logging operations

This makes the framework extensible to new test runners without modifying core logic.

### Insight 6: Documentation Challenges
The existence of two API approaches initially caused documentation confusion:
- Some docs only covered one approach
- Project structure was unclear (3 vs 5 projects)
- Migration path wasn't documented

**Resolution**: Comprehensive documentation that treats both approaches equally and provides clear migration guidance.

## Current Project Health

### ✅ What's Working Well
- Core framework is stable and well-tested
- Two API approaches both work reliably
- Test coverage is comprehensive
- Async support works throughout
- NSpec integration is solid
- Framework agnosticism through adapters

### 🔄 In Progress
- Memory bank restructuring to standard format
- Documentation improvements
- Clarifying two-approach architecture

### 📋 Known Limitations
- Primary focus on NSpec integration (other runners possible but not implemented)
- Some documentation still being consolidated
- Examples could be more comprehensive

### 🎯 Success Metrics
- Zero known bugs in core framework
- Comprehensive test suite (2,900+ lines in lifecycle tests alone)
- Clear separation between framework layers
- Both API approaches fully functional
- Documentation improving continuously

## Questions & Open Topics

### Q1: Should we keep custom memory bank files?
Options:
- Keep as "additional context" alongside core files
- Merge content into core files and delete
- Archive in a separate folder

**Recommendation**: Keep core-components, test-patterns, glossary, roles, devops-formatting as additional context. They provide valuable supplementary information.

### Q2: How to handle architecture.md content?
- Much content should move to `systemPatterns.md` (standard core file)
- Some architectural comparisons might stay in `architecture.md` as additional context
- Need to avoid duplication

**Recommendation**: Create `systemPatterns.md` with core architectural patterns, keep `architecture.md` as additional context for deeper comparisons and migration details.

### Q3: Should project-overview.md be merged into core files?
- Some content overlaps with `projectbrief.md`
- Some content belongs in `techContext.md`
- Could be simplified or removed after core files are complete

**Recommendation**: Review after all core files are created, likely merge and remove.
