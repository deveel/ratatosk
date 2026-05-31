namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "Message")]
public class MessageSenderPreservationTests
{
    [Fact]
    public void Should_PreserveISender_When_CopyConstructorWithSender()
    {
        var sender = new PhoneSender("+1234567890", name: "my-phone");
        var original = new Message
        {
            Sender = sender,
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+0987654321"),
            Content = new TextContent("Hello")
        };

        var copy = new Message(original);

        Assert.Same(sender, copy.Sender);
        Assert.IsAssignableFrom<ISender>(copy.Sender);
    }

    [Fact]
    public void Should_PreserveISender_When_CopyConstructorWithReceiverAsISender()
    {
        var receiver = new PhoneSender("+0987654321", name: "receiver-phone");
        var original = new Message
        {
            Sender = new Endpoint(EndpointType.PhoneNumber, "+1234567890"),
            Receiver = receiver,
            Content = new TextContent("Hello")
        };

        var copy = new Message(original);

        Assert.Same(receiver, copy.Receiver);
        Assert.IsAssignableFrom<ISender>(copy.Receiver);
    }

    [Fact]
    public void Should_PreserveISender_When_MessageBuilderFromWithISender()
    {
        var sender = new PhoneSender("+1234567890", name: "my-phone");

        var message = new MessageBuilder()
            .From(sender)
            .Build();

        Assert.Same(sender, message.Sender);
        Assert.IsAssignableFrom<ISender>(message.Sender);
    }

    [Fact]
    public void Should_PreserveISender_When_MessageBuilderToWithISender()
    {
        var receiver = new PhoneSender("+0987654321", name: "receiver-phone");

        var message = new MessageBuilder()
            .To(receiver)
            .Build();

        Assert.Same(receiver, message.Receiver);
        Assert.IsAssignableFrom<ISender>(message.Receiver);
    }

    [Fact]
    public void Should_WrapIEndpoint_When_CopyConstructorWithNonSender()
    {
        var endpoint = new Endpoint(EndpointType.EmailAddress, "test@example.com");
        var original = new Message
        {
            Sender = endpoint,
            Receiver = new Endpoint(EndpointType.EmailAddress, "other@example.com"),
            Content = new TextContent("Hello")
        };

        var copy = new Message(original);

        Assert.NotSame(endpoint, copy.Sender);
        Assert.Equal(endpoint.Address, copy.Sender!.Address);
        Assert.Equal(endpoint.Type, copy.Sender.Type);
    }

    [Fact]
    public void Should_PassNullSender_When_CopyConstructorWithNullSender()
    {
        var original = new Message
        {
            Sender = null,
            Content = new TextContent("Hello")
        };

        var copy = new Message(original);

        Assert.Null(copy.Sender);
    }
}
