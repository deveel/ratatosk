//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Provides descriptive information about a named connector registration.
	/// </summary>
	/// <remarks>
	/// A named connector is a uniquely named instance of a channel connector type that
	/// carries its own schema (which controls behavior) and settings (which hold
	/// identity and configuration values). Multiple named connectors can share the
	/// same underlying connector type but differ in schema and/or settings —
	/// for example, two Twilio SMS connectors, one for marketing and one for support,
	/// each with a different phone number and authentication credential.
	/// </remarks>
	public sealed class NamedConnectorDescriptor
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NamedConnectorDescriptor"/> class.
		/// </summary>
		/// <param name="name">The unique name that identifies this connector instance.</param>
		/// <param name="connectorType">The type of connector used by this named instance.</param>
		/// <param name="schema">The schema that defines this connector's behavior and capabilities.</param>
		/// <param name="settings">
		/// An optional dictionary of connector-specific settings, such as credentials,
		/// sender identities, or custom configuration values.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="name"/>, <paramref name="connectorType"/>,
		/// or <paramref name="schema"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="connectorType"/> does not implement
		/// <see cref="IChannelConnector"/>, or when <paramref name="name"/> is
		/// empty or whitespace.
		/// </exception>
		public NamedConnectorDescriptor(
			string name,
			Type connectorType,
			IChannelSchema schema,
			IReadOnlyDictionary<string, object?>? settings = null)
		{
			ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
			ArgumentNullException.ThrowIfNull(connectorType, nameof(connectorType));
			ArgumentNullException.ThrowIfNull(schema, nameof(schema));

			if (!typeof(IChannelConnector).IsAssignableFrom(connectorType))
				throw new ArgumentException(
					$"Type '{connectorType.Name}' must implement {nameof(IChannelConnector)}.",
					nameof(connectorType));

			Name = name;
			ConnectorType = connectorType;
			Schema = schema;
			Settings = settings ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Gets the unique name that identifies this connector instance.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the type of the connector used by this named instance.
		/// </summary>
		public Type ConnectorType { get; }

		/// <summary>
		/// Gets the schema that defines this connector's behavior and capabilities.
		/// </summary>
		/// <remarks>
		/// This schema may be a restriction or customization of the connector's master
		/// schema, allowing different named instances of the same connector type to
		/// expose different capabilities.
		/// </remarks>
		public IChannelSchema Schema { get; }

		/// <summary>
		/// Gets the connector-specific settings dictionary.
		/// </summary>
		/// <remarks>
		/// Settings typically include identity values (sender phone number, email address,
		/// bot token) and credentials (API keys, secrets) that are unique to this
		/// connector instance.
		/// </remarks>
		public IReadOnlyDictionary<string, object?> Settings { get; }

		/// <summary>
		/// Gets the channel provider identifier from the schema.
		/// </summary>
		public string ChannelProvider => Schema.ChannelProvider;

		/// <summary>
		/// Gets the channel type identifier from the schema.
		/// </summary>
		public string ChannelType => Schema.ChannelType;

		/// <summary>
		/// Gets the schema version.
		/// </summary>
		public string Version => Schema.Version;

		/// <summary>
		/// Gets the optional display name from the schema.
		/// </summary>
		public string? DisplayName => Schema.DisplayName;

		/// <summary>
		/// Gets the capabilities defined by the schema for this connector instance.
		/// </summary>
		public ChannelCapability Capabilities => Schema.Capabilities;

		/// <summary>
		/// Returns the value of a setting with the specified key, cast to
		/// <typeparamref name="T"/>, or the default value of <typeparamref name="T"/>
		/// if the key does not exist or the value cannot be cast.
		/// </summary>
		/// <typeparam name="T">The expected type of the setting value.</typeparam>
		/// <param name="key">The setting key to look up (case-insensitive).</param>
		/// <returns>
		/// The typed setting value, or <c>default(T)</c> if not found or incompatible.
		/// </returns>
		public T? GetSetting<T>(string key)
		{
			if (Settings.TryGetValue(key, out var value) && value is T typed)
				return typed;
			return default;
		}

		/// <summary>
		/// Attempts to retrieve a setting value by key, cast to <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The expected type of the setting value.</typeparam>
		/// <param name="key">The setting key to look up (case-insensitive).</param>
		/// <param name="value">
		/// When this method returns <c>true</c>, contains the typed value;
		/// otherwise the default value of <typeparamref name="T"/>.
		/// </param>
		/// <returns>
		/// <c>true</c> if the key exists and the value can be cast to
		/// <typeparamref name="T"/>; otherwise <c>false</c>.
		/// </returns>
		public bool TryGetSetting<T>(string key, out T? value)
		{
			if (Settings.TryGetValue(key, out var raw) && raw is T typed)
			{
				value = typed;
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Returns a string representation of this named connector descriptor.
		/// </summary>
		public override string ToString() =>
			$"{Name} ({Schema.GetLogicalIdentity()})";

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			obj is NamedConnectorDescriptor other &&
			string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

		/// <inheritdoc/>
		public override int GetHashCode() =>
			StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
	}
}

