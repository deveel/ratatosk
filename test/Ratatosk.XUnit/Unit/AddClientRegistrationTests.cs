using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "AddClient")]
    public class AddClientRegistrationTests
    {
        [Fact]
        public void Should_RegisterIMessagingClient_When_AddClientIsCalled()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddClient();

            Assert.Contains(services, d => d.ServiceType == typeof(IMessagingClient));
        }

        [Fact]
        public void Should_RegisterIMessagingClientAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddClient();

            var registration = services.First(d => d.ServiceType == typeof(IMessagingClient));
            Assert.Equal(ServiceLifetime.Singleton, registration.Lifetime);
        }

        [Fact]
        public void Should_RegisterMessagingClientOptions()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddClient();

            Assert.Contains(services, d => d.ServiceType == typeof(MessagingClientOptions));
        }

        [Fact]
        public void Should_ApplyOptions_When_AddClientWithConfigureIsCalled()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddClient(o => o.AutoInitialize = false);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<MessagingClientOptions>();

            Assert.False(options.AutoInitialize);
        }

        [Fact]
        public void Should_DefaultAutoInitializeToTrue()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddClient();

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<MessagingClientOptions>();

            Assert.True(options.AutoInitialize);
        }

        [Fact]
        public void Should_ResolveIMessagingClient_When_AddClientIsCalled()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddClient();

            var provider = services.BuildServiceProvider();
            var client = provider.GetRequiredService<IMessagingClient>();

            Assert.NotNull(client);
            Assert.IsAssignableFrom<IMessagingClient>(client);
        }

        [Fact]
        public void Should_RegisterSingleInstance_When_AddClientIsCalledMultipleTimes()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddClient()
                .AddClient();

            var registrations = services.Where(d => d.ServiceType == typeof(IMessagingClient)).ToList();
            Assert.Single(registrations);
        }
    }
}
