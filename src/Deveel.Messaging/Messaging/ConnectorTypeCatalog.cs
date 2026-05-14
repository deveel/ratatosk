using System.Collections.Concurrent;

namespace Deveel.Messaging
{
    public class ConnectorTypeCatalog
    {
        private readonly ConcurrentDictionary<string, ConnectorTypeEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

        internal void Register(string name, Type connectorType)
        {
            _entries[name] = new ConnectorTypeEntry(name, connectorType);
        }

        public bool TryGetEntry(string name, out ConnectorTypeEntry? entry)
            => _entries.TryGetValue(name, out entry);

        public bool TryGetEntry(Type connectorType, out ConnectorTypeEntry? entry)
        {
            entry = _entries.Values.FirstOrDefault(e => e.ConnectorType == connectorType);
            return entry != null;
        }
    }

    public sealed class ConnectorTypeEntry
    {
        private IChannelSchema? _cachedSchema;
        private readonly object _schemaLock = new();

        public string Name { get; }
        public Type ConnectorType { get; }

        internal ConnectorTypeEntry(string name, Type connectorType)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(connectorType);

            Name = name;
            ConnectorType = connectorType;
        }

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
