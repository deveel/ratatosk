//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Deveel.Messaging.Testing;

namespace Deveel.Messaging;

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

    [Fact]
    public void Should_MapButtonContent_When_BuildFacebookMessage()
    {
        var content = InteractiveContentBuilder.CreateButton();
        var message = CreateMessage(content);

        var fbMessage = FacebookMessageBuilder.BuildFacebookMessage(message);

        FacebookInteractiveMappingAssertions.AssertMapsToButton(content, fbMessage);
    }

    [Fact]
    public void Should_MapQuickReplyContent_When_BuildFacebookMessage()
    {
        var content = InteractiveContentBuilder.CreateQuickReply();
        var message = CreateMessage(content);

        var fbMessage = FacebookMessageBuilder.BuildFacebookMessage(message);

        Assert.NotNull(fbMessage.QuickReplies);
        var qr = Assert.Single(fbMessage.QuickReplies);
        FacebookInteractiveMappingAssertions.AssertMapsToQuickReply(content, qr);
    }

    [Fact]
    public void Should_MapCarouselContent_When_BuildFacebookMessage()
    {
        var content = InteractiveContentBuilder.CreateCarousel(3);
        var message = CreateMessage(content);

        var fbMessage = FacebookMessageBuilder.BuildFacebookMessage(message);

        FacebookInteractiveMappingAssertions.AssertMapsToCarousel(content, fbMessage);
    }

    [Fact]
    public void Should_MapListPickerContent_When_BuildFacebookMessage()
    {
        var content = InteractiveContentBuilder.CreateListPicker(3);
        var message = CreateMessage(content);

        var fbMessage = FacebookMessageBuilder.BuildFacebookMessage(message);

        FacebookInteractiveMappingAssertions.AssertMapsToListPicker(content, fbMessage);
    }
}
