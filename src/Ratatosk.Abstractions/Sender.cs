//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Represents a concrete sender identity that can be persisted and resolved
    /// by the sender registry.
    /// </summary>
    /// <remarks>
    /// This is the default concrete implementation of <see cref="ISender"/> used
    /// by the in-memory sender repository. Custom repository implementations may
    /// use their own entity types that implement <see cref="ISender"/>.
    /// </remarks>
    public class Sender : ISender
    {
        /// <summary>
        /// Gets or sets the unique identifier of the sender.
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
        /// Gets or sets the endpoint type of the sender.
        /// </summary>
        public EndpointType EndpointType { get; set; } = EndpointType.Any;

        /// <summary>
        /// Gets or sets whether the sender is active and can be used to send messages.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the date and time when the sender was created.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the sender was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <inheritdoc />
        EndpointType IEndpoint.Type => EndpointType;
    }
}
