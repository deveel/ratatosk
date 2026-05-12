using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ConnectorRegistration")]
    public class TwilioSmsRegistrationTests
    {
        private IServiceCollection CreateServices() => new ServiceCollection();

        [Fact]
        public void Should_RegisterConnector_When_NoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTwilioSms();

            Assert.Contains(services, d => d.ServiceType == typeof(TwilioSmsConnector));
        }

        [Fact]
        public void Should_RegisterConnectorAsSingleton_When_ConfigureShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTwilioSms(c =>
            {
                c.WithSetting("AccountSid", "AC_test");
                c.WithSetting("AuthToken", "test-token");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(TwilioSmsConnector) &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_RegisterIChannelConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTwilioSms("sms", c =>
            {
                c.WithSetting("AccountSid", "AC_test");
                c.WithSetting("AuthToken", "test-token");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("sms") == true &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_ResolveConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTwilioSms("sms", c =>
            {
                c.WithSetting("AccountSid", "AC_test");
                c.WithSetting("AuthToken", "test-token");
            });

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredKeyedService<IChannelConnector>("sms");

            Assert.NotNull(connector);
            Assert.IsType<TwilioSmsConnector>(connector);
        }

        [Fact]
        public void Should_RegisterConnector_When_ConnectionStringShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging()
                .AddTwilioSms("AccountSid=AC_test;AuthToken=test-token");

            Assert.Contains(services, d => d.ServiceType == typeof(TwilioSmsConnector));
        }

        [Fact]
        public void Should_RegisterNamedConnector_When_NamedNoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTwilioSms("sms", _ => { });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("sms") == true);
        }
    }
}
