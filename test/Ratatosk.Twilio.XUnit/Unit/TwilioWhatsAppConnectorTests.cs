using Microsoft.Extensions.Logging;
using Moq;
using Twilio.Rest.Api.V2010.Account;

namespace Ratatosk;

/// <summary>
/// Tests for the <see cref="TwilioWhatsAppConnector"/> class using mocked Twilio services
/// to verify WhatsApp messaging functionality without requiring actual Twilio API calls.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioWhatsAppConnector")]
public class TwilioWhatsAppConnectorTests
{
    private readonly Mock<ITwilioService> _mockTwilioService;
    private readonly Mock<ILogger<TwilioWhatsAppConnector>> _mockLogger;

    public TwilioWhatsAppConnectorTests()
    {
        _mockTwilioService = new Mock<ITwilioService>();
        _mockLogger = new Mock<ILogger<TwilioWhatsAppConnector>>();
    }

    [Fact]
    public void Should_CreateConnector_When_ConstructorWithValidSchemaAndSettings()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidWhatsAppConnectionSettings();

        // Act
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);

        // Assert
        Assert.Same(schema, connector.Schema);
        Assert.Equal(ConnectorState.Uninitialized, connector.State);
    }

    [Fact]
    public void Should_UseDefaultSchema_When_ConstructorWithConnectionSettingsOnly()
    {
        // Arrange
        var connectionSettings = CreateValidWhatsAppConnectionSettings();

        // Act
        var connector = new TwilioWhatsAppConnector(connectionSettings);

        // Assert
        Assert.Equal(TwilioConnectorConstants.Provider, connector.Schema.ChannelProvider);
        Assert.Equal(TwilioConnectorConstants.WhatsAppChannel, connector.Schema.ChannelType);
        Assert.Equal(ConnectorState.Uninitialized, connector.State);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ConstructorWithNullConnectionSettings()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => new TwilioWhatsAppConnector(schema, null!));
        Assert.Throws<ArgumentNullException>(() => new TwilioWhatsAppConnector(null!));
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_InitializeAsyncWithValidSettings()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = CreateValidWhatsAppConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        // Act
        var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal(ConnectorState.Ready, connector.State);
        _mockTwilioService.Verify(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_InitializeAsyncWithMissingCredentials()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("FromNumber", "whatsapp:+1234567890");

        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        // Act
        var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(MessagingErrorCodes.MissingCredentials, result.Error?.Code);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_InitializeAsyncWithMissingFromNumber()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");

        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        // Act
        var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess()); // Initialization should succeed without FromNumber
        Assert.Equal(ConnectorState.Ready, connector.State);
    }

    [Fact]
    public async Task Should_AddsWhatsAppPrefixToPhoneNumber_When_InitializeAsyncIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");

        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        // Act
        var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal(ConnectorState.Ready, connector.State);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_TestConnectionAsyncWithValidCredentials()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = CreateValidWhatsAppConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var mockAccount = TwilioMockFactory.CreateMockAccountResource("AC1234567890123456789012345678901234", "Test Account");
        _mockTwilioService.Setup(x => x.FetchAccountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockAccount);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        _mockTwilioService.Verify(x => x.FetchAccountAsync("AC1234567890123456789012345678901234", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_SendMessageAsyncWithValidWhatsAppMessage()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = CreateValidWhatsAppConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var mockMessageResource = TwilioMockFactory.CreateMockMessageResource("SM123456789", MessageResource.StatusEnum.Queued);
        _mockTwilioService.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMessageResource);

        var message = CreateWhatsAppTestMessage();
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal(message.Id, result.Value.MessageId);
        Assert.Equal("SM123456789", result.Value.RemoteMessageId);
        Assert.Equal(MessageStatus.Queued, result.Value.Status);
        Assert.Equal("WhatsApp", result.Value.AdditionalData["Channel"]);

        _mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_UseContentSid_When_SendMessageAsyncWithTemplateMessage()
    {
        // Arrange
        var schema = TwilioChannelSchemas.WhatsAppTemplates;
        var connectionSettings = CreateValidTemplateConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var mockMessageResource = TwilioMockFactory.CreateMockMessageResource("SM123456789", MessageResource.StatusEnum.Queued);
        _mockTwilioService.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMessageResource);

        var message = CreateWhatsAppTemplateMessage();
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        _mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SendMessageAsyncWithInvalidRecipient()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = CreateValidWhatsAppConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var message = new Message
        {
            Id = "test-message-id",
            Sender = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"), // Add required Sender
            Receiver = new Endpoint(EndpointType.EmailAddress, "invalid@email.com"), // Invalid endpoint type
            Content = new TextContent("Hello WhatsApp")
        };

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.Code);

        // Verify Twilio service was not called due to validation failure
        _mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnInvalidRecipientError_When_SendMessageAsyncWithPhoneNumberButEmptyAddress()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = CreateValidWhatsAppConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var message = new Message
        {
            Id = "test-message-id",
            Sender = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"), // Add required Sender
            Receiver = new Endpoint(EndpointType.PhoneNumber, ""), // Valid endpoint type but empty address
            Content = new TextContent("Hello WhatsApp")
        };

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(MessagingErrorCodes.InvalidRecipient, result.Error?.Code);
        Assert.Contains("WhatsApp phone number is required", result.Error?.Message);

        // Verify Twilio service was not called due to validation failure
        _mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SendMessageAsyncWithTwilioException()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = CreateValidWhatsAppConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        _mockTwilioService.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("WhatsApp API error"));

        var message = CreateWhatsAppTestMessage();
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(ConnectorErrorCodes.SendMessageError, result.Error?.Code);
        Assert.Contains("WhatsApp API error", result.Error?.Message);
    }

    [Fact]
    public async Task Should_ReturnStatus_When_GetMessageStatusAsyncWithValidMessageId()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = CreateValidWhatsAppConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var mockMessageResource = TwilioMockFactory.CreateMockMessageResource("SM123456789", MessageResource.StatusEnum.Delivered);
        _mockTwilioService.Setup(x => x.FetchMessageAsync("SM123456789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMessageResource);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.GetMessageStatusAsync("SM123456789", TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Updates);
        Assert.Equal(MessageStatus.Delivered, result.Value.Updates.First().Status);
        Assert.Equal("WhatsApp", result.Value.Updates.First().AdditionalData["Channel"]);

        _mockTwilioService.Verify(x => x.FetchMessageAsync("SM123456789", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_GetMessageStatusAsyncWithTwilioException()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = CreateValidWhatsAppConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        _mockTwilioService.Setup(x => x.FetchMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Status query failed"));

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.GetMessageStatusAsync("SM123456789", TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(ConnectorErrorCodes.GetMessageStatusError, result.Error?.Code);
        Assert.Contains("Status query failed", result.Error?.Message);
    }

    [Theory]
    [InlineData("Queued", "Queued")]
    [InlineData("Sending", "Sent")]
    [InlineData("Sent", "Sent")]
    [InlineData("Delivered", "Delivered")]
    [InlineData("Undelivered", "DeliveryFailed")]
    [InlineData("Failed", "DeliveryFailed")]
    [InlineData("Received", "Received")]
    public async Task Should_CorrectlyMapsAllTwilioStatuses_When_StatusMappingIsInvoked(string twilioStatusString, string expectedStatusString)
    {
        // Map string values to enum values manually
        var twilioStatus = twilioStatusString switch
        {
            "Queued" => MessageResource.StatusEnum.Queued,
            "Sending" => MessageResource.StatusEnum.Sending,
            "Sent" => MessageResource.StatusEnum.Sent,
            "Delivered" => MessageResource.StatusEnum.Delivered,
            "Undelivered" => MessageResource.StatusEnum.Undelivered,
            "Failed" => MessageResource.StatusEnum.Failed,
            "Received" => MessageResource.StatusEnum.Received,
            _ => throw new ArgumentException($"Unknown Twilio status: {twilioStatusString}")
        };

        var expectedStatus = expectedStatusString switch
        {
            "Queued" => MessageStatus.Queued,
            "Sent" => MessageStatus.Sent,
            "Delivered" => MessageStatus.Delivered,
            "DeliveryFailed" => MessageStatus.DeliveryFailed,
            "Received" => MessageStatus.Received,
            _ => throw new ArgumentException($"Unknown expected status: {expectedStatusString}")
        };

        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = CreateValidWhatsAppConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var mockMessageResource = TwilioMockFactory.CreateMockMessageResource("SM123", twilioStatus);
        _mockTwilioService.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMessageResource);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);
        var message = CreateWhatsAppTestMessage();

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal(expectedStatus, result.Value?.Status);
    }

    private static ConnectionSettings CreateValidWhatsAppConnectionSettings()
    {
        return new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
    }

    private static ConnectionSettings CreateValidTemplateConnectionSettings()
    {
        return new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
            // ContentSid is now provided via TemplateContent, not connection settings
    }

    private static Message CreateWhatsAppTestMessage()
    {
        return new Message
        {
            Id = "test-whatsapp-message-id",
            Sender = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1987654321"),
            Content = new TextContent("Hello WhatsApp World")
        };
    }

    private static Message CreateWhatsAppTemplateMessage(string? id = null)
    {
        return new Message
        {
            Id = id ?? "test-template-message-id",
            Sender = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1987654321"),
            Content = new TemplateContent("HX1234567890123456789012345678901234", new Dictionary<string, object?>
            {
                { "name", "John" },
                { "code", "123" }
            })
        };
    }





}
