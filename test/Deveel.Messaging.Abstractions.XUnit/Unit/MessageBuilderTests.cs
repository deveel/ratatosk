using System.Collections.Generic;

namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "MessageBuilder")]
public class MessageBuilderTests
{
    [Fact]
    public void Should_CreateEmptyMessage_When_MessageDefaultConstructor()
    {
        var message = new Message();

        Assert.NotNull(message);
        Assert.Null(message.Id);
        Assert.Null(message.Sender);
        Assert.Null(message.Receiver);
        Assert.Null(message.Content);
        Assert.Null(message.Properties);
    }

    [Fact]
    public void Should_CopiesMessage_When_MessageWithExistingMessage()
    {
        var originalMessage = new Message
        {
            Id = "test-id",
            Sender = new Endpoint(EndpointType.EmailAddress, "sender@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "receiver@test.com"),
            Content = new TextContent("Test content"),
            Properties = new Dictionary<string, MessageProperty> { { "key", new MessageProperty("key", "value") } }
        };

        var copiedMessage = new Message(originalMessage);

        Assert.Equal("test-id", copiedMessage.Id);
        Assert.Equal("sender@test.com", copiedMessage.Sender!.Address);
        Assert.Equal("receiver@test.com", copiedMessage.Receiver!.Address);
        Assert.IsType<TextContent>(copiedMessage.Content);
        Assert.Equal("Test content", ((TextContent)copiedMessage.Content).Text);
        Assert.Contains("key", copiedMessage.Properties!.Keys);
        Assert.Equal("value", copiedMessage.Properties["key"].Value);
    }

    [Fact]
    public void Should_BuildMessageWithId()
    {
        var messageId = "unique-message-id";
        var message = new MessageBuilder()
            .WithId(messageId)
            .Build();

        Assert.Equal(messageId, message.Id);
    }

    [Fact]
    public void Should_ThrowArgumentException_When_BuildWithEmptyId()
    {
        Assert.Throws<ArgumentException>(() => new MessageBuilder().WithId(""));
    }

    [Fact]
    public void Should_BuildMessageWithSender()
    {
        var sender = new Endpoint(EndpointType.EmailAddress, "sender@test.com");
        var message = new MessageBuilder()
            .From(sender)
            .Build();

        Assert.Equal(EndpointType.EmailAddress, message.Sender!.Type);
        Assert.Equal("sender@test.com", message.Sender.Address);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_BuildWithNullSender()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().From(null!));
    }

    [Fact]
    public void Should_BuildMessageWithEmailSender()
    {
        var email = "sender@test.com";
        var message = new MessageBuilder()
            .FromEmail(email)
            .Build();

        Assert.Equal(EndpointType.EmailAddress, message.Sender!.Type);
        Assert.Equal(email, message.Sender.Address);
    }

    [Fact]
    public void Should_BuildMessageWithPhoneSender()
    {
        var phone = "+1234567890";
        var message = new MessageBuilder()
            .FromPhone(phone)
            .Build();

        Assert.Equal(EndpointType.PhoneNumber, message.Sender!.Type);
        Assert.Equal(phone, message.Sender.Address);
    }

    [Fact]
    public void Should_BuildMessageWithReceiver()
    {
        var receiver = new Endpoint(EndpointType.EmailAddress, "receiver@test.com");
        var message = new MessageBuilder()
            .To(receiver)
            .Build();

        Assert.Equal(EndpointType.EmailAddress, message.Receiver!.Type);
        Assert.Equal("receiver@test.com", message.Receiver.Address);
    }

    [Fact]
    public void Should_BuildMessageWithEmailReceiver()
    {
        var email = "receiver@test.com";
        var message = new MessageBuilder()
            .ToEmail(email)
            .Build();

        Assert.Equal(EndpointType.EmailAddress, message.Receiver!.Type);
        Assert.Equal(email, message.Receiver.Address);
    }

    [Fact]
    public void Should_BuildMessageWithPhoneReceiver()
    {
        var phone = "+1234567890";
        var message = new MessageBuilder()
            .ToPhone(phone)
            .Build();

        Assert.Equal(EndpointType.PhoneNumber, message.Receiver!.Type);
        Assert.Equal(phone, message.Receiver.Address);
    }

