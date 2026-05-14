namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "Endpoint")]
public class EndpointExtendedTests
{
    [Fact]
    public void Should_CreateEndpoint_WithExplicitType()
    {
        var ep = new Endpoint(EndpointType.Url, "https://example.com");
        Assert.Equal(EndpointType.Url, ep.Type);
        Assert.Equal("https://example.com", ep.Address);
    }

    [Fact]
    public void Should_CreateFromIEndpoint()
    {
        var source = new Endpoint(EndpointType.PhoneNumber, "+123");
        var copy = new Endpoint((IEndpoint)source);
        Assert.Equal(EndpointType.PhoneNumber, copy.Type);
        Assert.Equal("+123", copy.Address);
    }

    [Fact]
    public void Should_CreateEmailAddress()
    {
        var ep = Endpoint.EmailAddress("user@example.com");
        Assert.Equal(EndpointType.EmailAddress, ep.Type);
        Assert.Equal("user@example.com", ep.Address);
    }

    [Fact]
    public void Should_CreatePhoneNumber()
    {
        var ep = Endpoint.PhoneNumber("+15551234567");
        Assert.Equal(EndpointType.PhoneNumber, ep.Type);
        Assert.Equal("+15551234567", ep.Address);
    }

    [Fact]
    public void Should_Create_WithStringType()
    {
        var ep = new Endpoint("email", "a@b.com");
        Assert.Equal(EndpointType.EmailAddress, ep.Type);
        Assert.Equal("a@b.com", ep.Address);
    }

    [Fact]
    public void Should_CreateFromIEndpoint_Null()
    {
        var result = Endpoint.Create((IEndpoint?)null);
        Assert.Null(result);
    }

    [Fact]
    public void Should_CreateFromIEndpoint_Direct()
    {
        var ep = new Endpoint(EndpointType.Label, "test");
        var result = Endpoint.Create((IEndpoint)ep);
        Assert.Same(ep, result);
    }

    [Fact]
    public void Should_Throw_OnInvalidStringType()
    {
        Assert.Throws<ArgumentException>(() => new Endpoint("invalid-type", "addr"));
    }

    [Fact]
    public void Should_Create_Id()
    {
        var ep = Endpoint.Id("abcd");
        Assert.Equal(EndpointType.Id, ep.Type);
    }

    [Fact]
    public void Should_Create_Url()
    {
        var ep = Endpoint.Url("https://example.com");
        Assert.Equal(EndpointType.Url, ep.Type);
    }

    [Fact]
    public void Should_Create_Application()
    {
        var ep = Endpoint.Application("app-1");
        Assert.Equal(EndpointType.ApplicationId, ep.Type);
    }

    [Fact]
    public void Should_Create_User()
    {
        var ep = Endpoint.User("user-1");
        Assert.Equal(EndpointType.UserId, ep.Type);
    }

    [Fact]
    public void Should_Create_Device()
    {
        var ep = Endpoint.Device("device-1");
        Assert.Equal(EndpointType.DeviceId, ep.Type);
    }

    [Fact]
    public void Should_Create_AlphaNumeric()
    {
        var ep = Endpoint.AlphaNumeric("SUPPORT");
        Assert.Equal(EndpointType.Label, ep.Type);
    }

    [Fact]
    public void Should_Create_WithCreateMethod()
    {
        var ep = Endpoint.Create(EndpointType.Topic, "/topics/news");
        Assert.Equal(EndpointType.Topic, ep.Type);
        Assert.Equal("/topics/news", ep.Address);
    }

    [Fact]
    public void Should_Create_WithStringCreateMethod()
    {
        var ep = Endpoint.Create("email", "test@example.com");
        Assert.Equal(EndpointType.EmailAddress, ep.Type);
    }

    [Fact]
    public void Should_Throw_OnCreateWithNullAddress()
    {
        Assert.Throws<ArgumentNullException>(() => Endpoint.Create(EndpointType.Id, null!));
    }

    [Fact]
    public void Should_Throw_OnCreateWithEmptyAddress()
    {
        Assert.Throws<ArgumentException>(() => Endpoint.Create(EndpointType.Id, ""));
    }

    [Fact]
    public void Should_Create_DefaultConstructor()
    {
        var ep = new Endpoint();
        Assert.Equal(EndpointType.PhoneNumber, ep.Type);
        Assert.Equal("", ep.Address);
    }
}
