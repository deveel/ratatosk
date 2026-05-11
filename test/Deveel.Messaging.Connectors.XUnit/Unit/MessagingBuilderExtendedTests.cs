// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Deveel.Messaging.XUnit
{
	/// <summary>
	/// Extended tests for MessagingBuilder covering edge cases and error handling.
	/// </summary>
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "MessagingBuilder")]
	public class MessagingBuilderExtendedTests
	{
		[Fact]
		public void Should_ThrowArgumentNullException_When_AddMessagingWithNullServices()
		{
			Assert.Throws<ArgumentNullException>(() =>
				((IServiceCollection)null!).AddMessaging());
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_AddConnectorWithNullConnectorType()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			Assert.Throws<ArgumentNullException>(() =>
				builder.AddConnector(null!));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_AddConnectorWithNonConnectorType()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			Assert.Throws<ArgumentException>(() =>
				builder.AddConnector(typeof(string)));
		}

		[Fact]
		public void Should_ReturnBuilderForChaining_When_AddConnectorIsInvoked()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			var result = builder.AddConnector<TestConnector>();

			Assert.Same(builder, result);
		}

		[Fact]
		public void Should_ReturnBuilderForChaining_When_AddConnectorByType()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			var result = builder.AddConnector(typeof(TestConnector));

			Assert.Same(builder, result);
		}

		[Fact]
		public void Should_ReturnBuilderForChaining_When_AddConnectorWithFactory()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			var result = builder.AddConnector<TestConnector>((sp, schema) => new TestConnector(schema));

			Assert.Same(builder, result);
		}

		[Fact]
		public void Should_ReturnBuilderForChaining_When_AddConnectorByTypeWithFactory()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			var result = builder.AddConnector(typeof(TestConnector),
				(sp, schema) => new TestConnector(schema));

			Assert.Same(builder, result);
		}

		[Fact]
		public void Should_AllowMultipleConnectorRegistrations_When_BuilderIsInvoked()
		{
			var services = new ServiceCollection();

			services.AddMessaging()
				.AddConnector<TestConnector>()
				.AddConnector<AnotherTestConnector>()
				.AddConnector(typeof(ThirdTestConnector));

			// IChannelSchemaRegistry (not IChannelRegistry) is the registered service.
			Assert.Contains(services, d => d.ServiceType == typeof(IChannelSchemaRegistry));
		}

		[Fact]
		public void Should_RegisterConnectors_When_ServiceProviderIsBuilt()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>();

			var provider = services.BuildServiceProvider();

			// Connector resolvable as concrete type.
			var connector = provider.GetService<TestConnector>();
			Assert.NotNull(connector);
		}

		[Fact]
		public void Should_UseFactory_When_ConnectorIsCreatedAfterRegistration()
		{
			var factoryCalled = false;
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>((sp, schema) =>
				{
					factoryCalled = true;
					return new TestConnector(schema);
				});

			var provider = services.BuildServiceProvider();
			provider.GetRequiredService<TestConnector>();

			Assert.True(factoryCalled);
		}

		[Fact]
		public void Should_RegisterAllConnectors_When_MultipleConnectorsAreConfigured()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>()
				.AddConnector<AnotherTestConnector>()
				.AddConnector(typeof(ThirdTestConnector), (sp, schema) => new ThirdTestConnector(schema));

			var provider = services.BuildServiceProvider();
			var connectors = provider.GetServices<IChannelConnector>().ToList();

			Assert.Contains(connectors, c => c is TestConnector);
			Assert.Contains(connectors, c => c is AnotherTestConnector);
			Assert.Contains(connectors, c => c is ThirdTestConnector);
		}

		[Fact]
		public void Should_RegisterIChannelSchemaRegistry_When_AddConnectorIsInvoked()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>();

			Assert.Contains(services, descriptor =>
				descriptor.ServiceType == typeof(IChannelSchemaRegistry) &&
				descriptor.Lifetime == ServiceLifetime.Singleton);
		}

		[Fact]
		public void Should_ThrowArgumentException_When_AddConnectorWithConnectorWithoutSchemaAttribute()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			var exception = Assert.Throws<ArgumentException>(() =>
				builder.AddConnector<ConnectorWithoutAttribute>());

			Assert.Contains("must be decorated with", exception.Message);
		}

		[Fact]
		public void Should_ExposeServicesProperty_When_AddMessagingIsInvoked()
		{
			var services = new ServiceCollection();

			var builder = services.AddMessaging();

			Assert.Same(services, builder.Services);
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class TestConnector : ChannelConnectorBase
		{
			public TestConnector(IChannelSchema schema) : base(schema) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken) { SetState(ConnectorState.Ready); return ValueTask.CompletedTask; }
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotImplementedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
		}

		[ChannelSchema(typeof(AnotherTestSchemaFactory))]
		private class AnotherTestConnector : ChannelConnectorBase
		{
			public AnotherTestConnector(IChannelSchema schema) : base(schema) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken) { SetState(ConnectorState.Ready); return ValueTask.CompletedTask; }
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotImplementedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
		}

		[ChannelSchema(typeof(ThirdTestSchemaFactory))]
		private class ThirdTestConnector : ChannelConnectorBase
		{
			public ThirdTestConnector(IChannelSchema schema) : base(schema) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken) { SetState(ConnectorState.Ready); return ValueTask.CompletedTask; }
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotImplementedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
		}

		private class ConnectorWithoutAttribute : ChannelConnectorBase
		{
			public ConnectorWithoutAttribute(IChannelSchema schema) : base(schema) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotImplementedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
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

		private class ThirdTestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema() =>
				new ChannelSchema("ThirdProvider", "ThirdType", "1.0.0")
					.WithCapabilities(ChannelCapability.ReceiveMessages)
					.HandlesMessageEndpoint(EndpointType.Url)
					.AddContentType(MessageContentType.Json);
		}
	}
}
