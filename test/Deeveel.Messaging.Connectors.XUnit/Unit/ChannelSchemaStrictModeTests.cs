using System.ComponentModel.DataAnnotations;

namespace Deveel.Messaging;

/// <summary>
/// Tests for the strict mode functionality of the <see cref="ChannelSchema"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "ChannelSchemaStrictMode")]
public class ChannelSchemaStrictModeTests
{
	[Fact]
	public void Should_BeTrue_When_ConstructorDefaultIsStrict()
	{
		// Arrange
		// Act
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").Build();

		// Assert
		Assert.True(schema.IsStrict);
	}

	[Fact]
	public void Should_SetCorrectly_When_ConstructorWithStrictModeConfigured()
	{
		// Arrange
		// Act
		var strictSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").Build();
		var flexibleSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").WithFlexibleMode().Build();

		// Assert
		Assert.True(strictSchema.IsStrict);
		Assert.False(flexibleSchema.IsStrict);
	}

	[Fact]
	public void Should_FromSourceSchema_When_CopyConstructorCopiesStrictMode()
	{
		// Arrange
		var strictSourceSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").Build();
		var flexibleSourceSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").WithFlexibleMode().Build();

		// Act
		var strictCopy = ChannelSchemaBuilder.From(strictSourceSchema, "Strict Copy").Build();
		var flexibleCopy = ChannelSchemaBuilder.From(flexibleSourceSchema, "Flexible Copy").Build();

		// Assert
		Assert.True(strictCopy.IsStrict);
		Assert.False(flexibleCopy.IsStrict);
	}

	[Fact]
	public void Should_SetStrictModeCorrectly_When_WithStrictModeIsInvoked()
	{
		// Arrange
		var builder = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").WithFlexibleMode();

		// Act
		var result = builder.WithStrictMode(true);
		var schema = builder.Build();

		// Assert
		Assert.Same(builder, result);
		Assert.True(schema.IsStrict);
	}

	[Fact]
	public void Should_EnablesStrictMode_When_WithStrictModeNoParameters()
	{
		// Arrange
		var builder = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").WithFlexibleMode();

		// Act
		var result = builder.WithStrictMode();
		var schema = builder.Build();

		// Assert
		Assert.Same(builder, result);
		Assert.True(schema.IsStrict);
	}

	[Fact]
	public void Should_DisablesStrictMode_When_WithFlexibleModeIsInvoked()
	{
		// Arrange
		var builder = new ChannelSchemaBuilder("Provider", "Type", "1.0.0");

		// Act
		var result = builder.WithFlexibleMode();
		var schema = builder.Build();

		// Assert
		Assert.Same(builder, result);
		Assert.False(schema.IsStrict);
	}

