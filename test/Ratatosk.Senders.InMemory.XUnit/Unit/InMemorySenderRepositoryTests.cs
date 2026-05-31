namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "InMemorySenderRepository")]
public class InMemorySenderRepositoryTests
{
    private static SenderEntity CreateEntity(string name = "test-sender")
    {
        var entity = new SenderEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            DisplayName = name,
            Address = "+1234567890",
            Type = EndpointType.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        entity.Activate();
        return entity;
    }

    [Fact]
    public void Should_BeEmpty_When_NoSeedData()
    {
        var store = new InMemorySenderRepository();

        var queryable = ((IQueryableRepository<SenderEntity, string>)store).AsQueryable();
        var count = System.Linq.Enumerable.Count(queryable);

        Assert.Equal(0, count);
    }

    [Fact]
    public void Should_ContainSeededEntities()
    {
        var entity = CreateEntity("seed-sender");
        var store = new InMemorySenderRepository(new[] { entity });

        var queryable = ((IQueryableRepository<SenderEntity, string>)store).AsQueryable();
        var entities = System.Linq.Enumerable.ToList(queryable);

        Assert.Single(entities);
        Assert.Equal("seed-sender", entities[0].Name);
    }

    [Fact]
    public void Should_ImplementIQueryableRepository()
    {
        var store = new InMemorySenderRepository();

        Assert.IsAssignableFrom<IQueryableRepository<SenderEntity>>(store);
    }
}
