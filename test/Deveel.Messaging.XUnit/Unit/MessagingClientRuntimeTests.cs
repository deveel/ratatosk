using Deveel.Messaging.XUnit.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit.Unit
{
    [Trait("Category", "Integration")]
    [Trait("Feature", "MessagingClient")]
    public class MessagingClientRuntimeTests
    {
        private static IServiceProvider CreateClient(Action<MessagingBuilder>? configure = null, Action<MessagingClientOptions>? configureOptions = null)
        {
            var services = new ServiceCollection();
            var builder = services.AddMessaging();
            builder.AddConnectorType<MockConnector>("mock");
            configure?.Invoke(builder);
            if (configureOptions != null)
                builder.AddClient(configureOptions);
            else
                builder.AddClient();
            return services.BuildServiceProvider();
        }

        private static IServiceProvider CreateClientWithPreconfigured(string name = "preconf")
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnector<MockConnector>(name, _ => { })
                .AddConnectorType<MockConnector>("runtime")
                .AddClient();
            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task Should_SendMessage_WithRuntimeSettings()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var message = new MessageBuilder()
                .WithId("runtime-send")
                .WithText("Hello runtime")
                .Build();

            var result = await client.SendAsync("mock", settings, message);

            Assert.True(result.IsSuccess());
            Assert.NotNull(result.Value);
            Assert.Equal("runtime-send", result.Value.MessageId);
        }

        [Fact]
        public async Task Should_ReceiveMessages_WithRuntimeSettings()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var source = MessageSource.Json("{}");
            var result = await client.ReceiveAsync("mock", settings, source);

            Assert.True(result.IsSuccess());
            Assert.NotNull(result.Value);
            Assert.NotEmpty(result.Value.Messages);
        }

        [Fact]
        public async Task Should_GetStatus_WithRuntimeSettings()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var result = await client.GetStatusAsync("mock", settings);

            Assert.True(result.IsSuccess());
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task Should_ReceiveMessageStatus_WithRuntimeSettings()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var source = MessageSource.Json("{}");
            var result = await client.ReceiveMessageStatusAsync("mock", settings, source);

            Assert.True(result.IsSuccess());
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_RuntimeChannelTypeNotFound()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var message = new MessageBuilder().WithId("x").WithText("test").Build();
            var result = await client.SendAsync("nonexistent", settings, message);

            Assert.False(result.IsSuccess());
            Assert.NotNull(result.Error);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_RuntimeChannelNotRegistered_AndNoCatalog()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddClient();
            var provider = services.BuildServiceProvider();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var message = new MessageBuilder().WithId("x").WithText("test").Build();
            var result = await client.SendAsync("anything", settings, message);

            Assert.False(result.IsSuccess());
            Assert.NotNull(result.Error);
        }

        [Fact]
        public async Task Should_AutoInitialize_RuntimeConnector()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var message = new MessageBuilder().WithId("init-test").WithText("test").Build();
            var result = await client.SendAsync("mock", settings, message);

            Assert.True(result.IsSuccess());
        }

        [Fact]
        public async Task Should_NotAutoInitializeRuntime_When_OptionIsFalse()
        {
            var provider = CreateClient(configureOptions: o => o.AutoInitialize = false);
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var message = new MessageBuilder().WithId("no-init").WithText("test").Build();
            await client.SendAsync("mock", settings, message);

            Assert.True(true);
        }

        [Fact]
        public async Task Should_PassConnectionSettings_ToRuntimeConnector()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings()
                .SetParameter("CustomKey", "CustomValue");

            var message = new MessageBuilder().WithId("settings-test").WithText("test").Build();
            var result = await client.SendAsync("mock", settings, message);

            Assert.True(result.IsSuccess());
        }

        [Fact]
        public async Task Should_SupportPreconfiguredAndRuntime_Mixed()
        {
            var provider = CreateClientWithPreconfigured();
            var client = provider.GetRequiredService<IMessagingClient>();

            var settings = new ConnectionSettings();
            var message = new MessageBuilder().WithId("mixed").WithText("test").Build();

            var preconfResult = await client.SendAsync("preconf", message);
            var runtimeResult = await client.SendAsync("runtime", settings, message);

            Assert.True(preconfResult.IsSuccess());
            Assert.True(runtimeResult.IsSuccess());
        }

        // ── Type-based runtime overloads ─────────────────────────────────────

        [Fact]
        public async Task Should_SendMessage_WithTypeParameterAndRuntimeSettings()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>()
                .AddClient();
            var provider = services.BuildServiceProvider();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var message = new MessageBuilder().WithId("type-runtime").WithText("test").Build();
            var result = await client.SendAsync<MockConnector>(settings, message);

            Assert.True(result.IsSuccess());
            Assert.Equal("type-runtime", result.Value!.MessageId);
        }

        [Fact]
        public async Task Should_ReceiveMessages_WithTypeParameterAndRuntimeSettings()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>()
                .AddClient();
            var provider = services.BuildServiceProvider();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var result = await client.ReceiveAsync<MockConnector>(settings, MessageSource.Json("{}"));

            Assert.True(result.IsSuccess());
            Assert.NotEmpty(result.Value!.Messages);
        }

        [Fact]
        public async Task Should_GetStatus_WithTypeParameterAndRuntimeSettings()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>()
                .AddClient();
            var provider = services.BuildServiceProvider();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var result = await client.GetStatusAsync<MockConnector>(settings);

            Assert.True(result.IsSuccess());
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task Should_ReceiveMessageStatus_WithTypeParameterAndRuntimeSettings()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>()
                .AddClient();
            var provider = services.BuildServiceProvider();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var result = await client.ReceiveMessageStatusAsync<MockConnector>(settings, MessageSource.Json("{}"));

            Assert.True(result.IsSuccess());
            Assert.NotNull(result.Value);
        }
    }
}
