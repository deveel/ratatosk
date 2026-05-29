//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Holds the per-connector sender resolution configuration.
    /// </summary>
    public class SenderConnectorConfiguration
    {
        /// <summary>
        /// Gets the default sender entity to use when no sender
        /// is explicitly specified on a message.
        /// </summary>
        public SenderEntity? DefaultSender { get; init; }

        /// <summary>
        /// Gets the selector strategy used to choose a sender
        /// when multiple candidates are available.
        /// </summary>
        public ISenderSelector? Selector { get; init; }

        /// <summary>
        /// Gets the cache used for sender resolution results.
        /// </summary>
        public ISenderCache? Cache { get; init; }

        /// <summary>
        /// Gets the TTL for cached sender resolution results.
        /// </summary>
        public TimeSpan? CacheTtl { get; init; }
    }
}
