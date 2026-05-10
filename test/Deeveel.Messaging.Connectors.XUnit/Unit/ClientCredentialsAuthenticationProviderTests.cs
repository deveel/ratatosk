//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Xunit;

namespace Deveel.Messaging
{
    /// <summary>
    /// Additional tests for <see cref="ClientCredentialsAuthenticationProvider"/>
    /// covering error paths and refresh flows.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Application")]
    [Trait("Feature", "ClientCredentialsAuthentication")]
    public class ClientCredentialsAuthenticationProviderTests
    {
        private static ConnectionSettings BuildFullSettings(
            string clientId = "client-id",
            string clientSecret = "client-secret",
            string tokenEndpoint = "https://oauth.example.com/token",
            string? scope = null)
        {
            var settings = new ConnectionSettings()
                .SetParameter("ClientId", clientId)
                .SetParameter("ClientSecret", clientSecret)
                .SetParameter("TokenEndpoint", tokenEndpoint);

            if (scope != null)
                settings = settings.SetParameter("Scope", scope);

            return settings;
        }

        private static MockHttpMessageHandler CreateHandler(int statusCode, string responseBody)
        {
            var handler = new MockHttpMessageHandler();
            handler.SetupResponse(statusCode, responseBody);
            return handler;
        }

        #region ObtainCredentialAsync

        [Fact]
        public async Task Should_ReturnFailure_When_TokenRequestReturnsNonSuccessStatus()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(401, @"{""error"":""invalid_client""}");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();

            // Act
            var result = await provider.ObtainCredentialAsync(settings, cancellationToken);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("TOKEN_REQUEST_FAILED", result.ErrorCode);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_ResponseMissingAccessToken()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{""token_type"":""Bearer""}");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();

            // Act
            var result = await provider.ObtainCredentialAsync(settings, cancellationToken);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_TOKEN_RESPONSE", result.ErrorCode);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_AccessTokenIsEmpty()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{""access_token"":""""}");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();

            // Act
            var result = await provider.ObtainCredentialAsync(settings, cancellationToken);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("EMPTY_ACCESS_TOKEN", result.ErrorCode);
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_ResponseIncludesScope()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{
                ""access_token"": ""scoped-token"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 3600,
                ""scope"": ""read write""
            }");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings(scope: "read write");

            // Act
            var result = await provider.ObtainCredentialAsync(settings, cancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal("read write", result.Credential!.Properties["Scope"]);
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_ResponseIncludesRefreshToken()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{
                ""access_token"": ""access-token-123"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 3600,
                ""refresh_token"": ""refresh-token-456""
            }");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();

            // Act
            var result = await provider.ObtainCredentialAsync(settings, cancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal("refresh-token-456", result.Credential!.Properties["RefreshToken"]);
        }

        [Fact]
        public async Task Should_UseDefaultTokenType_When_ResponseMissingTokenType()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{""access_token"":""token-no-type""}");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();

            // Act
            var result = await provider.ObtainCredentialAsync(settings, cancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal("Bearer", result.Credential!.Properties["TokenType"]);
        }

        [Fact]
        public async Task Should_ReturnNullExpiresAt_When_ResponseMissingExpiresIn()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{""access_token"":""token-no-expiry"",""token_type"":""Bearer""}");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();

            // Act
            var result = await provider.ObtainCredentialAsync(settings, cancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Null(result.Credential!.ExpiresAt);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_NetworkErrorOccurs()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = new FailingHttpMessageHandler(new HttpRequestException("Connection refused"));
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();

            // Act
            var result = await provider.ObtainCredentialAsync(settings, cancellationToken);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("NETWORK_ERROR", result.ErrorCode);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_InvalidJsonResponse()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, "not-valid-json{{{");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();

            // Act
            var result = await provider.ObtainCredentialAsync(settings, cancellationToken);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_JSON", result.ErrorCode);
        }

        #endregion

        #region RefreshCredentialAsync

        [Fact]
        public async Task Should_ObtainNewToken_When_RefreshCredentialAsyncNoRefreshToken()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{""access_token"":""refreshed-token"",""token_type"":""Bearer""}");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();
            var existingCredential = AuthenticationCredential.CreateToken("old-token", null, "Bearer");

            // Act
            var result = await provider.RefreshCredentialAsync(existingCredential, settings, cancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal("refreshed-token", result.Credential!.CredentialValue);
        }

        [Fact]
        public async Task Should_UseRefreshTokenFlow_When_CredentialHasRefreshToken()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{
                ""access_token"": ""refreshed-access"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 1800
            }");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();
            var existingCredential = AuthenticationCredential.CreateToken("old-token", null, "Bearer");
            existingCredential.Properties["RefreshToken"] = "existing-refresh-token";

            // Act
            var result = await provider.RefreshCredentialAsync(existingCredential, settings, cancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal("refreshed-access", result.Credential!.CredentialValue);
            // Should preserve old refresh token when not provided in response
            Assert.Equal("existing-refresh-token", result.Credential.Properties["RefreshToken"]);
        }

        [Fact]
        public async Task Should_UpdateRefreshToken_When_NewRefreshTokenInRefreshResponse()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{
                ""access_token"": ""new-access"",
                ""token_type"": ""Bearer"",
                ""refresh_token"": ""new-refresh-token""
            }");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();
            var existingCredential = AuthenticationCredential.CreateToken("old-token", null, "Bearer");
            existingCredential.Properties["RefreshToken"] = "old-refresh-token";

            // Act
            var result = await provider.RefreshCredentialAsync(existingCredential, settings, cancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal("new-refresh-token", result.Credential!.Properties["RefreshToken"]);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_RefreshResponseMissingAccessToken()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{""token_type"":""Bearer""}");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();
            var existingCredential = AuthenticationCredential.CreateToken("old-token", null, "Bearer");
            existingCredential.Properties["RefreshToken"] = "existing-refresh-token";

            // Act
            var result = await provider.RefreshCredentialAsync(existingCredential, settings, cancellationToken);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_REFRESH_RESPONSE", result.ErrorCode);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_RefreshResponseEmptyAccessToken()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var handler = CreateHandler(200, @"{""access_token"":""""}");
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();
            var existingCredential = AuthenticationCredential.CreateToken("old-token", null, "Bearer");
            existingCredential.Properties["RefreshToken"] = "existing-refresh-token";

