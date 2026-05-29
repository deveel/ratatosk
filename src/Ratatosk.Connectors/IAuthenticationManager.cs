//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Manages a registry of <see cref="IAuthenticationProvider"/> instances and
    /// coordinates credential acquisition with caching.
    /// </summary>
    public interface IAuthenticationManager
    {
        /// <summary>
        /// Registers an authentication provider.
        /// </summary>
        /// <param name="provider">The provider to register.</param>
        void RegisterProvider(IAuthenticationProvider provider);

        /// <summary>
        /// Authenticates by finding a suitable provider for the given
        /// <paramref name="configuration"/> and delegating to it.
        /// </summary>
        /// <param name="connectionSettings">The settings containing credential values.</param>
        /// <param name="configuration">The configuration describing which fields to use.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task whose result is the authentication outcome.</returns>
        Task<AuthenticationResult> AuthenticateAsync(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears all cached credentials.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Removes a specific credential from the cache.
        /// </summary>
        /// <param name="connectionSettings">The settings used for the cached credential.</param>
        /// <param name="configuration">The configuration used for the cached credential.</param>
        void InvalidateCredential(ConnectionSettings connectionSettings, AuthenticationConfiguration configuration);
    }
}
