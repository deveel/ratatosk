namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "EmailSender")]
public class EmailSenderTests
{
    [Fact]
    public void Should_SetProperties_When_ConstructorWithRequired()
    {
        var sender = new EmailSender("test@example.com");

        Assert.Equal("test@example.com", sender.Address);
        Assert.Equal("test@example.com", sender.Name);
        Assert.Equal("test@example.com", sender.DisplayName);
        Assert.True(sender.IsActive);
    }

    [Fact]
    public void Should_SetDisplayName_When_ConstructorWithDisplayName()
    {
        var sender = new EmailSender("test@example.com", displayName: "Test User");

        Assert.Equal("Test User", sender.DisplayName);
    }

    [Fact]
    public void Should_SetOptionalName_When_ConstructorWithName()
    {
        var sender = new EmailSender("test@example.com", name: "my-email");

        Assert.Equal("my-email", sender.Name);
    }

    [Fact]
    public void Should_DisplayNameFallbackToName_When_NoDisplayName()
    {
        var sender = new EmailSender("test@example.com", name: "my-email");

        Assert.Equal("my-email", sender.DisplayName);
    }

    [Fact]
    public void Should_SetInactive_When_ConstructorWithIsActiveFalse()
    {
        var sender = new EmailSender("test@example.com", isActive: false);

        Assert.False(sender.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_ConstructorWithInvalidAddress(string address)
    {
        Assert.Throws<ArgumentException>(() => new EmailSender(address));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ConstructorWithNullAddress()
    {
        Assert.Throws<ArgumentNullException>(() => new EmailSender(null!));
    }

    [Fact]
    public void Should_HaveEmailAddressEndpointType()
    {
        var sender = new EmailSender("test@example.com");
        Assert.Equal(EndpointType.EmailAddress, sender.Type);
    }
}
