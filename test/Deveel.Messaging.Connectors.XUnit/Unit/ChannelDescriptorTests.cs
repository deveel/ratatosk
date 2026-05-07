using System.ComponentModel.DataAnnotations;
using Deveel.Messaging;
using Xunit;

namespace Deveel.Messaging.XUnit
{
	/// <summary>
	/// Tests for the ConnectorDescriptor class functionality.
	/// </summary>
	[Trait("Category", "Unit")]
	[Trait("Layer", "Application")]
	[Trait("Feature", "ChannelDescriptor")]
	public class ConnectorDescriptorTests
	{
		[Fact]
		public void Should_SetProperties_When_ConstructorWithValidParameters()
		{
			// Arrange
			var connectorType = typeof(TestConnector);
			var schema = CreateTestSchema();

			// Act
			var descriptor = new ConnectorDescriptor(connectorType, schema);

			// Assert
			Assert.Equal(connectorType, descriptor.ConnectorType);
			Assert.Equal(schema, descriptor.Schema);
			Assert.Equal("TestProvider", descriptor.ChannelProvider);
			Assert.Equal("TestType", descriptor.ChannelType);
		}

		[Fact]
		public void Should_ThrowException_When_ConstructorWithNullConnectorType()
		{
			// Arrange
			var schema = CreateTestSchema();

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => new ConnectorDescriptor(null!, schema));
		}

		[Fact]
		public void Should_ThrowException_When_ConstructorWithNullSchema()
		{
			// Arrange
			var connectorType = typeof(TestConnector);

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => new ConnectorDescriptor(connectorType, null!));
		}

		[Fact]
		public void Should_ReturnSchemaDisplayName_When_DisplayNameWhenSchemaHasDisplayName()
		{
			// Arrange
			var connectorType = typeof(TestConnector);
			var schema = new ChannelSchema("TestProvider", "TestType", "1.0.0")
				.WithDisplayName("Custom Display Name")
				.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
				.HandlesMessageEndpoint(EndpointType.PhoneNumber)
				.AddContentType(MessageContentType.PlainText)
				.AddAuthenticationType(AuthenticationType.Basic);
			var descriptor = new ConnectorDescriptor(connectorType, schema);

			// Act
			var displayName = descriptor.DisplayName;

			// Assert
			Assert.Equal("Custom Display Name", displayName);
		}

		[Fact]
		public void Should_ReturnConnectorTypeName_When_DisplayNameWhenSchemaHasNoDisplayName()
		{
			// Arrange
			var connectorType = typeof(TestConnector);
			var schema = CreateTestSchema();
			var descriptor = new ConnectorDescriptor(connectorType, schema);

			// Act
			var displayName = descriptor.DisplayName;

			// Assert
			Assert.Equal("TestConnector", displayName);
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsCapabilityWithSupportedCapability()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsCapability(ChannelCapability.SendMessages);

			// Assert
			Assert.True(supports);
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsCapabilityWithUnsupportedCapability()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsCapability(ChannelCapability.Templates);

			// Assert
			Assert.False(supports);
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsAnyCapabilityWithOneSupportedCapability()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsAnyCapability(ChannelCapability.SendMessages | ChannelCapability.Templates);

			// Assert
			Assert.True(supports);
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsAnyCapabilityWithNoSupportedCapabilities()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsAnyCapability(ChannelCapability.Templates | ChannelCapability.MediaAttachments);

			// Assert
			Assert.False(supports);
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsAllCapabilitiesWithAllSupportedCapabilities()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsAllCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages);

			// Assert
			Assert.True(supports);
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsAllCapabilitiesWithPartiallySupported()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsAllCapabilities(ChannelCapability.SendMessages | ChannelCapability.Templates);

			// Assert
			Assert.False(supports);
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsContentTypeWithSupportedContentType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsContentType(MessageContentType.PlainText);

			// Assert
			Assert.True(supports);
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsContentTypeWithUnsupportedContentType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsContentType(MessageContentType.Html);

			// Assert
			Assert.False(supports);
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsEndpointTypeWithSupportedEndpointType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsEndpointType(EndpointType.PhoneNumber);

			// Assert
			Assert.True(supports);
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsEndpointTypeWithUnsupportedEndpointType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsEndpointType(EndpointType.EmailAddress);

			// Assert
			Assert.False(supports);
		}

		[Fact]
		public void Should_ReturnTrue_When_SupportsAuthenticationTypeWithSupportedAuthenticationType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsAuthenticationType(AuthenticationType.Basic);

			// Assert
			Assert.True(supports);
		}

		[Fact]
		public void Should_ReturnFalse_When_SupportsAuthenticationTypeWithUnsupportedAuthenticationType()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var supports = descriptor.SupportsAuthenticationType(AuthenticationType.ApiKey);

			// Assert
			Assert.False(supports);
		}

		[Fact]
		public void Should_ReturnCorrectFormat_When_GetLogicalIdentityIsInvoked()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var identity = descriptor.GetLogicalIdentity();

			// Assert
			Assert.Equal("TestProvider/TestType/1.0.0", identity);
		}

		[Fact]
		public void Should_ReturnFormattedString_When_ToStringIsInvoked()
		{
			// Arrange
			var descriptor = CreateTestDescriptor();

			// Act
			var result = descriptor.ToString();

			// Assert
			Assert.Equal("TestConnector (TestProvider/TestType/1.0.0)", result);
		}

		[Fact]
		public void Should_ReturnTrue_When_EqualsWithSameConnectorType()
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
		public void Should_ReturnFalse_When_EqualsWithDifferentConnectorType()
		{
			// Arrange
			var descriptor1 = CreateTestDescriptor();
			var descriptor2 = new ConnectorDescriptor(typeof(string), CreateTestSchema());

			// Act
			// Assert
			Assert.False(descriptor1.Equals(descriptor2));
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

		// Simple test connector class that doesn't need to implement IChannelConnector fully
		private class TestConnector
		{
		}
	}
}