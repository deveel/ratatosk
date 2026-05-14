using System.Text.Json;
using Xunit;

namespace Deveel.Messaging;

/// <summary>
/// Comprehensive tests for TwilioWhatsAppConnector JSON message source handling including
/// message receiving, status updates, template interactions, and error scenarios.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioWhatsAppConnectorJson")]
public class TwilioWhatsAppConnectorJsonTests
{
    [Fact]
    public async Task Should_ParseCorrectly_When_ReceiveMessagesAsyncWithTwilioWhatsAppJsonWebhookSingleMessage()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate Twilio JSON webhook for single WhatsApp message
        var webhookJson = new
        {
            MessageSid = "SM1234567890abcdef",
            From = "whatsapp:+1234567890",
            To = "whatsapp:+1987654321",
            Body = "Hello from WhatsApp JSON webhook!",
            MessageStatus = "received",
            ProfileName = "John Doe",
            AccountSid = "AC1234567890123456789012345678901234",
            DateCreated = "2023-12-01T10:30:00Z",
            DateUpdated = "2023-12-01T10:30:05Z"
        };

        var jsonPayload = JsonSerializer.Serialize(webhookJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);

        var message = result.Value.Messages.First();
        Assert.Equal("SM1234567890abcdef", message.Id);
        Assert.Equal("whatsapp:+1234567890", message.Sender?.Address);
        Assert.Equal(EndpointType.PhoneNumber, message.Sender?.Type);
        Assert.Equal("whatsapp:+1987654321", message.Receiver?.Address);
        Assert.Equal(EndpointType.PhoneNumber, message.Receiver?.Type);
        Assert.Equal("Hello from WhatsApp JSON webhook!", ((ITextContent)message.Content!).Text);
        Assert.Equal(MessageContentType.PlainText, message.Content!.ContentType);
    }

    [Fact]
    public async Task Should_ParseAll_When_ReceiveMessagesAsyncWithTwilioWhatsAppJsonWebhookBatchMessages()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate Twilio JSON webhook for batch of WhatsApp messages
        var webhookJson = new
        {
            Messages = new[]
            {
                new { MessageSid = "SM1111111111", From = "whatsapp:+1111111111", To = "whatsapp:+1987654321", Body = "First WhatsApp message" },
                new { MessageSid = "SM2222222222", From = "whatsapp:+2222222222", To = "whatsapp:+1987654321", Body = "Second WhatsApp message" },
                new { MessageSid = "SM3333333333", From = "whatsapp:+3333333333", To = "whatsapp:+1987654321", Body = "Third WhatsApp message" }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(webhookJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Messages.Count);

        var messages = result.Value.Messages.ToList();
        Assert.Equal("SM1111111111", messages[0].Id);
        Assert.Equal("SM2222222222", messages[1].Id);
        Assert.Equal("SM3333333333", messages[2].Id);

        Assert.Equal("whatsapp:+1111111111", messages[0].Sender?.Address);
        Assert.Equal("whatsapp:+2222222222", messages[1].Sender?.Address);
        Assert.Equal("whatsapp:+3333333333", messages[2].Sender?.Address);

        Assert.Equal("First WhatsApp message", ((ITextContent)messages[0].Content!).Text);
        Assert.Equal("Second WhatsApp message", ((ITextContent)messages[1].Content!).Text);
        Assert.Equal("Third WhatsApp message", ((ITextContent)messages[2].Content!).Text);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_ReceiveMessagesAsyncWithWhatsAppButtonResponse()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate WhatsApp button response (empty body with button data)
        var webhookJson = new
        {
            MessageSid = "SM4444444444",
            From = "whatsapp:+1234567890",
            To = "whatsapp:+1987654321",
            Body = "",
            MessageStatus = "received",
            ButtonText = "Confirm Booking",
            ButtonPayload = "booking_confirmed",
            ProfileName = "Customer Name"
        };

        var jsonPayload = JsonSerializer.Serialize(webhookJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);

        var message = result.Value.Messages.First();
        Assert.Equal("SM4444444444", message.Id);
        Assert.Equal("", ((ITextContent)message.Content!).Text);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_ReceiveMessageStatusAsyncWithWhatsAppJsonStatusCallback()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate Twilio JSON status callback for WhatsApp
        var statusJson = new
        {
            MessageSid = "SM1234567890abcdef",
            MessageStatus = "delivered",
            To = "whatsapp:+1987654321",
            From = "whatsapp:+1234567890",
            AccountSid = "AC1234567890123456789012345678901234",
            MessagePrice = "0.0050",
            MessagePriceUnit = "USD",
            Timestamp = "2023-12-01T10:35:00Z"
        };

        var jsonPayload = JsonSerializer.Serialize(statusJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal("SM1234567890abcdef", result.Value.MessageId);
        Assert.Equal(MessageStatus.Delivered, result.Value.Status);

        // Check WhatsApp-specific additional data
        Assert.True(result.Value.AdditionalData.ContainsKey("Channel"));
        Assert.Equal("WhatsApp", result.Value.AdditionalData["Channel"]);
        Assert.True(result.Value.AdditionalData.ContainsKey("MessagePrice"));
        Assert.Equal("0.0050", result.Value.AdditionalData["MessagePrice"]);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_ReceiveMessageStatusAsyncWithWhatsAppReadStatus()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate WhatsApp-specific "read" status
        var statusJson = new
        {
            MessageSid = "SM1234567890abcdef",
            MessageStatus = "read",
            To = "whatsapp:+1987654321",
            From = "whatsapp:+1234567890",
            ProfileName = "Reader Name"
        };

        var jsonPayload = JsonSerializer.Serialize(statusJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal("SM1234567890abcdef", result.Value.MessageId);
        Assert.Equal(MessageStatus.Delivered, result.Value.Status); // "read" maps to Delivered

        Assert.True(result.Value.AdditionalData.ContainsKey("Channel"));
        Assert.Equal("WhatsApp", result.Value.AdditionalData["Channel"]);
        Assert.True(result.Value.AdditionalData.ContainsKey("ProfileName"));
        Assert.Equal("Reader Name", result.Value.AdditionalData["ProfileName"]);
    }

    [Theory]
    [InlineData("queued", MessageStatus.Queued)]
    [InlineData("accepted", MessageStatus.Queued)]
    [InlineData("sending", MessageStatus.Sent)]
    [InlineData("sent", MessageStatus.Sent)]
    [InlineData("delivered", MessageStatus.Delivered)]
    [InlineData("read", MessageStatus.Delivered)]
    [InlineData("undelivered", MessageStatus.DeliveryFailed)]
    [InlineData("failed", MessageStatus.DeliveryFailed)]
    [InlineData("received", MessageStatus.Received)]
    [InlineData("unknown_status", MessageStatus.Unknown)]
    public async Task Should_MapsCorrectly_When_ReceiveMessageStatusAsyncWithWhatsAppJsonStatusCallbackAllStatuses(string twilioStatus, MessageStatus expectedStatus)
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var statusJson = new
        {
            MessageSid = "SM1234567890abcdef",
            MessageStatus = twilioStatus,
            To = "whatsapp:+1987654321",
            From = "whatsapp:+1234567890"
        };

        var jsonPayload = JsonSerializer.Serialize(statusJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal(expectedStatus, result.Value.Status);
        Assert.Equal("WhatsApp", result.Value.AdditionalData["Channel"]);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_ReceiveMessagesAsyncWithWhatsAppMediaMessage()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate WhatsApp media message
        var webhookJson = new
        {
            MessageSid = "MM1234567890",
            From = "whatsapp:+1234567890",
            To = "whatsapp:+1987654321",
            Body = "Check out this image!",
            MessageStatus = "received",
            NumMedia = "1",
            MediaUrl0 = "https://api.twilio.com/2010-04-01/Accounts/AC123/Messages/MM123/Media/ME123",
            MediaContentType0 = "image/jpeg",
            ProfileName = "Sender Name"
        };

        var jsonPayload = JsonSerializer.Serialize(webhookJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);

        var message = result.Value.Messages.First();
        Assert.Equal("MM1234567890", message.Id);
        Assert.Equal("Check out this image!", ((ITextContent)message.Content!).Text);
    }

    [Fact]
    public async Task Should_ReturnError_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookMissingMessageSid()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate WhatsApp JSON webhook missing required MessageSid
        var invalidJson = new
        {
            From = "whatsapp:+1234567890",
            To = "whatsapp:+1987654321",
            Body = "Message without SID",
            MessageStatus = "received"
        };

        var jsonPayload = JsonSerializer.Serialize(invalidJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.NotNull(result.Error);
        Assert.Equal(MessagingErrorCodes.InvalidWebhookData, result.Error?.Code);
    }

    [Fact]
    public async Task Should_ReturnError_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookMissingFromOrTo()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate WhatsApp JSON webhook missing From field
        var invalidJson = new
        {
            MessageSid = "SM1234567890",
            To = "whatsapp:+1987654321",
            Body = "Message without From",
            MessageStatus = "received"
        };

        var jsonPayload = JsonSerializer.Serialize(invalidJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.NotNull(result.Error);
        Assert.Equal(MessagingErrorCodes.InvalidWebhookData, result.Error?.Code);
    }

    [Fact]
    public async Task Should_ReturnError_When_ReceiveMessagesAsyncWithInvalidJson()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Invalid JSON content
        var invalidJson = "{ \"MessageSid\": \"SM123\", \"From\": \"whatsapp:+123456789";
        var source = MessageSource.Json(invalidJson);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.NotNull(result.Error);
        Assert.Equal(ConnectorErrorCodes.ReceiveMessagesError, result.Error?.Code);
    }

    [Fact]
    public async Task Should_ReturnError_When_ReceiveMessageStatusAsyncWithInvalidJson()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Invalid JSON content
        var invalidJson = "{ \"MessageSid\": \"SM123\", \"MessageStatus\":";
        var source = MessageSource.Json(invalidJson);

        // Act
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.NotNull(result.Error);
        Assert.Equal(ConnectorErrorCodes.ReceiveStatusError, result.Error?.Code);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_ReceiveMessagesAsyncWithWhatsAppTemplateInteraction()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate WhatsApp template interaction response
        var webhookJson = new
        {
            MessageSid = "SM1234567890",
            From = "whatsapp:+1234567890",
            To = "whatsapp:+1987654321",
            Body = "",
            MessageStatus = "received",
            ButtonText = "Book Now",
            ButtonPayload = "book_appointment_123",
            ListId = "main_menu",
            ListTitle = "Services",
            ProfileName = "Customer"
        };

        var jsonPayload = JsonSerializer.Serialize(webhookJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);

        var message = result.Value.Messages.First();
        Assert.Equal("SM1234567890", message.Id);
        Assert.Equal("", ((ITextContent)message.Content!).Text); // Empty body for template interactions
    }

    [Fact]
    public async Task Should_PreservesEncoding_When_ReceiveMessagesAsyncWithWhatsAppUnicodeContent()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate WhatsApp message with Unicode content
        var unicodeMessage = "Hola! ?? Testing WhatsApp mojis and  special characters ????? ??";
        var webhookJson = new
        {
            MessageSid = "SM1234567890abcdef",
            From = "whatsapp:+1234567890",
            To = "whatsapp:+1987654321",
            Body = unicodeMessage,
            MessageStatus = "received",
            ProfileName = "Jos Mara"
        };

        var jsonPayload = JsonSerializer.Serialize(webhookJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);

        var message = result.Value.Messages.First();
        Assert.Equal(unicodeMessage, ((ITextContent)message.Content!).Text);
    }

    [Fact]
    public async Task Should_PreservesData_When_ReceiveMessageStatusAsyncWithWhatsAppJsonStatusCallbackAllAdditionalProperties()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Simulate comprehensive WhatsApp JSON status callback with all possible fields
        var statusJson = new
        {
            MessageSid = "SM1234567890abcdef",
            MessageStatus = "delivered",
            To = "whatsapp:+1987654321",
            From = "whatsapp:+1234567890",
            AccountSid = "AC1234567890123456789012345678901234",
            MessagePrice = "0.0050",
            MessagePriceUnit = "USD",
            ProfileName = "Customer Name",
            ButtonText = "Yes",
            ButtonPayload = "confirm_yes",
            NumSegments = "1",
            Direction = "inbound",
            DateCreated = "2023-12-01T10:30:00Z",
            DateUpdated = "2023-12-01T10:35:00Z",
            DateSent = "2023-12-01T10:30:01Z",
            Uri = "/2010-04-01/Accounts/AC123/Messages/SM123.json",
            CustomField = "whatsapp_custom_value",
            Extra = "additional_whatsapp_data"
        };

        var jsonPayload = JsonSerializer.Serialize(statusJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal("SM1234567890abcdef", result.Value.MessageId);
        Assert.Equal(MessageStatus.Delivered, result.Value.Status);

        // Verify that all additional properties (except MessageSid and MessageStatus) are preserved
        Assert.True(result.Value.AdditionalData.ContainsKey("Channel"));
        Assert.Equal("WhatsApp", result.Value.AdditionalData["Channel"]);
        Assert.True(result.Value.AdditionalData.ContainsKey("To"));
        Assert.True(result.Value.AdditionalData.ContainsKey("From"));
        Assert.True(result.Value.AdditionalData.ContainsKey("ProfileName"));
        Assert.True(result.Value.AdditionalData.ContainsKey("ButtonText"));
        Assert.True(result.Value.AdditionalData.ContainsKey("ButtonPayload"));
        Assert.True(result.Value.AdditionalData.ContainsKey("CustomField"));
        Assert.True(result.Value.AdditionalData.ContainsKey("Extra"));

        Assert.Equal("whatsapp_custom_value", result.Value.AdditionalData["CustomField"]);
        Assert.Equal("additional_whatsapp_data", result.Value.AdditionalData["Extra"]);
    }

    private static ConnectionSettings CreateValidConnectionSettings()
    {
        return new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
    }
}
