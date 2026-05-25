using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ConnectorRegistration")]
    public class SendGridEmailRegistrationTests
    {
        private IServiceCollection CreateServices() => new ServiceCollection();

        [Fact]
        public void Should_RegisterConnector_When_NoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddSendGridEmail();

            Assert.Contains(services, d => d.ServiceType == typeof(SendGridEmailConnector));
        }

        [Fact]
        public void Should_RegisterConnectorAsSingleton_When_ConfigureShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddSendGridEmail(c =>
            {
                c.WithSetting("ApiKey", "test-key");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(SendGridEmailConnector) &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_RegisterIChannelConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddSendGridEmail("email", c =>
            {
                c.WithSetting("ApiKey", "test-key");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("email") == true &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_ResolveConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddSendGridEmail("email", c =>
            {
                c.WithSetting("ApiKey", "test-key");
            });

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredKeyedService<IChannelConnector>("email");

            Assert.NotNull(connector);
            Assert.IsType<SendGridEmailConnector>(connector);
        }

        [Fact]
        public void Should_RegisterConnector_When_ConnectionStringShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging()
                .AddSendGridEmail("ApiKey=test-key");

            Assert.Contains(services, d => d.ServiceType == typeof(SendGridEmailConnector));
        }

        [Fact]
        public void Should_RegisterNamedConnector_When_NamedNoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddSendGridEmail("email", _ => { });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("email") == true);
        }
    }
}
