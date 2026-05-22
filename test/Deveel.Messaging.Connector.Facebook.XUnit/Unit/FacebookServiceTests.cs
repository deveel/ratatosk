//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using RestSharp;
using Moq;
using System.Text.Json;

namespace Deveel.Messaging;

/// <summary>
/// Unit tests for the FacebookService class using RestSharp for Facebook Graph API integration.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "FacebookService")]
public class FacebookServiceTests
{
    [Fact]
    public void Should_SetToken_When_InitializeWithValidToken()
    {
        // Arrange
        var service = new FacebookService();

        // Act
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        // Assert
        Assert.True(true); // If we get here, initialization succeeded
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_InitializeWithNullToken()
    {
        // Arrange
        var service = new FacebookService();

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => service.Initialize(null!));
    }

    [Fact]
    public void Should_ThrowArgumentException_When_InitializeWithInvalidToken()
    {
        // Arrange
        var service = new FacebookService();

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => service.Initialize("invalid-token"));
        Assert.Throws<ArgumentException>(() => service.Initialize("short"));
        Assert.Throws<ArgumentException>(() => service.Initialize("token with spaces"));
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_FetchPageAsyncWithoutInitialization()
    {
        // Arrange
        var service = new FacebookService();

        // Act
        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.FetchPageAsync("test-page-id", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_SendMessageAsyncWithoutInitialization()
    {
        // Arrange
        var service = new FacebookService();
        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = "Test" }
        };

        // Act
        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SendMessageAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_SendMessageAsyncWithInvalidRequest()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.SendMessageAsync(null!, TestContext.Current.CancellationToken));

        // Act
        // Assert
        var emptyRecipientRequest = new FacebookMessageRequest
        {
            Recipient = "",
            Message = new FacebookMessage { Text = "Test" }
        };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessageAsync(emptyRecipientRequest, TestContext.Current.CancellationToken));

        // Act
        // Assert
        var nullMessageRequest = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = null!
        };
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.SendMessageAsync(nullMessageRequest, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_SendMessageAsyncWithTooLongText()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var longTextRequest = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = new string('A', 2001) } // Exceeds Facebook's 2000 char limit
        };

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessageAsync(longTextRequest, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_SendMessageAsyncWithTooManyQuickReplies()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage
            {
                Text = "Choose an option:",
                QuickReplies = Enumerable.Range(1, 14).Select(i => new FacebookQuickReply
                {
                    ContentType = "text",
                    Title = $"Option {i}",
                    Payload = $"OPTION_{i}"
                }).ToList()
            }
        };

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessageAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_SendMessageAsyncWithInvalidMessagingType()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = "Test" },
            MessagingType = "INVALID_TYPE"
        };

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessageAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_SendMessageAsyncWithInvalidNotificationType()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = "Test" },
            NotificationType = "INVALID_NOTIFICATION"
        };

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessageAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Should_CreateDefaultClient_When_ConstructorWithoutRestClient()
    {
        // Arrange
        // Act
        var service = new FacebookService();

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Should_UseProvidedClient_When_ConstructorWithRestClient()
    {
        // Arrange
        var restClient = new RestClient("https://graph.facebook.com");

        // Act
        var service = new FacebookService(restClient);

        // Assert
        Assert.NotNull(service);
    }

    #region Message Validation Edge Cases

    [Fact]
    public async Task Should_ThrowArgumentException_When_SendMessageAsyncWithEmptyTextAndNoAttachment()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = "", Attachment = null }
        };

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessageAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_SendMessageAsyncWithNullTextAndNoAttachment()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = null, Attachment = null }
        };

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessageAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_SendMessageAsyncWithWhitespaceOnlyText()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = "   ", Attachment = null }
        };

        // Act
        // Assert
        // The validation method in FacebookService checks for text/attachment content before making API call
        // So whitespace text with no attachment should throw ArgumentException during validation
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessageAsync(request, TestContext.Current.CancellationToken));
    }

    #endregion

    #region Default Values and Edge Cases

    #endregion

    #region BuildMessageContent Tests

    private static object? BuildMessageContent(FacebookMessage message) {
        var method = typeof(FacebookService).GetMethod("BuildMessageContent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return method!.Invoke(null, new object[] { message });
    }

    private static Dictionary<string, object?> AsDict(object? obj) =>
        obj as Dictionary<string, object?> ?? new();

    [Fact]
    public void Should_SetText_When_BuildMessageContentWithTextOnly() {
        var message = new FacebookMessage { Text = "Hello" };

        var content = BuildMessageContent(message);
        var dict = AsDict(content);

        Assert.Contains("text", dict.Keys);
        Assert.Equal("Hello", dict["text"]);
        Assert.DoesNotContain("attachment", dict.Keys);
    }

    [Fact]
    public void Should_SetAttachment_When_BuildMessageContentWithAttachment() {
        var message = new FacebookMessage {
            Text = "See this:",
            Attachment = new FacebookAttachment {
                Type = "image",
                Payload = new FacebookPayload {
                    Url = "https://example.com/img.png",
                    IsReusable = true
                }
            }
        };

        var content = BuildMessageContent(message);
        var dict = AsDict(content);

        Assert.Contains("attachment", dict.Keys);
        var attachment = (dynamic)dict["attachment"]!;
        Assert.Equal("image", attachment.type);
    }

    [Fact]
    public void Should_SetTemplate_When_BuildMessageContentWithTemplate() {
        var message = new FacebookMessage {
            Text = "Pick one:",
            Template = new FacebookTemplate {
                Payload = new Dictionary<string, object> {
                    ["template_type"] = "button",
                    ["text"] = "Tap",
                    ["buttons"] = new[] { new Dictionary<string, object> { ["type"] = "postback", ["title"] = "A", ["payload"] = "A" } }
                }
            }
        };

        var content = BuildMessageContent(message);
        var dict = AsDict(content);

        Assert.Contains("attachment", dict.Keys);
        var attachment = (dynamic)dict["attachment"]!;
        Assert.Equal("template", attachment.type);
    }

    [Fact]
    public void Should_PreferAttachmentOverTemplate_When_BothSet() {
        var message = new FacebookMessage {
            Text = "Hi",
            Attachment = new FacebookAttachment {
                Type = "image",
                Payload = new FacebookPayload {
                    Url = "https://example.com/img.png",
                    IsReusable = false
                }
            },
            Template = new FacebookTemplate {
                Payload = new Dictionary<string, object> {
                    ["template_type"] = "button",
                    ["text"] = "Tap",
                    ["buttons"] = Array.Empty<object>()
                }
            }
        };

        var content = BuildMessageContent(message);
        var dict = AsDict(content);
        var attachment = (dynamic)dict["attachment"]!;

        // Should be the media attachment, not the template
        Assert.Equal("image", attachment.type);
    }

    [Fact]
    public void Should_NotContainAttachment_When_NeitherSet() {
        var message = new FacebookMessage {
            Text = "Just text"
        };

        var content = BuildMessageContent(message);
        var dict = AsDict(content);

        Assert.DoesNotContain("attachment", dict.Keys);
    }

    #endregion

    #region Argument Validation Edge Cases

    [Fact]
    public async Task Should_ThrowArgumentException_When_FetchPageAsyncWithNullPageId()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.FetchPageAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_FetchPageAsyncWithEmptyPageId()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.FetchPageAsync("", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_FetchPageAsyncWithWhitespacePageId()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.FetchPageAsync("   ", TestContext.Current.CancellationToken));
    }

    #endregion
}
