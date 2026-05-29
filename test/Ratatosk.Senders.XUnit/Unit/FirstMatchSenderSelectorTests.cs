namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "FirstMatchSenderSelector")]
public class FirstMatchSenderSelectorTests
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
    public async Task Should_ReturnFirstActive_When_MultipleSenders()
    {
        var selector = new FirstMatchSenderSelector();
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
    public async Task Should_SkipInactive_When_FirstIsInactive()
    {
        var selector = new FirstMatchSenderSelector();
        var senders = new List<SenderEntity>
        {
            CreateEntity("first", isActive: false),
            CreateEntity("second"),
            CreateEntity("third")
        };

        var result = await selector.SelectAsync(senders);

        Assert.NotNull(result);
        Assert.Equal("second", result.Name);
    }

    [Fact]
    public async Task Should_ReturnNull_When_NoneActive()
    {
        var selector = new FirstMatchSenderSelector();
        var senders = new List<SenderEntity>
        {
            CreateEntity("first", isActive: false),
            CreateEntity("second", isActive: false)
        };

        var result = await selector.SelectAsync(senders);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_EmptyList()
    {
        var selector = new FirstMatchSenderSelector();
        var senders = new List<SenderEntity>();

        var result = await selector.SelectAsync(senders);

        Assert.Null(result);
    }
}
