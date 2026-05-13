//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides a default implementation of <see cref="IChannelConnectorFactory{TConnector}"/>
    /// that creates connector instances using the dependency injection container,
    /// with pooling to reuse connectors that match the same settings and schema.
    /// </summary>
    /// <typeparam name="TConnector">
    /// The type of the connector that is created by the factory.
    /// </typeparam>
    public class ChannelConnectorFactory<TConnector> : IChannelConnectorFactory<TConnector>
        where TConnector : class, IChannelConnector
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<ConnectorPoolKey, TConnector> _pool = new();
        private IChannelSchema? _cachedSchema;
        private readonly object _schemaLock = new();

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
            var key = new ConnectorPoolKey(settings, effectiveSchema);

            return _pool.GetOrAdd(key, _ =>
                ActivatorUtilities.CreateInstance<TConnector>(_serviceProvider, effectiveSchema, settings));
        }

        private IChannelSchema DiscoverSchema()
        {
            if (_cachedSchema != null)
                return _cachedSchema;

            lock (_schemaLock)
            {
                if (_cachedSchema != null)
                    return _cachedSchema;

                var connectorType = typeof(TConnector);
                var attribute = connectorType.GetCustomAttribute<ChannelSchemaAttribute>();
                if (attribute == null)
                    throw new ArgumentException(
                        $"Connector type '{connectorType.Name}' must be decorated with {nameof(ChannelSchemaAttribute)}.");

                _cachedSchema = ConnectorSchemaHelper.CreateSchema(_serviceProvider, attribute.SchemaType);
                return _cachedSchema;
            }
        }

        internal readonly struct ConnectorPoolKey : IEquatable<ConnectorPoolKey>
        {
            public ConnectionSettingsKey Settings { get; }
            public IChannelSchema Schema { get; }

            public ConnectorPoolKey(ConnectionSettings settings, IChannelSchema schema)
            {
                Settings = new ConnectionSettingsKey(settings);
                Schema = schema;
            }

            public bool Equals(ConnectorPoolKey other) =>
                Settings.Equals(other.Settings) && ReferenceEquals(Schema, other.Schema);

            public override bool Equals(object? obj) =>
                obj is ConnectorPoolKey other && Equals(other);

            public override int GetHashCode()
            {
                var hash = new HashCode();
                hash.Add(Settings);
                hash.Add(RuntimeHelpers.GetHashCode(Schema));
                return hash.ToHashCode();
            }

            public static bool operator ==(ConnectorPoolKey left, ConnectorPoolKey right) => left.Equals(right);
            public static bool operator !=(ConnectorPoolKey left, ConnectorPoolKey right) => !left.Equals(right);
        }

        internal readonly struct ConnectionSettingsKey : IEquatable<ConnectionSettingsKey>
        {
            private readonly IReadOnlyDictionary<string, object?> _parameters;

            public ConnectionSettingsKey(ConnectionSettings settings)
            {
                var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in settings.Parameters)
                    dict[kvp.Key] = kvp.Value;
                _parameters = dict;
            }

            public bool Equals(ConnectionSettingsKey other)
            {
                if (_parameters.Count != other._parameters.Count)
                    return false;

                foreach (var kvp in _parameters)
                {
                    if (!other._parameters.TryGetValue(kvp.Key, out var otherValue))
                        return false;
                    if (!Equals(kvp.Value, otherValue))
                        return false;
                }
                return true;
            }

            public override bool Equals(object? obj) =>
                obj is ConnectionSettingsKey other && Equals(other);

            public override int GetHashCode()
            {
                var hash = new HashCode();
                foreach (var kvp in _parameters)
                {
                    hash.Add(kvp.Key, StringComparer.OrdinalIgnoreCase);
                    hash.Add(kvp.Value);
                }
                return hash.ToHashCode();
            }

            public static bool operator ==(ConnectionSettingsKey left, ConnectionSettingsKey right) => left.Equals(right);
            public static bool operator !=(ConnectionSettingsKey left, ConnectionSettingsKey right) => !left.Equals(right);
        }
    }
}
