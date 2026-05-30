//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk.Senders
{
    /// <summary>
    /// Options for per-connector sender resolution configuration.
    /// </summary>
    public class SenderConnectorOptions
    {
        /// <summary>
        /// Gets or sets the default sender to use when no sender
        /// is explicitly specified on a message.
        /// </summary>
        public ISender? DefaultSender { get; set; }

        /// <summary>
        /// Gets or sets the cache used for sender resolution results.
        /// </summary>
        public ISenderCache? Cache { get; set; }

        /// <summary>
        /// Gets or sets the TTL for cached sender resolution results.
        /// </summary>
        public TimeSpan? CacheTtl { get; set; }
    }
}
