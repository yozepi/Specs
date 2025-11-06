# Test Patterns: Specs Framework

## Common Test Patterns

This document illustrates common test patterns and usage examples for the Specs framework.

---

## 1. Basic Test Structure

### Simple Test Case
```csharp
public class Calculator_Tests : nSpecTestHarness
{
    [Test]
    public void Addition_Specs()
    {
        RunMySpecs();
    }
    
    void method_context()
    {
        var calculator = new Calculator();
        int result = 0;
        
        context["when adding two numbers"] = () =>
        {
            act = () => result = calculator.Add(2, 3);
            
            it["should return the sum"] = () => 
                result.should_be(5);
        };
    }
}
```

---

## 2. Hierarchical Contexts

### Nested Contexts with Shared Setup
```csharp
void describe_database_operations()
{
    Database db = null;
    
    beforeAll = () => db = new Database("test.db");
    afterAll = () => db.Dispose();
    
    context["when inserting data"] = () =>
    {
        beforeEach = () => db.Clear();
        
        context["with valid data"] = () =>
        {
            act = () => db.Insert(new User { Name = "John" });
            
            it["should succeed"] = () => 
                db.Count().should_be(1);
        };
        
        context["with invalid data"] = () =>
        {
            it["should throw validation error"] = expect<ValidationException>(
                () => db.Insert(new User { Name = "" }));
        };
    };
}
```

**Key Points**:
- Parent `beforeAll` runs once for all child contexts
- Each child context can have its own `beforeEach`
- Setup flows from parent to child
- Teardown flows from child to parent

---

## 3. Sharing State Across Tests

### Using Context Variables
```csharp
void describe_user_service()
{
    UserService service = null;
    User testUser = null;
    
    before = () =>
    {
        service = new UserService();
        testUser = new User { Name = "Alice", Age = 30 };
    };
    
    context["creating users"] = () =>
    {
        act = () => service.Create(testUser);
        
        it["should assign an ID"] = () => 
            testUser.Id.should_not_be(0);
            
        it["should add to database"] = () =>
            service.GetById(testUser.Id).should_not_be_null();
    };
}
```

**Pattern**: Variables declared at the context level are shared across all tests in that context.

---

## 4. Async Test Support

### Async Setup and Tests
```csharp
void describe_async_operations()
{
    ApiClient client = null;
    ApiResponse response = null;
    
    beforeAll = async () => 
    {
        client = new ApiClient();
        await client.ConnectAsync();
    };
    
    afterAll = async () => await client.DisconnectAsync();
    
    context["when fetching data"] = () =>
    {
        actAsync = async () => response = await client.GetAsync("/users");
        
        it["should return success"] = () => 
            response.StatusCode.should_be(200);
            
        it["should contain data"] = () =>
            response.Data.should_not_be_empty();
    };
}
```

**Key Points**:
- Use `beforeAll`, `before`, `act`, `after`, `afterAll` for sync operations
- Use `async` versions for async operations
- Framework handles async/await conversion internally

---

## 5. Parameterized Tests

### Testing Multiple Scenarios
```csharp
void describe_validation()
{
    var validator = new EmailValidator();
    
    new[]
    {
        ("valid@email.com", true),
        ("invalid.email", false),
        ("", false),
        ("test@domain", false),
        ("user@domain.co.uk", true)
    }.Do(testCase =>
    {
        context[$"when validating '{testCase.Item1}'"] = () =>
        {
            bool result = false;
            
            act = () => result = validator.IsValid(testCase.Item1);
            
            it[$"should return {testCase.Item2}"] = () =>
                result.should_be(testCase.Item2);
        };
    });
}
```

**Pattern**: Use array iteration with `.Do()` to generate multiple test contexts.

---

## 6. Exception Testing

### Expecting Exceptions
```csharp
void describe_error_handling()
{
    var parser = new JsonParser();
    
    context["when parsing invalid JSON"] = () =>
    {
        it["should throw JsonException"] = expect<JsonException>(
            () => parser.Parse("{ invalid json }"));
    };
    
    context["when parsing null input"] = () =>
    {
        it["should throw ArgumentNullException"] = expect<ArgumentNullException>(
            () => parser.Parse(null));
    };
}
```

**Pattern**: Use `expect<TException>()` to assert that an exception is thrown.

---

## 7. Integration Testing Pattern

