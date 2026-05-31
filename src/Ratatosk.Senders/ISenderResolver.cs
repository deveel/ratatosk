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
    /// Resolution transforms an endpoint reference or concrete sender into
    /// the canonical identity stored in the registry:
    /// </para>
    /// <list type="bullet">
    ///   <item><see cref="IUnresolvedSender"/> — resolved by logical name.</item>
    ///   <item>Concrete <see cref="ISender"/> — resolved by endpoint type and address.</item>
    ///   <item>Plain <see cref="IEndpoint"/> — not resolved (returns <c>null</c>).</item>
    /// </list>
    /// <para>
    /// If no matching entity is found in the registry, the resolver falls back
    /// to the default sender configured in the connection settings, or returns
    /// <c>null</c> to keep the original sender.
    /// </para>
    /// </remarks>
    public interface ISenderResolver
    {
        /// <summary>
        /// Resolves the sender for the given context.
        /// </summary>
        /// <param name="context">
        /// The resolution context carrying the sender endpoint, connection settings,
        /// and optional tenant information.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// The resolved <see cref="ISender"/> instance, or <c>null</c>
        /// if no matching identity was found and no default is configured.
        /// </returns>
        ValueTask<ISender?> ResolveAsync(SenderResolutionContext context, CancellationToken cancellationToken = default);
    }
}
