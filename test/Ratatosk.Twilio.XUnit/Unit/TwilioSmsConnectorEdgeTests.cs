using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioSmsConnector")]
public class TwilioSmsConnectorEdgeTests
{
    private static IChannelSchema CreateSchema()
        => new ChannelSchemaBuilder("Twilio", "sms", "1.0")
            .WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.HealthCheck | ChannelCapability.ReceiveMessages |
                              ChannelCapability.HandleMessageState | ChannelCapability.MessageStatusQuery)
            .HandlesMessageEndpoint(EndpointType.PhoneNumber, e => { e.CanSend = true; e.CanReceive = true; })
            .AddAuthenticationScheme(AuthenticationScheme.Basic)
            .Build();

    [Fact]
    public void Should_ThrowArgumentNull_When_NullConnectionSettings()
    {
        var schema = CreateSchema();
        Assert.Throws<ArgumentNullException>(() => new TwilioSmsConnector(schema, null!));
    }

    [Fact]
    public async Task Should_ThrowConnectorException_When_SendWithMissingSenderAndNoMessagingServiceSid()
    {
        var mockService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioSmsConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+1234567890"),
            Content = new TextContent("Hello")
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.False(result.IsSuccess());
        Assert.Equal(TwilioErrorCodes.MissingFromNumber, result.Error?.Code);
    }

    [Fact]
    public async Task Should_ThrowConnectorException_When_SendWithMissingRecipient()
    {
        var mockService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioSmsConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Sender = new PhoneSender("+1987654321"),
            Content = new TextContent("Hello")
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.False(result.IsSuccess());
        Assert.Equal(MessagingErrorCodes.InvalidRecipient, result.Error?.Code);
    }

    [Fact]
    public async Task Should_SendMessage_When_UsingMessagingServiceSid()
    {
        var mockService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        settings.SetParameter("MessagingServiceSid", "MGabc123");
        var schema = CreateSchema();
        var connector = new TwilioSmsConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Sender = new PhoneSender("+1987654321"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+1234567890"),
            Content = new TextContent("Hello via service")
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.True(result.IsSuccess());
        mockService.Verify(x => x.CreateMessageAsync(
            It.Is<CreateMessageOptions>(o => o.MessagingServiceSid == "MGabc123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_SendWithMediaUrl_When_MediaContentProvided()
    {
        var mockTwilio = new Mock<ITwilioService>();
        mockTwilio.Setup(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>()));
        mockTwilio.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TwilioMockFactory.CreateMockMessageResource());

        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        settings.SetParameter("MessagingServiceSid", "MGabc123");
        var schema = CreateSchema();
        var connector = new TwilioSmsConnector(schema, settings, mockTwilio.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+1234567890"),
            Content = new MediaContent()
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.True(result.IsSuccess());
    }

    [Fact]
    public async Task Should_GetStatus_When_GetStatusAsyncCalled()
    {
        var mockTwilio = new Mock<ITwilioService>();
        mockTwilio.Setup(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>()));
        mockTwilio.Setup(x => x.FetchAccountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TwilioMockFactory.CreateMockAccountResource());
        mockTwilio.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TwilioMockFactory.CreateMockMessageResource());

        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        settings.SetParameter("MessagingServiceSid", "MGabc123");
        var schema = CreateSchema();
        var connector = new TwilioSmsConnector(schema, settings, mockTwilio.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var status = await connector.GetStatusAsync(CancellationToken.None);
        Assert.True(status.IsSuccess());
        Assert.Contains("AccountSid", status.Value.AdditionalData);
    }

    [Fact]
    public async Task Should_GetHealth_When_Healthy()
    {
        var mockTwilio = TwilioMockFactory.CreateFullyConfiguredMockTwilioService();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        settings.SetParameter("MessagingServiceSid", "MGabc123");
        var schema = CreateSchema();
        var connector = new TwilioSmsConnector(schema, settings, mockTwilio.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var health = await connector.GetHealthAsync(CancellationToken.None);
        Assert.True(health.IsSuccess());
        Assert.True(health.Value.IsHealthy);
    }

    [Fact]
    public async Task Should_ReturnErrorResult_When_UnsupportedContentTypeInStatusReceive()
    {
        var mockTwilio = new Mock<ITwilioService>();
        mockTwilio.Setup(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>()));
        mockTwilio.Setup(x => x.FetchAccountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TwilioMockFactory.CreateMockAccountResource());

        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioSmsConnector(schema, settings, mockTwilio.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var source = MessageSource.Text("unsupported");
        var result = await connector.ReceiveMessageStatusAsync(source, CancellationToken.None);
        Assert.False(result.IsSuccess());
    }

    [Fact]
    public async Task Should_QueryMessageStatus_When_GetMessageStatusAsyncCalled()
    {
        var mockTwilio = TwilioMockFactory.CreateFullyConfiguredMockTwilioService();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        settings.SetParameter("MessagingServiceSid", "MGabc123");
        var schema = CreateSchema();
        var connector = new TwilioSmsConnector(schema, settings, mockTwilio.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var statusResult = await connector.GetMessageStatusAsync("SM123456789", CancellationToken.None);
        Assert.True(statusResult.IsSuccess());
        Assert.NotEmpty(statusResult.Value.Updates);
    }

    [Fact]
    public async Task Should_ApplyValidityPeriod_When_SendingWithSettings()
    {
        var mockTwilio = new Mock<ITwilioService>();
        mockTwilio.Setup(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>()));
        mockTwilio.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TwilioMockFactory.CreateMockMessageResource());

        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        settings.SetParameter("MessagingServiceSid", "MGabc123");
        settings.SetParameter("ValidityPeriod", 3600);
        var schema = CreateSchema();
        var connector = new TwilioSmsConnector(schema, settings, mockTwilio.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Sender = new PhoneSender("+1987654321"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+1234567890"),
            Content = new TextContent("Hello with validity")
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.True(result.IsSuccess());
    }

    [Fact]
    public async Task Should_Fail_When_InitializeWithoutCredentials()
    {
        var schema = CreateSchema();
        var settings = new ConnectionSettings();
        var connector = new TwilioSmsConnector(schema, settings);
        var result = await connector.InitializeAsync(CancellationToken.None);
        Assert.False(result.IsSuccess());
    }
}
