//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ratatosk
{
    /// <summary>
    /// Provides a fluent API for configuring a connector before
    /// registering it into the messaging services.
    /// </summary>
    /// <typeparam name="TConnector">
    /// The type of the connector being configured.
    /// </typeparam>
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

        /// <summary>
        /// Gets the parent <see cref="MessagingBuilder"/> instance
        /// that is used to register the connector.
        /// </summary>
        public MessagingBuilder MessagingBuilder { get; }

        // ── Schema override ────────────────────────────────────────────────────

        /// <summary>
        /// Overrides the default schema of the connector with the given one.
        /// </summary>
        /// <param name="schema">
        /// The schema to use for the connector.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="schema"/> is <c>null</c>.
        /// </exception>
        public ChannelConnectorBuilder<TConnector> WithSchema(IChannelSchema schema)
        {
            ArgumentNullException.ThrowIfNull(schema, nameof(schema));
            _schemaOverride = schema;
            return this;
        }

        // ── Connection settings ────────────────────────────────────────────────

        /// <summary>
        /// Configures the connector using a connection string.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string to parse and apply as settings.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="connectionString"/> is <c>null</c>
        /// or empty.
        /// </exception>
        public ChannelConnectorBuilder<TConnector> WithConnectionString(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

            var settings = ConnectionSettings.Parse(connectionString);
            foreach (var (key, value) in settings.Parameters)
                _fluentSettings[key] = value;

            return this;
        }

        /// <summary>
        /// Configures the connector using a named configuration section.
        /// </summary>
        /// <param name="configurationSection">
        /// The name of the configuration section that contains the settings.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="configurationSection"/> is <c>null</c>
        /// or empty.
        /// </exception>
        public ChannelConnectorBuilder<TConnector> WithSettings(string configurationSection)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(configurationSection, nameof(configurationSection));
            _configSection = configurationSection;
            return this;
        }

        /// <summary>
        /// Sets a single setting value for the connector.
        /// </summary>
        /// <param name="key">
        /// The name of the setting.
        /// </param>
        /// <param name="value">
        /// The value of the setting.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        public ChannelConnectorBuilder<TConnector> WithSetting(string key, object? value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            _fluentSettings[key] = value;
            return this;
        }

        /// <summary>
        /// Configures the settings of the connector using a delegate.
        /// </summary>
        /// <param name="configure">
        /// A delegate that receives a dictionary of settings to configure.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="configure"/> is <c>null</c>.
        /// </exception>
        public ChannelConnectorBuilder<TConnector> WithSettings(
            Action<IDictionary<string, object?>> configure)
        {
            ArgumentNullException.ThrowIfNull(configure, nameof(configure));
            configure(_fluentSettings);
            return this;
        }

        /// <summary>
        /// Configures the connector using typed options.
        /// </summary>
        /// <typeparam name="TOptions">
        /// The type of the options class that implements <see cref="IConnectorOptions"/>.
        /// </typeparam>
        /// <param name="options">
        /// The typed options instance to convert into connection settings.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public ChannelConnectorBuilder<TConnector> WithOptions<TOptions>(TOptions options)
            where TOptions : IConnectorOptions
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            var settings = options.ToConnectionSettings();
            foreach (var (key, value) in settings.Parameters)
                _fluentSettings[key] = value;

            return this;
        }
        
        // ── Factory override ───────────────────────────────────────────────────

        /// <summary>
        /// Overrides the default factory used to create the connector
        /// with a custom factory type.
        /// </summary>
        /// <typeparam name="TFactory">
        /// The type of the custom factory that implements
        /// <see cref="IChannelConnectorFactory{TConnector}"/>.
        /// </typeparam>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        public ChannelConnectorBuilder<TConnector> WithFactory<TFactory>()
            where TFactory : class, IChannelConnectorFactory<TConnector>
        {
            _factoryType = typeof(TFactory);
            _factoryInstance = null;
            return this;
        }

        /// <summary>
        /// Overrides the default factory used to create the connector
        /// with a provided factory instance.
        /// </summary>
        /// <param name="factory">
        /// The factory instance to use for creating the connector.
        /// </param>
        /// <returns>
        /// Returns the current builder instance to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="factory"/> is <c>null</c>.
        /// </exception>
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
