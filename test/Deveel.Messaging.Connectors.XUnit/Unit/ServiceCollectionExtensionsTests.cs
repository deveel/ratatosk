//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Deveel.Messaging.XUnit
{
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ServiceCollectionExtensions")]
	public class ServiceCollectionExtensionsTests
	{
		#region AddMessaging

		[Fact]
		public void Should_ThrowArgumentNullException_When_AddMessagingWithNullServices()
		{
			IServiceCollection services = null!;
			Assert.Throws<ArgumentNullException>(() => services.AddMessaging());
		}

		[Fact]
		public void Should_ReturnMessagingBuilder_When_AddMessagingIsInvoked()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			Assert.NotNull(builder);
			Assert.Same(services, builder.Services);
		}

		[Fact]
		public void Should_RegisterChannelSchemaRegistryAsSingleton_When_AddMessagingIsInvoked()
		{
			var services = new ServiceCollection();
			services.AddMessaging();

			Assert.Contains(services, descriptor =>
				descriptor.ServiceType == typeof(IChannelSchemaRegistry) &&
				descriptor.Lifetime == ServiceLifetime.Singleton);
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

		#endregion

		#region MessagingBuilder.AddConnector

		[Fact]
		public void Should_ReturnSameBuilder_When_AddConnectorGenericIsInvoked()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			var result = builder.AddConnector<TestConnector>();

			Assert.Same(builder, result);
		}

		[Fact]
		public void Should_ReturnSameBuilder_When_AddConnectorByTypeIsInvoked()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			var result = builder.AddConnector(typeof(TestConnector));

			Assert.Same(builder, result);
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
		public void Should_ThrowArgumentNullException_When_AddConnectorByTypeWithNullConnectorType()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			Assert.Throws<ArgumentNullException>(() => builder.AddConnector(null!));
		}

		[Fact]
		public void Should_SupportFluentChaining_When_MultipleAddConnectorCallsAreChained()
		{
			var services = new ServiceCollection();

			var builder = services.AddMessaging()
				.AddConnector<TestConnector>()
				.AddConnector<AnotherTestConnector>();

			Assert.NotNull(builder);
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelSchemaRegistry));
		}

		#endregion

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

		private class ConnectorWithoutAttribute : ChannelConnectorBase
		{
			public ConnectorWithoutAttribute(IChannelSchema schema, ConnectionSettings? settings = null) : base(schema, settings) { }
			protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
			protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken) => throw new NotImplementedException();
			protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
		}

		private class TestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema() =>
				new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
					.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
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
	}
}
