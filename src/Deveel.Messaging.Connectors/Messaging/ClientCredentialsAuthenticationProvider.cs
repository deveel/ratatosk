//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Deveel.Messaging
{
    public class ClientCredentialsAuthenticationProvider : AuthenticationProviderBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClientCredentialsAuthenticationProvider> _logger;

        public ClientCredentialsAuthenticationProvider(HttpClient? httpClient = null, ILogger<ClientCredentialsAuthenticationProvider>? logger = null)
            : base(AuthenticationScheme.OAuthClientCredentials, "OAuth 2.0 Client Credentials")
        {
            _httpClient = httpClient ?? new HttpClient();
            _logger = logger ?? NullLogger<ClientCredentialsAuthenticationProvider>.Instance;
        }

        public override async Task<AuthenticationResult> ObtainCredentialAsync(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogObtainingAccessToken();

                var principalFields = configuration.GetFieldsByRole("principal").ToList();
                var credentialFields = configuration.GetFieldsByRole("credential").ToList();

                var clientId = principalFields
                    .Select(f => GetStringParameter(connectionSettings, f.FieldName))
                    .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

                var clientSecret = credentialFields
                    .Select(f => GetStringParameter(connectionSettings, f.FieldName))
                    .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                {
                    return Failure("Client ID and Client Secret are required for OAuth Client Credentials flow", "MISSING_PARAMETERS");
                }

                var tokenEndpoint = GetStringParameter(connectionSettings, "TokenEndpoint");
                if (string.IsNullOrWhiteSpace(tokenEndpoint))
                {
                    return Failure("Token endpoint URL is required for OAuth Client Credentials flow", "MISSING_TOKEN_ENDPOINT");
                }

                var scope = GetStringParameter(connectionSettings, "Scope");

                var tokenRequest = new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret
                };

                if (!string.IsNullOrWhiteSpace(scope))
                {
                    tokenRequest["scope"] = scope;
                }

                var requestContent = new FormUrlEncodedContent(tokenRequest);

                _logger.LogTokenRequestSent(tokenEndpoint);
                var response = await _httpClient.PostAsync(tokenEndpoint, requestContent, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogTokenRequestFailed(response.StatusCode.ToString(), errorContent);
                    return Failure($"Token request failed: {response.StatusCode} - {errorContent}", "TOKEN_REQUEST_FAILED");
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (!tokenResponse.TryGetProperty("access_token", out var accessTokenElement))
                {
                    _logger.LogMissingAccessToken();
                    return Failure("Invalid token response: missing access_token", "INVALID_TOKEN_RESPONSE");
                }

                var accessToken = accessTokenElement.GetString();
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return Failure("Empty access token received", "EMPTY_ACCESS_TOKEN");
                }

                DateTime? expiresAt = null;
                if (tokenResponse.TryGetProperty("expires_in", out var expiresInElement) && expiresInElement.TryGetInt32(out var expiresIn))
                {
                    expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
                }

                var tokenType = "Bearer";
                if (tokenResponse.TryGetProperty("token_type", out var tokenTypeElement))
                {
                    tokenType = tokenTypeElement.GetString() ?? "Bearer";
                }

                var credential = AuthenticationCredential.ForClientCredentials(accessToken, expiresAt, tokenType);

                if (tokenResponse.TryGetProperty("scope", out var scopeElement))
                {
                    credential.Properties["Scope"] = scopeElement.GetString();
                }

                if (tokenResponse.TryGetProperty("refresh_token", out var refreshTokenElement))
                {
                    credential.Properties["RefreshToken"] = refreshTokenElement.GetString();
                }

                _logger.LogTokenObtained(expiresAt?.ToString("O"));
                return Success(credential);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogNetworkErrorDuringTokenRequest(ex);
                return Failure($"Network error: {ex.Message}", "NETWORK_ERROR");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogTokenRequestTimedOut(ex);
                return Failure("Token request timed out", "TIMEOUT");
            }
            catch (JsonException ex)
            {
                _logger.LogTokenParseFailed(ex);
                return Failure($"Invalid JSON response: {ex.Message}", "INVALID_JSON");
            }
            catch (Exception ex)
            {
                _logger.LogUnexpectedTokenError(ex);
                return Failure($"Unexpected error: {ex.Message}", "UNEXPECTED_ERROR");
            }
        }

        public override async Task<AuthenticationResult> RefreshCredentialAsync(AuthenticationCredential existingCredential, ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default)
        {
            try
            {
                if (existingCredential.Properties.TryGetValue("RefreshToken", out var refreshTokenObj) &&
                    refreshTokenObj is string refreshToken && !string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogRefreshingWithRefreshToken();
                    return await RefreshUsingRefreshToken(refreshToken, connectionSettings, cancellationToken);
                }

                _logger.LogNoRefreshTokenAvailable();
                return await ObtainCredentialAsync(connectionSettings, configuration, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogTokenRefreshError(ex);
                return Failure($"Token refresh failed: {ex.Message}", "REFRESH_FAILED");
            }
        }

        private async Task<AuthenticationResult> RefreshUsingRefreshToken(string refreshToken, ConnectionSettings connectionSettings, CancellationToken cancellationToken)
        {
            var clientId = GetStringParameter(connectionSettings, "ClientId");
            var clientSecret = GetStringParameter(connectionSettings, "ClientSecret");
            var tokenEndpoint = GetStringParameter(connectionSettings, "TokenEndpoint");

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(tokenEndpoint))
            {
                return Failure("Missing OAuth parameters for token refresh", "MISSING_PARAMETERS");
            }

            var refreshRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            };

            var requestContent = new FormUrlEncodedContent(refreshRequest);
            var response = await _httpClient.PostAsync(tokenEndpoint, requestContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogTokenRefreshFailedWithStatus(response.StatusCode.ToString(), errorContent);

                _logger.LogRetryingTokenObtainment();
                var principalFields = new[] { "ClientId", "ClientSecret" };
                var config = new AuthenticationConfiguration(AuthenticationScheme.OAuthClientCredentials, "OAuth Client Credentials");
                foreach (var f in principalFields)
                    config.WithField(f, DataType.String, field => field.AuthenticationRole = "principal");
                config.WithField("ClientSecret", DataType.String, field => field.AuthenticationRole = "credential");
                config.WithField("TokenEndpoint", DataType.String, _ => { });

                return await ObtainCredentialAsync(connectionSettings, config, cancellationToken);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (!tokenResponse.TryGetProperty("access_token", out var accessTokenElement))
            {
                return Failure("Invalid refresh response: missing access_token", "INVALID_REFRESH_RESPONSE");
            }

            var accessToken = accessTokenElement.GetString();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Failure("Empty access token received from refresh", "EMPTY_REFRESH_TOKEN");
            }

            DateTime? expiresAt = null;
            if (tokenResponse.TryGetProperty("expires_in", out var expiresInElement) && expiresInElement.TryGetInt32(out var expiresIn))
            {
                expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            }

            var tokenType = "Bearer";
            if (tokenResponse.TryGetProperty("token_type", out var tokenTypeElement))
            {
                tokenType = tokenTypeElement.GetString() ?? "Bearer";
            }

            var credential = AuthenticationCredential.ForClientCredentials(accessToken, expiresAt, tokenType);

            if (tokenResponse.TryGetProperty("refresh_token", out var newRefreshTokenElement))
            {
                credential.Properties["RefreshToken"] = newRefreshTokenElement.GetString();
            }
            else
            {
                credential.Properties["RefreshToken"] = refreshToken;
            }

            _logger.LogTokenRefreshed(expiresAt?.ToString("O"));
            return Success(credential);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
