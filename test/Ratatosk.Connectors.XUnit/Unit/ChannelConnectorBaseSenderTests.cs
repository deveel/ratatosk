namespace Ratatosk.XUnit;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "ChannelConnectorBase")]
public class ChannelConnectorBaseSenderTests
{
    private class TestConnector : ChannelConnectorBase
    {
        public TestConnector(IChannelSchema schema, ConnectionSettings? settings = null, ISenderResolver? senderResolver = null)
            : base(schema, settings, senderResolver: senderResolver)
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

    private class ResolverStub : ISenderResolver
    {
        private readonly ISender? _result;

        public ResolverStub(ISender? result) => _result = result;

        public int CallCount { get; private set; }

        public ValueTask<ISender?> ResolveSenderAsync(IEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return ValueTask.FromResult(_result);
        }
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
    public async Task Should_CallResolver_When_SenderResolverSet()
    {
        var stub = new ResolverStub(new PhoneSender("+1234567890", name: "resolved"));
        var schema = CreateSchema();
        var connector = new TestConnector(schema, senderResolver: stub);

        var message = new MessageBuilder()
            .FromSender("my-sender")
            .Build();

        var result = await connector.SendMessageAsync(message, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess());
        Assert.Equal(1, stub.CallCount);
    }

    [Fact]
    public async Task Should_NotCallResolver_When_NoSenderResolverSet()
    {
        var schema = CreateSchema();
        var connector = new TestConnector(schema);

        var message = new Message();

        var result = await connector.SendMessageAsync(message, default);

        Assert.False(result.IsSuccess());
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

    [Fact]
    public async Task Should_ReplaceSender_When_ResolverReturnsSender()
    {
        var originalSender = new SenderRef("to-be-resolved");
        var resolvedSender = new PhoneSender("+1234567890", name: "resolved");

        var stub = new ResolverStub(resolvedSender);
        var schema = CreateSchema();
        var connector = new TestConnector(schema, senderResolver: stub);

        var message = new MessageBuilder()
            .From(originalSender)
            .Build();

        var result = await connector.SendMessageAsync(message, default);

        Assert.False(result.IsSuccess());
        Assert.Same(resolvedSender, message.Sender);
    }

    [Fact]
    public async Task Should_NotReplaceSender_When_ResolverReturnsNull()
    {
        var originalSender = new SenderRef("will-not-resolve");
        var stub = new ResolverStub(null);
        var schema = CreateSchema();
        var connector = new TestConnector(schema, senderResolver: stub);

        var message = new MessageBuilder()
            .From(originalSender)
            .Build();

        var result = await connector.SendMessageAsync(message, default);

        Assert.False(result.IsSuccess());
        Assert.Same(originalSender, message.Sender);
    }
}
