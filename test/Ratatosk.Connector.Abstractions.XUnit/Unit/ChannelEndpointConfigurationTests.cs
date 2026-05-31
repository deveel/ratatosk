namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "ChannelEndpointConfiguration")]
public class ChannelEndpointConfigurationTests
{
    [Fact]
    public void Should_ToString_ReturnFormattedString()
    {
        var config = new ChannelEndpointConfiguration(EndpointType.PhoneNumber);
        var str = config.ToString();
        Assert.Contains("PhoneNumber", str);
    }

    [Fact]
    public void Should_SetCanSendAndReceive()
    {
        var config = new ChannelEndpointConfiguration(EndpointType.EmailAddress);
        config.CanSend = true;
        config.CanReceive = true;
        Assert.True(config.CanSend);
        Assert.True(config.CanReceive);
    }
}
