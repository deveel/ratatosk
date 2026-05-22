namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "Enum")]
public class MediaTypeTests
{
    [Fact]
    public void Should_HaveCorrectValues_When_MediaTypeIsInvoked()
    {
        // Act
        // Assert
        Assert.Equal(0, (int)MediaType.Image);
        Assert.Equal(1, (int)MediaType.Audio);
        Assert.Equal(2, (int)MediaType.Video);
        Assert.Equal(3, (int)MediaType.Document);
        Assert.Equal(4, (int)MediaType.File);
    }

    [Fact]
    public void Should_AllValuesAreDistinct_When_MediaTypeIsInvoked()
    {
        // Arrange
        var values = Enum.GetValues<MediaType>();

        // Act
        var distinctValues = values.Distinct().ToArray();

        // Assert
        Assert.Equal(values.Length, distinctValues.Length);
    }

    [Theory]
    [InlineData(MediaType.Image)]
    [InlineData(MediaType.Audio)]
    [InlineData(MediaType.Video)]
    [InlineData(MediaType.Document)]
    [InlineData(MediaType.File)]
    public void Should_CanBeUsedInSwitchStatement_When_MediaTypeIsInvoked(MediaType mediaType)
    {
        // Act
        // Assert
        var result = mediaType switch
        {
            MediaType.Image => "Image content",
            MediaType.Audio => "Audio content",
            MediaType.Video => "Video content",
            MediaType.Document => "Document content",
            MediaType.File => "File content",
            _ => "Unknown content"
        };

        Assert.NotEqual("Unknown content", result);
    }

    [Fact]
    public void Should_CanBeConvertedToString_When_MediaTypeIsInvoked()
    {
        // Arrange
        // Act
        var imageString = MediaType.Image.ToString();
        var audioString = MediaType.Audio.ToString();
        var videoString = MediaType.Video.ToString();
        var documentString = MediaType.Document.ToString();
        var fileString = MediaType.File.ToString();

        // Assert
        Assert.Equal("Image", imageString);
        Assert.Equal("Audio", audioString);
        Assert.Equal("Video", videoString);
        Assert.Equal("Document", documentString);
        Assert.Equal("File", fileString);
    }
}

public class MessageStatusTests
{
    [Fact]
    public void Should_HaveCorrectValues_When_MessageStatusIsInvoked()
    {
        // Act
        // Assert
        Assert.Equal(0, (int)MessageStatus.Unknown);
        Assert.Equal(1, (int)MessageStatus.Received);
        Assert.Equal(2, (int)MessageStatus.Queued);
        Assert.Equal(3, (int)MessageStatus.Routed);
        Assert.Equal(4, (int)MessageStatus.RouteFailed);
        Assert.Equal(5, (int)MessageStatus.Sent);
        Assert.Equal(6, (int)MessageStatus.Delivered);
        Assert.Equal(7, (int)MessageStatus.DeliveryFailed);
        Assert.Equal(8, (int)MessageStatus.Read);
        Assert.Equal(9, (int)MessageStatus.Deleted);
    }

    [Fact]
    public void Should_AllValuesAreDistinct_When_MessageStatusIsInvoked()
    {
        // Arrange
        var values = Enum.GetValues<MessageStatus>();

        // Act
        var distinctValues = values.Distinct().ToArray();

        // Assert
        Assert.Equal(values.Length, distinctValues.Length);
    }

    [Theory]
    [InlineData(MessageStatus.Unknown)]
    [InlineData(MessageStatus.Received)]
    [InlineData(MessageStatus.Queued)]
    [InlineData(MessageStatus.Routed)]
    [InlineData(MessageStatus.RouteFailed)]
    [InlineData(MessageStatus.Sent)]
    [InlineData(MessageStatus.Delivered)]
    [InlineData(MessageStatus.DeliveryFailed)]
    [InlineData(MessageStatus.Read)]
    [InlineData(MessageStatus.Deleted)]
    public void Should_CanBeUsedInSwitchStatement_When_MessageStatusIsInvoked(MessageStatus status)
    {
        // Act
        // Assert
        var description = status switch
        {
            MessageStatus.Unknown => "Status is unknown",
            MessageStatus.Received => "Message received",
            MessageStatus.Queued => "Message queued",
            MessageStatus.Routed => "Message routed",
            MessageStatus.RouteFailed => "Routing failed",
            MessageStatus.Sent => "Message sent",
            MessageStatus.Delivered => "Message delivered",
            MessageStatus.DeliveryFailed => "Delivery failed",
            MessageStatus.Read => "Message read",
            MessageStatus.Deleted => "Message deleted",
            _ => "Invalid status"
        };

        Assert.NotEqual("Invalid status", description);
    }

