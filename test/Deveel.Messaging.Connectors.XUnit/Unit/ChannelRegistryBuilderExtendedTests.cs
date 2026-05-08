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
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ChannelRegistryBuilderExtended")]
	public class ChannelRegistryBuilderExtendedTests
	{
		[Fact]
		public void Should_ThrowArgumentNullException_When_AddChannelRegistryWithNullServices()
		{
			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => 
				((IServiceCollection)null!).AddChannelRegistry());
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_RegisterConnectorWithNullConnectorType()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => 
				builder.RegisterConnector(null!));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_RegisterConnectorWithNonConnectorType()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => 
				builder.RegisterConnector(typeof(string)));
		}

		[Fact]
		public void Should_ReturnBuilderForChaining_When_RegisterConnectorIsInvoked()
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
		public void Should_ReturnBuilderForChaining_When_RegisterConnectorByType()
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
		public void Should_ReturnBuilderForChaining_When_RegisterConnectorWithFactory()
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
		public void Should_ReturnBuilderForChaining_When_RegisterConnectorByTypeWithFactory()
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
		public void Should_AllowsMultipleConnectorRegistrations_When_BuilderIsInvoked()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act
			builder.RegisterConnector<TestConnector>()
				.RegisterConnector<AnotherTestConnector>()
				.RegisterConnector(typeof(ThirdTestConnector));

			// Assert
			Assert.NotNull(builder);
		}

		[Fact]
		public async Task Should_PerformRegistrations_When_ConnectorRegistrationServiceStartAsync()
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

			// Assert
			Assert.True(registry.IsConnectorRegistered<TestConnector>());
		}

		[Fact]
		public async Task Should_UseFactory_When_ConnectorRegistrationServiceStartAsyncWithFactory()
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

			// Assert
			Assert.True(registry.IsConnectorRegistered<TestConnector>());
			
			// Test that we can create a connector (which would use the factory)
			var connector = await registry.CreateConnectorAsync<TestConnector>();
			Assert.NotNull(connector);
		}

		[Fact]
		public async Task Should_CompleteSuccessfully_When_ConnectorRegistrationServiceStopAsync()
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

			// Act
			// Assert
			await registrationService!.StopAsync(CancellationToken.None);
		}

		[Fact]
		public void Should_SetServicesProperty_When_ChannelRegistryBuilderIsInvoked()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var builder = services.AddChannelRegistry();

			// Assert
			Assert.Same(services, builder.Services);
		}

		[Fact]
		public async Task Should_HandleMultipleRegistrations_When_ConnectorRegistrationServiceIsInvoked()
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

			// Assert
			Assert.True(registry.IsConnectorRegistered<TestConnector>());
			Assert.True(registry.IsConnectorRegistered<AnotherTestConnector>());
			Assert.True(registry.IsConnectorRegistered<ThirdTestConnector>());
		}

		[Fact]
		public async Task Should_HandleGracefully_When_ConnectorRegistrationServiceWithCancellation()
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

			// Act
			await registrationService!.StartAsync(cts.Token);

			// Assert
			Assert.True(registry.IsConnectorRegistered<TestConnector>());
		}

		[Fact]
		public void Should_RegisterHostedService_When_BuilderIsInvoked()
		{
			// Arrange
			var services = new ServiceCollection();

			// Act
			var builder = services.AddChannelRegistry();
			builder.RegisterConnector<TestConnector>();

			// Assert
			Assert.Contains(services, descriptor => 
				descriptor.ServiceType == typeof(IHostedService) &&
				descriptor.Lifetime == ServiceLifetime.Singleton);
		}

		[Fact]
		public void Should_ThrowArgumentException_When_RegisterConnectorWithConnectorWithoutSchemaAttribute()
		{
			// Arrange
			var services = new ServiceCollection();
			var builder = services.AddChannelRegistry();

			// Act
			// Assert
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

		[ChannelSchema(typeof(ThirdTestSchemaFactory))]
		private class ThirdTestConnector : ChannelConnectorBase
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
					.WithCapabilities(ChannelCapability.SendMessages)
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

		private class ThirdTestSchemaFactory : IChannelSchemaFactory
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