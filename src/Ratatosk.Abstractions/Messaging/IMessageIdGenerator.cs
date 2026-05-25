//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk {
	/// <summary>
	/// Defines a service for generating unique identifiers for messages and message batches.
	/// </summary>
	/// <remarks>
	/// This service is used by connectors when a message or batch is sent without
	/// an ID specified, ensuring that all messages and batches have a unique identifier
	/// that can be tracked in send results.
	/// </remarks>
	public interface IMessageIdGenerator {
		/// <summary>
		/// Generates a unique identifier for a message.
		/// </summary>
		/// <returns>
		/// A unique identifier string for a message.
		/// </returns>
		string GenerateMessageId();

		/// <summary>
		/// Generates a unique identifier for a message batch.
		/// </summary>
		/// <returns>
		/// A unique identifier string for a message batch.
		/// </returns>
		string GenerateBatchId();
	}
}
