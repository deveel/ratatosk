//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Represents a persisted sender identity entity stored in the registry.
    /// </summary>
    public class SenderEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier of the sender entity.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the logical name used to reference the sender.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable display name of the sender.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint address of the sender.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint type of the sender as a string
        /// (e.g. <c>email</c>, <c>phone</c>, <c>label</c>).
        /// </summary>
        public string EndpointType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the sender is active and can be
        /// used to send messages.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the date and time when the entity was created.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
