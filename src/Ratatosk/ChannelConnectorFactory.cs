//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;
using System.Reflection;

namespace Ratatosk
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

            // Probe the pool first (cheap path).
            if (_pool.TryGetValue(key, out var pooled) && pooled.IsReusable)
                return pooled;

            // Create a per-connector sender resolver if sender services are registered.
            var senderResolver = CreateSenderResolver(settings);

            // Create a new instance, then conditionally store it.
            var instance = senderResolver != null
                ? ActivatorUtilities.CreateInstance<TConnector>(_serviceProvider, effectiveSchema, settings, senderResolver)
                : ActivatorUtilities.CreateInstance<TConnector>(_serviceProvider, effectiveSchema, settings);

            if (instance.IsReusable)
                return _pool.GetOrAdd(key, instance);

            return instance;
        }

        private ISenderResolver? CreateSenderResolver(ConnectionSettings settings)
        {
            var repository = _serviceProvider.GetService<ISenderRepository<ISender>>();
            if (repository == null)
                return null;

            var defaultCache = _serviceProvider.GetService<ISenderCache>();
            if (defaultCache == null)
                return null;

            var optionsMonitor = _serviceProvider.GetService<IOptionsMonitor<SenderConnectorOptions>>();
            var connectorOptions = optionsMonitor?.Get(typeof(TConnector).FullName!);
            var cache = connectorOptions?.Cache ?? defaultCache;

            // ConnectionSettings (per operation/session) overrides SenderConnectorOptions (build-time default).
            var defaultSender = BuildDefaultSenderFromSettings(settings)
                ?? connectorOptions?.DefaultSender;

            return new SenderResolver(repository, cache, defaultSender);
        }

        private static ISender? BuildDefaultSenderFromSettings(ConnectionSettings settings)
        {
            var name = settings.GetParameter<string>("DefaultSenderName");
            var address = settings.GetParameter<string>("DefaultSenderAddress");
            var typeStr = settings.GetParameter<string>("DefaultSenderType");

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(address))
                return null;

            var type = Enum.TryParse<EndpointType>(typeStr, ignoreCase: true, out var parsed)
                ? parsed
                : EndpointType.Any;

            return new SenderBuilder()
                .WithName(name ?? "default")
                .WithAddress(address ?? string.Empty)
                .WithEndpointType(type)
                .Build();
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
