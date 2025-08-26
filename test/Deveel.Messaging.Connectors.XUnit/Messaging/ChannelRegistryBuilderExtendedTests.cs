//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Deveel.Messaging.XUnit
{
	/// <summary>
	/// Extended tests for ChannelRegistryBuilder covering edge cases and error handling.
	/// </summary>
	public class ChannelRegistryBuilderExtendedTests
	{
		[Fact]
		public void AddChannelRegistry_WithNullServices_ThrowsArgumentNullException()
		{
			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => 
				((IServiceCollection)null!).AddChannelRegistry());
		}

		[Fact]
		public void RegisterConnector_WithNullConnectorType_ThrowsArgumentNullException()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => 
				builder.RegisterConnector(null!));
		}

		[Fact]
		public void RegisterConnector_WithNonConnectorType_ThrowsArgumentException()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act & Assert
			Assert.Throws<ArgumentException>(() => 
				builder.RegisterConnector(typeof(string)));
		}

		[Fact]
		public void RegisterConnector_ReturnsBuilderForChaining()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act
			var result = builder.RegisterConnector<TestConnector>();

			// Assert
			Assert.Same(builder, result);
		}

		[Fact]
		public void RegisterConnector_ByType_ReturnsBuilderForChaining()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act
			var result = builder.RegisterConnector(typeof(TestConnector));

			// Assert
			Assert.Same(builder, result);
		}

		[Fact]
		public void RegisterConnector_WithFactory_ReturnsBuilderForChaining()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act
			var result = builder.RegisterConnector<TestConnector>((sp, schema) => new TestConnector(schema));

			// Assert
			Assert.Same(builder, result);
		}

		[Fact]
		public void RegisterConnector_ByType_WithFactory_ReturnsBuilderForChaining()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act
			var result = builder.RegisterConnector(typeof(TestConnector), (sp, schema) => new TestConnector(schema));

			// Assert
			Assert.Same(builder, result);
		}

		[Fact]
		public void Builder_AllowsMultipleConnectorRegistrations()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act - Should not throw
			builder.RegisterConnector<TestConnector>()
				.RegisterConnector<AnotherTestConnector>()
				.RegisterConnector(typeof(ThirdTestConnector));

			// Assert - Builder should be returned for chaining
			Assert.NotNull(builder);
		}

		[Fact]
		public async Task ConnectorRegistrationService_StartAsync_PerformsRegistrations()
		{
			// Arrange
			var services = new ServiceCollection();
			
			// Add the real registry first, then use the builder
			var builder = services.AddChannelRegistry();
			builder.RegisterConnector<TestConnector>();
			
			var provider = services.BuildServiceProvider();
			
			// Get both the registry and the hosted service
			var registry = provider.GetRequiredService<IChannelRegistry>();
			var hostedServices = provider.GetServices<IHostedService>();
			var registrationService = hostedServices.FirstOrDefault(s => 
				s.GetType().Name.Contains("ConnectorRegistrationService"));

			// Act
			await registrationService!.StartAsync(CancellationToken.None);

			// Assert - Check that the connector was actually registered in the real registry
			Assert.True(registry.IsConnectorRegistered<TestConnector>());
		}

		[Fact]
		public async Task ConnectorRegistrationService_StartAsync_WithFactory_UsesFactory()
		{
			// Arrange
			var services = new ServiceCollection();
			
			var builder = services.AddChannelRegistry();
			builder.RegisterConnector<TestConnector>((sp, schema) => new TestConnector(schema));
			
			var provider = services.BuildServiceProvider();
			
			// Get both the registry and the hosted service
			var registry = provider.GetRequiredService<IChannelRegistry>();
			var hostedServices = provider.GetServices<IHostedService>();
			var registrationService = hostedServices.FirstOrDefault(s => 
				s.GetType().Name.Contains("ConnectorRegistrationService"));

			// Act
			await registrationService!.StartAsync(CancellationToken.None);

			// Assert - Check that the connector was registered and can be created
			Assert.True(registry.IsConnectorRegistered<TestConnector>());
			
			// Test that we can create a connector (which would use the factory)
			var connector = await registry.CreateConnectorAsync<TestConnector>();
			Assert.NotNull(connector);
		}

		[Fact]
		public async Task ConnectorRegistrationService_StopAsync_CompletesSuccessfully()
		{
			// Arrange
			var services = new ServiceCollection();
			var mockRegistry = new MockChannelRegistry();
			services.AddSingleton<IChannelRegistry>(mockRegistry);
			
			var builder = services.AddChannelRegistry();
			var provider = services.BuildServiceProvider();
			var hostedServices = provider.GetServices<IHostedService>();
			var registrationService = hostedServices.FirstOrDefault(s => 
				s.GetType().Name.Contains("ConnectorRegistrationService"));

			// Act & Assert - Should not throw
			await registrationService!.StopAsync(CancellationToken.None);
		}

		[Fact]
		public void ChannelRegistryBuilder_SetsServicesProperty()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var builder = services.AddChannelRegistry();

			// Assert
			Assert.Same(services, builder.Services);
		}

		[Fact]
		public async Task ConnectorRegistrationService_HandlesMultipleRegistrations()
		{
			// Arrange
			var services = new ServiceCollection();
			
			var builder = services.AddChannelRegistry();
			builder.RegisterConnector<TestConnector>()
				.RegisterConnector<AnotherTestConnector>()
				.RegisterConnector(typeof(ThirdTestConnector), (sp, schema) => new ThirdTestConnector(schema));
			
			var provider = services.BuildServiceProvider();
			
			var registry = provider.GetRequiredService<IChannelRegistry>();
			var hostedServices = provider.GetServices<IHostedService>();
			var registrationService = hostedServices.FirstOrDefault(s => 
				s.GetType().Name.Contains("ConnectorRegistrationService"));

			// Act
			await registrationService!.StartAsync(CancellationToken.None);

			// Assert - Check that all three connectors were registered
			Assert.True(registry.IsConnectorRegistered<TestConnector>());
			Assert.True(registry.IsConnectorRegistered<AnotherTestConnector>());
			Assert.True(registry.IsConnectorRegistered<ThirdTestConnector>());
		}

		[Fact]
		public async Task ConnectorRegistrationService_WithCancellation_HandlesGracefully()
		{
			// Arrange
			var services = new ServiceCollection();
			
			var builder = services.AddChannelRegistry();
			builder.RegisterConnector<TestConnector>();
			
			var provider = services.BuildServiceProvider();
			
			var registry = provider.GetRequiredService<IChannelRegistry>();
			var hostedServices = provider.GetServices<IHostedService>();
			var registrationService = hostedServices.FirstOrDefault(s => 
				s.GetType().Name.Contains("ConnectorRegistrationService"));

			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Act - Should complete even with cancelled token since registration is synchronous
			await registrationService!.StartAsync(cts.Token);

			// Assert - Registration should still happen since StartAsync doesn't check cancellation
			Assert.True(registry.IsConnectorRegistered<TestConnector>());
		}

		[Fact]
		public void Builder_RegistersHostedService()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var builder = services.AddChannelRegistry();
			builder.RegisterConnector<TestConnector>();

			// Assert - Check that a hosted service was registered
			Assert.Contains(services, descriptor => 
				descriptor.ServiceType == typeof(IHostedService) &&
				descriptor.Lifetime == ServiceLifetime.Singleton);
		}

		[Fact]
		public void RegisterConnector_WithConnectorWithoutSchemaAttribute_ThrowsArgumentException()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act & Assert - The builder validates the schema attribute immediately
			var exception = Assert.Throws<ArgumentException>(() => 
				builder.RegisterConnector<ConnectorWithoutAttribute>());
			
			Assert.Contains("must be decorated with", exception.Message);
		}

		private class MockChannelRegistry : IChannelRegistry
		{
			public bool WasRegisterConnectorCalled { get; private set; }
			public bool WasRegisterConnectorWithFactoryCalled { get; private set; }
			public int RegisterConnectorCallCount { get; private set; }

			public void RegisterConnector<TConnector>(Func<IChannelSchema, TConnector>? connectorFactory = null) where TConnector : class, IChannelConnector
			{
				WasRegisterConnectorCalled = true;
				RegisterConnectorCallCount++;
				if (connectorFactory != null)
					WasRegisterConnectorWithFactoryCalled = true;
			}

			public void RegisterConnector(Type connectorType, Func<IChannelSchema, IChannelConnector>? connectorFactory = null)
			{
				WasRegisterConnectorCalled = true;
				RegisterConnectorCallCount++;
				if (connectorFactory != null)
					WasRegisterConnectorWithFactoryCalled = true;
			}

			// All other methods throw NotImplementedException for this mock
			public Task<TConnector> CreateConnectorAsync<TConnector>(CancellationToken cancellationToken = default) where TConnector : class, IChannelConnector => throw new NotImplementedException();
			public Task<TConnector> CreateConnectorAsync<TConnector>(IChannelSchema runtimeSchema, CancellationToken cancellationToken = default) where TConnector : class, IChannelConnector => throw new NotImplementedException();
			public Task<IChannelConnector> CreateConnectorAsync(Type connectorType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
			public Task<IChannelConnector> CreateConnectorAsync(Type connectorType, IChannelSchema runtimeSchema, CancellationToken cancellationToken = default) => throw new NotImplementedException();
			public IChannelSchema GetConnectorSchema<TConnector>() where TConnector : class, IChannelConnector => throw new NotImplementedException();
			public IChannelSchema GetConnectorSchema(Type connectorType) => throw new NotImplementedException();
			public IChannelSchema? FindSchema(string channelProvider, string channelType) => throw new NotImplementedException();
			public Type? FindConnector(string channelProvider, string channelType) => throw new NotImplementedException();
			public IEnumerable<ValidationResult> ValidateSchema<TConnector>(IChannelSchema runtimeSchema) where TConnector : class, IChannelConnector => throw new NotImplementedException();
			public IEnumerable<ValidationResult> ValidateSchema(Type connectorType, IChannelSchema runtimeSchema) => throw new NotImplementedException();
			public IEnumerable<Type> GetConnectorTypes() => throw new NotImplementedException();
			public IEnumerable<ConnectorDescriptor> GetConnectorDescriptors(Func<ConnectorDescriptor, bool>? predicate = null) => throw new NotImplementedException();
			public IEnumerable<IChannelSchema> QuerySchemas(Func<IChannelSchema, bool> predicate) => throw new NotImplementedException();
			public bool IsConnectorRegistered<TConnector>() where TConnector : class, IChannelConnector => throw new NotImplementedException();
			public bool IsConnectorRegistered(Type connectorType) => throw new NotImplementedException();
			public bool UnregisterConnector<TConnector>() where TConnector : class, IChannelConnector => throw new NotImplementedException();
			public bool UnregisterConnector(Type connectorType) => throw new NotImplementedException();
			public void Dispose() { }
			public ValueTask DisposeAsync() => ValueTask.CompletedTask;
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

		[ChannelSchema(typeof(ThirdTestSchemaFactory))]
		public class ThirdTestConnector : ChannelConnectorBase
		{
			public ThirdTestConnector(IChannelSchema schema) : base(schema) { }

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
					.WithCapabilities(ChannelCapability.SendMessages)
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

		public class ThirdTestSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema()
			{
				return new ChannelSchema("ThirdProvider", "ThirdType", "1.0.0")
					.WithCapabilities(ChannelCapability.ReceiveMessages)
					.HandlesMessageEndpoint(EndpointType.Url)
					.AddContentType(MessageContentType.Json);
			}
		}
	}
}