namespace Ratatosk
{
    /// <summary>
    /// Provides a high-level facade for sending and receiving messages through
    /// registered messaging channels.
    /// </summary>
    public interface IMessagingClient : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Sends a message through the specified channel.
        /// </summary>
        /// <param name="request">The send request containing the message and optional context.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{SendResult}"/> that indicates
        /// whether the send succeeded and carries the result details.
        /// </returns>
        Task<OperationResult<SendResult>> SendAsync(SendRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a batch of messages through the specified channel.
        /// </summary>
        /// <param name="request">The batch send request containing the batch and optional context.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{BatchSendResult}"/> that indicates
        /// whether the batch send succeeded and carries the result details.
        /// </returns>
        Task<OperationResult<BatchSendResult>> SendBatchAsync(BatchSendRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives messages from the specified channel.
        /// </summary>
        /// <param name="request">The receive request containing the source and optional context.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{ReceiveResult}"/> containing
        /// the parsed messages.
        /// </returns>
        Task<OperationResult<ReceiveResult>> ReceiveAsync(ReceiveRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current operational status of the specified channel.
        /// </summary>
        /// <param name="request">The status request containing the channel name and optional context.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{StatusInfo}"/> containing
        /// the channel's status information.
        /// </returns>
        Task<OperationResult<StatusInfo>> GetStatusAsync(StatusRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives a status update (e.g., delivery receipt) for a message
        /// sent through the specified channel.
        /// </summary>
        /// <param name="request">The receive status request containing the source and optional context.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{StatusUpdateResult}"/> containing
        /// the parsed status update.
        /// </returns>
        Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(ReceiveStatusRequest request, CancellationToken cancellationToken = default);
    }
}
