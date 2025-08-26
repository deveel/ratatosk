//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Deveel.Messaging.XUnit
{
	/// <summary>
	/// Extended tests for ChannelDescriptor covering edge cases and branch coverage.
	/// </summary>
	public class ChannelDescriptorExtendedTests
	{
		[Fact]
		public void Constructor_WithNullChannelId_ThrowsArgumentNullException()
		{
			// Arrange
			var connectorType = typeof(TestConnector);
			var schema = CreateTestSchema();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => 
				new ChannelDescriptor(null!, connectorType, schema));
		}

		[Fact]
		public void Constructor_WithEmptyChannelId_ThrowsArgumentException()
		{
			// Arrange
			var connectorType = typeof(TestConnector);
			var schema = CreateTestSchema();

			// Act & Assert
			Assert.Throws<ArgumentException>(() => 
				new ChannelDescriptor("", connectorType, schema));
		}

		[Fact]
		public void Constructor_WithWhitespaceChannelId_ThrowsArgumentException()
		{
			// Arrange
			var connectorType = typeof(TestConnector);
			var schema = CreateTestSchema();

			// Act & Assert
			Assert.Throws<ArgumentException>(() => 
				new ChannelDescriptor("   ", connectorType, schema));
		}

		[Fact]
		public void Constructor_WithNullConnectorType_ThrowsArgumentNullException()
		{
			// Arrange
			var schema = CreateTestSchema();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => 
				new ChannelDescriptor("test-channel", null!, schema));
		}

		[Fact]
		public void Constructor_WithNullMasterSchema_ThrowsArgumentNullException()
		{
			// Arrange
			var connectorType = typeof(TestConnector);

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => 
				new ChannelDescriptor("test-channel", connectorType, null!));
		}

		[Fact]
		public void Constructor_WithNonConnectorType_ThrowsArgumentException()
		{
			// Arrange
			var schema = CreateTestSchema();

			// Act & Assert
			var exception = Assert.Throws<ArgumentException>(() => 
				new ChannelDescriptor("test-channel", typeof(string), schema));
			
			Assert.Contains("must implement IChannelConnector", exception.Message);
		}

		[Fact]
		public void Constructor_WithValidParameters_SetsAllProperties()
		{
			// Arrange
			const string channelId = "test-channel-id";
			var connectorType = typeof(TestConnector);
			var schema = CreateTestSchema();

			// Act
			var descriptor = new ChannelDescriptor(channelId, connectorType, schema);

			// Assert
			Assert.Equal(channelId, descriptor.ChannelId);
			Assert.Equal(connectorType, descriptor.ConnectorType);
			Assert.Same(schema, descriptor.MasterSchema);
			Assert.Equal("TestProvider", descriptor.ChannelProvider);
			Assert.Equal("TestType", descriptor.ChannelType);
			Assert.Equal("1.0.0", descriptor.Version);
		}

		[Fact]
		public void DisplayName_WhenSchemaHasDisplayName_ReturnsSchemaDisplayName()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithDisplayName("Custom Display Name");
			var descriptor = new ChannelDescriptor("test", typeof(TestConnector), schema);

			// Act & Assert
			Assert.Equal("Custom Display Name", descriptor.DisplayName);
		}

		[Fact]
		public void DisplayName_WhenSchemaDisplayNameIsNull_ReturnsNull()
		{
			// Arrange
			var schema = CreateTestSchema(); // No display name set
			var descriptor = new ChannelDescriptor("test", typeof(TestConnector), schema);

			// Act & Assert
			Assert.Null(descriptor.DisplayName);
		}

		[Fact]
		public void LogicalIdentity_ReturnsCorrectFormat()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var identity = descriptor.LogicalIdentity;

			// Assert
			Assert.Equal("TestProvider/TestType/1.0.0", identity);
		}

		[Fact]
		public void SupportsCapability_WithSupportedCapability_ReturnsTrue()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.True(descriptor.SupportsCapability(ChannelCapability.SendMessages));
		}

		[Fact]
		public void SupportsCapability_WithUnsupportedCapability_ReturnsFalse()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.False(descriptor.SupportsCapability(ChannelCapability.Templates));
		}

		[Fact]
		public void SupportsContentType_WithSupportedContentType_ReturnsTrue()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.True(descriptor.SupportsContentType(MessageContentType.PlainText));
		}

		[Fact]
		public void SupportsContentType_WithUnsupportedContentType_ReturnsFalse()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.False(descriptor.SupportsContentType(MessageContentType.Html));
		}

		[Fact]
		public void SupportsEndpointType_WithSupportedEndpointType_ReturnsTrue()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.True(descriptor.SupportsEndpointType(EndpointType.PhoneNumber));
		}

		[Fact]
		public void SupportsEndpointType_WithUnsupportedEndpointType_ReturnsFalse()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.False(descriptor.SupportsEndpointType(EndpointType.EmailAddress));
		}

		[Fact]
		public void SupportsEndpointType_WithAnyEndpointType_ReturnsTrue()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.HandlesMessageEndpoint(EndpointType.Any);
			var descriptor = new ChannelDescriptor("test", typeof(TestConnector), schema);

			// Act & Assert
			Assert.True(descriptor.SupportsEndpointType(EndpointType.EmailAddress));
			Assert.True(descriptor.SupportsEndpointType(EndpointType.PhoneNumber));
		}

		[Fact]
		public void SupportsAuthenticationType_WithSupportedAuthenticationType_ReturnsTrue()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.True(descriptor.SupportsAuthenticationType(AuthenticationType.Basic));
		}

		[Fact]
		public void SupportsAuthenticationType_WithUnsupportedAuthenticationType_ReturnsFalse()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.False(descriptor.SupportsAuthenticationType(AuthenticationType.ApiKey));
		}

		[Fact]
		public void ToString_ReturnsFormattedString()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var result = descriptor.ToString();

			// Assert
			Assert.Equal("test-channel (TestProvider/TestType/1.0.0)", result);
		}

		[Fact]
		public void Equals_WithSameChannelId_ReturnsTrue()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("test-channel", typeof(AnotherTestConnector), CreateTestSchema());

			// Act & Assert
			Assert.True(descriptor1.Equals(descriptor2));
		}

		[Fact]
		public void Equals_WithDifferentChannelId_ReturnsFalse()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("different-channel", typeof(TestConnector), CreateTestSchema());

			// Act & Assert
			Assert.False(descriptor1.Equals(descriptor2));
		}

		[Fact]
		public void Equals_WithSameChannelIdDifferentCase_ReturnsTrue()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("TEST-CHANNEL", typeof(TestConnector), CreateTestSchema());

			// Act & Assert
			Assert.True(descriptor1.Equals(descriptor2));
		}

		[Fact]
		public void Equals_WithNull_ReturnsFalse()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.False(descriptor.Equals(null));
		}

		[Fact]
		public void Equals_WithNonChannelDescriptor_ReturnsFalse()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.False(descriptor.Equals("string"));
		}

		[Fact]
		public void GetHashCode_SameChannelId_SameHashCode()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("test-channel", typeof(AnotherTestConnector), CreateTestSchema());

			// Act & Assert
			Assert.Equal(descriptor1.GetHashCode(), descriptor2.GetHashCode());
		}

		[Fact]
		public void GetHashCode_SameChannelIdDifferentCase_SameHashCode()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("TEST-CHANNEL", typeof(TestConnector), CreateTestSchema());

			// Act & Assert
			Assert.Equal(descriptor1.GetHashCode(), descriptor2.GetHashCode());
		}

		[Fact]
		public void GetHashCode_DifferentChannelId_DifferentHashCode()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("different-channel", typeof(TestConnector), CreateTestSchema());

			// Act & Assert
			Assert.NotEqual(descriptor1.GetHashCode(), descriptor2.GetHashCode());
		}

		[Fact]
		public void Capabilities_ReturnsSchemaCapabilities()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.Equal(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages, descriptor.Capabilities);
		}

		private static ChannelDescriptor CreateTestDescriptor()
		{
			return new ChannelDescriptor("test-channel", typeof(TestConnector), CreateTestSchema());
		}

		private static IChannelSchema CreateTestSchema()
		{
			return new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
				.HandlesMessageEndpoint(EndpointType.PhoneNumber)
				.AddContentType(MessageContentType.PlainText)
				.AddAuthenticationType(AuthenticationType.Basic);
		}

		// Test connector classes that implement IChannelConnector
		public class TestConnector : IChannelConnector
		{
			public IChannelSchema Schema { get; } = CreateTestSchema();
			public ConnectorState State => ConnectorState.Ready;

			public Task<ConnectorResult<bool>> InitializeAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			public Task<ConnectorResult<bool>> TestConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			public Task<ConnectorResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<StatusInfo>> GetStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<ConnectorHealth>> GetHealthAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task ShutdownAsync(CancellationToken cancellationToken)
				=> Task.CompletedTask;
		}

		public class AnotherTestConnector : IChannelConnector
		{
			public IChannelSchema Schema { get; } = CreateTestSchema();
			public ConnectorState State => ConnectorState.Ready;

			public Task<ConnectorResult<bool>> InitializeAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			public Task<ConnectorResult<bool>> TestConnectionAsync(CancellationToken cancellationToken)
				=> Task.FromResult(ConnectorResult<bool>.Success(true));

			public Task<ConnectorResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<StatusInfo>> GetStatusAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task<ConnectorResult<ConnectorHealth>> GetHealthAsync(CancellationToken cancellationToken)
				=> throw new NotImplementedException();

			public Task ShutdownAsync(CancellationToken cancellationToken)
				=> Task.CompletedTask;
		}
	}
}