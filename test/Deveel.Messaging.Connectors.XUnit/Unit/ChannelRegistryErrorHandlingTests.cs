//
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
	/// Tests for error handling scenarios in ChannelRegistry.
	/// </summary>
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ChannelRegistryErrorHandling")]
	public class ChannelRegistryErrorHandlingTests
	{
		private static ChannelRegistry CreateRegistry() => new ChannelRegistry(new ServiceCollection().BuildServiceProvider());

		[Fact]
		public void Should_ThrowArgumentNullException_When_RegisterConnectorWithNullType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.RegisterConnector(null!));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_RegisterConnectorWithNonConnectorType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => registry.RegisterConnector(typeof(string)));
		}

		[Fact]
		public async Task Should_ThrowArgumentNullException_When_CreateConnectorAsyncWithNullType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			await Assert.ThrowsAsync<ArgumentNullException>(() => registry.CreateConnectorAsync(null!));
		}

		[Fact]
		public async Task Should_ThrowArgumentNullException_When_CreateConnectorAsyncWithNullSchema()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			// Assert
			await Assert.ThrowsAsync<ArgumentNullException>(() => 
				registry.CreateConnectorAsync(typeof(TestConnector), null!));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_GetConnectorSchemaWithNullType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.GetConnectorSchema(null!));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_FindSchemaWithNullProvider()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.FindSchema(null!, "type"));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_FindSchemaWithEmptyProvider()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => registry.FindSchema("", "type"));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_FindSchemaWithWhitespaceProvider()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => registry.FindSchema("   ", "type"));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_FindSchemaWithNullType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.FindSchema("provider", null!));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_FindSchemaWithEmptyType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => registry.FindSchema("provider", ""));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_FindSchemaWithWhitespaceType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => registry.FindSchema("provider", "   "));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_FindConnectorWithNullProvider()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.FindConnector(null!, "type"));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_FindConnectorWithEmptyProvider()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => registry.FindConnector("", "type"));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_FindConnectorWithWhitespaceProvider()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => registry.FindConnector("   ", "type"));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_FindConnectorWithNullType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.FindConnector("provider", null!));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_FindConnectorWithEmptyType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => registry.FindConnector("provider", ""));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_FindConnectorWithWhitespaceType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => registry.FindConnector("provider", "   "));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_ValidateSchemaWithNullConnectorType()
		{
			// Arrange
			var registry = CreateRegistry();
			var schema = CreateTestSchema();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.ValidateSchema(null!, schema));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_ValidateSchemaWithNullSchema()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.ValidateSchema(typeof(TestConnector), null!));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_QuerySchemasWithNullPredicate()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.QuerySchemas(null!));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_IsConnectorRegisteredWithNullType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.IsConnectorRegistered(null!));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_UnregisterConnectorWithNullType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => registry.UnregisterConnector(null!));
		}

		[Fact]
		public async Task Should_ThrowInvalidOperationException_When_CreateConnectorAsyncWithFailedInitialization()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<FailingInitializationConnector>();

			// Act
			// Assert
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
				registry.CreateConnectorAsync<FailingInitializationConnector>());
			
			Assert.Contains("Failed to initialize connector", exception.Message);
		}

		[Fact]
		public async Task Should_ThrowInvalidOperationException_When_CreateConnectorAsyncWithExceptionDuringInitialization()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<ExceptionDuringInitializationConnector>();

			// Act
			// Assert
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
				registry.CreateConnectorAsync<ExceptionDuringInitializationConnector>());
			
			Assert.Contains("Failed to initialize connector", exception.Message);
		}

		[Fact]
		public void Should_ThrowArgumentException_When_RegisterConnectorWithInvalidSchemaFactory()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => 
				registry.RegisterConnector<ConnectorWithInvalidSchemaFactory>());
		}

		[Fact]
		public void Should_ThrowInvalidOperationException_When_RegisterConnectorWithExceptionInSchemaFactory()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<InvalidOperationException>(() => 
				registry.RegisterConnector<ConnectorWithFailingSchemaFactory>());
		}

		[Fact]
		public async Task Should_CreateSuccessfully_When_CreateConnectorAsyncWithSchemaFactoryInstance()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<ConnectorWithSchemaFactoryInstance>();

			// Act
			var connector = await registry.CreateConnectorAsync<ConnectorWithSchemaFactoryInstance>();

			// Assert
			Assert.NotNull(connector);
			Assert.Equal(ConnectorState.Ready, connector.State);
		}

		[Fact]
		public async Task Should_UseFactory_When_CreateConnectorAsyncWithCustomFactory()
		{
			// Arrange
			var registry = CreateRegistry();
			var factoryCalled = false;
			
			registry.RegisterConnector<TestConnector>(schema =>
			{
				factoryCalled = true;
				return new TestConnector(schema);
			});

			// Act
			var connector = await registry.CreateConnectorAsync<TestConnector>();

			// Assert
			Assert.NotNull(connector);
			Assert.True(factoryCalled);
		}

		[Fact]
		public async Task Should_ThrowInvalidOperationException_When_CreateConnectorAsyncWithFactoryException()
		{
			// Arrange
			var registry = CreateRegistry();
			
			registry.RegisterConnector<TestConnector>(schema =>
			{
				throw new Exception("Factory error");
			});

			// Act
			// Assert
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
				registry.CreateConnectorAsync<TestConnector>());
			
			Assert.Contains("Failed to create and initialize connector", exception.Message);
		}

		[Fact]
		public async Task Should_HandleGracefully_When_CreateConnectorAsyncWithInitializationTimeout()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<SlowInitializationConnector>();

			// Use a short cancellation token to test timeout handling
			using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

			// Act
			// Assert
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
				registry.CreateConnectorAsync<SlowInitializationConnector>(cts.Token));
			
			Assert.Contains("Failed to initialize connector", exception.Message);
			Assert.Contains("A task was canceled", exception.Message);
		}

		private static IChannelSchema CreateTestSchema()
		{
			return new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
				.HandlesMessageEndpoint(EndpointType.PhoneNumber)
				.AddContentType(MessageContentType.PlainText);
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

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class FailingInitializationConnector : ChannelConnectorBase
		{
			public FailingInitializationConnector(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				return Task.FromResult(ConnectorResult<bool>.Fail("INIT_FAILED", "Initialization failed"));
			}

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();
		}

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class ExceptionDuringInitializationConnector : ChannelConnectorBase
		{
			public ExceptionDuringInitializationConnector(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				throw new Exception("Initialization exception");
			}

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();
		}

		[ChannelSchema(typeof(InvalidSchemaFactory))]
		private class ConnectorWithInvalidSchemaFactory : ChannelConnectorBase
		{
			public ConnectorWithInvalidSchemaFactory(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();
		}

		[ChannelSchema(typeof(FailingSchemaFactory))]
		private class ConnectorWithFailingSchemaFactory : ChannelConnectorBase
		{
			public ConnectorWithFailingSchemaFactory(IChannelSchema schema) : base(schema) { }

			protected override Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<bool>> TestConnectorConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			protected override Task<ConnectorResult<SendResult>> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			protected override Task<ConnectorResult<StatusInfo>> GetConnectorStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();
		}

		[ChannelSchema(typeof(DirectSchemaInstance))]
		private class ConnectorWithSchemaFactoryInstance : ChannelConnectorBase
		{
			public ConnectorWithSchemaFactoryInstance(IChannelSchema schema) : base(schema) { }

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

		[ChannelSchema(typeof(TestSchemaFactory))]
		private class SlowInitializationConnector : ChannelConnectorBase
		{
			public SlowInitializationConnector(IChannelSchema schema) : base(schema) { }

			protected override async Task<ConnectorResult<bool>> InitializeConnectorAsync(CancellationToken cancellationToken)
			{
				// Simulate slow initialization
				await Task.Delay(5000, cancellationToken);
				SetState(ConnectorState.Ready);
				return ConnectorResult<bool>.Success(true);
			}

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

		// Invalid schema factory that doesn't implement required interfaces
		private class InvalidSchemaFactory
		{
		}

		private class FailingSchemaFactory : IChannelSchemaFactory
		{
			public IChannelSchema CreateSchema()
			{
				throw new Exception("Schema factory error");
			}
		}

		// Direct schema instance for testing schema creation
		private class DirectSchemaInstance : IChannelSchema
		{
			public string ChannelProvider => "DirectProvider";
			public string ChannelType => "DirectType";
			public string Version => "1.0.0";
			public string? DisplayName => "Direct Schema";
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
