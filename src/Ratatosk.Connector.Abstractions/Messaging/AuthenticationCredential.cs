//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Represents an authentication credential obtained from an <see cref="IAuthenticationProvider"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="Value"/> property holds the primary secret (token, key, Base64-encoded
    /// credentials). Additional parts (e.g. username for Basic auth, token type) are stored
    /// in the <see cref="Properties"/> dictionary.
    /// </remarks>
    public class AuthenticationCredential
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="scheme">The scheme this credential was obtained for.</param>
        /// <param name="credentialValue">The primary credential value (token, key, etc.).</param>
        /// <param name="expiresAt">Optional expiration time.</param>
        /// <exception cref="ArgumentNullException"><paramref name="scheme"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="credentialValue"/> is <c>null</c> or empty.</exception>
        public AuthenticationCredential(AuthenticationScheme scheme, string credentialValue, DateTime? expiresAt = null)
        {
            Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            ArgumentException.ThrowIfNullOrWhiteSpace(credentialValue, nameof(credentialValue));
            Value = credentialValue;
            ExpiresAt = expiresAt;
            ObtainedAt = DateTime.UtcNow;
            Properties = new Dictionary<string, object?>();
        }

        /// <summary>
        /// Gets the authentication scheme this credential is for.
        /// </summary>
        public AuthenticationScheme Scheme { get; }

        /// <summary>
        /// Gets the primary credential value (e.g. access token, API key, Base64-encoded credentials).
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the expiration time of this credential, if applicable.
        /// </summary>
        public DateTime? ExpiresAt { get; }

        /// <summary>
        /// Gets the UTC timestamp when this credential was obtained.
        /// </summary>
        public DateTime ObtainedAt { get; }

        /// <summary>
        /// Gets additional properties associated with this credential (e.g. username, token type, refresh token).
        /// </summary>
        public Dictionary<string, object?> Properties { get; }

        /// <summary>
        /// Gets whether this credential has expired.
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;

        /// <summary>
        /// Gets whether this credential will expire within the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The look-ahead window.</param>
        /// <returns><c>true</c> if expiration falls within the window.</returns>
        public bool WillExpireSoon(TimeSpan buffer)
        {
            return ExpiresAt.HasValue && DateTime.UtcNow.Add(buffer) >= ExpiresAt.Value;
        }

        /// <summary>
        /// Gets the remaining time until expiration, or <c>null</c> if the credential never expires.
        /// </summary>
        /// <returns>The remaining time, or <see cref="TimeSpan.Zero"/> if already expired.</returns>
        public TimeSpan? GetTimeUntilExpiration()
        {
            if (!ExpiresAt.HasValue)
                return null;

            var remaining = ExpiresAt.Value - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        /// <summary>
        /// Creates a credential for a bearer or basic token.
        /// </summary>
        /// <param name="token">The token value.</param>
        /// <param name="expiresAt">Optional expiration.</param>
        /// <param name="tokenType">The token type (defaults to <c>"Bearer"</c>).</param>
        /// <returns>A new credential.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="token"/> is <c>null</c> or empty.</exception>
        public static AuthenticationCredential ForBearerToken(string token, DateTime? expiresAt = null, string? tokenType = null)
        {
            var scheme = string.Equals(tokenType, "Basic", StringComparison.OrdinalIgnoreCase)
                ? AuthenticationScheme.Basic
                : AuthenticationScheme.Bearer;

            var credential = new AuthenticationCredential(scheme, token, expiresAt);
            credential.Properties["TokenType"] = tokenType ?? "Bearer";
            return credential;
        }

        /// <summary>
        /// Creates a credential for an API key.
        /// </summary>
        /// <param name="apiKey">The API key value.</param>
        /// <returns>A new credential.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="apiKey"/> is <c>null</c> or empty.</exception>
        public static AuthenticationCredential ForApiKey(string apiKey)
        {
            return new AuthenticationCredential(AuthenticationScheme.ApiKey, apiKey);
        }

        /// <summary>
        /// Creates a credential for HTTP Basic authentication.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>A new credential whose <see cref="Value"/> is the Base64-encoded <c>username:password</c>.</returns>
        /// <exception cref="ArgumentNullException">Either parameter is <c>null</c> or empty.</exception>
        public static AuthenticationCredential ForBasic(string username, string password)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(username, nameof(username));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(password, nameof(password));
            var basicValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
            var credential = new AuthenticationCredential(AuthenticationScheme.Basic, basicValue);
            credential.Properties["Username"] = username;
            credential.Properties["Password"] = password;
            return credential;
        }

        /// <summary>
        /// Creates a credential for an OAuth 2.0 Client Credentials flow.
        /// </summary>
        /// <param name="accessToken">The access token returned by the token endpoint.</param>
        /// <param name="expiresAt">Optional expiration.</param>
        /// <param name="tokenType">The token type (defaults to <c>"Bearer"</c>).</param>
        /// <param name="refreshToken">An optional refresh token for credential refresh.</param>
        /// <returns>A new credential.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="accessToken"/> is <c>null</c> or empty.</exception>
        public static AuthenticationCredential ForClientCredentials(string accessToken, DateTime? expiresAt = null, string? tokenType = null, string? refreshToken = null)
        {
            var credential = new AuthenticationCredential(AuthenticationScheme.Bearer, accessToken, expiresAt);
            credential.Properties["TokenType"] = tokenType ?? "Bearer";
            credential.Properties["GrantType"] = "client_credentials";
            if (refreshToken != null)
                credential.Properties["RefreshToken"] = refreshToken;
            return credential;
        }

        /// <summary>
        /// Creates a credential for certificate-based authentication.
        /// </summary>
        /// <param name="certificateData">The certificate data, path, or thumbprint.</param>
        /// <param name="password">An optional password protecting the certificate.</param>
        /// <returns>A new credential.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="certificateData"/> is <c>null</c> or empty.</exception>
        public static AuthenticationCredential ForCertificate(string certificateData, string? password = null)
        {
            var credential = new AuthenticationCredential(AuthenticationScheme.Certificate, certificateData);
            credential.Properties["CredentialType"] = "Certificate";
            if (password != null)
                credential.Properties["CertificatePassword"] = password;
            return credential;
        }
    }
}