### Testing with External Dependencies
```csharp
void describe_order_processing()
{
    TestDatabase db = null;
    OrderService service = null;
    Order testOrder = null;
    
    beforeAll = () =>
    {
        // Setup test database
        db = TestDatabase.Create();
        db.Seed();
    };
    
    afterAll = () => db.Dispose();
    
    before = () =>
    {
        // Fresh service per test
        service = new OrderService(db);
        testOrder = new Order { CustomerId = 1, Items = CreateTestItems() };
    };
    
    after = () => db.Reset();
    
    context["when order is valid"] = () =>
    {
        act = () => service.ProcessOrder(testOrder);
        
        it["should save to database"] = () =>
            db.Orders.Count().should_be(1);
            
        it["should update inventory"] = () =>
            db.Inventory.Should().Reflect(testOrder.Items);
            
        it["should send confirmation"] = () =>
            service.SentEmails.should_contain(testOrder.CustomerId);
    };
    
    context["when payment fails"] = () =>
    {
        before = () => service.PaymentGateway = new FailingPaymentGateway();
        
        it["should throw PaymentException"] = expect<PaymentException>(
            () => service.ProcessOrder(testOrder));
            
        it["should not save order"] = () =>
            db.Orders.Count().should_be(0);
    };
}
```

**Key Points**:
- `beforeAll`/`afterAll`: Expensive setup (database connection)
- `before`/`after`: Per-test setup (fresh service instance, reset data)
- Nested contexts can override dependencies

---

## 8. Test Isolation Pattern

### Ensuring Test Independence
```csharp
void describe_isolated_tests()
{
    // BAD: Shared mutable state
    static int counter = 0; // Don't do this!
    
    // GOOD: Fresh state per test
    int counter = 0; // Reset for each test
    
    context["first test"] = () =>
    {
        act = () => counter++;
        it["should be 1"] = () => counter.should_be(1);
    };
    
    context["second test"] = () =>
    {
        act = () => counter++;
        it["should be 1"] = () => counter.should_be(1); // Still 1!
    };
}
```

**Best Practice**: Always reset state in `before` or use fresh instances per test.

---

## 9. Arrange-Act-Assert Pattern

### Clear Test Structure
```csharp
void describe_shopping_cart()
{
    ShoppingCart cart = null;
    Product product = null;
    
    // Arrange
    before = () =>
    {
        cart = new ShoppingCart();
        product = new Product { Id = 1, Price = 10.00m };
    };
    
    context["adding items"] = () =>
    {
        // Act
        act = () => cart.Add(product, quantity: 2);
        
        // Assert
        it["should have 2 items"] = () =>
            cart.ItemCount.should_be(2);
            
        it["should total 20.00"] = () =>
            cart.Total.should_be(20.00m);
    };
}
```

**Pattern**:
- **Arrange**: Setup in `before`
- **Act**: Action in `act`
- **Assert**: Expectations in `it`

---

## 10. Testing with Mocks

### Using Mock Objects
```csharp
void describe_notification_service()
{
    NotificationService service = null;
    IEmailClient mockEmailClient = null;
    User testUser = null;
    
    before = () =>
    {
        mockEmailClient = Substitute.For<IEmailClient>();
        service = new NotificationService(mockEmailClient);
        testUser = new User { Email = "test@example.com" };
    };
    
    context["sending welcome email"] = () =>
    {
        act = () => service.SendWelcomeEmail(testUser);
        
        it["should call email client"] = () =>
            mockEmailClient.Received().SendEmail(
                testUser.Email, 
                "Welcome!",
                Arg.Any<string>());
                
        it["should log the action"] = () =>
            service.LastAction.should_be("WelcomeEmail sent");
    };
}
```

**Pattern**: Create mocks in `before`, verify interactions in assertions.

---

## 11. Builder Pattern for Test Data

### Creating Complex Test Objects
```csharp
void describe_complex_objects()
{
    context["with complete order"] = () =>
    {
        Order order = null;
        
        before = () =>
        {
            order = new OrderBuilder()
                .WithCustomer("John Doe")
                .WithAddress("123 Main St")
                .WithItem("Widget", 10.00m, quantity: 2)
                .WithItem("Gadget", 15.00m, quantity: 1)
                .WithShipping(ShippingMethod.Express)
                .Build();
        };
        
        act = () => OrderProcessor.Process(order);
        
        it["should calculate total correctly"] = () =>
            order.Total.should_be(35.00m);
    };
}
```

**Benefits**:
- Readable test data setup
- Reusable across tests
- Easy to modify for different scenarios

---

## 12. Focus Tags for Debugging

### Running Specific Tests
```csharp
void describe_feature()
{
    context["normal test"] = () =>
    {
        it["runs when all tests run"] = () => true.should_be_true();
    };
    
    // Focus on this test during development
    fcontext["debugging test"] = () =>
    {
        it["runs exclusively when focused"] = () => true.should_be_true();
    };
}
```

**Usage**: 
- `fcontext` or `fit`: Focus on specific contexts/tests
- Only focused tests run when any focus tag exists
- Remove focus tags before committing

---

## Best Practices Summary

1. **One Assertion per Test**: Each `it` should test one thing
2. **Descriptive Names**: Use clear, descriptive context and test names
3. **Isolated Tests**: Tests should not depend on execution order
4. **Minimal Setup**: Only setup what's needed for the test
5. **Consistent Structure**: Follow
