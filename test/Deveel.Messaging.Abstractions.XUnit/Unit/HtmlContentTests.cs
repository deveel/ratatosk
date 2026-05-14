namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "HtmlContent")]
public class HtmlContentTests
{
    [Fact]
    public void Should_SetProperties_When_HtmlContentConstructorWithHtml()
    {
        // Arrange
        var html = "<html><body>Hello, World!</body></html>";

        // Act
        var content = new HtmlContent(html);

        // Assert
        Assert.Equal(html, content.Html);
        Assert.NotNull(content.Attachments);
        Assert.Empty(content.Attachments);
        Assert.Equal(MessageContentType.Html, content.ContentType);
    }

    [Fact]
    public void Should_SetProperties_When_HtmlContentConstructorWithHtmlAndAttachments()
    {
        // Arrange
        var html = "<html><body>Hello, World!</body></html>";
        var attachments = new List<MessageAttachment>
        {
            new MessageAttachment("1", "file1.txt", "text/plain", "base64content1"),
            new MessageAttachment("2", "file2.pdf", "application/pdf", "base64content2")
        };

        // Act
        var content = new HtmlContent(html, attachments);

        // Assert
        Assert.Equal(html, content.Html);
        Assert.NotNull(content.Attachments);
        Assert.Equal(2, content.Attachments.Count);
        Assert.Equal("file1.txt", content.Attachments[0].FileName);
        Assert.Equal("file2.pdf", content.Attachments[1].FileName);
    }

    [Fact]
    public void Should_CopiesProperties_When_HtmlContentConstructorWithIHtmlContent()
    {
        // Arrange
        var sourceAttachments = new List<MessageAttachment>
        {
            new MessageAttachment("1", "source.txt", "text/plain", "content")
        };
        var sourceContent = new HtmlContent("<p>Source HTML</p>", sourceAttachments);

        // Act
        var content = new HtmlContent(sourceContent);

        // Assert
        Assert.Equal("<p>Source HTML</p>", content.Html);
        Assert.NotNull(content.Attachments);
        Assert.Single(content.Attachments);
        Assert.Equal("source.txt", content.Attachments[0].FileName);
        Assert.NotSame(sourceContent.Attachments, content.Attachments); // Should be a copy
    }

    [Fact]
    public void Should_CreateEmptyList_When_HtmlContentConstructorWithNullAttachments()
    {
        // Arrange
        var html = "<html><body>Test</body></html>";

        // Act
        var content = new HtmlContent(html, null);

        // Assert
        Assert.Equal(html, content.Html);
        Assert.NotNull(content.Attachments);
        Assert.Empty(content.Attachments);
    }

    [Fact]
    public void Should_UpdateHtml_When_HtmlContentPropertySetter()
    {
        // Arrange
        var content = new HtmlContent("<p>Original</p>");

        // Act
        content.Html = "<p>Updated</p>";

        // Assert
        Assert.Equal("<p>Updated</p>", content.Html);
    }

    [Fact]
    public void Should_CanBeModified_When_HtmlContentAttachmentsProperty()
    {
        // Arrange
        var content = new HtmlContent("<p>Test</p>");

        // Act
        content.Attachments.Add(new MessageAttachment("1", "test.txt", "text/plain", "content"));

        // Assert
        Assert.Single(content.Attachments);
        Assert.Equal("test.txt", content.Attachments[0].FileName);
    }

    [Fact]
    public void Should_ExposeCorrectProperties_When_IHtmlContentImplementation()
    {
        // Arrange
        var attachments = new List<MessageAttachment>
        {
            new MessageAttachment("1", "file.txt", "text/plain", "content")
        };
        var content = new HtmlContent("<p>Test HTML</p>", attachments);

        // Act
        // Assert
        IHtmlContent iHtmlContent = content;
        Assert.Equal("<p>Test HTML</p>", iHtmlContent.Html);
        Assert.NotNull(iHtmlContent.Attachments);
        Assert.Single(iHtmlContent.Attachments);
        Assert.IsAssignableFrom<IAttachment>(iHtmlContent.Attachments.First());
    }

    [Fact]
    public void Should_ExposeCorrectContentType_When_IMessageContentImplementation()
    {
        // Arrange
        var content = new HtmlContent("<p>Test</p>");

        // Act
        // Assert
        IMessageContent iMessageContent = content;
        Assert.Equal(MessageContentType.Html, iMessageContent.ContentType);
    }

    [Theory]
    [InlineData("<html><body>Simple HTML</body></html>")]
    [InlineData("<p>Paragraph with <strong>bold</strong> text</p>")]
    [InlineData("<div><h1>Title</h1><p>Content</p></div>")]
    [InlineData("")]
    public void Should_HandleCorrectly_When_HtmlContentVariousHtmlValues(string html)
    {
        // Arrange
        // Act
        var content = new HtmlContent(html);

        // Assert
        Assert.Equal(html, content.Html);
        Assert.Equal(MessageContentType.Html, content.ContentType);
    }
}