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
		public void Should_AllowMultipleConnectorRegistrations_When_BuilderIsInvoked()
		{
			var services = new ServiceCollection();

			services.AddMessaging()
				.AddConnector<TestConnector>()
				.AddConnector<AnotherTestConnector>()
				.AddConnector(typeof(ThirdTestConnector));

			Assert.Contains(services, d => d.ServiceType == typeof(IChannelSchemaRegistry));
		}

		[Fact]
		public void Should_RegisterConnectors_When_ServiceProviderIsBuilt()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>();

			var provider = services.BuildServiceProvider();

			var connector = provider.GetService<TestConnector>();
			Assert.NotNull(connector);
		}

		[Fact]
		public void Should_RegisterAllConnectors_When_MultipleConnectorsAreConfigured()
		{
			var services = new ServiceCollection();
			services.AddMessaging()
				.AddConnector<TestConnector>()
				.AddConnector<AnotherTestConnector>()
				.AddConnector(typeof(ThirdTestConnector));

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
			public TestConnector(IChannelSchema schema, ConnectionSettings? settings = null) : base(schema, settings) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken) { SetState(ConnectorState.Ready); return ValueTask.CompletedTask; }
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotSupportedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
		}

		[ChannelSchema(typeof(AnotherTestSchemaFactory))]
		private class AnotherTestConnector : ChannelConnectorBase
		{
			public AnotherTestConnector(IChannelSchema schema, ConnectionSettings? settings = null) : base(schema, settings) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken) { SetState(ConnectorState.Ready); return ValueTask.CompletedTask; }
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotSupportedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
		}

		[ChannelSchema(typeof(ThirdTestSchemaFactory))]
		private class ThirdTestConnector : ChannelConnectorBase
		{
			public ThirdTestConnector(IChannelSchema schema, ConnectionSettings? settings = null) : base(schema, settings) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken) { SetState(ConnectorState.Ready); return ValueTask.CompletedTask; }
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotSupportedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
		}

		private class ConnectorWithoutAttribute : ChannelConnectorBase
		{
			public ConnectorWithoutAttribute(IChannelSchema schema, ConnectionSettings? settings = null) : base(schema, settings) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotSupportedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
		}

		private class TestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema() =>
				new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
					.WithCapabilities(ChannelCapability.SendMessages)
					.HandlesMessageEndpoint(EndpointType.PhoneNumber)
					.AddContentType(MessageContentType.PlainText)
					.Build();
		}

		private class AnotherTestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema() =>
				new ChannelSchemaBuilder("AnotherProvider", "AnotherType", "1.0.0")
					.WithCapabilities(ChannelCapability.SendMessages)
					.HandlesMessageEndpoint(EndpointType.EmailAddress)
					.AddContentType(MessageContentType.Html)
					.Build();
		}

		private class ThirdTestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema() =>
				new ChannelSchemaBuilder("ThirdProvider", "ThirdType", "1.0.0")
					.WithCapabilities(ChannelCapability.ReceiveMessages)
					.HandlesMessageEndpoint(EndpointType.Url)
					.AddContentType(MessageContentType.Json)
					.Build();
		}
	}
}
