using Xunit;

namespace Deveel.Messaging;

/// <summary>
/// Tests to verify that Twilio schemas correctly declare all the capabilities
/// that are implemented by the connectors.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioSchemaCapability")]
public class TwilioSchemaCapabilityTests
{
    [Fact]
    public void Should_HaveReceiveMessagesCapability_When_TwilioSmsSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioSms;

        // Act
        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages),
            "TwilioSms schema should have ReceiveMessages capability");
    }

    [Fact]
    public void Should_HaveHandlerMessageStateCapability_When_TwilioSmsSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioSms;

        // Act
        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState),
            "TwilioSms schema should have HandleMessageState capability for receiving status updates via webhooks");
    }

    [Fact]
    public void Should_HaveMessageStatusQueryCapability_When_TwilioSmsSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioSms;

        // Act
        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery),
            "TwilioSms schema should have MessageStatusQuery capability for active status queries");
    }

    [Fact]
    public void Should_HaveReceiveMessagesCapability_When_TwilioWhatsAppSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Act
        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages),
            "TwilioWhatsApp schema should have ReceiveMessages capability");
    }

    [Fact]
    public void Should_HaveHandlerMessageStateCapability_When_TwilioWhatsAppSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Act
        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState),
            "TwilioWhatsApp schema should have HandleMessageState capability for receiving status updates via webhooks");
    }

    [Fact]
    public void Should_HaveMessageStatusQueryCapability_When_TwilioWhatsAppSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;

        // Act
        // Assert
        Assert.True(schema.Capabilities.HasFlag(ChannelCapability.MessageStatusQuery),
            "TwilioWhatsApp schema should have MessageStatusQuery capability for active status queries");
    }

    [Fact]
    public async Task Should_SupportReceiveMessagesAsync_When_TwilioSmsConnectorIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioSms;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioSmsConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Create a valid Twilio webhook source
        var webhookData = "MessageSid=SM1234567890&From=%2B1234567890&To=%2B1987654321&Body=Test&MessageStatus=received";
        var source = MessageSource.UrlPost(webhookData);

        // Act
        // Assert
        var result = await connector.ReceiveMessagesAsync(source, CancellationToken.None);
        
        // The result may fail due to parsing (which is expected since we're using a mock),
        // but it should not fail due to capability validation
        Assert.True(result.Successful || result.Error?.ErrorCode != "CAPABILITY_NOT_SUPPORTED");
    }

    [Fact]
    public async Task Should_SupportReceiveMessageStatusAsync_When_TwilioSmsConnectorIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioSms;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        
        var connector = new TwilioSmsConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Create a valid Twilio status callback source
        var statusData = "MessageSid=SM1234567890&MessageStatus=delivered&To=%2B1987654321&From=%2B1234567890";
        var source = MessageSource.UrlPost(statusData);

        // Act
        // Assert
        var result = await connector.ReceiveMessageStatusAsync(source, CancellationToken.None);
        
        // The result should be successful since we have valid status callback data
        Assert.True(result.Successful);
        Assert.Equal("SM1234567890", result.Value?.MessageId);
        Assert.Equal(MessageStatus.Delivered, result.Value?.Status);
    }

    [Fact]
    public void Should_DoNotHaveReceiveMessagesCapability_When_SimpleSmsSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;

        // Act
        // Assert
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages),
            "SimpleSms schema should not have ReceiveMessages capability as it's designed for send-only scenarios");
    }

    [Fact]
    public void Should_DoNotHaveHandlerMessageStateCapability_When_SimpleSmsSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;

        // Act
        // Assert
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState),
            "SimpleSms schema should not have HandleMessageState capability as it's designed for send-only scenarios");
    }

    [Fact]
    public void Should_HaveAllExpectedCapabilities_When_TwilioSmsSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioSms;
        var expectedCapabilities = 
            ChannelCapability.SendMessages |
            ChannelCapability.ReceiveMessages |
            ChannelCapability.MessageStatusQuery |
            ChannelCapability.HandleMessageState |
            ChannelCapability.BulkMessaging |
            ChannelCapability.HealthCheck;

        // Act
        // Assert
        Assert.Equal(expectedCapabilities, schema.Capabilities);
    }

    [Fact]
    public void Should_HaveAllExpectedCapabilities_When_TwilioWhatsAppSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var expectedCapabilities = 
            ChannelCapability.SendMessages |
            ChannelCapability.ReceiveMessages |
            ChannelCapability.MessageStatusQuery |
            ChannelCapability.HandleMessageState |
            ChannelCapability.Templates |
            ChannelCapability.MediaAttachments |
            ChannelCapability.HealthCheck;

        // Act
        // Assert
        Assert.Equal(expectedCapabilities, schema.Capabilities);
    }

    [Fact]
    public void Should_DoNotHaveReceiveMessagesCapability_When_SimpleWhatsAppSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;

        // Act
        // Assert
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages),
            "SimpleWhatsApp schema should not have ReceiveMessages capability as it's designed for send-only scenarios");
    }

    [Fact]
    public void Should_DoNotHaveHandlerMessageStateCapability_When_SimpleWhatsAppSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleWhatsApp;

        // Act
        // Assert
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState),
            "SimpleWhatsApp schema should not have HandleMessageState capability as it's designed for send-only scenarios");
    }

    [Fact]
    public void Should_DoNotHaveReceiveMessagesCapability_When_WhatsAppTemplatesSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.WhatsAppTemplates;

        // Act
        // Assert
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages),
            "WhatsAppTemplates schema should not have ReceiveMessages capability as it's designed for send-only scenarios");
    }

    [Fact]
    public void Should_DoNotHaveHandlerMessageStateCapability_When_WhatsAppTemplatesSchemaIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.WhatsAppTemplates;

        // Act
        // Assert
        Assert.False(schema.Capabilities.HasFlag(ChannelCapability.HandleMessageState),
            "WhatsAppTemplates schema should not have HandleMessageState capability as it's designed for send-only scenarios");
    }
}