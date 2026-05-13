namespace Deveel.Messaging
{
    /// <summary>
    /// Provides a high-level facade for sending and receiving messages through
    /// registered messaging channels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IMessagingClient"/> abstracts away the lifecycle of channel
    /// connectors, including resolution, initialization, and caching. Callers
    /// do not need to manage connector state or initialization directly.
    /// </para>
    /// <para>
    /// Channels are resolved by name from the service provider (registered via
    /// <see cref="MessagingBuilder.AddConnector{TConnector}(string, Action{ChannelConnectorBuilder{TConnector}})"/>
    /// or through runtime overloads that accept connection settings directly.
    /// </para>
    /// </remarks>
    public interface IMessagingClient
    {
        /// <summary>
        /// Sends a message through the specified named channel.
        /// </summary>
        /// <param name="channelName">
        /// The name of the channel to send the message through, as registered
        /// at startup with <c>AddConnector&lt;T&gt;(name, ...)</c>.
        /// </param>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{SendResult}"/> that indicates
        /// whether the send succeeded and carries the result details.
        /// </returns>
        Task<OperationResult<SendResult>> SendAsync(string channelName, IMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives messages from the specified named channel.
        /// </summary>
        /// <param name="channelName">
        /// The name of the channel to receive messages from.
        /// </param>
        /// <param name="source">
        /// The source containing the raw payload to parse into messages.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{ReceiveResult}"/> containing
        /// the parsed messages.
        /// </returns>
        Task<OperationResult<ReceiveResult>> ReceiveAsync(string channelName, MessageSource source, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current operational status of the specified named channel.
        /// </summary>
        /// <param name="channelName">
        /// The name of the channel to query.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{StatusInfo}"/> containing
        /// the connector's status information.
        /// </returns>
        Task<OperationResult<StatusInfo>> GetStatusAsync(string channelName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives a status update (e.g., delivery receipt) for a message
        /// sent through the specified named channel.
        /// </summary>
        /// <param name="channelName">
        /// The name of the channel that received the status callback.
        /// </param>
        /// <param name="source">
        /// The source containing the raw status callback payload.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{StatusUpdateResult}"/> containing
        /// the parsed status update.
        /// </returns>
        Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(string channelName, MessageSource source, CancellationToken cancellationToken = default);
    }
}
