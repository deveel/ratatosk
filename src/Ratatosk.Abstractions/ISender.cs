//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Represents a sender identity that can be used to send messages
    /// through a messaging channel.
    /// </summary>
    /// <remarks>
    /// A sender is an endpoint that is registered and managed by the messaging
    /// framework, providing additional metadata such as a logical name and
    /// active status, beyond the bare endpoint address.
    /// </remarks>
    public interface ISender : IEndpoint
    {
        /// <summary>
        /// Gets the logical name of the sender, used to reference it
        /// in configuration and message building.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the human-readable display name of the sender.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets a value indicating whether the sender is currently active
        /// and can be used to send messages.
        /// </summary>
        bool IsActive { get; }
    }
}
