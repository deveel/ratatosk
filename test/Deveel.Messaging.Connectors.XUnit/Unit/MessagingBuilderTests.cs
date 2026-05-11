//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging.XUnit
{
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "MessagingBuilder")]
	public class MessagingBuilderTests
	{
		private IServiceCollection CreateServices() => new ServiceCollection();

		[Fact]
		public void Should_RegisterIChannelSchemaRegistry_When_AddMessagingIsInvoked()
		{
			var services = CreateServices();
			services.AddMessaging();

			Assert.Contains(services, d => d.ServiceType == typeof(IChannelSchemaRegistry));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_AddConnectorWithNonConnectorType()
		{
			var services = CreateServices();
			var builder = services.AddMessaging();

			Assert.Throws<ArgumentException>(() => builder.AddConnector(typeof(string)));
		}

		[Fact]
		public void Should_RegisterConcreteTypeAsSingleton_When_AddConnectorIsInvoked()
		{
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>();

			Assert.Contains(services, d =>
				d.ServiceType == typeof(TestConnector) &&
				d.Lifetime == ServiceLifetime.Singleton);
		}

		[Fact]
		public void Should_RegisterIChannelConnector_When_AddConnectorIsInvoked()
		{
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>();

			// At least one IChannelConnector registration should exist.
			Assert.Contains(services, d =>
				d.ServiceType == typeof(IChannelConnector) &&
				d.Lifetime == ServiceLifetime.Singleton);
		}

		[Fact]
		public void Should_ResolveConnectorByConcretType_When_ServiceProviderIsBuilt()
		{
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>((sp, schema) => new TestConnector(schema));

			var provider = services.BuildServiceProvider();

			var connector = provider.GetRequiredService<TestConnector>();
			Assert.NotNull(connector);
		}

		[Fact]
		public void Should_ResolveConnectorAsIChannelConnector_When_ServiceProviderIsBuilt()
		{
			var services = CreateServices();
			services.AddMessaging().AddConnector<TestConnector>();

			var provider = services.BuildServiceProvider();

			var connectors = provider.GetServices<IChannelConnector>().ToList();
			Assert.Contains(connectors, c => c is TestConnector);
		}

		[Fact]
		public void Should_AccumulateConnectors_When_AddMessagingIsCalledMultipleTimes()
		{
			var services = CreateServices();
			services.AddMessaging().AddConnector<TestConnector>();
			services.AddMessaging().AddConnector<AnotherTestConnector>();

			var provider = services.BuildServiceProvider();

			var connectors = provider.GetServices<IChannelConnector>().ToList();
			Assert.Contains(connectors, c => c is TestConnector);
			Assert.Contains(connectors, c => c is AnotherTestConnector);
		}

		[Fact]
		public void Should_RegisterKeyedConnector_When_AddConnectorWithNameIsInvoked()
		{
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>("marketing");

			var provider = services.BuildServiceProvider();

			var connector = provider.GetRequiredKeyedService<IChannelConnector>("marketing");
			Assert.NotNull(connector);
			Assert.IsType<TestConnector>(connector);
		}

		[Fact]
		public void Should_RegisterNamedConnectorDescriptor_When_AddConnectorWithNameIsInvoked()
		{
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>("marketing");

			var provider = services.BuildServiceProvider();

			var descriptors = provider.GetServices<NamedConnectorDescriptor>().ToList();
			Assert.Contains(descriptors, d => d.Name == "marketing");
		}

		[Fact]
		public void Should_RegisterMultipleNamedConnectors_When_AddConnectorWithNameCalledMultipleTimes()
		{
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>("marketing")
				.AddConnector<TestConnector>("support");

			var provider = services.BuildServiceProvider();

			var marketing = provider.GetRequiredKeyedService<IChannelConnector>("marketing");
			var support   = provider.GetRequiredKeyedService<IChannelConnector>("support");

			Assert.IsType<TestConnector>(marketing);
			Assert.IsType<TestConnector>(support);
		}

		[Fact]
		public void Should_ThrowArgumentException_When_AddConnectorWithNameAndConnectorWithoutSchemaAttribute()
		{
			var services = CreateServices();
			var builder = services.AddMessaging();

			Assert.Throws<ArgumentException>(() =>
				builder.AddConnector("my-connector", typeof(ConnectorWithoutAttribute)));
		}

		[Fact]
		public void Should_ExposeSettings_When_AddConnectorWithNameAndSettings()
		{
			var services = CreateServices();
			var settings = new Dictionary<string, object?> { ["PhoneNumber"] = "+15550001111" };
			services.AddMessaging()
				.AddConnector<TestConnector>("support", settings: settings);

			var provider = services.BuildServiceProvider();

			var descriptor = provider.GetServices<NamedConnectorDescriptor>()
				.First(d => d.Name == "support");
			Assert.Equal("+15550001111", descriptor.GetSetting<string>("PhoneNumber"));
		}

		[Fact]
		public void Should_UseFactory_When_AddConnectorWithNameAndConnectorFactory()
		{
			var factoryCalled = false;
			var services = CreateServices();
			services.AddMessaging()
				.AddConnector<TestConnector>("marketing",
					connectorFactory: (sp, schema) =>
					{
						factoryCalled = true;
						return new TestConnector(schema);
					});

			var provider = services.BuildServiceProvider();

			provider.GetRequiredKeyedService<IChannelConnector>("marketing");
			Assert.True(factoryCalled);
		}

		// ── Test connector types ──────────────────────────────────────────────

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class TestConnector : IChannelConnector
		{
			public TestConnector(IChannelSchema schema) { Schema = schema; }
			public IChannelSchema Schema { get; }
			public ConnectorState State => ConnectorState.Uninitialized;
			public Task<OperationResult<bool>> InitializeAsync(CancellationToken ct) => Task.FromResult(OperationResult<bool>.Success(true));
			public Task<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => Task.FromResult(OperationResult<bool>.Success(true));
			public Task<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotImplementedException();
			public IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotImplementedException();
			public Task ShutdownAsync(CancellationToken ct) => Task.CompletedTask;
		}

		[ChannelSchema(typeof(AnotherTestSchemaFactory))]
		private class AnotherTestConnector : IChannelConnector
		{
			public AnotherTestConnector(IChannelSchema schema) { Schema = schema; }
			public IChannelSchema Schema { get; }
			public ConnectorState State => ConnectorState.Uninitialized;
			public Task<OperationResult<bool>> InitializeAsync(CancellationToken ct) => Task.FromResult(OperationResult<bool>.Success(true));
			public Task<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => Task.FromResult(OperationResult<bool>.Success(true));
			public Task<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotImplementedException();
			public IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotImplementedException();
			public Task ShutdownAsync(CancellationToken ct) => Task.CompletedTask;
		}

		private class ConnectorWithoutAttribute : IChannelConnector
		{
			public ConnectorWithoutAttribute(IChannelSchema schema) { Schema = schema; }
			public IChannelSchema Schema { get; }
			public ConnectorState State => ConnectorState.Uninitialized;
			public Task<OperationResult<bool>> InitializeAsync(CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<bool>> TestConnectionAsync(CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken ct) => throw new NotImplementedException();
			public IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken ct) => throw new NotImplementedException();
			public Task<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken ct) => throw new NotImplementedException();
			public Task ShutdownAsync(CancellationToken ct) => Task.CompletedTask;
		}

		private class TestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema() => new DummySchema("Test", "Test");
		}

		private class AnotherTestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema() => new DummySchema("AnotherProvider", "AnotherType");
		}

		private class DummySchema : IChannelSchema
		{
			public DummySchema(string channelProvider, string channelType)
			{
				ChannelProvider = channelProvider;
				ChannelType = channelType;
			}
			public string ChannelProvider { get; }
			public string ChannelType { get; }
			public string Version => "1.0";
			public string? DisplayName => null;
			public bool IsStrict => false;
			public ChannelCapability Capabilities => ChannelCapability.SendMessages;
			public IReadOnlyList<ChannelEndpointConfiguration> Endpoints => new List<ChannelEndpointConfiguration>();
			public IReadOnlyList<ChannelParameter> Parameters => new List<ChannelParameter>();
			public IReadOnlyList<MessagePropertyConfiguration> MessageProperties => new List<MessagePropertyConfiguration>();
			public IReadOnlyList<MessageContentType> ContentTypes => new List<MessageContentType>();
			public IReadOnlyList<AuthenticationConfiguration> AuthenticationConfigurations => new List<AuthenticationConfiguration>();
		}
	}
}
