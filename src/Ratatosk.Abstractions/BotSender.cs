//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Represents a sender identity that is a bot or platform identity,
    /// such as a Facebook Page ID, a Telegram Bot ID, or a Slack App ID.
    /// </summary>
    public class BotSender : ISender
    {
        /// <summary>
        /// Constructs a new bot sender identity.
        /// </summary>
        /// <param name="platformId">The unique identifier of the bot on the platform.</param>
        /// <param name="name">
        /// An optional logical name for the sender. If not provided,
        /// the platform identifier is used.
        /// </param>
        /// <param name="displayName">
        /// An optional human-readable display name. If not provided,
        /// the logical name is used.
        /// </param>
        /// <param name="isActive">Whether the sender is initially active.</param>
        public BotSender(string platformId, string? name = null, string? displayName = null, bool isActive = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(platformId, nameof(platformId));

            PlatformId = platformId;
            Name = name ?? platformId;
            DisplayName = displayName ?? Name;
            IsActive = isActive;
        }

        /// <summary>
        /// Gets the unique identifier of the bot on the platform.
        /// </summary>
        public string PlatformId { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <inheritdoc />
        public bool IsActive { get; }

        /// <inheritdoc />
        public EndpointType Type => EndpointType.Id;

        /// <inheritdoc />
        public string Address => PlatformId;
    }
}
