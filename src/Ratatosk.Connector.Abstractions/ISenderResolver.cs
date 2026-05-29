//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Resolves a sender identity at send time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Resolution transforms a sender reference or concrete sender into
    /// the canonical identity stored in the registry:
    /// </para>
    /// <list type="bullet">
    ///   <item><see cref="SenderRef"/> — resolved by logical name.</item>
    ///   <item>Concrete <see cref="ISender"/> — resolved by endpoint type and address.</item>
    /// </list>
    /// <para>
    /// If no matching entity is found in the registry, the resolver returns
    /// <c>null</c> and the caller keeps the original sender.
    /// </para>
    /// </remarks>
    public interface ISenderResolver
    {
        /// <summary>
        /// Resolves the given sender to its canonical registry identity.
        /// </summary>
        /// <param name="sender">
        /// The sender to resolve. If it is a <see cref="SenderRef"/>,
        /// resolution is by logical name; otherwise by endpoint type
        /// and address.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// The resolved <see cref="ISender"/> instance, or <c>null</c>
        /// if no matching identity was found in the registry.
        /// </returns>
        ValueTask<ISender?> ResolveSenderAsync(ISender sender, CancellationToken cancellationToken = default);
    }
}
