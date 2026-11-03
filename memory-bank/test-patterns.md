# Test Patterns: Specs Framework

## Suite → Feature Set → Specification Hierarchy

The Specs framework organizes tests in a clear hierarchy:

1. **Suite** (Specification class) — One class per subject under test
2. **Feature Set** (`[Describe]` decorated methods) — One method per feature/aspect of the subject
3. **Scenarios** (`Context[...]`/`Describe[...]` indexers) — Organize specs within a feature set
4. **Specifications** (`It[...]` indexer) — Individual test assertions

This is a single paradigm, not separate approaches. The `[Describe]` attribute and `Context[...]` indexer work together at different levels of the hierarchy.

---

## 1. Complete Suite Structure

### The Full Hierarchy
```csharp
public class Weather_Controller : Specifications  // SUITE: one per subject
{
    // SUITE-LEVEL LIFECYCLE: applies to ALL feature sets
    BeforeAll(() => { /* once before any feature runs */ });
    AfterAll(() => { /* once after all features complete */ });
    BeforeEach(() => { /* before each spec across ALL features */ });
    AfterEach(() => { /* after each spec across ALL features */ });

    [Describe("Getting forecasts")]  // FEATURE SET: one aspect of the subject
    public void getting_forecasts()
    {
        // FEATURE-LEVEL LIFECYCLE: applies only to specs in THIS feature
        BeforeEach(() => { /* before each spec in this feature */ });
        AfterEach(() => { /* after each spec in this feature */ });

        Context["when requesting today's forecast"] = () =>  // SCENARIO
        {
            Act(() => { /* action under test */ });

            It["should return current temperature"] = () =>  // SPEC
            {
                result.Temperature.Should().NotBeNull();
            };

            It["should include humidity"] = () =>  // SPEC
            {
                result.Humidity.Should().BeGreaterThan(0);
            };
        };

        Context["when requesting weekly forecast"] = () =>  // ANOTHER SCENARIO
        {
            Act(() => { /* different action */ });

            It["should return 7 days"] = () =>
            {
                results.Count.Should().Be(7);
            };
        };
    }

    [Describe("Updating weather data")]  // ANOTHER FEATURE SET
    public void updating_weather_data()
    {
        // Different lifecycle for this feature
        BeforeEach(() => { /* different setup for update specs */ });

        Describe["with valid data"] = () =>
        {
            Act(() => { /* update action */ });

            It["should persist changes"] = () => { /* assertion */ };
        };

        Describe["with invalid data"] = () =>
        {
            It["should reject the update"] = () => { /* assertion */ };
        };
    }
}
```

**Key Points**:
- The class IS the suite (one subject under test)
- `[Describe]` methods define feature sets within the suite
- `Context[...]`/`Describe[...]` organize scenarios within a feature set
- `It[...]` defines individual specifications
- Suite-level lifecycle wraps all feature sets
- Feature-level lifecycle is scoped to its own specs

---

## 2. Lifecycle Inheritance

### Two Levels of Lifecycle Control

```csharp
public class Order_Service : Specifications
{
    // SUITE LEVEL: shared across ALL feature sets
    BeforeAll(() => { db = TestDatabase.Create(); });
    AfterAll(() => { db.Dispose(); });
    BeforeEach(() => { service = new OrderService(db); });

    [Describe("Creating orders")]
    public void creating_orders()
    {
        // FEATURE LEVEL: only for specs in this feature
        BeforeEach(() => { testOrder = new Order { CustomerId = 1 }; });
        AfterEach(() => { db.Reset(); });

        Context["with valid order"] = () =>
        {
            Act(() => { service.Create(testOrder); });

            // This spec gets: suite BeforeEach → feature BeforeEach → Act → spec → feature AfterEach → suite AfterEach
            It["should save to database"] = () =>
            {
                db.Orders.Count().Should().Be(1);
            };
        };
    }

    [Describe("Canceling orders")]
    public void canceling_orders()
    {
        // Different feature, different lifecycle
        BeforeEach(() => { existingOrder = db.SeedOrder(); });

        Context["when order exists"] = () =>
        {
            Act(() => { service.Cancel(existingOrder.Id); });

            It["should mark as canceled"] = () =>
            {
                existingOrder.Status.Should().Be(OrderStatus.Canceled);
            };
        };
    }
}
```

