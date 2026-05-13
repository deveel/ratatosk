namespace Deveel.Messaging.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "MessageBuilder")]
    public class MessageBuilderTests
    {
        [Fact]
        public void Should_BuildMessageWithId()
        {
            var message = new MessageBuilder()
                .WithId("msg-1")
                .Build();

            Assert.Equal("msg-1", message.Id);
        }

        [Fact]
        public void Should_Throw_When_EmptyId()
        {
            Assert.Throws<ArgumentException>(() => new MessageBuilder().WithId(""));
        }

        [Fact]
        public void Should_BuildMessageWithEmailSender()
        {
            var message = new MessageBuilder()
                .FromEmail("test@example.com")
                .Build();

            Assert.NotNull(message.Sender);
            Assert.Equal(EndpointType.EmailAddress, message.Sender.Type);
            Assert.Equal("test@example.com", message.Sender.Address);
        }

        [Fact]
        public void Should_BuildMessageWithPhoneSender()
        {
            var message = new MessageBuilder()
                .FromPhone("+15551234567")
                .Build();

            Assert.NotNull(message.Sender);
            Assert.Equal(EndpointType.PhoneNumber, message.Sender.Type);
            Assert.Equal("+15551234567", message.Sender.Address);
        }

        [Fact]
        public void Should_BuildMessageWithEmailReceiver()
        {
            var message = new MessageBuilder()
                .ToEmail("recipient@example.com")
                .Build();

            Assert.NotNull(message.Receiver);
            Assert.Equal(EndpointType.EmailAddress, message.Receiver.Type);
            Assert.Equal("recipient@example.com", message.Receiver.Address);
        }

        [Fact]
        public void Should_BuildMessageWithPhoneReceiver()
        {
            var message = new MessageBuilder()
                .ToPhone("+15557654321")
                .Build();

            Assert.NotNull(message.Receiver);
            Assert.Equal(EndpointType.PhoneNumber, message.Receiver.Type);
            Assert.Equal("+15557654321", message.Receiver.Address);
        }

        [Fact]
        public void Should_BuildMessageWithTextContent()
        {
            var message = new MessageBuilder()
                .WithText("Hello, world!")
                .Build();

            Assert.NotNull(message.Content);
            Assert.Equal(MessageContentType.PlainText, message.Content.ContentType);

            if (message.Content is TextContent text)
            {
                Assert.Equal("Hello, world!", text.Text);
            }
        }

        [Fact]
        public void Should_BuildMessageWithHtmlContent()
        {
            var message = new MessageBuilder()
                .WithHtml("<h1>Hello</h1>")
                .Build();

            Assert.NotNull(message.Content);
            Assert.Equal(MessageContentType.Html, message.Content.ContentType);
        }

        [Fact]
        public void Should_BuildMessageWithSubject()
        {
            var message = new MessageBuilder()
                .WithSubject("Test Subject")
                .Build();

            Assert.NotNull(message.Properties);
            Assert.True(message.Properties.ContainsKey(KnownMessageProperties.Subject));
            Assert.Equal("Test Subject", message.Properties[KnownMessageProperties.Subject].Value);
        }

        [Fact]
        public void Should_BuildMessageWithCustomProperty()
        {
            var message = new MessageBuilder()
                .WithProperty("CustomKey", "CustomValue")
                .Build();

            Assert.NotNull(message.Properties);
            Assert.True(message.Properties.ContainsKey("CustomKey"));
            Assert.Equal("CustomValue", message.Properties["CustomKey"].Value);
        }

        [Fact]
        public void Should_BuildMessageWithMultipleProperties()
        {
            var message = new MessageBuilder()
                .WithProperty("Key1", "Value1")
                .WithProperty("Key2", 42)
                .Build();

            Assert.NotNull(message.Properties);
            Assert.Equal(2, message.Properties.Count);
            Assert.Equal("Value1", message.Properties["Key1"].Value);
            Assert.Equal(42, message.Properties["Key2"].Value);
        }

        [Fact]
        public void Should_BuildFullyPopulatedMessage()
        {
            var message = new MessageBuilder()
                .WithId("full-msg")
                .FromEmail("from@example.com")
                .ToPhone("+15557654321")
                .WithText("Full message body")
                .WithSubject("Full Subject")
                .WithProperty("Priority", "High")
                .Build();

            Assert.Equal("full-msg", message.Id);
            Assert.NotNull(message.Sender);
            Assert.Equal(EndpointType.EmailAddress, message.Sender.Type);
            Assert.NotNull(message.Receiver);
            Assert.Equal(EndpointType.PhoneNumber, message.Receiver.Type);
            Assert.NotNull(message.Content);
            Assert.Equal(MessageContentType.PlainText, message.Content.ContentType);
            Assert.NotNull(message.Properties);
            Assert.Equal(2, message.Properties.Count);
        }

        [Fact]
        public void Should_BuildWithCustomEndpoint()
        {
            var endpoint = new Endpoint(EndpointType.Url, "https://example.com/webhook");

            var message = new MessageBuilder()
                .From(endpoint)
                .To(endpoint)
                .Build();

            Assert.NotNull(message.Sender);
            Assert.Equal(EndpointType.Url, message.Sender.Type);
            Assert.Equal("https://example.com/webhook", message.Sender.Address);
            Assert.NotNull(message.Receiver);
            Assert.Equal(message.Sender.Address, message.Receiver.Address);
            Assert.Equal(message.Sender.Type, message.Receiver.Type);
        }

        [Fact]
        public void Should_ChainAllMethods()
        {
            var message = new MessageBuilder()
                .WithId("chain")
                .FromEmail("a@b.com")
                .ToPhone("+123")
                .WithText("body")
                .WithSubject("subj")
                .WithProperty("k", "v")
                .Build();

            Assert.NotNull(message);
        }
    }
}
