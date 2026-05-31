//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Defines the contract for obtaining and refreshing authentication credentials
    /// from connection settings.
    /// </summary>
    /// <remarks>
    /// Implementations handle a single <see cref="AuthenticationScheme"/> and use
    /// the <see cref="AuthenticationConfiguration"/> passed to each method to determine
    /// which fields to extract from <see cref="ConnectionSettings"/>.
    /// </remarks>
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Gets the authentication scheme this provider supports.
        /// </summary>
        AuthenticationScheme Scheme { get; }

        /// <summary>
        /// Gets the human-readable display name of this provider.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Determines whether this provider can handle the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to check.</param>
        /// <returns><c>true</c> if this provider can handle the configuration.</returns>
        bool CanHandle(AuthenticationConfiguration configuration);

        /// <summary>
        /// Obtains a credential by extracting the required fields from <paramref name="connectionSettings"/>
        /// as described by <paramref name="configuration"/>.
        /// </summary>
        /// <param name="connectionSettings">The settings containing credential values.</param>
        /// <param name="configuration">The configuration describing which fields to use.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task whose result is the authentication outcome.</returns>
        Task<AuthenticationResult> ObtainCredentialAsync(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to refresh an existing credential, falling back to a fresh
        /// <see cref="ObtainCredentialAsync"/> if refresh is not supported.
        /// </summary>
        /// <param name="existingCredential">The credential to refresh.</param>
        /// <param name="connectionSettings">The settings containing credential values.</param>
        /// <param name="configuration">The configuration describing which fields to use.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task whose result is the authentication outcome.</returns>
        Task<AuthenticationResult> RefreshCredentialAsync(AuthenticationCredential existingCredential, ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default);
    }
}
