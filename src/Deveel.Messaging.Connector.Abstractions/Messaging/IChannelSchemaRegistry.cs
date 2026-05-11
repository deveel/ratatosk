//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
	/// <summary>
	/// Defines a registry that provides access to the master schemas 
	/// of all known channel connectors.
	/// </summary>
	/// <remarks>
	/// An implementation of this interface aggregates schemas from all
	/// sources of connectors available in the application — including
	/// connectors registered through the messaging builder and any
	/// <see cref="IChannelConnector"/> instances registered directly
	/// in the dependency injection container.
	/// </remarks>
	public interface IChannelSchemaRegistry
	{
		/// <summary>
		/// Returns all channel schemas available in the registry.
		/// </summary>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> of <see cref="IChannelSchema"/> instances,
		/// one per known connector, deduplicated by channel provider and channel type.
		/// </returns>
		IEnumerable<IChannelSchema> GetSchemas();

		/// <summary>
		/// Finds the schema for a specific channel provider and type combination.
		/// </summary>
		/// <param name="channelProvider">The channel provider identifier (case-insensitive).</param>
		/// <param name="channelType">The channel type identifier (case-insensitive).</param>
		/// <returns>
		/// The matching <see cref="IChannelSchema"/>, or <c>null</c> if no schema is registered
		/// for the given provider and type.
		/// </returns>
		IChannelSchema? FindSchema(string channelProvider, string channelType);

		/// <summary>
		/// Determines whether a schema exists for the given channel provider and type.
		/// </summary>
		/// <param name="channelProvider">The channel provider identifier (case-insensitive).</param>
		/// <param name="channelType">The channel type identifier (case-insensitive).</param>
		/// <returns>
		/// <c>true</c> if a schema is registered for the given provider and type; otherwise <c>false</c>.
		/// </returns>
		bool HasSchema(string channelProvider, string channelType);
	}
}

