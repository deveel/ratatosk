namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "MessageBuilderExtensions")]
public class MessageBuilderExtensionsTests
{
    [Fact]
    public void Should_SetSenderRef_When_FromSender()
    {
        var message = new MessageBuilder()
            .FromSender("my-sender")
            .Build();

        Assert.NotNull(message.Sender);
        Assert.IsType<SenderRef>(message.Sender);
        var senderRef = (SenderRef)message.Sender;
        Assert.Equal("my-sender", senderRef.SenderName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_FromSenderWithInvalidName(string name)
    {
        Assert.Throws<ArgumentException>(() => new MessageBuilder().FromSender(name));
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_FromSenderWithNullName()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().FromSender(null!));
    }

    [Fact]
    public void Should_PreserveSenderRef_When_Build()
    {
        var message = new MessageBuilder()
            .FromSender("deferred-sender")
            .WithText("Hello")
            .Build();

        Assert.IsType<SenderRef>(message.Sender);
        Assert.Equal("deferred-sender", ((SenderRef)message.Sender!).SenderName);
    }

    [Fact]
    public void Should_ReturnBuilder_When_FromSender()
    {
        var builder = new MessageBuilder();
        var result = builder.FromSender("test-sender");

        Assert.Same(builder, result);
    }
}
