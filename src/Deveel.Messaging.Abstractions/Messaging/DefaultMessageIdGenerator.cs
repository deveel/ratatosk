//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging {
	/// <summary>
	/// A default implementation of <see cref="IMessageIdGenerator"/> that uses
	/// GUIDs to generate unique identifiers for messages and batches.
	/// </summary>
	public class DefaultMessageIdGenerator : IMessageIdGenerator {
		/// <inheritdoc/>
		public string GenerateMessageId() {
			return Guid.NewGuid().ToString("N");
		}

		/// <inheritdoc/>
		public string GenerateBatchId() {
			return Guid.NewGuid().ToString("N");
		}
	}
}
