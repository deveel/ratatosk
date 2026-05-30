//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Configures sender resolution for a specific connector type.
    /// </summary>
    public class SenderConnectorBuilder
    {
        private SenderBuilder? _defaultSenderBuilder;
        private ISenderCache? _cache;
        private TimeSpan? _cacheTtl;

        /// <summary>
        /// Configures a default sender for the connector, used when
        /// no sender is explicitly specified on a message.
        /// </summary>
        /// <param name="configure">An action to configure the sender.</param>
        /// <returns>This builder instance.</returns>
        public SenderConnectorBuilder WithDefault(Action<SenderBuilder> configure)
        {
            var builder = new SenderBuilder();
            configure(builder);
            _defaultSenderBuilder = builder;
            return this;
        }

        /// <summary>
        /// Sets the cache used for sender resolution results.
        /// </summary>
        /// <param name="cache">The cache instance.</param>
        /// <returns>This builder instance.</returns>
        public SenderConnectorBuilder WithCache(ISenderCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            return this;
        }

        /// <summary>
        /// Sets the TTL for cached sender resolution results.
        /// </summary>
        /// <param name="ttl">The time-to-live duration.</param>
        /// <returns>This builder instance.</returns>
        public SenderConnectorBuilder WithCacheTtl(TimeSpan ttl)
        {
            _cacheTtl = ttl;
            return this;
        }

        /// <summary>
        /// Builds the sender connector options.
        /// </summary>
        /// <returns>
        /// A <see cref="SenderConnectorOptions"/> instance.
        /// </returns>
        public SenderConnectorOptions Build()
        {
            return new SenderConnectorOptions
            {
                DefaultSender = _defaultSenderBuilder?.Build(),
                Cache = _cache,
                CacheTtl = _cacheTtl
            };
        }
    }
}