**Lifecycle execution order for each spec**:
1. Suite `BeforeAll` (once for entire suite)
2. Suite `BeforeEach` (before each spec, all features)
3. Feature `BeforeEach` (before each spec, this feature only)
4. `Act` (action under test)
5. Spec assertion (`It[...]`)
6. Feature `AfterEach` (after each spec, this feature only)
7. Suite `AfterEach` (after each spec, all features)
8. Suite `AfterAll` (once for entire suite)

This follows the parent→child pattern:
- **Setup/Act**: Suite-level first → Feature-level second (top-down)
- **Teardown**: Feature-level first → Suite-level second (bottom-up)

---

## 3. Nested Scenarios Within a Feature

### Hierarchical Context Organization
```csharp
[Describe("User authentication")]
public void user_authentication()
{
    AuthService service = null;

    BeforeEach(() => { service = new AuthService(); });

    Context["when user has valid credentials"] = () =>
    {
        User user = null;
        BeforeEach(() => { user = new User("alice", "correct-password"); });

        Act(() => { result = service.Authenticate(user); });

        It["should return success"] = () =>
        {
            result.Success.Should().BeTrue();
        };

        It["should set auth token"] = () =>
        {
            result.Token.Should().NotBeNullOrEmpty();
        };

        Context["and user has admin role"] = () =>
        {
            BeforeEach(() => { user.AddRole("admin"); });

            It["should include admin claims"] = () =>
            {
                result.Claims.Should().Contain("admin");
            };
        };
    };

    Context["when user has invalid credentials"] = () =>
    {
        Act(() => { result = service.Authenticate(new User("alice", "wrong")); });

        It["should return failure"] = () =>
        {
            result.Success.Should().BeFalse();
        };

        It["should not set auth token"] = () =>
        {
            result.Token.Should().BeNull();
        };
    };
}
```

**Key Points**:
- Scenarios (`Context[...]`) can nest within each other
- Each nested level can add its own lifecycle hooks
- Inner scenarios inherit all outer lifecycle hooks
- Variables declared at outer scope are accessible to inner scenarios

---

## 4. Async Support

### Async Lifecycle and Tests
```csharp
[Describe("Async API operations")]
public void async_api_operations()
{
    ApiClient client = null;
    ApiResponse response = null;

    BeforeEach(async () =>
    {
        client = new ApiClient();
        await client.ConnectAsync();
    });

    AfterEach(async () => { await client.DisconnectAsync(); });

    Context["when fetching data"] = () =>
    {
        Act(async () => { response = await client.GetAsync("/users"); });

        It["should return success"] = () =>
        {
            response.StatusCode.Should().Be(200);
        };

        ItAsync["should contain valid data"] = async () =>
        {
            var data = await response.ParseAsync();
            data.Should().NotBeEmpty();
        };
    };
}
```

**Key Points**:
- All lifecycle methods support both sync and async
- Use `Act(async () => ...)` for async actions
- Use `ItAsync[...]` for async assertions
- Framework handles async/await conversion internally

---

## 5. Sharing State Across Specs

### Using Context Variables
```csharp
[Describe("User service operations")]
public void user_service_operations()
{
    UserService service = null;
    User testUser = null;

    BeforeEach(() =>
    {
        service = new UserService();
        testUser = new User { Name = "Alice", Age = 30 };
    });

    Context["creating users"] = () =>
    {
        Act(() => { service.Create(testUser); });

        It["should assign an ID"] = () =>
        {
            testUser.Id.Should().NotBe(0);
        };

        It["should add to database"] = () =>
        {
            service.GetById(testUser.Id).Should().NotBeNull();
        };
    };
}
```

**Pattern**: Variables declared at the feature or scenario level are shared across all specs in that scope.

---

## 6. Parameterized Tests

### Testing Multiple Scenarios
```csharp
[Describe("Email validation")]
public void email_validation()
{
    var validator = new EmailValidator();

    var testCases = new[]
    {
        ("valid@email.com", true),
        ("invalid.email", false),
        ("", false),
        ("test@domain", false),
        ("user@domain.co.uk", true)
    };

    foreach (var (email, expected) in testCases)
    {
        Context[$"when validating '{email}'"] = () =>
        {
            bool result = false;

            Act(() => { result = validator.IsValid(email); });

            It[$"should return {expected}"] = () =>
            {
                result.Should().Be(expected);
            };
        };
    }
}
```

**Pattern**: Use loops to generate multiple scenarios with different input values.

---

## 7. Exception Testing

