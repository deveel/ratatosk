using System.ComponentModel.DataAnnotations;
using Deveel.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Deveel.Messaging.XUnit
{
	/// <summary>
	/// Tests for the channel registry functionality with attribute-driven connector registration.
	/// </summary>
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ChannelRegistry")]
	public class ChannelRegistryTests
	{
		private static ChannelRegistry CreateRegistry() => new ChannelRegistry(new ServiceCollection().BuildServiceProvider());

		[Fact]
		public void Should_Succeed_When_RegisterConnectorWithValidConnector()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			registry.RegisterConnector<TestConnector>();
		}

		[Fact]
		public void Should_ThrowException_When_RegisterConnectorWithSameConnectorTypeTwice()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			// Assert
			Assert.Throws<InvalidOperationException>(() =>
				registry.RegisterConnector<TestConnector>());
		}

		[Fact]
		public void Should_ThrowException_When_RegisterConnectorWithoutSchemaAttribute()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() =>
				registry.RegisterConnector<ConnectorWithoutAttribute>());
		}

		[Fact]
		public void Should_ReturnSchema_When_GetMasterSchemaWithRegisteredConnector()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var schema = registry.GetConnectorSchema<TestConnector>();

			// Assert
			Assert.NotNull(schema);
			Assert.Equal("TestProvider", schema.ChannelProvider);
			Assert.Equal("TestType", schema.ChannelType);
		}

		[Fact]
		public void Should_ThrowException_When_GetMasterSchemaWithUnregisteredConnector()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			// Assert
			Assert.Throws<InvalidOperationException>(() =>
				registry.GetConnectorSchema<TestConnector>());
		}

		[Fact]
		public void Should_ReturnSchema_When_FindSchemaByProviderAndTypeWithMatchingSchema()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var schema = registry.FindSchema("TestProvider", "TestType");

			// Assert
			Assert.NotNull(schema);
			Assert.Equal("TestProvider", schema.ChannelProvider);
			Assert.Equal("TestType", schema.ChannelType);
		}

		[Fact]
		public void Should_ReturnNull_When_FindSchemaByProviderAndTypeWithNoMatch()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var schema = registry.FindSchema("NonExistent", "Provider");

			// Assert
			Assert.Null(schema);
		}

		[Fact]
		public void Should_ReturnType_When_FindConnectorTypeByProviderAndTypeWithMatchingConnector()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var connectorType = registry.FindConnector("TestProvider", "TestType");

			// Assert
			Assert.NotNull(connectorType);
			Assert.Equal(typeof(TestConnector), connectorType);
		}

		[Fact]
		public void Should_ReturnMatchingSchemas_When_QuerySchemasWithPredicate()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var schemas = registry.QuerySchemas(s => s.ChannelType == "TestType");

			// Assert
			Assert.Single(schemas);
		}

		[Fact]
		public void Should_ReturnAllDescriptors_When_GetConnectorDescriptorsWithoutPredicate()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var descriptors = registry.GetConnectorDescriptors();

			// Assert
			Assert.Single(descriptors);
			var descriptor = descriptors.First();
			Assert.Equal(typeof(TestConnector), descriptor.ConnectorType);
			Assert.Equal("TestProvider", descriptor.ChannelProvider);
			Assert.Equal("TestType", descriptor.ChannelType);
		}

		[Fact]
		public void Should_ReturnFilteredDescriptors_When_GetConnectorDescriptorsWithPredicate()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var descriptors = registry.GetConnectorDescriptors(d => d.ChannelType == "TestType");

			// Assert
			Assert.Single(descriptors);
		}

		[Fact]
		public async Task Should_ReturnConnector_When_CreateConnectorAsyncWithRegisteredType()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var connector = await registry.CreateConnectorAsync<TestConnector>(TestContext.Current.CancellationToken);

			// Assert
			Assert.NotNull(connector);
			Assert.IsType<TestConnector>(connector);
			Assert.Equal(ConnectorState.Ready, connector.State);
		}

		[Fact]
		public async Task Should_ValidateAndCreatesConnector_When_CreateConnectorAsyncWithRuntimeSchema()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			var masterSchema = registry.GetConnectorSchema<TestConnector>();
			var runtimeSchema = new ChannelSchema(masterSchema, "Runtime Schema")
				.RemoveCapability(ChannelCapability.ReceiveMessages);

			// Act
			var connector = await registry.CreateConnectorAsync<TestConnector>(runtimeSchema, TestContext.Current.CancellationToken);

			// Assert
			Assert.NotNull(connector);
			Assert.Equal(ChannelCapability.SendMessages, connector.Schema.Capabilities);
		}

		[Fact]
		public async Task Should_ThrowException_When_CreateConnectorAsyncWithIncompatibleSchema()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			var incompatibleSchema = new ChannelSchema("DifferentProvider", "DifferentType", "1.0.0");

			// Act
			// Assert
			await Assert.ThrowsAsync<InvalidOperationException>(async () =>
				await registry.CreateConnectorAsync<TestConnector>(incompatibleSchema, TestContext.Current.CancellationToken));
		}

		[Fact]
		public void Should_ReturnNoErrors_When_ValidateRuntimeSchemaWithCompatibleSchema()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			var masterSchema = registry.GetConnectorSchema<TestConnector>();
			var runtimeSchema = new ChannelSchema(masterSchema, "Valid Runtime");

			// Act
			var validationResults = registry.ValidateSchema<TestConnector>(runtimeSchema);

			// Assert
			Assert.Empty(validationResults);
		}

		[Fact]
		public void Should_ReturnErrors_When_ValidateRuntimeSchemaWithIncompatibleSchema()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			var incompatibleSchema = new ChannelSchema("DifferentProvider", "DifferentType", "1.0.0");

			// Act
			var validationResults = registry.ValidateSchema<TestConnector>(incompatibleSchema);

			// Assert
			Assert.NotEmpty(validationResults);
		}

		[Fact]
		public void Should_ReturnTrue_When_IsConnectorRegisteredWithRegisteredType()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var isRegistered = registry.IsConnectorRegistered<TestConnector>();

			// Assert
			Assert.True(isRegistered);
		}

		[Fact]
		public void Should_ReturnFalse_When_IsConnectorRegisteredWithUnregisteredType()
		{
			// Arrange
			var registry = CreateRegistry();

			// Act
			var isRegistered = registry.IsConnectorRegistered<TestConnector>();

			// Assert
			Assert.False(isRegistered);
		}

		[Fact]
		public void Should_ReturnTrue_When_UnregisterConnectorWithRegisteredType()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Act
			var unregistered = registry.UnregisterConnector<TestConnector>();

			// Assert
			Assert.True(unregistered);
			Assert.False(registry.IsConnectorRegistered<TestConnector>());
		}

		[Fact]
		public async Task Should_DisposesConnectors_When_DisposeRegistryWithTrackedConnectors()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Create some connectors that will be tracked
			var connector1 = await registry.CreateConnectorAsync<TestConnector>(TestContext.Current.CancellationToken);
			var connector2 = await registry.CreateConnectorAsync<TestConnector>(TestContext.Current.CancellationToken);

			// Verify connectors are in ready state
			Assert.Equal(ConnectorState.Ready, connector1.State);
			Assert.Equal(ConnectorState.Ready, connector2.State);

			// Act
			registry.Dispose();

			// Assert
			Assert.Equal(ConnectorState.Shutdown, connector1.State);
			Assert.Equal(ConnectorState.Shutdown, connector2.State);
		}

		[Fact]
		public async Task Should_DisposesConnectors_When_DisposeAsyncRegistryWithTrackedConnectors()
		{
			// Arrange
			var registry = CreateRegistry();
			registry.RegisterConnector<TestConnector>();

			// Create some connectors that will be tracked
			var connector1 = await registry.CreateConnectorAsync<TestConnector>(TestContext.Current.CancellationToken);
			var connector2 = await registry.CreateConnectorAsync<TestConnector>(TestContext.Current.CancellationToken);

			// Verify connectors are in ready state
			Assert.Equal(ConnectorState.Ready, connector1.State);
			Assert.Equal(ConnectorState.Ready, connector2.State);

			// Act
			await registry.DisposeAsync();

			// Assert
			Assert.Equal(ConnectorState.Shutdown, connector1.State);
			Assert.Equal(ConnectorState.Shutdown, connector2.State);
		}

		private static IChannelSchema CreateTestSchema()
		{
			return new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
				.HandlesMessageEndpoint(EndpointType.PhoneNumber)
				.AddContentType(MessageContentType.PlainText);
		}
	}

	/// <summary>
	/// Test schema factory for the test connector.
	/// </summary>
	internal class TestSchemaFactory : IChannelSchemaFactory
	{
		public IChannelSchema CreateSchema()
		{
			return new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
				.HandlesMessageEndpoint(EndpointType.PhoneNumber)
				.AddContentType(MessageContentType.PlainText);
		}
	}

	/// <summary>
	/// Test connector implementation for unit tests with schema attribute.
	/// </summary>
	[ChannelSchema(typeof(TestSchemaFactory))]
	internal class TestConnector : ChannelConnectorBase
	{
		public TestConnector(IChannelSchema schema) : base(schema)
		{
		}

		protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
		{
			SetState(ConnectorState.Ready);
            return ValueTask.CompletedTask;
		}

		protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
		}

		protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
		{
			var result = new SendResult("test-" + Guid.NewGuid().ToString("N")[..8], "remote-" + Guid.NewGuid().ToString("N")[..8]);
			result.Status = MessageStatus.Sent;
			return Task.FromResult(result);
		}

		protected override Task<BatchSendResult> SendBatchCoreAsync(IMessageBatch batch, CancellationToken cancellationToken)
		{
			var batchId = "test-batch-" + Guid.NewGuid().ToString("N")[..8];
			var remoteBatchId = "remote-batch-" + Guid.NewGuid().ToString("N")[..8];
			var messageResults = new Dictionary<string, SendResult>();

			foreach (var message in batch.Messages)
			{
				var sendResult = new SendResult(message.Id, "remote-" + Guid.NewGuid().ToString("N")[..8]);
				sendResult.Status = MessageStatus.Sent;
				messageResults[message.Id] = sendResult;
			}

			var batchResult = new BatchSendResult(batchId, remoteBatchId, messageResults);
			return Task.FromResult(batchResult);
		}

		protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
		{
			var status = new StatusInfo("Test Connector is running", "All systems operational");
			return Task.FromResult(status);
		}

		protected override Task<StatusUpdatesResult> GetMessageStatusCoreAsync(string messageId, CancellationToken cancellationToken)
		{
			var updates = new StatusUpdatesResult(messageId, Array.Empty<StatusUpdateResult>());
			return Task.FromResult(updates);
		}

		protected override Task<StatusUpdateResult> ReceiveMessageStatusCoreAsync(MessageSource source, CancellationToken cancellationToken)
		{
			var result = new StatusUpdateResult("test-message-id", MessageStatus.Delivered);
			return Task.FromResult(result);
		}

		protected override Task<ReceiveResult> ReceiveMessagesCoreAsync(MessageSource source, CancellationToken cancellationToken)
		{
			var result = new ReceiveResult("test-batch-" + Guid.NewGuid().ToString("N")[..8], Array.Empty<IMessage>());
			return Task.FromResult(result);
		}

		protected override Task<ConnectorHealth> GetConnectorHealthAsync(CancellationToken cancellationToken)
		{
			var health = new ConnectorHealth
			{
				State = State,
				IsHealthy = true,
				LastHealthCheck = DateTime.UtcNow,
				Uptime = TimeSpan.FromMinutes(10)
			};
			return Task.FromResult(health);
		}

		protected override Task ShutdownConnectorAsync(CancellationToken cancellationToken)
		{
			SetState(ConnectorState.Shutdown);
			return Task.CompletedTask;
		}
	}

	/// <summary>
	/// Test connector without schema attribute to verify error handling.
	/// </summary>
	internal class ConnectorWithoutAttribute : ChannelConnectorBase
	{
		public ConnectorWithoutAttribute(IChannelSchema schema) : base(schema)
		{
		}

		protected override ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
		{
			SetState(ConnectorState.Ready);
            return ValueTask.CompletedTask;
		}

		protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
		{
			return ValueTask.CompletedTask;
		}

		protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected override Task<BatchSendResult> SendBatchCoreAsync(IMessageBatch batch, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected override Task<StatusUpdatesResult> GetMessageStatusCoreAsync(string messageId, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected override Task<StatusUpdateResult> ReceiveMessageStatusCoreAsync(MessageSource source, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected override Task<ReceiveResult> ReceiveMessagesCoreAsync(MessageSource source, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected override Task<ConnectorHealth> GetConnectorHealthAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected override Task ShutdownConnectorAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