    [Fact]
    public void Should_BuildMessageWithContent()
    {
        var content = new TextContent("Test content");
        var message = new MessageBuilder()
            .WithContent(content)
            .Build();

        Assert.IsType<TextContent>(message.Content);
        Assert.Equal("Test content", ((TextContent)message.Content).Text);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_BuildWithNullContent()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().WithContent(null!));
    }

    [Fact]
    public void Should_BuildMessageWithTextContent()
    {
        var text = "Hello, World!";
        var message = new MessageBuilder()
            .WithText(text)
            .Build();

        Assert.IsType<TextContent>(message.Content);
        Assert.Equal(text, ((TextContent)message.Content).Text);
    }

    [Fact]
    public void Should_BuildMessageWithTextContentAndEncoding()
    {
        var text = "Hello, World!";
        var encoding = "utf-8";
        var message = new MessageBuilder()
            .WithText(text, encoding)
            .Build();

        Assert.IsType<TextContent>(message.Content);
        var textContent = (TextContent)message.Content;
        Assert.Equal(text, textContent.Text);
        Assert.Equal(encoding, textContent.Encoding);
    }

    [Fact]
    public void Should_BuildMessageWithProperties()
    {
        var properties = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 123 }
        };

        var message = new MessageBuilder()
            .WithProperties(properties)
            .Build();

        Assert.NotNull(message.Properties);
        Assert.Equal("value1", message.Properties["key1"].Value);
        Assert.Equal(123, message.Properties["key2"].Value);
    }

    [Fact]
    public void Should_BuildMessageWithSingleProperty()
    {
        var message = new MessageBuilder()
            .WithProperty("testKey", "testValue")
            .Build();

        Assert.NotNull(message.Properties);
        Assert.Equal("testValue", message.Properties["testKey"].Value);
    }

    [Fact]
    public void Should_MergeMultipleProperties()
    {
        var message = new MessageBuilder()
            .WithProperty("existingKey", "existingValue")
            .WithProperty("newKey", "newValue")
            .Build();

        Assert.NotNull(message.Properties);
        Assert.Equal("existingValue", message.Properties["existingKey"].Value);
        Assert.Equal("newValue", message.Properties["newKey"].Value);
    }

    [Fact]
    public void Should_BuildMessageWithSubject()
    {
        var subject = "Test Subject";
        var message = new MessageBuilder()
            .WithSubject(subject)
            .Build();

        Assert.NotNull(message.Properties);
        Assert.Equal(subject, message.Properties[KnownMessageProperties.Subject].Value);
    }

    [Fact]
    public void Should_BuildMessageWithRemoteId()
    {
        var remoteId = "remote-message-id";
        var message = new MessageBuilder()
            .WithRemoteId(remoteId)
            .Build();

        Assert.NotNull(message.Properties);
        Assert.Equal(remoteId, message.Properties[KnownMessageProperties.RemoteMessageId].Value);
    }

    [Fact]
    public void Should_BuildMessageWithReplyTo()
    {
        var replyToId = "reply-to-message-id";
        var message = new MessageBuilder()
            .WithReplyTo(replyToId)
            .Build();

        Assert.NotNull(message.Properties);
        Assert.Equal(replyToId, message.Properties[KnownMessageProperties.ReplyTo].Value);
    }

    [Fact]
    public void Should_BuildCompleteMessage_When_FluentInterfaceMethodChaining()
    {
        var message = new MessageBuilder()
            .WithId("test-message-id")
            .FromEmail("sender@test.com")
            .ToEmail("receiver@test.com")
            .WithText("Hello, World!")
            .WithSubject("Test Subject")
            .WithRemoteId("remote-123")
            .WithReplyTo("original-message-id")
            .Build();

        Assert.Equal("test-message-id", message.Id);
        Assert.Equal("sender@test.com", message.Sender!.Address);
        Assert.Equal("receiver@test.com", message.Receiver!.Address);
        Assert.Equal("Hello, World!", ((TextContent)message.Content!).Text);
        Assert.Equal("Test Subject", message.Properties![KnownMessageProperties.Subject].Value);
        Assert.Equal("remote-123", message.Properties[KnownMessageProperties.RemoteMessageId].Value);
        Assert.Equal("original-message-id", message.Properties[KnownMessageProperties.ReplyTo].Value);
    }

    [Fact]
    public void Should_ReturnCorrectValues_When_BuiltMessageAsIMessage()
    {
        var message = new MessageBuilder()
            .WithId("test-id")
            .FromEmail("sender@test.com")
            .ToEmail("receiver@test.com")
            .WithText("Test content")
            .WithProperty("testProp", "testValue")
            .Build();

        IMessage iMessage = message;
        Assert.Equal("test-id", iMessage.Id);
        Assert.Equal("sender@test.com", iMessage.Sender!.Address);
        Assert.Equal("receiver@test.com", iMessage.Receiver!.Address);
        Assert.IsAssignableFrom<IMessageContent>(iMessage.Content);
        Assert.NotNull(iMessage.Properties);
        Assert.Equal("testValue", iMessage.Properties["testProp"].Value);
    }
}
