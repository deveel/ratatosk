namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "JsonContent")]
public class JsonContentTests
{
    [Fact]
    public void Should_CreateEmptyInstance_When_JsonContentDefaultConstructor()
    {
        // Arrange
        // Act
        var content = new JsonContent();

        // Assert
        Assert.Equal("", content.Json);
        Assert.Equal(MessageContentType.Json, content.ContentType);
    }

    [Fact]
    public void Should_SetProperty_When_JsonContentConstructorWithJsonString()
    {
        // Arrange
        var json = "{\"name\":\"John\",\"age\":30}";

        // Act
        var content = new JsonContent(json);

        // Assert
        Assert.Equal(json, content.Json);
        Assert.Equal(MessageContentType.Json, content.ContentType);
    }

    [Fact]
    public void Should_CopiesProperty_When_JsonContentConstructorWithIJsonContent()
    {
        // Arrange
        var sourceContent = new JsonContent("{\"source\":\"data\"}");

        // Act
        var content = new JsonContent(sourceContent);

        // Assert
        Assert.Equal("{\"source\":\"data\"}", content.Json);
        Assert.Equal(MessageContentType.Json, content.ContentType);
    }

    [Fact]
    public void Should_SetEmptyString_When_JsonContentConstructorWithNullIJsonContent()
    {
        // Arrange
        // Act
        var content = new JsonContent((IJsonContent)null!);

        // Assert
        Assert.Equal("", content.Json);
        Assert.Equal(MessageContentType.Json, content.ContentType);
    }

    [Fact]
    public void Should_UpdateJson_When_JsonContentPropertySetter()
    {
        // Arrange
        var content = new JsonContent();

        // Act
        content.Json = "{\"updated\":\"value\"}";

        // Assert
        Assert.Equal("{\"updated\":\"value\"}", content.Json);
    }

    [Fact]
    public void Should_ExposeCorrectProperty_When_IJsonContentImplementation()
    {
        // Arrange
        var content = new JsonContent("{\"interface\":\"test\"}");

        // Act
        // Assert
        IJsonContent iJsonContent = content;
        Assert.Equal("{\"interface\":\"test\"}", iJsonContent.Json);
    }

    [Fact]
    public void Should_ExposeCorrectContentType_When_IMessageContentImplementation()
    {
        // Arrange
        var content = new JsonContent();

        // Act
        // Assert
        IMessageContent iMessageContent = content;
        Assert.Equal(MessageContentType.Json, iMessageContent.ContentType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("{\"simple\":\"object\"}")]
    [InlineData("{\"nested\":{\"object\":{\"value\":123}}}")]
    [InlineData("[1,2,3,4,5]")]
    [InlineData("[{\"id\":1,\"name\":\"John\"},{\"id\":2,\"name\":\"Jane\"}]")]
    [InlineData("\"simple string\"")]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("null")]
    public void Should_HandleCorrectly_When_JsonContentVariousJsonValues(string json)
    {
        // Arrange
        // Act
        var content = new JsonContent(json);

        // Assert
        Assert.Equal(json, content.Json);
        Assert.Equal(MessageContentType.Json, content.ContentType);
    }

    [Fact]
    public void Should_HandleCorrectly_When_JsonContentComplexJsonObject()
    {
        // Arrange
        var complexJson = @"{
            ""user"": {
                ""id"": 123,
                ""name"": ""John Doe"",
                ""email"": ""john@example.com"",
                ""preferences"": {
                    ""theme"": ""dark"",
                    ""notifications"": true
                },
                ""tags"": [""admin"", ""power-user""]
            },
            ""timestamp"": ""2023-12-01T10:30:00Z""
        }";

        // Act
        var content = new JsonContent(complexJson);

        // Assert
        Assert.Equal(complexJson, content.Json);
        Assert.Equal(MessageContentType.Json, content.ContentType);
    }

    [Fact]
    public void Should_CopiesCorrectly_When_JsonContentWithMockIJsonContent()
    {
        // Arrange
        var mockJsonContent = new MockJsonContent
        {
            Json = "{\"mock\":\"content\"}"
        };

        // Act
        var content = new JsonContent(mockJsonContent);

        // Assert
        Assert.Equal("{\"mock\":\"content\"}", content.Json);
    }

    // Helper class for testing
    private class MockJsonContent : IJsonContent
    {
        public string Json { get; set; } = "";
        public MessageContentType ContentType => MessageContentType.Json;
    }
}