//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Moq;

using System.Text.Json;

namespace Deveel.Messaging;

/// <summary>
/// Integration tests for the FacebookMessengerConnector class that test complete workflows
/// including webhook message receiving, error scenarios, and performance characteristics.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "FacebookMessengerConnector")]
public class FacebookMessengerConnectorIntegrationTests
{
    [Fact]
    public async Task Should_WorksEndToEnd_When_FacebookMessengerFullLifecycle()
    {
        // Arrange
        var mockFacebookService = new Mock<IFacebookService>();
        var pageInfo = new FacebookPageInfo { Id = "test-page-id", Name = "Test Page", Category = "Business" };
        var messageResponse = new FacebookMessageResponse { MessageId = "fb-msg-123", RecipientId = "user-456" };

        mockFacebookService.Setup(x => x.FetchPageAsync("test-page-id", It.IsAny<CancellationToken>()))
                          .ReturnsAsync(pageInfo);
        mockFacebookService.Setup(x => x.SendMessageAsync(It.IsAny<FacebookMessageRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(messageResponse);

        var connectionSettings = new ConnectionSettings()
            .SetParameter("PageAccessToken", "test-access-token")
            .SetParameter("PageId", "test-page-id")
            .SetParameter("WebhookUrl", "https://example.com/webhook")
            .SetParameter("VerifyToken", "verify-token");

        var connector = new FacebookMessengerConnector(connectionSettings, mockFacebookService.Object);

        // Act
        // Assert

        // 1. Initialize
        var initResult = await connector.InitializeAsync(TestContext.Current.CancellationToken);
        Assert.True(initResult.Successful);
        Assert.Equal(ConnectorState.Ready, connector.State);

        // 2. Test connection
        var connectionResult = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);
        Assert.True(connectionResult.Successful);

        // 3. Get status
        var statusResult = await connector.GetStatusAsync(TestContext.Current.CancellationToken);
        Assert.True(statusResult.Successful);
        Assert.Contains("Facebook Messenger Connector", statusResult.Value!.Description);

        // 4. Get health
        var healthResult = await connector.GetHealthAsync(TestContext.Current.CancellationToken);
        Assert.True(healthResult.Successful);
        Assert.True(healthResult.Value!.IsHealthy);

        // 5. Send message
        var message = new Message
        {
            Id = "test-msg-1",
            Receiver = new Endpoint(EndpointType.UserId, "user-456"),
            Content = new TextContent("Hello from integration test!")
        };

        var sendResult = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);
        Assert.True(sendResult.Successful);
        Assert.Equal("test-msg-1", sendResult.Value!.MessageId);
        Assert.Equal("fb-msg-123", sendResult.Value.RemoteMessageId);

        // 6. Shutdown
        await connector.ShutdownAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ConnectorState.Shutdown, connector.State);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_ReceiveMessagesFromFacebookWebhook()
    {
        // Arrange
        var mockFacebookService = new Mock<IFacebookService>();
        var connectionSettings = new ConnectionSettings()
            .SetParameter("PageAccessToken", "test-access-token")
            .SetParameter("PageId", "test-page-id");

        var connector = new FacebookMessengerConnector(connectionSettings, mockFacebookService.Object);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create Facebook webhook JSON payload
        var webhookPayload = new
        {
            @object = "page",
            entry = new[]
            {
                new
                {
                    id = "test-page-id",
                    time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    messaging = new[]
                    {
                        new
                        {
                            sender = new { id = "user-123" },
                            recipient = new { id = "test-page-id" },
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            message = new
                            {
                                mid = "fb-msg-456",
                                text = "Hello from user!"
                            }
                        }
                    }
                }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(webhookPayload);
        var messageSource = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(messageSource, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Successful);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);

        var receivedMessage = result.Value.Messages.First();
        Assert.Equal("fb-msg-456", receivedMessage.Id);
        Assert.Equal(EndpointType.UserId, receivedMessage.Sender!.Type);
        Assert.Equal("user-123", receivedMessage.Sender.Address);
        Assert.Equal(EndpointType.UserId, receivedMessage.Receiver!.Type);
        Assert.Equal("test-page-id", receivedMessage.Receiver.Address);
        Assert.Equal(MessageContentType.PlainText, receivedMessage.Content!.ContentType);
        Assert.Equal("Hello from user!", ((ITextContent)receivedMessage.Content).Text);
    }

    [Fact]
    public async Task Should_ParseCorrectly_When_ReceiveMessagesWithMediaAttachment()
    {
        // Arrange
        var mockFacebookService = new Mock<IFacebookService>();
        var connectionSettings = new ConnectionSettings()
            .SetParameter("PageAccessToken", "test-access-token")
            .SetParameter("PageId", "test-page-id");

        var connector = new FacebookMessengerConnector(connectionSettings, mockFacebookService.Object);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create Facebook webhook JSON payload with media attachment
        var webhookPayload = new
        {
            @object = "page",
            entry = new[]
            {
                new
                {
                    id = "test-page-id",
                    time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    messaging = new[]
                    {
                        new
                        {
                            sender = new { id = "user-789" },
                            recipient = new { id = "test-page-id" },
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            message = new
                            {
                                mid = "fb-msg-media-123",
                                attachments = new[]
                                {
                                    new
                                    {
                                        type = "image",
                                        payload = new
                                        {
                                            url = "https://example.com/image.jpg"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(webhookPayload);
        var messageSource = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(messageSource, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Successful);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Messages);

        var receivedMessage = result.Value.Messages.First();
        Assert.Equal("fb-msg-media-123", receivedMessage.Id);
        Assert.Equal("user-789", receivedMessage.Sender!.Address);
        Assert.Equal(MessageContentType.Media, receivedMessage.Content!.ContentType);

        var mediaContent = (IMediaContent)receivedMessage.Content;
        Assert.Equal("https://example.com/image.jpg", mediaContent.FileUrl);
        Assert.Equal(MediaType.Image, mediaContent.MediaType);
    }

    [Fact]
    public async Task Should_ReturnError_When_ReceiveMessagesInvalidWebhookData()
    {
        // Arrange
        var mockFacebookService = new Mock<IFacebookService>();
        var connectionSettings = new ConnectionSettings()
            .SetParameter("PageAccessToken", "test-access-token")
            .SetParameter("PageId", "test-page-id");

        var connector = new FacebookMessengerConnector(connectionSettings, mockFacebookService.Object);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create invalid webhook payload (missing required fields)
        var invalidPayload = new { invalid = "data" };
        var jsonPayload = JsonSerializer.Serialize(invalidPayload);
        var messageSource = MessageSource.Json(jsonPayload);

        // Act
        var result = await connector.ReceiveMessagesAsync(messageSource, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.Successful);
        Assert.Equal(FacebookErrorCodes.InvalidWebhookData, result.Error!.ErrorCode);
    }

    [Fact]
    public async Task Should_ReturnError_When_ReceiveMessagesNonJsonContentType()
    {
        // Arrange
        var mockFacebookService = new Mock<IFacebookService>();
        var connectionSettings = new ConnectionSettings()
            .SetParameter("PageAccessToken", "test-access-token")
            .SetParameter("PageId", "test-page-id");

        var connector = new FacebookMessengerConnector(connectionSettings, mockFacebookService.Object);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var messageSource = MessageSource.UrlPost("invalid=form&data=true");

        // Act
        var result = await connector.ReceiveMessagesAsync(messageSource, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.Successful);
        Assert.Equal(FacebookErrorCodes.UnsupportedContentType, result.Error!.ErrorCode);
    }

    [Fact]
    public async Task Should_SendCorrectRequest_When_SendMessageWithQuickReplies()
    {
        // Arrange
        var mockFacebookService = new Mock<IFacebookService>();
        var messageResponse = new FacebookMessageResponse { MessageId = "fb-msg-quick-123", RecipientId = "user-999" };

        FacebookMessageRequest? capturedRequest = null;
        mockFacebookService.Setup(x => x.SendMessageAsync(It.IsAny<FacebookMessageRequest>(), It.IsAny<CancellationToken>()))
                          .Callback<FacebookMessageRequest, CancellationToken>((req, ct) => capturedRequest = req)
                          .ReturnsAsync(messageResponse);

        var connectionSettings = new ConnectionSettings()
            .SetParameter("PageAccessToken", "test-access-token")
            .SetParameter("PageId", "test-page-id");

        var connector = new FacebookMessengerConnector(connectionSettings, mockFacebookService.Object);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var quickRepliesJson = JsonSerializer.Serialize(new[]
        {
            new { content_type = "text", title = "Yes", payload = "YES_PAYLOAD" },
            new { content_type = "text", title = "No", payload = "NO_PAYLOAD" }
        });

        var message = new Message
        {
            Id = "test-msg-quick",
            Receiver = new Endpoint(EndpointType.UserId, "user-999"),
            Content = new TextContent("Do you agree?"),
            Properties = new Dictionary<string, MessageProperty>
            {
                { "QuickReplies", new MessageProperty("QuickReplies", quickRepliesJson) },
                { "NotificationType", new MessageProperty("NotificationType", "SILENT_PUSH") }
            }
        };

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Successful);
        Assert.NotNull(capturedRequest);
        Assert.Equal("user-999", capturedRequest.Recipient);
        Assert.Equal("Do you agree?", capturedRequest.Message.Text);
        Assert.Equal("SILENT_PUSH", capturedRequest.NotificationType);
        Assert.NotNull(capturedRequest.Message.QuickReplies);
        Assert.Equal(2, capturedRequest.Message.QuickReplies.Count);
        Assert.Equal("Yes", capturedRequest.Message.QuickReplies[0].Title);
        Assert.Equal("YES_PAYLOAD", capturedRequest.Message.QuickReplies[0].Payload);
    }

    [Fact]
    public async Task Should_ReturnValidationError_When_ValidateMessageWithVeryLongText()
    {
        // Arrange
        var mockFacebookService = new Mock<IFacebookService>();
        var connectionSettings = new ConnectionSettings()
            .SetParameter("PageAccessToken", "test-access-token")
            .SetParameter("PageId", "test-page-id");

        var connector = new FacebookMessengerConnector(connectionSettings, mockFacebookService.Object);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Create message with text exceeding Facebook's limit (2000 characters)
        var longText = new string('A', 2001);
        var message = new Message
        {
            Id = "test-msg-long",
            Receiver = new Endpoint(EndpointType.UserId, "user-123"),
            Content = new TextContent(longText)
        };

        // Act
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        await foreach (var result in connector.ValidateMessageAsync(message, TestContext.Current.CancellationToken))
        {
            if (result != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                validationResults.Add(result);
        }

        // Assert
        Assert.NotEmpty(validationResults);
    }

    [Fact]
    public async Task Should_ReturnUnhealthyStatus_When_HealthCheckWhenConnectionFails()
    {
        // Arrange
        var mockFacebookService = new Mock<IFacebookService>();
        mockFacebookService.Setup(x => x.FetchPageAsync("test-page-id", It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new HttpRequestException("Network error"));

        var connectionSettings = new ConnectionSettings()
            .SetParameter("PageAccessToken", "test-access-token")
            .SetParameter("PageId", "test-page-id");

        var connector = new FacebookMessengerConnector(connectionSettings, mockFacebookService.Object);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var healthResult = await connector.GetHealthAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(healthResult.Successful); // Health check itself should succeed
        Assert.NotNull(healthResult.Value);
        Assert.False(healthResult.Value.IsHealthy); // But connector should be unhealthy
        Assert.Single(healthResult.Value.Issues);
        Assert.Contains("Network error", healthResult.Value.Issues.First());
    }

    [Fact]
    public async Task Should_HandleCorrectly_When_ConcurrentMessageSendingHighThroughput()
    {
        // Arrange
        var mockFacebookService = new Mock<IFacebookService>();
        var messageCounter = 0;

        mockFacebookService.Setup(x => x.SendMessageAsync(It.IsAny<FacebookMessageRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(() => new FacebookMessageResponse
                          {
                              MessageId = $"fb-msg-{Interlocked.Increment(ref messageCounter)}",
                              RecipientId = "user-123"
                          });

        var connectionSettings = new ConnectionSettings()
            .SetParameter("PageAccessToken", "test-access-token")
            .SetParameter("PageId", "test-page-id");

        var connector = new FacebookMessengerConnector(connectionSettings, mockFacebookService.Object);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var messageCount = 50;
        var semaphore = new SemaphoreSlim(10); // Limit concurrency to simulate real-world conditions

        // Act
        var tasks = Enumerable.Range(1, messageCount).Select(async i =>
        {
            await semaphore.WaitAsync(TestContext.Current.CancellationToken);
            try
            {
                var message = new Message
                {
                    Id = $"test-msg-{i}",
                    Receiver = new Endpoint(EndpointType.UserId, "user-123"),
                    Content = new TextContent($"Concurrent message {i}")
                };

                return await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result => Assert.True(result.Successful));
        Assert.Equal(messageCount, results.Length);

        // Verify all message IDs are unique
        var messageIds = results.Select(r => r.Value!.RemoteMessageId).ToHashSet();
        Assert.Equal(messageCount, messageIds.Count);

        // Verify service was called the expected number of times
        mockFacebookService.Verify(x => x.SendMessageAsync(It.IsAny<FacebookMessageRequest>(), It.IsAny<CancellationToken>()),
                                  Times.Exactly(messageCount));
    }

    [Fact]
    public async Task Should_ReturnAppropriateError_When_ErrorHandlingServiceThrowsException()
    {
        // Arrange
        var mockFacebookService = new Mock<IFacebookService>();
        mockFacebookService.Setup(x => x.SendMessageAsync(It.IsAny<FacebookMessageRequest>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new HttpRequestException("Facebook Graph API error: 403 - Forbidden"));

        var connectionSettings = new ConnectionSettings()
            .SetParameter("PageAccessToken", "test-access-token")
            .SetParameter("PageId", "test-page-id");

        var connector = new FacebookMessengerConnector(connectionSettings, mockFacebookService.Object);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var message = new Message
        {
            Id = "test-msg-error",
            Receiver = new Endpoint(EndpointType.UserId, "user-123"),
            Content = new TextContent("This will fail")
        };

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.Successful);
        Assert.Equal(ConnectorErrorCodes.SendMessageError, result.Error!.ErrorCode);
        Assert.Contains("Facebook Graph API error", result.Error.ErrorMessage);
    }
}
