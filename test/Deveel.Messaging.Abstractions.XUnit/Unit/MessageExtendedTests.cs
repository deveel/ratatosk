namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "Message")]
public class MessageExtendedTests
{
    [Fact]
    public void Should_CreateFromIMessage()
    {
        IMessage source = new Message
        {
            Id = "src-1",
            Sender = Endpoint.EmailAddress("from@test.com"),
            Receiver = Endpoint.PhoneNumber("+123"),
            Content = new TextContent("body"),
            Properties = new Dictionary<string, MessageProperty>
            {
                ["key"] = new MessageProperty("key", "val")
            }
        };

        var copy = new Message(source);

        Assert.Equal("src-1", copy.Id);
        Assert.NotNull(copy.Sender);
        Assert.Equal("from@test.com", copy.Sender.Address);
        Assert.NotNull(copy.Receiver);
        Assert.Equal("+123", copy.Receiver.Address);
        Assert.NotNull(copy.Content);
        Assert.NotNull(copy.Properties);
        Assert.True(copy.Properties.ContainsKey("key"));
    }

    [Fact]
    public void Should_CreateFromIMessage_WithNullContent()
    {
        IMessage source = new Message { Id = "src-1" };
        var copy = new Message(source);
        Assert.Equal("src-1", copy.Id);
        Assert.Null(copy.Content);
        Assert.Null(copy.Properties);
    }

    [Fact]
    public void Should_BuildWithBuilder()
    {
        var msg = new MessageBuilder()
            .WithId("b1")
            .WithText("text")
            .FromEmail("a@b.com")
            .ToPhone("+1")
            .Build();

        Assert.Equal("b1", msg.Id);
        Assert.NotNull(msg.Content);
        Assert.Equal(MessageContentType.PlainText, msg.Content.ContentType);
    }
}
