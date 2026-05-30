using System.ComponentModel.DataAnnotations;
using Ratatosk.XUnit.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk.XUnit.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "AddConnectorType")]
    public class AddConnectorTypeRegistrationTests
    {
        [Fact]
        public void Should_RegisterConnectorType()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>("mock");

            Assert.Contains(services, d =>
                d.ServiceType == typeof(ConnectorTypeRegistration) &&
                d.ImplementationInstance is ConnectorTypeRegistration reg &&
                reg.Name == "mock" &&
                reg.ConnectorType == typeof(MockConnector));
        }

        [Fact]
        public void Should_RegisterResolver_When_AddClientCalled()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>("mock")
                .AddClient();

            Assert.Contains(services, d => d.ServiceType == typeof(IChannelConnectorResolver));
        }

        [Fact]
        public void Should_RegisterCatalog_When_ConnectorTypeAdded()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>("mock")
                .AddClient();

            Assert.Contains(services, d => d.ServiceType == typeof(ConnectorTypeCatalog));
        }

        [Fact]
        public void Should_NotRegisterCatalog_When_NoConnectorTypeAdded()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddClient();

            Assert.DoesNotContain(services, d => d.ServiceType == typeof(ConnectorTypeCatalog));
        }

        [Fact]
        public void Should_RegisterDefaultFactory_ForConnectorType()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>("mock")
                .AddClient();

            var provider = services.BuildServiceProvider();
            var factory = provider.GetService<IChannelConnectorFactory<MockConnector>>();

            Assert.NotNull(factory);
        }

        [Fact]
        public void Should_ResolveRuntimeConnector_ThroughClient()
        {
            var provider = CreateProvider();
            var client = provider.GetRequiredService<IMessagingClient>();
            var settings = new ConnectionSettings();

            var message = new MessageBuilder().WithId("reg-test").WithText("test").Build();
            var result = client.SendAsync("mock", settings, message).GetAwaiter().GetResult();

            Assert.True(result.IsSuccess());
        }

        [Fact]
        public void Should_SupportMultipleConnectorTypes()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>("first")
                .AddConnectorType<MockConnector>("second");

            var registrations = services
                .Where(d => d.ServiceType == typeof(ConnectorTypeRegistration))
                .Select(d => (ConnectorTypeRegistration)d.ImplementationInstance!)
                .ToList();

            Assert.Equal(2, registrations.Count);
            Assert.Contains(registrations, r => r.Name == "first");
            Assert.Contains(registrations, r => r.Name == "second");
        }

        [Fact]
        public void Should_Throw_When_ConnectorTypeLacksChannelSchema()
        {
            var services = new ServiceCollection();
            var builder = services.AddMessaging();

            Assert.Throws<ArgumentException>(() =>
                builder.AddConnectorType<InvalidConnector>("bad"));
        }

        [Fact]
        public void Should_Throw_When_NameIsNull()
        {
            var services = new ServiceCollection();
            var builder = services.AddMessaging();

            Assert.Throws<ArgumentNullException>(() =>
                builder.AddConnectorType<MockConnector>(null!));
        }

        [Fact]
        public void Should_PopulateCatalog_WithAllRegisteredTypes()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>("first")
                .AddConnectorType<MockConnector>("second")
                .AddClient();

            var provider = services.BuildServiceProvider();
            var catalog = provider.GetRequiredService<ConnectorTypeCatalog>();

            Assert.True(catalog.TryGetEntry("first", out _));
            Assert.True(catalog.TryGetEntry("second", out _));
        }

        [Fact]
        public void Should_NotThrow_When_AddConnectorTypeUsed_WithoutAddClient()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>("mock");

            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider);
        }

        [Fact]
        public void Should_ResolveWithServiceProviderResolver_When_OnlyAddConnectorTypeUsed()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>("mock")
                .AddClient();

            var provider = services.BuildServiceProvider();
            var resolver = provider.GetRequiredService<IChannelConnectorResolver>();

            Assert.IsAssignableFrom<ServiceProviderConnectorResolver>(resolver);
        }

        [Fact]
        public void Should_RegisterConnectorType_WithoutName()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>();

            Assert.Contains(services, d =>
                d.ServiceType == typeof(ConnectorTypeRegistration) &&
                d.ImplementationInstance is ConnectorTypeRegistration reg &&
                reg.Name == "MockConnector" &&
                reg.ConnectorType == typeof(MockConnector));
        }

        [Fact]
        public void Should_PopulateCatalog_WithNamelessType()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>()
                .AddClient();

            var provider = services.BuildServiceProvider();
            var catalog = provider.GetRequiredService<ConnectorTypeCatalog>();

            Assert.True(catalog.TryGetEntry("MockConnector", out _));
        }

        [Fact]
        public void Should_LookupInCatalog_ByType()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>("named")
                .AddClient();

            var provider = services.BuildServiceProvider();
            var catalog = provider.GetRequiredService<ConnectorTypeCatalog>();

            Assert.True(catalog.TryGetEntry(typeof(MockConnector), out var entry));
            Assert.NotNull(entry);
            Assert.Equal("named", entry!.Name);
        }

        [Fact]
        public void Should_ReturnFalse_When_LookupByTypeNotFound()
        {
            var catalog = new ConnectorTypeCatalog();

            var found = catalog.TryGetEntry(typeof(MockConnector), out var entry);

            Assert.False(found);
            Assert.Null(entry);
        }

        private static IServiceProvider CreateProvider()
        {
            var services = new ServiceCollection();
            services.AddMessaging()
                .AddConnectorType<MockConnector>("mock")
                .AddClient();
            return services.BuildServiceProvider();
        }

        private class InvalidConnector : IChannelConnector
        {
            public IChannelSchema Schema => throw new NotSupportedException();
            public ConnectorState State => ConnectorState.Uninitialized;

            public ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken cancellationToken)
                => new(OperationResult<bool>.Fail("X", "X", "X"));

            public ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                yield break;
            }

            public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public ValueTask ShutdownAsync(CancellationToken cancellationToken)
                => default;
        }
    }
}
