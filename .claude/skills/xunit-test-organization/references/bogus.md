# Bogus — Randomized Test Data

This reference supports the `xunit-test-organization` skill's guidance on
generating realistic, reproducible test data with Bogus.

## Official Resources

- Bogus repository: <https://github.com/bchavez/Bogus>
- `Bogus` NuGet package: <https://www.nuget.org/packages/Bogus>

## Required package

```xml
<PackageReference Include="Bogus" Version="34.*" />
```

Place this in `test/Directory.Build.props` (under the unconditional `<ItemGroup>`)
so it is available to all test and support library projects automatically.

## Rules

- Always use `Faker<T>` with explicit `RuleFor` for every property — never rely
  on Bogus auto-generation without rules, as it can produce unexpected nulls
- Define one `Faker<T>` per entity type per fixture as a `static readonly` field
  — do not instantiate new `Faker<T>` inside individual test methods
- Use `f.Random.Guid()` instead of `Guid.NewGuid()` so randomization flows
  through the Bogus seed
- Use locale `"en"` explicitly when string format matters (phone numbers,
  postcodes, etc.): `new Faker<Order>("en")`
- Prefer exact-value assertions when the expected value is deterministic and
  meaningful for the test intent; when using non-deterministic randomized data,
  assert on behaviour, shape, or range instead (e.g. `Assert.True(result > 0)`)
- Use seeded fakers only when deterministic reproduction is required (e.g.
  `[MemberData]` datasets or regression reproduction)

## Faker placement in fixtures

Fakers are defined as `static readonly` fields inside the fixture that owns
the entity, not inside individual test methods. This keeps data generation
centralized and reusable.

```csharp
// test/MyLib.Testing/Fixtures/OrderServiceFixture.cs
namespace MyLib.Testing.Fixtures;

public class OrderServiceFixture
{
    // One Faker<T> per domain entity, defined once
    private static readonly Faker<Order> OrderFaker = new Faker<Order>("en")
        .RuleFor(o => o.Id,           f => f.Random.Guid())
        .RuleFor(o => o.ProductId,    f => f.Commerce.Ean13())
        .RuleFor(o => o.Quantity,     f => f.Random.Int(1, 100))
        .RuleFor(o => o.CustomerName, f => f.Name.FullName())
        .RuleFor(o => o.Email,        f => f.Internet.Email())
        .RuleFor(o => o.CreatedAt,    f => f.Date.RecentOffset().UtcDateTime);

    public OrderService Sut { get; }

    public OrderServiceFixture()
    {
        var repo = new InMemoryOrderRepository();
        Sut = new OrderService(repo);
    }

    // Builder methods delegate to the Faker with overrides for specific scenarios
    public Order BuildValidOrder() =>
        OrderFaker.Generate();

    public Order BuildOrderWithQuantity(int quantity) =>
        OrderFaker.Clone().RuleFor(o => o.Quantity, quantity).Generate();

    public IEnumerable<Order> BuildOrders(int count) =>
        OrderFaker.Generate(count);
}
```

## Seeding for reproducibility

Use a fixed seed only in `[Theory]` / `[MemberData]` scenarios where
determinism is required; elsewhere let Bogus randomize freely.

```csharp
// Deterministic seed for MemberData — use when the exact values matter
private static readonly Faker<Order> SeededOrderFaker =
    new Faker<Order>("en").UseSeed(12345)
        .RuleFor(o => o.Id,       f => f.Random.Guid())
        .RuleFor(o => o.Quantity, f => f.Random.Int(1, 100));
```

When a randomly seeded test fails, xUnit's output includes the generated data
values — capture them and promote the failing case to a named `[InlineData]`
or `[MemberData]` entry so it becomes a permanent regression test.

## `[MemberData]` with Bogus

Use `[MemberData]` with a Bogus-generated dataset for `[Theory]` tests on
complex objects:

```csharp
public static IEnumerable<object[]> InvalidOrders =>
    new Faker<Order>("en")
        .RuleFor(o => o.Id,       f => f.Random.Guid())
        .RuleFor(o => o.Quantity, f => f.Random.Int(-100, 0)) // always invalid
        .RuleFor(o => o.ProductId, f => f.Commerce.Ean13())
        .Generate(5)
        .Select(o => new object[] { o });

[Theory]
[MemberData(nameof(InvalidOrders))]
[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "OrderProcessing")]
public void Should_ThrowArgumentException_When_QuantityIsNotPositive(Order order)
{
    // Act & Assert
    var ex = Assert.Throws<ArgumentException>(
        () => _fixture.Sut.ProcessOrder(order));
    Assert.Contains("quantity", ex.Message);
}
```

## Assertion strategy with randomized data

| Data kind | Assertion approach |
|-----------|-------------------|
| Seeded / explicitly overridden value | Exact-value assertion (`Assert.Equal`) |
| Freely randomized value | Behavioural / shape assertion (`Assert.True(result > 0)`, `Assert.NotNull`) |
| Collection size | Count assertion (`Assert.Equal(5, result.Count)`) |

Never assert the exact value of a freely randomized field (e.g. `Assert.Equal("John", order.CustomerName)`) —
assert on observable behaviour or constraints instead.
