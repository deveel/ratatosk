//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Ratatosk.Senders;

namespace Ratatosk
{
    /// <summary>
    /// Provides a fluent builder for configuring sender resolution
    /// for a specific connector type, with the ability to return
    /// to the parent <see cref="ChannelConnectorBuilder{TConnector}"/>.
    /// </summary>
    /// <typeparam name="TConnector">The type of the channel connector being configured.</typeparam>
    public sealed class SenderRegistrationBuilder<TConnector>
        where TConnector : class, IChannelConnector
    {
        private readonly ChannelConnectorBuilder<TConnector> _parent;
        private readonly MessagingBuilder _messagingBuilder;
        private readonly SenderConnectorBuilder _senderBuilder;

        internal SenderRegistrationBuilder(
            ChannelConnectorBuilder<TConnector> parent,
            MessagingBuilder messagingBuilder)
        {
            _parent = parent;
            _messagingBuilder = messagingBuilder;
            _senderBuilder = new SenderConnectorBuilder();
        }

        /// <summary>
        /// Configures a default sender for the connector, used when
        /// no sender is explicitly specified on a message.
        /// </summary>
        /// <param name="configure">An action to configure the sender.</param>
        /// <returns>This builder instance.</returns>
        public SenderRegistrationBuilder<TConnector> WithDefault(Action<SenderBuilder> configure)
        {
            _senderBuilder.WithDefault(configure);
            return this;
        }

        /// <summary>
        /// Sets the cache used for sender resolution results.
        /// </summary>
        /// <param name="cache">The cache instance.</param>
        /// <returns>This builder instance.</returns>
        public SenderRegistrationBuilder<TConnector> WithCache(ISenderCache cache)
        {
            _senderBuilder.WithCache(cache);
            return this;
        }

        /// <summary>
        /// Sets the TTL for cached sender resolution results.
        /// </summary>
        /// <param name="ttl">The time-to-live duration.</param>
        /// <returns>This builder instance.</returns>
        public SenderRegistrationBuilder<TConnector> WithCacheTtl(TimeSpan ttl)
        {
            _senderBuilder.WithCacheTtl(ttl);
            return this;
        }

        /// <summary>
        /// Completes the sender configuration and returns to the
        /// parent <see cref="ChannelConnectorBuilder{TConnector}"/>.
        /// </summary>
        /// <returns>The parent connector builder for continued configuration.</returns>
        public ChannelConnectorBuilder<TConnector> Done()
        {
            var config = _senderBuilder.Build();
            _messagingBuilder.AddSenders();
            _messagingBuilder.RegisterSenderConfiguration<TConnector>(config);
            return _parent;
        }
    }
}
