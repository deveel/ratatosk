//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace Deveel.Messaging
{
    public class ChannelConnectorFactory<TConnector> : IChannelConnectorFactory<TConnector>
        where TConnector : class, IChannelConnector
    {
        private readonly IServiceProvider _serviceProvider;

        public ChannelConnectorFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }

        public TConnector Create(ConnectionSettings settings)
            => Create(settings, null);

        public TConnector Create(ConnectionSettings settings, IChannelSchema? schema)
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            var effectiveSchema = schema ?? DiscoverSchema();

            return ActivatorUtilities.CreateInstance<TConnector>(_serviceProvider, effectiveSchema, settings);
        }

        private IChannelSchema DiscoverSchema()
        {
            var connectorType = typeof(TConnector);
            var attribute = connectorType.GetCustomAttribute<ChannelSchemaAttribute>();
            if (attribute == null)
                throw new ArgumentException(
                    $"Connector type '{connectorType.Name}' must be decorated with {nameof(ChannelSchemaAttribute)}.");

            return ConnectorSchemaHelper.CreateSchema(_serviceProvider, attribute.SchemaType);
        }
    }
}
