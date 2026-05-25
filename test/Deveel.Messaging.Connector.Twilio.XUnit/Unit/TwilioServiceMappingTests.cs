namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioService")]
public class TwilioServiceMappingTests
{
    [Theory]
    [InlineData(21211, MessagingErrorCodes.InvalidRecipient)]
    [InlineData(21610, MessagingErrorCodes.InvalidRecipient)]
    [InlineData(21614, TwilioErrorCodes.InvalidSender)]
    [InlineData(21408, TwilioErrorCodes.InvalidSender)]
    [InlineData(20001, TwilioErrorCodes.InvalidMessage)]
    [InlineData(99999, MessagingErrorCodes.SendMessageFailed)]
    [InlineData(null, MessagingErrorCodes.SendMessageFailed)]
    public void Should_MapCorrectly_When_TwilioErrorCodeProvided(int? twilioCode, string expected)
    {
        var result = TwilioService.MapTwilioErrorCode(twilioCode);
        Assert.Equal(expected, result);
    }
}
