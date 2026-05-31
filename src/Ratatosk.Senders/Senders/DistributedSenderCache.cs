//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Ratatosk.Senders
{
    /// <summary>
    /// An <see cref="ISenderCache"/> implementation backed by <see cref="IDistributedCache"/>.
    /// </summary>
    /// <remarks>
    /// Each sender is stored under two keys: one by name and one by endpoint (type + address).
    /// This allows fast lookups regardless of whether resolution started from a name reference
    /// or an inline sender.
    /// </remarks>
    public class DistributedSenderCache : ISenderCache
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IDistributedCache _cache;
        private readonly SenderCacheOptions _options;

        /// <summary>
        /// Constructs the cache with the given distributed cache and options.
        /// </summary>
        public DistributedSenderCache(IDistributedCache cache, IOptions<SenderCacheOptions> options)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public async ValueTask<ISender?> GetByNameAsync(string senderName, CancellationToken cancellationToken = default)
        {
            var key = CacheKey.ForName(senderName);
            var data = await _cache.GetAsync(key, cancellationToken);
            if (data == null)
                return null;

            return JsonSerializer.Deserialize<ISender>(data, _jsonOptions);
        }

        /// <inheritdoc />
        public async ValueTask<ISender?> GetByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken = default)
        {
            var key = CacheKey.ForEndpoint(address, endpointType);
            var data = await _cache.GetAsync(key, cancellationToken);
            if (data == null)
                return null;

            return JsonSerializer.Deserialize<ISender>(data, _jsonOptions);
        }

        /// <inheritdoc />
        public async ValueTask SetAsync(ISender sender, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sender);

            var data = JsonSerializer.SerializeToUtf8Bytes(sender, _jsonOptions);
            var effectiveTtl = ttl ?? _options.DefaultTtl;

            var distributedOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = effectiveTtl
            };

            var nameKey = CacheKey.ForName(sender.Name);
            var endpointKey = CacheKey.ForEndpoint(sender.Address, sender.Type);

            await Task.WhenAll(
                _cache.SetAsync(nameKey, data, distributedOptions, cancellationToken),
                _cache.SetAsync(endpointKey, data, distributedOptions, cancellationToken)
            );
        }

        /// <inheritdoc />
        public async ValueTask RemoveByNameAsync(string senderName, CancellationToken cancellationToken = default)
        {
            var key = CacheKey.ForName(senderName);
            await _cache.RemoveAsync(key, cancellationToken);
        }

        private static class CacheKey
        {
            public static string ForName(string name) => $"ratatosk:sender:name:{name}";

            public static string ForEndpoint(string address, EndpointType type) =>
                $"ratatosk:sender:endpoint:{type}:{address}";
        }
    }
}