### Expecting Exceptions
```csharp
[Describe("Error handling")]
public void error_handling()
{
    var parser = new JsonParser();

    Context["when parsing invalid JSON"] = () =>
    {
        It["should throw JsonException"] = expect<JsonException>(
            () => parser.Parse("{ invalid json }"));
    };

    Context["when parsing null input"] = () =>
    {
        It["should throw ArgumentNullException"] = expect<ArgumentNullException>(
            () => parser.Parse(null));
    };
}
```

**Pattern**: Use `expect<TException>()` to assert that an exception is thrown.

---

## 8. Testing with Mocks

### Using Mock Objects
```csharp
[Describe("Notification service")]
public void notification_service()
{
    NotificationService service = null;
    IEmailClient mockEmailClient = null;
    User testUser = null;

    BeforeEach(() =>
    {
        mockEmailClient = Substitute.For<IEmailClient>();
        service = new NotificationService(mockEmailClient);
        testUser = new User { Email = "test@example.com" };
    });

    Context["sending welcome email"] = () =>
    {
        Act(() => { service.SendWelcomeEmail(testUser); });

        It["should call email client"] = () =>
        {
            mockEmailClient.Received().SendEmail(
                testUser.Email,
                "Welcome!",
                Arg.Any<string>());
        };

        It["should log the action"] = () =>
        {
            service.LastAction.Should().Be("WelcomeEmail sent");
        };
    };
}
```

**Pattern**: Create mocks in `BeforeEach`, verify interactions in assertions.

---

## 9. Test Isolation

### Ensuring Spec Independence
```csharp
[Describe("Isolated specs")]
public void isolated_specs()
{
    // GOOD: Fresh state per spec via BeforeEach
    int counter = 0;
    BeforeEach(() => { counter = 0; });

    Context["first spec"] = () =>
    {
        Act(() => { counter++; });
        It["should be 1"] = () => { counter.Should().Be(1); };
    };

    Context["second spec"] = () =>
    {
        Act(() => { counter++; });
        It["should also be 1"] = () => { counter.Should().Be(1); }; // Still 1!
    };
}
```

**Best Practice**: Always reset state in `BeforeEach` or use fresh instances per spec. Never use static mutable state.

---

## 10. Arrange-Act-Assert Pattern

### Clear Spec Structure
```csharp
[Describe("Shopping cart")]
public void shopping_cart()
{
    ShoppingCart cart = null;
    Product product = null;

    // ARRANGE
    BeforeEach(() =>
    {
        cart = new ShoppingCart();
        product = new Product { Id = 1, Price = 10.00m };
    });

    Context["adding items"] = () =>
    {
        // ACT
        Act(() => { cart.Add(product, quantity: 2); });

        // ASSERT
        It["should have 2 items"] = () =>
        {
            cart.ItemCount.Should().Be(2);
        };

        It["should total 20.00"] = () =>
        {
            cart.Total.Should().Be(20.00m);
        };
    };
}
```

**Pattern**:
- **Arrange**: Setup in `BeforeEach`
- **Act**: Action in `Act()`
- **Assert**: Expectations in `It[...]`

---

## 11. Builder Pattern for Test Data

### Creating Complex Test Objects
```csharp
[Describe("Complex order processing")]
public void complex_order_processing()
{
    Context["with complete order"] = () =>
    {
        Order order = null;

        BeforeEach(() =>
        {
            order = new OrderBuilder()
                .WithCustomer("John Doe")
                .WithAddress("123 Main St")
                .WithItem("Widget", 10.00m, quantity: 2)
                .WithItem("Gadget", 15.00m, quantity: 1)
                .WithShipping(ShippingMethod.Express)
                .Build();
        });

        Act(() => { OrderProcessor.Process(order); });

        It["should calculate total correctly"] = () =>
        {
            order.Total.Should().Be(35.00m);
        };
    };
}
```

---

## Best Practices Summary

1. **One Subject per Suite**: Each specification class tests one subject
2. **One Feature per `[Describe]` Method**: Each `[Describe]` method covers one aspect/feature
3. **One Assertion per Spec**: Each `It[...]` should test one thing
4. **Descriptive Names**: Use clear names at every level (suite, feature, scenario, spec)
5. **Isolated Specs**: Specs should not depend on execution order
6. **Suite-Level for Shared Resources**: Use class-level lifecycle for expensive shared setup (DB, connections)
7. **Feature-Level for Feature Setup**: Use lifecycle inside `[Describe]` methods for feature-specific setup
8. **Minimal Setup**: Only set up what's needed at each level
9. **Consistent Structure**: Follow the Suite → Feature → Scenario → Spec hierarchy
10. **Async When Needed**: Use async lifecycle hooks and `ItAsync` only when testing async behavior
