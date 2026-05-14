namespace Deveel.Messaging;

/// <summary>
/// Tests for the <see cref="ChannelSchema"/> class copy and logical identity functionality.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "ChannelSchemaDerived")]
public class ChannelSchemaDerivedTests
{
	[Fact]
	public void Should_CreateCorrectCopy_When_ConstructorWithSourceSchema()
	{
		// Arrange
		var sourceSchema = new ChannelSchemaBuilder("Twilio", "SMS", "1.0.0")
			.WithDisplayName("Twilio SMS Base")
			.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages | ChannelCapability.MessageStatusQuery)
			.AddRequiredParameter("AccountSid", DataType.String)
			.AddRequiredParameter("AuthToken", DataType.String, true)
			.AddContentType(MessageContentType.PlainText)
			.AddContentType(MessageContentType.Media)
			.AddAuthenticationScheme(AuthenticationScheme.Bearer)
			.HandlesMessageEndpoint(EndpointType.PhoneNumber, e =>
			{
				e.CanSend = true;
				e.CanReceive = true;
			})
			.HandlesMessageEndpoint(EndpointType.Url, e =>
			{
				e.CanSend = false;
				e.CanReceive = true;
			})
			.AddMessageProperty("MessageType", DataType.String).Build();

		// Act
		var copiedSchema = ChannelSchemaBuilder.From(sourceSchema, "Custom Twilio SMS").Build();

		// Assert
		Assert.Equal("Twilio", copiedSchema.ChannelProvider);
		Assert.Equal("SMS", copiedSchema.ChannelType);
		Assert.Equal("1.0.0", copiedSchema.Version);
		Assert.Equal("Custom Twilio SMS", copiedSchema.DisplayName);
		Assert.Equal(sourceSchema.Capabilities, copiedSchema.Capabilities);
		
		// Verify logical compatibility
		Assert.True(sourceSchema.IsCompatibleWith(copiedSchema));
		Assert.True(copiedSchema.IsCompatibleWith(sourceSchema));
		Assert.Equal(sourceSchema.GetLogicalIdentity(), copiedSchema.GetLogicalIdentity());

		// Verify all collections are copied
		Assert.Equal(sourceSchema.Parameters.Count, copiedSchema.Parameters.Count);
		Assert.Equal(sourceSchema.MessageProperties.Count, copiedSchema.MessageProperties.Count);
		Assert.Equal(sourceSchema.ContentTypes.Count, copiedSchema.ContentTypes.Count);
		Assert.Equal(sourceSchema.AuthenticationConfigurations.Count, copiedSchema.AuthenticationConfigurations.Count);
		Assert.Equal(sourceSchema.Endpoints.Count, copiedSchema.Endpoints.Count);

		// Verify parameters are copied
		Assert.Contains(copiedSchema.Parameters, p => p.Name == "AccountSid" && p.IsRequired);
		Assert.Contains(copiedSchema.Parameters, p => p.Name == "AuthToken" && p.IsSensitive);

		// Verify content types are copied
		Assert.Contains(MessageContentType.PlainText, copiedSchema.ContentTypes);
		Assert.Contains(MessageContentType.Media, copiedSchema.ContentTypes);

		// Verify authentication types are copied
		Assert.Contains(AuthenticationScheme.Bearer, copiedSchema.AuthenticationSchemes);

		// Verify endpoints are copied
		Assert.Contains(copiedSchema.Endpoints, e => e.Type == EndpointType.PhoneNumber && e.CanSend && e.CanReceive);
		Assert.Contains(copiedSchema.Endpoints, e => e.Type == EndpointType.Url && !e.CanSend && e.CanReceive);

