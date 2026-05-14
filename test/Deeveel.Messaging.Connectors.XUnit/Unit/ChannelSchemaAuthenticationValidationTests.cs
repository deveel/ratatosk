using System.ComponentModel.DataAnnotations;

namespace Deveel.Messaging;

/// <summary>
/// Tests for authentication validation functionality in the <see cref="ChannelSchema"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "ChannelSchemaAuthenticationValidation")]
public class ChannelSchemaAuthenticationValidationTests
{
	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsNoAuthenticationTypes()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0").Build();
		var connectionSettings = new ConnectionSettings();

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsNoneAuthenticationType()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddAuthenticationType(AuthenticationType.None).Build();
		var connectionSettings = new ConnectionSettings();

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsBasicAuthTwilioStyleValidCredentials()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Twilio", "SMS", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Basic)
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
	public void Should_PassesValidation_When_ValidateConnectionSettingsBasicAuthStandardStyleValidCredentials()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("SMTP", "Email", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Basic)
			.AddRequiredParameter("Username", DataType.String)
			.AddRequiredParameter("Password", DataType.String, true).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("Username", "user@example.com")
			.SetParameter("Password", "password123");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_FailValidation_When_ValidateConnectionSettingsBasicAuthMissingCredentials()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "Type", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Basic).Build();

		var connectionSettings = new ConnectionSettings();
		// Don't add any parameters to avoid unknown parameter validation issues

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Flexible Basic Authentication", results[0].ErrorMessage);
		Assert.Contains("Authentication", results[0].MemberNames);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsApiKeyAuthValidKey()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.ApiKey)
			.AddRequiredParameter("ApiKey", DataType.String, true).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("ApiKey", "api_key_12345");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsApiKeyAuthAlternativeKeyNames()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.ApiKey).Build();

		// Test different key parameter names
		var testCases = new[]
		{
			new { ParamName = "Key", Value = "key_123" },
			new { ParamName = "AccessKey", Value = "access_key_456" }
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
	public void Should_FailValidation_When_ValidateConnectionSettingsApiKeyAuthMissingKey()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Provider", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.ApiKey).Build();

		var connectionSettings = new ConnectionSettings();

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Flexible API Key Authentication", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsTokenAuthValidToken()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("OAuth", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Token)
			.AddRequiredParameter("AccessToken", DataType.String, true).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("AccessToken", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsTokenAuthAlternativeTokenNames()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("OAuth", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Token).Build();

		var testCases = new[]
		{
			new { ParamName = "Token", Value = "token_123" },
			new { ParamName = "BearerToken", Value = "bearer_456" },
			new { ParamName = "AuthToken", Value = "auth_789" }
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
	public void Should_PassesValidation_When_ValidateConnectionSettingsClientCredentialsAuthValidCredentials()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("OAuth2", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.ClientCredentials)
			.AddRequiredParameter("ClientId", DataType.String)
			.AddRequiredParameter("ClientSecret", DataType.String, true).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("ClientId", "client_12345")
			.SetParameter("ClientSecret", "secret_67890");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_FailValidation_When_ValidateConnectionSettingsClientCredentialsAuthMissingCredentials()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("OAuth2", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.ClientCredentials).Build();

		// Test missing ClientId
		var connectionSettingsMissingId = new ConnectionSettings()
			.SetParameter("ClientSecret", "secret_67890");

		// Test missing ClientSecret
		var connectionSettingsMissingSecret = new ConnectionSettings()
			.SetParameter("ClientId", "client_12345");

		// Act
		// Assert
		var resultsMissingId = schema.ValidateConnectionSettings(connectionSettingsMissingId).ToList();
		Assert.Single(resultsMissingId);
		Assert.Contains("Client Credentials Authentication", resultsMissingId[0].ErrorMessage);

		// Act
		// Assert
		var resultsMissingSecret = schema.ValidateConnectionSettings(connectionSettingsMissingSecret).ToList();
		Assert.Single(resultsMissingSecret);
		Assert.Contains("Client Credentials Authentication", resultsMissingSecret[0].ErrorMessage);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsCertificateAuthValidCertificate()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Secure", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Certificate).Build();

		var testCases = new[]
		{
			new { ParamName = "Certificate", Value = "cert_data_here" },
			new { ParamName = "CertificatePath", Value = "/path/to/cert.pem" },
			new { ParamName = "CertificateThumbprint", Value = "1234567890ABCDEF" },
			new { ParamName = "PfxFile", Value = "/path/to/cert.pfx" }
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
	public void Should_PassesValidation_When_ValidateConnectionSettingsCertificateAuthFileBasedWithPassword()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Secure", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Certificate).Build();

		var connectionSettings = new ConnectionSettings()
			.SetParameter("PfxFile", "/path/to/cert.pfx")
			.SetParameter("PfxPassword", "password123");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsCustomAuthValidCustomParameters()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Custom", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Custom).Build();

		var testCases = new[]
		{
			new { ParamName = "CustomAuth", Value = "custom_auth_data" },
			new { ParamName = "AuthenticationData", Value = "auth_data" },
			new { ParamName = "Credentials", Value = "creds" },
			new { ParamName = "SecretKey", Value = "secret" }
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
	public void Should_PassesValidation_When_ValidateConnectionSettingsMultipleAuthTypesOneValid()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Flexible", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Basic)
			.AddAuthenticationType(AuthenticationType.ApiKey)
			.AddAuthenticationType(AuthenticationType.Token).Build();

		// Provide only API Key authentication
		var connectionSettings = new ConnectionSettings()
			.SetParameter("ApiKey", "api_key_12345");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results); // Should pass because API Key auth is satisfied
	}

	[Fact]
	public void Should_FailValidation_When_ValidateConnectionSettingsMultipleAuthTypesNoneValid()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Flexible", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Basic)
			.AddAuthenticationType(AuthenticationType.ApiKey)
			.AddAuthenticationType(AuthenticationType.Token).Build();

		// Provide no authentication parameters to avoid unknown parameter validation
		var connectionSettings = new ConnectionSettings();

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings).ToList();

		// Assert
		Assert.Single(results);
		Assert.Contains("Connection settings do not satisfy any of the supported authentication methods", results[0].ErrorMessage);
		Assert.Contains("Flexible Basic Authentication, Flexible API Key Authentication, Flexible Token Authentication", results[0].ErrorMessage);
	}

	[Fact]
	public void Should_PassesValidation_When_ValidateConnectionSettingsMultipleAuthTypesWithNoneIncompleteAuth()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Optional", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.None)
			.AddAuthenticationType(AuthenticationType.Basic)
			.AddAuthenticationType(AuthenticationType.ApiKey)
			.AddParameter("SomeOtherParam", DataType.String).Build(); // Define the parameter to avoid unknown parameter error

		// Provide no authentication parameters
		var connectionSettings = new ConnectionSettings()
			.SetParameter("SomeOtherParam", "value");

		// Act
		var results = schema.ValidateConnectionSettings(connectionSettings);

		// Assert
		Assert.Empty(results); // Should pass because None authentication is supported
	}

	[Fact]
	public void Should_RealisticScenario_When_ValidateConnectionSettingsTwilioLikeProvider()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Twilio", "SMS", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Basic)
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
			}).Build();

		// Valid Twilio-style configuration
		var validSettings = new ConnectionSettings()
			.SetParameter("AccountSid", "AC123456789abcdef123456789abcdef12")
			.SetParameter("AuthToken", "your_auth_token_here");

		// Invalid configuration - missing AuthToken
		var invalidSettings = new ConnectionSettings()
			.SetParameter("AccountSid", "AC123456789abcdef123456789abcdef12");

		// Act
		var validResults = schema.ValidateConnectionSettings(validSettings);
		var invalidResults = schema.ValidateConnectionSettings(invalidSettings).ToList();

		// Assert
		Assert.Empty(validResults);
		Assert.Equal(2, invalidResults.Count); // Missing required parameter + authentication failure
		Assert.Contains(invalidResults, r => r.ErrorMessage!.Contains("Required parameter 'AuthToken'"));
		Assert.Contains(invalidResults, r => r.ErrorMessage!.Contains("Flexible Basic Authentication"));
	}

	[Fact]
	public void Should_RealisticScenario_When_ValidateConnectionSettingsEmailProvider()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("SMTP", "Email", "1.0.0")
			.AddAuthenticationType(AuthenticationType.Basic)
			.AddRequiredParameter("Host", DataType.String)
			.AddParameter("Port", DataType.Integer, param => param.DefaultValue = 587 )
			.AddRequiredParameter("Username", DataType.String)
			.AddRequiredParameter("Password", DataType.String, true)
			.AddParameter("EnableSsl", DataType.Boolean, param => param.DefaultValue = true).Build();

		// Valid SMTP configuration
		var validSettings = new ConnectionSettings()
			.SetParameter("Host", "smtp.gmail.com")
			.SetParameter("Port", 587)
			.SetParameter("Username", "user@gmail.com")
			.SetParameter("Password", "app_password_here")
			.SetParameter("EnableSsl", true);

		// Invalid configuration - missing password
		var invalidSettings = new ConnectionSettings()
			.SetParameter("Host", "smtp.gmail.com")
			.SetParameter("Username", "user@gmail.com");

		// Act
		var validResults = schema.ValidateConnectionSettings(validSettings);
		var invalidResults = schema.ValidateConnectionSettings(invalidSettings).ToList();

		// Assert
		Assert.Empty(validResults);
		Assert.Equal(2, invalidResults.Count); // Missing required parameter + authentication failure
		Assert.Contains(invalidResults, r => r.ErrorMessage!.Contains("Required parameter 'Password'"));
		Assert.Contains(invalidResults, r => r.ErrorMessage!.Contains("Flexible Basic Authentication"));
	}

	[Fact]
	public void Should_RealisticScenario_When_ValidateConnectionSettingsOAuthProvider()
	{
		// Arrange
		var schema = new ChannelSchemaBuilder("Google", "API", "1.0.0")
			.AddAuthenticationType(AuthenticationType.ClientCredentials)
			.AddAuthenticationType(AuthenticationType.Token)
			.AddParameter("ClientId", DataType.String)
			.AddParameter("ClientSecret", DataType.String, param => param.IsSensitive = true)
			.AddParameter("AccessToken", DataType.String, param => param.IsSensitive = true)
			.AddParameter("BaseUrl", DataType.String, param => param.DefaultValue = "https://api.google.com").Build();

		// Valid with Client Credentials
		var clientCredentialsSettings = new ConnectionSettings()
			.SetParameter("ClientId", "client_id_123")
			.SetParameter("ClientSecret", "client_secret_456")
			.SetParameter("BaseUrl", "https://api.google.com");

		// Valid with Access Token
		var tokenSettings = new ConnectionSettings()
			.SetParameter("AccessToken", "ya29.access_token_here")
			.SetParameter("BaseUrl", "https://api.google.com");

		// Invalid - neither authentication method satisfied
		var invalidSettings = new ConnectionSettings()
			.SetParameter("BaseUrl", "https://api.google.com");

		// Act
		var clientCredentialsResults = schema.ValidateConnectionSettings(clientCredentialsSettings);
		var tokenResults = schema.ValidateConnectionSettings(tokenSettings);
		var invalidResults = schema.ValidateConnectionSettings(invalidSettings).ToList();

		// Assert
		Assert.Empty(clientCredentialsResults);
		Assert.Empty(tokenResults);
		Assert.Single(invalidResults);
		Assert.Contains("Connection settings do not satisfy any of the supported authentication methods", invalidResults[0].ErrorMessage);
	}
}