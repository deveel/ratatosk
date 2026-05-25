//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Provides a mechanism to create instances of a channel connector.
    /// </summary>
    /// <typeparam name="TConnector">
    /// The type of the connector that is created by the factory.
    /// </typeparam>
    public interface IChannelConnectorFactory<TConnector>
        where TConnector : class, IChannelConnector
    {
        /// <summary>
        /// Creates a new instance of the connector using the given settings.
        /// </summary>
        /// <param name="settings">
        /// The settings used to configure the connector.
        /// </param>
        /// <returns>
        /// Returns an instance of <typeparamref name="TConnector"/> that
        /// is configured with the given settings.
        /// </returns>
        TConnector Create(ConnectionSettings settings);

        /// <summary>
        /// Creates a new instance of the connector using the given settings
        /// and an optional schema.
        /// </summary>
        /// <param name="settings">
        /// The settings used to configure the connector.
        /// </param>
        /// <param name="schema">
        /// An optional schema that defines the structure of the channel.
        /// </param>
        /// <returns>
        /// Returns an instance of <typeparamref name="TConnector"/> that
        /// is configured with the given settings and schema.
        /// </returns>
        TConnector Create(ConnectionSettings settings, IChannelSchema? schema);
    }
}
