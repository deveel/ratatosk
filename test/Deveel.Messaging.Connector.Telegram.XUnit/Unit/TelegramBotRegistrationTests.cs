using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "ConnectorRegistration")]
    public class TelegramBotRegistrationTests
    {
        private IServiceCollection CreateServices() => new ServiceCollection();

        [Fact]
        public void Should_RegisterConnector_When_NoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTelegramBot();

            Assert.Contains(services, d => d.ServiceType == typeof(TelegramBotConnector));
        }

        [Fact]
        public void Should_RegisterConnectorAsSingleton_When_ConfigureShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTelegramBot(c =>
            {
                c.WithSetting("BotToken", "test-token");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(TelegramBotConnector) &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_RegisterIChannelConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTelegramBot("bot", c =>
            {
                c.WithSetting("BotToken", "test-token");
            });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("bot") == true &&
                d.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_ResolveConnector_When_NamedShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTelegramBot("bot", c =>
            {
                c.WithSetting("BotToken", "test-token");
            });

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredKeyedService<IChannelConnector>("bot");

            Assert.NotNull(connector);
            Assert.IsType<TelegramBotConnector>(connector);
        }

        [Fact]
        public void Should_RegisterConnector_When_ConnectionStringShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging()
                .AddTelegramBot("BotToken=test-token");

            Assert.Contains(services, d => d.ServiceType == typeof(TelegramBotConnector));
        }

        [Fact]
        public void Should_RegisterNamedConnector_When_NamedNoConfigShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging().AddTelegramBot("bot", _ => { });

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("bot") == true);
        }

        [Fact]
        public void Should_RegisterNamedConnector_When_NamedConnectionStringShortcutIsUsed()
        {
            var services = CreateServices();
            services.AddMessaging()
                .AddTelegramBot("bot", "BotToken=test-token");

            Assert.Contains(services, d =>
                d.ServiceType == typeof(IChannelConnector) &&
                d.ServiceKey?.Equals("bot") == true);
        }
    }
}
