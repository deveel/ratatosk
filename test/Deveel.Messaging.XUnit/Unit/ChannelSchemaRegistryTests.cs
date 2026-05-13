using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ChannelSchemaRegistry")]
    public class ChannelSchemaRegistryTests
    {
        private class DirectSchema : IChannelSchema
        {
            public string ChannelProvider { get; set; } = "DirectP";
            public string ChannelType { get; set; } = "DirectT";
            public string Version => "1.0";
            public string? DisplayName => null;
            public bool IsStrict => false;
            public ChannelCapability Capabilities => ChannelCapability.SendMessages;
            public IReadOnlyList<ChannelEndpointConfiguration> Endpoints => new List<ChannelEndpointConfiguration>();
            public IReadOnlyList<ChannelParameter> Parameters => new List<ChannelParameter>();
            public IReadOnlyList<MessagePropertyConfiguration> MessageProperties => new List<MessagePropertyConfiguration>();
            public IReadOnlyList<MessageContentType> ContentTypes => new List<MessageContentType>();
            public IReadOnlyList<AuthenticationConfiguration> AuthenticationConfigurations => new List<AuthenticationConfiguration>();
        }

        private class DirectSchemaFactory : IChannelSchemaFactory
        {
            public IChannelSchema CreateSchema() => new DirectSchema();
        }

        private class NamedSchema : IChannelSchema
        {
            public string ChannelProvider { get; set; } = "NamedP";
            public string ChannelType { get; set; } = "NamedT";
            public string Version => "1.0";
            public string? DisplayName => null;
            public bool IsStrict => false;
            public ChannelCapability Capabilities => ChannelCapability.SendMessages;
            public IReadOnlyList<ChannelEndpointConfiguration> Endpoints => new List<ChannelEndpointConfiguration>();
            public IReadOnlyList<ChannelParameter> Parameters => new List<ChannelParameter>();
            public IReadOnlyList<MessagePropertyConfiguration> MessageProperties => new List<MessagePropertyConfiguration>();
            public IReadOnlyList<MessageContentType> ContentTypes => new List<MessageContentType>();
            public IReadOnlyList<AuthenticationConfiguration> AuthenticationConfigurations => new List<AuthenticationConfiguration>();
        }

        private class NamedSchemaFactory : IChannelSchemaFactory
        {
            public IChannelSchema CreateSchema() => new NamedSchema();
        }

        [ChannelSchema(typeof(DirectSchemaFactory))]
        private class DirectConnector : IChannelConnector
        {
            public DirectConnector(IChannelSchema schema, ConnectionSettings? settings = null)
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

        [ChannelSchema(typeof(NamedSchemaFactory))]
        private class NamedConnector : IChannelConnector
        {
            public NamedConnector(IChannelSchema schema, ConnectionSettings? settings = null)
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

        private static IServiceProvider CreateProvider(Action<MessagingBuilder> configure)
        {
            var services = new ServiceCollection();
            var builder = services.AddMessaging();
            configure(builder);
            return services.BuildServiceProvider();
        }

        [Fact]
        public void Should_GetSchema_FromDirectConnector()
        {
            var provider = CreateProvider(b => b.AddConnector<DirectConnector>());
            var registry = provider.GetRequiredService<IChannelSchemaRegistry>();

            var schemas = registry.GetSchemas().ToList();

            Assert.Contains(schemas, s => s.ChannelProvider == "DirectP" && s.ChannelType == "DirectT");
        }

        [Fact]
        public void Should_GetSchema_FromNamedConnector()
        {
            var provider = CreateProvider(b => b.AddConnector<NamedConnector>("ch", _ => { }));
            var registry = provider.GetRequiredService<IChannelSchemaRegistry>();

            var schemas = registry.GetSchemas().ToList();

            Assert.Contains(schemas, s => s.ChannelProvider == "NamedP" && s.ChannelType == "NamedT");
        }

        [Fact]
        public void Should_ReturnMultipleSchemas()
        {
            var provider = CreateProvider(b =>
            {
                b.AddConnector<DirectConnector>();
                b.AddConnector<NamedConnector>("ch2", _ => { });
            });
            var registry = provider.GetRequiredService<IChannelSchemaRegistry>();

            var schemas = registry.GetSchemas().ToList();

            Assert.Equal(2, schemas.Count);
        }

        [Fact]
        public void Should_FindSchema_ByProviderAndType()
        {
            var provider = CreateProvider(b => b.AddConnector<DirectConnector>());
            var registry = provider.GetRequiredService<IChannelSchemaRegistry>();

            var found = registry.FindSchema("DirectP", "DirectT");

            Assert.NotNull(found);
        }

        [Fact]
        public void Should_FindSchema_CaseInsensitive()
        {
            var provider = CreateProvider(b => b.AddConnector<DirectConnector>());
            var registry = provider.GetRequiredService<IChannelSchemaRegistry>();

            var found = registry.FindSchema("directp", "directt");

            Assert.NotNull(found);
        }

        [Fact]
        public void Should_ReturnNull_When_SchemaNotFound()
        {
            var provider = CreateProvider(b => b.AddConnector<DirectConnector>());
            var registry = provider.GetRequiredService<IChannelSchemaRegistry>();

            var found = registry.FindSchema("NotFound", "Nope");

            Assert.Null(found);
        }

        [Fact]
        public void Should_HaveSchema_When_Exists()
        {
            var provider = CreateProvider(b => b.AddConnector<DirectConnector>());
            var registry = provider.GetRequiredService<IChannelSchemaRegistry>();

            Assert.True(registry.HasSchema("DirectP", "DirectT"));
        }

        [Fact]
        public void Should_NotHaveSchema_When_NotExists()
        {
            var provider = CreateProvider(b => b.AddConnector<DirectConnector>());
            var registry = provider.GetRequiredService<IChannelSchemaRegistry>();

            Assert.False(registry.HasSchema("X", "Y"));
        }

        [Fact]
        public void Should_ReturnEmpty_When_NoConnectorsRegistered()
        {
            var services = new ServiceCollection();
            services.AddMessaging();
            var provider = services.BuildServiceProvider();
            var registry = provider.GetRequiredService<IChannelSchemaRegistry>();

            Assert.Empty(registry.GetSchemas());
        }
    }
}
