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
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ChannelDescriptorExtended")]
	public class ChannelDescriptorExtendedTests
	{
		[Fact]
		public void Should_ThrowArgumentNullException_When_ConstructorWithNullChannelId()
		{
			// Arrange
			var connectorType = typeof(TestConnector);
			var schema = CreateTestSchema();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => 
				new ChannelDescriptor(null!, connectorType, schema));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_ConstructorWithEmptyChannelId()
		{
			// Arrange
			var connectorType = typeof(TestConnector);
			var schema = CreateTestSchema();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => 
				new ChannelDescriptor("", connectorType, schema));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_ConstructorWithWhitespaceChannelId()
		{
			// Arrange
			var connectorType = typeof(TestConnector);
			var schema = CreateTestSchema();

			// Act
			// Assert
			Assert.Throws<ArgumentException>(() => 
				new ChannelDescriptor("   ", connectorType, schema));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_ConstructorWithNullConnectorType()
		{
			// Arrange
			var schema = CreateTestSchema();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => 
				new ChannelDescriptor("test-channel", null!, schema));
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_ConstructorWithNullMasterSchema()
		{
			// Arrange
			var connectorType = typeof(TestConnector);

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => 
				new ChannelDescriptor("test-channel", connectorType, null!));
		}

		[Fact]
		public void Should_ThrowArgumentException_When_ConstructorWithNonConnectorType()
		{
			// Arrange
			var schema = CreateTestSchema();

			// Act
			// Assert
			var exception = Assert.Throws<ArgumentException>(() => 
				new ChannelDescriptor("test-channel", typeof(string), schema));
			
			Assert.Contains("must implement IChannelConnector", exception.Message);
		}

		[Fact]
		public void Should_SetAllProperties_When_ConstructorWithValidParameters()
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
		public void Should_ReturnSchemaDisplayName_When_DisplayNameWhenSchemaHasDisplayName()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
				.WithDisplayName("Custom Display Name").Build();
			var descriptor = new ChannelDescriptor("test", typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.Equal("Custom Display Name", descriptor.DisplayName);
		}

		[Fact]
		public void Should_ReturnNull_When_DisplayNameWhenSchemaDisplayNameIsNull()
		{
			// Arrange
			var schema = CreateTestSchema(); // No display name set
			var descriptor = new ChannelDescriptor("test", typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.Null(descriptor.DisplayName);
		}

		[Fact]
		public void Should_ReturnCorrectFormat_When_LogicalIdentityIsInvoked()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var identity = descriptor.LogicalIdentity;

			// Assert
			Assert.Equal("TestProvider/TestType/1.0.0", identity);
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsCapabilityWithSupportedCapability()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.True(descriptor.SupportsCapability(ChannelCapability.SendMessages));
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsCapabilityWithUnsupportedCapability()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.False(descriptor.SupportsCapability(ChannelCapability.Templates));
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsContentTypeWithSupportedContentType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.True(descriptor.SupportsContentType(MessageContentType.PlainText));
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsContentTypeWithUnsupportedContentType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.False(descriptor.SupportsContentType(MessageContentType.Html));
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsEndpointTypeWithSupportedEndpointType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.True(descriptor.SupportsEndpointType(EndpointType.PhoneNumber));
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsEndpointTypeWithUnsupportedEndpointType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.False(descriptor.SupportsEndpointType(EndpointType.EmailAddress));
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsEndpointTypeWithAnyEndpointType()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
				.HandlesMessageEndpoint(EndpointType.Any).Build();
			var descriptor = new ChannelDescriptor("test", typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.True(descriptor.SupportsEndpointType(EndpointType.EmailAddress));
			Assert.True(descriptor.SupportsEndpointType(EndpointType.PhoneNumber));
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsAuthenticationTypeWithSupportedAuthenticationType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.True(descriptor.SupportsAuthenticationScheme(AuthenticationScheme.Basic));
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsAuthenticationTypeWithUnsupportedAuthenticationType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.False(descriptor.SupportsAuthenticationScheme(AuthenticationScheme.ApiKey));
		}

		[Fact]
		public void Should_ReturnFormattedString_When_ToStringIsInvoked()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var result = descriptor.ToString();

			// Assert
			Assert.Equal("test-channel (TestProvider/TestType/1.0.0)", result);
		}

		[Fact]
		public void Should_ReturnTrue_When_EqualsWithSameChannelId()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("test-channel", typeof(AnotherTestConnector), CreateTestSchema());

			// Act
			// Assert
			Assert.True(descriptor1.Equals(descriptor2));
		}

		[Fact]
		public void Should_ReturnFalse_When_EqualsWithDifferentChannelId()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("different-channel", typeof(TestConnector), CreateTestSchema());

			// Act
			// Assert
			Assert.False(descriptor1.Equals(descriptor2));
		}

		[Fact]
		public void Should_ReturnTrue_When_EqualsWithSameChannelIdDifferentCase()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("TEST-CHANNEL", typeof(TestConnector), CreateTestSchema());

			// Act
			// Assert
			Assert.True(descriptor1.Equals(descriptor2));
		}

		[Fact]
		public void Should_ReturnFalse_When_EqualsWithNull()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.False(descriptor.Equals(null));
		}

		[Fact]
		public void Should_ReturnFalse_When_EqualsWithNonChannelDescriptor()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.False(descriptor.Equals("string"));
		}

		[Fact]
		public void Should_SameHashCode_When_GetHashCodeSameChannelId()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("test-channel", typeof(AnotherTestConnector), CreateTestSchema());

			// Act
			// Assert
			Assert.Equal(descriptor1.GetHashCode(), descriptor2.GetHashCode());
		}

		[Fact]
		public void Should_SameHashCode_When_GetHashCodeSameChannelIdDifferentCase()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("TEST-CHANNEL", typeof(TestConnector), CreateTestSchema());

			// Act
			// Assert
			Assert.Equal(descriptor1.GetHashCode(), descriptor2.GetHashCode());
		}

		[Fact]
		public void Should_DifferentHashCode_When_GetHashCodeDifferentChannelId()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ChannelDescriptor("different-channel", typeof(TestConnector), CreateTestSchema());

			// Act
			// Assert
			Assert.NotEqual(descriptor1.GetHashCode(), descriptor2.GetHashCode());
		}

		[Fact]
		public void Should_ReturnSchemaCapabilities_When_CapabilitiesIsInvoked()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.Equal(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages, descriptor.Capabilities);
		}

		private static ChannelDescriptor CreateTestDescriptor()
		{
			return new ChannelDescriptor("test-channel", typeof(TestConnector), CreateTestSchema());
		}

		private static IChannelSchema CreateTestSchema()
		{
			return new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
				.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
				.HandlesMessageEndpoint(EndpointType.PhoneNumber)
				.AddContentType(MessageContentType.PlainText)
				.AddAuthenticationScheme(AuthenticationScheme.Basic).Build();
		}

		// Test connector classes that implement IChannelConnector
		private class TestConnector : IChannelConnector
		{
			public IChannelSchema Schema { get; } = CreateTestSchema();
			public ConnectorState State => ConnectorState.Ready;

			public ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken cancellationToken)
				=> new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));

			public ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken cancellationToken)
				=> new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));

			public ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask ShutdownAsync(CancellationToken cancellationToken)
				=> default;
		}

		private class AnotherTestConnector : IChannelConnector
		{
			public IChannelSchema Schema { get; } = CreateTestSchema();
			public ConnectorState State => ConnectorState.Ready;

			public ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken cancellationToken)
				=> new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));

			public ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken cancellationToken)
				=> new ValueTask<OperationResult<bool>>(OperationResult<bool>.Success(true));

			public ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken cancellationToken)
				=> throw new NotSupportedException();

			public ValueTask ShutdownAsync(CancellationToken cancellationToken)
				=> default;
		}
	}
}