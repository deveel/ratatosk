//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Represents a sender identity that is an alphanumeric branded sender name,
    /// commonly used for SMS sender IDs that display a brand name instead of a number.
    /// </summary>
    public class AlphaNumericSender : ISender
    {
        /// <summary>
        /// Constructs a new alphanumeric sender identity.
        /// </summary>
        /// <param name="brandName">The branded sender name.</param>
        /// <param name="name">
        /// An optional logical name for the sender. If not provided,
        /// the brand name is used.
        /// </param>
        /// <param name="displayName">
        /// An optional human-readable display name. If not provided,
        /// the logical name is used.
        /// </param>
        /// <param name="isActive">Whether the sender is initially active.</param>
        public AlphaNumericSender(string brandName, string? name = null, string? displayName = null, bool isActive = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(brandName, nameof(brandName));

            BrandName = brandName;
            Name = name ?? brandName;
            DisplayName = displayName ?? Name;
            IsActive = isActive;
        }

        /// <summary>
        /// Gets the branded sender name.
        /// </summary>
        public string BrandName { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <inheritdoc />
        public bool IsActive { get; }

        /// <inheritdoc />
        public EndpointType Type => EndpointType.Label;

        /// <inheritdoc />
        public string Address => BrandName;
    }
}
