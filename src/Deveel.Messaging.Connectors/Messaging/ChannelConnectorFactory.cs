//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides a default implementation of <see cref="IChannelConnectorFactory{TConnector}"/>
    /// that creates connector instances using the dependency injection container.
    /// </summary>
    /// <typeparam name="TConnector">
    /// The type of the connector that is created by the factory.
    /// </typeparam>
    public class ChannelConnectorFactory<TConnector> : IChannelConnectorFactory<TConnector>
        where TConnector : class, IChannelConnector
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructs the factory with a service provider.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve dependencies
        /// and create the connector instances.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="serviceProvider"/> is <c>null</c>.
        /// </exception>
        public ChannelConnectorFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a new instance of the connector using the given settings.
        /// </summary>
        /// <param name="settings">
        /// The settings used to configure the connector.
        /// </param>
        /// <returns>
        /// Returns a new instance of <typeparamref name="TConnector"/>.
        /// </returns>
        public TConnector Create(ConnectionSettings settings)
            => Create(settings, null);

        /// <summary>
        /// Creates a new instance of the connector using the given settings
        /// and schema.
        /// </summary>
        /// <param name="settings">
        /// The settings used to configure the connector.
        /// </param>
        /// <param name="schema">
        /// The schema that defines the structure of the channel,
        /// or <c>null</c> to discover it automatically.
        /// </param>
        /// <returns>
        /// Returns a new instance of <typeparamref name="TConnector"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="settings"/> is <c>null</c>.
        /// </exception>
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
