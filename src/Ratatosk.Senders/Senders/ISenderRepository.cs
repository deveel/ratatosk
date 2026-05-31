namespace Ratatosk.Senders
{
    /// <summary>
    /// Defines the contract for a repository of sender identities,
    /// extending standard CRUD with sender-specific query operations.
    /// </summary>
    /// <typeparam name="TSender">
    /// The type of sender entity, which must implement <see cref="ISender"/>.
    /// </typeparam>
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

        /// <summary>
        /// Sets the active state of a sender by its identifier.
        /// </summary>
        /// <param name="sender">The sender to update.</param>
        /// <param name="isActive">
        /// <c>true</c> to activate the sender; <c>false</c> to deactivate.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        Task SetActiveAsync(TSender sender, bool isActive, CancellationToken cancellationToken = default);
    }
}
