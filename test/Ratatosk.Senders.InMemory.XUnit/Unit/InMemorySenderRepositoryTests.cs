using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "InMemorySenderRepository")]
public class InMemorySenderRepositoryTests
{
    private static SenderEntity CreateEntity(string name = "test-sender", string address = "+1234567890", EndpointType type = EndpointType.PhoneNumber, bool isActive = true)
    {
        var entity = new SenderEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            DisplayName = name,
            Address = address,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        if (isActive)
            entity.Activate();
        else
            entity.Deactivate();
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

    [Fact]
    public async Task Should_FindByName_When_SenderExists()
    {
        // Arrange
        var entity = CreateEntity("my-sender");
        var store = new InMemorySenderRepository(new[] { entity });

        // Act
        var result = await store.FindByNameAsync("my-sender");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("my-sender", result.Name);
    }

    [Fact]
    public async Task Should_ReturnNull_When_FindByNameAndSenderNotFound()
    {
        // Arrange
        var store = new InMemorySenderRepository();

        // Act
        var result = await store.FindByNameAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Should_FindByEndpoint_When_SenderExists()
    {
        // Arrange
        var entity = CreateEntity("email-sender", "test@example.com", EndpointType.EmailAddress);
        var store = new InMemorySenderRepository(new[] { entity });

        // Act
        var result = await store.FindByEndpointAsync("test@example.com", EndpointType.EmailAddress);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("email-sender", result.Name);
    }

    [Fact]
    public async Task Should_ReturnNull_When_FindByEndpointAndSenderNotFound()
    {
        // Arrange
        var store = new InMemorySenderRepository();

        // Act
        var result = await store.FindByEndpointAsync("nonexistent@example.com", EndpointType.EmailAddress);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnAllActiveSenders_When_GetAllActiveAsync()
    {
        // Arrange
        var active1 = CreateEntity("active1", isActive: true);
        var active2 = CreateEntity("active2", isActive: true);
        var inactive = CreateEntity("inactive", isActive: false);
        var store = new InMemorySenderRepository(new[] { active1, active2, inactive });

        // Act
        var result = await store.GetAllActiveAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Name == "active1");
        Assert.Contains(result, s => s.Name == "active2");
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoActiveSenders()
    {
        // Arrange
        var inactive1 = CreateEntity("inactive1", isActive: false);
        var inactive2 = CreateEntity("inactive2", isActive: false);
        var store = new InMemorySenderRepository(new[] { inactive1, inactive2 });

        // Act
        var result = await store.GetAllActiveAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_ActivateSender_When_SetActiveAsyncTrue()
    {
        // Arrange
        var entity = CreateEntity("test-sender", isActive: false);
        var store = new InMemorySenderRepository(new[] { entity });

        // Act
        await store.SetActiveAsync(entity, true);

        // Assert
        Assert.True(entity.IsActive);
    }

    [Fact]
    public async Task Should_DeactivateSender_When_SetActiveAsyncFalse()
    {
        // Arrange
        var entity = CreateEntity("test-sender", isActive: true);
        var store = new InMemorySenderRepository(new[] { entity });

        // Act
        await store.SetActiveAsync(entity, false);

        // Assert
        Assert.False(entity.IsActive);
    }

    [Fact]
    public async Task Should_AddEntity_When_AddAsync()
    {
        // Arrange
        var store = new InMemorySenderRepository();
        var entity = CreateEntity("new-sender");

        // Act
        await store.AddAsync(entity);

        // Assert
        var result = await store.FindByNameAsync("new-sender");
        Assert.NotNull(result);
        Assert.Equal("new-sender", result.Name);
    }

    [Fact]
    public async Task Should_UpdateEntity_When_UpdateAsync()
    {
        // Arrange
        var entity = CreateEntity("test-sender");
        var store = new InMemorySenderRepository(new[] { entity });

        // Act
        entity.DisplayName = "Updated Name";
        await store.UpdateAsync(entity);

        // Assert
        var result = await store.FindAsync(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result!.DisplayName);
    }

    [Fact]
    public async Task Should_RemoveEntity_When_RemoveAsync()
    {
        // Arrange
        var entity = CreateEntity("test-sender");
        var store = new InMemorySenderRepository(new[] { entity });

        // Act
        await store.RemoveAsync(entity);

        // Assert
        var result = await store.FindAsync(entity.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task Should_FindById_When_FindAsync()
    {
        // Arrange
        var entity = CreateEntity("test-sender");
        entity.Id = "specific-id";
        var store = new InMemorySenderRepository(new[] { entity });

        // Act
        var result = await store.FindAsync("specific-id");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("specific-id", result.Id);
    }

    [Fact]
    public async Task Should_ReturnNull_When_FindAsyncAndNotFound()
    {
        // Arrange
        var store = new InMemorySenderRepository();

        // Act
        var result = await store.FindAsync("nonexistent-id");

        // Assert
        Assert.Null(result);
    }
}
