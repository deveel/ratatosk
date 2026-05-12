using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ConnectorRegistration")]
    public class FirebasePushRegistrationTests
    {
        private IServiceCollection CreateServices() => new ServiceCollection();

        [Fact]
        public void Should_RegisterConnector_When_NoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddFirebasePush();

            Assert.Contains(services, d => d.ServiceType == typeof(FirebasePushConnector));
        }

        [Fact]
        public void Should_RegisterConnectorAsSingleton_When_ConfigureShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddFirebasePush(c =>
            {
                c.WithSetting("ProjectId", "test-project");
                c.WithSetting("ServiceAccountKey", "{\"test\":\"key\"}");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(FirebasePushConnector) &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_RegisterIChannelConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddFirebasePush("fcm", c =>
            {
                c.WithSetting("ProjectId", "test-project");
                c.WithSetting("ServiceAccountKey", "{\"test\":\"key\"}");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("fcm") == true &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_ResolveConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddFirebasePush("fcm", c =>
            {
                c.WithSetting("ProjectId", "test-project");
                c.WithSetting("ServiceAccountKey", "{\"test\":\"key\"}");
            });

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredKeyedService<IChannelConnector>("fcm");

            Assert.NotNull(connector);
            Assert.IsType<FirebasePushConnector>(connector);
        }

        [Fact]
        public void Should_RegisterConnector_When_ConnectionStringShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging()
                .AddFirebasePush("ProjectId=test;ServiceAccountKey={\"key\":\"val\"}");

            Assert.Contains(services, d => d.ServiceType == typeof(FirebasePushConnector));
        }

        [Fact]
        public void Should_RegisterNamedConnector_When_NamedNoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddFirebasePush("fcm", _ => { });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("fcm") == true);
        }
    }
}
