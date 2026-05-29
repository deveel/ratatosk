//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Selects a sender from a list of candidates when resolving
    /// the sender identity for a message.
    /// </summary>
    /// <remarks>
    /// Implementations define the strategy used to choose a sender
    /// when the message does not specify an explicit sender name.
    /// </remarks>
    public interface ISenderSelector
    {
        /// <summary>
        /// Selects a sender from the given list of candidates.
        /// </summary>
        /// <param name="senders">The list of candidate sender entities.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// The selected <see cref="SenderEntity"/>, or <c>null</c>
        /// if no sender could be selected.
        /// </returns>
        ValueTask<SenderEntity?> SelectAsync(IReadOnlyList<SenderEntity> senders, CancellationToken cancellationToken = default);
    }
}
