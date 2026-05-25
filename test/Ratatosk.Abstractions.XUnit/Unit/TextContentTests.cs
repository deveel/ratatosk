namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "TextContent")]
public class TextContentTests
{
    [Fact]
    public void Should_SetTextAndEncoding_When_TextContentConstructor()
    {
        // Arrange
        var text = "Hello, World!";
        var encoding = "utf-8";

        // Act
        var content = new TextContent(text, encoding);

        // Assert
        Assert.Equal(text, content.Text);
        Assert.Equal(encoding, content.Encoding);
        Assert.Equal(MessageContentType.PlainText, content.ContentType);
    }

    [Fact]
    public void Should_SetTextOnly_When_TextContentConstructorWithoutEncoding()
    {
        // Arrange
        var text = "Hello, World!";

        // Act
        var content = new TextContent(text);

        // Assert
        Assert.Equal(text, content.Text);
        Assert.Null(content.Encoding);
        Assert.Equal(MessageContentType.PlainText, content.ContentType);
    }

    [Fact]
    public void Should_AcceptsNull_When_TextContentConstructorWithNullText()
    {
        // Arrange
        // Act
        var content = new TextContent(null!);

        // Assert
        Assert.Null(content.Text);
        Assert.Null(content.Encoding);
        Assert.Equal(MessageContentType.PlainText, content.ContentType);
    }

    [Fact]
    public void Should_CopiesProperties_When_TextContentConstructorWithITextContent()
    {
        // Arrange
        var sourceContent = new TextContent("Source text", "utf-8");

        // Act
        var content = new TextContent(sourceContent);

        // Assert
        Assert.Equal("Source text", content.Text);
        Assert.Equal("utf-8", content.Encoding);
        Assert.Equal(MessageContentType.PlainText, content.ContentType);
    }

    [Fact]
    public void Should_UpdateValues_When_TextContentPropertySetters()
    {
        // Arrange
        var content = new TextContent("Initial text");

        // Act
        content.Text = "Updated text";
        content.Encoding = "utf-16";

        // Assert
        Assert.Equal("Updated text", content.Text);
        Assert.Equal("utf-16", content.Encoding);
    }

    [Fact]
    public void Should_ExposeCorrectProperties_When_ITextContentImplementation()
    {
        // Arrange
        var content = new TextContent("Test text", "utf-8");

        // Act
        // Assert
        ITextContent iTextContent = content;
        Assert.Equal("Test text", iTextContent.Text);
        Assert.Equal("utf-8", iTextContent.Encoding);
    }

    [Fact]
    public void Should_ExposeCorrectContentType_When_IMessageContentImplementation()
    {
        // Arrange
        var content = new TextContent("Test text");

        // Act
        // Assert
        IMessageContent iMessageContent = content;
        Assert.Equal(MessageContentType.PlainText, iMessageContent.ContentType);
    }

    [Theory]
    [InlineData("Simple text")]
    [InlineData("Text with special characters: ")]
    [InlineData("Text with numbers: 123456789")]
    [InlineData("Text with symbols: !@#$%^&*()")]
    [InlineData("")]
    public void Should_HandleCorrectly_When_TextContentVariousTextValues(string text)
    {
        // Arrange
        // Act
        var content = new TextContent(text);

        // Assert
        Assert.Equal(text, content.Text);
        Assert.Equal(MessageContentType.PlainText, content.ContentType);
    }

    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("ascii")]
    [InlineData("iso-8859-1")]
    public void Should_HandleCorrectly_When_TextContentVariousEncodings(string encoding)
    {
        // Arrange
        // Act
        var content = new TextContent("Test text", encoding);

        // Assert
        Assert.Equal(encoding, content.Encoding);
        Assert.Equal("Test text", content.Text);
    }
}