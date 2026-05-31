//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Carries all information needed to resolve a sender identity at send time.
    /// </summary>
    public class SenderResolutionContext
    {
        /// <summary>
        /// Constructs a new resolution context.
        /// </summary>
        /// <param name="sender">The sender endpoint to resolve.</param>
        /// <param name="settings">The connection settings for the resolution.</param>
        /// <param name="tenantId">An optional tenant identifier for multi-tenant scenarios.</param>
        public SenderResolutionContext(IEndpoint? sender, ConnectionSettings settings, string? tenantId = null)
        {
            Sender = sender;
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            TenantId = tenantId;
        }

        /// <summary>
        /// Gets the sender endpoint from the message.
        /// May be <see cref="ISender"/>, <see cref="SenderRef"/>, or a plain <see cref="IEndpoint"/>.
        /// </summary>
        public IEndpoint? Sender { get; }

        /// <summary>
        /// Gets the connection settings that may contain default sender configuration.
        /// </summary>
        public ConnectionSettings Settings { get; }

        /// <summary>
        /// Gets an optional tenant/owner identifier for multi-tenant scenarios.
        /// </summary>
        public string? TenantId { get; }
    }
}
