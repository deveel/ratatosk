//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Concurrent;
using System.Reflection;

namespace Ratatosk
{
    /// <summary>
    /// Provides a default implementation of <see cref="IChannelConnectorFactory{TConnector}"/>
    /// that creates connector instances using the dependency injection container,
    /// with pooling to reuse connectors that match the same settings and schema.
    /// </summary>
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
        public ChannelConnectorFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public TConnector Create(ConnectionSettings settings)
            => Create(settings, null);

        /// <inheritdoc />
        public TConnector Create(ConnectionSettings settings, IChannelSchema? schema)
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            var effectiveSchema = schema ?? DiscoverSchema();
            var key = new ConnectorPoolKey(settings, effectiveSchema);

            if (_pool.TryGetValue(key, out var pooled) && pooled.IsReusable)
                return pooled;

            var instance = ActivatorUtilities.CreateInstance<TConnector>(_serviceProvider, effectiveSchema, settings);

            if (instance.IsReusable)
                return _pool.GetOrAdd(key, instance);

            return instance;
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
                Settings.Equals(other.Settings) &&
                string.Equals(Schema.ChannelProvider, other.Schema.ChannelProvider, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Schema.ChannelType, other.Schema.ChannelType, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Schema.Version, other.Schema.Version, StringComparison.OrdinalIgnoreCase);

            public override bool Equals(object? obj) =>
                obj is ConnectorPoolKey other && Equals(other);

            public override int GetHashCode()
            {
                var hash = new HashCode();
                hash.Add(Settings);
                hash.Add(Schema.ChannelProvider, StringComparer.OrdinalIgnoreCase);
                hash.Add(Schema.ChannelType, StringComparer.OrdinalIgnoreCase);
                hash.Add(Schema.Version, StringComparer.OrdinalIgnoreCase);
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
