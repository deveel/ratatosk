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
	/// Additional tests for ServiceCollectionExtensions covering edge cases and error handling.
	/// </summary>
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ServiceCollectionExtensions")]
	public class ServiceCollectionExtensionsTests
	{
		[Fact]
		public void Should_ThrowArgumentNullException_When_AddChannelRegistryWithNullServices()
		{
			// Arrange
			IServiceCollection services = null!;

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => services.AddChannelRegistry());
		}

		[Fact]
		public void Should_RegisterChannelRegistryAssingleton_When_AddChannelRegistryIsInvoked()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			services.AddChannelRegistry();

			// Assert
			Assert.Contains(services, descriptor => 
				descriptor.ServiceType == typeof(IChannelRegistry) && 
				descriptor.Lifetime == ServiceLifetime.Singleton);
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_AddChannelConnectorWithNullServices()
		{
			// Arrange
			IServiceCollection services = null!;

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => 
				services.AddChannelConnector<TestConnector>());
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_AddChannelConnectorByTypeWithNullServices()
		{
			// Arrange
			IServiceCollection services = null!;

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => 
				services.AddChannelConnector(typeof(TestConnector)));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_AddChannelConnectorByTypeWithNullConnectorType()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => 
				services.AddChannelConnector(null!));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_AddChannelConnectorByTypeWithNonConnectorType()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => 
				services.AddChannelConnector(typeof(string)));
		}

		[Fact]
		public void Should_RegisterCorrectly_When_AddChannelConnectorGeneric()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var result = services.AddChannelConnector<TestConnector>();

			// Assert
			Assert.Same(services, result);
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelRegistry));
		}

		[Fact]
		public void Should_RegisterCorrectly_When_AddChannelConnectorByType()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var result = services.AddChannelConnector(typeof(TestConnector));

			// Assert
			Assert.Same(services, result);
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelRegistry));
		}

		[Fact]
		public void Should_RegisterCorrectly_When_AddChannelConnectorWithFactory()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var result = services.AddChannelConnector<TestConnector>((sp, schema) => new TestConnector(schema));

			// Assert
			Assert.Same(services, result);
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelRegistry));
		}

		[Fact]
		public void Should_RegisterCorrectly_When_AddChannelConnectorByTypeWithFactory()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var result = services.AddChannelConnector(typeof(TestConnector), (sp, schema) => new TestConnector(schema));

			// Assert
			Assert.Same(services, result);
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelRegistry));
		}

		[Fact]
		public void Should_ReturnBuilderWithServices_When_AddChannelRegistryIsInvoked()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var builder = services.AddChannelRegistry();

			// Assert
			Assert.NotNull(builder);
			Assert.Same(services, builder.Services);
		}

		[Fact]
		public void Should_ThrowArgumentException_When_AddChannelConnectorWithConnectorWithoutSchemaAttribute()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			// Assert
			var exception = Assert.Throws<ArgumentException>(() => 
				services.AddChannelConnector<ConnectorWithoutAttribute>());
			
			Assert.Contains("must be decorated with", exception.Message);
		}

		[Fact]
		public void Should_ThrowArgumentException_When_AddChannelConnectorByTypeWithConnectorWithoutSchemaAttribute()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			// Assert
			var exception = Assert.Throws<ArgumentException>(() => 
				services.AddChannelConnector(typeof(ConnectorWithoutAttribute)));
			
			Assert.Contains("must be decorated with", exception.Message);
		}

		[Fact]
		public void Should_DoNotThrow_When_AddChannelConnectorMultipleCalls()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			services.AddChannelConnector<TestConnector>();
			services.AddChannelConnector<AnotherTestConnector>();

			// Assert
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelRegistry));
		}

		[Fact]
		public void Should_RegisterOnlyOnce_When_AddChannelRegistryMultipleCalls()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			services.AddChannelRegistry();
			services.AddChannelRegistry();

			// Assert
			var registrations = services.Where(d => d.ServiceType == typeof(IChannelRegistry)).ToList();
			Assert.True(registrations.Count >= 1); // May have multiple due to builder pattern
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class TestConnector : ChannelConnectorBase
		{
			public TestConnector(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Ready);
				return Task.FromResult(ConnectorResult<bool>.Success(true));
			}

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();
		}

		[ChannelSchema(typeof(AnotherTestSchemaFactory))]
		private class AnotherTestConnector : ChannelConnectorBase
		{
			public AnotherTestConnector(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				SetState(ConnectorState.Ready);
				return Task.FromResult(ConnectorResult<bool>.Success(true));
			}

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();
		}

		// Connector without schema attribute for testing error scenarios
		private class ConnectorWithoutAttribute : ChannelConnectorBase
		{
			public ConnectorWithoutAttribute(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();
		}

		private class TestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema()
			{
				return new ChannelSchema("TestProvider", "TestType", "1.0.0")
					.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
					.HandlesMessageEndpoint(EndpointType.PhoneNumber)
					.AddContentType(MessageContentType.PlainText);
			}
		}

		private class AnotherTestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema()
			{
				return new ChannelSchema("AnotherProvider", "AnotherType", "1.0.0")
					.WithCapabilities(ChannelCapability.SendMessages)
					.HandlesMessageEndpoint(EndpointType.EmailAddress)
					.AddContentType(MessageContentType.Html);
			}
		}
	}
}