		// Verify message properties are copied
		Assert.Contains(copiedSchema.MessageProperties, p => p.Name == "MessageType" && !p.IsRequired);
	}

	[Fact]
	public void Should_CreateDefaultDisplayName_When_ConstructorWithSourceSchemaAndNoDisplayName()
	{
		// Arrange
		var sourceSchema = new ChannelSchemaBuilder("Twilio", "SMS", "1.0.0")
			.WithDisplayName("Twilio SMS Base").Build();

		// Act
		var copiedSchema = ChannelSchemaBuilder.From(sourceSchema).Build();

		// Assert
		Assert.Equal("Twilio SMS Base (Copy)", copiedSchema.DisplayName);
		Assert.Equal(sourceSchema.ChannelProvider, copiedSchema.ChannelProvider);
		Assert.Equal(sourceSchema.ChannelType, copiedSchema.ChannelType);
		Assert.Equal(sourceSchema.Version, copiedSchema.Version);
		Assert.Equal(sourceSchema.GetLogicalIdentity(), copiedSchema.GetLogicalIdentity());
	}

	[Fact]
	public void Should_ThrowArgumentNullException_When_ConstructorWithNullSourceSchema()
	{
		// Act
		// Assert
		Assert.Throws<ArgumentNullException>(() => 
			ChannelSchemaBuilder.From(null!, "Custom Display Name").Build());
	}

	[Fact]
	public void Should_FromSourceSchema_When_CopiedSchemaModificationsAreIndependent()
	{
		// Arrange
		var sourceSchema = new ChannelSchemaBuilder("Base", "Base", "1.0.0")
			.AddParameter("SharedParam", DataType.String)
			.AddContentType(MessageContentType.PlainText)
			.HandlesMessageEndpoint(EndpointType.EmailAddress).Build();

		var builder = ChannelSchemaBuilder.From(sourceSchema, "Modified Schema");

		// Act
		builder.AddParameter("NewParam", DataType.Integer);
		builder.AddContentType(MessageContentType.Html);
		builder.HandlesMessageEndpoint(EndpointType.PhoneNumber);
		var copiedSchema = builder.Build();

		// Assert
		Assert.Single(sourceSchema.Parameters);
		Assert.Single(sourceSchema.ContentTypes);
		Assert.Single(sourceSchema.Endpoints);

		// Copied schema should have the new items
		Assert.Equal(2, copiedSchema.Parameters.Count);
		Assert.Equal(2, copiedSchema.ContentTypes.Count);
		Assert.Equal(2, copiedSchema.Endpoints.Count);

		// Core properties should match (logical identity)
		Assert.Equal(sourceSchema.ChannelProvider, copiedSchema.ChannelProvider);
		Assert.Equal(sourceSchema.ChannelType, copiedSchema.ChannelType);
		Assert.Equal(sourceSchema.Version, copiedSchema.Version);
		Assert.True(sourceSchema.IsCompatibleWith(copiedSchema));
	}

	[Fact]
	public void Should_HaveCorrectProperties_When_BaseSchemaCreatedDirectly()
	{
		// Arrange
		// Act
		var baseSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").Build();

		// Assert
		Assert.Equal("Provider/Type/1.0.0", baseSchema.GetLogicalIdentity());
		Assert.Equal("Provider", baseSchema.ChannelProvider);
		Assert.Equal("Type", baseSchema.ChannelType);
		Assert.Equal("1.0.0", baseSchema.Version);
	}

	[Fact]
	public void Should_CorePropertiesMatchSource_When_CopiedSchemaIsInvoked()
	{
		// Arrange
		var sourceSchema = new ChannelSchemaBuilder("MyProvider", "MyType", "2.0.0")
			.WithDisplayName("Source Schema").Build();

		// Act
		var copiedSchema = ChannelSchemaBuilder.From(sourceSchema, "Copy Schema").Build();

		// Assert
		Assert.Equal(sourceSchema.ChannelProvider, copiedSchema.ChannelProvider);
		Assert.Equal(sourceSchema.ChannelType, copiedSchema.ChannelType);
		Assert.Equal(sourceSchema.Version, copiedSchema.Version);
		Assert.Equal(sourceSchema.GetLogicalIdentity(), copiedSchema.GetLogicalIdentity());
		
		// Display name can be different
		Assert.NotEqual(sourceSchema.DisplayName, copiedSchema.DisplayName);
		Assert.Equal("Copy Schema", copiedSchema.DisplayName);
	}

	[Fact]
	public void Should_CanRestrictSourceCapabilities_When_CopiedSchemaIsInvoked()
	{
		// Arrange
		var sourceSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages | ChannelCapability.Templates).Build();

		// Act
		var copiedSchema = ChannelSchemaBuilder.From(sourceSchema, "Restricted Schema")
			.RestrictCapabilities(ChannelCapability.SendMessages).Build();

		// Assert
		Assert.True(sourceSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
		Assert.True(sourceSchema.Capabilities.HasFlag(ChannelCapability.Templates));
		
		Assert.True(copiedSchema.Capabilities.HasFlag(ChannelCapability.SendMessages));
		Assert.False(copiedSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));
		Assert.False(copiedSchema.Capabilities.HasFlag(ChannelCapability.Templates));
		
		// Core properties should still match
		Assert.Equal(sourceSchema.ChannelProvider, copiedSchema.ChannelProvider);
		Assert.Equal(sourceSchema.ChannelType, copiedSchema.ChannelType);
		Assert.Equal(sourceSchema.Version, copiedSchema.Version);
		Assert.True(sourceSchema.IsCompatibleWith(copiedSchema));
	}

	[Fact]
	public void Should_ReturnTrue_When_IsCompatibleWithSameLogicalIdentity()
	{
		// Arrange
		var schema1 = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithDisplayName("Schema 1").Build();
		var schema2 = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithDisplayName("Schema 2").Build();

		// Act
		// Assert
		Assert.True(schema1.IsCompatibleWith(schema2));
		Assert.True(schema2.IsCompatibleWith(schema1));
		Assert.Equal(schema1.GetLogicalIdentity(), schema2.GetLogicalIdentity());
	}

	[Fact]
	public void Should_ReturnFalse_When_IsCompatibleWithDifferentLogicalIdentity()
	{
		// Arrange
		var schema1 = new ChannelSchemaBuilder("Provider1", "Type", "1.0.0").Build();
		var schema2 = new ChannelSchemaBuilder("Provider2", "Type", "1.0.0").Build();
		var schema3 = new ChannelSchemaBuilder("Provider1", "Type", "2.0.0").Build();

		// Act
		// Assert
		Assert.False(schema1.IsCompatibleWith(schema2));
		Assert.False(schema1.IsCompatibleWith(schema3));
		Assert.NotEqual(schema1.GetLogicalIdentity(), schema2.GetLogicalIdentity());
		Assert.NotEqual(schema1.GetLogicalIdentity(), schema3.GetLogicalIdentity());
	}

	[Fact]
	public void Should_ReturnEmpty_When_ValidateAsRestrictionOfValidRestriction()
	{
		// Arrange
		var baseSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
			.AddParameter("Param1", DataType.String)
			.AddParameter("Param2", DataType.String)
			.AddContentType(MessageContentType.PlainText)
			.AddContentType(MessageContentType.Html).Build();

		var restrictedSchema = ChannelSchemaBuilder.From(baseSchema, "Restricted")
			.RestrictCapabilities(ChannelCapability.SendMessages)
			.RemoveParameter("Param2")
			.RestrictContentTypes(MessageContentType.PlainText).Build();

		// Act
		var validationResults = restrictedSchema.ValidateAsRestrictionOf(baseSchema);

		// Assert
		Assert.Empty(validationResults);
	}

	[Fact]
	public void Should_ReturnValidationError_When_ValidateAsRestrictionOfIncompatibleSchema()
	{
		// Arrange
		var schema1 = new ChannelSchemaBuilder("Provider1", "Type", "1.0.0").Build();
		var schema2 = new ChannelSchemaBuilder("Provider2", "Type", "1.0.0").Build();

		// Act
		var validationResults = schema1.ValidateAsRestrictionOf(schema2).ToList();

		// Assert
		Assert.Single(validationResults);
		Assert.Contains("Schema is not compatible", validationResults[0].ErrorMessage);
	}

	[Fact]
	public void Should_CanRemoveSourceCapabilities_When_DerivedSchemaIsInvoked()
	{
		// Arrange
		var baseSchema = new ChannelSchemaBuilder("TestProvider", "SMS", "1.0.0")
			.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
			.HandlesMessageEndpoint(EndpointType.PhoneNumber)
			.HandlesMessageEndpoint(EndpointType.Url)
			.AddContentType(MessageContentType.PlainText)
			.AddContentType(MessageContentType.Media).Build();

		// Act
		var derivedSchema = ChannelSchemaBuilder.From(baseSchema, "SMS Send Only")
			.RemoveCapability(ChannelCapability.ReceiveMessages)
			.RemoveContentType(MessageContentType.Media).Build();

		// Assert
		Assert.Equal("TestProvider", derivedSchema.ChannelProvider);
		Assert.Equal("SMS", derivedSchema.ChannelType);
		Assert.Equal("1.0.0", derivedSchema.Version);
		Assert.Equal("SMS Send Only", derivedSchema.DisplayName);

		// Verify capability restriction
		Assert.Equal(ChannelCapability.SendMessages, derivedSchema.Capabilities);
		Assert.False(derivedSchema.Capabilities.HasFlag(ChannelCapability.ReceiveMessages));

		// Verify content type restriction
		Assert.Single(derivedSchema.ContentTypes);
		Assert.Contains(MessageContentType.PlainText, derivedSchema.ContentTypes);
		Assert.DoesNotContain(MessageContentType.Media, derivedSchema.ContentTypes);

		// Verify endpoints are copied
		Assert.Equal(2, derivedSchema.Endpoints.Count);
		Assert.Equal(baseSchema.Endpoints.Count, derivedSchema.Endpoints.Count);
	}

	[Fact]
	public void Should_CanUpdateSourceParameter_When_DerivedSchemaIsInvoked()
	{
		// Arrange
		var baseSchema = new ChannelSchemaBuilder("TestProvider", "Multi", "1.0.0")
			.WithCapabilities(ChannelCapability.SendMessages | ChannelCapability.ReceiveMessages)
			.HandlesMessageEndpoint(EndpointType.EmailAddress)
			.AddContentType(MessageContentType.PlainText)
			.AddRequiredParameter("TestParam", DataType.String).Build();

		// Act
		var derivedSchema = ChannelSchemaBuilder.From(baseSchema, "Modified Schema")
			.UpdateParameter("TestParam", param => 
			{
				param.IsRequired = false;
				param.DefaultValue = "default";
			})
			.RemoveEndpoint(EndpointType.EmailAddress).Build();

		// Assert
		Assert.Equal("TestProvider", derivedSchema.ChannelProvider);
		Assert.Equal("Multi", derivedSchema.ChannelType);
		Assert.Equal("1.0.0", derivedSchema.Version);
		Assert.Equal("Modified Schema", derivedSchema.DisplayName);

		// Verify parameter is updated
		var testParam = derivedSchema.Parameters.Single(p => p.Name == "TestParam");
		Assert.False(testParam.IsRequired);
		Assert.Equal("default", testParam.DefaultValue);

		// Verify endpoint is removed
		Assert.Empty(derivedSchema.Endpoints);
	}
}