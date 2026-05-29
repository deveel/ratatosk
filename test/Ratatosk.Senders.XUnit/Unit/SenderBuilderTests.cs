namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "SenderBuilder")]
public class SenderBuilderTests
{
    [Fact]
    public void Should_BuildEntity_When_AllPropertiesSet()
    {
        var entity = new SenderBuilder()
            .WithName("test-sender")
            .WithDisplayName("Test Sender")
            .WithAddress("+1234567890")
            .WithEndpointType(EndpointType.PhoneNumber)
            .AsActive(true)
            .Build();

        Assert.Equal("test-sender", entity.Name);
        Assert.Equal("Test Sender", entity.DisplayName);
        Assert.Equal("+1234567890", entity.Address);
        Assert.Equal("PhoneNumber", entity.EndpointType);
        Assert.True(entity.IsActive);
        Assert.NotNull(entity.Id);
        Assert.NotNull(entity.CreatedAt);
        Assert.NotNull(entity.UpdatedAt);
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
        var entity = new SenderBuilder()
            .WithName("test-sender")
            .WithAddress("+1234567890")
            .WithEndpointType(EndpointType.PhoneNumber)
            .Build();

        Assert.Equal("test-sender", entity.DisplayName);
    }

    [Fact]
    public void Should_BeActiveByDefault()
    {
        var entity = new SenderBuilder()
            .WithName("test-sender")
            .WithAddress("+1234567890")
            .WithEndpointType(EndpointType.PhoneNumber)
            .Build();

        Assert.True(entity.IsActive);
    }

    [Fact]
    public void Should_SupportEndpointTypeAsString()
    {
        var entity = new SenderBuilder()
            .WithName("test-sender")
            .WithAddress("test@example.com")
            .WithEndpointType("email")
            .Build();

        Assert.Equal("email", entity.EndpointType);
    }

    [Fact]
    public void Should_SupportChaining()
    {
        var entity = new SenderBuilder()
            .WithName("test")
            .WithDisplayName("Test")
            .WithAddress("addr")
            .WithEndpointType(EndpointType.EmailAddress)
            .AsActive(false)
            .Build();

        Assert.False(entity.IsActive);
    }
}
