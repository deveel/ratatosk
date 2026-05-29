//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Always selects the sender with the specified name from the
    /// candidate list.
    /// </summary>
    public class ExplicitSenderSelector : ISenderSelector
    {
        private readonly string _senderName;

        /// <summary>
        /// Constructs the selector with the sender name to always select.
        /// </summary>
        /// <param name="senderName">The logical name of the sender to select.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="senderName"/> is null or whitespace.
        /// </exception>
        public ExplicitSenderSelector(string senderName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(senderName, nameof(senderName));
            _senderName = senderName;
        }

        /// <inheritdoc />
        public ValueTask<SenderEntity?> SelectAsync(IReadOnlyList<SenderEntity> senders, CancellationToken cancellationToken = default)
        {
            var selected = senders.FirstOrDefault(s =>
                s.IsActive &&
                string.Equals(s.Name, _senderName, StringComparison.OrdinalIgnoreCase));

            return ValueTask.FromResult(selected);
        }
    }
}
