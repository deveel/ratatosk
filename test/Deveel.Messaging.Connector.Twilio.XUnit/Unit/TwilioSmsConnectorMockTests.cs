using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using Twilio.Rest.Api.V2010;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Deveel.Messaging;

/// <summary>
/// Tests for the <see cref="TwilioSmsConnector"/> class using mocked Twilio services
/// to verify send functionalities without requiring actual Twilio API calls.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioSmsConnectorMock")]
public class TwilioSmsConnectorMockTests
{
    private readonly Mock<ITwilioService> _mockTwilioService;
    private readonly Mock<ILogger<TwilioSmsConnector>> _mockLogger;

    public TwilioSmsConnectorMockTests()
    {
        _mockTwilioService = new Mock<ITwilioService>();
        _mockLogger = new Mock<ILogger<TwilioSmsConnector>>();
    }

    [Fact]
    public async Task Should_CallsTwilioServiceAndReturnsSuccess_When_SendMessageAsyncWithValidMessage()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var mockMessageResource = CreateMockMessageResource();
        _mockTwilioService.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMessageResource);

        var message = CreateTestMessage();
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal(message.Id, result.Value.MessageId);
        Assert.Equal("SM123456789", result.Value.RemoteMessageId);
        Assert.Equal(MessageStatus.Queued, result.Value.Status);

        // Verify the Twilio service was called
        _mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_UseMessagingServiceSid_When_SendMessageAsyncWithMessagingService()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms; // This schema doesn't restrict receiving
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678")
            .SetParameter("MessagingServiceSid", "MG1234567890123456789012345678901234"); // Add messaging service

        var connector = new TwilioSmsConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var mockMessageResource = CreateMockMessageResource();
        _mockTwilioService.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMessageResource);

        var message = new Message
        {
            Id = "test-message-id",
            Sender = new Endpoint(EndpointType.PhoneNumber, "+1234567890"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+1987654321"),
            Content = new TextContent("Hello World")
        };

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(), $"Expected successful result but got error: {result.Error?.Code} - {result.Error?.Message}");

        // Verify the Twilio service was called
        _mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SendMessageAsyncWithTwilioException()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        _mockTwilioService.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Twilio API error"));

        var message = CreateTestMessage();
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(ConnectorErrorCodes.SendMessageError, result.Error?.Code);
        Assert.Contains("Twilio API error", result.Error?.Message);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SendMessageAsyncWithInvalidRecipient()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var message = new Message
        {
            Id = "test-message-id",
            Receiver = new Endpoint(EndpointType.EmailAddress, "invalid@email.com"), // Invalid endpoint type
            Content = new TextContent("Hello World")
        };

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(ConnectorErrorCodes.MessageValidationFailed, result.Error?.Code);

        // Verify Twilio service was not called
        _mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnStatusUpdate_When_GetMessageStatusAsyncWithValidMessageId()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var mockMessageResource = CreateMockMessageResource(MessageResource.StatusEnum.Delivered);
        _mockTwilioService.Setup(x => x.FetchMessageAsync("SM123456789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMessageResource);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.GetMessageStatusAsync("SM123456789", TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Value);
        Assert.Equal("SM123456789", result.Value.MessageId);
        Assert.Single(result.Value.Updates);
        Assert.Equal(MessageStatus.Delivered, result.Value.Updates.First().Status);

        _mockTwilioService.Verify(x => x.FetchMessageAsync("SM123456789", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_GetMessageStatusAsyncWithTwilioException()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        _mockTwilioService.Setup(x => x.FetchMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Message not found"));

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.GetMessageStatusAsync("SM123456789", TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(ConnectorErrorCodes.GetMessageStatusError, result.Error?.Code);
        Assert.Contains("Message not found", result.Error?.Message);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_TestConnectionAsyncWithValidCredentials()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var mockAccount = CreateMockAccountResource();
        _mockTwilioService.Setup(x => x.FetchAccountAsync("AC1234567890123456789012345678901234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockAccount);

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        _mockTwilioService.Verify(x => x.FetchAccountAsync("AC1234567890123456789012345678901234", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_TestConnectionAsyncWithInvalidCredentials()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        _mockTwilioService.Setup(x => x.FetchAccountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(ConnectorErrorCodes.ConnectionTestError, result.Error?.Code);
        Assert.Contains("Invalid credentials", result.Error?.Message);
    }

    [Fact]
    public async Task Should_CallsTwilioServiceInitialize_When_InitializeAsyncIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        // Act
        var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess());
        _mockTwilioService.Verify(x => x.Initialize("AC1234567890123456789012345678901234", "auth_token_1234567890123456789012345678"), Times.Once);
    }

    [Fact]
    public async Task Should_AppliesPropertiesCorrectly_When_SendMessageAsyncWithMessageProperties()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings, _mockTwilioService.Object, _mockLogger.Object);

        var mockMessageResource = CreateMockMessageResource();
        _mockTwilioService.Setup(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMessageResource);

        // Create a message that should pass validation
        var message = new Message
        {
            Id = "test-message-id",
            Sender = new Endpoint(EndpointType.PhoneNumber, "+1234567890"), // Valid E.164 format
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+1987654321"), // Valid E.164 format
            Content = new TextContent("Hello World"),
            Properties = new Dictionary<string, MessageProperty>
            {
                { "ValidityPeriod", new MessageProperty("ValidityPeriod", 3600) }, // Use integer instead of string
                { "MaxPrice", new MessageProperty("MaxPrice", 0.05m) } // Use decimal instead of string
            }
        };

        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(), $"Expected successful result but got error: {result.Error?.Code} - {result.Error?.Message}");

        // Verify the Twilio service was called
        _mockTwilioService.Verify(x => x.CreateMessageAsync(It.IsAny<CreateMessageOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ConnectionSettings CreateValidConnectionSettings()
    {
        return new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
    }

    private static Message CreateTestMessage()
    {
        return new Message
        {
            Id = "test-message-id",
            Sender = new Endpoint(EndpointType.PhoneNumber, "+1234567890"),
            Receiver = new Endpoint(EndpointType.PhoneNumber, "+1987654321"),
            Content = new TextContent("Hello World")
        };
    }

    private static MessageResource CreateMockMessageResource()
    {
        return TwilioMockFactory.CreateMockMessageResource("SM123456789", MessageResource.StatusEnum.Queued);
    }

    private static MessageResource CreateMockMessageResource(MessageResource.StatusEnum status)
    {
        return TwilioMockFactory.CreateMockMessageResource("SM123456789", status);
    }

    private static AccountResource CreateMockAccountResource()
    {
        // Create a mock AccountResource using reflection
        var accountResource = (AccountResource)Activator.CreateInstance(typeof(AccountResource), true)!;

        typeof(AccountResource).GetProperty("Sid")?.SetValue(accountResource, "AC1234567890123456789012345678901234");
        typeof(AccountResource).GetProperty("FriendlyName")?.SetValue(accountResource, "Test Account");
        typeof(AccountResource).GetProperty("Status")?.SetValue(accountResource, AccountResource.StatusEnum.Active);

        return accountResource;
    }



}
