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
            public string ChannelProvider => "PublicTest";
            public string ChannelType => "HelperTest";
            public string Version => "1.0";
            public string? DisplayName => "Public Test Schema";
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

        [ChannelSchema(typeof(TestSchemaFactory))]
        private class ConnectorWithFactory : IChannelConnector
        {
            public ConnectorWithFactory(IChannelSchema schema, ConnectionSettings? settings = null)
            {
                Schema = schema;
                ConnectionSettings = settings ?? new ConnectionSettings();
            }
            public IChannelSchema Schema { get; }
            public ConnectionSettings ConnectionSettings { get; }
            public ConnectorState State => ConnectorState.Uninitialized;
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
        private class ConnectorWithDirectSchema : IChannelConnector
        {
            public ConnectorWithDirectSchema(IChannelSchema schema, ConnectionSettings? settings = null)
            {
                Schema = schema;
                ConnectionSettings = settings ?? new ConnectionSettings();
            }
            public IChannelSchema Schema { get; }
            public ConnectionSettings ConnectionSettings { get; }
            public ConnectorState State => ConnectorState.Uninitialized;
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
        public void Should_DiscoverSchema_When_ConnectorHasFactoryAttribute()
        {
            var services = new ServiceCollection();
            services.AddMessaging().AddConnector<ConnectorWithFactory>();
            var provider = services.BuildServiceProvider();

            var connector = provider.GetRequiredService<ConnectorWithFactory>();

            Assert.NotNull(connector);
            Assert.Equal("PublicTest", connector.Schema.ChannelProvider);
            Assert.Equal("HelperTest", connector.Schema.ChannelType);
        }

        [Fact]
        public void Should_DiscoverSchema_When_ConnectorHasDirectSchemaAttribute()
        {
            var services = new ServiceCollection();
            services.AddMessaging().AddConnector<ConnectorWithDirectSchema>();
            var provider = services.BuildServiceProvider();

            var connector = provider.GetRequiredService<ConnectorWithDirectSchema>();

            Assert.NotNull(connector.Schema);
            Assert.Equal("PublicTest", connector.Schema.ChannelProvider);
            Assert.Equal("HelperTest", connector.Schema.ChannelType);
        }

        [Fact]
        public void Should_Throw_When_ConnectorHasNoSchemaAttribute()
        {
            var services = new ServiceCollection();
            var builder = services.AddMessaging();

            Assert.Throws<ArgumentException>(() => builder.AddConnector(typeof(string)));
        }

        [Fact]
        public void Should_RegisterConnector_WithNamedRegistration()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnector<ConnectorWithFactory>("test-channel", _ => { });
            var provider = services.BuildServiceProvider();

            var connector = provider.GetRequiredKeyedService<IChannelConnector>("test-channel");

            Assert.NotNull(connector);
            Assert.Equal("PublicTest", connector.Schema.ChannelProvider);
        }

        [Fact]
        public void Should_RegisterConnector_WithConnectionString()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnector<ConnectorWithFactory>("cfg", b => b.WithConnectionString("Key=Value"));
            var provider = services.BuildServiceProvider();

            var connector = provider.GetRequiredKeyedService<IChannelConnector>("cfg");

            Assert.NotNull(connector);
            Assert.Equal("PublicTest", connector.Schema.ChannelProvider);
        }
    }
}
