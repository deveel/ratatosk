using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ChannelConnectorBuilder")]
    public class ConnectorOptionsTests
    {
        private sealed class TestOptions : IConnectorOptions
        {
            public string? ApiKey { get; set; }
            public int Timeout { get; set; } = 30;

            public ConnectionSettings ToConnectionSettings()
            {
                var settings = new ConnectionSettings();
                if (!string.IsNullOrWhiteSpace(ApiKey))
                    settings.SetParameter("ApiKey", ApiKey);
                if (Timeout != 30)
                    settings.SetParameter("Timeout", Timeout);
                return settings;
            }
        }

        [ChannelSchema(typeof(OptionsTestSchemaFactory))]
        private sealed class OptionsTestConnector : IChannelConnector
        {
            public OptionsTestConnector(IChannelSchema schema, ConnectionSettings? settings = null)
            {
                Schema = schema;
                ConnectionSettings = settings ?? new ConnectionSettings();
            }

            public IChannelSchema Schema { get; }
            public ConnectionSettings ConnectionSettings { get; }
            public ConnectorState State => ConnectorState.Uninitialized;

            public ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken ct) => new(OperationResult<bool>.Success(true));
            public ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => new(OperationResult<bool>.Success(true));
            public ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage m, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch b, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string id, CancellationToken ct) => throw new NotSupportedException();
            public IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage m, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource s, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource s, CancellationToken ct) => throw new NotSupportedException();
            public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotSupportedException();
            public ValueTask ShutdownAsync(CancellationToken ct) => default;
        }

        private sealed class OptionsTestSchemaFactory : IChannelSchemaFactory
        {
            public IChannelSchema CreateSchema() => new OptionsDummySchema("TestProvider", "TestType");
        }

        private sealed class OptionsDummySchema : IChannelSchema
        {
            public OptionsDummySchema(string channelProvider, string channelType)
            {
                ChannelProvider = channelProvider;
                ChannelType = channelType;
            }
            public string ChannelProvider { get; }
            public string ChannelType { get; }
            public string Version => "1.0";
            public string? DisplayName => null;
            public bool IsStrict => false;
            public ChannelCapability Capabilities => ChannelCapability.SendMessages;
            public IReadOnlyList<ChannelEndpointConfiguration> Endpoints => [];
            public IReadOnlyList<ChannelParameter> Parameters => [];
            public IReadOnlyList<MessagePropertyConfiguration> MessageProperties => [];
            public IReadOnlyList<MessageContentType> ContentTypes => [];
            public IReadOnlyList<AuthenticationConfiguration> AuthenticationConfigurations => [];
        }

        [Fact]
        public void WithOptions_ShouldSetSettingsFromOptions()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnector<OptionsTestConnector>(b => b
                    .WithOptions(new TestOptions
                    {
                        ApiKey = "sk-test-123",
                        Timeout = 60
                    }));

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredService<OptionsTestConnector>();

            Assert.Equal("sk-test-123", connector.ConnectionSettings.GetParameter("ApiKey"));
            Assert.Equal(60, connector.ConnectionSettings.GetParameter<int>("Timeout"));
        }

        [Fact]
        public void WithOptions_ShouldMergeWithOtherSettings()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnector<OptionsTestConnector>(b => b
                    .WithSetting("BaseUrl", "https://api.example.com")
                    .WithOptions(new TestOptions { ApiKey = "sk-test-123" }));

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredService<OptionsTestConnector>();

            Assert.Equal("sk-test-123", connector.ConnectionSettings.GetParameter("ApiKey"));
            Assert.Equal("https://api.example.com", connector.ConnectionSettings.GetParameter("BaseUrl"));
        }

        [Fact]
        public void WithOptions_ShouldOverrideConnectionStringSettings()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnector<OptionsTestConnector>(b => b
                    .WithConnectionString("ApiKey=original-key;Timeout=15")
                    .WithOptions(new TestOptions { ApiKey = "overridden-key" }));

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredService<OptionsTestConnector>();

            Assert.Equal("overridden-key", connector.ConnectionSettings.GetParameter("ApiKey"));
            Assert.Equal(15, connector.ConnectionSettings.GetParameter<int>("Timeout"));
        }

        [Fact]
        public void WithOptions_ShouldThrow_WhenOptionsIsNull()
        {
            var services = new ServiceCollection();
            var builder = services.AddMessaging();

            Assert.Throws<ArgumentNullException>(() =>
                builder.AddConnector<OptionsTestConnector>(b => b
                    .WithOptions<TestOptions>(null!)));
        }

        [Fact]
        public void WithOptions_EmptyOptions_ShouldNotSetSettings()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnector<OptionsTestConnector>(b => b
                    .WithOptions(new TestOptions()));

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredService<OptionsTestConnector>();

            Assert.Null(connector.ConnectionSettings.GetParameter("ApiKey"));
        }

        [Fact]
        public void WithOptions_ShouldWorkWithNamedConnectors()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnector<OptionsTestConnector>("primary", b => b
                    .WithOptions(new TestOptions { ApiKey = "key-primary" }))
                .AddConnector<OptionsTestConnector>("secondary", b => b
                    .WithOptions(new TestOptions { ApiKey = "key-secondary" }));

            var provider = services.BuildServiceProvider();
            var primary = provider.GetRequiredKeyedService<IChannelConnector>("primary") as OptionsTestConnector;
            var secondary = provider.GetRequiredKeyedService<IChannelConnector>("secondary") as OptionsTestConnector;

            Assert.NotNull(primary);
            Assert.NotNull(secondary);
            Assert.Equal("key-primary", primary.ConnectionSettings.GetParameter("ApiKey"));
            Assert.Equal("key-secondary", secondary.ConnectionSettings.GetParameter("ApiKey"));
        }
    }
}
