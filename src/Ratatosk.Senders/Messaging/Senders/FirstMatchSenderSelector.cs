//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Selects the first active sender from the candidate list.
    /// </summary>
    public class FirstMatchSenderSelector : ISenderSelector
    {
        /// <inheritdoc />
        public ValueTask<SenderEntity?> SelectAsync(IReadOnlyList<SenderEntity> senders, CancellationToken cancellationToken = default)
        {
            var selected = senders.FirstOrDefault(s => s.IsActive);
            return ValueTask.FromResult(selected);
        }
    }
}
