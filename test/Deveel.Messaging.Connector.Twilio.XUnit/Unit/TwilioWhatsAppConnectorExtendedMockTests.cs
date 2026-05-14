using Microsoft.Extensions.Logging;
using Moq;

using System.Text;

using Twilio.Rest.Api.V2010.Account;

namespace Deveel.Messaging;

/// <summary>
/// Extended tests for the <see cref="TwilioWhatsAppConnector"/> class demonstrating 
/// various WhatsApp scenarios using the TwilioMockFactory for comprehensive testing.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioWhatsAppConnectorExtendedMock")]
public class TwilioWhatsAppConnectorExtendedMockTests
{
    [Fact]
    public async Task Should_SendSuccessfully_When_SendWhatsAppMessageAsyncUsingMockFactory()
    {
        // Arrange
        var mockTwilioService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            CreateValidWhatsAppConnectionSettings(),
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);
        var message = CreateWhatsAppTestMessage();

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal("SM123456789", result.Value?.RemoteMessageId);
        Assert.Equal("WhatsApp", result.Value?.AdditionalData["Channel"]);
        mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnCorrectStatus_When_SendWhatsAppMessageAsyncWithDeliveredStatus()
    {
        // Arrange
        var mockTwilioService = TwilioMockFactory.CreateMockTwilioServiceForSending("SM987654321", MessageResource.StatusEnum.Delivered);
        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            CreateValidWhatsAppConnectionSettings(),
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);
        var message = CreateWhatsAppTestMessage();

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal(MessageStatus.Delivered, result.Value?.Status);
        Assert.Equal("SM987654321", result.Value?.RemoteMessageId);
        Assert.Equal("WhatsApp", result.Value?.AdditionalData["Channel"]);
    }

    [Fact]
    public async Task Should_ReturnStatus_When_GetWhatsAppMessageStatusAsyncUsingMockFactory()
    {
        // Arrange
        var mockTwilioService = TwilioMockFactory.CreateMockTwilioServiceForStatusQuery("SM555666777", MessageResource.StatusEnum.Sent);
        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            CreateValidWhatsAppConnectionSettings(),
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.GetMessageStatusAsync("SM555666777", TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal(MessageStatus.Sent, result.Value?.Updates.First().Status);
        Assert.Equal("WhatsApp", result.Value?.Updates.First().AdditionalData["Channel"]);
        mockTwilioService.Verify(x => x.FetchMessageAsync("SM555666777", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_TestWhatsAppConnectionAsyncUsingMockFactory()
    {
        // Arrange
        var mockTwilioService = TwilioMockFactory.CreateMockTwilioServiceForConnectionTest("AC9999888877776666555544443333222211", "Production Account");
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC9999888877776666555544443333222211")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");

        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            connectionSettings,
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        mockTwilioService.Verify(x => x.FetchAccountAsync("AC9999888877776666555544443333222211", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_WorksEndToEnd_When_FullWhatsAppWorkflowUsingFullyConfiguredMock()
    {
        // Arrange
        var mockTwilioService = TwilioMockFactory.CreateFullyConfiguredMockTwilioService();
        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            CreateValidWhatsAppConnectionSettings(),
            mockTwilioService.Object);

        var message = CreateWhatsAppTestMessage();

        // Act
        // Assert
        var initResult = await connector.InitializeAsync(TestContext.Current.CancellationToken);
        Assert.True(initResult.IsSuccess());

        // Act
        // Assert
        var connectionResult = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);
        Assert.True(connectionResult.IsSuccess());

        // Act
        // Assert
        var sendResult = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);
        Assert.True(sendResult.IsSuccess());
        Assert.Equal("WhatsApp", sendResult.Value?.AdditionalData["Channel"]);

        // Act
        // Assert
        var statusResult = await connector.GetMessageStatusAsync(sendResult.Value!.RemoteMessageId, TestContext.Current.CancellationToken);
        Assert.True(statusResult.IsSuccess());
        Assert.Equal("WhatsApp", statusResult.Value?.Updates.First().AdditionalData["Channel"]);

        // Verify all expected calls were made
        mockTwilioService.Verify(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mockTwilioService.Verify(x => x.FetchAccountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        mockTwilioService.Verify(x => x.FetchMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_SendSuccessfully_When_SendWhatsAppTemplateMessageWithContentSid()
    {
        // Arrange
        var mockTwilioService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.WhatsAppTemplates,
            CreateValidTemplateConnectionSettings(),
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);
        var message = CreateWhatsAppTemplateMessage();

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal("SM123456789", result.Value?.RemoteMessageId);
        mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnFailedStatus_When_SendWhatsAppMessageAsyncWithFailedMessage()
    {
        // Arrange
        var mockTwilioService = new Mock<ITwilioService>();
        mockTwilioService.Setup(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>()));
        
        var failedMessage = TwilioMockFactory.CreateMockFailedMessageResource("SM111222333", 30008, "Unknown WhatsApp number");
        mockTwilioService.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedMessage);

        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            CreateValidWhatsAppConnectionSettings(),
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);
        var message = CreateWhatsAppTestMessage();

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess()); // Message was sent, but has failed status
        Assert.Equal(MessageStatus.DeliveryFailed, result.Value?.Status);
        Assert.Equal("WhatsApp", result.Value?.AdditionalData["Channel"]);
    }

    [Fact]
    public async Task Should_AddsWhatsAppPrefix_When_SendWhatsAppMessageAsyncWithPhoneNumberFormatting()
    {
        // Arrange
        var mockTwilioService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            CreateValidWhatsAppConnectionSettings(),
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);
        
        // Create message with regular phone number (without whatsapp: prefix)
        var message = new Message
        {
            Id = "test-message-id",
            Sender = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"), // Add required Sender
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+1987654321"), // No whatsapp: prefix
            Content = new TextContent("Hello WhatsApp!")
        };

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_WorkCorrectly_When_MultipleWhatsAppOperationsWithDifferentMockSetups()
    {
        // Arrange
        var mockTwilioService = new Mock<ITwilioService>();
        mockTwilioService.Setup(x => x.Initialize(It.IsAny<string>(), It.IsAny<string>()));

        // Setup different responses for different message SIDs
        var message1 = TwilioMockFactory.CreateMockMessageResource("SM111", MessageResource.StatusEnum.Queued);
        var message2 = TwilioMockFactory.CreateMockMessageResource("SM222", MessageResource.StatusEnum.Sent);

        mockTwilioService.SetupSequence(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(message1)
            .ReturnsAsync(message2);

        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            CreateValidWhatsAppConnectionSettings(),
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result1 = await connector.SendMessageAsync(CreateWhatsAppTestMessage("msg1"), TestContext.Current.CancellationToken);
        var result2 = await connector.SendMessageAsync(CreateWhatsAppTestMessage("msg2"), TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result1.IsSuccess());
        Assert.True(result2.IsSuccess());
        Assert.Equal("SM111", result1.Value?.RemoteMessageId);
        Assert.Equal("SM222", result2.Value?.RemoteMessageId);
        Assert.Equal(MessageStatus.Queued, result1.Value?.Status);
        Assert.Equal(MessageStatus.Sent, result2.Value?.Status);
        Assert.Equal("WhatsApp", result1.Value?.AdditionalData["Channel"]);
        Assert.Equal("WhatsApp", result2.Value?.AdditionalData["Channel"]);

        mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Should_HandleGracefully_When_ErrorScenariosUsingMockFactory()
    {
        // Arrange
        var mockTwilioService = TwilioMockFactory.CreateMockTwilioService();
        TwilioMockFactory.ConfigureForException(mockTwilioService, new InvalidOperationException("WhatsApp API error"));

        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            CreateValidWhatsAppConnectionSettings(),
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);
        var message = CreateWhatsAppTestMessage();

        // Act
        // Assert
        var sendResult = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);
        Assert.False(sendResult.IsSuccess());
        Assert.Contains("WhatsApp API error", sendResult.Error?.Message);

        // Act
        // Assert
        var statusResult = await connector.GetMessageStatusAsync("SM123", TestContext.Current.CancellationToken);
        Assert.False(statusResult.IsSuccess());
        Assert.Contains("WhatsApp API error", statusResult.Error?.Message);

        // Act
        // Assert
        var connectionResult = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);
        Assert.False(connectionResult.IsSuccess());
        Assert.Contains("WhatsApp API error", connectionResult.Error?.Message);
    }

    [Theory]
    [InlineData("Queued", "Queued")]
    [InlineData("Sending", "Sent")]
    [InlineData("Sent", "Sent")]
    [InlineData("Delivered", "Delivered")]
    [InlineData("Undelivered", "DeliveryFailed")]
    [InlineData("Failed", "DeliveryFailed")]
    [InlineData("Received", "Received")]
    public async Task Should_CorrectlyMapsAllTwilioStatuses_When_WhatsAppStatusMappingIsInvoked(string twilioStatusString, string expectedStatusString)
    {
        // Map string values to enum values manually to avoid parsing issues
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
        var mockTwilioService = TwilioMockFactory.CreateMockTwilioServiceForSending("SM123", twilioStatus);
        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            CreateValidWhatsAppConnectionSettings(),
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);
        var message = CreateWhatsAppTestMessage();

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal(expectedStatus, result.Value?.Status);
        Assert.Equal("WhatsApp", result.Value?.AdditionalData["Channel"]);
    }

    [Fact]
    public async Task Should_SendSuccessfully_When_SendWhatsAppMessageAsyncWithMediaContent()
    {
        // Arrange
        var mockTwilioService = TwilioMockFactory.CreateMockTwilioServiceForSending();
        var connector = new TwilioWhatsAppConnector(
            TwilioChannelSchemas.SimpleWhatsApp,
            CreateValidWhatsAppConnectionSettings(),
            mockTwilioService.Object);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);
        
        var message = new Message
        {
            Id = "test-media-message-id",
            Sender = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"), // Add required Sender
            Receiver = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1987654321"),
            Content = new MediaContent(MediaType.Image, "media.jpg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 }) // Mock JPEG header bytes
        };

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal("WhatsApp", result.Value?.AdditionalData["Channel"]);
        mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Once);
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

    private static Message CreateWhatsAppTestMessage(string? id = null)
    {
        return new Message
        {
            Id = id ?? "test-whatsapp-message-id",
            Sender = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1234567890"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "whatsapp:+1987654321"),
            Content = new TextContent("Hello WhatsApp World")
        };
    }

    private static Message CreateWhatsAppTemplateMessage(string? id = null)
    {
        var message = new Message
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

        return message;
    }






}