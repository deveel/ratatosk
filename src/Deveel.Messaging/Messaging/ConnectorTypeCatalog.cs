using System.Collections.Concurrent;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides a catalog of registered connector types that can be
    /// resolved by name or by type.
    /// </summary>
    public class ConnectorTypeCatalog
    {
        private readonly ConcurrentDictionary<string, ConnectorTypeEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

        internal void Register(string name, Type connectorType)
        {
            _entries[name] = new ConnectorTypeEntry(name, connectorType);
        }

        /// <summary>
        /// Attempts to retrieve the connector type entry by its registered name.
        /// </summary>
        /// <param name="name">The name of the connector type entry.</param>
        /// <param name="entry">
        /// When this method returns, contains the <see cref="ConnectorTypeEntry"/>
        /// if found, or <c>null</c> if not found.
        /// </param>
        /// <returns>
        /// <c>true</c> if an entry with the given name was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetEntry(string name, out ConnectorTypeEntry? entry)
            => _entries.TryGetValue(name, out entry);

        /// <summary>
        /// Attempts to retrieve the connector type entry by its connector type.
        /// </summary>
        /// <param name="connectorType">The type of the connector to find.</param>
        /// <param name="entry">
        /// When this method returns, contains the <see cref="ConnectorTypeEntry"/>
        /// if found, or <c>null</c> if not found.
        /// </param>
        /// <returns>
        /// <c>true</c> if an entry with the given connector type was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetEntry(Type connectorType, out ConnectorTypeEntry? entry)
        {
            entry = _entries.Values.FirstOrDefault(e => e.ConnectorType == connectorType);
            return entry != null;
        }
    }

    /// <summary>
    /// Represents a registered connector type with its name and
    /// cached schema information.
    /// </summary>
    public sealed class ConnectorTypeEntry
    {
        private IChannelSchema? _cachedSchema;
        private readonly object _schemaLock = new();

        /// <summary>
        /// Gets the name under which the connector type is registered.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the connector implementation.
        /// </summary>
        public Type ConnectorType { get; }

        internal ConnectorTypeEntry(string name, Type connectorType)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(connectorType);

            Name = name;
            ConnectorType = connectorType;
        }

        /// <summary>
        /// Gets or creates the schema for this connector type.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve dependencies for schema creation.
        /// </param>
        /// <returns>
        /// The <see cref="IChannelSchema"/> describing the capabilities and
        /// configuration requirements of this connector.
        /// </returns>
        public IChannelSchema GetSchema(IServiceProvider serviceProvider)
        {
            if (_cachedSchema != null)
                return _cachedSchema;

            lock (_schemaLock)
            {
                if (_cachedSchema != null)
                    return _cachedSchema;

                _cachedSchema = ConnectorSchemaHelper.DiscoverConnectorSchema(serviceProvider, ConnectorType);
                return _cachedSchema;
            }
        }
    }
}
