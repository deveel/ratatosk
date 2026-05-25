//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk {
	/// <summary>
	/// Extension methods for <see cref="IMessageIdGenerator"/>.
	/// </summary>
	public static class MessageIdGeneratorExtensions {
		/// <summary>
		/// Ensures that the message has an ID, generating one if necessary.
		/// </summary>
		/// <param name="generator">The ID generator instance.</param>
		/// <param name="message">The message to ensure has an ID.</param>
		/// <returns>
		/// The message ID, either existing or newly generated.
		/// </returns>
		public static string EnsureMessageId(this IMessageIdGenerator generator, IMessage message) {
			ArgumentNullException.ThrowIfNull(generator);
			ArgumentNullException.ThrowIfNull(message);

			if (string.IsNullOrEmpty(message.Id)) {
				message.Id = generator.GenerateMessageId();
			}

			return message.Id!;
		}

		/// <summary>
		/// Ensures that the batch has an ID, generating one if necessary.
		/// </summary>
		/// <param name="generator">The ID generator instance.</param>
		/// <param name="batch">The batch to ensure has an ID.</param>
		/// <returns>
		/// The batch ID, either existing or newly generated.
		/// </returns>
		public static string EnsureBatchId(this IMessageIdGenerator generator, IMessageBatch batch) {
			ArgumentNullException.ThrowIfNull(generator);
			ArgumentNullException.ThrowIfNull(batch);

			if (string.IsNullOrEmpty(batch.Id)) {
				batch.Id = generator.GenerateBatchId();
			}

			return batch.Id!;
		}
	}
}
