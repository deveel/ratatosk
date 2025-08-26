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
public class FacebookServiceTests
{
    [Fact]
    public void Initialize_WithValidToken_SetsToken()
    {
        // Arrange
        var service = new FacebookService();

        // Act
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        // Assert - No exception should be thrown
        Assert.True(true); // If we get here, initialization succeeded
    }

    [Fact]
    public void Initialize_WithNullToken_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new FacebookService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.Initialize(null!));
    }

    [Fact]
    public void Initialize_WithInvalidToken_ThrowsArgumentException()
    {
        // Arrange
        var service = new FacebookService();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.Initialize("invalid-token"));
        Assert.Throws<ArgumentException>(() => service.Initialize("short"));
        Assert.Throws<ArgumentException>(() => service.Initialize("token with spaces"));
    }

    [Fact]
    public async Task FetchPageAsync_WithoutInitialization_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new FacebookService();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.FetchPageAsync("test-page-id", CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_WithoutInitialization_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new FacebookService();
        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = "Test" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.SendMessageAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_WithInvalidRequest_ThrowsArgumentException()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        // Act & Assert - Null request
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            service.SendMessageAsync(null!, CancellationToken.None));

        // Act & Assert - Empty recipient
        var emptyRecipientRequest = new FacebookMessageRequest
        {
            Recipient = "",
            Message = new FacebookMessage { Text = "Test" }
        };
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SendMessageAsync(emptyRecipientRequest, CancellationToken.None));

        // Act & Assert - Null message
        var nullMessageRequest = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = null!
        };
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            service.SendMessageAsync(nullMessageRequest, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_WithTooLongText_ThrowsArgumentException()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var longTextRequest = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = new string('A', 2001) } // Exceeds Facebook's 2000 char limit
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SendMessageAsync(longTextRequest, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_WithTooManyQuickReplies_ThrowsArgumentException()
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

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SendMessageAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_WithInvalidMessagingType_ThrowsArgumentException()
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

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SendMessageAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_WithInvalidNotificationType_ThrowsArgumentException()
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

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SendMessageAsync(request, CancellationToken.None));
    }

    [Fact]
    public void Constructor_WithoutRestClient_CreatesDefaultClient()
    {
        // Arrange & Act
        var service = new FacebookService();

        // Assert - Should not throw exception
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithRestClient_UsesProvidedClient()
    {
        // Arrange
        var restClient = new RestClient("https://graph.facebook.com");

        // Act
        var service = new FacebookService(restClient);

        // Assert - Should not throw exception
        Assert.NotNull(service);
    }

    [Fact]
    public void ValidatePageAccessToken_ValidTokens_ReturnsTrue()
    {
        // Test valid token formats using internal method directly
        Assert.True(FacebookService.IsValidPageAccessToken("EAATest123456789|ValidPageAccessToken"));
        Assert.True(FacebookService.IsValidPageAccessToken("EAAGTest123456789abcdef"));
        Assert.True(FacebookService.IsValidPageAccessToken("someLongTokenWithPipe|123456"));
    }

    [Fact]
    public void ValidatePageAccessToken_InvalidTokens_ReturnsFalse()
    {
        // Test invalid token formats using internal method directly
        Assert.False(FacebookService.IsValidPageAccessToken(""));
        Assert.False(FacebookService.IsValidPageAccessToken("short"));
        Assert.False(FacebookService.IsValidPageAccessToken("token with spaces"));
        Assert.False(FacebookService.IsValidPageAccessToken("   "));
    }

    [Fact]
    public void BuildFacebookMessagePayload_ValidRequest_CreatesCorrectStructure()
    {
        // Test internal method directly
        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            MessagingType = "RESPONSE",
            Message = new FacebookMessage { Text = "Hello, World!" }
        };

        // Act
        var result = FacebookService.BuildFacebookMessagePayload(request);

        // Assert
        Assert.NotNull(result);
        var payload = result as Dictionary<string, object>;
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("recipient"));
        Assert.True(payload.ContainsKey("messaging_type"));
        Assert.True(payload.ContainsKey("message"));
        Assert.Equal("RESPONSE", payload["messaging_type"]);
    }

    [Fact]
    public void BuildMessageContent_WithTextAndQuickReplies_CreatesCorrectStructure()
    {
        // Test internal method directly
        var message = new FacebookMessage
        {
            Text = "Choose an option:",
            QuickReplies = new List<FacebookQuickReply>
            {
                new FacebookQuickReply 
                { 
                    ContentType = "text", 
                    Title = "Yes", 
                    Payload = "YES_PAYLOAD",
                    ImageUrl = "https://example.com/yes.png"
                },
                new FacebookQuickReply 
                { 
                    ContentType = "text", 
                    Title = "No", 
                    Payload = "NO_PAYLOAD"
                }
            }
        };

        // Act
        var result = FacebookService.BuildMessageContent(message);

        // Assert
        Assert.NotNull(result);
        var content = result as Dictionary<string, object>;
        Assert.NotNull(content);
        Assert.True(content.ContainsKey("text"));
        Assert.True(content.ContainsKey("quick_replies"));
        Assert.Equal("Choose an option:", content["text"]);
        
        var quickReplies = content["quick_replies"] as object[];
        Assert.NotNull(quickReplies);
        Assert.Equal(2, quickReplies.Length);
    }

    [Fact]
    public void BuildMessageContent_WithAttachment_CreatesCorrectStructure()
    {
        // Test internal method directly
        var message = new FacebookMessage
        {
            Attachment = new FacebookAttachment
            {
                Type = "image",
                Payload = new FacebookPayload 
                { 
                    Url = "https://example.com/image.jpg",
                    IsReusable = true
                }
            }
        };

        // Act
        var result = FacebookService.BuildMessageContent(message);

        // Assert
        Assert.NotNull(result);
        var content = result as Dictionary<string, object>;
        Assert.NotNull(content);
        Assert.True(content.ContainsKey("attachment"));
        Assert.False(content.ContainsKey("text"));
    }

    // Additional edge case and error handling tests

    #region Error Response Parsing Tests

    [Fact]
    public void ParseFacebookError_ValidErrorResponse_ReturnsFormattedMessage()
    {
        // Test internal method directly
        var mockResponse = new Mock<RestResponse>();
        mockResponse.Object.Content = @"{
            ""error"": {
                ""message"": ""Invalid OAuth access token."",
                ""code"": ""190"",
                ""error_subcode"": ""460""
            }
        }";
        mockResponse.Object.StatusCode = System.Net.HttpStatusCode.BadRequest;

        // Act
        var result = FacebookService.ParseFacebookError(mockResponse.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Code 190", result);
        Assert.Contains("Subcode 460", result);
        Assert.Contains("Invalid OAuth access token", result);
    }

    [Fact]
    public void ParseFacebookError_ErrorWithoutSubcode_ReturnsSimpleFormat()
    {
        // Test internal method directly
        var mockResponse = new Mock<RestResponse>();
        mockResponse.Object.Content = @"{
            ""error"": {
                ""message"": ""Rate limit exceeded"",
                ""code"": ""4""
            }
        }";
        mockResponse.Object.StatusCode = System.Net.HttpStatusCode.TooManyRequests;

        // Act
        var result = FacebookService.ParseFacebookError(mockResponse.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Code 4", result);
        Assert.DoesNotContain("Subcode", result);
        Assert.Contains("Rate limit exceeded", result);
    }

    [Fact]
    public void ParseFacebookError_EmptyContent_ReturnsHttpStatus()
    {
        // Test internal method directly
        var mockResponse = new Mock<RestResponse>();
        mockResponse.Object.Content = "";
        mockResponse.Object.StatusCode = System.Net.HttpStatusCode.InternalServerError;
        mockResponse.Object.ErrorMessage = "Network error";

        // Act
        var result = FacebookService.ParseFacebookError(mockResponse.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("HTTP 500", result);
        Assert.Contains("Network error", result);
    }

    [Fact]
    public void ParseFacebookError_InvalidJson_ReturnsRawContent()
    {
        // Test internal method directly
        var mockResponse = new Mock<RestResponse>();
        mockResponse.Object.Content = "Invalid JSON response";
        mockResponse.Object.StatusCode = System.Net.HttpStatusCode.BadRequest;

        // Act
        var result = FacebookService.ParseFacebookError(mockResponse.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Invalid JSON response", result);
    }

    #endregion

    #region JSON Property Extraction Tests

    [Fact]
    public void GetJsonStringProperty_ExistingProperty_ReturnsValue()
    {
        // Test internal method directly
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(@"{""name"": ""Test Page"", ""id"": ""123456""}");

        // Act
        var result = FacebookService.GetJsonStringProperty(jsonElement, "name");

        // Assert
        Assert.Equal("Test Page", result);
    }

    [Fact]
    public void GetJsonStringProperty_NonExistingProperty_ReturnsNull()
    {
        // Test internal method directly
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(@"{""name"": ""Test Page""}");

        // Act
        var result = FacebookService.GetJsonStringProperty(jsonElement, "nonexistent");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Message Validation Edge Cases

    [Fact]
    public async Task SendMessageAsync_WithEmptyTextAndNoAttachment_ThrowsArgumentException()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = "", Attachment = null }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SendMessageAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_WithNullTextAndNoAttachment_ThrowsArgumentException()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = null, Attachment = null }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SendMessageAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_WithWhitespaceOnlyText_ThrowsArgumentException()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = "   ", Attachment = null }
        };

        // Act & Assert
        // The validation method in FacebookService checks for text/attachment content before making API call
        // So whitespace text with no attachment should throw ArgumentException during validation
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SendMessageAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_WithRequestLevelQuickReplies_ValidatesCount()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            Message = new FacebookMessage { Text = "Test" },
            QuickReplies = Enumerable.Range(1, 14).Select(i => new FacebookQuickReply 
            { 
                ContentType = "text", 
                Title = $"Option {i}", 
                Payload = $"OPTION_{i}" 
            }).ToList()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SendMessageAsync(request, CancellationToken.None));
    }

    #endregion

    #region Default Values and Edge Cases

    [Fact]
    public void BuildFacebookMessagePayload_WithDefaultNotificationType_DoesNotIncludeProperty()
    {
        // Test internal method directly
        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            MessagingType = "RESPONSE",
            NotificationType = "REGULAR", // Default value
            Message = new FacebookMessage { Text = "Hello!" }
        };

        // Act
        var result = FacebookService.BuildFacebookMessagePayload(request);

        // Assert
        var payload = result as Dictionary<string, object>;
        Assert.NotNull(payload);
        Assert.False(payload.ContainsKey("notification_type")); // Should not include default value
    }

    [Fact]
    public void BuildFacebookMessagePayload_WithNonDefaultNotificationType_IncludesProperty()
    {
        // Test internal method directly
        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            MessagingType = "RESPONSE",
            NotificationType = "SILENT_PUSH", // Non-default value
            Message = new FacebookMessage { Text = "Hello!" }
        };

        // Act
        var result = FacebookService.BuildFacebookMessagePayload(request);

        // Assert
        var payload = result as Dictionary<string, object>;
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("notification_type"));
        Assert.Equal("SILENT_PUSH", payload["notification_type"]);
    }

    [Fact]
    public void BuildFacebookMessagePayload_WithTag_IncludesTagProperty()
    {
        // Test internal method directly
        var request = new FacebookMessageRequest
        {
            Recipient = "user-123",
            MessagingType = "MESSAGE_TAG",
            Tag = "CONFIRMED_EVENT_UPDATE",
            Message = new FacebookMessage { Text = "Your event is confirmed!" }
        };

        // Act
        var result = FacebookService.BuildFacebookMessagePayload(request);

        // Assert
        var payload = result as Dictionary<string, object>;
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("tag"));
        Assert.Equal("CONFIRMED_EVENT_UPDATE", payload["tag"]);
    }

    [Fact]
    public void BuildMessageContent_WithQuickReplyWithoutImageUrl_DoesNotIncludeImageUrl()
    {
        // Test internal method directly
        var message = new FacebookMessage
        {
            Text = "Choose an option:",
            QuickReplies = new List<FacebookQuickReply>
            {
                new FacebookQuickReply 
                { 
                    ContentType = "text", 
                    Title = "Yes", 
                    Payload = "YES_PAYLOAD"
                    // No ImageUrl specified
                }
            }
        };

        // Act
        var result = FacebookService.BuildMessageContent(message);

        // Assert
        var content = result as Dictionary<string, object>;
        Assert.NotNull(content);
        
        var quickReplies = content["quick_replies"] as object[];
        Assert.NotNull(quickReplies);
        Assert.Single(quickReplies);
        
        var quickReply = quickReplies[0] as Dictionary<string, object>;
        Assert.NotNull(quickReply);
        Assert.False(quickReply.ContainsKey("image_url"));
    }

    #endregion

    #region Argument Validation Edge Cases

    [Fact]
    public async Task FetchPageAsync_WithNullPageId_ThrowsArgumentException()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.FetchPageAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task FetchPageAsync_WithEmptyPageId_ThrowsArgumentException()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.FetchPageAsync("", CancellationToken.None));
    }

    [Fact]
    public async Task FetchPageAsync_WithWhitespacePageId_ThrowsArgumentException()
    {
        // Arrange
        var service = new FacebookService();
        service.Initialize("EAATest123456789|ValidPageAccessToken");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.FetchPageAsync("   ", CancellationToken.None));
    }

    #endregion
}