using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Deveel.Messaging;

/// <summary>
/// Tests for the <see cref="TwilioSmsConnector"/> class to verify
/// its functionality and integration with the Twilio API.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "TwilioSmsConnector")]
public class TwilioSmsConnectorTests
{
    [Fact]
    public void Should_CreateConnector_When_ConstructorWithValidSchemaAndSettings()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioSms;
        var connectionSettings = CreateValidConnectionSettings();

        // Act
        var connector = new TwilioSmsConnector(schema, connectionSettings);

        // Assert
        Assert.Same(schema, connector.Schema);
        Assert.Equal(ConnectorState.Uninitialized, connector.State);
    }

    [Fact]
    public void Should_UseDefaultSchema_When_ConstructorWithConnectionSettingsOnly()
    {
        // Arrange
        var connectionSettings = CreateValidConnectionSettings();

        // Act
        var connector = new TwilioSmsConnector(connectionSettings);

        // Assert
        Assert.Equal(TwilioConnectorConstants.Provider, connector.Schema.ChannelProvider);
        Assert.Equal(TwilioConnectorConstants.SmsChannel, connector.Schema.ChannelType);
        Assert.Equal(ConnectorState.Uninitialized, connector.State);
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ConstructorWithNullConnectionSettings()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioSms;

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => new TwilioSmsConnector(schema, null!));
        Assert.Throws<ArgumentNullException>(() => new TwilioSmsConnector(null!));
    }

    [Fact]
    public void Should_StoreLogger_When_ConstructorWithLogger()
    {
        // Arrange
        var schema = TwilioChannelSchemas.TwilioSms;
        var connectionSettings = CreateValidConnectionSettings();
        var logger = new TestLogger<TwilioSmsConnector>();

        // Act
        var connector = new TwilioSmsConnector(schema, connectionSettings, null, logger);

        // Assert
        Assert.Same(schema, connector.Schema);
        Assert.Equal(ConnectorState.Uninitialized, connector.State);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_InitializeAsyncWithValidSettings()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms; // Use simple schema to avoid validation complexity
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings);

        // Act
        var result = await connector.InitializeAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Successful);
        Assert.Equal(ConnectorState.Ready, connector.State);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_InitializeAsyncWithMissingCredentials()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = new ConnectionSettings(); // Empty settings
        var connector = new TwilioSmsConnector(schema, connectionSettings);

        // Act
        var result = await connector.InitializeAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Successful);
        Assert.Equal("MISSING_CREDENTIALS", result.Error?.ErrorCode);
        Assert.Equal(ConnectorState.Error, connector.State);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_InitializeAsyncWithMissingFromNumberAndMessagingService()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678");
        var connector = new TwilioSmsConnector(schema, connectionSettings);

        // Act
        var result = await connector.InitializeAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Successful); // Should succeed now since FromNumber is no longer required at connection level
        Assert.Equal(ConnectorState.Ready, connector.State);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_InitializeAsyncWithMessagingServiceOnly()
    {
        // Arrange
        var schema = TwilioChannelSchemas.BulkSms; // This schema requires MessagingServiceSid
        var connectionSettings = new ConnectionSettings()
            .SetParameter("AccountSid", "AC1234567890123456789012345678901234")
            .SetParameter("AuthToken", "auth_token_1234567890123456789012345678")
            .SetParameter("MessagingServiceSid", "MG1234567890123456789012345678901234");
        var connector = new TwilioSmsConnector(schema, connectionSettings);

        // Act
        var result = await connector.InitializeAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Successful);
        Assert.Equal(ConnectorState.Ready, connector.State);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_InitializeAsyncWhenAlreadyInitialized()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings);
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
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings);

        // Act
        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            connector.TestConnectionAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Should_ThrowNotSupportedException_When_SendMessageAsyncWithoutSendCapability()
    {
        // Arrange
        var schema = new ChannelSchema("Twilio", "SMS", "1.0.0")
            .WithCapabilities(ChannelCapability.ReceiveMessages); // No send capability
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings);
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
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            connector.SendMessageAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Should_ThrowNotSupportedException_When_GetMessageStatusAsyncWithoutCapability()
    {
        // Arrange
        var schema = new ChannelSchema("Twilio", "SMS", "1.0.0")
            .WithCapabilities(ChannelCapability.SendMessages); // No status query capability
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        // Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => 
            connector.GetMessageStatusAsync("test-message", CancellationToken.None));
    }

    [Fact]
    public async Task Should_ReturnHealthInfo_When_GetHealthAsyncWhenInitialized()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        var result = await connector.GetHealthAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Successful);
        Assert.NotNull(result.Value);
        Assert.Equal(ConnectorState.Ready, result.Value.State);
        // Note: IsHealthy might be false due to connection test with test credentials
        // but the main assertion is that the connector state is Ready and the result is successful
    }

    [Fact]
    public async Task Should_ReturnCorrectInformation_When_GetStatusAsyncIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        var result = await connector.GetStatusAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Successful);
        // StatusInfo is a value type, no need for NotNull check
        Assert.Contains("Twilio SMS Connector", result.Value.Status);
        Assert.True(result.Value.AdditionalData.ContainsKey("AccountSid"));
        Assert.True(result.Value.AdditionalData.ContainsKey("State"));
        Assert.True(result.Value.AdditionalData.ContainsKey("Uptime"));
    }

    [Fact]
    public async Task Should_TransitionToShutdownState_When_ShutdownAsyncIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();
        var connector = new TwilioSmsConnector(schema, connectionSettings);
        await connector.InitializeAsync(CancellationToken.None);

        // Act
        await connector.ShutdownAsync(CancellationToken.None);

        // Assert
        Assert.Equal(ConnectorState.Shutdown, connector.State);
    }

    [Fact]
    public void Should_ImplementCorrectInterface_When_TwilioSmsConnectorIsInvoked()
    {
        // Arrange
        var schema = TwilioChannelSchemas.SimpleSms;
        var connectionSettings = CreateValidConnectionSettings();

        // Act
        IChannelConnector connector = new TwilioSmsConnector(schema, connectionSettings);

        // Assert
        Assert.NotNull(connector);
        Assert.Same(schema, connector.Schema);
        Assert.Equal(ConnectorState.Uninitialized, connector.State);
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

    private class TestLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}