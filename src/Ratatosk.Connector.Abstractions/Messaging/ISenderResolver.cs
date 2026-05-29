//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Resolves the sender identity for a message at send time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementations resolve <see cref="IMessage.Sender"/> to a concrete
    /// <see cref="ISender"/> before the message is validated and sent.
    /// </para>
    /// <para>
    /// Resolution is opt-in: if no resolver is registered, the connector
    /// passes the sender through as-is. A resolver may return <c>null</c>
    /// to signal that no sender identity could be determined, in which
    /// case the connector may use a default or reject the message.
    /// </para>
    /// </remarks>
    public interface ISenderResolver
    {
        /// <summary>
        /// Resolves the sender identity for the given message.
        /// </summary>
        /// <param name="message">
        /// The message for which to resolve the sender identity.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// The resolved <see cref="ISender"/> instance, or <c>null</c>
        /// if no sender identity could be determined.
        /// </returns>
        ValueTask<ISender?> ResolveSenderAsync(IMessage message, CancellationToken cancellationToken = default);
    }
}
