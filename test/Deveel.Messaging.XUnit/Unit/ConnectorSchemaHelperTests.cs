using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ConnectorSchemaHelper")]
    public class ConnectorSchemaHelperTests
    {
        private class TestSchema : IChannelSchema
        {
            public string ChannelProvider => "Test";
            public string ChannelType => "Test";
            public string Version => "1.0";
            public string? DisplayName => "Test Schema";
            public bool IsStrict => false;
            public ChannelCapability Capabilities => ChannelCapability.SendMessages;
            public IReadOnlyList<ChannelEndpointConfiguration> Endpoints => new List<ChannelEndpointConfiguration>();
            public IReadOnlyList<ChannelParameter> Parameters => new List<ChannelParameter>();
            public IReadOnlyList<MessagePropertyConfiguration> MessageProperties => new List<MessagePropertyConfiguration>();
            public IReadOnlyList<MessageContentType> ContentTypes => new List<MessageContentType>();
            public IReadOnlyList<AuthenticationConfiguration> AuthenticationConfigurations => new List<AuthenticationConfiguration>();
        }

        private class TestSchemaFactory : IChannelSchemaFactory
        {
            public IChannelSchema CreateSchema() => new TestSchema();
        }

        private class NonSchemaType { }

        [ChannelSchema(typeof(TestSchemaFactory))]
        private class SchemaConnector : IChannelConnector
        {
            public SchemaConnector(IChannelSchema schema) { Schema = schema; State = ConnectorState.Uninitialized; }
            public IChannelSchema Schema { get; }
            public ConnectorState State { get; }
            public Task<OperationResult<bool>> InitializeAsync(CancellationToken ct) => Task.FromResult(OperationResult<bool>.Success(true));
            public Task<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => Task.FromResult(OperationResult<bool>.Success(true));
            public Task<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotImplementedException();
            public Task<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotImplementedException();
            public Task<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotImplementedException();
            public Task<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotImplementedException();
            public async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [EnumeratorCancellation] CancellationToken ct) { await Task.CompletedTask; yield break; }
            public Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotImplementedException();
            public Task<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotImplementedException();
            public Task<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotImplementedException();
            public Task ShutdownAsync(CancellationToken ct) => Task.CompletedTask;
        }

        [ChannelSchema(typeof(TestSchema))]
        private class DirectSchemaConnector : IChannelConnector
        {
            public DirectSchemaConnector(IChannelSchema schema) { Schema = schema; State = ConnectorState.Uninitialized; }
            public IChannelSchema Schema { get; }
            public ConnectorState State { get; }
            public Task<OperationResult<bool>> InitializeAsync(CancellationToken ct) => Task.FromResult(OperationResult<bool>.Success(true));
            public Task<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => Task.FromResult(OperationResult<bool>.Success(true));
            public Task<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotImplementedException();
            public Task<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotImplementedException();
            public Task<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotImplementedException();
            public Task<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotImplementedException();
            public async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [EnumeratorCancellation] CancellationToken ct) { await Task.CompletedTask; yield break; }
            public Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotImplementedException();
            public Task<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotImplementedException();
            public Task<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotImplementedException();
            public Task ShutdownAsync(CancellationToken ct) => Task.CompletedTask;
        }

        [Fact]
        public void Should_DiscoverSchemaViaFactory()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var schema = ConnectorSchemaHelper.DiscoverConnectorSchema(services, typeof(SchemaConnector));

            Assert.NotNull(schema);
            Assert.Equal("Test", schema.ChannelProvider);
            Assert.Equal("Test", schema.ChannelType);
        }

        [Fact]
        public void Should_Throw_When_NoChannelSchemaAttribute()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            Assert.Throws<ArgumentException>(() =>
                ConnectorSchemaHelper.DiscoverConnectorSchema(services, typeof(string)));
        }

        [Fact]
        public void Should_CreateSchemaFromFactoryType()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var schema = ConnectorSchemaHelper.CreateSchema(services, typeof(TestSchemaFactory));

            Assert.NotNull(schema);
            Assert.Equal("Test", schema.ChannelProvider);
        }

        [Fact]
        public void Should_CreateSchemaFromDirectType()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var schema = ConnectorSchemaHelper.CreateSchema(services, typeof(TestSchema));

            Assert.NotNull(schema);
            Assert.Equal("Test", schema.ChannelProvider);
        }

        [Fact]
        public void Should_Throw_When_TypeIsNotSchemaOrFactory()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            Assert.Throws<InvalidOperationException>(() =>
                ConnectorSchemaHelper.CreateSchema(services, typeof(NonSchemaType)));
        }
    }
}
