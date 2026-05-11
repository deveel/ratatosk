//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Messaging
{
	/// <summary>
	/// A fluent builder for configuring a single channel connector registration,
	/// including its connection settings and optional factory override.
	/// </summary>
	/// <typeparam name="TConnector">
	/// The concrete connector type being configured. Must implement
	/// <see cref="IChannelConnector"/> and be decorated with
	/// <see cref="ChannelSchemaAttribute"/>.
	/// </typeparam>
	/// <remarks>
	/// <para>
	/// Obtain an instance via
	/// <see cref="MessagingBuilder.AddConnector{TConnector}(Action{ChannelConnectorBuilder{TConnector}})"/>
	/// or its named-connector overload.
	/// </para>
	/// <para>
	/// Connection settings are built lazily at DI resolution time by merging two sources
	/// in priority order (highest wins):
	/// <list type="number">
	///   <item>Fluent settings set via <see cref="WithSetting"/> / <see cref="WithSettings(Action{IDictionary{string,object?}})"/>.</item>
	///   <item>Settings loaded from an <see cref="IConfiguration"/> section via <see cref="WithSettings(string)"/>.</item>
	/// </list>
	/// </para>
	/// <para>
	/// The default connector factory passes the resolved <see cref="IChannelSchema"/> and
	/// <see cref="ConnectionSettings"/> to
	/// <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider,object[])"/>.
	/// Replace it with <see cref="UseFactory"/> when custom construction is needed.
	/// </para>
	/// </remarks>
	public sealed class ChannelConnectorBuilder<TConnector>
		where TConnector : class, IChannelConnector
	{
		private string? _configSection;
		private readonly Dictionary<string, object?> _fluentSettings =
			new(StringComparer.OrdinalIgnoreCase);
		private IChannelSchema? _schemaOverride;
		private Func<IServiceProvider, IChannelSchema, ConnectionSettings, TConnector>? _factory;

		internal ChannelConnectorBuilder(MessagingBuilder messagingBuilder)
		{
			ArgumentNullException.ThrowIfNull(messagingBuilder, nameof(messagingBuilder));
			MessagingBuilder = messagingBuilder;
		}

		/// <summary>
		/// Gets the parent <see cref="MessagingBuilder"/> that owns this builder,
		/// allowing call-chaining back to the messaging configuration.
		/// </summary>
		public MessagingBuilder MessagingBuilder { get; }

		// ── Schema override ────────────────────────────────────────────────────

		/// <summary>
		/// Overrides the connector schema discovered from <see cref="ChannelSchemaAttribute"/>.
		/// </summary>
		/// <param name="schema">The schema to use instead of the attribute-derived one.</param>
		/// <returns>This builder for chaining.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="schema"/> is <see langword="null"/>.
		/// </exception>
		public ChannelConnectorBuilder<TConnector> WithSchema(IChannelSchema schema)
		{
			ArgumentNullException.ThrowIfNull(schema, nameof(schema));
			_schemaOverride = schema;
			return this;
		}

		// ── Connection settings ────────────────────────────────────────────────

		/// <summary>
		/// Loads connection settings from the <see cref="IConfiguration"/> section
		/// at the given path. Values bound from configuration have lower priority than
		/// settings set fluently via <see cref="WithSetting"/> or
		/// <see cref="WithSettings(Action{IDictionary{string,object?}})"/>.
		/// </summary>
		/// <param name="configurationSection">
		/// The dotted path of the configuration section (e.g.
		/// <c>"Messaging:Twilio"</c>). All immediate children of that section are
		/// mapped to connection-setting keys.
		/// </param>
		/// <returns>This builder for chaining.</returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="configurationSection"/> is null or whitespace.
		/// </exception>
		public ChannelConnectorBuilder<TConnector> WithSettings(string configurationSection)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(configurationSection, nameof(configurationSection));
			_configSection = configurationSection;
			return this;
		}

		/// <summary>
		/// Adds or overrides a single connection setting key–value pair.
		/// Fluent settings always override values loaded from configuration.
		/// </summary>
		/// <param name="key">The setting key (case-insensitive).</param>
		/// <param name="value">The setting value. May be <see langword="null"/>.</param>
		/// <returns>This builder for chaining.</returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="key"/> is null or whitespace.
		/// </exception>
		public ChannelConnectorBuilder<TConnector> WithSetting(string key, object? value)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
			_fluentSettings[key] = value;
			return this;
		}

		/// <summary>
		/// Configures multiple connection settings via a delegate that receives the
		/// mutable settings dictionary directly. Entries added here always override
		/// values loaded from configuration.
		/// </summary>
		/// <param name="configure">
		/// A delegate that populates the settings dictionary. Must not be
		/// <see langword="null"/>.
		/// </param>
		/// <returns>This builder for chaining.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="configure"/> is <see langword="null"/>.
		/// </exception>
		public ChannelConnectorBuilder<TConnector> WithSettings(
			Action<IDictionary<string, object?>> configure)
		{
			ArgumentNullException.ThrowIfNull(configure, nameof(configure));
			configure(_fluentSettings);
			return this;
		}

		// ── Factory override ───────────────────────────────────────────────────

		/// <summary>
		/// Replaces the default connector factory.
		/// </summary>
		/// <param name="factory">
		/// A delegate that creates a <typeparamref name="TConnector"/> given the
		/// resolved service provider, the effective schema and the resolved
		/// connection settings.
		/// </param>
		/// <returns>This builder for chaining.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="factory"/> is <see langword="null"/>.
		/// </exception>
		public ChannelConnectorBuilder<TConnector> UseFactory(
			Func<IServiceProvider, IChannelSchema, ConnectionSettings, TConnector> factory)
		{
			ArgumentNullException.ThrowIfNull(factory, nameof(factory));
			_factory = factory;
			return this;
		}

		// ── Internal surface (used by MessagingBuilder) ────────────────────────

		/// <summary>
		/// Gets the schema override set via <see cref="WithSchema"/>, or
		/// <see langword="null"/> if none was configured (the connector's
		/// <see cref="ChannelSchemaAttribute"/> schema is used instead).
		/// </summary>
		internal IChannelSchema? SchemaOverride => _schemaOverride;

		/// <summary>
		/// Builds a <see cref="ConnectionSettings"/> at DI resolution time by merging
		/// the optional configuration section with the fluent overrides.
		/// </summary>
		/// <param name="sp">
		/// The service provider used to resolve <see cref="IConfiguration"/>.
		/// </param>
		/// <returns>
		/// A <see cref="ConnectionSettings"/> instance containing the merged values.
		/// </returns>
		internal ConnectionSettings BuildConnectionSettings(IServiceProvider sp)
		{
			// Configuration values have lower priority — written first.
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

			// Fluent settings override configuration values.
			foreach (var (key, value) in _fluentSettings)
				merged[key] = value;

			return new ConnectionSettings(merged);
		}

		/// <summary>
		/// Creates a connector instance using either the custom factory (if set via
		/// <see cref="UseFactory"/>) or the default
		/// <see cref="ActivatorUtilities.CreateInstance{T}"/> factory.
		/// </summary>
		/// <param name="sp">The resolved service provider.</param>
		/// <param name="schema">The effective schema for this connector.</param>
		/// <returns>A new <typeparamref name="TConnector"/> instance.</returns>
		internal TConnector CreateConnector(IServiceProvider sp, IChannelSchema schema)
		{
			var settings = BuildConnectionSettings(sp);

			if (_factory != null)
				return _factory(sp, schema, settings);

			// Default: pass schema + settings so ChannelConnectorBase subclasses work
			// without an explicit factory.
			return ActivatorUtilities.CreateInstance<TConnector>(sp, schema, settings);
		}
	}
}

