using Xunit;

namespace Ratatosk;

/// <summary>
/// Tests for TwilioWhatsAppConnector webhook and status callback capabilities to verify
/// that the connector properly supports receiving messages and status updates via JSON and form data.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioWhatsAppConnectorWebhook")]
public class TwilioWhatsAppConnectorWebhookTests
{
    [Fact]
    public async Task Should_SupportReceiveMessagesAsync_When_TwilioWhatsAppConnectorIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create a valid Twilio WhatsApp webhook source
        var webhookData = "MessageSid=SM1234567890&From=whatsapp%3A%2B1234567890&To=whatsapp%3A%2B1987654321&Body=Test%20WhatsApp&MessageStatus=received&ProfileName=John%20Doe";
        var source = MessageSource.UrlPost(webhookData);

        // Act
        // Assert
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);
        
        // The result should be successful since we have valid webhook data
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);
        
        var message = result.Value.Messages.First();
        Assert.Equal("SM1234567890", message.Id);
        Assert.Equal("whatsapp:+1234567890", message.Sender?.Address);
        Assert.Equal("whatsapp:+1987654321", message.Receiver?.Address);
        Assert.Equal("Test WhatsApp", ((ITextContent)message.Content!).Text);
    }

    [Fact]
    public async Task Should_SupportReceiveMessageStatusAsync_When_TwilioWhatsAppConnectorIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create a valid Twilio WhatsApp status callback source
        var statusData = "MessageSid=SM1234567890&MessageStatus=delivered&To=whatsapp%3A%2B1987654321&From=whatsapp%3A%2B1234567890&ProfileName=Customer";
        var source = MessageSource.UrlPost(statusData);

        // Act
        // Assert
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);
        
        // The result should be successful since we have valid status callback data
        Assert.True(result.IsSuccess());
        Assert.Equal("SM1234567890", result.Value?.MessageId);
        Assert.Equal(MessageStatus.Delivered, result.Value?.Status);
        
        // Check WhatsApp-specific fields
        Assert.True(result.Value?.AdditionalData.ContainsKey("Channel"));
        Assert.Equal("WhatsApp", result.Value?.AdditionalData["Channel"]);
        Assert.True(result.Value?.AdditionalData.ContainsKey("ProfileName"));
        Assert.Equal("Customer", result.Value?.AdditionalData["ProfileName"]);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_TwilioWhatsAppConnectorReceiveMessagesAsyncWithJsonSource()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create a WhatsApp JSON webhook payload
        var webhookJson = """
        {
            "MessageSid": "SM1234567890abcdef",
            "From": "whatsapp:+1234567890",
            "To": "whatsapp:+1987654321",
            "Body": "Hello from WhatsApp JSON!",
            "MessageStatus": "received",
            "ProfileName": "JSON User"
        }
        """;

        var source = MessageSource.Json(webhookJson);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);
        
        var message = result.Value.Messages.First();
        Assert.Equal("SM1234567890abcdef", message.Id);
        Assert.Equal("whatsapp:+1234567890", message.Sender?.Address);
        Assert.Equal("whatsapp:+1987654321", message.Receiver?.Address);
        Assert.Equal("Hello from WhatsApp JSON!", ((ITextContent)message.Content!).Text);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_TwilioWhatsAppConnectorReceiveMessageStatusAsyncWithJsonSource()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create a WhatsApp JSON status callback payload
        var statusJson = """
        {
            "MessageSid": "SM1234567890abcdef",
            "MessageStatus": "read",
            "To": "whatsapp:+1987654321",
            "From": "whatsapp:+1234567890",
            "ProfileName": "Read User"
        }
        """;

        var source = MessageSource.Json(statusJson);

        // Act
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.Equal("SM1234567890abcdef", result.Value?.MessageId);
        Assert.Equal(MessageStatus.Delivered, result.Value?.Status); // "read" maps to Delivered for WhatsApp
        Assert.Equal("WhatsApp", result.Value?.AdditionalData["Channel"]);
        Assert.Equal("Read User", result.Value?.AdditionalData["ProfileName"]);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_TwilioWhatsAppConnectorReceiveMessagesAsyncWithButtonResponse()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create a WhatsApp button response webhook (empty body, button data)
        var webhookData = "MessageSid=SM1234567890&From=whatsapp%3A%2B1234567890&To=whatsapp%3A%2B1987654321&Body=&ButtonText=Yes&ButtonPayload=confirm_booking&MessageStatus=received";
        var source = MessageSource.UrlPost(webhookData);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);
        
        var message = result.Value.Messages.First();
        Assert.Equal("SM1234567890", message.Id);
        Assert.Equal("", ((ITextContent)message.Content!).Text); // Empty body for button response
        
        // Check that button data is stored in properties
        Assert.NotNull(message.Properties);
        Assert.True(message.Properties.ContainsKey("ButtonText"));
        Assert.Equal("Yes", message.Properties["ButtonText"].Value);
        Assert.True(message.Properties.ContainsKey("ButtonPayload"));
        Assert.Equal("confirm_booking", message.Properties["ButtonPayload"].Value);
    }

    [Fact]
    public async Task Should_ReturnError_When_TwilioWhatsAppConnectorReceiveMessagesAsyncWithInvalidSource()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create an unsupported source type (XML)
        var xmlData = "<message>Not supported</message>";
        var source = MessageSource.Xml(xmlData);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.NotNull(result.Error);
        Assert.Equal(MessagingErrorCodes.UnsupportedContentType, result.Error?.Code);
        Assert.Contains("Only form data and JSON are supported", result.Error?.Message);
    }

    [Fact]
    public async Task Should_ReturnError_When_TwilioWhatsAppConnectorReceiveMessageStatusAsyncWithInvalidSource()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create an unsupported source type (XML)
        var xmlData = "<status>Not supported</status>";
        var source = MessageSource.Xml(xmlData);

        // Act
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.NotNull(result.Error);
        Assert.Equal(MessagingErrorCodes.UnsupportedContentType, result.Error?.Code);
        Assert.Contains("Only form data and JSON are supported", result.Error?.Message);
    }

    [Fact]
    public void Should_HaveReceiveMessagesCapability_When_TwilioWhatsAppSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Act
        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages),
            "TwilioWhatsApp schema should have ReceiveMessages capability for webhook support");
    }

    [Fact]
    public void Should_HaveHandleMessageStateCapability_When_TwilioWhatsAppSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Act
        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState),
            "TwilioWhatsApp schema should have HandleMessageState capability for status callbacks");
    }

    [Fact]
    public void Should_DoNotHaveReceiveCapabilities_When_SimpleWhatsAppSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;

        // Act
        // Assert
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages),
            "SimpleWhatsApp schema should not have ReceiveMessages capability as it's designed for send-only scenarios");
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState),
            "SimpleWhatsApp schema should not have HandleMessageState capability as it's designed for send-only scenarios");
    }

    [Fact]
    public async Task Should_ThrowNotSupportedException_When_SimpleWhatsAppConnectorReceiveMessages()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var webhookData = "MessageSid=SM1234567890&From=whatsapp%3A%2B1234567890&To=whatsapp%3A%2B1987654321&Body=Test&MessageStatus=received";
        var source = MessageSource.UrlPost(webhookData);

        // Act
        // Assert
        await Assert.ThrowsAsync<MessagingException>(async () => 
            await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowNotSupportedException_When_SimpleWhatsAppConnectorReceiveMessageStatus()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var statusData = "MessageSid=SM1234567890&MessageStatus=delivered&To=whatsapp%3A%2B1987654321&From=whatsapp%3A%2B1234567890";
        var source = MessageSource.UrlPost(statusData);

        // Act
        // Assert
        await Assert.ThrowsAsync<MessagingException>(async () => 
            await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken));
    }
}