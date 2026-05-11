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
	/// <summary>
	/// Tests for ServiceCollectionExtensions covering AddMessaging and backward-compatible AddChannelConnector.
	/// </summary>
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
		public void Should_ReturnSameBuilder_When_AddConnectorWithFactoryIsInvoked()
		{
			var services = new ServiceCollection();
			var builder = services.AddMessaging();

			var result = builder.AddConnector<TestConnector>((sp, schema) => new TestConnector(schema));

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

		#region AddChannelConnector (obsolete — backward-compatibility coverage)

#pragma warning disable CS0618

		[Fact]
		public void Should_ThrowArgumentNullException_When_AddChannelConnectorWithNullServices()
		{
			IServiceCollection services = null!;
			Assert.Throws<ArgumentNullException>(() => services.AddChannelConnector<TestConnector>());
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_AddChannelConnectorByTypeWithNullServices()
		{
			IServiceCollection services = null!;
			Assert.Throws<ArgumentNullException>(() => services.AddChannelConnector(typeof(TestConnector)));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_AddChannelConnectorByTypeWithNullConnectorType()
		{
			var services = new ServiceCollection();
			Assert.Throws<ArgumentNullException>(() => services.AddChannelConnector(null!));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_AddChannelConnectorByTypeWithNonConnectorType()
		{
			var services = new ServiceCollection();
			Assert.Throws<ArgumentException>(() => services.AddChannelConnector(typeof(string)));
		}

		[Fact]
		public void Should_RegisterCorrectly_When_AddChannelConnectorGeneric()
		{
			var services = new ServiceCollection();
			var result = services.AddChannelConnector<TestConnector>();

			Assert.Same(services, result);
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelSchemaRegistry));
		}

		[Fact]
		public void Should_RegisterCorrectly_When_AddChannelConnectorByType()
		{
			var services = new ServiceCollection();
			var result = services.AddChannelConnector(typeof(TestConnector));

			Assert.Same(services, result);
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelSchemaRegistry));
		}

		[Fact]
		public void Should_RegisterCorrectly_When_AddChannelConnectorWithFactory()
		{
			var services = new ServiceCollection();
			var result = services.AddChannelConnector<TestConnector>((sp, schema) => new TestConnector(schema));

			Assert.Same(services, result);
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelSchemaRegistry));
		}

		[Fact]
		public void Should_RegisterCorrectly_When_AddChannelConnectorByTypeWithFactory()
		{
			var services = new ServiceCollection();
			var result = services.AddChannelConnector(typeof(TestConnector), (sp, schema) => new TestConnector(schema));

			Assert.Same(services, result);
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelSchemaRegistry));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_AddChannelConnectorWithConnectorWithoutSchemaAttribute()
		{
			var services = new ServiceCollection();
			var exception = Assert.Throws<ArgumentException>(() =>
				services.AddChannelConnector<ConnectorWithoutAttribute>());

			Assert.Contains("must be decorated with", exception.Message);
		}

		[Fact]
		public void Should_ThrowArgumentException_When_AddChannelConnectorByTypeWithConnectorWithoutSchemaAttribute()
		{
			var services = new ServiceCollection();
			var exception = Assert.Throws<ArgumentException>(() =>
				services.AddChannelConnector(typeof(ConnectorWithoutAttribute)));

			Assert.Contains("must be decorated with", exception.Message);
		}

		[Fact]
		public void Should_DoNotThrow_When_AddChannelConnectorMultipleCalls()
		{
			var services = new ServiceCollection();
			services.AddChannelConnector<TestConnector>();
			services.AddChannelConnector<AnotherTestConnector>();

			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelSchemaRegistry));
		}

#pragma warning restore CS0618

		#endregion

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class TestConnector : ChannelConnectorBase
		{
			public TestConnector(IChannelSchema schema) : base(schema) { }
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
			public AnotherTestConnector(IChannelSchema schema) : base(schema) { }
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
					.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
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
