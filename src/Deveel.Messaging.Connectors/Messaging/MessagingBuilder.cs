//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Reflection;

namespace Deveel.Messaging
{
    public sealed class MessagingBuilder
    {
        internal MessagingBuilder(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            Services = services;
            services.TryAddSingleton<IChannelSchemaRegistry, ChannelSchemaRegistry>();
        }

        public IServiceCollection Services { get; }

        // ── Unnamed connector registration ────────────────────────────────────

        public MessagingBuilder AddConnector<TConnector>()
            where TConnector : class, IChannelConnector
        {
            return AddConnector(typeof(TConnector));
        }

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

        // ── Named + fluent connector registration ─────────────────────────────

        public MessagingBuilder AddConnector<TConnector>(string connectorName)
            where TConnector : class, IChannelConnector
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(connectorName, nameof(connectorName));
            return AddConnector<TConnector>(connectorName, _ => { });
        }

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