            // Act
            var result = await provider.RefreshCredentialAsync(existingCredential, settings, cancellationToken);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("EMPTY_REFRESH_TOKEN", result.ErrorCode);
        }

        [Fact]
        public async Task Should_FallbackToObtainCredential_When_RefreshTokenRequestFails()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            // First response (refresh attempt) = failure; second = success (fallback to obtain)
            var handler = new SequentialHttpMessageHandler(new[]
            {
                (401, @"{""error"":""invalid_refresh_token""}"),
                (200, @"{""access_token"":""fallback-token"",""token_type"":""Bearer""}")
            });
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));
            var settings = BuildFullSettings();
            var existingCredential = AuthenticationCredential.CreateToken("old-token", null, "Bearer");
            existingCredential.Properties["RefreshToken"] = "bad-refresh-token";

            // Act
            var result = await provider.RefreshCredentialAsync(existingCredential, settings, cancellationToken);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal("fallback-token", result.Credential!.CredentialValue);
        }

        #endregion

        #region Dispose

        [Fact]
        public void Should_NotThrow_When_Disposed()
        {
            // Arrange
            var handler = new MockHttpMessageHandler();
            var provider = new ClientCredentialsAuthenticationProvider(new HttpClient(handler));

            // Act & Assert
            var ex = Record.Exception(() => provider.Dispose());
            Assert.Null(ex);
        }

        #endregion
    }

    /// <summary>
    /// HTTP message handler that always throws an exception.
    /// </summary>
    internal sealed class FailingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public FailingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }

    /// <summary>
    /// HTTP message handler that returns responses in sequence.
    /// </summary>
    internal sealed class SequentialHttpMessageHandler : HttpMessageHandler
    {
        private readonly (int StatusCode, string Content)[] _responses;
        private int _index;

        public SequentialHttpMessageHandler((int StatusCode, string Content)[] responses)
        {
            _responses = responses;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_index >= _responses.Length)
                throw new InvalidOperationException("No more responses configured.");

            var (statusCode, content) = _responses[_index++];
            var response = new HttpResponseMessage((System.Net.HttpStatusCode)statusCode)
            {
                Content = new StringContent(content)
            };
            return Task.FromResult(response);
        }
    }
}

