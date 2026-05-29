namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "AlphaNumericSender")]
public class AlphaNumericSenderTests
{
    [Fact]
    public void Should_SetProperties_When_ConstructorWithRequired()
    {
        var sender = new AlphaNumericSender("MyBrand");

        Assert.Equal("MyBrand", sender.BrandName);
        Assert.Equal("MyBrand", sender.Name);
        Assert.Equal("MyBrand", sender.DisplayName);
        Assert.True(sender.IsActive);
    }

    [Fact]
    public void Should_SetOptionalName_When_ConstructorWithName()
    {
        var sender = new AlphaNumericSender("MyBrand", name: "brand-sender");

        Assert.Equal("brand-sender", sender.Name);
    }

    [Fact]
    public void Should_SetDisplayName_When_ConstructorWithDisplayName()
    {
        var sender = new AlphaNumericSender("MyBrand", displayName: "My Brand");

        Assert.Equal("My Brand", sender.DisplayName);
    }

    [Fact]
    public void Should_SetInactive_When_ConstructorWithIsActiveFalse()
    {
        var sender = new AlphaNumericSender("MyBrand", isActive: false);

        Assert.False(sender.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_ConstructorWithInvalidBrandName(string brand)
    {
        Assert.Throws<ArgumentException>(() => new AlphaNumericSender(brand));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ConstructorWithNullBrandName()
    {
        Assert.Throws<ArgumentNullException>(() => new AlphaNumericSender(null!));
    }

    [Fact]
    public void Should_HaveLabelEndpointType()
    {
        var sender = new AlphaNumericSender("MyBrand");
        Assert.Equal(EndpointType.Label, sender.Type);
    }

    [Fact]
    public void Should_ReturnBrandNameAsAddress()
    {
        var sender = new AlphaNumericSender("MyBrand");
        Assert.Equal("MyBrand", sender.Address);
    }
}
