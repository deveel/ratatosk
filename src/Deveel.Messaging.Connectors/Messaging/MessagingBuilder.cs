//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Reflection;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides a builder for configuring and registering messaging
    /// connectors into a service collection.
    /// </summary>
    public sealed class MessagingBuilder
    {
        internal MessagingBuilder(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            Services = services;
            services.TryAddSingleton<IChannelSchemaRegistry, ChannelSchemaRegistry>();
        }

        /// <summary>
        /// Gets the collection of services that is used to register
        /// the messaging components.
        /// </summary>
        public IServiceCollection Services { get; }

        // ── Unnamed connector registration ────────────────────────────────────

        /// <summary>
        /// Registers a connector of the specified type into the services.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to register.
        /// </typeparam>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        public MessagingBuilder AddConnector<TConnector>()
            where TConnector : class, IChannelConnector
        {
            return AddConnector(typeof(TConnector));
        }

        /// <summary>
        /// Registers a connector of the specified type into the services.
        /// </summary>
        /// <param name="connectorType">
        /// The type of the connector to register.
        /// </param>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="connectorType"/> is <c>null</c>.
        /// </exception>
        public MessagingBuilder AddConnector(Type connectorType)
        {
            ArgumentNullException.ThrowIfNull(connectorType, nameof(connectorType));
            EnsureValidConnectorType(connectorType);

            RegisterDefaultFactory(connectorType);

            Services.TryAddSingleton(connectorType, sp =>
            {
                var schema = ConnectorSchemaHelper.DiscoverConnectorSchema(sp, connectorType);
                return CreateFromFactory(sp, connectorType, new ConnectionSettings(), schema);
            });

            Services.AddSingleton<IChannelConnector>(sp =>
                (IChannelConnector)sp.GetRequiredService(connectorType));

            return this;
        }

        // ── Connection string overloads ────────────────────────────────────────

        /// <summary>
        /// Registers a connector using a connection string that is parsed
        /// into the settings for the connector.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to register.
        /// </typeparam>
        /// <param name="connectionString">
        /// The connection string to parse and apply as settings.
        /// </param>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        public MessagingBuilder AddConnector<TConnector>(string connectionString)
            where TConnector : class, IChannelConnector
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));
            return AddConnector<TConnector>(c => c.WithConnectionString(connectionString));
        }

        /// <summary>
        /// Registers a named connector using a connection string that is parsed
        /// into the settings for the connector.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to register.
        /// </typeparam>
        /// <param name="connectorName">
        /// The name that identifies the connector instance.
        /// </param>
        /// <param name="connectionString">
        /// The connection string to parse and apply as settings.
        /// </param>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        public MessagingBuilder AddConnector<TConnector>(string connectorName, string connectionString)
            where TConnector : class, IChannelConnector
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(connectorName, nameof(connectorName));
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));
            return AddConnector<TConnector>(connectorName, c => c.WithConnectionString(connectionString));
        }

        // ── Named + fluent connector registration ─────────────────────────────

        /// <summary>
        /// Registers a connector with a configuration action.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to register.
        /// </typeparam>
        /// <param name="configure">
        /// An action to configure the connector builder.
        /// </param>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="configure"/> is <c>null</c>.
        /// </exception>
        public MessagingBuilder AddConnector<TConnector>(
            Action<ChannelConnectorBuilder<TConnector>> configure)
            where TConnector : class, IChannelConnector
        {
            ArgumentNullException.ThrowIfNull(configure, nameof(configure));

            var connectorType = typeof(TConnector);
            EnsureValidConnectorType(connectorType);

            RegisterDefaultFactory(connectorType);

            var connectorBuilder = new ChannelConnectorBuilder<TConnector>(this);
            configure(connectorBuilder);

            ApplyFactoryOverride(connectorType, connectorBuilder);

            Services.TryAddSingleton(connectorType, sp =>
            {
                var schema = connectorBuilder.SchemaOverride ?? ConnectorSchemaHelper.DiscoverConnectorSchema(sp, connectorType);
                return connectorBuilder.CreateConnector(sp, schema);
            });

            Services.AddSingleton<IChannelConnector>(sp =>
                (IChannelConnector)sp.GetRequiredService(connectorType));

            return this;
        }

        /// <summary>
        /// Registers a named connector with a configuration action.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to register.
        /// </typeparam>
        /// <param name="connectorName">
        /// The name that identifies the connector instance.
        /// </param>
        /// <param name="configure">
        /// An action to configure the connector builder.
        /// </param>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="connectorName"/> or
        /// <paramref name="configure"/> is <c>null</c>.
        /// </exception>
        public MessagingBuilder AddConnector<TConnector>(
            string connectorName,
            Action<ChannelConnectorBuilder<TConnector>> configure)
            where TConnector : class, IChannelConnector
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(connectorName, nameof(connectorName));
            ArgumentNullException.ThrowIfNull(configure, nameof(configure));

            var connectorType = typeof(TConnector);
            EnsureValidConnectorType(connectorType);

            RegisterDefaultFactory(connectorType);

            var connectorBuilder = new ChannelConnectorBuilder<TConnector>(this);
            configure(connectorBuilder);

            ApplyFactoryOverride(connectorType, connectorBuilder);

            Services.AddKeyedSingleton<IChannelConnector>(connectorName, (sp, _) =>
            {
                var schema = connectorBuilder.SchemaOverride ?? ConnectorSchemaHelper.DiscoverConnectorSchema(sp, connectorType);
                return connectorBuilder.CreateConnector(sp, schema);
            });

            Services.AddSingleton<NamedConnectorDescriptor>(sp =>
            {
                var schema   = connectorBuilder.SchemaOverride ?? ConnectorSchemaHelper.DiscoverConnectorSchema(sp, connectorType);
                var settings = connectorBuilder.BuildConnectionSettings(sp);
                return new NamedConnectorDescriptor(connectorName, connectorType, schema, settings.Parameters);
            });

            return this;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void EnsureValidConnectorType(Type connectorType)
        {
            if (!typeof(IChannelConnector).IsAssignableFrom(connectorType))
                throw new ArgumentException(
                    $"Type '{connectorType.Name}' must implement {nameof(IChannelConnector)}.",
                    nameof(connectorType));

            if (!Attribute.IsDefined(connectorType, typeof(ChannelSchemaAttribute)))
                throw new ArgumentException(
                    $"Type '{connectorType.Name}' must be decorated with {nameof(ChannelSchemaAttribute)}.",
                    nameof(connectorType));
        }

        private void RegisterDefaultFactory(Type connectorType)
        {
            var factoryInterface = typeof(IChannelConnectorFactory<>).MakeGenericType(connectorType);
            var defaultFactoryType = typeof(ChannelConnectorFactory<>).MakeGenericType(connectorType);

            Services.TryAddSingleton(factoryInterface, defaultFactoryType);
        }

        private void ApplyFactoryOverride<TConnector>(Type connectorType, ChannelConnectorBuilder<TConnector> builder)
            where TConnector : class, IChannelConnector
        {
            var overrideType = builder.FactoryOverrideType;
            if (overrideType == null)
                return;

            var factoryInterface = typeof(IChannelConnectorFactory<>).MakeGenericType(connectorType);
            Services.AddSingleton(factoryInterface, overrideType);
        }

        private static object ResolveFactory(IServiceProvider sp, Type connectorType)
        {
            var factoryInterface = typeof(IChannelConnectorFactory<>).MakeGenericType(connectorType);
            return sp.GetRequiredService(factoryInterface);
        }

        private static IChannelConnector CreateFromFactory(IServiceProvider sp, Type connectorType, ConnectionSettings settings, IChannelSchema schema)
        {
            var factory = ResolveFactory(sp, connectorType);
            var factoryType = factory.GetType();

            var createMethod = factoryType.GetMethod("Create", new[] { typeof(ConnectionSettings), typeof(IChannelSchema) })
                ?? throw new InvalidOperationException($"The factory for '{connectorType.Name}' does not expose a compatible Create method.");

            return (IChannelConnector)createMethod.Invoke(factory, new object[] { settings, schema })!;
        }
    }
}