    [Fact]
    public void Should_CanBeConvertedToString_When_MessageStatusIsInvoked()
    {
        // Arrange
        // Act
        var unknownString = MessageStatus.Unknown.ToString();
        var receivedString = MessageStatus.Received.ToString();
        var queuedString = MessageStatus.Queued.ToString();
        var routedString = MessageStatus.Routed.ToString();
        var routeFailedString = MessageStatus.RouteFailed.ToString();
        var sentString = MessageStatus.Sent.ToString();
        var deliveredString = MessageStatus.Delivered.ToString();
        var deliveryFailedString = MessageStatus.DeliveryFailed.ToString();
        var readString = MessageStatus.Read.ToString();
        var deletedString = MessageStatus.Deleted.ToString();

        // Assert
        Assert.Equal("Unknown", unknownString);
        Assert.Equal("Received", receivedString);
        Assert.Equal("Queued", queuedString);
        Assert.Equal("Routed", routedString);
        Assert.Equal("RouteFailed", routeFailedString);
        Assert.Equal("Sent", sentString);
        Assert.Equal("Delivered", deliveredString);
        Assert.Equal("DeliveryFailed", deliveryFailedString);
        Assert.Equal("Read", readString);
        Assert.Equal("Deleted", deletedString);
    }

    [Fact]
    public void Should_LogicalOrder_When_MessageStatusStatusProgression()
    {
        // This test verifies that the status values follow a logical progression
        // Arrange
        // Act
        Assert.True((int)MessageStatus.Received < (int)MessageStatus.Queued);
        Assert.True((int)MessageStatus.Queued < (int)MessageStatus.Routed);
        Assert.True((int)MessageStatus.Routed < (int)MessageStatus.Sent);
        Assert.True((int)MessageStatus.Sent < (int)MessageStatus.Delivered);
        Assert.True((int)MessageStatus.Delivered < (int)MessageStatus.Read);
        Assert.True((int)MessageStatus.Read < (int)MessageStatus.Deleted);
    }

    [Fact]
    public void Should_BeDistinctFromSuccessStatuses_When_MessageStatusFailureStatuses()
    {
        // Arrange
        var failureStatuses = new[] { MessageStatus.RouteFailed, MessageStatus.DeliveryFailed };
        var successStatuses = new[] 
        { 
            MessageStatus.Received, MessageStatus.Queued, MessageStatus.Routed,
            MessageStatus.Sent, MessageStatus.Delivered, MessageStatus.Read
        };

        // Act
        // Assert
        foreach (var failureStatus in failureStatuses)
        {
            Assert.DoesNotContain(failureStatus, successStatuses);
        }
    }
}

public class MessageContentTypeTests
{
    [Fact]
    public void Should_HaveCorrectValues_When_MessageContentTypeIsInvoked()
    {
        // Act
        // Assert
        Assert.Equal(1, (int)MessageContentType.PlainText);
        Assert.Equal(2, (int)MessageContentType.Html);
        Assert.Equal(3, (int)MessageContentType.Multipart);
        Assert.Equal(4, (int)MessageContentType.Template);
        Assert.Equal(5, (int)MessageContentType.Media);
        Assert.Equal(6, (int)MessageContentType.Json);
        Assert.Equal(7, (int)MessageContentType.Binary);
        Assert.Equal(9, (int)MessageContentType.Button);
        Assert.Equal(10, (int)MessageContentType.QuickReply);
        Assert.Equal(11, (int)MessageContentType.Carousel);
        Assert.Equal(12, (int)MessageContentType.ListPicker);
    }

    [Fact]
    public void Should_AllValuesAreDistinct_When_MessageContentTypeIsInvoked()
    {
        // Arrange
        var values = Enum.GetValues<MessageContentType>();

        // Act
        var distinctValues = values.Distinct().ToArray();

        // Assert
        Assert.Equal(values.Length, distinctValues.Length);
    }

    [Theory]
    [InlineData(MessageContentType.PlainText)]
    [InlineData(MessageContentType.Html)]
    [InlineData(MessageContentType.Multipart)]
    [InlineData(MessageContentType.Template)]
    [InlineData(MessageContentType.Media)]
    [InlineData(MessageContentType.Json)]
    [InlineData(MessageContentType.Binary)]
    [InlineData(MessageContentType.Button)]
    [InlineData(MessageContentType.QuickReply)]
    public void Should_CanBeUsedInSwitchStatement_When_MessageContentTypeIsInvoked(MessageContentType contentType)
    {
        // Act
        // Assert
        var description = contentType switch
        {
            MessageContentType.PlainText => "Plain text content",
            MessageContentType.Html => "HTML content",
            MessageContentType.Multipart => "Multipart content",
            MessageContentType.Template => "Template content",
            MessageContentType.Media => "Media content",
            MessageContentType.Json => "JSON content",
            MessageContentType.Binary => "Binary content",
            MessageContentType.Button => "Button content",
            MessageContentType.QuickReply => "Quick reply content",
            MessageContentType.Carousel => "Carousel content",
            MessageContentType.ListPicker => "List picker content",
            _ => "Unknown content type"
        };

        Assert.NotEqual("Unknown content type", description);
    }

