//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for <see cref="MessageBuilder"/> to
    /// support sender identity references.
    /// </summary>
    public static class MessageBuilderExtensions
    {
        /// <summary>
        /// Sets the sender of the message to a <see cref="SenderRef"/>
        /// that will be resolved at send time.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="senderName">The logical name of the sender.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="senderName"/> is null or whitespace.
        /// </exception>
        public static MessageBuilder FromSender(this MessageBuilder builder, string senderName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(senderName, nameof(senderName));
            return builder.From(new SenderRef(senderName));
        }
    }
}
