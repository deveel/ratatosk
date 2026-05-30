//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Ratatosk.Senders;

namespace Ratatosk
{
    /// <summary>
    /// Provides a builder for configuring and registering messaging
    /// connectors into a service collection.
    /// </summary>
    public sealed class MessagingBuilder
    {
        internal MessagingBuilder(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            Services = services;
            services.TryAddSingleton<IChannelSchemaRegistry, ChannelSchemaRegistry>();
            services.TryAddSingleton<IMessageIdGenerator, DefaultMessageIdGenerator>();
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
            ArgumentNullException.ThrowIfNull(connectorType);
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
            ArgumentException.ThrowIfNullOrWhiteSpace(connectorName);
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
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

        // ── Connector type registration (no settings) ──────────────────────────

        /// <summary>
        /// Registers a connector type under the given name, without providing
        /// connection settings. This is used for multi-tenant scenarios where
        /// settings are resolved at runtime via
        /// <see cref="IMessagingClient"/> overloads that accept
        /// <see cref="ConnectionSettings"/>.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to register.
        /// </typeparam>
        /// <param name="name">
        /// The name that identifies the connector type for runtime resolution.
        /// </param>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <typeparamref name="TConnector"/> does not implement
        /// <see cref="IChannelConnector"/> or is missing the
        /// <see cref="ChannelSchemaAttribute"/>.
        /// </exception>
        public MessagingBuilder AddConnectorType<TConnector>(string name)
            where TConnector : class, IChannelConnector
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            var connectorType = typeof(TConnector);
            EnsureValidConnectorType(connectorType);
            RegisterDefaultFactory(connectorType);

            Services.AddSingleton(new ConnectorTypeRegistration(name, connectorType));
            Services.AddKeyedSingleton<ConnectorTypeEntry>(name, new ConnectorTypeEntry(name, connectorType));
            Services.TryAddSingleton<ConnectorTypeCatalog, ServiceProviderConnectorTypeCatalog>();

            return this;
        }

        /// <summary>
        /// Registers a connector type without a name, allowing runtime creation
        /// via the generic overloads of <see cref="IMessagingClient"/>.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to register.
        /// </typeparam>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        public MessagingBuilder AddConnectorType<TConnector>()
            where TConnector : class, IChannelConnector
        {
            return AddConnectorType<TConnector>(typeof(TConnector).Name);
        }

        // ── Sender identity services ──────────────────────────────────────────

        /// <summary>
        /// Registers the sender identity services (configuration registry,
        /// cache, repository, resolver) into the messaging infrastructure.
        /// </summary>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        public MessagingBuilder AddSenders()
        {
            Services.TryAddSingleton<ISenderCache>(sp => new InMemorySenderCache(TimeSpan.FromMinutes(5)));
            Services.TryAddScoped<ISenderRepository<ISender>, SenderManager<ISender>>();
            Services.TryAddScoped<ISenderResolver, SenderResolver>();
            return this;
        }

        /// <summary>
        /// Registers a sender configuration for a specific connector type
        /// using the named options pattern.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The connector type to associate the configuration with.
        /// </typeparam>
        /// <param name="options">The sender connector options.</param>
        internal void RegisterSenderConfiguration<TConnector>(SenderConnectorOptions options)
            where TConnector : IChannelConnector
        {
            Services.Configure<SenderConnectorOptions>(typeof(TConnector).FullName!, _ => { });
            Services.PostConfigure<SenderConnectorOptions>(typeof(TConnector).FullName!, o =>
            {
                o.DefaultSender = options.DefaultSender;
                o.Cache = options.Cache;
                o.CacheTtl = options.CacheTtl;
            });
        }

        // ── Message ID generator registration ─────────────────────────────────

        /// <summary>
        /// Registers a custom implementation of <see cref="IMessageIdGenerator"/>
        /// to override the default ID generation for messages and batches.
        /// </summary>
        /// <typeparam name="TGenerator">
        /// The type of the ID generator to register.
        /// </typeparam>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        public MessagingBuilder AddMessageIdGenerator<TGenerator>()
            where TGenerator : class, IMessageIdGenerator
        {
            Services.Replace(ServiceDescriptor.Singleton<IMessageIdGenerator, TGenerator>());
            return this;
        }

        /// <summary>
        /// Registers a custom implementation of <see cref="IMessageIdGenerator"/>
        /// using a factory function to create the instance.
        /// </summary>
        /// <param name="factory">
        /// A function that creates the ID generator instance.
        /// </param>
        /// <returns>
        /// Returns the current <see cref="MessagingBuilder"/> instance
        /// to allow chaining.
        /// </returns>
        public MessagingBuilder AddMessageIdGenerator(Func<IServiceProvider, IMessageIdGenerator> factory)
        {
            ArgumentNullException.ThrowIfNull(factory, nameof(factory));
            Services.Replace(ServiceDescriptor.Singleton<IMessageIdGenerator>(factory));
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
