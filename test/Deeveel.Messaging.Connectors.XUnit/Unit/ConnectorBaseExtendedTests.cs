using Microsoft.Extensions.Logging;

namespace Deveel.Messaging;

[Trait("Category", "Unit")]
[Trait("Feature", "ChannelConnectorBase")]
public class ConnectorBaseExtendedTests
{
    private class SimpleTestConnector : ChannelConnectorBase
    {
        public SimpleTestConnector(IChannelSchema schema, ConnectionSettings? settings = null, ILogger? logger = null)
            : base(schema, settings, logger)
        {
        }

        protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            if (message.Id == "test-1" && message.Content?.ContentType == MessageContentType.PlainText)
                return Task.FromResult(new SendResult(message.Id, remoteMessageId: "remote-1"));

            throw new ConnectorException("INVALID", Schema.ChannelType, "Message not supported");
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
            => Task.FromResult(new StatusInfo("Operational"));

        protected override Task<ConnectorHealth> GetConnectorHealthAsync(CancellationToken cancellationToken)
        {
            var health = new ConnectorHealth
            {
                IsHealthy = State == ConnectorState.Ready,
                State = State,
                LastHealthCheck = DateTime.UtcNow
            };
            return Task.FromResult(health);
        }

        protected override Task ShutdownConnectorAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private static ChannelSchema CreateSchema()
        => new ChannelSchema("Test", "Test", "1.0")
            .WithCapability(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages | ChannelCapability.HealthCheck)
            .AddContentType(MessageContentType.PlainText)
            .WithFlexibleMode();

    [Fact]
    public async Task Should_TestConnection_When_Ready()
    {
        var schema = CreateSchema();
        var connector = new SimpleTestConnector(schema);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.TestConnectionAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess());
    }

    [Fact]
    public async Task Should_Shutdown_AndTransitionState()
    {
        var schema = CreateSchema();
        var connector = new SimpleTestConnector(schema);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        await connector.ShutdownAsync(TestContext.Current.CancellationToken);

        Assert.Equal(ConnectorState.Shutdown, connector.State);
    }

    [Fact]
    public async Task Should_NotFail_When_ShutdownTwice()
    {
        var schema = CreateSchema();
        var connector = new SimpleTestConnector(schema);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);
        await connector.ShutdownAsync(TestContext.Current.CancellationToken);

        await connector.ShutdownAsync(TestContext.Current.CancellationToken);

        Assert.Equal(ConnectorState.Shutdown, connector.State);
    }

    [Fact]
    public async Task Should_GetHealth_When_Ready()
    {
        var schema = CreateSchema();
        var connector = new SimpleTestConnector(schema);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.GetHealthAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess());
        Assert.True(result.Value!.IsHealthy);
    }

    [Fact]
    public async Task Should_GetHealth_When_NotReady()
    {
        var schema = CreateSchema();
        var connector = new SimpleTestConnector(schema);

        var result = await connector.GetHealthAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess());
        Assert.False(result.Value!.IsHealthy);
    }

    [Fact]
    public async Task Should_GetStatus_When_Ready()
    {
        var schema = CreateSchema();
        var connector = new SimpleTestConnector(schema);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var result = await connector.GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess());
    }

    [Fact]
    public async Task Should_SendMessage_When_Ready()
    {
        var schema = CreateSchema();
        var connector = new SimpleTestConnector(schema);
        await connector.InitializeAsync(TestContext.Current.CancellationToken);

        var message = new Message { Id = "test-1", Content = new TextContent("hello") };
        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess());
        Assert.Equal("remote-1", result.Value!.RemoteMessageId);
    }

}
