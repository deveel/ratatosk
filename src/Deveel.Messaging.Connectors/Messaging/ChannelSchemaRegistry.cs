//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// A default implementation of <see cref="IChannelSchemaRegistry"/> that aggregates
	/// schemas from connectors and named connectors registered in the DI container.
	/// </summary>
	/// <remarks>
	/// Schemas are deduplicated by <c>(ChannelProvider, ChannelType)</c> pair.
	/// Unnamed connectors registered via <c>MessagingBuilder.AddConnector&lt;TConnector&gt;()</c>
	/// or directly as <see cref="IChannelConnector"/> take precedence over named connectors
	/// with the same identity.
	/// </remarks>
	internal sealed class ChannelSchemaRegistry : IChannelSchemaRegistry
	{
		private readonly IEnumerable<NamedConnectorDescriptor> _namedConnectors;
		private readonly IEnumerable<IChannelConnector> _directConnectors;

		/// <summary>
		/// Initializes a new instance of <see cref="ChannelSchemaRegistry"/>.
		/// </summary>
		/// <param name="namedConnectors">
		/// Descriptors for named connectors registered via
		/// <see cref="MessagingBuilder.AddConnector{TConnector}(string,IChannelSchema?,IReadOnlyDictionary{string,object?}?,Func{IServiceProvider,IChannelSchema,TConnector}?)"/>.
		/// </param>
		/// <param name="directConnectors">
		/// Any <see cref="IChannelConnector"/> instances registered directly
		/// in the DI container (may be empty).
		/// </param>
		public ChannelSchemaRegistry(
			IEnumerable<NamedConnectorDescriptor> namedConnectors,
			IEnumerable<IChannelConnector> directConnectors)
		{
			_namedConnectors  = namedConnectors  ?? Enumerable.Empty<NamedConnectorDescriptor>();
			_directConnectors = directConnectors ?? Enumerable.Empty<IChannelConnector>();
		}

		/// <inheritdoc/>
		public IEnumerable<IChannelSchema> GetSchemas()
		{
			var seen = new HashSet<(string provider, string type)>();
			var result = new List<IChannelSchema>();

			// Primary: schemas from unnamed connectors (registered via AddConnector or directly).
			foreach (var connector in _directConnectors)
			{
				var s   = connector.Schema;
				var key = (s.ChannelProvider.ToLowerInvariant(), s.ChannelType.ToLowerInvariant());
				if (seen.Add(key))
					result.Add(s);
			}

			// Secondary: schemas from named connectors (deduplicated).
			foreach (var descriptor in _namedConnectors)
			{
				var s   = descriptor.Schema;
				var key = (s.ChannelProvider.ToLowerInvariant(), s.ChannelType.ToLowerInvariant());
				if (seen.Add(key))
					result.Add(s);
			}

			return result;
		}

		/// <inheritdoc/>
		public IChannelSchema? FindSchema(string channelProvider, string channelType)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(channelProvider, nameof(channelProvider));
			ArgumentException.ThrowIfNullOrWhiteSpace(channelType, nameof(channelType));

			return GetSchemas().FirstOrDefault(s =>
				s.ChannelProvider.Equals(channelProvider, StringComparison.OrdinalIgnoreCase) &&
				s.ChannelType.Equals(channelType, StringComparison.OrdinalIgnoreCase));
		}

		/// <inheritdoc/>
		public bool HasSchema(string channelProvider, string channelType)
			=> FindSchema(channelProvider, channelType) != null;
	}
}
