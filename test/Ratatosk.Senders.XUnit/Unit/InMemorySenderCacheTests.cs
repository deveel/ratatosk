namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "InMemorySenderCache")]
public class InMemorySenderCacheTests
{
    private static ISender CreateSender(string name = "test-sender") => new Sender
    {
        Id = Guid.NewGuid().ToString(),
        Name = name,
        DisplayName = "Test Sender",
        Address = "+1234567890",
        EndpointType = EndpointType.PhoneNumber,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Should_ReturnNull_When_GetByNameForMissingKey()
    {
        var cache = new InMemorySenderCache();

        var result = await cache.GetByNameAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnSender_When_SetThenGetByName()
    {
        var cache = new InMemorySenderCache();
        var sender = CreateSender();

        await cache.SetByNameAsync("test-sender", sender);
        var result = await cache.GetByNameAsync("test-sender");

        Assert.NotNull(result);
        Assert.Equal(sender.Name, result.Name);
    }

    [Fact]
    public async Task Should_ReturnNull_When_EntryExpired()
    {
        var cache = new InMemorySenderCache(TimeSpan.FromMilliseconds(50));
        var sender = CreateSender();

        await cache.SetByNameAsync("test-sender", sender, TimeSpan.FromMilliseconds(10));
        await Task.Delay(100);

        var result = await cache.GetByNameAsync("test-sender");
        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_Removed()
    {
        var cache = new InMemorySenderCache();
        var sender = CreateSender();

        await cache.SetByNameAsync("test-sender", sender);
        await cache.RemoveByNameAsync("test-sender");

        var result = await cache.GetByNameAsync("test-sender");
        Assert.Null(result);
    }

    [Fact]
    public async Task Should_RespectCustomTtl_When_SetWithTtl()
    {
        var cache = new InMemorySenderCache(TimeSpan.FromHours(1));
        var sender = CreateSender();

        await cache.SetByNameAsync("test-sender", sender, TimeSpan.FromMilliseconds(50));
        await Task.Delay(100);

        var result = await cache.GetByNameAsync("test-sender");
        Assert.Null(result);
    }

    [Fact]
    public async Task Should_BeCaseInsensitive_When_GetByName()
    {
        var cache = new InMemorySenderCache();
        var sender = CreateSender("Test-Sender");

        await cache.SetByNameAsync("Test-Sender", sender);

        var result = await cache.GetByNameAsync("test-sender");
        Assert.NotNull(result);
    }
}
