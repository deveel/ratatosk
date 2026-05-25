using System.Text.Json;
using Xunit;

namespace Ratatosk;

/// <summary>
/// Edge case tests for TwilioWhatsAppConnector JSON message source handling,
/// covering various error scenarios, malformed data, and WhatsApp-specific cases.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioWhatsAppConnectorJsonEdgeCase")]
public class TwilioWhatsAppConnectorJsonEdgeCaseTests
{
    [Fact]
    public async Task Should_ReturnError_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookEmptyJsonObject()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var emptyJson = "{}";
        var source = MessageSource.Json(emptyJson);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.NotNull(result.Error);
        Assert.Equal(MessagingErrorCodes.InvalidWebhookData, result.Error?.Code);
    }

    [Fact]
    public async Task Should_HandleGracefully_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookNullStringValues()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // JSON with null string values (which JSON.NET might deserialize as null)
        var webhookJson = new
        {
            MessageSid = "SM1234567890",
            From = "whatsapp:+1234567890",
            To = "whatsapp:+1987654321",
            Body = (string?)null,
            MessageStatus = "received",
            ProfileName = (string?)null
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
        Assert.Equal("", ((ITextContent)message.Content!).Text); // null body should become empty string
    }

    [Fact]
    public async Task Should_ReturnValidOnes_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookBatchWithSomeInvalidMessages()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Batch with some invalid messages (missing required fields)
        var webhookJson = new
        {
            Messages = new object[]
            {
                new { MessageSid = "SM1111111111", From = "whatsapp:+1111111111", To = "whatsapp:+1987654321", Body = "Valid WhatsApp message 1" },
                new { MessageSid = "", From = "whatsapp:+2222222222", To = "whatsapp:+1987654321", Body = "Invalid - empty SID" },
                new { MessageSid = "SM3333333333", From = "", To = "whatsapp:+1987654321", Body = "Invalid - empty From" },
                new { MessageSid = "SM4444444444", From = "whatsapp:+4444444444", To = "whatsapp:+1987654321", Body = "Valid WhatsApp message 2" }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(webhookJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Messages.Count); // Only 2 valid messages

        var messages = result.Value.Messages.ToList();
        Assert.Equal("SM1111111111", messages[0].Id);
        Assert.Equal("SM4444444444", messages[1].Id);
    }

    [Fact]
    public async Task Should_HandleCorrectly_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookMixedEndpointFormats()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Test with mixed endpoint formats
        var webhookJson = new
        {
            MessageSid = "SM1234567890",
            From = "whatsapp:+1234567890", // WhatsApp format
            To = "+1987654321", // Regular phone format
            Body = "Mixed format test",
            MessageStatus = "received"
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
        Assert.Equal("whatsapp:+1234567890", message.Sender?.Address);
        Assert.Equal("+1987654321", message.Receiver?.Address);
        Assert.Equal(EndpointType.PhoneNumber, message.Sender?.Type);
        Assert.Equal(EndpointType.PhoneNumber, message.Receiver?.Type);
    }

    [Fact]
    public async Task Should_HandleCorrectly_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookVeryLargeJson()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create a JSON with many additional WhatsApp properties to test large payload handling
        var largePayload = new Dictionary<string, object>
        {
            ["MessageSid"] = "SM1234567890",
            ["From"] = "whatsapp:+1234567890",
            ["To"] = "whatsapp:+1987654321",
            ["Body"] = "WhatsApp message with many extra fields",
            ["MessageStatus"] = "received",
            ["ProfileName"] = "Test User"
        };

        // Add 100 additional WhatsApp-specific properties
        for (int i = 0; i < 100; i++)
        {
            largePayload[$"WhatsAppField{i}"] = $"WhatsAppValue{i}";
        }

        var jsonPayload = JsonSerializer.Serialize(largePayload);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);

        var message = result.Value.Messages.First();
        Assert.Equal("SM1234567890", message.Id);
        Assert.Equal("WhatsApp message with many extra fields", ((ITextContent)message.Content!).Text);
    }

    [Fact]
    public async Task Should_UseDefault_When_ReceiveMessageStatusAsyncWithWhatsAppJsonStatusCallbackMissingMessageSid()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Status callback missing MessageSid
        var statusJson = new
        {
            MessageStatus = "delivered",
            To = "whatsapp:+1987654321",
            From = "whatsapp:+1234567890",
            ProfileName = "User Name"
        };

        var jsonPayload = JsonSerializer.Serialize(statusJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal("unknown", result.Value.MessageId); // Should default to "unknown"
        Assert.Equal(MessageStatus.Delivered, result.Value.Status);
        Assert.Equal("WhatsApp", result.Value.AdditionalData["Channel"]);
    }

    [Fact]
    public async Task Should_UseDefault_When_ReceiveMessageStatusAsyncWithWhatsAppJsonStatusCallbackMissingMessageStatus()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Status callback missing MessageStatus
        var statusJson = new
        {
            MessageSid = "SM1234567890",
            To = "whatsapp:+1987654321",
            From = "whatsapp:+1234567890",
            ProfileName = "User Name"
        };

        var jsonPayload = JsonSerializer.Serialize(statusJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal("SM1234567890", result.Value.MessageId);
        Assert.Equal(MessageStatus.Unknown, result.Value.Status); // Should default to Unknown
        Assert.Equal("WhatsApp", result.Value.AdditionalData["Channel"]);
    }

    [Fact]
    public async Task Should_HandleCorrectly_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookCaseSensitiveFields()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Test case sensitivity - these should work (Twilio uses PascalCase)
        var jsonPayload = """
        {
            "MessageSid": "SM1234567890",
            "From": "whatsapp:+1234567890",
            "To": "whatsapp:+1987654321",
            "Body": "WhatsApp case sensitive test",
            "MessageStatus": "received",
            "ProfileName": "Test User"
        }
        """;

        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);

        var message = result.Value.Messages.First();
        Assert.Equal("SM1234567890", message.Id);
    }

    [Fact]
    public async Task Should_ReturnError_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookWrongCaseFields()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Test wrong case (camelCase instead of PascalCase)
        var jsonPayload = """
        {
            "messageSid": "SM1234567890",
            "from": "whatsapp:+1234567890",
            "to": "whatsapp:+1987654321",
            "body": "Wrong case test",
            "messageStatus": "received",
            "profileName": "Test User"
        }
        """;

        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.NotNull(result.Error);
        Assert.Equal(MessagingErrorCodes.InvalidWebhookData, result.Error?.Code);
    }

    [Fact]
    public async Task Should_HandleCorrectly_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookSpecialCharactersInFields()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Test with special characters in various fields
        var webhookJson = new
        {
            MessageSid = "SM123-456_789.abc",
            From = "whatsapp:+1234567890",
            To = "whatsapp:+1987654321",
            Body = "Special chars test: @#$%^&*()",
            MessageStatus = "received",
            ProfileName = "Jos Mara oo-Gonzlez",
            ButtonText = "Click Me! ??",
            ButtonPayload = "action_123-abc_xyz.confirm"
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
        Assert.Equal("SM123-456_789.abc", message.Id);
        Assert.Equal("Special chars test: @#$%^&*()", ((ITextContent)message.Content!).Text);
    }

    [Fact]
    public async Task Should_HandleCorrectly_When_ReceiveMessageStatusAsyncWithWhatsAppJsonStatusCallbackExtremelyLongProperties()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Status callback with extremely long property values
        var longString = new string('W', 10000); // WhatsApp allows longer messages
        var statusJson = new
        {
            MessageSid = "SM1234567890",
            MessageStatus = "failed",
            ErrorMessage = longString,
            ProfileName = longString.Substring(0, 1000), // Shorter profile name
            ExtraData = longString
        };

        var jsonPayload = JsonSerializer.Serialize(statusJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessageStatusAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal("SM1234567890", result.Value.MessageId);
        Assert.Equal(MessageStatus.DeliveryFailed, result.Value.Status);
        Assert.Equal("WhatsApp", result.Value.AdditionalData["Channel"]);

        // Verify long property is preserved
        Assert.True(result.Value.AdditionalData.ContainsKey("ErrorMessage"));
        Assert.Equal(longString, result.Value.AdditionalData["ErrorMessage"]);
    }

    [Fact]
    public async Task Should_ReturnError_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookEmptyArrayInMessages()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // JSON with empty Messages array
        var webhookJson = new
        {
            Messages = new object[] { }
        };

        var jsonPayload = JsonSerializer.Serialize(webhookJson);
        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.NotNull(result.Error);
        Assert.Equal(MessagingErrorCodes.InvalidWebhookData, result.Error?.Code);
    }

    [Fact]
    public async Task Should_ReturnError_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookNonArrayMessages()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // JSON with Messages as a string instead of array
        var jsonPayload = """
        {
            "Messages": "not an array"
        }
        """;

        var source = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(source, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.NotNull(result.Error);
        Assert.Equal(ConnectorErrorCodes.ReceiveMessagesError, result.Error?.Code);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookComplexTemplateResponse()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Complex WhatsApp template response with multiple interaction elements
        var webhookJson = new
        {
            MessageSid = "SM1234567890",
            From = "whatsapp:+1234567890",
            To = "whatsapp:+1987654321",
            Body = "",
            MessageStatus = "received",
            ButtonText = "Book Appointment",
            ButtonPayload = "book_123",
            ListId = "services_menu",
            ListTitle = "Available Services",
            ListDescription = "Please select a service",
            ListSelection = "service_haircut",
            ReferralMessage = "Referred from website",
            ContextMessageId = "wamid.abc123",
            ProfileName = "Mara Jos",
            ForwardedCount = "2",
            FrequentlyForwarded = "true"
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
    public async Task Should_ParseCorrectly_When_ReceiveMessagesAsyncWithWhatsAppJsonWebhookBusinessAccountInfo()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioWhatsApp;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioWhatsAppConnector(schema, connectionSettings);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // WhatsApp Business API specific fields
        var webhookJson = new
        {
            MessageSid = "SM1234567890",
            From = "whatsapp:+1234567890",
            To = "whatsapp:+1987654321",
            Body = "Hello from WhatsApp Business",
            MessageStatus = "received",
            ProfileName = "Business Customer",
            BusinessDisplayName = "Local Business Inc",
            BusinessVerified = "true",
            BusinessCategory = "retail",
            BusinessDescription = "Your local business",
            Latitude = "40.7128",
            Longitude = "-74.0060",
            Address = "123 Business St, New York, NY"
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
        Assert.Equal("Hello from WhatsApp Business", ((ITextContent)message.Content!).Text);
    }

    private static ConnectionSettings CreateValidConnectionSettings()
    {
        return new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
    }
}
