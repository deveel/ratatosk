namespace Ratatosk.Senders;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderBuilder")]
public class SenderBuilderTests
{
    [Fact]
    public void Should_BuildSender_When_AllPropertiesSet()
    {
        var sender = new SenderBuilder()
            .WithName("test-sender")
            .WithDisplayName("Test Sender")
            .WithAddress("+1234567890")
            .WithEndpointType(EndpointType.PhoneNumber)
            .AsActive(true)
            .Build();

        Assert.Equal("test-sender", sender.Name);
        Assert.Equal("Test Sender", sender.DisplayName);
        Assert.Equal("+1234567890", sender.Address);
        Assert.Equal(EndpointType.PhoneNumber, sender.EndpointType);
        Assert.True(sender.IsActive);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_NameNotSet()
    {
        var builder = new SenderBuilder()
            .WithAddress("+1234567890")
            .WithEndpointType(EndpointType.PhoneNumber);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_AddressNotSet()
    {
        var builder = new SenderBuilder()
            .WithName("test-sender")
            .WithEndpointType(EndpointType.PhoneNumber);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_EndpointTypeNotSet()
    {
        var builder = new SenderBuilder()
            .WithName("test-sender")
            .WithAddress("+1234567890");

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Should_DefaultDisplayNameToName_When_NotSet()
    {
        var sender = new SenderBuilder()
            .WithName("test-sender")
            .WithAddress("+1234567890")
            .WithEndpointType(EndpointType.PhoneNumber)
            .Build();

        Assert.Equal("test-sender", sender.DisplayName);
    }

    [Fact]
    public void Should_BeActiveByDefault()
    {
        var sender = new SenderBuilder()
            .WithName("test-sender")
            .WithAddress("+1234567890")
            .WithEndpointType(EndpointType.PhoneNumber)
            .Build();

        Assert.True(sender.IsActive);
    }

    [Fact]
    public void Should_SupportChaining()
    {
        var sender = new SenderBuilder()
            .WithName("test")
            .WithDisplayName("Test")
            .WithAddress("addr")
            .WithEndpointType(EndpointType.EmailAddress)
            .AsActive(false)
            .Build();

        Assert.False(sender.IsActive);
    }

    [Fact]
    public void Should_NotSetId_When_Built()
    {
        var sender = new SenderBuilder()
            .WithName("test-sender")
            .WithAddress("+1234567890")
            .WithEndpointType(EndpointType.PhoneNumber)
            .Build();

        Assert.Equal(string.Empty, sender.Id);
    }

    [Fact]
    public void Should_SetUpdatedAt_When_Built()
    {
        var sender = new SenderBuilder()
            .WithName("test-sender")
            .WithAddress("+1234567890")
            .WithEndpointType(EndpointType.PhoneNumber)
            .Build();

        Assert.Null(sender.CreatedAt);
        Assert.NotNull(sender.UpdatedAt);
    }
}
