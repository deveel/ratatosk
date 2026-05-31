//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk.Senders
{
    /// <summary>
    /// Caches sender identities to avoid repeated lookups in the registry.
    /// </summary>
    /// <remarks>
    /// Implementations index senders by both name and by endpoint (type + address).
    /// A single <see cref="SetAsync"/> call stores the sender under both keys.
    /// </remarks>
    public interface ISenderCache
    {
        /// <summary>
        /// Gets a cached sender by its logical name.
        /// </summary>
        ValueTask<ISender?> GetByNameAsync(string senderName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a cached sender by its endpoint address and type.
        /// </summary>
        ValueTask<ISender?> GetByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores a sender in the cache, indexed by both name and endpoint.
        /// </summary>
        ValueTask SetAsync(ISender sender, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a cached sender by its logical name.
        /// </summary>
        ValueTask RemoveByNameAsync(string senderName, CancellationToken cancellationToken = default);
    }
}
