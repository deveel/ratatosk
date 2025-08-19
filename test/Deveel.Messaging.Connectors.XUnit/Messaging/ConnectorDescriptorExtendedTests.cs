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
	/// Additional tests for ConnectorDescriptor edge cases and comprehensive branch coverage.
	/// </summary>
	public class ConnectorDescriptorExtendedTests
	{
		[Fact]
		public void Constructor_WithNonConnectorType_DoesNotValidateInterface()
		{
			// Arrange
			var schema = CreateTestSchema();

			// Act - The ConnectorDescriptor constructor doesn't validate interface implementation
			var descriptor = new ConnectorDescriptor(typeof(string), schema);

			// Assert - Should succeed even though string doesn't implement IChannelConnector
			Assert.Equal(typeof(string), descriptor.ConnectorType);
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
		public void Equals_WithNonDescriptorObject_ReturnsFalse()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.False(descriptor.Equals("string"));
		}

		[Fact]
		public void SupportsCapability_WithZeroCapability_ReturnsFalse()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithCapabilities((ChannelCapability)0); // Explicitly set zero capabilities
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act & Assert
			Assert.False(descriptor.SupportsCapability(ChannelCapability.SendMessages));
		}

		[Fact]
		public void SupportsAnyCapability_WithZeroCapabilities_ReturnsFalse()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0");
			// The ChannelSchema constructor sets SendMessages as default capability
			// To test zero capabilities, we need to explicitly clear them
			var schemaWithNoCaps = schema.WithCapabilities((ChannelCapability)0);
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schemaWithNoCaps);

			// Act & Assert
			// When schema has no capabilities (0), checking for any capability should return false
			Assert.False(descriptor.SupportsAnyCapability(ChannelCapability.SendMessages));
			
			// Verify the schema actually has no capabilities
			Assert.Equal((ChannelCapability)0, schemaWithNoCaps.Capabilities);
		}

		[Fact]
		public void SupportsAllCapabilities_WithZeroCapabilities_ReturnsFalse()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithCapabilities((ChannelCapability)0); // Explicitly set zero capabilities
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act & Assert
			Assert.False(descriptor.SupportsAllCapabilities(ChannelCapability.SendMessages));
		}

		[Fact]
		public void SupportsEndpointType_WithAnyEndpointType_ReturnsTrue()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.HandlesMessageEndpoint(EndpointType.Any);
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act & Assert
			Assert.True(descriptor.SupportsEndpointType(EndpointType.EmailAddress));
			Assert.True(descriptor.SupportsEndpointType(EndpointType.PhoneNumber));
		}

		[Fact]
		public void SupportsEndpointType_WithNoEndpoints_ReturnsFalse()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0");
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act & Assert
			Assert.False(descriptor.SupportsEndpointType(EndpointType.EmailAddress));
		}

		[Fact]
		public void SupportsContentType_WithEmptyContentTypes_ReturnsFalse()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0");
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act & Assert
			Assert.False(descriptor.SupportsContentType(MessageContentType.PlainText));
		}

		[Fact]
		public void SupportsAuthenticationType_WithNoAuthenticationConfigurations_ReturnsFalse()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0");
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act & Assert
			Assert.False(descriptor.SupportsAuthenticationType(AuthenticationType.Basic));
		}

		[Fact]
		public void DisplayName_WithNullDisplayName_ReturnsConnectorTypeName()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0"); // DisplayName will be null
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act & Assert
			Assert.Equal("TestConnector", descriptor.DisplayName);
		}

		[Fact]
		public void DisplayName_WithEmptyDisplayName_ReturnsConnectorTypeName()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithDisplayName("");
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			var displayName = descriptor.DisplayName;

			// Assert
			Assert.Equal("TestConnector", displayName);
		}

		[Fact]
		public void DisplayName_WithWhitespaceDisplayName_ReturnsConnectorTypeName()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithDisplayName("   ");
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			var displayName = descriptor.DisplayName;

			// Assert
			Assert.Equal("TestConnector", displayName);
		}

		[Fact]
		public void ToString_WithComplexTypeName_ReturnsFormattedString()
		{
			// Arrange
			var descriptor = new ConnectorDescriptor(typeof(ComplexNamedConnector), CreateTestSchema());

			// Act
			var result = descriptor.ToString();

			// Assert
			Assert.Equal("ComplexNamedConnector (TestProvider/TestType/1.0.0)", result);
		}

		[Fact]
		public void GetHashCode_ConsistentWithEquals()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = CreateTestDescriptor();

			// Act & Assert - Objects that are equal should have the same hash code
			Assert.True(descriptor1.Equals(descriptor2));
			Assert.Equal(descriptor1.GetHashCode(), descriptor2.GetHashCode());
		}

		[Fact]
		public void GetHashCode_DifferentForDifferentTypes()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ConnectorDescriptor(typeof(ComplexNamedConnector), CreateTestSchema());

			// Act & Assert - Different objects should likely have different hash codes
			Assert.False(descriptor1.Equals(descriptor2));
			Assert.NotEqual(descriptor1.GetHashCode(), descriptor2.GetHashCode());
		}

		[Fact]
		public void SupportsAnyCapability_WithExactMatch_ReturnsTrue()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.True(descriptor.SupportsAnyCapability(ChannelCapability.SendMessages));
		}

		[Fact]
		public void SupportsAllCapabilities_WithExactMatch_ReturnsTrue()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act & Assert
			Assert.True(descriptor.SupportsAllCapabilities(ChannelCapability.SendMessages));
		}

		[Fact]
		public void SupportsAllCapabilities_WithSubsetCapabilities_ReturnsTrue()
		{
			// Arrange
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages | ChannelCapability.Templates);
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act & Assert
			Assert.True(descriptor.SupportsAllCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages));
		}

		private static ConnectorDescriptor CreateTestDescriptor()
		{
			return new ConnectorDescriptor(typeof(TestConnector), CreateTestSchema());
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

		public class ComplexNamedConnector : IChannelConnector
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