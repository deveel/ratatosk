namespace Ratatosk
{
	internal sealed class ChannelSchemaRegistry : IChannelSchemaRegistry
	{
		private readonly IEnumerable<NamedConnectorDescriptor> _namedConnectors;
		private readonly IEnumerable<IChannelConnector> _directConnectors;

		public ChannelSchemaRegistry(
			IEnumerable<NamedConnectorDescriptor> namedConnectors,
			IEnumerable<IChannelConnector> directConnectors)
		{
			_namedConnectors  = namedConnectors  ?? Enumerable.Empty<NamedConnectorDescriptor>();
			_directConnectors = directConnectors ?? Enumerable.Empty<IChannelConnector>();
		}

		public IEnumerable<IChannelSchema> GetSchemas()
		{
			var seen = new HashSet<(string provider, string type)>();
			var result = new List<IChannelSchema>();

			foreach (var connector in _directConnectors)
			{
				var s   = connector.Schema;
				var key = (s.ChannelProvider.ToLowerInvariant(), s.ChannelType.ToLowerInvariant());
				if (seen.Add(key))
					result.Add(s);
			}

			foreach (var descriptor in _namedConnectors)
			{
				var s   = descriptor.Schema;
				var key = (s.ChannelProvider.ToLowerInvariant(), s.ChannelType.ToLowerInvariant());
				if (seen.Add(key))
					result.Add(s);
			}

			return result;
		}

		public IChannelSchema? FindSchema(string channelProvider, string channelType)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(channelProvider, nameof(channelProvider));
			ArgumentException.ThrowIfNullOrWhiteSpace(channelType, nameof(channelType));

			return GetSchemas().FirstOrDefault(s =>
				s.ChannelProvider.Equals(channelProvider, StringComparison.OrdinalIgnoreCase) &&
				s.ChannelType.Equals(channelType, StringComparison.OrdinalIgnoreCase));
		}

		public bool HasSchema(string channelProvider, string channelType)
			=> FindSchema(channelProvider, channelType) != null;
	}
}
