using Ratatosk.Testing;

namespace Ratatosk;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "FacebookInteractiveMapping")]
public class FacebookInteractiveMappingTests
{
    private static Message CreateMessage(MessageContent content)
    {
        return new Message
        {
            Id = "msg-1",
            Sender = new Endpoint(EndpointType.UserId, "sender-1"),
            Receiver = new Endpoint(EndpointType.UserId, "user-1"),
            Content = content
        };
    }

    private static FacebookMessage BuildFacebookMessage(Message message)
    {
        var type = typeof(FacebookService).Assembly.GetTypes()
            .First(t => t.Name == "FacebookMessageBuilder");
        var method = type.GetMethod("BuildFacebookMessage",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        return (FacebookMessage)method!.Invoke(null, new object?[] { message, null })!;
    }

    [Fact]
    public void Should_MapButtonContent_When_BuildFacebookMessage()
    {
        var content = InteractiveContentBuilder.CreateButton();
        var message = CreateMessage(content);

        var fbMessage = BuildFacebookMessage(message);

        FacebookInteractiveMappingAssertions.AssertMapsToButton(content, fbMessage);
    }

    [Fact]
    public void Should_MapQuickReplyContent_When_BuildFacebookMessage()
    {
        var content = InteractiveContentBuilder.CreateQuickReply();
        var message = CreateMessage(content);

        var fbMessage = BuildFacebookMessage(message);

        Assert.NotNull(fbMessage.QuickReplies);
        var qr = Assert.Single(fbMessage.QuickReplies);
        FacebookInteractiveMappingAssertions.AssertMapsToQuickReply(content, qr);
    }

    [Fact]
    public void Should_MapCarouselContent_When_BuildFacebookMessage()
    {
        var content = InteractiveContentBuilder.CreateCarousel(3);
        var message = CreateMessage(content);

        var fbMessage = BuildFacebookMessage(message);

        FacebookInteractiveMappingAssertions.AssertMapsToCarousel(content, fbMessage);
    }

    [Fact]
    public void Should_MapListPickerContent_When_BuildFacebookMessage()
    {
        var content = InteractiveContentBuilder.CreateListPicker(3);
        var message = CreateMessage(content);

        var fbMessage = BuildFacebookMessage(message);

        FacebookInteractiveMappingAssertions.AssertMapsToListPicker(content, fbMessage);
    }
}
