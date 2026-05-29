namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "RoundRobinSenderSelector")]
public class RoundRobinSenderSelectorTests
{
    private static SenderEntity CreateEntity(string name, bool isActive = true) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = name,
        DisplayName = name,
        Address = $"{name}@test.com",
        EndpointType = "email",
        IsActive = isActive
    };

    [Fact]
    public async Task Should_ReturnFirst_When_FirstCall()
    {
        var selector = new RoundRobinSenderSelector();
        var senders = new List<SenderEntity>
        {
            CreateEntity("first"),
            CreateEntity("second"),
            CreateEntity("third")
        };

        var result = await selector.SelectAsync(senders);

        Assert.NotNull(result);
        Assert.Equal("first", result.Name);
    }

    [Fact]
    public async Task Should_Rotate_When_MultipleCalls()
    {
        var selector = new RoundRobinSenderSelector();
        var senders = new List<SenderEntity>
        {
            CreateEntity("first"),
            CreateEntity("second"),
            CreateEntity("third")
        };

        var first = await selector.SelectAsync(senders);
        var second = await selector.SelectAsync(senders);
        var third = await selector.SelectAsync(senders);
        var fourth = await selector.SelectAsync(senders);

        Assert.Equal("first", first!.Name);
        Assert.Equal("second", second!.Name);
        Assert.Equal("third", third!.Name);
        Assert.Equal("first", fourth!.Name);
    }

    [Fact]
    public async Task Should_SkipInactive_When_Rotating()
    {
        var selector = new RoundRobinSenderSelector();
        var senders = new List<SenderEntity>
        {
            CreateEntity("first"),
            CreateEntity("second", isActive: false),
            CreateEntity("third")
        };

        var results = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            var result = await selector.SelectAsync(senders);
            results.Add(result!.Name);
        }

        Assert.DoesNotContain("second", results);
    }

    [Fact]
    public async Task Should_ReturnNull_When_NoneActive()
    {
        var selector = new RoundRobinSenderSelector();
        var senders = new List<SenderEntity>
        {
            CreateEntity("first", isActive: false)
        };

        var result = await selector.SelectAsync(senders);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_BeThreadSafe_When_ConcurrentCalls()
    {
        var selector = new RoundRobinSenderSelector();
        var senders = new List<SenderEntity>
        {
            CreateEntity("first"),
            CreateEntity("second"),
            CreateEntity("third")
        };

        var tasks = Enumerable.Range(0, 100).Select(_ =>
            Task.Run(() => selector.SelectAsync(senders).AsTask()));

        var results = await Task.WhenAll(tasks);

        Assert.All(results, r => Assert.NotNull(r));
    }
}