    [Fact]
    public void Should_CanBeConvertedToString_When_MessageContentTypeIsInvoked()
    {
        // Arrange
        // Act
        var plainTextString = MessageContentType.PlainText.ToString();
        var htmlString = MessageContentType.Html.ToString();
        var multipartString = MessageContentType.Multipart.ToString();
        var templateString = MessageContentType.Template.ToString();
        var mediaString = MessageContentType.Media.ToString();
        var jsonString = MessageContentType.Json.ToString();
        var binaryString = MessageContentType.Binary.ToString();
        var buttonString = MessageContentType.Button.ToString();
        var quickReplyString = MessageContentType.QuickReply.ToString();
        var carouselString = MessageContentType.Carousel.ToString();
        var listPickerString = MessageContentType.ListPicker.ToString();

        // Assert
        Assert.Equal("PlainText", plainTextString);
        Assert.Equal("Html", htmlString);
        Assert.Equal("Multipart", multipartString);
        Assert.Equal("Template", templateString);
        Assert.Equal("Media", mediaString);
        Assert.Equal("Json", jsonString);
        Assert.Equal("Binary", binaryString);
        Assert.Equal("Button", buttonString);
        Assert.Equal("QuickReply", quickReplyString);
        Assert.Equal("Carousel", carouselString);
        Assert.Equal("ListPicker", listPickerString);
    }

    [Fact]
    public void Should_StartsFromOne_When_MessageContentTypeNoZeroValue()
    {
        // Arrange
        var values = Enum.GetValues<MessageContentType>();
        var minValue = values.Min();

        // Act
        // Assert
        Assert.Equal(MessageContentType.PlainText, minValue);
        Assert.Equal(1, (int)minValue);
    }
}

public class ButtonTypeTests
{
    [Fact]
    public void Should_HaveCorrectValues_When_ButtonTypeIsInvoked()
    {
        Assert.Equal(0, (int)ButtonType.Url);
        Assert.Equal(1, (int)ButtonType.Postback);
        Assert.Equal(2, (int)ButtonType.PhoneNumber);
    }

    [Fact]
    public void Should_AllValuesAreDistinct_When_ButtonTypeIsInvoked()
    {
        var values = Enum.GetValues<ButtonType>();
        var distinctValues = values.Distinct().ToArray();
        Assert.Equal(values.Length, distinctValues.Length);
    }

    [Theory]
    [InlineData(ButtonType.Url)]
    [InlineData(ButtonType.Postback)]
    [InlineData(ButtonType.PhoneNumber)]
    public void Should_CanBeUsedInSwitchStatement_When_ButtonTypeIsInvoked(ButtonType buttonType)
    {
        var description = buttonType switch
        {
            ButtonType.Url => "URL button",
            ButtonType.Postback => "Postback button",
            ButtonType.PhoneNumber => "Phone number button",
            _ => "Unknown button type"
        };

        Assert.NotEqual("Unknown button type", description);
    }

    [Fact]
    public void Should_CanBeConvertedToString_When_ButtonTypeIsInvoked()
    {
        Assert.Equal("Url", ButtonType.Url.ToString());
        Assert.Equal("Postback", ButtonType.Postback.ToString());
        Assert.Equal("PhoneNumber", ButtonType.PhoneNumber.ToString());
    }
}

public class ListPickerStyleTests
{
    [Fact]
    public void Should_HaveCorrectValues_When_ListPickerStyleIsInvoked()
    {
        Assert.Equal(0, (int)ListPickerStyle.Inlined);
        Assert.Equal(1, (int)ListPickerStyle.Compact);
        Assert.Equal(2, (int)ListPickerStyle.Large);
    }

    [Fact]
    public void Should_AllValuesAreDistinct_When_ListPickerStyleIsInvoked()
    {
        var values = Enum.GetValues<ListPickerStyle>();
        var distinctValues = values.Distinct().ToArray();
        Assert.Equal(values.Length, distinctValues.Length);
    }

    [Theory]
    [InlineData(ListPickerStyle.Inlined)]
    [InlineData(ListPickerStyle.Compact)]
    [InlineData(ListPickerStyle.Large)]
    public void Should_CanBeUsedInSwitchStatement_When_ListPickerStyleIsInvoked(ListPickerStyle style)
    {
        var description = style switch
        {
            ListPickerStyle.Inlined => "Inlined list",
            ListPickerStyle.Compact => "Compact list",
            ListPickerStyle.Large => "Large list",
            _ => "Unknown style"
        };

        Assert.NotEqual("Unknown style", description);
    }

    [Fact]
    public void Should_CanBeConvertedToString_When_ListPickerStyleIsInvoked()
    {
        Assert.Equal("Inlined", ListPickerStyle.Inlined.ToString());
        Assert.Equal("Compact", ListPickerStyle.Compact.ToString());
        Assert.Equal("Large", ListPickerStyle.Large.ToString());
    }
}