	[Fact]
	public void Should_RejectsUnknownParameters_When_ValidateConnectionSettingsStrictMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddParameter("KnownParam", DataType.String).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("KnownParam", "value")
			.SetParameter("UnknownParam", "unknown value");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Unknown parameter 'UnknownParam' is not supported by this schema", results[0].ErrorMessage);
		Assert.Contains("UnknownParam", results[0].MemberNames);
	}

	[Fact]
	public void Should_AllowsUnknownParameters_When_ValidateConnectionSettingsFlexibleMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithFlexibleMode()
			.AddParameter("KnownParam", DataType.String).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("KnownParam", "value")
			.SetParameter("UnknownParam", "unknown value");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_RejectsUnknownProperties_When_ValidateMessageStrictMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddMessageProperty("KnownProperty", DataType.String).Build();

		var message = CreateTestMessage(properties: new Dictionary<string, object?>
		{
			{ "KnownProperty", "value" },
			{ "UnknownProperty", "unknown value" }
		});

		// Act
		var results = schema.ValidateMessage(message).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Unknown message property 'UnknownProperty' is not supported by this schema", results[0].ErrorMessage);
		Assert.Contains("UnknownProperty", results[0].MemberNames);
	}

	[Fact]
	public void Should_AllowsUnknownProperties_When_ValidateMessageFlexibleMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithFlexibleMode()
			.AddMessageProperty("KnownProperty", DataType.String).Build();

		var message = CreateTestMessage(properties: new Dictionary<string, object?>
		{
			{ "KnownProperty", "value" },
			{ "UnknownProperty", "unknown value" }
		});

		// Act
		var results = schema.ValidateMessage(message);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_StillValidatesRequiredParameters_When_ValidateConnectionSettingsStrictMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddRequiredParameter("RequiredParam", DataType.String).Build();

		var connectionSettings = new ConnectionSettings();

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Required parameter 'RequiredParam' is missing", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_StillValidatesRequiredParameters_When_ValidateConnectionSettingsFlexibleMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithFlexibleMode()
			.AddRequiredParameter("RequiredParam", DataType.String).Build();

		var connectionSettings = new ConnectionSettings();

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Required parameter 'RequiredParam' is missing", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_StillValidatesParameterTypes_When_ValidateConnectionSettingsStrictMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddParameter("IntParam", DataType.Integer).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("IntParam", "not an integer");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Parameter 'IntParam' has an incompatible type", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_StillValidatesParameterTypes_When_ValidateConnectionSettingsFlexibleMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithFlexibleMode()
			.AddParameter("IntParam", DataType.Integer).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("IntParam", "not an integer");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Parameter 'IntParam' has an incompatible type", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_StillValidatesRequiredProperties_When_ValidateMessageStrictMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddMessageProperty("RequiredProp", DataType.String, p => p.IsRequired = true).Build();

		var message = CreateTestMessage(properties: new Dictionary<string, object?>());

		// Act
		var results = schema.ValidateMessage(message).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Required message property 'RequiredProp' is missing", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_StillValidatesRequiredProperties_When_ValidateMessageFlexibleMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithFlexibleMode()
			.AddMessageProperty("RequiredProp", DataType.String, p => p.IsRequired = true).Build();

		var message = CreateTestMessage(properties: new Dictionary<string, object?>());

		// Act
		var results = schema.ValidateMessage(message).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Required message property 'RequiredProp' is missing", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_StillValidatesPropertyTypes_When_ValidateMessageStrictMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddMessageProperty("IntProp", DataType.Integer).Build();

		var message = CreateTestMessage(properties: new Dictionary<string, object?>
		{
			{ "IntProp", "not an integer" }
		});

		// Act
		var results = schema.ValidateMessage(message).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Message property 'IntProp' has an incompatible type", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_StillValidatesPropertyTypes_When_ValidateMessageFlexibleMode()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithFlexibleMode()
			.AddMessageProperty("IntProp", DataType.Integer).Build();

		var message = CreateTestMessage(properties: new Dictionary<string, object?>
		{
			{ "IntProp", "not an integer" }
		});

		// Act
		var results = schema.ValidateMessage(message).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Message property 'IntProp' has an incompatible type", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_WorksCorrectly_When_FluentConfigurationStrictModeIntegration()
	{
		// Arrange
		// Act
		var strictSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithDisplayName("Strict Schema")
			.WithStrictMode()
			.AddParameter("Param1", DataType.String)
			.AddMessageProperty("Prop1", DataType.String).Build();

		var flexibleSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithDisplayName("Flexible Schema")
			.WithFlexibleMode()
			.AddParameter("Param1", DataType.String)
			.AddMessageProperty("Prop1", DataType.String).Build();

		// Assert
		Assert.True(strictSchema.IsStrict);
		Assert.False(flexibleSchema.IsStrict);
		Assert.Equal("Strict Schema", strictSchema.DisplayName);
		Assert.Equal("Flexible Schema", flexibleSchema.DisplayName);
	}

	[Fact]
	public void Should_CanChangeBetweenModes_When_StrictModeToggleIsInvoked()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").Build();

		// Act
		// Assert
		Assert.True(schema.IsStrict);

		// Switch to flexible
		schema.IsStrict = false;
		Assert.False(schema.IsStrict);

		// Switch back to strict
		schema.IsStrict = true;
		Assert.True(schema.IsStrict);

		// Switch using boolean parameter
		schema.IsStrict = false;
		Assert.False(schema.IsStrict);

		schema.IsStrict = true;
		Assert.True(schema.IsStrict);
	}

	[Fact]
	public void Should_DemonstratesDifference_When_ComplexValidationScenarioStrictVsFlexible()
	{
		// Arrange
		var strictSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddRequiredParameter("RequiredParam", DataType.String)
			.AddParameter("OptionalParam", DataType.Integer)
			.AddMessageProperty("RequiredProp", DataType.String, p => p.IsRequired = true)
			.AddMessageProperty("OptionalProp", DataType.Boolean).Build();

		var flexibleSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithFlexibleMode()
			.AddRequiredParameter("RequiredParam", DataType.String)
			.AddParameter("OptionalParam", DataType.Integer)
			.AddMessageProperty("RequiredProp", DataType.String, p => p.IsRequired = true)
			.AddMessageProperty("OptionalProp", DataType.Boolean).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("RequiredParam", "valid value")
			.SetParameter("OptionalParam", 123)
			.SetParameter("CustomParam1", "custom value 1")
			.SetParameter("CustomParam2", "custom value 2");

		var messageProperties = new Dictionary<string, object?>
		{
			{ "RequiredProp", "valid value" },
			{ "OptionalProp", true },
			{ "CustomProp1", "custom value 1" },
			{ "CustomProp2", "custom value 2" }
		};
		var validMessage = CreateTestMessage(properties: messageProperties);

		// Act
		var strictConnectionResults = strictSchema.ValidateConnectionSettings(connectionSettings).ToList();
		var strictMessageResults = strictSchema.ValidateMessage(validMessage).ToList();

		// Act
		var flexibleConnectionResults = flexibleSchema.ValidateConnectionSettings(connectionSettings).ToList();
		var flexibleMessageResults = flexibleSchema.ValidateMessage(validMessage).ToList();

		// Assert
		Assert.Equal(2, strictConnectionResults.Count);
		Assert.Contains(strictConnectionResults, r => r.ErrorMessage!.Contains("Unknown parameter 'CustomParam1'"));
		Assert.Contains(strictConnectionResults, r => r.ErrorMessage!.Contains("Unknown parameter 'CustomParam2'"));

		Assert.Equal(2, strictMessageResults.Count);
		Assert.Contains(strictMessageResults, r => r.ErrorMessage!.Contains("Unknown message property 'CustomProp1'"));
		Assert.Contains(strictMessageResults, r => r.ErrorMessage!.Contains("Unknown message property 'CustomProp2'"));

		// Assert
		Assert.Empty(flexibleConnectionResults);
		Assert.Empty(flexibleMessageResults);
	}

	[Fact]
	public void Should_PreservesStrictMode_When_SchemaDerivationIsInvoked()
	{
		// Arrange
		var strictBaseSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddParameter("Param1", DataType.String)
			.AddParameter("Param2", DataType.String).Build();

		var flexibleBaseSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.WithFlexibleMode()
			.AddParameter("Param1", DataType.String)
			.AddParameter("Param2", DataType.String).Build();

		// Act
		var strictDerived = ChannelSchemaBuilder.From(strictBaseSchema, "Strict Derived")
			.RemoveParameter("Param2").Build();

		var flexibleDerived = ChannelSchemaBuilder.From(flexibleBaseSchema, "Flexible Derived")
			.RemoveParameter("Param2").Build();

		// Assert
		Assert.True(strictDerived.IsStrict);
		Assert.False(flexibleDerived.IsStrict);

		// Verify strict derived still rejects unknown parameters
		var connectionSettings = new ConnectionSettings()
			.SetParameter("Param1", "value")
			.SetParameter("UnknownParam", "unknown");

		var strictResults = strictDerived.ValidateConnectionSettings(connectionSettings).ToList();
		var flexibleResults = flexibleDerived.ValidateConnectionSettings(connectionSettings).ToList();

		Assert.Single(strictResults);
		Assert.Contains("Unknown parameter 'UnknownParam'", strictResults[0].ErrorMessage);

		Assert.Empty(flexibleResults);
	}

	[Fact]
	public void Should_CanOverrideStrictMode_When_SchemaDerivationIsInvoked()
	{
		// Arrange
		var strictBaseSchema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").Build();

		// Act
		var flexibleDerived = ChannelSchemaBuilder.From(strictBaseSchema, "Flexible Derived")
			.WithFlexibleMode().Build();

		// Assert
		Assert.True(strictBaseSchema.IsStrict);
		Assert.False(flexibleDerived.IsStrict);
	}

	#region Helper Methods

	private static Message CreateTestMessage(
		string id = "test-message-id",
		IEndpoint? sender = null,
		IEndpoint? receiver = null,
		IMessageContent? content = null,
		IDictionary<string, object?>? properties = null)
	{
		return new Message
		{
			Id = id,
			Sender = sender != null ? new Endpoint(sender) : null,
			Receiver = receiver != null ? new Endpoint(receiver) : null,
			Content = content != null ? MessageContent.Create(content) : new TextContent("Test message"),
			Properties = properties?.ToDictionary(
				kvp => kvp.Key,
				kvp => new MessageProperty(kvp.Key, kvp.Value),
				StringComparer.OrdinalIgnoreCase)
		};
	}

	#endregion
}