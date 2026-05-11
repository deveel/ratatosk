//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace Deveel.Messaging.XUnit
{
	/// <summary>
	/// Tests for the IChannelSchemaRegistry registration and behaviour.
	/// </summary>
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ChannelSchemaRegistry")]
	public class ChannelSchemaRegistryTests
	{
		// ── registration tests ────────────────────────────────────────────────

		[Fact]
		public void Should_RegisterIChannelSchemaRegistryAsSingleton_When_AddMessagingIsInvoked()
		{
			var services = new ServiceCollection();
			services.AddMessaging();

			Assert.Contains(services, d =>
				d.ServiceType == typeof(IChannelSchemaRegistry) &&
				d.Lifetime == ServiceLifetime.Singleton);
		}

		[Fact]
		public void Should_RegisterChannelSchemaRegistryOnlyOnce_When_AddMessagingCalledMultipleTimes()
		{
			var services = new ServiceCollection();
			services.AddMessaging();
			services.AddMessaging();

			var registrations = services.Where(d => d.ServiceType == typeof(IChannelSchemaRegistry)).ToList();
			Assert.Single(registrations);
		}

		[Fact]
		public void Should_ResolveIChannelSchemaRegistry_When_ServiceProviderIsBuilt()
		{
			var services = new ServiceCollection();
			services.AddMessaging();

			var provider = services.BuildServiceProvider();

			var registry = provider.GetService<IChannelSchemaRegistry>();
			Assert.NotNull(registry);
		}

		// ── schema from AddConnector ──────────────────────────────────────────

		[Fact]
		public void Should_ReturnSchema_When_ConnectorRegisteredViaAddConnector()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>();

			var provider = services.BuildServiceProvider();
			var schemaRegistry = provider.GetRequiredService<IChannelSchemaRegistry>();
			var schemas = schemaRegistry.GetSchemas().ToList();

			Assert.Single(schemas);
			Assert.Equal("TestProvider", schemas[0].ChannelProvider);
			Assert.Equal("TestType", schemas[0].ChannelType);
		}

		[Fact]
		public void Should_ReturnAllSchemas_When_MultipleConnectorsRegisteredViaAddConnector()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>()
				.AddConnector<AnotherTestConnector>();

			var provider = services.BuildServiceProvider();
			var schemaRegistry = provider.GetRequiredService<IChannelSchemaRegistry>();
			var schemas = schemaRegistry.GetSchemas().ToList();

			Assert.Equal(2, schemas.Count);
		}

		[Fact]
		public void Should_FindSchema_When_ConnectorRegisteredViaAddConnector()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>();

			var provider = services.BuildServiceProvider();
			var schemaRegistry = provider.GetRequiredService<IChannelSchemaRegistry>();

			var schema = schemaRegistry.FindSchema("TestProvider", "TestType");
			Assert.NotNull(schema);
			Assert.Equal("TestProvider", schema!.ChannelProvider);
		}

		[Fact]
		public void Should_ReturnNull_When_SchemaNotRegistered()
		{
			var services = new ServiceCollection();
			services.AddMessaging();

			var provider = services.BuildServiceProvider();
			var schemaRegistry = provider.GetRequiredService<IChannelSchemaRegistry>();

			var schema = schemaRegistry.FindSchema("Unknown", "Unknown");
			Assert.Null(schema);
		}

		[Fact]
		public void Should_ReturnTrue_When_HasSchemaForRegisteredConnector()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>();

			var provider = services.BuildServiceProvider();
			var schemaRegistry = provider.GetRequiredService<IChannelSchemaRegistry>();
			Assert.True(schemaRegistry.HasSchema("TestProvider", "TestType"));
		}

		[Fact]
		public void Should_ReturnFalse_When_HasSchemaForUnregisteredConnector()
		{
			var services = new ServiceCollection();
			services.AddMessaging();

			var provider = services.BuildServiceProvider();
			var schemaRegistry = provider.GetRequiredService<IChannelSchemaRegistry>();
			Assert.False(schemaRegistry.HasSchema("Unknown", "Unknown"));
		}

		[Fact]
		public void Should_FindSchema_CaseInsensitive_When_LookingUpSchema()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>();

			var provider = services.BuildServiceProvider();
			var schemaRegistry = provider.GetRequiredService<IChannelSchemaRegistry>();

			Assert.NotNull(schemaRegistry.FindSchema("TESTPROVIDER", "TESTTYPE"));
			Assert.NotNull(schemaRegistry.FindSchema("testprovider", "testtype"));
		}

		// ── schema from directly registered IChannelConnector ─────────────────

		[Fact]
		public void Should_ReturnSchema_When_ConnectorRegisteredDirectlyInDI()
		{
			var services = new ServiceCollection();
			services.AddMessaging();
			services.AddSingleton<IChannelConnector>(new DirectTestConnector());

			var provider = services.BuildServiceProvider();

			var schemaRegistry = provider.GetRequiredService<IChannelSchemaRegistry>();
			var schemas = schemaRegistry.GetSchemas().ToList();

			Assert.Single(schemas);
			Assert.Equal("DirectProvider", schemas[0].ChannelProvider);
		}

		[Fact]
		public void Should_DeduplicateSchemas_When_SameConnectorInBothSources()
		{
			// Register TestConnector both via AddConnector and as a direct DI singleton with same schema identity.
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>();
			// Simulate a direct registration of a connector with the same schema identity
			services.AddSingleton<IChannelConnector>(new DirectDuplicateTestConnector());

			var provider = services.BuildServiceProvider();
			var schemaRegistry = provider.GetRequiredService<IChannelSchemaRegistry>();
			var schemas = schemaRegistry.GetSchemas().ToList();

			// DirectDuplicateTestConnector shares provider/type with TestConnector → deduplicated
			Assert.Single(schemas);
		}

		[Fact]
		public void Should_CombineSchemas_When_ConnectorInBothSources()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>();
			services.AddSingleton<IChannelConnector>(new DirectTestConnector());

			var provider = services.BuildServiceProvider();
			var schemaRegistry = provider.GetRequiredService<IChannelSchemaRegistry>();
			var schemas = schemaRegistry.GetSchemas().ToList();

			Assert.Equal(2, schemas.Count);
		}

		// ── argument validation ───────────────────────────────────────────────

		[Fact]
		public void Should_ThrowArgumentException_When_FindSchemaWithNullProvider()
		{
			var services = new ServiceCollection();
			services.AddMessaging();
			var provider = services.BuildServiceProvider();

			var registry = provider.GetRequiredService<IChannelSchemaRegistry>();
			Assert.Throws<ArgumentNullException>(() => registry.FindSchema(null!, "TestType"));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_FindSchemaWithNullType()
		{
			var services = new ServiceCollection();
			services.AddMessaging();
			var provider = services.BuildServiceProvider();

			var registry = provider.GetRequiredService<IChannelSchemaRegistry>();
			Assert.Throws<ArgumentNullException>(() => registry.FindSchema("TestProvider", null!));
		}

		// ── test connector types ──────────────────────────────────────────────

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class TestConnector : ChannelConnectorBase
		{
			public TestConnector(IChannelSchema schema, ConnectionSettings? settings = null) : base(schema, settings) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Ready);
				return ValueTask.CompletedTask;
			}
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotImplementedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
		}

		[ChannelSchema(typeof(AnotherTestSchemaFactory))]
		private class AnotherTestConnector : ChannelConnectorBase
		{
			public AnotherTestConnector(IChannelSchema schema, ConnectionSettings? settings = null) : base(schema, settings) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Ready);
				return ValueTask.CompletedTask;
			}
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotImplementedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
		}

		/// <summary>A connector registered directly in the DI container with a distinct schema.</summary>
		private sealed class DirectTestConnector : IChannelConnector
		{
			public IChannelSchema Schema { get; } = new ChannelSchema("DirectProvider", "DirectType", "1.0.0")
				.WithCapabilities(ChannelCapability.SendMessages)
				.HandlesMessageEndpoint(EndpointType.EmailAddress)
				.AddContentType(MessageContentType.PlainText);

			public ConnectorState State => ConnectorState.Ready;
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

		/// <summary>Same schema identity as TestConnector — used to test deduplication.</summary>
		private sealed class DirectDuplicateTestConnector : IChannelConnector
		{
			public IChannelSchema Schema { get; } = new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithCapabilities(ChannelCapability.SendMessages)
				.HandlesMessageEndpoint(EndpointType.PhoneNumber)
				.AddContentType(MessageContentType.PlainText);

			public ConnectorState State => ConnectorState.Ready;
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

		private class TestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema() =>
				new ChannelSchema("TestProvider", "TestType", "1.0.0")
					.WithCapabilities(ChannelCapability.SendMessages)
					.HandlesMessageEndpoint(EndpointType.PhoneNumber)
					.AddContentType(MessageContentType.PlainText);
		}

		private class AnotherTestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema() =>
				new ChannelSchema("AnotherProvider", "AnotherType", "1.0.0")
					.WithCapabilities(ChannelCapability.SendMessages)
					.HandlesMessageEndpoint(EndpointType.EmailAddress)
					.AddContentType(MessageContentType.Html);
		}
	}
}

