namespace Deveel.Messaging;

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
