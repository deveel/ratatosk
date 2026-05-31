namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "PhoneSender")]
public class PhoneSenderTests
{
    [Fact]
    public void Should_SetProperties_When_ConstructorWithRequired()
    {
        var sender = new PhoneSender("+1234567890");

        Assert.Equal("+1234567890", sender.PhoneNumber);
        Assert.Equal("+1234567890", sender.Name);
        Assert.Equal("+1234567890", sender.DisplayName);
        Assert.True(sender.IsActive);
    }

    [Fact]
    public void Should_SetOptionalName_When_ConstructorWithName()
    {
        var sender = new PhoneSender("+1234567890", name: "my-phone");

        Assert.Equal("my-phone", sender.Name);
    }

    [Fact]
    public void Should_SetDisplayName_When_ConstructorWithDisplayName()
    {
        var sender = new PhoneSender("+1234567890", displayName: "My Phone");

        Assert.Equal("My Phone", sender.DisplayName);
    }

    [Fact]
    public void Should_SetInactive_When_ConstructorWithIsActiveFalse()
    {
        var sender = new PhoneSender("+1234567890", isActive: false);

        Assert.False(sender.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_ConstructorWithInvalidPhone(string phone)
    {
        Assert.Throws<ArgumentException>(() => new PhoneSender(phone));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ConstructorWithNullPhone()
    {
        Assert.Throws<ArgumentNullException>(() => new PhoneSender(null!));
    }

    [Fact]
    public void Should_HavePhoneNumberEndpointType()
    {
        var sender = new PhoneSender("+1234567890");
        Assert.Equal(EndpointType.PhoneNumber, sender.Type);
    }

    [Fact]
    public void Should_ReturnPhoneNumberAsAddress()
    {
        var sender = new PhoneSender("+1234567890");
        Assert.Equal("+1234567890", sender.Address);
    }
}
