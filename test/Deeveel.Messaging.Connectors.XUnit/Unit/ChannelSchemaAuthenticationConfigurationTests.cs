using System.ComponentModel.DataAnnotations;

namespace Deveel.Messaging;

/// <summary>
/// Tests for the new authentication configuration functionality in the <see cref="ChannelSchema"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "ChannelSchemaAuthenticationConfiguration")]
public class ChannelSchemaAuthenticationConfigurationTests
{
	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsTwilioAuthConfigurationValidCredentials()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Twilio", "SMS", "1.0.0")
			.AddAuthenticationConfiguration(AuthenticationConfigurations.TwilioBasicAuthentication())
			.AddRequiredParameter("AccountSid", DataType.String)
			.AddRequiredParameter("AuthToken", DataType.String).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("AccountSid", "AC123456789")
			.SetParameter("AuthToken", "auth_token_123");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsCustomBasicAuthConfigurationValidCredentials()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("CustomProvider", "API", "1.0.0")
			.AddAuthenticationConfiguration(AuthenticationConfigurations.CustomBasicAuthentication("UserId", "SecretKey"))
			.AddRequiredParameter("UserId", DataType.String)
			.AddRequiredParameter("SecretKey", DataType.String).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("UserId", "user123")
			.SetParameter("SecretKey", "secret456");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsFlexibleApiKeyConfigurationValidKey()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "API", "1.0.0")
			.AddAuthenticationConfiguration(AuthenticationConfigurations.FlexibleApiKeyAuthentication("ApiKey", "Key", "AccessKey")).Build();

		// Test different key parameter names
		var testCases = new[]
		{
			new { ParamName = "ApiKey", Value = "api_key_123" },
			new { ParamName = "Key", Value = "key_456" },
			new { ParamName = "AccessKey", Value = "access_key_789" }
		};

		foreach (var testCase in testCases)
		{
			var connectionSettings = new ConnectionSettings()
				.SetParameter(testCase.ParamName, testCase.Value);

			// Act
			var results = schema.ValidateConnectionSettings(connectionSettings);

			// Assert
			Assert.Empty(results);
		}
	}

	[Fact]
	public void Should_FailValidation_When_ValidateConnectionSettingsAuthConfigurationMissingRequiredField()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Twilio", "SMS", "1.0.0")
			.AddAuthenticationConfiguration(AuthenticationConfigurations.TwilioBasicAuthentication()).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("AccountSid", "AC123456789");
			// Missing AuthToken

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Twilio Basic Authentication", results[0].ErrorMessage);
		Assert.Contains("Required authentication field 'AuthToken'", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsMultipleAuthConfigurationsOneValid()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Flexible", "API", "1.0.0")
			.AddAuthenticationConfiguration(AuthenticationConfigurations.BasicAuthentication())
			.AddAuthenticationConfiguration(AuthenticationConfigurations.ApiKeyAuthentication())
			.AddAuthenticationConfiguration(AuthenticationConfigurations.TokenAuthentication()).Build();

		// Provide only API Key authentication
		var connectionSettings = new ConnectionSettings()
			.SetParameter("ApiKey", "api_key_12345");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results); // Should pass because API Key auth is satisfied
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsCustomAuthConfigurationValidFields()
	{
		// Arrange
		var requiredFields = new[]
		{
			new AuthenticationField("TenantId", DataType.String) 
			{ 
				DisplayName = "Tenant ID", 
				Description = "The tenant identifier",
				AuthenticationRole = "TenantId"
			},
			new AuthenticationField("ApiSecret", DataType.String) 
			{ 
				DisplayName = "API Secret", 
				Description = "The secret key for the tenant",
				AuthenticationRole = "Secret",
				IsSensitive = true
			}
		};

		var optionalFields = new[]
		{
			new AuthenticationField("Region", DataType.String) 
			{ 
				DisplayName = "Region", 
				Description = "The deployment region",
				AuthenticationRole = "Region"
			}
		};

		var customAuth = AuthenticationConfigurations.CustomAuthentication(
			"Multi-Tenant Authentication", 
			requiredFields, 
			optionalFields);

		var schema = new ChannelSchemaBuilder("CustomProvider", "API", "1.0.0")
			.AddAuthenticationConfiguration(customAuth).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("TenantId", "tenant123")
			.SetParameter("ApiSecret", "secret456")
			.SetParameter("Region", "us-east-1");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsFlexibleCertificateAuthValidCertificateThumbprint()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Secure", "API", "1.0.0")
			.AddAuthenticationConfiguration(AuthenticationConfigurations.FlexibleCertificateAuthentication()).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("CertificateThumbprint", "1234567890ABCDEF");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsFlexibleCertificateAuthValidPfxFile()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Secure", "API", "1.0.0")
			.AddAuthenticationConfiguration(AuthenticationConfigurations.FlexibleCertificateAuthentication()).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("PfxFile", "/path/to/cert.pfx")
			.SetParameter("PfxPassword", "password123");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_ThrowException_When_AddAuthenticationConfigurationDuplicateAuthenticationType()
	{
		// Arrange
		var builder = new ChannelSchemaBuilder("Provider", "Type", "1.0.0");
		builder.AddAuthenticationConfiguration(AuthenticationConfigurations.BasicAuthentication());
		var schema = builder.Build();

		// Act
		// Assert
		var exception = Assert.Throws<InvalidOperationException>(() =>
			builder.AddAuthenticationConfiguration(AuthenticationConfigurations.TwilioBasicAuthentication()));

		Assert.Contains("An authentication configuration for 'Basic' authentication type already exists", exception.Message);
	}

	[Fact]
	public void Should_AuthenticationTypesIncluded_When_AuthenticationConfigurationBackwardCompatibility()
	{
		// Arrange
		// Act
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddAuthenticationConfiguration(AuthenticationConfigurations.BasicAuthentication())
			.AddAuthenticationConfiguration(AuthenticationConfigurations.ApiKeyAuthentication()).Build();

		// Assert
		Assert.Equal(2, schema.AuthenticationConfigurations.Count);
		Assert.Contains(AuthenticationType.Basic, schema.GetAuthenticationTypes());
		Assert.Contains(AuthenticationType.ApiKey, schema.GetAuthenticationTypes());
		
		Assert.Equal(2, schema.AuthenticationConfigurations.Count);
		Assert.Contains(schema.AuthenticationConfigurations, c => c.AuthenticationType == AuthenticationType.Basic);
		Assert.Contains(schema.AuthenticationConfigurations, c => c.AuthenticationType == AuthenticationType.ApiKey);
	}

	[Fact]
	public void Should_RemovesBothConfigurationAndType_When_RemoveAuthenticationConfigurationExistingConfiguration()
	{
		// Arrange
		var builder = new ChannelSchemaBuilder("Provider", "Type", "1.0.0");
		builder.AddAuthenticationConfiguration(AuthenticationConfigurations.BasicAuthentication())
			   .AddAuthenticationConfiguration(AuthenticationConfigurations.ApiKeyAuthentication());

		// Act
		builder.RemoveAuthenticationConfiguration(AuthenticationType.Basic);
		var schema = builder.Build();

		// Assert
		Assert.Single(schema.AuthenticationConfigurations);
		Assert.Single(schema.GetAuthenticationTypes());
		Assert.Contains(AuthenticationType.ApiKey, schema.GetAuthenticationTypes());
		Assert.DoesNotContain(AuthenticationType.Basic, schema.GetAuthenticationTypes());
	}

	[Fact]
	public void Should_ReplacesExisting_When_RestrictAuthenticationConfigurationsNewConfigurations()
	{
		// Arrange
		var builder = new ChannelSchemaBuilder("Provider", "Type", "1.0.0");
		builder.AddAuthenticationConfiguration(AuthenticationConfigurations.BasicAuthentication())
			   .AddAuthenticationConfiguration(AuthenticationConfigurations.ApiKeyAuthentication())
			   .AddAuthenticationConfiguration(AuthenticationConfigurations.TokenAuthentication());

		var restrictedConfigs = new[]
		{
			AuthenticationConfigurations.TwilioBasicAuthentication(),
			AuthenticationConfigurations.FlexibleTokenAuthentication()
		};

		// Act
		builder.RestrictAuthenticationConfigurations(restrictedConfigs);
		var schema = builder.Build();

		// Assert
		Assert.Equal(2, schema.AuthenticationConfigurations.Count);
		Assert.Equal(2, schema.GetAuthenticationTypes().Count());
		Assert.Contains(AuthenticationType.Basic, schema.AuthenticationTypes);
		Assert.Contains(AuthenticationType.Token, schema.AuthenticationTypes);
		Assert.DoesNotContain(AuthenticationType.ApiKey, schema.AuthenticationTypes);
		
		// Verify the configurations are the new ones
		var basicConfig = schema.AuthenticationConfigurations.First(c => c.AuthenticationType == AuthenticationType.Basic);
		Assert.Equal("Twilio Basic Authentication", basicConfig.DisplayName);
	}

	[Fact]
	public void Should_WithConfiguration_When_ValidateConnectionSettingsRealisticTwilioScenario()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Twilio", "SMS", "1.0.0")
			.AddAuthenticationConfiguration(AuthenticationConfigurations.TwilioBasicAuthentication())
			.AddParameter("AccountSid", DataType.String, param =>
			{
				param.IsRequired = true;
				param.Description = "Twilio Account SID";
			})
			.AddParameter("AuthToken", DataType.String, param =>
			{
				param.IsRequired = true;
				param.IsSensitive = true;
				param.Description = "Twilio Auth Token";
			})
			.AddParameter("FromNumber", DataType.String, param =>
			{
				param.IsRequired = true;
				param.Description = "Sender phone number";
			}).Build();

		// Valid Twilio-style configuration
		var validSettings = new ConnectionSettings()
			.SetParameter("AccountSid", "AC123456789abcdef123456789abcdef12")
			.SetParameter("AuthToken", "your_auth_token_here")
			.SetParameter("FromNumber", "+1234567890");

		// Invalid configuration - missing AuthToken
		var invalidSettings = new ConnectionSettings()
			.SetParameter("AccountSid", "AC123456789abcdef123456789abcdef12")
			.SetParameter("FromNumber", "+1234567890");

		// Act
		var validResults = schema.ValidateConnectionSettings(validSettings);
		var invalidResults = schema.ValidateConnectionSettings(invalidSettings).ToList();

		// Assert
		Assert.Empty(validResults);
		Assert.Equal(2, invalidResults.Count); // Missing required parameter + authentication failure
		Assert.Contains(invalidResults, r => r.ErrorMessage!.Contains("Required parameter 'AuthToken'"));
		Assert.Contains(invalidResults, r => r.ErrorMessage!.Contains("Twilio Basic Authentication"));
	}
}