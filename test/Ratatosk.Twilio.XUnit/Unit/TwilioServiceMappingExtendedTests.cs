using System.Reflection;
using Moq;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioService")]
public class TwilioServiceMappingExtendedTests
{
    private static TwilioService CreateService(ITwilioApiClient client)
    {
        var ctor = typeof(TwilioService).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(c => c.GetParameters().Length == 1);
        return (TwilioService)ctor.Invoke(new object[] { client });
    }

    [Theory]
    [InlineData(21211, MessagingErrorCodes.InvalidRecipient)]
    [InlineData(21610, MessagingErrorCodes.InvalidRecipient)]
    [InlineData(21614, TwilioErrorCodes.InvalidSender)]
    [InlineData(21408, TwilioErrorCodes.InvalidSender)]
    [InlineData(20001, TwilioErrorCodes.InvalidMessage)]
    [InlineData(0, MessagingErrorCodes.SendMessageFailed)]
    [InlineData(null, MessagingErrorCodes.SendMessageFailed)]
    public void Should_MapTwilioErrorCode_When_VariousCodes(int? twilioCode, string expected)
    {
        var result = TwilioService.MapTwilioErrorCode(twilioCode);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Should_ThrowConnectorExceptionWithInvalidSenderCode_When_CreateMessageAsyncThrowsApiException21614()
    {
        var client = new Mock<ITwilioApiClient>();
        client.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>()))
            .ThrowsAsync(new ApiException(21614, 400, "Invalid from number", null, null, null));

        var service = CreateService(client.Object);
        var ex = await Assert.ThrowsAsync<ConnectorException>(() =>
            service.CreateMessageAsync(new CreateMessageOptions("+123"), CancellationToken.None));

        Assert.Equal(TwilioErrorCodes.InvalidSender, ex.ErrorCode);
    }

    [Fact]
    public async Task Should_ThrowConnectorExceptionWithInvalidMessageCode_When_CreateMessageAsyncThrowsApiException20001()
    {
        var client = new Mock<ITwilioApiClient>();
        client.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>()))
            .ThrowsAsync(new ApiException(20001, 400, "Invalid message", null, null, null));

        var service = CreateService(client.Object);
        var ex = await Assert.ThrowsAsync<ConnectorException>(() =>
            service.CreateMessageAsync(new CreateMessageOptions("+123"), CancellationToken.None));

        Assert.Equal(TwilioErrorCodes.InvalidMessage, ex.ErrorCode);
    }

    [Fact]
    public async Task Should_ThrowConnectorExceptionWithGenericCode_When_UnmappedError()
    {
        var client = new Mock<ITwilioApiClient>();
        client.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>()))
            .ThrowsAsync(new ApiException(99999, 400, "Unknown error", null, null, null));

        var service = CreateService(client.Object);
        var ex = await Assert.ThrowsAsync<ConnectorException>(() =>
            service.CreateMessageAsync(new CreateMessageOptions("+123"), CancellationToken.None));

        Assert.Equal(MessagingErrorCodes.SendMessageFailed, ex.ErrorCode);
    }
}
