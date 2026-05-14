using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ConnectorRegistration")]
    public class TwilioWhatsAppRegistrationTests
    {
        private IServiceCollection CreateServices() => new ServiceCollection();

        [Fact]
        public void Should_RegisterConnector_When_NoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTwilioWhatsApp();

            Assert.Contains(services, d => d.ServiceType == typeof(TwilioWhatsAppConnector));
        }

        [Fact]
        public void Should_RegisterConnectorAsSingleton_When_ConfigureShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTwilioWhatsApp(c =>
            {
                c.WithSetting("AccountSid", "AC_test");
                c.WithSetting("AuthToken", "test-token");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(TwilioWhatsAppConnector) &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_RegisterIChannelConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTwilioWhatsApp("wa", c =>
            {
                c.WithSetting("AccountSid", "AC_test");
                c.WithSetting("AuthToken", "test-token");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("wa") == true &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_ResolveConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTwilioWhatsApp("wa", c =>
            {
                c.WithSetting("AccountSid", "AC_test");
                c.WithSetting("AuthToken", "test-token");
            });

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredKeyedService<IChannelConnector>("wa");

            Assert.NotNull(connector);
            Assert.IsType<TwilioWhatsAppConnector>(connector);
        }

        [Fact]
        public void Should_RegisterConnector_When_ConnectionStringShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging()
                .AddTwilioWhatsApp("AccountSid=AC_test;AuthToken=test-token");

            Assert.Contains(services, d => d.ServiceType == typeof(TwilioWhatsAppConnector));
        }

        [Fact]
        public void Should_RegisterNamedConnector_When_NamedNoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTwilioWhatsApp("wa", _ => { });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("wa") == true);
        }
    }
}
