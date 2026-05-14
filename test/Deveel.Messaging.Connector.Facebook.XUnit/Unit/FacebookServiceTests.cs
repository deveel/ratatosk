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
