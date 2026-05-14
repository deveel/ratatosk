using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Deveel.Messaging.XUnit.Fixtures
{
    [ChannelSchema(typeof(MockSchemaFactory))]
    public class MockConnector : IChannelConnector, IDisposable
    {
        private ConnectorState _state = ConnectorState.Uninitialized;
        private int _initCount;

        public MockConnector(IChannelSchema schema, ConnectionSettings? settings = null)
        {
            Schema = schema;
            ConnectionSettings = settings ?? new ConnectionSettings();
        }

        public IChannelSchema Schema { get; }
        public ConnectionSettings ConnectionSettings { get; }
        public ConnectorState State => _state;
        public int InitCount => _initCount;
        public bool FailOnInitialize { get; set; }
        public bool FailOnTestConnection { get; set; }
        public Func<IMessage, SendResult>? OnSend { get; set; }
        public Func<MessageSource, ReceiveResult>? OnReceive { get; set; }

        public async ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken cancellationToken)
        {
            if (_state != ConnectorState.Uninitialized)
                return OperationResult<bool>.Fail("ALREADY_INIT", Schema.ChannelType, "Already initialized");

            _state = ConnectorState.Initializing;

            if (FailOnInitialize)
            {
                _state = ConnectorState.Error;
                return OperationResult<bool>.Fail("INIT_FAILED", Schema.ChannelType, "Simulated init failure");
            }

            await Task.Delay(1, cancellationToken);
            _initCount++;
            _state = ConnectorState.Ready;
            return true;
        }

        public async ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken cancellationToken)
        {
            if (FailOnTestConnection)
                return OperationResult<bool>.Fail("TEST_FAILED", Schema.ChannelType, "Simulated test failure");

            await Task.Delay(1, cancellationToken);
            return true;
        }

        public async ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            if (OnSend != null)
            {
                var result = OnSend(message);
                return result;
            }

            await Task.Delay(1, cancellationToken);
            return new SendResult(message.Id, $"remote-{message.Id}");
        }

        public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken cancellationToken)
        {
            var info = new StatusInfo("Mock connector is ready");
            return new ValueTask<OperationResult<StatusInfo>>(OperationResult<StatusInfo>.Success(info));
        }

        public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken)
            => new ValueTask<OperationResult<StatusUpdatesResult>>(
                OperationResult<StatusUpdatesResult>.Success(
                    new StatusUpdatesResult(messageId, new[] { new StatusUpdateResult(messageId, MessageStatus.Sent) })));

        public async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }

        public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken cancellationToken)
        {
            var result = new StatusUpdateResult("msg-id", MessageStatus.Delivered);
            return new ValueTask<OperationResult<StatusUpdateResult>>(OperationResult<StatusUpdateResult>.Success(result));
        }

        public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken cancellationToken)
        {
            if (OnReceive != null)
            {
                var result = OnReceive(source);
                return new ValueTask<OperationResult<ReceiveResult>>(OperationResult<ReceiveResult>.Success(result));
            }

            var messages = new List<IMessage> { new Message { Id = "rcvd-1" } };
            var receiveResult = new ReceiveResult("batch-1", messages);
            return new ValueTask<OperationResult<ReceiveResult>>(OperationResult<ReceiveResult>.Success(receiveResult));
        }

        public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken cancellationToken)
        {
            var health = new ConnectorHealth
            {
                IsHealthy = _state == ConnectorState.Ready,
                State = _state
            };
            return new ValueTask<OperationResult<ConnectorHealth>>(OperationResult<ConnectorHealth>.Success(health));
        }

        public ValueTask ShutdownAsync(CancellationToken cancellationToken)
        {
            _state = ConnectorState.Shutdown;
            return default;
        }

        void IDisposable.Dispose()
        {
            _state = ConnectorState.Shutdown;
        }
    }

    public class MockSchemaFactory : IChannelSchemaFactory
    {
        public IChannelSchema CreateSchema()
            => new MockSchema();
    }

    public class MockSchema : IChannelSchema
    {
        public string ChannelProvider { get; set; } = "MockProvider";
        public string ChannelType { get; set; } = "MockChannel";
        public string Version { get; set; } = "1.0";
        public string? DisplayName { get; set; } = "Mock Connector";
        public bool IsStrict { get; set; } = false;
        public ChannelCapability Capabilities { get; set; } = ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages | ChannelCapability.MessageStatusQuery | ChannelCapability.HealthCheck;
        public IReadOnlyList<ChannelEndpointConfiguration> Endpoints { get; set; } = new List<ChannelEndpointConfiguration>();
        public IReadOnlyList<ChannelParameter> Parameters { get; set; } = new List<ChannelParameter>();
        public IReadOnlyList<MessagePropertyConfiguration> MessageProperties { get; set; } = new List<MessagePropertyConfiguration>();
        public IReadOnlyList<MessageContentType> ContentTypes { get; set; } = new List<MessageContentType>();
        public IReadOnlyList<AuthenticationConfiguration> AuthenticationConfigurations { get; set; } = new List<AuthenticationConfiguration>();
    }
}
