namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "BotSender")]
public class BotSenderTests
{
    [Fact]
    public void Should_SetProperties_When_ConstructorWithRequired()
    {
        var sender = new BotSender("bot-123");

        Assert.Equal("bot-123", sender.PlatformId);
        Assert.Equal("bot-123", sender.Name);
        Assert.Equal("bot-123", sender.DisplayName);
        Assert.True(sender.IsActive);
    }

    [Fact]
    public void Should_SetOptionalName_When_ConstructorWithName()
    {
        var sender = new BotSender("bot-123", name: "my-bot");

        Assert.Equal("my-bot", sender.Name);
    }

    [Fact]
    public void Should_SetDisplayName_When_ConstructorWithDisplayName()
    {
        var sender = new BotSender("bot-123", displayName: "My Bot");

        Assert.Equal("My Bot", sender.DisplayName);
    }

    [Fact]
    public void Should_SetInactive_When_ConstructorWithIsActiveFalse()
    {
        var sender = new BotSender("bot-123", isActive: false);

        Assert.False(sender.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_ConstructorWithInvalidPlatformId(string id)
    {
        Assert.Throws<ArgumentException>(() => new BotSender(id));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ConstructorWithNullPlatformId()
    {
        Assert.Throws<ArgumentNullException>(() => new BotSender(null!));
    }

    [Fact]
    public void Should_HaveIdEndpointType()
    {
        var sender = new BotSender("bot-123");
        Assert.Equal(EndpointType.Id, sender.Type);
    }

    [Fact]
    public void Should_ReturnPlatformIdAsAddress()
    {
        var sender = new BotSender("bot-123");
        Assert.Equal("bot-123", sender.Address);
    }
}
