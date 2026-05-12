using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ConnectorRegistration")]
    public class FacebookMessengerRegistrationTests
    {
        private IServiceCollection CreateServices() => new ServiceCollection();

        [Fact]
        public void Should_RegisterConnector_When_NoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddFacebookMessenger();

            Assert.Contains(services, d => d.ServiceType == typeof(FacebookMessengerConnector));
        }

        [Fact]
        public void Should_RegisterConnectorAsSingleton_When_ConfigureShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddFacebookMessenger(c =>
            {
                c.WithSetting("PageAccessToken", "test-token");
                c.WithSetting("PageId", "test-page");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(FacebookMessengerConnector) &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_RegisterIChannelConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddFacebookMessenger("fb", c =>
            {
                c.WithSetting("PageAccessToken", "test-token");
                c.WithSetting("PageId", "test-page");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("fb") == true &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_ResolveConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddFacebookMessenger("fb", c =>
            {
                c.WithSetting("PageAccessToken", "test-token");
                c.WithSetting("PageId", "test-page");
            });

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredKeyedService<IChannelConnector>("fb");

            Assert.NotNull(connector);
            Assert.IsType<FacebookMessengerConnector>(connector);
        }

        [Fact]
        public void Should_RegisterConnector_When_ConnectionStringShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging()
                .AddFacebookMessenger("PageAccessToken=test-token;PageId=test-page");

            Assert.Contains(services, d => d.ServiceType == typeof(FacebookMessengerConnector));
        }

        [Fact]
        public void Should_RegisterNamedConnector_When_NamedNoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddFacebookMessenger("fb", _ => { });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("fb") == true);
        }
    }
}
