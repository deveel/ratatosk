//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Collections.Concurrent;

namespace Ratatosk
{
    /// <summary>
    /// An in-memory implementation of <see cref="ISenderCache"/> that
    /// stores sender entities in a concurrent dictionary.
    /// </summary>
    public class InMemorySenderCache : ISenderCache
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);
        private readonly TimeSpan _defaultTtl;

        /// <summary>
        /// Constructs the cache with the given default TTL.
        /// </summary>
        /// <param name="defaultTtl">
        /// The default time-to-live for cache entries. Defaults to 5 minutes.
        /// </param>
        public InMemorySenderCache(TimeSpan? defaultTtl = null)
        {
            _defaultTtl = defaultTtl ?? TimeSpan.FromMinutes(5);
        }

        /// <inheritdoc />
        public ValueTask<SenderEntity?> GetByNameAsync(string senderName, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(senderName, out var entry))
            {
                if (entry.ExpiresAt > DateTime.UtcNow)
                {
                    return ValueTask.FromResult<SenderEntity?>(entry.Entity);
                }

                _cache.TryRemove(senderName, out _);
            }

            return ValueTask.FromResult<SenderEntity?>(null);
        }

        /// <inheritdoc />
        public ValueTask SetByNameAsync(string senderName, SenderEntity sender, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
            var expiresAt = ttl.HasValue
                ? DateTime.UtcNow.Add(ttl.Value)
                : DateTime.UtcNow.Add(_defaultTtl);

            _cache[senderName] = new CacheEntry(sender, expiresAt);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask RemoveByNameAsync(string senderName, CancellationToken cancellationToken = default)
        {
            _cache.TryRemove(senderName, out _);

            return ValueTask.CompletedTask;
        }

        private class CacheEntry
        {
            public SenderEntity Entity { get; }
            public DateTime ExpiresAt { get; }

            public CacheEntry(SenderEntity entity, DateTime expiresAt)
            {
                Entity = entity;
                ExpiresAt = expiresAt;
            }
        }
    }
}
