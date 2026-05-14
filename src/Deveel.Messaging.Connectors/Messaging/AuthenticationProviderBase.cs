//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides a base implementation of <see cref="IAuthenticationProvider"/> with
    /// common helpers for extracting parameters from <see cref="ConnectionSettings"/>
    /// and building results.
    /// </summary>
    public abstract class AuthenticationProviderBase : IAuthenticationProvider
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="scheme">The scheme this provider supports.</param>
        /// <param name="displayName">A human-readable name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="scheme"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="displayName"/> is <c>null</c> or empty.</exception>
        protected AuthenticationProviderBase(AuthenticationScheme scheme, string displayName)
        {
            Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));
            DisplayName = displayName;
        }

        /// <inheritdoc/>
        public AuthenticationScheme Scheme { get; }

        /// <inheritdoc/>
        public string DisplayName { get; }

        /// <inheritdoc/>
        public virtual bool CanHandle(AuthenticationConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            return configuration.Scheme == Scheme;
        }

        /// <inheritdoc/>
        public abstract Task<AuthenticationResult> ObtainCredentialAsync(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual Task<AuthenticationResult> RefreshCredentialAsync(AuthenticationCredential existingCredential, ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default)
        {
            return ObtainCredentialAsync(connectionSettings, configuration, cancellationToken);
        }

        /// <summary>
        /// Creates a failed <see cref="AuthenticationResult"/>.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="errorCode">An optional error code.</param>
        /// <returns>A failed result.</returns>
        protected static AuthenticationResult Failure(string errorMessage, string? errorCode = null)
        {
            return AuthenticationResult.Failure(errorMessage, errorCode);
        }

        /// <summary>
        /// Creates a successful <see cref="AuthenticationResult"/>.
        /// </summary>
        /// <param name="credential">The obtained credential.</param>
        /// <returns>A successful result.</returns>
        protected static AuthenticationResult Success(AuthenticationCredential credential)
        {
            return AuthenticationResult.Success(credential);
        }
    }
}
