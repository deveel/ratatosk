namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "MessageContent")]
public class MessageContentFactoryTests
{
    [Fact]
    public void Should_CreateTextContent()
    {
        var content = MessageContent.Create(new TextContent("hello"));
        Assert.IsType<TextContent>(content);
        Assert.Equal("hello", ((TextContent)content).Text);
    }

    [Fact]
    public void Should_CreateHtmlContent()
    {
        var content = MessageContent.Create(new HtmlContent("<p>hello</p>"));
        Assert.IsType<HtmlContent>(content);
        Assert.Equal("<p>hello</p>", ((HtmlContent)content).Html);
    }

    [Fact]
    public void Should_CreateJsonContent()
    {
        var content = MessageContent.Create(new JsonContent(@"{""key"":""value""}"));
        Assert.IsType<JsonContent>(content);
        Assert.NotNull(((JsonContent)content).Json);
    }

    [Fact]
    public void Should_CreateBinaryContent()
    {
        var content = MessageContent.Create(new BinaryContent(new byte[] { 1, 2, 3 }, "application/octet-stream"));
        Assert.IsType<BinaryContent>(content);
    }

    [Fact]
    public void Should_CreateMediaContent()
    {
        var urlContent = new MediaContent();
        urlContent.FileUrl = "https://example.com/file.pdf";
        var content = MessageContent.Create(urlContent);
        Assert.IsType<MediaContent>(content);
    }

    [Fact]
    public void Should_CreateLocationContent()
    {
        var content = MessageContent.Create(new LocationContent(45.0, 9.0));
        Assert.IsType<LocationContent>(content);
    }

    [Fact]
    public void Should_CreateButtonContent()
    {
        var content = MessageContent.Create(new ButtonContent("Click", ButtonType.Url, "https://example.com"));
        Assert.IsType<ButtonContent>(content);
    }

    [Fact]
    public void Should_PreserveExistingMessageContent()
    {
        var original = new TextContent("hello");
        var result = MessageContent.Create(original);
        Assert.Same(original, result);
    }

    [Fact]
    public void Should_ReturnNull_When_Null()
    {
        var result = MessageContent.Create(null!);
        Assert.Null(result);
    }
}
