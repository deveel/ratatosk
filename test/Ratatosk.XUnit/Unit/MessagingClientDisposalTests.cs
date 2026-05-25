using Ratatosk.XUnit.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk.XUnit.Unit
{
    [Trait("Category", "Integration")]
    [Trait("Feature", "MessagingClient")]
    public class MessagingClientDisposalTests
    {
        private static IServiceProvider CreateClient()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnector<MockConnector>("mock", _ => { })
                .AddClient();
            return services.BuildServiceProvider();
        }

        [Fact]
        public void Dispose_ShouldTransitionConnectorToShutdown()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();
            var connector = provider.GetRequiredKeyedService<IChannelConnector>("mock") as MockConnector;

            Assert.NotNull(connector);

            // Trigger initialization
            var message = new MessageBuilder().WithId("x").WithText("test").Build();
            client.SendAsync("mock", message).GetAwaiter().GetResult();
            Assert.Equal(ConnectorState.Ready, connector.State);

            // Dispose
            client.Dispose();

            Assert.Equal(ConnectorState.Shutdown, connector.State);
        }

        [Fact]
        public void Dispose_ShouldAllowMultipleCalls()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();

            client.Dispose();
            client.Dispose();
        }

        [Fact]
        public async Task DisposeAsync_ShouldTransitionConnectorToShutdown()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();
            var connector = provider.GetRequiredKeyedService<IChannelConnector>("mock") as MockConnector;

            Assert.NotNull(connector);

            var message = new MessageBuilder().WithId("x").WithText("test").Build();
            await client.SendAsync("mock", message);
            Assert.Equal(ConnectorState.Ready, connector.State);

            await ((IAsyncDisposable)client).DisposeAsync();

            Assert.Equal(ConnectorState.Shutdown, connector.State);
        }

        [Fact]
        public async Task DisposeAsync_ShouldAllowMultipleCalls()
        {
            var provider = CreateClient();
            var client = provider.GetRequiredService<IMessagingClient>();

            await ((IAsyncDisposable)client).DisposeAsync();
            await ((IAsyncDisposable)client).DisposeAsync();
        }

        [Fact]
        public async Task UsingStatement_ShouldShutdownConnectors()
        {
            var provider = CreateClient();

            MockConnector? captured = null;

            await using (var client = provider.GetRequiredService<IMessagingClient>())
            {
                var connector = provider.GetRequiredKeyedService<IChannelConnector>("mock") as MockConnector;
                Assert.NotNull(connector);
                captured = connector;

                var message = new MessageBuilder().WithId("x").WithText("test").Build();
                await client.SendAsync("mock", message);
                Assert.Equal(ConnectorState.Ready, connector.State);
            }

            Assert.NotNull(captured);
            Assert.Equal(ConnectorState.Shutdown, captured.State);
        }

        [Fact]
        public async Task Dispose_ShouldShutdownAllCachedConnectors()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnector<MockConnector>("first", _ => { })
                .AddConnector<MockConnector>("second", _ => { })
                .AddClient();

            var provider = services.BuildServiceProvider();
            var client = provider.GetRequiredService<IMessagingClient>();
            var firstConnector = provider.GetRequiredKeyedService<IChannelConnector>("first") as MockConnector;
            var secondConnector = provider.GetRequiredKeyedService<IChannelConnector>("second") as MockConnector;

            Assert.NotNull(firstConnector);
            Assert.NotNull(secondConnector);

            var message = new MessageBuilder().WithId("x").WithText("test").Build();
            await client.SendAsync("first", message);
            await client.ReceiveAsync("second", MessageSource.Json("{}"));

            client.Dispose();

            Assert.Equal(ConnectorState.Shutdown, firstConnector.State);
            Assert.Equal(ConnectorState.Shutdown, secondConnector.State);
        }
    }
}
