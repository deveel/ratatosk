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
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ConnectorDescriptorExtended")]
	public class ConnectorDescriptorExtendedTests
	{
		[Fact]
		public void Should_DoNotValidateInterface_When_ConstructorWithNonConnectorType()
		{
			// Arrange
			var schema = CreateTestSchema();

			// Act
			var descriptor = new ConnectorDescriptor(typeof(string), schema);

			// Assert
			Assert.Equal(typeof(string), descriptor.ConnectorType);
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
		public void Should_ReturnFalse_When_EqualsWithNonDescriptorObject()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.False(descriptor.Equals("string"));
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsCapabilityWithZeroCapability()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
				.WithCapabilities((ChannelCapability)0).Build(); // Explicitly set zero capabilities
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.False(descriptor.SupportsCapability(ChannelCapability.SendMessages));
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsAnyCapabilityWithZeroCapabilities()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0").Build();
			// The ChannelSchema constructor sets SendMessages as default capability
			// To test zero capabilities, we need to explicitly clear them
			var builder = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0");
			var schemaWithNoCaps = builder.WithCapabilities((ChannelCapability)0).Build();
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schemaWithNoCaps);

			// Act
			// Assert
			// When schema has no capabilities (0), checking for any capability should return false
			Assert.False(descriptor.SupportsAnyCapability(ChannelCapability.SendMessages));
			
			// Verify the schema actually has no capabilities
			Assert.Equal((ChannelCapability)0, schemaWithNoCaps.Capabilities);
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsAllCapabilitiesWithZeroCapabilities()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
				.WithCapabilities((ChannelCapability)0).Build(); // Explicitly set zero capabilities
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.False(descriptor.SupportsAllCapabilities(ChannelCapability.SendMessages));
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsEndpointTypeWithAnyEndpointType()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
				.HandlesMessageEndpoint(EndpointType.Any).Build();
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.True(descriptor.SupportsEndpointType(EndpointType.EmailAddress));
			Assert.True(descriptor.SupportsEndpointType(EndpointType.PhoneNumber));
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsEndpointTypeWithNoEndpoints()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0").Build();
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.False(descriptor.SupportsEndpointType(EndpointType.EmailAddress));
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsContentTypeWithEmptyContentTypes()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0").Build();
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.False(descriptor.SupportsContentType(MessageContentType.PlainText));
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsAuthenticationTypeWithNoAuthenticationConfigurations()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0").Build();
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.False(descriptor.SupportsAuthenticationScheme(AuthenticationScheme.Basic));
		}

		[Fact]
		public void Should_ReturnConnectorTypeName_When_DisplayNameWithNullDisplayName()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0").Build(); // DisplayName will be null
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.Equal("TestConnector", descriptor.DisplayName);
		}

		[Fact]
		public void Should_ReturnConnectorTypeName_When_DisplayNameWithEmptyDisplayName()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
				.WithDisplayName("").Build();
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			var displayName = descriptor.DisplayName;

			// Assert
			Assert.Equal("TestConnector", displayName);
		}

		[Fact]
		public void Should_ReturnConnectorTypeName_When_DisplayNameWithWhitespaceDisplayName()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
				.WithDisplayName("   ").Build();
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			var displayName = descriptor.DisplayName;

			// Assert
			Assert.Equal("TestConnector", displayName);
		}

		[Fact]
		public void Should_ReturnFormattedString_When_ToStringWithComplexTypeName()
		{
			// Arrange
			var descriptor = new ConnectorDescriptor(typeof(ComplexNamedConnector), CreateTestSchema());

			// Act
			var result = descriptor.ToString();

			// Assert
			Assert.Equal("ComplexNamedConnector (TestProvider/TestType/1.0.0)", result);
		}

		[Fact]
		public void Should_ConsistentWithEquals_When_GetHashCodeIsInvoked()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = CreateTestDescriptor();

			// Act
			// Assert
			Assert.True(descriptor1.Equals(descriptor2));
			Assert.Equal(descriptor1.GetHashCode(), descriptor2.GetHashCode());
		}

		[Fact]
		public void Should_DifferentForDifferentTypes_When_GetHashCodeIsInvoked()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ConnectorDescriptor(typeof(ComplexNamedConnector), CreateTestSchema());

			// Act
			// Assert
			Assert.False(descriptor1.Equals(descriptor2));
			Assert.NotEqual(descriptor1.GetHashCode(), descriptor2.GetHashCode());
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsAnyCapabilityWithExactMatch()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.True(descriptor.SupportsAnyCapability(ChannelCapability.SendMessages));
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsAllCapabilitiesWithExactMatch()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			// Assert
			Assert.True(descriptor.SupportsAllCapabilities(ChannelCapability.SendMessages));
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsAllCapabilitiesWithSubsetCapabilities()
		{
			// Arrange
			var schema = new ChannelSchemaBuilder("TestProvider", "TestType", "1.0.0")
				.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages | ChannelCapability.Templates).Build();
			var descriptor = new ConnectorDescriptor(typeof(TestConnector), schema);

			// Act
			// Assert
			Assert.True(descriptor.SupportsAllCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages));
		}

		private static ConnectorDescriptor CreateTestDescriptor()
		{
			return new ConnectorDescriptor(typeof(TestConnector), CreateTestSchema());
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

		private class ComplexNamedConnector : IChannelConnector
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