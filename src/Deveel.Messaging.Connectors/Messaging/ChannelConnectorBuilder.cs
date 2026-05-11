//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging
{
    public sealed class ChannelConnectorBuilder<TConnector>
        where TConnector : class, IChannelConnector
    {
        private string? _configSection;
        private readonly Dictionary<string, object?> _fluentSettings =
            new(StringComparer.OrdinalIgnoreCase);
        private IChannelSchema? _schemaOverride;
        private IChannelConnectorFactory<TConnector>? _factoryInstance;
        private Type? _factoryType;

        internal ChannelConnectorBuilder(MessagingBuilder messagingBuilder)
        {
            ArgumentNullException.ThrowIfNull(messagingBuilder, nameof(messagingBuilder));
            MessagingBuilder = messagingBuilder;
        }

        public MessagingBuilder MessagingBuilder { get; }

        // ── Schema override ────────────────────────────────────────────────────

        public ChannelConnectorBuilder<TConnector> WithSchema(IChannelSchema schema)
        {
            ArgumentNullException.ThrowIfNull(schema, nameof(schema));
            _schemaOverride = schema;
            return this;
        }

        // ── Connection settings ────────────────────────────────────────────────

        public ChannelConnectorBuilder<TConnector> WithConnectionString(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

            var parsed = ConnectionStringParser.Parse(connectionString);
            foreach (var (key, value) in parsed)
                _fluentSettings[key] = value;

            return this;
        }

        public ChannelConnectorBuilder<TConnector> WithSettings(string configurationSection)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(configurationSection, nameof(configurationSection));
            _configSection = configurationSection;
            return this;
        }

        public ChannelConnectorBuilder<TConnector> WithSetting(string key, object? value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            _fluentSettings[key] = value;
            return this;
        }

        public ChannelConnectorBuilder<TConnector> WithSettings(
            Action<IDictionary<string, object?>> configure)
        {
            ArgumentNullException.ThrowIfNull(configure, nameof(configure));
            configure(_fluentSettings);
            return this;
        }

        // ── Factory override ───────────────────────────────────────────────────

        public ChannelConnectorBuilder<TConnector> WithFactory<TFactory>()
            where TFactory : class, IChannelConnectorFactory<TConnector>
        {
            _factoryType = typeof(TFactory);
            _factoryInstance = null;
            return this;
        }

        public ChannelConnectorBuilder<TConnector> WithFactory(IChannelConnectorFactory<TConnector> factory)
        {
            ArgumentNullException.ThrowIfNull(factory, nameof(factory));
            _factoryInstance = factory;
            _factoryType = null;
            return this;
        }

        // ── Internal surface (used by MessagingBuilder) ────────────────────────

        internal IChannelSchema? SchemaOverride => _schemaOverride;

        internal Type? FactoryOverrideType => _factoryType;

        internal ConnectionSettings BuildConnectionSettings(IServiceProvider sp)
        {
            var merged = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            if (_configSection != null)
            {
                var config = sp.GetService<IConfiguration>();
                if (config != null)
                {
                    foreach (var child in config.GetSection(_configSection).GetChildren())
                        merged[child.Key] = child.Value;
                }
            }

            foreach (var (key, value) in _fluentSettings)
                merged[key] = value;

            return new ConnectionSettings(merged);
        }

        internal TConnector CreateConnector(IServiceProvider sp, IChannelSchema schema)
        {
            var settings = BuildConnectionSettings(sp);

            if (_factoryInstance != null)
                return _factoryInstance.Create(settings, schema);

            var factory = sp.GetRequiredService<IChannelConnectorFactory<TConnector>>();
            return factory.Create(settings, schema);
        }
    }
}
