namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "Endpoint")]
public class ParseEndpointTypeTests
{
    [Theory]
    [InlineData("email", EndpointType.EmailAddress)]
    [InlineData("Email", EndpointType.EmailAddress)]
    [InlineData("EMAIL", EndpointType.EmailAddress)]
    [InlineData("phone", EndpointType.PhoneNumber)]
    [InlineData("url", EndpointType.Url)]
    [InlineData("topic", EndpointType.Topic)]
    [InlineData("user-id", EndpointType.UserId)]
    [InlineData("userid", EndpointType.UserId)]
    [InlineData("UserId", EndpointType.UserId)]
    [InlineData("app-id", EndpointType.ApplicationId)]
    [InlineData("appid", EndpointType.ApplicationId)]
    [InlineData("applicationid", EndpointType.ApplicationId)]
    [InlineData("endpoint-id", EndpointType.Id)]
    [InlineData("id", EndpointType.Id)]
    [InlineData("endpointid", EndpointType.Id)]
    [InlineData("device-id", EndpointType.DeviceId)]
    [InlineData("device", EndpointType.DeviceId)]
    [InlineData("deviceid", EndpointType.DeviceId)]
    [InlineData("label", EndpointType.Label)]
    [InlineData("alphanumeric", EndpointType.Label)]
    public void Should_ParseValidType_When_ValidString(string input, EndpointType expected)
    {
        var result = Endpoint.ParseEndpointType(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Should_ThrowArgumentException_When_UnknownType()
    {
        Assert.Throws<ArgumentException>(() => Endpoint.ParseEndpointType("unknown"));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_NullType()
    {
        Assert.Throws<ArgumentNullException>(() => Endpoint.ParseEndpointType(null!));
    }
}
