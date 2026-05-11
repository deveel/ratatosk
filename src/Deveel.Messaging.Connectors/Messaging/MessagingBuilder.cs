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
	/// Provides a builder for configuring messaging services, including connector and
	/// named-connector registrations backed by the native .NET dependency injection container.
	/// </summary>
	/// <remarks>
	/// Connectors registered via <c>AddConnector&lt;TConnector&gt;()</c> are added as
	/// singletons and can be resolved as both the concrete type and as
	/// <see cref="IChannelConnector"/> (all implementations are returned by
	/// <c>IEnumerable&lt;IChannelConnector&gt;</c>).
	/// Named connectors registered via <see cref="AddConnector{TConnector}(string,IChannelSchema?,IReadOnlyDictionary{string,object?}?,Func{IServiceProvider,IChannelSchema,TConnector}?)"/>
	/// are registered as keyed <see cref="IChannelConnector"/> singletons using the connector
	/// name as the service key.
	/// </remarks>
	public sealed class MessagingBuilder
	{
		internal MessagingBuilder(IServiceCollection services)
		{
			ArgumentNullException.ThrowIfNull(services, nameof(services));
			Services = services;
			// Register IChannelSchemaRegistry exactly once across multiple AddMessaging() calls.
			services.TryAddSingleton<IChannelSchemaRegistry, ChannelSchemaRegistry>();
		}

		/// <summary>Gets the service collection being configured.</summary>
		public IServiceCollection Services { get; }

		// ── Unnamed connector registration ────────────────────────────────────

		/// <summary>
		/// Registers a channel connector type with automatic schema discovery,
		/// backed by the native DI container.
		/// </summary>
		/// <typeparam name="TConnector">The type of connector to register.</typeparam>
		/// <param name="connectorFactory">
		/// An optional factory to create connector instances. When <c>null</c>,
		/// the connector is created via ActivatorUtilities.
		/// </param>
		public MessagingBuilder AddConnector<TConnector>(
			Func<IServiceProvider, IChannelSchema, TConnector>? connectorFactory = null)
			where TConnector : class, IChannelConnector
		{
			return AddConnector(
				typeof(TConnector),
				connectorFactory != null
					? (sp, schema) => (IChannelConnector)connectorFactory(sp, schema)
					: null);
		}

		/// <summary>
		/// Registers a channel connector type with automatic schema discovery,
		/// backed by the native DI container.
		/// </summary>
		/// <param name="connectorType">The type of connector to register.</param>
		/// <param name="connectorFactory">An optional factory to create connector instances.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="connectorType"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Thrown when the type does not implement <see cref="IChannelConnector"/> or lacks
		/// a <see cref="ChannelSchemaAttribute"/>.
		/// </exception>
		public MessagingBuilder AddConnector(
			Type connectorType,
			Func<IServiceProvider, IChannelSchema, IChannelConnector>? connectorFactory = null)
		{
			ArgumentNullException.ThrowIfNull(connectorType, nameof(connectorType));

			if (!typeof(IChannelConnector).IsAssignableFrom(connectorType))
				throw new ArgumentException(
					$"Type '{connectorType.Name}' must implement {nameof(IChannelConnector)}.",
					nameof(connectorType));

			if (!Attribute.IsDefined(connectorType, typeof(ChannelSchemaAttribute)))
				throw new ArgumentException(
					$"Type '{connectorType.Name}' must be decorated with {nameof(ChannelSchemaAttribute)}.",
					nameof(connectorType));

			var capturedFactory = connectorFactory;

			// Register the concrete type as a singleton (TryAdd is a no-op if already registered).
			Services.TryAdd(ServiceDescriptor.Singleton(connectorType, sp =>
			{
				var schema = DiscoverConnectorSchema(sp, connectorType);
				if (capturedFactory != null)
					return capturedFactory(sp, schema);
				return ActivatorUtilities.CreateInstance(sp, connectorType, schema);
			}));

			// Expose via IChannelConnector so that IEnumerable<IChannelConnector> includes it.
			Services.AddSingleton<IChannelConnector>(sp =>
				(IChannelConnector)sp.GetRequiredService(connectorType));

			return this;
		}

		// ── Named connector registration ──────────────────────────────────────

		/// <summary>
		/// Registers a channel connector type using a fluent <see cref="ChannelConnectorBuilder{TConnector}"/>
		/// for configuring connection settings loaded from <see cref="IConfiguration"/> or built
		/// fluently, plus an optional factory override.
		/// </summary>
		/// <typeparam name="TConnector">The type of connector to register.</typeparam>
		/// <param name="configure">A delegate that configures the connector builder.</param>
		/// <returns>This builder for chaining.</returns>
		public MessagingBuilder AddConnector<TConnector>(
			Action<ChannelConnectorBuilder<TConnector>> configure)
			where TConnector : class, IChannelConnector
		{
			ArgumentNullException.ThrowIfNull(configure, nameof(configure));

			var connectorType = typeof(TConnector);
			EnsureValidConnectorType(connectorType);

			var connectorBuilder = new ChannelConnectorBuilder<TConnector>(this);
			configure(connectorBuilder);

			Services.TryAdd(ServiceDescriptor.Singleton(connectorType, sp =>
			{
				var schema = connectorBuilder.SchemaOverride ?? DiscoverConnectorSchema(sp, connectorType);
				return connectorBuilder.CreateConnector(sp, schema);
			}));

			Services.AddSingleton<IChannelConnector>(sp =>
				(IChannelConnector)sp.GetRequiredService(connectorType));

			return this;
		}

		/// <summary>
		/// Registers a named connector using a fluent <see cref="ChannelConnectorBuilder{TConnector}"/>
		/// for configuring connection settings and an optional factory override.
		/// The connector is exposed as a keyed <see cref="IChannelConnector"/> singleton.
		/// </summary>
		/// <typeparam name="TConnector">The type of connector to register.</typeparam>
		/// <param name="connectorName">The unique name for this connector instance.</param>
		/// <param name="configure">A delegate that configures the connector builder.</param>
		/// <returns>This builder for chaining.</returns>
		public MessagingBuilder AddConnector<TConnector>(
			string connectorName,
			Action<ChannelConnectorBuilder<TConnector>> configure)
			where TConnector : class, IChannelConnector
		{
			ArgumentNullException.ThrowIfNullOrWhiteSpace(connectorName, nameof(connectorName));
			ArgumentNullException.ThrowIfNull(configure, nameof(configure));

			var connectorType = typeof(TConnector);
			EnsureValidConnectorType(connectorType);

			var connectorBuilder = new ChannelConnectorBuilder<TConnector>(this);
			configure(connectorBuilder);

			Services.AddKeyedSingleton<IChannelConnector>(connectorName, (sp, _) =>
			{
				var schema = connectorBuilder.SchemaOverride ?? DiscoverConnectorSchema(sp, connectorType);
				return connectorBuilder.CreateConnector(sp, schema);
			});

			Services.AddSingleton<NamedConnectorDescriptor>(sp =>
			{
				var schema   = connectorBuilder.SchemaOverride ?? DiscoverConnectorSchema(sp, connectorType);
				var settings = connectorBuilder.BuildConnectionSettings(sp);
				return new NamedConnectorDescriptor(connectorName, connectorType, schema, settings.Parameters);
			});

			return this;
		}

		/// <summary>
		/// Registers a named connector backed by the specified connector type as a keyed
		/// <see cref="IChannelConnector"/> singleton.
		/// Resolve it with <c>serviceProvider.GetRequiredKeyedService&lt;IChannelConnector&gt;(connectorName)</c>.
		/// </summary>
		/// <typeparam name="TConnector">The connector type that handles this named instance.</typeparam>
		/// <param name="connectorName">The unique name for this connector instance.</param>
		/// <param name="schema">
		/// Optional schema override. When <c>null</c> the master schema from the
		/// <see cref="ChannelSchemaAttribute"/> is used.
		/// </param>
		/// <param name="settings">Optional connector-specific settings (e.g. credentials, sender identity).</param>
		/// <param name="connectorFactory">Optional connector factory.</param>
		public MessagingBuilder AddConnector<TConnector>(
			string connectorName,
			IChannelSchema? schema = null,
			IReadOnlyDictionary<string, object?>? settings = null,
			Func<IServiceProvider, IChannelSchema, TConnector>? connectorFactory = null)
			where TConnector : class, IChannelConnector
		{
			return AddConnector(
				connectorName,
				typeof(TConnector),
				schema,
				settings,
				connectorFactory != null
					? (sp, s) => (IChannelConnector)connectorFactory(sp, s)
					: null);
		}

		/// <summary>
		/// Registers a named connector backed by the specified connector type as a keyed
		/// <see cref="IChannelConnector"/> singleton.
		/// </summary>
		/// <param name="connectorName">The unique name for this connector instance.</param>
		/// <param name="connectorType">The connector type that handles this named instance.</param>
		/// <param name="schema">
		/// Optional schema override. When <c>null</c> the master schema from the
		/// <see cref="ChannelSchemaAttribute"/> is used.
		/// </param>
		/// <param name="settings">Optional connector-specific settings.</param>
		/// <param name="connectorFactory">Optional connector factory.</param>
		public MessagingBuilder AddConnector(
			string connectorName,
			Type connectorType,
			IChannelSchema? schema = null,
			IReadOnlyDictionary<string, object?>? settings = null,
			Func<IServiceProvider, IChannelSchema, IChannelConnector>? connectorFactory = null)
		{
			ArgumentNullException.ThrowIfNullOrWhiteSpace(connectorName, nameof(connectorName));
			ArgumentNullException.ThrowIfNull(connectorType, nameof(connectorType));
			EnsureValidConnectorType(connectorType);

			var capturedSchema   = schema;
			var capturedSettings = settings
				?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			var capturedFactory  = connectorFactory;
			var capturedType     = connectorType;

			// Register as keyed IChannelConnector — resolved via GetRequiredKeyedService.
			Services.AddKeyedSingleton<IChannelConnector>(connectorName, (sp, key) =>
			{
				var effectiveSchema = ResolveEffectiveSchema(sp, capturedType, capturedSchema);
				if (capturedFactory != null)
					return capturedFactory(sp, effectiveSchema);
				return (IChannelConnector)ActivatorUtilities.CreateInstance(
					sp, capturedType, effectiveSchema);
			});

			// Register a NamedConnectorDescriptor so callers can enumerate named connectors
			// via IEnumerable<NamedConnectorDescriptor> and for IChannelSchemaRegistry.
			Services.AddSingleton<NamedConnectorDescriptor>(sp =>
			{
				var effectiveSchema = ResolveEffectiveSchema(sp, capturedType, capturedSchema);
				return new NamedConnectorDescriptor(
					connectorName, capturedType, effectiveSchema, capturedSettings);
			});

			return this;
		}

		// ── Obsolete named-channel overloads (kept for source compatibility) ──

		/// <inheritdoc cref="AddConnector{TConnector}(string,IChannelSchema?,IReadOnlyDictionary{string,object?}?,Func{IServiceProvider,IChannelSchema,TConnector}?)"/>
		[Obsolete("Use AddConnector(connectorName, ...) instead. 'Channel' was a misnomer for 'Channel Connector'.")]
		public MessagingBuilder AddChannel<TConnector>(
			string channelName,
			IChannelSchema? schema = null,
			IReadOnlyDictionary<string, object?>? settings = null,
			Func<IServiceProvider, IChannelSchema, TConnector>? connectorFactory = null)
			where TConnector : class, IChannelConnector
			=> AddConnector(channelName, schema, settings, connectorFactory);

		/// <inheritdoc cref="AddConnector(string,Type,IChannelSchema?,IReadOnlyDictionary{string,object?}?,Func{IServiceProvider,IChannelSchema,IChannelConnector}?)"/>
		[Obsolete("Use AddConnector(connectorName, connectorType, ...) instead. 'Channel' was a misnomer for 'Channel Connector'.")]
		public MessagingBuilder AddChannel(
			string channelName,
			Type connectorType,
			IChannelSchema? schema = null,
			IReadOnlyDictionary<string, object?>? settings = null,
			Func<IServiceProvider, IChannelSchema, IChannelConnector>? connectorFactory = null)
			=> AddConnector(channelName, connectorType, schema, settings, connectorFactory);

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

		private static IChannelSchema ResolveEffectiveSchema(
			IServiceProvider sp, Type connectorType, IChannelSchema? overrideSchema)
		{
			var masterSchema = DiscoverConnectorSchema(sp, connectorType);
			return overrideSchema ?? masterSchema;
		}

		internal static IChannelSchema DiscoverConnectorSchema(
			IServiceProvider services, Type connectorType)
		{
			var attribute = connectorType.GetCustomAttribute<ChannelSchemaAttribute>();
			if (attribute == null)
				throw new ArgumentException(
					$"Connector type '{connectorType.Name}' must be decorated with " +
					$"{nameof(ChannelSchemaAttribute)}.",
					nameof(connectorType));

			try
			{
				return CreateSchema(services, attribute.SchemaType);
			}
			catch (Exception ex) when (ex is not ArgumentException)
			{
				throw new InvalidOperationException(
					$"Failed to create schema for connector type '{connectorType.Name}': " +
					$"{ex.Message}", ex);
			}
		}

		private static IChannelSchema CreateSchema(IServiceProvider services, Type schemaType)
		{
			if (typeof(IChannelSchemaFactory).IsAssignableFrom(schemaType))
			{
				var factory = ActivatorUtilities.CreateInstance(services, schemaType)
					as IChannelSchemaFactory
					?? throw new InvalidOperationException(
						$"Failed to create instance of schema factory '{schemaType.Name}'.");
				return factory.CreateSchema();
			}

			if (typeof(IChannelSchema).IsAssignableFrom(schemaType))
			{
				var schemaInstance = ActivatorUtilities.CreateInstance(services, schemaType)
					as IChannelSchema
					?? throw new InvalidOperationException(
						$"Failed to create instance of schema '{schemaType.Name}'.");
				return schemaInstance;
			}

			throw new InvalidOperationException(
				$"Type '{schemaType.Name}' is not a valid schema factory or schema type.");
		}
	}
}
