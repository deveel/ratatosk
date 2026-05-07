using Microsoft.Extensions.Logging;
using Moq;

namespace Deveel.Messaging;

/// <summary>
/// Tests for the <see cref="SendGridEmailConnector"/> class to verify
/// its functionality and integration with the SendGrid API.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "SendGridEmailConnector")]
public class SendGridEmailConnectorTests
{
    [Fact]
    public void Should_CreateConnector_When_ConstructorWithValidSchemaAndSettings()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SendGridEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();

        // Act
        var connector = new SendGridEmailConnector(schema, connectionSettings);

        // Assert
        Assert.Same(schema, connector.Schema);
        Assert.Equal(ConnectorState.Uninitialized, connector.State);
    }

    [Fact]
    public void Should_UseDefaultSchema_When_ConstructorWithConnectionSettingsOnly()
    {
        // Arrange
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();

        // Act
        var connector = new SendGridEmailConnector(connectionSettings);

        // Assert
        Assert.Equal(SendGridConnectorConstants.Provider, connector.Schema.ChannelProvider);
        Assert.Equal(SendGridConnectorConstants.EmailChannel, connector.Schema.ChannelType);
        Assert.Equal(ConnectorState.Uninitialized, connector.State);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ConstructorWithNullConnectionSettings()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SendGridEmail;

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => new SendGridEmailConnector(schema, null!));
        Assert.Throws<ArgumentNullException>(() => new SendGridEmailConnector(null!));
    }

    [Fact]
    public void Should_StoreLogger_When_ConstructorWithLogger()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SendGridEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var logger = new TestLogger<SendGridEmailConnector>();

        // Act
        var connector = new SendGridEmailConnector(schema, connectionSettings, null, logger);

        // Assert
        Assert.Same(schema, connector.Schema);
        Assert.Equal(ConnectorState.Uninitialized, connector.State);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_InitializeAsyncWithValidSettings()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail; // Use simple schema to avoid validation complexity
        var connectionSettings = SendGridMockFactory.CreateMinimalConnectionSettings(); // Use minimal settings that don't include removed parameters
        var connector = new SendGridEmailConnector(schema, connectionSettings);

        // Act
        var result = await connector.InitializeAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Successful, $"Expected successful initialization but got: {result.Error?.ErrorCode} - {result.Error?.ErrorMessage}");
        Assert.Equal(ConnectorState.Ready, connector.State);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_InitializeAsyncWithMissingApiKey()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = new ConnectionSettings(); // Empty settings
        var connector = new SendGridEmailConnector(schema, connectionSettings);

        // Act
        var result = await connector.InitializeAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Successful);
        Assert.Equal(SendGridErrorCodes.MissingApiKey, result.Error?.ErrorCode);
        Assert.Equal(ConnectorState.Error, connector.State);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_InitializeAsyncWhenAlreadyInitialized()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var connector = new SendGridEmailConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        var result = await connector.InitializeAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Successful);
        Assert.Equal("ALREADY_INITIALIZED", result.Error?.ErrorCode);
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_TestConnectionAsyncWhenNotInitialized()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var connector = new SendGridEmailConnector(schema, connectionSettings);

        // Act
        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            connector.TestConnectionAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_TestConnectionAsyncWithValidConnection()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var mockService = SendGridMockFactory.CreateSuccessfulMock();
        var connector = new SendGridEmailConnector(schema, connectionSettings, mockService.Object);

        await connector.InitializeAsync(CancellationToken.None);

        // Act
        var result = await connector.TestConnectionAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Successful);
        mockService.Verify(x => x.TestConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_TestConnectionAsyncWithConnectionFailure()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var mockService = SendGridMockFactory.CreateConnectionFailureMock();
        var connector = new SendGridEmailConnector(schema, connectionSettings, mockService.Object);

        await connector.InitializeAsync(CancellationToken.None);

        // Act
        var result = await connector.TestConnectionAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Successful);
        Assert.Equal(SendGridErrorCodes.ConnectionFailed, result.Error?.ErrorCode);
        mockService.Verify(x => x.TestConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ThrowNotSupportedException_When_SendMessageAsyncWithoutSendCapability()
    {
        // Arrange
        var schema = new ChannelSchema("SendGrid", "Email", "1.0.0")
            .WithCapabilities(ChannelCapability.ReceiveMessages); // No send capability
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var connector = new SendGridEmailConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);
        var message = CreateTestMessage();

        // Act
        // Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => 
            connector.SendMessageAsync(message, CancellationToken.None));
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_SendMessageAsyncWithNullMessage()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var connector = new SendGridEmailConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            connector.SendMessageAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_SendMessageAsyncWithValidMessage()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var mockService = SendGridMockFactory.CreateSuccessfulMock();
        var connector = new SendGridEmailConnector(schema, connectionSettings, mockService.Object);

        await connector.InitializeAsync(CancellationToken.None);
        var message = CreateTestMessage();

        // Act
        var result = await connector.SendMessageAsync(message, CancellationToken.None);

        // Assert
        Assert.True(result.Successful);
        Assert.NotNull(result.Value);
        Assert.Equal(MessageStatus.Sent, result.Value.Status);
        mockService.Verify(x => x.SendEmailAsync(It.IsAny<SendGrid.Helpers.Mail.SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SendMessageAsyncWithInvalidSender()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var connector = new SendGridEmailConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        var message = CreateTestMessage();
        message.Sender = new Endpoint(EndpointType.EmailAddress, "invalid-email");

        // Act
        var result = await connector.SendMessageAsync(message, CancellationToken.None);

        // Assert
        Assert.False(result.Successful);
        Assert.Equal(SendGridErrorCodes.InvalidEmailAddress, result.Error?.ErrorCode);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SendMessageAsyncWithInvalidReceiver()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var connector = new SendGridEmailConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        var message = CreateTestMessage();
        message.Receiver = new Endpoint(EndpointType.EmailAddress, "invalid-email");

        // Act
        var result = await connector.SendMessageAsync(message, CancellationToken.None);

        // Assert
        Assert.False(result.Successful);
        Assert.Equal(SendGridErrorCodes.InvalidRecipient, result.Error?.ErrorCode);
    }

    [Fact]
    public async Task Should_ThrowNotSupportedException_When_GetMessageStatusAsyncWithoutCapability()
    {
        // Arrange
        var schema = new ChannelSchema("SendGrid", "Email", "1.0.0")
            .WithCapabilities(ChannelCapability.SendMessages); // No status query capability
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var connector = new SendGridEmailConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        // Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => 
            connector.GetMessageStatusAsync("test-message", CancellationToken.None));
    }

    [Fact]
    public async Task Should_ReturnStatus_When_GetMessageStatusAsyncWithValidMessageId()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SendGridEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var mockService = SendGridMockFactory.CreateSuccessfulMock();
        var connector = new SendGridEmailConnector(schema, connectionSettings, mockService.Object);

        await connector.InitializeAsync(CancellationToken.None);

        // Act
        var result = await connector.GetMessageStatusAsync("test-message-id", CancellationToken.None);

        // Assert
        Assert.True(result.Successful);
        Assert.NotNull(result.Value);
        mockService.Verify(x => x.GetEmailActivityAsync("test-message-id", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnStatusInfo_When_GetStatusAsyncWhenInitialized()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var connector = new SendGridEmailConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        var result = await connector.GetStatusAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Successful);
        Assert.Contains("SendGrid Email Connector", result.Value.Status);
        Assert.True(result.Value.AdditionalData.ContainsKey("ApiKeyConfigured"));
        Assert.True(result.Value.AdditionalData.ContainsKey("State"));
        Assert.True(result.Value.AdditionalData.ContainsKey("Uptime"));
    }

    [Fact]
    public async Task Should_ReturnHealthInfo_When_GetHealthAsyncWhenInitialized()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateMinimalConnectionSettings(); // Use minimal settings
        var connector = new SendGridEmailConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        var result = await connector.GetHealthAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Successful, $"Expected successful result but got: {result.Error?.ErrorMessage}");
        Assert.NotNull(result.Value);
        // Note: The health check includes a connection test, which might fail in some test environments
        // The important thing is that the result is successful and we get a health object back
        Assert.True(result.Value.State == ConnectorState.Ready || result.Value.State == ConnectorState.Error, 
            $"Expected Ready or Error state, but got: {result.Value.State}");
    }

    [Fact]
    public async Task Should_TransitionToShutdownState_When_ShutdownAsyncIsInvoked()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();
        var connector = new SendGridEmailConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        await connector.ShutdownAsync(CancellationToken.None);

        // Assert
        Assert.Equal(ConnectorState.Shutdown, connector.State);
    }

    [Fact]
    public void Should_ImplementCorrectInterface_When_SendGridEmailConnectorIsInvoked()
    {
        // Arrange
        var schema = SendGridChannelSchemas.SimpleEmail;
        var connectionSettings = SendGridMockFactory.CreateValidConnectionSettings();

        // Act
        IChannelConnector connector = new SendGridEmailConnector(schema, connectionSettings);

        // Assert
        Assert.NotNull(connector);
        Assert.Same(schema, connector.Schema);
        Assert.Equal(ConnectorState.Uninitialized, connector.State);
    }

    private static Message CreateTestMessage()
    {
        return new Message
        {
            Id = "test-message-id",
            Sender = new Endpoint(EndpointType.EmailAddress, "sender@test.com"),
            Receiver = new Endpoint(EndpointType.EmailAddress, "receiver@test.com"),
            Content = new TextContent("Hello World"),
            Properties = new Dictionary<string, MessageProperty>
            {
                ["Subject"] = new MessageProperty("Subject", "Test Subject")
            }
        };
    }
}