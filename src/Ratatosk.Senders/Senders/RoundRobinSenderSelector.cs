//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Threading;

namespace Ratatosk
{
    /// <summary>
    /// Rotates through the active senders in the candidate list
    /// using a round-robin strategy.
    /// </summary>
    public class RoundRobinSenderSelector : ISenderSelector
    {
        private int _lastIndex = -1;

        /// <inheritdoc />
        public ValueTask<SenderEntity?> SelectAsync(IReadOnlyList<SenderEntity> senders, CancellationToken cancellationToken = default)
        {
            var active = senders.Where(s => s.IsActive).ToList();

            if (active.Count == 0)
                return ValueTask.FromResult<SenderEntity?>(null);

            var index = Interlocked.Increment(ref _lastIndex);
            var selected = active[index % active.Count];

            return ValueTask.FromResult<SenderEntity?>(selected);
        }
    }
}
