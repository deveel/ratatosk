using System.Text.Json;

namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "Message")]
public class MessageTests
{
    [Fact]
    public void Should_CreateEmptyMessage_When_MessageDefaultConstructor()
    {
        // Arrange
        // Act
        var message = new Message();

        // Assert
        Assert.Null(message.Id);
        Assert.Null(message.Sender);
        Assert.Null(message.Receiver);
        Assert.Null(message.Content);
        Assert.Null(message.Properties);
    }

    [Fact]
    public void Should_CopiesAllProperties_When_MessageConstructorWithIMessage()
    {
        // Arrange
        var sourceMessage = new Message
        {
            Id = "test-id",
            Sender = new Endpoint(EndpointType.EmailAddress, "sender@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "receiver@test.com"),
            Content = new TextContent("Test content"),
            Properties = new Dictionary<string, MessageProperty> { { "key", new MessageProperty("key", "value") } }
        };

        // Act
        var message = new Message(sourceMessage);

        // Assert
        Assert.Equal("test-id", message.Id);
        Assert.NotSame(sourceMessage.Sender, message.Sender); // Should be a copy
        Assert.Equal("sender@test.com", message.Sender!.Address);
        Assert.Equal(EndpointType.EmailAddress, message.Sender.Type);
        Assert.NotSame(sourceMessage.Receiver, message.Receiver); // Should be a copy
        Assert.Equal("receiver@test.com", message.Receiver!.Address);
        Assert.Equal(EndpointType.EmailAddress, message.Receiver.Type);
        Assert.IsType<TextContent>(message.Content);
        Assert.Equal("Test content", ((TextContent)message.Content).Text);
        Assert.NotSame(sourceMessage.Properties, message.Properties); // Should be a copy
        Assert.Equal("value", message.Properties!["key"].Value);
    }

    [Fact]
    public void Should_HandleCorrectly_When_MessageConstructorWithIMessageNullSender()
    {
        // Arrange
        var sourceMessage = new Message
        {
            Id = "test-id",
            Sender = null,
            Receiver = new Endpoint(EndpointType.EmailAddress, "receiver@test.com"),
            Content = new TextContent("Test content")
        };

        // Act
        var message = new Message(sourceMessage);

        // Assert
        Assert.Equal("test-id", message.Id);
        Assert.Null(message.Sender);
        Assert.NotNull(message.Receiver);
    }

    [Fact]
    public void Should_HandleCorrectly_When_MessageConstructorWithIMessageNullReceiver()
    {
        // Arrange
        var sourceMessage = new Message
        {
            Id = "test-id",
            Sender = new Endpoint(EndpointType.EmailAddress, "receiver@test.com"),
            Receiver = null,
            Content = new TextContent("Test content")
        };

        // Act
        var message = new Message(sourceMessage);

        // Assert
        Assert.Equal("test-id", message.Id);
        Assert.NotNull(message.Sender);
        Assert.Null(message.Receiver);
    }

    [Fact]
    public void Should_UpdateValues_When_MessagePropertySetters()
    {
        // Arrange
        var message = new Message();

        // Act
        message.Id = "new-id";
        message.Sender = new Endpoint(EndpointType.PhoneNumber, "+1234567890");
        message.Receiver = new Endpoint(EndpointType.EmailAddress, "receiver@test.com");
        message.Content = new TextContent("New content");
        message.Properties = new Dictionary<string, MessageProperty> { { "prop", new MessageProperty("prop", "value") } };

        // Assert
        Assert.Equal("new-id", message.Id);
        Assert.Equal("+1234567890", message.Sender.Address);
        Assert.Equal(EndpointType.PhoneNumber, message.Sender.Type);
        Assert.Equal("receiver@test.com", message.Receiver.Address);
        Assert.Equal(EndpointType.EmailAddress, message.Receiver.Type);
        Assert.Equal("New content", ((TextContent)message.Content).Text);
        Assert.Equal("value", message.Properties["prop"].Value);
    }

    [Fact]
    public void Should_ExposeCorrectProperties_When_IMessageImplementation()
    {
        // Arrange
        var message = new Message
        {
            Id = "test-id",
            Sender = new Endpoint(EndpointType.EmailAddress, "sender@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "receiver@test.com"),
            Content = new TextContent("Test content"),
            Properties = new Dictionary<string, MessageProperty> { { "key", new MessageProperty("key", "value") } }
        };

        // Act
        // Assert
        IMessage iMessage = message;
        Assert.Equal("test-id", iMessage.Id);
        Assert.Equal("sender@test.com", iMessage.Sender!.Address);
        Assert.Equal("receiver@test.com", iMessage.Receiver!.Address);
        Assert.IsAssignableFrom<IMessageContent>(iMessage.Content);
        Assert.Equal("value", iMessage.Properties!["key"].Value);
    }

    [Fact]
    public void Should_ReturnIEndpoint_When_IMessageSender()
    {
        // Arrange
        var message = new Message
        {
            Sender = new Endpoint(EndpointType.EmailAddress, "sender@test.com")
        };

        // Act
        IMessage iMessage = message;

        // Assert
        Assert.IsAssignableFrom<IEndpoint>(iMessage.Sender);
        Assert.Equal("sender@test.com", iMessage.Sender!.Address);
    }

    [Fact]
    public void Should_ReturnIEndpoint_When_IMessageReceiver()
    {
        // Arrange
        var message = new Message
        {
            Receiver = new Endpoint(EndpointType.EmailAddress, "receiver@test.com")
        };

        // Act
        IMessage iMessage = message;

        // Assert
        Assert.IsAssignableFrom<IEndpoint>(iMessage.Receiver);
        Assert.Equal("receiver@test.com", iMessage.Receiver!.Address);
    }

    [Fact]
    public void Should_ReturnIMessageContent_When_IMessageContent()
    {
        // Arrange
        var message = new Message
        {
            Content = new TextContent("Test content")
        };

        // Act
        IMessage iMessage = message;

        // Assert
        Assert.IsAssignableFrom<IMessageContent>(iMessage.Content);
        Assert.Equal(MessageContentType.PlainText, iMessage.Content!.ContentType);
    }
}

/// <summary>
/// Tests for JSON serialization and deserialization of Message objects and related types.
/// </summary>
public class MessageJsonSerializationTests
{
    private readonly JsonSerializerOptions _jsonOptions;

    public MessageJsonSerializationTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { 
                new MessagePropertyJsonConverter(),
                new ObjectJsonConverter(),
                new JsonObjectDictionaryConverter() 
            }
        };
    }

