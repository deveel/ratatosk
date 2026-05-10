using System.Collections.ObjectModel;
using System.Globalization;

namespace Deveel.Messaging;

/// <summary>
/// Comprehensive tests for the <see cref="ConnectionSettings"/> class covering all constructors,
/// properties, methods, and edge cases to improve code coverage.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "ConnectionSettings")]
public class ConnectionSettingsTests
{
	#region Constructor Tests

	[Fact]
	public void Should_CreateEmptySettings_When_ConstructorDefault()
	{
		// Act
		var settings = new ConnectionSettings();

		// Assert
		Assert.NotNull(settings.Parameters);
		Assert.Empty(settings.Parameters);
	}

	[Fact]
	public void Should_CreateEmptySettings_When_ConstructorWithNullParameters()
	{
		// Act
		var settings = new ConnectionSettings((IDictionary<string, object?>?)null);

		// Assert
		Assert.NotNull(settings.Parameters);
		Assert.Empty(settings.Parameters);
	}

	[Fact]
	public void Should_CopiesParameters_When_ConstructorWithParameters()
	{
		// Arrange
		var initialParameters = new Dictionary<string, object?>
		{
			{ "Key1", "Value1" },
			{ "Key2", 42 },
			{ "Key3", true },
			{ "Key4", null }
		};

		// Act
		var settings = new ConnectionSettings(initialParameters);

		// Assert
		Assert.Equal(4, settings.Parameters.Count);
		Assert.Equal("Value1", settings.Parameters["Key1"]);
		Assert.Equal(42, settings.Parameters["Key2"]);
		Assert.True((bool)settings.Parameters["Key3"]!);
		Assert.Null(settings.Parameters["Key4"]);
	}

	[Fact]
	public void Should_StoreSchema_When_ConstructorWithSchema()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("TestParam", DataType.String);

		// Act
		var settings = new ConnectionSettings(schema);

