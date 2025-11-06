# AI Assistant Rules for Specs Project

## 🔍 Before Starting ANY Task

**MANDATORY FIRST STEP**: Review the memory bank documentation before proceeding with any task.

1. **Start here**: Read `memory-bank/README.md` for navigation
2. **Understand the context**: Review relevant documentation based on task type:
   - Architecture or design changes → `architecture.md`
   - Working with specific components → `core-components.md`
   - Creating or modifying tests → `test-patterns.md`
   - Unfamiliar terms → `glossary.md`

## 📝 During Task Execution

### Code Modifications
- Follow existing patterns documented in the memory bank
- Respect the test lifecycle order: SetupOnce → SetupEach → Act → Test → TeardownEach → TeardownOnce
- Maintain separation between Builder, Model, and Execution layers
- Use adapter pattern for external dependencies

### Testing
- Follow patterns in `test-patterns.md`
- Ensure test isolation (no shared mutable state)
- Use appropriate lifecycle hooks
- Write descriptive context and test names

### Async Operations
- Remember: Framework wraps async methods to maintain Action-based interface
- All lifecycle hooks support both sync and async operations

## ✅ After Task Completion

1. **Update Documentation**: If your changes introduce:
   - New components → Update `core-components.md`
   - New patterns → Add to `test-patterns.md`
   - New terminology → Add to `glossary.md`
   - Architectural changes → Update `architecture.md`

2. **Maintain Consistency**: Keep memory bank documentation synchronized with code

## 🎯 Key Principles

- **Immutability**: Test cases are immutable after Build()
- **Top-Down Setup**: Setup/Act actions flow parent → child
- **Bottom-Up Teardown**: Teardown actions flow child → parent
- **Framework Agnostic**: Core remains independent through adapters

## 📍 Project Structure Reference

```
├── memory-bank/             # 📚 Documentation (READ THIS FIRST!)
└── src/
    ├── specs.core/          # Core framework
    │   └── Framework/
    │       ├── Adapters/    # External dependency abstractions
    │       ├── Processors/  # Test case models & executors
    │       └── Settings/    # Configuration
    ├── specs.core.UnitTests/ # Framework tests
    └── TestHarness/         # NSpec integration
```

## 🚫 Common Pitfalls to Avoid

- Don't use static mutable state in tests
- Don't assume execution order between tests
- Don't modify test cases after Build()
- Don't bypass adapters for external dependencies
- Don't forget to update memory bank after significant changes

## 📦 Git Operations & Commit Messages

### Automatic Commit Message Generation

When the user requests any of these operations:
- "check in"
- "commit"
- "push" (with pending changes)
- "generate commit message"
- Any variation of the above

**Follow this workflow:**

1. **Check git status**: Run `git status` to see staged/unstaged changes
2. **Stage files if needed**: 
   - If no files staged, ask user what to stage or run `git add .`
   - If specific files mentioned, stage only those
3. **Analyze changes**: Use `git diff --cached` and `git diff --stat` to understand what changed
4. **Review file changes**: Look at the actual modified files to understand the scope
5. **Generate commit message**: Follow the rules in `.cline-git-commit-rules`
   - Use Conventional Commits format
   - Include type and scope
   - Write summary + bullet points
   - Note any breaking changes
6. **Present for approval**: Show the generated message to the user
7. **Execute commit**: After approval, run `git commit -m "message"`
8. **Push if requested**: If user said "push", execute `git push` after committing

### Message Generation Guidelines

- **Analyze the diff**: Don't just describe files changed, understand what the changes DO
- **Infer intent**: Look at patterns in the changes to determine the appropriate type (feat/fix/test/etc.)
- **Be specific**: Use concrete scopes based on the files changed
- **List details**: Provide 3-7 bullet points describing key changes
- **Check for breaking changes**: Look for API changes, signature changes, behavior changes

### Example Triggers

User says any of:
- "check in"
- "commit these changes"
- "push this"
- "let's commit"
- "generate a commit message"

You automatically:
1. Check git status
2. Analyze the changes
3. Generate proper commit message per `.cline-git-commit-rules`
4. Show message for approval
5. Execute the commit
6. Push if requested

**Reference**: See `.cline-git-commit-rules` for detailed formatting rules and examples.
