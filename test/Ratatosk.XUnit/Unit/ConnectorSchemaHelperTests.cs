using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk.XUnit.Unit
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
            public ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken ct) => new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));
            public ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));
            public ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotSupportedException();
            public async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [EnumeratorCancellation] CancellationToken ct) { await Task.CompletedTask; yield break; }
            public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotSupportedException();
            public ValueTask ShutdownAsync(CancellationToken ct) => default;
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
            public ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken ct) => new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));
            public ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));
            public ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotSupportedException();
            public async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [EnumeratorCancellation] CancellationToken ct) { await Task.CompletedTask; yield break; }
            public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotSupportedException();
            public ValueTask ShutdownAsync(CancellationToken ct) => default;
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
        [ChannelSchema(typeof(int))]
        private class ConnectorWithInvalidSchemaType : IChannelConnector
        {
            public ConnectorWithInvalidSchemaType(IChannelSchema schema, ConnectionSettings? settings = null)
            {
                Schema = schema;
                ConnectionSettings = settings ?? new ConnectionSettings();
            }
            public IChannelSchema Schema { get; }
            public ConnectionSettings ConnectionSettings { get; }
            public ConnectorState State => ConnectorState.Uninitialized;
            public ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken ct) => new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));
            public ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));
            public ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotSupportedException();
            public async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [EnumeratorCancellation] CancellationToken ct) { await Task.CompletedTask; yield break; }
            public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotSupportedException();
            public ValueTask ShutdownAsync(CancellationToken ct) => default;
        }

        [Fact]
        public void Should_ThrowArgumentException_When_SchemaTypeIsNotValid()
        {
            var services = new ServiceCollection();
            services.AddMessaging().AddConnector<ConnectorWithInvalidSchemaType>();
            var provider = services.BuildServiceProvider();

            Assert.Throws<ArgumentException>(() => provider.GetRequiredService<ConnectorWithInvalidSchemaType>());
        }

        private class ThrowingSchemaFactory : IChannelSchemaFactory
        {
            public ThrowingSchemaFactory(string _)
            {
            }

            public IChannelSchema CreateSchema() => throw new NotSupportedException();
        }

        [ChannelSchema(typeof(ThrowingSchemaFactory))]
        private class ConnectorWithThrowingFactory : IChannelConnector
        {
            public ConnectorWithThrowingFactory(IChannelSchema schema, ConnectionSettings? settings = null)
            {
                Schema = schema;
                ConnectionSettings = settings ?? new ConnectionSettings();
            }
            public IChannelSchema Schema { get; }
            public ConnectionSettings ConnectionSettings { get; }
            public ConnectorState State => ConnectorState.Uninitialized;
            public ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken ct) => new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));
            public ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));
            public ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotSupportedException();
            public async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [EnumeratorCancellation] CancellationToken ct) { await Task.CompletedTask; yield break; }
            public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotSupportedException();
            public ValueTask ShutdownAsync(CancellationToken ct) => default;
        }

        [Fact]
        public void Should_ThrowInvalidOperation_When_FactoryCannotBeCreated()
        {
            var services = new ServiceCollection();
            services.AddMessaging().AddConnector<ConnectorWithThrowingFactory>();
            var provider = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ConnectorWithThrowingFactory>());
        }
    }
}