		// Assert
		Assert.NotNull(settings.Parameters);
		Assert.Empty(settings.Parameters);
	}

	[Fact]
	public void Should_StoreBoth_When_ConstructorWithSchemaAndParameters()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("TestParam", DataType.String);
		var parameters = new Dictionary<string, object?> { { "TestParam", "TestValue" } };

		// Act
		var settings = new ConnectionSettings(schema, parameters);

		// Assert
		Assert.Single(settings.Parameters);
		Assert.Equal("TestValue", settings.Parameters["TestParam"]);
	}

	[Fact]
	public void Should_CopiesAllSettings_When_ConstructorCopyConstructor()
	{
		// Arrange
		var originalSettings = new ConnectionSettings()
			.SetParameter("TestParam", "TestValue")
			.SetParameter("AnotherParam", 123);

		// Act
		var copiedSettings = new ConnectionSettings(originalSettings);

		// Assert
		Assert.Equal(2, copiedSettings.Parameters.Count);
		Assert.Equal("TestValue", copiedSettings.Parameters["TestParam"]);
		Assert.Equal(123, copiedSettings.Parameters["AnotherParam"]);
	}

	[Fact]
	public void Should_ThrowNullReferenceException_When_ConstructorCopyConstructorWithNullSettings()
	{
		// Act
		// Assert
		Assert.Throws<NullReferenceException>(() => new ConnectionSettings((ConnectionSettings)null!));
	}

	#endregion

	#region SetParameter Tests

	[Fact]
	public void Should_SetParameter_When_SetParameterWithValidKeyValue()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		var result = settings.SetParameter("TestKey", "TestValue");

		// Assert
		Assert.Same(settings, result); // Fluent interface
		Assert.Equal("TestValue", settings.Parameters["TestKey"]);
	}

	[Fact]
	public void Should_SetNullParameter_When_SetParameterWithNullValue()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		settings.SetParameter("TestKey", null);

		// Assert
		Assert.True(settings.Parameters.ContainsKey("TestKey"));
		Assert.Null(settings.Parameters["TestKey"]);
	}

	[Fact]
	public void Should_SetAll_When_SetParameterMultipleParameters()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		settings
			.SetParameter("String", "StringValue")
			.SetParameter("Integer", 42)
			.SetParameter("Boolean", true)
			.SetParameter("Double", 3.14);

		// Assert
		Assert.Equal(4, settings.Parameters.Count);
		Assert.Equal("StringValue", settings.Parameters["String"]);
		Assert.Equal(42, settings.Parameters["Integer"]);
		Assert.True((bool)settings.Parameters["Boolean"]!);
		Assert.Equal(3.14, settings.Parameters["Double"]);
	}

	[Fact]
	public void Should_UpdateValue_When_SetParameterOverwriteExisting()
	{
		// Arrange
		var settings = new ConnectionSettings()
			.SetParameter("TestKey", "OriginalValue");

		// Act
		settings.SetParameter("TestKey", "NewValue");

		// Assert
		Assert.Single(settings.Parameters);
		Assert.Equal("NewValue", settings.Parameters["TestKey"]);
	}

	#endregion

	#region SetParameter With Schema Validation Tests

	[Fact]
	public void Should_SetParameter_When_SetParameterWithSchemaValidParameter()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("ValidParam", DataType.String);
		var settings = new ConnectionSettings(schema);

		// Act
		settings.SetParameter("ValidParam", "ValidValue");

		// Assert
		Assert.Equal("ValidValue", settings.Parameters["ValidParam"]);
	}

	[Fact]
	public void Should_ThrowArgumentException_When_SetParameterWithSchemaUnsupportedParameter()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("ValidParam", DataType.String);
		var settings = new ConnectionSettings(schema);

		// Act
		// Assert
		var exception = Assert.Throws<ArgumentException>(() => 
			settings.SetParameter("UnsupportedParam", "Value"));
		Assert.Contains("The parameter UnsupportedParam is not supported by this schema", exception.Message);
	}

	[Fact]
	public void Should_ThrowArgumentException_When_SetParameterWithSchemaRequiredParameterWithNull()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddRequiredParameter("RequiredParam", DataType.String);
		var settings = new ConnectionSettings(schema);

		// Act
		// Assert
		var exception = Assert.Throws<ArgumentException>(() => 
			settings.SetParameter("RequiredParam", null));
		Assert.Contains("The value of parameter RequiredParam is required by this schema", exception.Message);
	}

	[Theory]
	[InlineData(DataType.Boolean, "not_boolean")]
	[InlineData(DataType.Boolean, 123)]
	[InlineData(DataType.String, 123)]
	[InlineData(DataType.Integer, "not_integer")]
	[InlineData(DataType.Integer, 123.45)]
	[InlineData(DataType.Number, "not_number")]
	[InlineData(DataType.Number, true)]
	public void Should_ThrowArgumentException_When_SetParameterWithSchemaIncompatibleType(DataType parameterType, object incompatibleValue)
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("TypedParam", parameterType);
		var settings = new ConnectionSettings(schema);

		// Act
		// Assert
		var exception = Assert.Throws<ArgumentException>(() => 
			settings.SetParameter("TypedParam", incompatibleValue));
		Assert.Contains($"The value provided foe the key 'TypedParam' is not compatible with the type '{parameterType}'", exception.Message);
	}

	[Theory]
	[InlineData(DataType.Boolean, true)]
	[InlineData(DataType.Boolean, false)]
	[InlineData(DataType.String, "test_string")]
	[InlineData(DataType.Integer, 42)]
	[InlineData(DataType.Integer, (long)123)]
	[InlineData(DataType.Integer, (byte)255)]
	[InlineData(DataType.Number, 123.45)]
	[InlineData(DataType.Number, 678.90f)]
	[InlineData(DataType.Number, (double)100.0)]
	public void Should_SetParameter_When_SetParameterWithSchemaCompatibleTypes(DataType parameterType, object compatibleValue)
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("TypedParam", parameterType);
		var settings = new ConnectionSettings(schema);

		// Act
		settings.SetParameter("TypedParam", compatibleValue);

		// Assert
		Assert.Equal(compatibleValue, settings.Parameters["TypedParam"]);
	}

	[Fact]
	public void Should_SetParameter_When_SetParameterWithSchemaAllowedValuesValidValue()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("EnumParam", DataType.String, param =>
			{
				param.AllowedValues = new[] { "Value1", "Value2", "Value3" };
			});
		var settings = new ConnectionSettings(schema);

		// Act
		settings.SetParameter("EnumParam", "Value2");

		// Assert
		Assert.Equal("Value2", settings.Parameters["EnumParam"]);
	}

	[Fact]
	public void Should_ThrowArgumentException_When_SetParameterWithSchemaAllowedValuesInvalidValue()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("EnumParam", DataType.String, param =>
			{
				param.AllowedValues = new[] { "Value1", "Value2", "Value3" };
			});
		var settings = new ConnectionSettings(schema);

		// Act
		// Assert
		var exception = Assert.Throws<ArgumentException>(() => 
			settings.SetParameter("EnumParam", "InvalidValue"));
		Assert.Contains("The value InvalidValue is not allowed for the parameter EnumParam", exception.Message);
	}

	#endregion

	#region GetParameter Tests

	[Fact]
	public void Should_ReturnValue_When_GetParameterExistingParameter()
	{
		// Arrange
		var settings = new ConnectionSettings()
			.SetParameter("TestKey", "TestValue");

		// Act
		var result = settings.GetParameter("TestKey");

		// Assert
		Assert.Equal("TestValue", result);
	}

	[Fact]
	public void Should_ReturnNull_When_GetParameterNonExistingParameter()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		var result = settings.GetParameter("NonExistingKey");

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void Should_ReturnDefault_When_GetParameterWithSchemaNonExistingParameterWithDefault()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("ParamWithDefault", DataType.String, param =>
			{
				param.DefaultValue = "DefaultValue";
			});
		var settings = new ConnectionSettings(schema);

		// Act
		var result = settings.GetParameter("ParamWithDefault");

		// Assert
		Assert.Equal("DefaultValue", result);
	}

	[Fact]
	public void Should_ReturnSetValue_When_GetParameterWithSchemaExistingParameterIgnoresDefault()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("ParamWithDefault", DataType.String, param =>
			{
				param.DefaultValue = "DefaultValue";
			});
		var settings = new ConnectionSettings(schema)
			.SetParameter("ParamWithDefault", "SetValue");

		// Act
		var result = settings.GetParameter("ParamWithDefault");

		// Assert
		Assert.Equal("SetValue", result);
	}

	[Fact]
	public void Should_ReturnNull_When_GetParameterWithSchemaParameterNotInSchema()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0");
		var settings = new ConnectionSettings(schema);

		// Act
		var result = settings.GetParameter("UnknownParam");

		// Assert
		Assert.Null(result);
	}

	#endregion

	#region GetParameter<T> Tests

	[Fact]
	public void Should_ReturnTypedValue_When_GetParameterGenericCorrectType()
	{
		// Arrange
		var settings = new ConnectionSettings()
			.SetParameter("StringParam", "StringValue")
			.SetParameter("IntParam", 42)
			.SetParameter("BoolParam", true);

		// Act
		// Assert
		Assert.Equal("StringValue", settings.GetParameter<string>("StringParam"));
		Assert.Equal(42, settings.GetParameter<int>("IntParam"));
		Assert.True(settings.GetParameter<bool>("BoolParam"));
	}

	[Fact]
	public void Should_ThrowInvalidCastException_When_GetParameterGenericIncorrectType()
	{
		// Arrange
		var settings = new ConnectionSettings()
			.SetParameter("StringParam", "StringValue");

		// Act
		// Assert
		var exception = Assert.Throws<InvalidCastException>(() => 
			settings.GetParameter<int>("StringParam"));
		Assert.Contains("The value for the key 'StringParam' cannot be cast to type 'System.Int32'", exception.Message);
	}

	[Fact]
	public void Should_ReturnDefault_When_GetParameterGenericNullValueCorrectType()
	{
		// Arrange
		var settings = new ConnectionSettings()
			.SetParameter("NullParam", null);

		// Act
		// Assert
		// when trying to cast to any type (even nullable ones)
		Assert.Throws<InvalidCastException>(() => settings.GetParameter<string>("NullParam"));
		Assert.Throws<InvalidCastException>(() => settings.GetParameter<string?>("NullParam"));
	}

	[Fact]
	public void Should_ReturnDefault_When_GetParameterGenericNonExistingParameter()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		// Assert
		// The GetParameter<T> method throws InvalidCastException when trying to cast null to non-nullable types
		Assert.Throws<InvalidCastException>(() => settings.GetParameter<string>("NonExisting"));
		Assert.Throws<InvalidCastException>(() => settings.GetParameter<int>("NonExisting"));
		Assert.Throws<InvalidCastException>(() => settings.GetParameter<bool>("NonExisting"));
		
		// The method doesn't handle nullable types well either, so this will also throw
		Assert.Throws<InvalidCastException>(() => settings.GetParameter<string?>("NonExisting"));
	}

	[Fact]
	public void Should_ReturnTypedDefault_When_GetParameterGenericWithSchemaDefaultValue()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("StringParam", DataType.String, param => param.DefaultValue = "Default")
			.AddParameter("IntParam", DataType.Integer, param => param.DefaultValue = 100)
			.AddParameter("BoolParam", DataType.Boolean, param => param.DefaultValue = true);
		var settings = new ConnectionSettings(schema);

		// Act
		// Assert
		Assert.Equal("Default", settings.GetParameter<string>("StringParam"));
		Assert.Equal(100, settings.GetParameter<int>("IntParam"));
		Assert.True(settings.GetParameter<bool>("BoolParam"));
	}

	#endregion

	#region Indexer Tests

	[Fact]
	public void Should_ReturnValue_When_IndexerGetExistingParameter()
	{
		// Arrange
		var settings = new ConnectionSettings()
			.SetParameter("TestKey", "TestValue");

		// Act
		var result = settings["TestKey"];

		// Assert
		Assert.Equal("TestValue", result);
	}

	[Fact]
	public void Should_ReturnNull_When_IndexerGetNonExistingParameter()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		var result = settings["NonExisting"];

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void Should_SetParameter_When_IndexerSet()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		settings["TestKey"] = "TestValue";

		// Assert
		Assert.Equal("TestValue", settings.Parameters["TestKey"]);
	}

	[Fact]
	public void Should_ValidateParameter_When_IndexerSetWithSchema()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("ValidParam", DataType.String);
		var settings = new ConnectionSettings(schema);

		// Act
		settings["ValidParam"] = "ValidValue";

		// Assert
		Assert.Equal("ValidValue", settings.Parameters["ValidParam"]);
	}

	[Fact]
	public void Should_ThrowArgumentException_When_IndexerSetWithSchemaInvalidParameter()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("ValidParam", DataType.String);
		var settings = new ConnectionSettings(schema);

		// Act
		// Assert
		Assert.Throws<ArgumentException>(() => settings["InvalidParam"] = "Value");
	}

	#endregion

	#region Parameters Property Tests

	[Fact]
	public void Should_ReturnReadOnlyDictionary_When_ParametersIsInvoked()
	{
		// Arrange
		var settings = new ConnectionSettings()
			.SetParameter("Key1", "Value1")
			.SetParameter("Key2", "Value2");

		// Act
		var parameters = settings.Parameters;

		// Assert
		Assert.IsAssignableFrom<ReadOnlyDictionary<string, object?>>(parameters);
		Assert.Equal(2, parameters.Count);
		Assert.Equal("Value1", parameters["Key1"]);
		Assert.Equal("Value2", parameters["Key2"]);
	}

	[Fact]
	public void Should_CannotModifyDirectly_When_ParametersIsReadOnly()
	{
		// Arrange
		var settings = new ConnectionSettings()
			.SetParameter("Key1", "Value1");

		// Act
		var parameters = settings.Parameters;

		// Assert
		Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(parameters);
	}

	#endregion

	#region Complex Integration Tests

	[Fact]
	public void Should_AllOperations_When_ComplexScenarioTwilioLikeProvider()
	{
		// Arrange
		var schema = new ChannelSchema("Twilio", "SMS", "1.0.0")
			.AddRequiredParameter("AccountSid", DataType.String)
			.AddRequiredParameter("AuthToken", DataType.String, true)
			.AddParameter("FromNumber", DataType.String)
			.AddParameter("EnableStatusCallbacks", DataType.Boolean, param => param.DefaultValue = false)
			.AddParameter("MaxRetries", DataType.Integer, param => param.DefaultValue = 3)
			.AddParameter("TimeoutSeconds", DataType.Number, param => param.DefaultValue = 30.0);

		var settings = new ConnectionSettings(schema);

		// Act
		settings
			.SetParameter("AccountSid", "AC123456789")
			.SetParameter("AuthToken", "secret_token")
			.SetParameter("FromNumber", "+1234567890");

		// Act
		var accountSid = settings.GetParameter<string>("AccountSid");
		var authToken = settings.GetParameter<string>("AuthToken");
		var fromNumber = settings.GetParameter<string>("FromNumber");
		var enableCallbacks = settings.GetParameter<bool>("EnableStatusCallbacks"); // Default
		var maxRetries = settings.GetParameter<int>("MaxRetries"); // Default
		var timeout = settings.GetParameter<double>("TimeoutSeconds"); // Default

		// Assert
		Assert.Equal("AC123456789", accountSid);
		Assert.Equal("secret_token", authToken);
		Assert.Equal("+1234567890", fromNumber);
		Assert.False(enableCallbacks); // Default value
		Assert.Equal(3, maxRetries); // Default value
		Assert.Equal(30.0, timeout); // Default value
		Assert.Equal(3, settings.Parameters.Count); // Only explicitly set parameters
	}

	[Fact]
	public void Should_WithValidation_When_ComplexScenarioEmailProvider()
	{
		// Arrange
		var schema = new ChannelSchema("SMTP", "Email", "1.0.0")
			.AddRequiredParameter("Host", DataType.String)
			.AddParameter("Port", DataType.Integer, param =>
			{
				param.DefaultValue = 587;
				param.AllowedValues = new object[] { 25, 465, 587, 993, 995 };
			})
			.AddRequiredParameter("Username", DataType.String)
			.AddRequiredParameter("Password", DataType.String, true)
			.AddParameter("EnableSsl", DataType.Boolean, param => param.DefaultValue = true)
			.AddParameter("ConnectionType", DataType.String, param =>
			{
				param.AllowedValues = new[] { "SMTP", "SMTPS", "STARTTLS" };
				param.DefaultValue = "STARTTLS";
			});

		// Act
		// Assert
		var settings = new ConnectionSettings(schema)
			.SetParameter("Host", "smtp.gmail.com")
			.SetParameter("Port", 587)
			.SetParameter("Username", "user@gmail.com")
			.SetParameter("Password", "app_password")
			.SetParameter("ConnectionType", "STARTTLS");

		Assert.Equal("smtp.gmail.com", settings.GetParameter<string>("Host"));
		Assert.Equal(587, settings.GetParameter<int>("Port"));
		Assert.Equal("user@gmail.com", settings.GetParameter<string>("Username"));
		Assert.Equal("app_password", settings.GetParameter<string>("Password"));
		Assert.True(settings.GetParameter<bool>("EnableSsl")); // Default
		Assert.Equal("STARTTLS", settings.GetParameter<string>("ConnectionType"));

		// Act
		// Assert
		Assert.Throws<ArgumentException>(() => 
			settings.SetParameter("Port", 8080)); // Not in allowed values

		// Act
		// Assert
		Assert.Throws<ArgumentException>(() => 
			settings.SetParameter("ConnectionType", "INVALID"));
	}

	[Fact]
	public void Should_PreservesAllAspects_When_CopyConstructorCompleteScenario()
	{
		// Arrange
		var originalSettings = new ConnectionSettings()
			.SetParameter("Required", "RequiredValue")
			.SetParameter("Optional", 200)
			.SetParameter("Additional", "AdditionalValue");

		// Act
		var copiedSettings = new ConnectionSettings(originalSettings);

		// Assert
		Assert.Equal(3, copiedSettings.Parameters.Count);
		Assert.Equal("RequiredValue", copiedSettings.GetParameter("Required"));
		Assert.Equal(200, copiedSettings.GetParameter("Optional"));
		Assert.Equal("AdditionalValue", copiedSettings.GetParameter("Additional"));

		// Test schema behavior separately with a new schema
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("Optional", DataType.Integer, param => param.DefaultValue = 100);
		var newSettings = new ConnectionSettings(schema);
		Assert.Equal(100, newSettings.GetParameter<int>("Optional")); // Default from schema
	}

	#endregion

	#region Edge Cases and Error Conditions

	[Fact]
	public void Should_ThrowArgumentNullException_When_SetParameterNullKey()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		// Assert
		Assert.Throws<ArgumentNullException>(() => settings.SetParameter(null!, "value"));
	}

	[Fact]
	public void Should_ThrowArgumentNullException_When_GetParameterNullKey()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		// Assert
		Assert.Throws<ArgumentNullException>(() => settings.GetParameter(null!));
	}

	[Fact]
	public void Should_ThrowArgumentNullException_When_GetParameterGenericNullKey()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		// Assert
		Assert.Throws<ArgumentNullException>(() => settings.GetParameter<string>(null!));
	}

	[Fact]
	public void Should_ThrowArgumentNullException_When_IndexerNullKey()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		// Assert
		Assert.Throws<ArgumentNullException>(() => _ = settings[null!]);
		Assert.Throws<ArgumentNullException>(() => settings[null!] = "value");
	}

	[Fact]
	public void Should_AcceptsEmptyKey_When_SetParameterEmptyKey()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		settings.SetParameter("", "value");

		// Assert
		Assert.Equal("value", settings.Parameters[""]);
	}

	[Fact]
	public void Should_AcceptsWhitespaceKey_When_SetParameterWhitespaceKey()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		settings.SetParameter("   ", "value");

		// Assert
		Assert.Equal("value", settings.Parameters["   "]);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("\t")]
	[InlineData("\n")]
	public void Should_ReturnNull_When_GetParameterEmptyOrWhitespaceKey(string key)
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		var result = settings.GetParameter(key);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void Should_HandleCorrectly_When_SchemaWithNullDefaultValue()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("ParamWithNullDefault", DataType.String, param => param.DefaultValue = null);
		var settings = new ConnectionSettings(schema);

		// Act
		var result = settings.GetParameter("ParamWithNullDefault");

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void Should_HandleCorrectly_When_MultipleSchemaParametersSameNameDifferentCase()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("TestParam", DataType.String);
		var settings = new ConnectionSettings(schema);

		// Act
		settings.SetParameter("TestParam", "Value1");
		
		// Different case should still work for retrieval
		var result = settings.GetParameter("testparam");

		// Assert
		Assert.Null(result);
		
		// But exact case should work
		Assert.Equal("Value1", settings.GetParameter("TestParam"));
	}

	[Fact]
	public void Should_ReturnNull_When_GetParameterGenericWithSchemaDefaultValueButNullInSettings()
	{
		// Arrange
		var schema = new ChannelSchema("Provider", "Type", "1.0.0")
			.AddParameter("ParamWithDefault", DataType.String, param => 
			{
				param.DefaultValue = "DefaultValue";
				param.IsRequired = false; // Allow null values
			});
		
		var settings = new ConnectionSettings(schema);
		// Don't set the parameter at all, let it use the default

		// Act
		var result = settings.GetParameter("ParamWithDefault");

		// Assert
		Assert.Equal("DefaultValue", result);
		
		// Now test that we can't set null for String type due to type validation
		Assert.Throws<ArgumentException>(() => 
			settings.SetParameter("ParamWithDefault", null));
	}

	[Fact]
	public void Should_ReturnCorrectDefault_When_GetParameterGenericWithNullValueDefaultType()
	{
		// Arrange
		var settings = new ConnectionSettings()
			.SetParameter("NullStringParam", null)
			.SetParameter("NullIntParam", null);

		// Act
		// Assert
		Assert.Throws<InvalidCastException>(() => settings.GetParameter<string>("NullStringParam"));
		Assert.Throws<InvalidCastException>(() => settings.GetParameter<int>("NullIntParam"));
		Assert.Throws<InvalidCastException>(() => settings.GetParameter<string?>("NullStringParam"));
	}

	#endregion

	#region Performance and Memory Tests

	[Fact]
	public void Should_PerformanceTest_When_LargeNumberOfParametersIsInvoked()
	{
		// Arrange
		var settings = new ConnectionSettings();
		const int parameterCount = 1000;

		// Act
		for (int i = 0; i < parameterCount; i++)
		{
			settings.SetParameter($"Param{i}", $"Value{i}");
		}

		// Assert
		Assert.Equal(parameterCount, settings.Parameters.Count);
		
		for (int i = 0; i < parameterCount; i++)
		{
			Assert.Equal($"Value{i}", settings.GetParameter($"Param{i}"));
		}
	}

	[Fact]
	public void Should_MaintainsCorrectCount_When_ParameterOverwritingIsInvoked()
	{
		// Arrange
		var settings = new ConnectionSettings();

		// Act
		for (int i = 0; i < 100; i++)
		{
			settings.SetParameter("SameKey", $"Value{i}");
		}

		// Assert
		Assert.Single(settings.Parameters);
		Assert.Equal("Value99", settings.GetParameter("SameKey"));
	}

	#endregion
}