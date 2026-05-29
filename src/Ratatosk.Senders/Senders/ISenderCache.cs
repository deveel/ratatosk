//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Caches sender identities by name to avoid repeated lookups in the registry.
    /// </summary>
    /// <remarks>
    /// This is a domain-level cache separate from the entity cache used by
    /// <c>EntityManager&lt;SenderEntity&gt;</c>. It caches resolution results
    /// keyed by sender name (not entity ID).
    /// </remarks>
    public interface ISenderCache
    {
        /// <summary>
        /// Gets a cached sender entity by its logical name.
        /// </summary>
        /// <param name="senderName">The logical name of the sender.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// The cached <see cref="SenderEntity"/>, or <c>null</c> if not cached.
        /// </returns>
        ValueTask<SenderEntity?> GetByNameAsync(string senderName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores a sender entity in the cache, keyed by its logical name.
        /// </summary>
        /// <param name="senderName">The logical name of the sender.</param>
        /// <param name="sender">The sender entity to cache.</param>
        /// <param name="ttl">
        /// An optional time-to-live for the cache entry. If <c>null</c>,
        /// the default TTL configured for the cache is used.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        ValueTask SetByNameAsync(string senderName, SenderEntity sender, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a cached sender entity by its logical name.
        /// </summary>
        /// <param name="senderName">The logical name of the sender.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        ValueTask RemoveByNameAsync(string senderName, CancellationToken cancellationToken = default);
    }
}
