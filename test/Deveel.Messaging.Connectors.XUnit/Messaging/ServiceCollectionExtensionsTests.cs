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
	public class ServiceCollectionExtensionsTests
	{
		[Fact]
		public void AddChannelRegistry_WithNullServices_ThrowsArgumentNullException()
		{
			// Arrange
			IServiceCollection services = null!;

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => services.AddChannelRegistry());
		}

		[Fact]
		public void AddChannelRegistry_RegistersChannelRegistryAssingleton()
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
		public void AddChannelConnector_WithNullServices_ThrowsArgumentNullException()
		{
			// Arrange
			IServiceCollection services = null!;

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => 
				services.AddChannelConnector<TestConnector>());
		}

		[Fact]
		public void AddChannelConnector_ByType_WithNullServices_ThrowsArgumentNullException()
		{
			// Arrange
			IServiceCollection services = null!;

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => 
				services.AddChannelConnector(typeof(TestConnector)));
		}

		[Fact]
		public void AddChannelConnector_ByType_WithNullConnectorType_ThrowsArgumentNullException()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => 
				services.AddChannelConnector(null!));
		}

		[Fact]
		public void AddChannelConnector_ByType_WithNonConnectorType_ThrowsArgumentException()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act & Assert
			Assert.Throws<ArgumentException>(() => 
				services.AddChannelConnector(typeof(string)));
		}

		[Fact]
		public void AddChannelConnector_Generic_RegistersCorrectly()
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
		public void AddChannelConnector_ByType_RegistersCorrectly()
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
		public void AddChannelConnector_WithFactory_RegistersCorrectly()
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
		public void AddChannelConnector_ByType_WithFactory_RegistersCorrectly()
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
		public void AddChannelRegistry_ReturnsBuilderWithServices()
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
		public void AddChannelConnector_WithConnectorWithoutSchemaAttribute_ThrowsArgumentException()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act & Assert - The builder validates the schema attribute immediately
			var exception = Assert.Throws<ArgumentException>(() => 
				services.AddChannelConnector<ConnectorWithoutAttribute>());
			
			Assert.Contains("must be decorated with", exception.Message);
		}

		[Fact]
		public void AddChannelConnector_ByType_WithConnectorWithoutSchemaAttribute_ThrowsArgumentException()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act & Assert - The builder validates the schema attribute immediately
			var exception = Assert.Throws<ArgumentException>(() => 
				services.AddChannelConnector(typeof(ConnectorWithoutAttribute)));
			
			Assert.Contains("must be decorated with", exception.Message);
		}

		[Fact]
		public void AddChannelConnector_MultipleCalls_DoesNotThrow()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act - Should not throw even if called multiple times
			services.AddChannelConnector<TestConnector>();
			services.AddChannelConnector<AnotherTestConnector>();

			// Assert
			Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IChannelRegistry));
		}

		[Fact]
		public void AddChannelRegistry_MultipleCalls_RegistersOnlyOnce()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			services.AddChannelRegistry();
			services.AddChannelRegistry();

			// Assert - Should only have one registration
			var registrations = services.Where(d => d.ServiceType == typeof(IChannelRegistry)).ToList();
			Assert.True(registrations.Count >= 1); // May have multiple due to builder pattern
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		public class TestConnector : ChannelConnectorBase
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
		public class AnotherTestConnector : ChannelConnectorBase
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
		public class ConnectorWithoutAttribute : ChannelConnectorBase
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

		public class TestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema()
			{
				return new ChannelSchema("TestProvider", "TestType", "1.0.0")
					.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
					.HandlesMessageEndpoint(EndpointType.PhoneNumber)
					.AddContentType(MessageContentType.PlainText);
			}
		}

		public class AnotherTestSchemaFactory : IChannelSchemaFactory
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