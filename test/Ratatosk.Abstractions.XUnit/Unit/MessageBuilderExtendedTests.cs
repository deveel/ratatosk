namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "MessageBuilder")]
public class MessageBuilderExtendedTests
{
    [Fact]
    public void Should_Throw_When_FromIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().From(null!));
    }

    [Fact]
    public void Should_Throw_When_ToIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().To(null!));
    }

    [Fact]
    public void Should_Throw_When_WithContentNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().WithContent(null!));
    }

    [Fact]
    public void Should_Throw_When_WithPropertiesNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().WithProperties((IDictionary<string, MessageProperty>)null!));
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().WithProperties((IDictionary<string, object>)null!));
    }

    [Fact]
    public void Should_Throw_When_WithCarouselConfigureNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().WithCarousel((Action<CarouselBuilder>)null!));
    }

    [Fact]
    public void Should_Throw_When_WithListPickerConfigureNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().WithListPicker((Action<ListPickerBuilder>)null!));
    }

    [Fact]
    public void Should_Throw_When_WithQuickReplyConfigureNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().WithQuickReply((Action<QuickReplyBuilder>)null!));
    }

    [Fact]
    public void Should_BuildWithAllProperties()
    {
        var msg = new MessageBuilder()
            .WithId("test-1")
            .FromEmail("sender@test.com")
            .ToPhone("+1234567890")
            .WithSubject("Test Subject")
            .WithProperty("CustomKey", "CustomValue")
            .WithProperties(new Dictionary<string, object> { { "Extra", "ExtraValue" } })
            .WithText("Hello World")
            .Build();

        Assert.Equal("test-1", msg.Id);
        Assert.NotNull(msg.Sender);
        Assert.NotNull(msg.Receiver);
        Assert.NotNull(msg.Content);
        Assert.NotNull(msg.Properties);
    }

    [Fact]
    public void Should_BuildWithNoSender_When_NotProvided()
    {
        var msg = new MessageBuilder()
            .ToPhone("+1234567890")
            .WithText("Hello")
            .Build();

        Assert.Null(msg.Sender);
    }

    [Fact]
    public void Should_BuildWithNoReceiver_When_NotProvided()
    {
        var msg = new MessageBuilder()
            .FromEmail("sender@test.com")
            .WithText("Hello")
            .Build();

        Assert.Null(msg.Receiver);
    }

    [Fact]
    public void Should_Throw_When_HtmlHasNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageBuilder().WithHtml(null!, null));
    }

    [Fact]
    public void Should_BuildWithHtmlConfig()
    {
        var msg = new MessageBuilder()
            .WithHtml("<cGF5bG9hZDogaGVsbG8=", null)
            .Build();

        Assert.NotNull(msg.Content);
        Assert.IsType<HtmlContent>(msg.Content);
    }

    [Fact]
    public void Should_BuildWithButton()
    {
        var msg = new MessageBuilder()
            .WithButton("Click", ButtonType.Url, "https://example.com")
            .Build();

        Assert.NotNull(msg.Content);
        Assert.IsType<ButtonContent>(msg.Content);
    }

    [Fact]
    public void Should_BuildWithQuickReply()
    {
        var msg = new MessageBuilder()
            .WithQuickReply("Option 1", "payload1")
            .Build();

        Assert.NotNull(msg.Content);
        Assert.IsType<QuickReplyContent>(msg.Content);
    }

    [Fact]
    public void Should_BuildWithQuickReplyConfigure()
    {
        var msg = new MessageBuilder()
            .WithQuickReply(b => { b.Title = "Yes"; b.Payload = "yes_payload"; })
            .Build();

        Assert.NotNull(msg.Content);
    }

    [Fact]
    public void Should_BuildWithCarouselCards()
    {
        var cards = new[] {
            new CarouselCard("Title1", "Subtitle1"),
            new CarouselCard("Title2", "Subtitle2")
        };
        var msg = new MessageBuilder()
            .WithCarousel(cards)
            .Build();

        Assert.NotNull(msg.Content);
        Assert.IsType<CarouselContent>(msg.Content);
    }

    [Fact]
    public void Should_BuildWithCarouselConfigure()
    {
        var msg = new MessageBuilder()
            .WithCarousel(b => b.AddCard(c => { c.Title = "Card 1"; c.Subtitle = "Sub 1"; }))
            .Build();

        Assert.NotNull(msg.Content);
    }

    [Fact]
    public void Should_BuildWithListPickerConfigure()
    {
        var msg = new MessageBuilder()
            .WithListPicker(b => { b.Title = "Pick one"; b.AddItem("Option A"); })
            .Build();

        Assert.NotNull(msg.Content);
    }

    [Fact]
    public void Should_BuildWithRemoteId()
    {
        var msg = new MessageBuilder()
            .WithRemoteId("remote-123")
            .Build();

        Assert.NotNull(msg.Properties);
        Assert.True(msg.Properties.ContainsKey(KnownMessageProperties.RemoteMessageId));
    }

    [Fact]
    public void Should_BuildWithReplyTo()
    {
        var msg = new MessageBuilder()
            .WithReplyTo("original-msg-123")
            .Build();

        Assert.NotNull(msg.Properties);
        Assert.True(msg.Properties.ContainsKey(KnownMessageProperties.ReplyTo));
    }

    [Fact]
    public void Should_FromPhone()
    {
        var msg = new MessageBuilder()
            .FromPhone("+1234567890")
            .Build();

        Assert.NotNull(msg.Sender);
        Assert.Equal(EndpointType.PhoneNumber, msg.Sender.Type);
    }

    [Fact]
    public void Should_ToEmail()
    {
        var msg = new MessageBuilder()
            .ToEmail("test@example.com")
            .Build();

        Assert.NotNull(msg.Receiver);
        Assert.Equal(EndpointType.EmailAddress, msg.Receiver.Type);
    }

    [Fact]
    public void Should_PreserveSenderInterface()
    {
        var sender = new PhoneSender("+123");
        var msg = new MessageBuilder()
            .From(sender)
            .Build();

        Assert.Same(sender, msg.Sender);
    }

    [Fact]
    public void Should_WrapNonSenderEndpoint()
    {
        var endpoint = new Endpoint(EndpointType.PhoneNumber, "+123");
        var msg = new MessageBuilder()
            .From(endpoint)
            .Build();

        Assert.NotNull(msg.Sender);
        Assert.NotSame(endpoint, msg.Sender);
        Assert.Equal(endpoint.Address, msg.Sender.Address);
    }

    [Fact]
    public void Should_BuildWithProperties_FromMessagePropertyDict()
    {
        var props = new Dictionary<string, MessageProperty>
        {
            ["Key1"] = new MessageProperty("Key1", "Value1")
        };

        var msg = new MessageBuilder()
            .WithProperties(props)
            .Build();

        Assert.NotNull(msg.Properties);
        Assert.Equal("Value1", msg.Properties["Key1"].Value);
    }

    [Fact]
    public void Should_BuildWithProperties_FromObjectDict()
    {
        var props = new Dictionary<string, object>
        {
            ["Key1"] = "Value1"
        };

        var msg = new MessageBuilder()
            .WithProperties(props)
            .Build();

        Assert.NotNull(msg.Properties);
        Assert.Equal("Value1", msg.Properties["Key1"].Value);
    }

    [Fact]
    public void Should_MergeProperties_When_AddingAfterWithProperties()
    {
        var msg = new MessageBuilder()
            .WithProperties(new Dictionary<string, object> { { "A", "1" } })
            .WithProperty("B", "2")
            .Build();

        Assert.NotNull(msg.Properties);
        Assert.Equal(2, msg.Properties.Count);
    }
}
