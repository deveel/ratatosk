using Deveel.Messaging.XUnit.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ServiceProviderConnectorResolver")]
    public class ServiceProviderConnectorResolverTests
    {
        private static IServiceProvider CreateProvider(Action<MessagingBuilder>? configure = null)
        {
            var services = new ServiceCollection();
            var builder = services.AddMessaging();
            builder.AddConnector<MockConnector>("mock", _ => { });
            configure?.Invoke(builder);
            builder.AddClient();
            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task Should_ResolveExistingConnector()
        {
            var provider = CreateProvider();
            var resolver = provider.GetRequiredService<IChannelConnectorResolver>();

            var connector = await resolver.ResolveAsync("mock");

            Assert.NotNull(connector);
            Assert.IsAssignableFrom<MockConnector>(connector);
        }

        [Fact]
        public async Task Should_ReturnNull_When_ChannelNotFound()
        {
            var provider = CreateProvider();
            var resolver = provider.GetRequiredService<IChannelConnectorResolver>();

            var connector = await resolver.ResolveAsync("nonexistent");

            Assert.Null(connector);
        }

        [Fact]
        public async Task Should_BeRegistered_ByAddClient()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddClient();

            var provider = services.BuildServiceProvider();
            var resolver = provider.GetService<IChannelConnectorResolver>();

            Assert.NotNull(resolver);
            Assert.IsAssignableFrom<ServiceProviderConnectorResolver>(resolver);
        }

        [Fact]
        public async Task Should_ResolveUsingExactName()
        {
            var provider = CreateProvider();
            var resolver = provider.GetRequiredService<IChannelConnectorResolver>();

            var connector = await resolver.ResolveAsync("mock");

            Assert.NotNull(connector);
        }

        [Fact]
        public async Task Should_ReturnNull_When_NameCaseDiffers()
        {
            var provider = CreateProvider();
            var resolver = provider.GetRequiredService<IChannelConnectorResolver>();

            var connector = await resolver.ResolveAsync("MOCK");

            Assert.Null(connector);
        }
    }
}
