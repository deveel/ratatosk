namespace Ratatosk.XUnit;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "ChannelConnectorBase")]
public class ChannelConnectorBaseSenderTests
{
    private class TestConnector : ChannelConnectorBase
    {
        public TestConnector(IChannelSchema schema, ConnectionSettings? settings = null)
            : base(schema, settings)
        {
        }

        protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            SetState(ConnectorState.Ready);
            return ValueTask.CompletedTask;
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private class CustomNameConnector : ChannelConnectorBase
    {
        public CustomNameConnector(IChannelSchema schema)
            : base(schema)
        {
        }

        public override string ConnectorName => "custom-name";

        protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            SetState(ConnectorState.Ready);
            return ValueTask.CompletedTask;
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private static IChannelSchema CreateSchema(string channelType = "test-channel")
        => new ChannelSchemaBuilder("TestProvider", channelType, "1.0.0").Build();

    [Fact]
    public void Should_DefaultConnectorName_When_NotOverridden()
    {
        var schema = CreateSchema("sms");
        var connector = new TestConnector(schema);

        Assert.Equal("sms", connector.ConnectorName);
    }

    [Fact]
    public void Should_UseCustomConnectorName_When_Overridden()
    {
        var schema = CreateSchema("sms");
        var connector = new CustomNameConnector(schema);

        Assert.Equal("custom-name", connector.ConnectorName);
    }

    [Fact]
    public async Task Should_PassThroughSender_When_NoResolver()
    {
        var schema = CreateSchema();
        var connector = new TestConnector(schema);

        var originalSender = new PhoneSender("+1234567890", name: "direct");
        var message = new MessageBuilder()
            .From(originalSender)
            .Build();

        var result = await connector.SendMessageAsync(message, default);

        Assert.False(result.IsSuccess());
        Assert.Same(originalSender, message.Sender);
    }
}