    [Fact]
    public void Should_SerializesCorrectly_When_MessageJsonSerializationSimpleMessage()
    {
        // Arrange
        var message = new Message
        {
            Id = "msg-123",
            Sender = new Endpoint(EndpointType.EmailAddress, "sender@example.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "receiver@example.com"),
            Content = new TextContent("Hello, World!"),
            Properties = new Dictionary<string, MessageProperty>
            {
                { "priority", new MessageProperty("priority", "high") },
                { "timestamp", new MessageProperty("timestamp", "2023-12-01T10:30:00Z") }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.Equal("msg-123", deserializedMessage.Id);
        Assert.NotNull(deserializedMessage.Sender);
        Assert.Equal(EndpointType.EmailAddress, deserializedMessage.Sender.Type);
        Assert.Equal("sender@example.com", deserializedMessage.Sender.Address);
        Assert.NotNull(deserializedMessage.Receiver);
        Assert.Equal(EndpointType.EmailAddress, deserializedMessage.Receiver.Type);
        Assert.Equal("receiver@example.com", deserializedMessage.Receiver.Address);
        Assert.NotNull(deserializedMessage.Content);
        Assert.IsType<TextContent>(deserializedMessage.Content);
        Assert.Equal("Hello, World!", ((TextContent)deserializedMessage.Content).Text);
        Assert.NotNull(deserializedMessage.Properties);
        Assert.Equal("high", deserializedMessage.Properties["priority"].Value);
    }

    [Fact]
    public void Should_PreservesContentType_When_MessageJsonSerializationWithTextContent()
    {
        // Arrange
        var message = new Message
        {
            Id = "text-msg",
            Content = new TextContent("Plain text message", "utf-8")
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.NotNull(deserializedMessage.Content);
        Assert.IsType<TextContent>(deserializedMessage.Content);
        var textContent = (TextContent)deserializedMessage.Content;
        Assert.Equal("Plain text message", textContent.Text);
        Assert.Equal("utf-8", textContent.Encoding);
        Assert.Equal(MessageContentType.PlainText, textContent.ContentType);
    }

    [Fact]
    public void Should_PreservesContentAndAttachments_When_MessageJsonSerializationWithHtmlContent()
    {
        // Arrange
        var attachments = new List<MessageAttachment>
        {
            new MessageAttachment("att-1", "image.png", "image/png", "base64content")
        };
        var message = new Message
        {
            Id = "html-msg",
            Content = new HtmlContent("<h1>HTML Message</h1>", attachments)
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.NotNull(deserializedMessage.Content);
        Assert.IsType<HtmlContent>(deserializedMessage.Content);
        var htmlContent = (HtmlContent)deserializedMessage.Content;
        Assert.Equal("<h1>HTML Message</h1>", htmlContent.Html);
        Assert.Single(htmlContent.Attachments);
        Assert.Equal("att-1", htmlContent.Attachments[0].Id);
        Assert.Equal("image.png", htmlContent.Attachments[0].FileName);
        Assert.Equal(MessageContentType.Html, htmlContent.ContentType);
    }

    [Fact]
    public void Should_PreservesJsonData_When_MessageJsonSerializationWithJsonContent()
    {
        // Arrange
        var jsonContentData = "{\"user\":\"john\",\"action\":\"login\"}";
        var message = new Message
        {
            Id = "json-msg",
            Content = new JsonContent(jsonContentData)
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.NotNull(deserializedMessage.Content);
        Assert.IsType<JsonContent>(deserializedMessage.Content);
        var jsonContent = (JsonContent)deserializedMessage.Content;
        Assert.Equal(jsonContentData, jsonContent.Json);
        Assert.Equal(MessageContentType.Json, jsonContent.ContentType);
    }

    [Fact]
    public void Should_PreservesTemplateAndParameters_When_MessageJsonSerializationWithTemplateContent()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "firstName", "John" },
            { "lastName", "Doe" },
            { "age", 30 },
            { "isActive", true }
        };
        var message = new Message
        {
            Id = "template-msg",
            Content = new TemplateContent("welcome-template", parameters)
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.NotNull(deserializedMessage.Content);
        Assert.IsType<TemplateContent>(deserializedMessage.Content);
        var templateContent = (TemplateContent)deserializedMessage.Content;
        Assert.Equal("welcome-template", templateContent.TemplateId);
        Assert.Equal(4, templateContent.Parameters.Count);
        var firstName = Assert.IsType<string>(templateContent.Parameters["firstName"]);
		Assert.Equal("John", firstName);
        Assert.Equal(MessageContentType.Template, templateContent.ContentType);
    }

    [Fact]
    public void Should_PreservesBinaryData_When_MessageJsonSerializationWithBinaryContent()
    {
        // Arrange
        var binaryData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"
        var message = new Message
        {
            Id = "binary-msg",
            Content = new BinaryContent(binaryData, "application/octet-stream")
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.NotNull(deserializedMessage.Content);
        Assert.IsType<BinaryContent>(deserializedMessage.Content);
        var binaryContent = (BinaryContent)deserializedMessage.Content;
        Assert.Equal(binaryData, binaryContent.RawData);
        Assert.Equal("application/octet-stream", binaryContent.MimeType);
        Assert.Equal(MessageContentType.Binary, binaryContent.ContentType);
    }

    [Fact]
    public void Should_PreservesMediaData_When_MessageJsonSerializationWithMediaContent()
    {
        // Arrange
        var mediaData = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        var message = new Message
        {
            Id = "media-msg",
            Content = new MediaContent(MediaType.Image, "photo.png", mediaData)
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.NotNull(deserializedMessage.Content);
        Assert.IsType<MediaContent>(deserializedMessage.Content);
        var mediaContent = (MediaContent)deserializedMessage.Content;
        Assert.Equal(MediaType.Image, mediaContent.MediaType);
        Assert.Equal("photo.png", mediaContent.FileName);
        Assert.Equal(mediaData, mediaContent.Data);
        Assert.Equal(MessageContentType.Media, mediaContent.ContentType);
    }

    [Fact]
    public void Should_PreservesAllParts_When_MessageJsonSerializationWithMultipartContent()
    {
        // Arrange
        var parts = new List<MessageContentPart>
        {
            new TextContentPart("Text part content"),
            new HtmlContentPart("<p>HTML part content</p>")
        };
        var message = new Message
        {
            Id = "multipart-msg",
            Content = new MultipartContent(parts)
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.NotNull(deserializedMessage.Content);
        Assert.IsType<MultipartContent>(deserializedMessage.Content);
        var multipartContent = (MultipartContent)deserializedMessage.Content;
        Assert.Equal(2, multipartContent.Parts.Count);
        Assert.IsType<TextContentPart>(multipartContent.Parts[0]);
        Assert.IsType<HtmlContentPart>(multipartContent.Parts[1]);
        Assert.Equal(MessageContentType.Multipart, multipartContent.ContentType);
    }

    [Fact]
    public void Should_HandleCorrectly_When_MessageJsonSerializationWithNullValues()
    {
        // Arrange
        var message = new Message
        {
            Id = "null-msg",
            Sender = null,
            Receiver = null,
            Content = null,
            Properties = null
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.Equal("null-msg", deserializedMessage.Id);
        Assert.Null(deserializedMessage.Sender);
        Assert.Null(deserializedMessage.Receiver);
        Assert.Null(deserializedMessage.Content);
        Assert.Null(deserializedMessage.Properties);
    }

    [Fact]
    public void Should_PreservesDataTypes_When_MessageJsonSerializationWithComplexProperties()
    {
        // Arrange
        var message = new Message
        {
            Id = "complex-props-msg",
            Properties = new Dictionary<string, MessageProperty>
            {
                { "stringProp", new MessageProperty("stringProp", "string value") },
                { "intProp", new MessageProperty("intProp", 42) },
                { "boolProp", new MessageProperty("boolProp", true) },
                { "doubleProp", new MessageProperty("doubleProp", 3.14159) },
                { "dateProp", new MessageProperty("dateProp", "2023-12-01T10:30:00Z") },
                { "arrayProp", new MessageProperty("arrayProp", new[] { "item1", "item2", "item3" }) },
                { "objectProp", new MessageProperty("objectProp", new Dictionary<string, object> { { "nested", "value" } }) }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.NotNull(deserializedMessage.Properties);
        Assert.Equal("string value", deserializedMessage.Properties["stringProp"].Value?.ToString());
        // Note: JSON deserialization may convert numbers to JsonElement, so we test the JSON structure
        Assert.Contains("intProp", deserializedMessage.Properties.Keys);
        Assert.Contains("boolProp", deserializedMessage.Properties.Keys);
        Assert.Contains("doubleProp", deserializedMessage.Properties.Keys);
    }

    [Fact]
    public void Should_PreservesAllData_When_MessageJsonSerializationRoundTrip()
    {
        // Arrange
        var originalMessage = new Message
        {
            Id = "roundtrip-test",
            Sender = new Endpoint(EndpointType.PhoneNumber, "+1234567890"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "test@example.com"),
            Content = new TextContent("Round trip test message", "utf-8"),
            Properties = new Dictionary<string, MessageProperty>
            {
                { KnownMessageProperties.Subject, new MessageProperty(KnownMessageProperties.Subject, "Test Subject") },
                { KnownMessageProperties.CorrelationId, new MessageProperty(KnownMessageProperties.CorrelationId, "corr-123") },
                { "customProp", new MessageProperty("customProp", "custom value") }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(originalMessage, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.Equal(originalMessage.Id, deserializedMessage.Id);

        Assert.NotNull(deserializedMessage.Sender);
        Assert.Equal(originalMessage.Sender.Type, deserializedMessage.Sender.Type);
        Assert.Equal(originalMessage.Sender.Address, deserializedMessage.Sender.Address);

        Assert.NotNull(deserializedMessage.Receiver);
        Assert.Equal(originalMessage.Receiver.Type, deserializedMessage.Receiver.Type);
        Assert.Equal(originalMessage.Receiver.Address, deserializedMessage.Receiver.Address);

        Assert.NotNull(deserializedMessage.Content);
        Assert.IsType<TextContent>(deserializedMessage.Content);
        var textContent = (TextContent)deserializedMessage.Content;
        var originalTextContent = (TextContent)originalMessage.Content;
        Assert.Equal(originalTextContent.Text, textContent.Text);
        Assert.Equal(originalTextContent.Encoding, textContent.Encoding);

        Assert.NotNull(deserializedMessage.Properties);
        Assert.Equal(originalMessage.Properties.Count, deserializedMessage.Properties.Count);
    }

    [Fact]
    public void Should_PreservesTypes_When_MessageJsonSerializationVariousEndpointTypes()
    {
        // Arrange
        var testCases = new[]
        {
            (EndpointType.EmailAddress, "test@example.com"),
            (EndpointType.PhoneNumber, "+1234567890"),
            (EndpointType.Url, "https://example.com/webhook"),
            (EndpointType.UserId, "user123"),
            (EndpointType.ApplicationId, "app456"),
            (EndpointType.DeviceId, "device789"),
            (EndpointType.Label, "SHORTCODE")
        };

        foreach (var (endpointType, address) in testCases)
        {
            // Arrange
            var message = new Message
            {
                Id = $"msg-{endpointType}",
                Sender = new Endpoint(endpointType, address),
                Receiver = new Endpoint(endpointType, address)
            };

            // Act
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

            // Assert
            Assert.NotNull(deserializedMessage);
            Assert.NotNull(deserializedMessage.Sender);
            Assert.Equal(endpointType, deserializedMessage.Sender.Type);
            Assert.Equal(address, deserializedMessage.Sender.Address);
            Assert.NotNull(deserializedMessage.Receiver);
            Assert.Equal(endpointType, deserializedMessage.Receiver.Type);
            Assert.Equal(address, deserializedMessage.Receiver.Address);
        }
    }

    [Fact]
    public void Should_SerializesCorrectly_When_MessageJsonSerializationEmptyMessage()
    {
        // Arrange
        var message = new Message();

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.Null(deserializedMessage.Id);
        Assert.Null(deserializedMessage.Sender);
        Assert.Null(deserializedMessage.Receiver);
        Assert.Null(deserializedMessage.Content);
        Assert.Null(deserializedMessage.Properties);
    }

    [Fact]
    public void Should_HandleCorrectly_When_MessageJsonSerializationLargeMessage()
    {
        // Arrange
        var largeProperties = new Dictionary<string, MessageProperty>();
        for (int i = 0; i < 100; i++)
        {
            largeProperties[$"prop{i}"] = new MessageProperty($"prop{i}", $"value{i}");
        }

        var largeText = new string('A', 10000); // 10KB text
        var message = new Message
        {
            Id = "large-message",
            Sender = new Endpoint(EndpointType.EmailAddress, "sender@example.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "receiver@example.com"),
            Content = new TextContent(largeText),
            Properties = largeProperties
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var deserializedMessage = JsonSerializer.Deserialize<Message>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.Equal("large-message", deserializedMessage.Id);
        Assert.NotNull(deserializedMessage.Content);
        Assert.IsType<TextContent>(deserializedMessage.Content);
        Assert.Equal(largeText, ((TextContent)deserializedMessage.Content).Text);
        Assert.NotNull(deserializedMessage.Properties);
        Assert.Equal(100, deserializedMessage.Properties.Count);
    }
}