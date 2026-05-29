namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ExplicitSenderSelector")]
public class ExplicitSenderSelectorTests
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
    public async Task Should_ReturnMatchingSender_When_NameExists()
    {
        var selector = new ExplicitSenderSelector("target");
        var senders = new List<SenderEntity>
        {
            CreateEntity("first"),
            CreateEntity("target"),
            CreateEntity("third")
        };

        var result = await selector.SelectAsync(senders);

        Assert.NotNull(result);
        Assert.Equal("target", result.Name);
    }

    [Fact]
    public async Task Should_ReturnNull_When_NameNotFound()
    {
        var selector = new ExplicitSenderSelector("nonexistent");
        var senders = new List<SenderEntity>
        {
            CreateEntity("first"),
            CreateEntity("second")
        };

        var result = await selector.SelectAsync(senders);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_BeCaseInsensitive_When_Matching()
    {
        var selector = new ExplicitSenderSelector("Target");
        var senders = new List<SenderEntity>
        {
            CreateEntity("first"),
            CreateEntity("target"),
            CreateEntity("third")
        };

        var result = await selector.SelectAsync(senders);

        Assert.NotNull(result);
        Assert.Equal("target", result.Name);
    }

    [Fact]
    public async Task Should_SkipInactive_When_Matching()
    {
        var selector = new ExplicitSenderSelector("target");
        var senders = new List<SenderEntity>
        {
            CreateEntity("target", isActive: false),
            CreateEntity("other")
        };

        var result = await selector.SelectAsync(senders);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_ConstructorWithInvalidName(string name)
    {
        Assert.Throws<ArgumentException>(() => new ExplicitSenderSelector(name));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ConstructorWithNullName()
    {
        Assert.Throws<ArgumentNullException>(() => new ExplicitSenderSelector(null!));
    }
}
