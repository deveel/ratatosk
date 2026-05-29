//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Configures sender resolution for a specific connector type.
    /// </summary>
    /// <typeparam name="TConnector">
    /// The type of the channel connector being configured.
    /// </typeparam>
    public class SenderConnectorBuilder<TConnector>
        where TConnector : IChannelConnector
    {
        private SenderBuilder? _defaultSenderBuilder;
        private ISenderSelector? _selector;
        private ISenderCache? _cache;
        private TimeSpan? _cacheTtl;

        /// <summary>
        /// Configures a default sender for the connector, used when
        /// no sender is explicitly specified on a message.
        /// </summary>
        /// <param name="configure">An action to configure the sender.</param>
        /// <returns>This builder instance.</returns>
        public SenderConnectorBuilder<TConnector> WithDefault(Action<SenderBuilder> configure)
        {
            var builder = new SenderBuilder();
            configure(builder);
            _defaultSenderBuilder = builder;
            return this;
        }

        /// <summary>
        /// Sets the selector strategy for choosing a sender
        /// when multiple candidates are available.
        /// </summary>
        /// <param name="selector">The selector strategy.</param>
        /// <returns>This builder instance.</returns>
        public SenderConnectorBuilder<TConnector> WithSelector(ISenderSelector selector)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            return this;
        }

        /// <summary>
        /// Sets the cache used for sender resolution results.
        /// </summary>
        /// <param name="cache">The cache instance.</param>
        /// <returns>This builder instance.</returns>
        public SenderConnectorBuilder<TConnector> WithCache(ISenderCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            return this;
        }

        /// <summary>
        /// Sets the TTL for cached sender resolution results.
        /// </summary>
        /// <param name="ttl">The time-to-live duration.</param>
        /// <returns>This builder instance.</returns>
        public SenderConnectorBuilder<TConnector> WithCacheTtl(TimeSpan ttl)
        {
            _cacheTtl = ttl;
            return this;
        }

        /// <summary>
        /// Builds the sender connector configuration.
        /// </summary>
        /// <returns>
        /// A <see cref="SenderConnectorConfiguration"/> instance.
        /// </returns>
        public SenderConnectorConfiguration Build()
        {
            return new SenderConnectorConfiguration
            {
                DefaultSender = _defaultSenderBuilder?.Build(),
                Selector = _selector,
                Cache = _cache,
                CacheTtl = _cacheTtl
            };
        }
    }
}
