//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Defines the contract for a registry of sender identities.
    /// </summary>
    /// <remarks>
    /// The registry provides CRUD operations over persisted sender entities,
    /// allowing the messaging framework to resolve, create, update, and
    /// delete senders that are used when sending messages.
    /// </remarks>
    public interface ISenderRegistry
    {
        /// <summary>
        /// Finds a sender entity by its logical name.
        /// </summary>
        /// <param name="name">The logical name of the sender.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="SenderEntity"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<SenderEntity?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a sender entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the sender.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="SenderEntity"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<SenderEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all registered sender entities.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A list of all <see cref="SenderEntity"/> instances.
        /// </returns>
        Task<IList<SenderEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new sender entity in the registry.
        /// </summary>
        /// <param name="sender">The sender entity to create.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// An <see cref="OperationResult{T}"/> containing the created entity.
        /// </returns>
        Task<OperationResult<SenderEntity>> CreateAsync(SenderEntity sender, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing sender entity in the registry.
        /// </summary>
        /// <param name="sender">The sender entity with updated values.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// An <see cref="OperationResult{T}"/> containing the updated entity.
        /// </returns>
        Task<OperationResult<SenderEntity>> UpdateAsync(SenderEntity sender, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a sender entity from the registry.
        /// </summary>
        /// <param name="sender">The sender entity to delete.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// An <see cref="OperationResult{T}"/> indicating success or failure.
        /// </returns>
        Task<OperationResult<bool>> DeleteAsync(SenderEntity sender, CancellationToken cancellationToken = default);
    }
}
