//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Marker interface for an endpoint that represents an unresolved sender reference.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface indicate that the sender identity has not yet
    /// been resolved to a concrete <see cref="ISender"/>. The sender resolver
    /// detects this interface to trigger resolution at send time.
    /// </remarks>
    public interface IUnresolvedSender : IEndpoint
    {
    }
}
