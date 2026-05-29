namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderRef")]
public class SenderRefTests
{
    [Fact]
    public void Should_CreateWithName_When_ConstructorWithString()
    {
        var senderRef = new SenderRef("my-sender");

        Assert.Equal("my-sender", senderRef.SenderName);
        Assert.Equal("my-sender", senderRef.Name);
        Assert.Equal("my-sender", senderRef.DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_ConstructorWithInvalidString(string name)
    {
        Assert.Throws<ArgumentException>(() => new SenderRef(name));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ConstructorWithNullString()
    {
        Assert.Throws<ArgumentNullException>(() => new SenderRef((string)null!));
    }

    [Fact]
    public void Should_CreateWithSenderName_When_ConstructorWithISender()
    {
        var sender = new PhoneSender("+1234567890", name: "my-phone");

        var senderRef = new SenderRef(sender);

        Assert.Equal("my-phone", senderRef.SenderName);
        Assert.Equal("my-phone", senderRef.Name);
    }

    [Fact]
    public void Should_ThrowNullReferenceException_When_ConstructorWithNullISender()
    {
        Assert.Throws<NullReferenceException>(() => new SenderRef((ISender)null!));
    }

    [Fact]
    public void Should_BeAlwaysActive()
    {
        var senderRef = new SenderRef("test");
        Assert.True(senderRef.IsActive);
    }

    [Fact]
    public void Should_HaveAnyEndpointType()
    {
        var senderRef = new SenderRef("test");
        Assert.Equal(EndpointType.Any, senderRef.Type);
    }

    [Fact]
    public void Should_ReturnNameAsAddress()
    {
        var senderRef = new SenderRef("test");
        Assert.Equal("test", senderRef.Address);
    }
}
