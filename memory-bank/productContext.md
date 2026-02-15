# Product Context: Specs

## Why This Project Exists

The Specs framework was created to address fundamental challenges in .NET testing:

1. **Hierarchical Test Organization**: Standard testing frameworks lack native support for nested test contexts with inherited setup/teardown behavior
2. **BDD-Style Expressiveness**: Developers needed a more readable, behavior-focused syntax beyond traditional test methods
3. **Complex Lifecycle Management**: Modern applications require sophisticated setup/teardown orchestration that standard frameworks don't provide elegantly
4. **Async/Await Testing**: Full first-class support for asynchronous testing patterns throughout the entire test lifecycle

## Problems It Solves

### Problem 1: Test Organization and Context Inheritance
**Challenge**: Tests often share common setup but differ in specific scenarios. Traditional approaches lead to:
- Duplicated setup code across test classes
- Difficult-to-maintain test hierarchies
- No clear way to express nested scenarios

**Solution**: Specs provides a **Suite → Feature Set → Specification** hierarchy with lifecycle inheritance:
```csharp
public class Auth_Controller : Specifications  // SUITE: one subject
{
    // Suite-level: shared across ALL feature sets
    BeforeAll(() => { /* expensive shared setup */ });

    [Context("User authentication")]  // FEATURE SET
    public void user_authentication()
    {
        BeforeEach(() => { /* setup for each auth spec */ });
        
        Context["when user has valid credentials"] = () => {
            It["should authenticate successfully"] = () => { /* spec */ };
        };
        
        Context["when user has expired credentials"] = () => {
            It["should reject authentication"] = () => { /* spec */ };
        };
    }

    [Context("Token management")]  // ANOTHER FEATURE SET
    public void token_management()
    {
        BeforeEach(() => { /* different setup for token specs */ });
        // ... token-related specs
    }
}
```

### Problem 2: Test Lifecycle Complexity
**Challenge**: Complex integration tests need:
- One-time expensive setup (database initialization)
- Per-test setup (data seeding)
- Action under test
- Per-test cleanup (transaction rollback)
- One-time teardown (database cleanup)

**Solution**: Specs provides explicit lifecycle phases:
- `BeforeAll` / `SetupOnce` - Runs once before all tests
- `BeforeEach` / `SetupEach` - Runs before each test
- `Act` - The action being tested
- `AfterEach` / `TeardownEach` - Runs after each test
- `AfterAll` / `TeardownOnce` - Runs once after all tests

### Problem 3: Async Testing Throughout
**Challenge**: Modern .NET applications are heavily async, but test frameworks often have limited async support or require different methods for sync vs async operations.

**Solution**: Specs supports async/await transparently in ALL lifecycle hooks:
```csharp
BeforeAll(async () => {
    await database.InitializeAsync();
});

Act(async () => {
    result = await service.ProcessAsync(request);
});

It["should return expected result"] = () => {
    // Assertion runs after async Act completes
    result.Should().Be(expected);
};
```

### Problem 4: Readable, Expressive Test Code
**Challenge**: Traditional test methods obscure the behavior being tested behind technical names and attributes.

**Solution**: BDD-style syntax makes tests read like specifications:
```csharp
Describe["Calculator"] = () => {
    Context["when adding positive numbers"] = () => {
        It["should return the sum"] = () => { /* test */ };
        It["should handle large numbers"] = () => { /* test */ };
    };
};
```

## How It Should Work

### User Experience Goal: Natural Test Flow

Developers should be able to express tests in a natural, readable way that mirrors how they think about behavior:

1. **Describe WHAT** you're testing (component/feature)
2. **Provide CONTEXT** for when/given scenarios
3. **Define SETUP** that runs at appropriate times
4. **Specify the ACTION** being tested
5. **Assert EXPECTATIONS** with clear "It should..." statements
6. **Clean up** resources automatically at the right time

### User Experience Goal: Hierarchical Thinking

Tests should reflect natural hierarchies:
```
Calculator
├── when adding numbers
│   ├── should return sum of positive numbers
│   └── should handle negative numbers
└── when dividing numbers
    ├── should return quotient
    └── should throw on division by zero
```

### User Experience Goal: Flexible API Choices

The framework provides TWO API styles to match team preferences:

**Modern Method-Based API** (`Specifications` class):
```csharp
public class CalculatorSpecs : Specifications
{
    BeforeEach(() => {
        calculator = new Calculator();
    });
    
    Act(() => {
        result = calculator.Add(2, 3);
    });
    
    It["should return 5"] = () => {
        result.Should().Be(5);
    };
}
```

**Legacy Property-Based API** (`Spec` class):
```csharp
public class CalculatorSpecs : Spec
{
    BeforeEach = () => {
        calculator = new Calculator();
    };
    
    Act = () => {
        result = calculator.Add(2, 3);
    };
    
    It["should return 5"] = () => {
        result.Should().Be(5);
    };
}
```

### User Experience Goal: Framework Independence

While built to work with NSpec, the core framework remains independent through adapters, allowing integration with other test runners.

## What Makes This Different

### vs. Traditional XUnit/NUnit/MSTest
- **Hierarchy**: Native support for nested contexts vs flat test classes
- **Expressiveness**: BDD syntax vs method-based tests
- **Lifecycle**: Sophisticated multi-phase lifecycle vs simple setup/teardown
- **Inheritance**: Child contexts inherit parent behavior naturally

### vs. BDD Frameworks (SpecFlow, etc.)
- **Code-First**: Write tests in C# vs external Gherkin files
- **Integration**: Tests are code, not parsed text
- **Flexibility**: Full programmatic control vs DSL constraints

### vs. NSpec (Direct)
- **Architecture**: Modern builder pattern vs runtime switching
- **Immutability**: Built test cases vs mutable runtime state
- **Extensibility**: Adapter-based integration vs tight coupling
- **Choice**: Two API styles vs single approach

## Target User Scenarios

### Scenario 1: Complex Integration Tests
A developer testing a multi-layered application needs:
- Database setup once per test suite
- Transaction setup/rollback per test
- Service initialization with mocked dependencies
- Complex async workflows

**Specs Solution**: Hierarchical contexts with explicit lifecycle phases handle this elegantly.

### Scenario 2: Readable Living Documentation
A team wants tests that serve as living documentation for stakeholders.

**Specs Solution**: BDD-style syntax produces tests that read like specifications.

### Scenario 3: Legacy Migration
A team has existing tests in an older style but wants modern patterns.

**Specs Solution**: Two API styles allow gradual migration without rewriting everything.

### Scenario 4: Async-Heavy Applications
A microservices application has async operations throughout.

**Specs Solution**: Full async support in all lifecycle phases, transparent to test authors.

## Success Criteria

The framework succeeds when:

1. **Tests are readable**: Non-developers can understand what's being tested
2. **Setup is DRY**: Common setup is written once and inherited appropriately
3. **Lifecycle is clear**: It's obvious when each action runs
4. **Async is natural**: No special handling needed for async operations
5. **Organization is intuitive**: Test hierarchy mirrors conceptual relationships
6. **Integration is smooth**: Works with existing .NET test tooling

## Evolution Path

The framework evolved from a single approach (`Spec` class) to supporting two patterns:
- **Original**: Property-based API with runtime context switching
- **Modern**: Method-based API with builder pattern and immutability

Both are maintained to support existing codebases while enabling new development with modern patterns.
