//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Represents a sender identity that is an email address,
    /// optionally with a display name.
    /// </summary>
    public class EmailSender : ISender
    {
        /// <summary>
        /// Constructs a new email sender identity.
        /// </summary>
        /// <param name="address">The email address of the sender.</param>
        /// <param name="displayName">An optional display name for the sender.</param>
        /// <param name="name">
        /// An optional logical name for the sender. If not provided,
        /// the email address is used.
        /// </param>
        /// <param name="isActive">Whether the sender is initially active.</param>
        public EmailSender(string address, string? displayName = null, string? name = null, bool isActive = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(address, nameof(address));

            Address = address;
            DisplayName = displayName ?? name ?? address;
            Name = name ?? address;
            IsActive = isActive;
        }

        /// <inheritdoc />
        public string Address { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool IsActive { get; }

        /// <inheritdoc />
        public EndpointType Type => EndpointType.EmailAddress;
    }
}
