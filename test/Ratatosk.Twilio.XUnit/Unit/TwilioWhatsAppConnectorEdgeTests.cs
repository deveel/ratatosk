using Moq;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioWhatsAppConnector")]
public class TwilioWhatsAppConnectorEdgeTests
{
    private static IChannelSchema CreateSchema()
        => new ChannelSchemaBuilder("Twilio", "whatsapp", "1.0")
            .WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.HealthCheck | ChannelCapability.ReceiveMessages |
                              ChannelCapability.HandleMessageState | ChannelCapability.MessageStatusQuery)
            .HandlesMessageEndpoint(EndpointType.PhoneNumber, e => { e.CanSend = true; e.CanReceive = true; })
            .AddAuthenticationScheme(AuthenticationScheme.Basic)
            .Build();

    [Fact]
    public void Should_ThrowArgumentNull_When_NullConnectionSettings()
    {
        var schema = CreateSchema();
        Assert.Throws<ArgumentNullException>(() => new TwilioWhatsAppConnector(schema, null!));
    }

    [Fact]
    public async Task Should_ThrowConnectorException_When_SendingWithoutSender()
    {
        var mockService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioWhatsAppConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+1234567890"),
            Content = new TextContent("Hello")
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.False(result.IsSuccess());
    }

    [Fact]
    public async Task Should_ThrowConnectorException_When_SendingWithoutReceiver()
    {
        var mockService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioWhatsAppConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Sender = new PhoneSender("whatsapp:+1987654321"),
            Content = new TextContent("Hello")
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.False(result.IsSuccess());
    }

    [Fact]
    public async Task Should_SendWhatsAppMessage_When_ValidMessage()
    {
        var mockService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioWhatsAppConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Sender = new PhoneSender("whatsapp:+1987654321"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"),
            Content = new TextContent("Hello WhatsApp")
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.True(result.IsSuccess());
        mockService.Verify(x => x.CreateMessageAsync(
            It.Is<CreateMessageOptions>(o => o.From.ToString().Contains("whatsapp:")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_SendTemplateMessage_When_TemplateContentProvided()
    {
        var mockService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioWhatsAppConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Sender = new PhoneSender("whatsapp:+1987654321"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"),
            Content = new TemplateContent("HXtemplate123", new Dictionary<string, object?> { { "1", "John" } })
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.True(result.IsSuccess());
        mockService.Verify(x => x.CreateMessageAsync(
            It.Is<CreateMessageOptions>(o => o.ContentSid == "HXtemplate123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_SendWithMediaUrl_When_MediaContentProvided()
    {
        var mockService = new Mock<ITwilioService>();
        mockService.Setup(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>()));
        mockService.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TwilioMockFactory.CreateMockMessageResource());

        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioWhatsAppConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Sender = new PhoneSender("whatsapp:+1987654321"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"),
            Content = new MediaContent()
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.True(result.IsSuccess());
    }

    [Fact]
    public async Task Should_ThrowConnectorException_When_TemplateWithoutContentSid()
    {
        var mockService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioWhatsAppConnector(schema, settings, mockService.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var msg = new Message
        {
            Id = "test-1",
            Sender = new PhoneSender("whatsapp:+1987654321"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"),
            Content = new TemplateContent("")
        };

        var result = await connector.SendMessageAsync(msg, CancellationToken.None);
        Assert.False(result.IsSuccess());
    }

    [Fact]
    public async Task Should_GetHealth_When_Healthy()
    {
        var mockTwilio = TwilioMockFactory.CreateFullyConfiguredMockTwilioService();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioWhatsAppConnector(schema, settings, mockTwilio.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var health = await connector.GetHealthAsync(CancellationToken.None);
        Assert.True(health.IsSuccess());
        Assert.True(health.Value.IsHealthy);
    }

    [Fact]
    public async Task Should_ReturnStatusWithWhatsAppChannel()
    {
        var mockTwilio = TwilioMockFactory.CreateFullyConfiguredMockTwilioService();
        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioWhatsAppConnector(schema, settings, mockTwilio.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var status = await connector.GetStatusAsync(CancellationToken.None);
        Assert.True(status.IsSuccess());
    }

    [Fact]
    public async Task Should_ReturnErrorResult_When_UnsupportedContentTypeInReceive()
    {
        var mockTwilio = new Mock<ITwilioService>();
        mockTwilio.Setup(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>()));
        mockTwilio.Setup(x => x.FetchAccountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TwilioMockFactory.CreateMockAccountResource());

        var settings = new ConnectionSettings();
        settings.SetParameter("AccountSid", "AC123");
        settings.SetParameter("AuthToken", "token123");
        var schema = CreateSchema();
        var connector = new TwilioWhatsAppConnector(schema, settings, mockTwilio.Object);
        await connector.InitializeAsync(CancellationToken.None);

        var source = MessageSource.Text("unsupported");
        var result = await connector.ReceiveMessagesAsync(source, CancellationToken.None);
        Assert.False(result.IsSuccess());
    }

    [Fact]
    public async Task Should_Fail_When_InitializeWithoutCredentials()
    {
        var schema = CreateSchema();
        var settings = new ConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, settings);
        var result = await connector.InitializeAsync(CancellationToken.None);
        Assert.False(result.IsSuccess());
    }
}
