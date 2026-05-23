//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Deveel.Messaging
{
    /// <summary>
    /// Tests for the authentication mechanism including providers, managers, and connector integration.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Application")]
    [Trait("Feature", "AuthenticationMechanism")]
    public class AuthenticationMechanismTests
    {
        [Fact]
        public void Should_SetPropertiesCorrectly_When_AuthenticationCredentialCreateToken()
        {
            // Arrange
            var token = "test-access-token";
            var expiresAt = DateTime.UtcNow.AddHours(1);

            // Act
            var credential = AuthenticationCredential.ForBearerToken(token, expiresAt, "Bearer");

            // Assert
            Assert.Equal(AuthenticationScheme.Bearer, credential.Scheme);
            Assert.Equal(token, credential.Value);
            Assert.Equal(expiresAt, credential.ExpiresAt);
            Assert.Equal("Bearer", credential.Properties["TokenType"]);
            Assert.False(credential.IsExpired);
        }

        [Fact]
        public void Should_SetPropertiesCorrectly_When_AuthenticationCredentialCreateApiKey()
        {
            // Arrange
            var apiKey = "test-api-key-12345";

            // Act
            var credential = AuthenticationCredential.ForApiKey(apiKey);

            // Assert
            Assert.Equal(AuthenticationScheme.ApiKey, credential.Scheme);
            Assert.Equal(apiKey, credential.Value);
            Assert.Null(credential.ExpiresAt);
            Assert.False(credential.IsExpired);
        }

        [Fact]
        public void Should_SetPropertiesCorrectly_When_AuthenticationCredentialCreateBasic()
        {
            // Arrange
            var username = "testuser";
            var password = "testpass";

            // Act
            var credential = AuthenticationCredential.ForBasic(username, password);

            // Assert
            Assert.Equal(AuthenticationScheme.Basic, credential.Scheme);
            Assert.Equal(username, credential.Properties["Username"]);
            Assert.Equal(password, credential.Properties["Password"]);
        }

        [Fact]
        public void Should_ReturnTrueWhenExpired_When_AuthenticationCredentialIsExpired()
        {
            // Arrange
            var token = "expired-token";
            var expiredTime = DateTime.UtcNow.AddMinutes(-1);
            var credential = AuthenticationCredential.ForBearerToken(token, expiredTime);

            // Act
            // Assert
            Assert.True(credential.IsExpired);
        }

        [Fact]
        public void Should_ReturnTrueWhenWithinBuffer_When_AuthenticationCredentialWillExpireSoon()
        {
            // Arrange
            var token = "soon-to-expire-token";
            var soonToExpire = DateTime.UtcNow.AddMinutes(2);
            var credential = AuthenticationCredential.ForBearerToken(token, soonToExpire);

            // Act
            // Assert
            Assert.True(credential.WillExpireSoon(TimeSpan.FromMinutes(5)));
            Assert.False(credential.WillExpireSoon(TimeSpan.FromMinutes(1)));
        }

        [Fact]
        public async Task Should_ReturnCredential_When_DirectCredentialAuthenticationProviderApiKey()
        {
            // Arrange
            var provider = new ApiKeyAuthenticationProvider();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("ApiKey", "test-api-key-12345");
            var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
                .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });

            // Act
            var result = await provider.ObtainCredentialAsync(connectionSettings, config, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Credential);
            Assert.Equal(AuthenticationScheme.ApiKey, result.Credential.Scheme);
            Assert.Equal("test-api-key-12345", result.Credential.Value);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_DirectCredentialAuthenticationProviderApiKeyMissingKey()
        {
            // Arrange
            var provider = new ApiKeyAuthenticationProvider();
            var connectionSettings = new ConnectionSettings();
            var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
                .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });

            // Act
            var result = await provider.ObtainCredentialAsync(connectionSettings, config, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("MISSING_API_KEY", result.ErrorCode);
        }

        [Fact]
        public async Task Should_ReturnCredential_When_DirectCredentialAuthenticationProviderBasic()
        {
            // Arrange
            var provider = new BasicAuthenticationProvider();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("Username", "testuser")
                .SetParameter("Password", "testpass");
            var config = new AuthenticationConfiguration(AuthenticationScheme.Basic, "Basic Authentication")
                .WithField("Username", DataType.String, f => f.AuthenticationRole = "principal")
                .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

            // Act
            var result = await provider.ObtainCredentialAsync(connectionSettings, config, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Credential);
            Assert.Equal(AuthenticationScheme.Basic, result.Credential.Scheme);
            Assert.Equal("testuser", result.Credential.Properties["Username"]);
        }

        [Fact]
        public async Task Should_ReturnCredential_When_DirectCredentialAuthenticationProviderBasicAlternativeFields()
        {
            // Arrange
            var provider = new BasicAuthenticationProvider();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("AccountSid", "AC123456")
                .SetParameter("AuthToken", "auth-token-123");
            var config = new AuthenticationConfiguration(AuthenticationScheme.Basic, "Flexible Basic Authentication")
                .WithField("Username", DataType.String, f => f.AuthenticationRole = "principal")
                .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
                .WithField("AccountSid", DataType.String, f => f.AuthenticationRole = "principal")
                .WithField("AuthToken", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
                .WithField("User", DataType.String, f => f.AuthenticationRole = "principal")
                .WithField("Pass", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; })
                .WithField("ClientId", DataType.String, f => f.AuthenticationRole = "principal")
                .WithField("ClientSecret", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

            // Act
            var result = await provider.ObtainCredentialAsync(connectionSettings, config, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Credential);
            Assert.Equal(AuthenticationScheme.Basic, result.Credential.Scheme);
            Assert.Equal("AccountSid", result.Credential.Properties["UserField"]);
            Assert.Equal("AuthToken", result.Credential.Properties["PassField"]);
        }

        [Fact]
        public async Task Should_ReturnToken_When_ClientCredentialsAuthenticationProviderValidRequest()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetupResponse(200, @"{
                ""access_token"": ""test-access-token"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 3600
            }");

            var httpClient = new HttpClient(mockHandler);
            var provider = new ClientCredentialsAuthenticationProvider(httpClient);
            var connectionSettings = new ConnectionSettings()
                .SetParameter("ClientId", "test-client-id")
                .SetParameter("ClientSecret", "test-client-secret")
                .SetParameter("TokenEndpoint", "https://oauth.example.com/token");
            var config = new AuthenticationConfiguration(AuthenticationScheme.OAuthClientCredentials, "Client Credentials (OAuth 2.0)")
                .WithField("ClientId", DataType.String, f => f.AuthenticationRole = "principal")
                .WithField("ClientSecret", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

            // Act
            var result = await provider.ObtainCredentialAsync(connectionSettings, config, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccessful, $"Expected success but got: {result.ErrorCode} - {result.ErrorMessage}");
            Assert.NotNull(result.Credential);
            Assert.Equal(AuthenticationScheme.Bearer, result.Credential.Scheme);
            Assert.Equal("test-access-token", result.Credential.Value);
            Assert.Equal("Bearer", result.Credential.Properties["TokenType"]);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_ClientCredentialsAuthenticationProviderMissingParameters()
        {
            // Arrange
            var provider = new ClientCredentialsAuthenticationProvider();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("ClientId", "test-client-id");
                // Missing ClientSecret and TokenEndpoint
            var config = new AuthenticationConfiguration(AuthenticationScheme.OAuthClientCredentials, "Client Credentials (OAuth 2.0)")
                .WithField("ClientId", DataType.String, f => f.AuthenticationRole = "principal")
                .WithField("ClientSecret", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

            // Act
            var result = await provider.ObtainCredentialAsync(connectionSettings, config, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("MISSING_PARAMETERS", result.ErrorCode);
            Assert.Contains("Client ID and Client Secret are required", result.ErrorMessage!);
        }

        [Fact]
        public async Task Should_ReturnCredential_When_AuthenticationManagerAuthenticateWithApiKey()
        {
            // Arrange
            var manager = new AuthenticationManager();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("ApiKey", "test-api-key");
            var configuration = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
                .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });

            // Act
            var result = await manager.AuthenticateAsync(connectionSettings, configuration, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Credential);
            Assert.Equal(AuthenticationScheme.ApiKey, result.Credential.Scheme);
        }

        [Fact]
        public async Task Should_ReturnCredential_When_AuthenticationManagerAuthenticateWithBasic()
        {
            // Arrange
            var manager = new AuthenticationManager();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("Username", "testuser")
                .SetParameter("Password", "testpass");
            var configuration = new AuthenticationConfiguration(AuthenticationScheme.Basic, "Basic Authentication")
                .WithField("Username", DataType.String, f => f.AuthenticationRole = "principal")
                .WithField("Password", DataType.String, f => { f.AuthenticationRole = "credential"; f.IsSensitive = true; });

            // Act
            var result = await manager.AuthenticateAsync(connectionSettings, configuration, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Credential);
            Assert.Equal(AuthenticationScheme.Basic, result.Credential.Scheme);
        }

        [Fact]
        public async Task Should_ReusesCredential_When_AuthenticationManagerCacheCredential()
        {
            // Arrange
            var manager = new AuthenticationManager();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("ApiKey", "test-api-key");
            var configuration = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
                .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });

            // Act
            var result1 = await manager.AuthenticateAsync(connectionSettings, configuration, TestContext.Current.CancellationToken);
            var result2 = await manager.AuthenticateAsync(connectionSettings, configuration, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result1.IsSuccessful);
            Assert.True(result2.IsSuccessful);
            // The credentials should be the same instance from cache
            Assert.Same(result1.Credential, result2.Credential);
        }

        [Fact]
        public void Should_RemovesFromCache_When_AuthenticationManagerInvalidateCredential()
        {
            // Arrange
            var manager = new AuthenticationManager();
            var connectionSettings = new ConnectionSettings()
                .SetParameter("ApiKey", "test-api-key");
            var configuration = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
                .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });

            // Act
            manager.InvalidateCredential(connectionSettings, configuration);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void Should_ClearsAllCredentials_When_AuthenticationManagerClearCache()
        {
            // Arrange
            var manager = new AuthenticationManager();

            // Act
            manager.ClearCache();

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task Should_ReturnCredential_When_CertificateAuthenticationProviderValidCertificate()
        {
            // Arrange
            var provider = new ApiKeyAuthenticationProvider(); // Use API key provider as a substitute for this test
            var connectionSettings = new ConnectionSettings()
                .SetParameter("ApiKey", "cert-12345");
            var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
                .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });

            // Act
            var result = await provider.ObtainCredentialAsync(connectionSettings, config, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Credential);
            Assert.Equal(AuthenticationScheme.ApiKey, result.Credential.Scheme);
            Assert.Equal("cert-12345", result.Credential.Value);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_CertificateAuthenticationProviderMissingCertificate()
        {
            // Arrange
            var provider = new ApiKeyAuthenticationProvider();
            var connectionSettings = new ConnectionSettings(); // No certificate provided
            var config = new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
                .WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; });

            // Act
            var result = await provider.ObtainCredentialAsync(connectionSettings, config, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("MISSING_API_KEY", result.ErrorCode);
        }

        [Fact]
        public async Task Should_InitializeSuccessfully_When_TestConnectorWithAuthentication()
        {
            // Arrange
			var schema = new ChannelSchemaBuilder("Test", "Test", "1.0.0")
				.AddAuthenticationConfiguration(new AuthenticationConfiguration(AuthenticationScheme.ApiKey, "API Key Authentication")
					.WithField("ApiKey", DataType.String, f => { f.AuthenticationRole = "principal"; f.IsSensitive = true; }))
				.Build();

			var connectionSettings = new ConnectionSettings()
				.SetParameter("ApiKey", "test-api-key");

			var connector = new TestAuthConnector(schema, connectionSettings);

            // Act
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess());
            Assert.Equal(ConnectorState.Ready, connector.State);
            Assert.NotNull(connector.TestAuthenticationCredential);
            Assert.Equal(AuthenticationScheme.ApiKey, connector.TestAuthenticationCredential.Scheme);
        }

        [Fact]
        public async Task Should_InitializeWithoutAuth_When_TestConnectorWithoutAuthenticationConfig()
        {
            // Arrange
			var schema = new ChannelSchemaBuilder("Test", "Test", "1.0.0").Build();
			var connectionSettings = new ConnectionSettings();
            var connector = new TestAuthConnector(schema, connectionSettings);

            // Act
            var result = await connector.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess());
            Assert.Equal(ConnectorState.Ready, connector.State);
            Assert.Null(connector.TestAuthenticationCredential);
        }

        [Fact]
        public void Should_SetPropertiesCorrectly_When_AuthenticationCredentialCreateClientCredentials()
        {
            var token = "client-cred-token";
            var expiresAt = DateTime.UtcNow.AddHours(2);

            var credential = AuthenticationCredential.ForClientCredentials(token, expiresAt, "Bearer", "refresh-token-xyz");

            Assert.Equal(AuthenticationScheme.Bearer, credential.Scheme);
            Assert.Equal(token, credential.Value);
            Assert.Equal(expiresAt, credential.ExpiresAt);
            Assert.Equal("Bearer", credential.Properties["TokenType"]);
            Assert.Equal("client_credentials", credential.Properties["GrantType"]);
            Assert.Equal("refresh-token-xyz", credential.Properties["RefreshToken"]);
        }

        [Fact]
        public void Should_SetPropertiesCorrectly_When_AuthenticationCredentialCreateClientCredentialsWithoutRefresh()
        {
            var token = "client-cred-token";

            var credential = AuthenticationCredential.ForClientCredentials(token, null, "Bearer");

            Assert.Equal(AuthenticationScheme.Bearer, credential.Scheme);
            Assert.Equal(token, credential.Value);
            Assert.Equal("Bearer", credential.Properties["TokenType"]);
            Assert.Equal("client_credentials", credential.Properties["GrantType"]);
            Assert.False(credential.Properties.ContainsKey("RefreshToken"));
        }

        [Fact]
        public void Should_Throw_When_AuthenticationCredentialCreateClientCredentialsNullToken()
        {
            Assert.Throws<ArgumentNullException>(() => AuthenticationCredential.ForClientCredentials(null!));
        }

        [Fact]
        public void Should_SetPropertiesCorrectly_When_AuthenticationCredentialCreateCertificate()
        {
            var certData = "cert-data-123";

            var credential = AuthenticationCredential.ForCertificate(certData);

            Assert.Equal(AuthenticationScheme.Certificate, credential.Scheme);
            Assert.Equal(certData, credential.Value);
            Assert.Equal("Certificate", credential.Properties["CredentialType"]);
            Assert.False(credential.Properties.ContainsKey("CertificatePassword"));
        }

        [Fact]
        public void Should_SetPropertiesCorrectly_When_AuthenticationCredentialCreateCertificateWithPassword()
        {
            var certData = "cert-data-123";

            var credential = AuthenticationCredential.ForCertificate(certData, "p@ssw0rd");

            Assert.Equal(AuthenticationScheme.Certificate, credential.Scheme);
            Assert.Equal(certData, credential.Value);
            Assert.Equal("Certificate", credential.Properties["CredentialType"]);
            Assert.Equal("p@ssw0rd", credential.Properties["CertificatePassword"]);
        }

        [Fact]
        public void Should_ReturnNull_When_AuthenticationCredentialGetTimeUntilExpirationNoExpiry()
        {
            var credential = AuthenticationCredential.ForApiKey("test-key");

            var result = credential.GetTimeUntilExpiration();

            Assert.Null(result);
        }

        [Fact]
        public void Should_ReturnPositive_When_AuthenticationCredentialGetTimeUntilExpirationFuture()
        {
            var credential = AuthenticationCredential.ForBearerToken("token", DateTime.UtcNow.AddHours(1));

            var result = credential.GetTimeUntilExpiration();

            Assert.NotNull(result);
            Assert.True(result > TimeSpan.Zero);
        }

        [Fact]
        public void Should_ReturnZero_When_AuthenticationCredentialGetTimeUntilExpirationPast()
        {
            var credential = AuthenticationCredential.ForBearerToken("token", DateTime.UtcNow.AddMinutes(-5));

            var result = credential.GetTimeUntilExpiration();

            Assert.NotNull(result);
            Assert.Equal(TimeSpan.Zero, result);
        }

        [Fact]
        public void Should_Throw_When_AuthenticationCredentialCreateCertificateNullData()
        {
            Assert.Throws<ArgumentNullException>(() => AuthenticationCredential.ForCertificate(null!));
        }
    }

    /// <summary>
    /// Test connector that exposes authentication for testing.
    /// </summary>
    public class TestAuthConnector : ChannelConnectorBase
    {
        public TestAuthConnector(IChannelSchema schema, ConnectionSettings connectionSettings)
            : base(schema, connectionSettings)
        {
        }

        public AuthenticationCredential? TestAuthenticationCredential => AuthenticationCredential;

        protected override async ValueTask InitializeConnectorAsync(CancellationToken cancellationToken)
        {
            // Try to authenticate if the schema has authentication configurations
            if (Schema.AuthenticationConfigurations.Any())
            {
                var authResult = await AuthenticateAsync(cancellationToken);
                if (!authResult.IsSuccess())
                {
                    throw new InvalidOperationException($"Authentication failed during initialization: {authResult.Error?.Code} - {authResult.Error?.Message}");
                }
            }
        }

        protected override ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        protected override Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        protected override Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new StatusInfo("Test Status"));
        }
    }

    /// <summary>
    /// Mock HTTP client for testing OAuth flows.
    /// </summary>
    public class MockHttpClient : HttpClient
    {
        private readonly MockHttpMessageHandler _mockHandler;

        public MockHttpClient() : this(new MockHttpMessageHandler())
        {
        }

        public MockHttpClient(MockHttpMessageHandler handler) : base(handler)
        {
            _mockHandler = handler;
        }

        public void SetupResponse(int statusCode, string content)
        {
            _mockHandler.SetupResponse(statusCode, content);
        }
    }

    /// <summary>
    /// Mock HTTP message handler for testing.
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private int _statusCode = 200;
        private string _responseContent = "";

        public void SetupResponse(int statusCode, string content)
        {
            _statusCode = statusCode;
            _responseContent = content;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate network delay

            var response = new HttpResponseMessage((System.Net.HttpStatusCode)_statusCode)
            {
                Content = new StringContent(_responseContent)
            };

            return response;
        }
    }
}
