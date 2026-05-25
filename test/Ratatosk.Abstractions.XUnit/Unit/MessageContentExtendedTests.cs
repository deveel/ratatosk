namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Feature", "MessageContent")]
public class MessageContentExtendedTests
{
    [Fact]
    public void Should_CreateTextContent_FromInterface()
    {
        IMessageContent source = new TextContent("hello");
        var result = MessageContent.Create(source);
        Assert.NotNull(result);
        Assert.Equal(MessageContentType.PlainText, result.ContentType);
    }

    [Fact]
    public void Should_CreateFromExistingMessageContent()
    {
        var source = new TextContent("hello");
        var result = MessageContent.Create(source);
        Assert.Same(source, result);
    }

    [Fact]
    public void Should_CreateButtonContent_FromInterface()
    {
        IMessageContent source = new ButtonContent("Click", ButtonType.Url, "url");
        var result = MessageContent.Create(source);
        Assert.NotNull(result);
        Assert.Equal(MessageContentType.Button, result.ContentType);
    }

    [Fact]
    public void Should_CreateQuickReplyContent_FromInterface()
    {
        IMessageContent source = new QuickReplyContent("Yes", "p");
        var result = MessageContent.Create(source);
        Assert.NotNull(result);
        Assert.Equal(MessageContentType.QuickReply, result.ContentType);
    }

    [Fact]
    public void Should_CreateCarouselContent_FromInterface()
    {
        IMessageContent source = new CarouselContent(new[] { new CarouselCard("Card") });
        var result = MessageContent.Create(source);
        Assert.NotNull(result);
        Assert.Equal(MessageContentType.Carousel, result.ContentType);
    }

    [Fact]
    public void Should_CreateListPickerContent_FromInterface()
    {
        IMessageContent source = new ListPickerContent(items: new[] { new ListPickerItem("Item") });
        var result = MessageContent.Create(source);
        Assert.NotNull(result);
        Assert.Equal(MessageContentType.ListPicker, result.ContentType);
    }

    [Fact]
    public void Should_ReturnNull_WhenInputIsNull()
    {
        var result = MessageContent.Create(null);
        Assert.Null(result);
    }

    [Fact]
    public void Should_Throw_WhenTypeNotSupported()
    {
        var unsupported = new UnsupportedContent();
        Assert.Throws<NotSupportedException>(() => MessageContent.Create(unsupported));
    }

    private class UnsupportedContent : IMessageContent
    {
        public MessageContentType ContentType => (MessageContentType)999;
    }
}
