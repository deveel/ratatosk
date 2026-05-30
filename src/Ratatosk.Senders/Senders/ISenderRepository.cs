//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Kista;

namespace Ratatosk
{
    /// <summary>
    /// Defines the contract for a repository of sender identities.
    /// </summary>
    /// <typeparam name="TSender">
    /// The type of sender entity, which must implement <see cref="ISender"/>.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// This interface extends <see cref="IRepository{TSender}"/> from Kista,
    /// inheriting all standard CRUD, pagination, and query capabilities.
    /// </para>
    /// <para>
    /// Custom repository implementations can use their own storage-bound
    /// entity types (e.g., <c>DbSender</c>, <c>MongoSender</c>) that
    /// implement <see cref="ISender"/>, handling the transformation between
    /// the domain model and storage format internally.
    /// </para>
    /// </remarks>
    public interface ISenderRepository<TSender> : IRepository<TSender>
        where TSender : class, ISender
    {
        /// <summary>
        /// Finds a sender by its logical name.
        /// </summary>
        /// <param name="name">The logical name of the sender.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// The sender if found; otherwise <c>null</c>.
        /// </returns>
        Task<TSender?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a sender by its endpoint address and type.
        /// </summary>
        /// <param name="address">The endpoint address to match.</param>
        /// <param name="endpointType">The endpoint type.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// The sender if found; otherwise <c>null</c>.
        /// </returns>
        Task<TSender?> FindByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all active sender entities.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A list of all active senders.
        /// </returns>
        Task<IList<TSender>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    }
}
