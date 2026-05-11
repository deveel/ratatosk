//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    public interface IChannelConnectorFactory<TConnector>
        where TConnector : class, IChannelConnector
    {
        TConnector Create(ConnectionSettings settings);

        TConnector Create(ConnectionSettings settings, IChannelSchema? schema);
    }
}
