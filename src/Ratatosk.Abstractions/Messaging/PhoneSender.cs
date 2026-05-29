//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Represents a sender identity that is a phone number,
    /// such as a long code, short code, or toll-free number.
    /// </summary>
    public class PhoneSender : ISender
    {
        /// <summary>
        /// Constructs a new phone number sender identity.
        /// </summary>
        /// <param name="phoneNumber">The phone number in E.164 format.</param>
        /// <param name="name">
        /// An optional logical name for the sender. If not provided,
        /// the phone number is used as the name.
        /// </param>
        /// <param name="isActive">Whether the sender is initially active.</param>
        /// <param name="displayName">
        /// An optional human-readable display name. If not provided,
        /// the logical name is used.
        /// </param>
        public PhoneSender(string phoneNumber, string? name = null, bool isActive = true, string? displayName = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));

            PhoneNumber = phoneNumber;
            Name = name ?? phoneNumber;
            IsActive = isActive;
            DisplayName = displayName ?? Name;
        }

        /// <summary>
        /// Gets the phone number of the sender.
        /// </summary>
        public string PhoneNumber { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <inheritdoc />
        public bool IsActive { get; }

        /// <inheritdoc />
        public EndpointType Type => EndpointType.PhoneNumber;

        /// <inheritdoc />
        public string Address => PhoneNumber;
    }
}
