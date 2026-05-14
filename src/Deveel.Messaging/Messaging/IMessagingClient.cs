namespace Deveel.Messaging
{
    /// <summary>
    /// Provides a high-level facade for sending and receiving messages through
    /// registered messaging channels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Channels are resolved by name from the service provider (registered via
    /// <see cref="MessagingBuilder.AddConnector{TConnector}(string, Action{ChannelConnectorBuilder{TConnector}})"/>
    /// or through runtime overloads that accept connection settings directly.
    /// </para>
    /// </remarks>
    public interface IMessagingClient : IDisposable, IAsyncDisposable
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
        /// Sends a message through a dynamically resolved channel, using
        /// runtime-provided connection settings.
        /// </summary>
        /// <param name="channelName">
        /// The name of the channel type to use, as registered via
        /// <c>AddConnectorType&lt;T&gt;(name)</c>.
        /// </param>
        /// <param name="settings">
        /// The connection settings to use for creating the connector at runtime.
        /// </param>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{SendResult}"/> that indicates
        /// whether the send succeeded and carries the result details.
        /// </returns>
        Task<OperationResult<SendResult>> SendAsync(string channelName, ConnectionSettings settings, IMessage message, CancellationToken cancellationToken = default);

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
        /// Receives messages from a dynamically resolved channel, using
        /// runtime-provided connection settings.
        /// </summary>
        /// <param name="channelName">
        /// The name of the channel type to use, as registered via
        /// <c>AddConnectorType&lt;T&gt;(name)</c>.
        /// </param>
        /// <param name="settings">
        /// The connection settings to use for creating the connector at runtime.
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
        Task<OperationResult<ReceiveResult>> ReceiveAsync(string channelName, ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default);

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
        /// Gets the status of a dynamically resolved channel, using
        /// runtime-provided connection settings.
        /// </summary>
        /// <param name="channelName">
        /// The name of the channel type to use, as registered via
        /// <c>AddConnectorType&lt;T&gt;(name)</c>.
        /// </param>
        /// <param name="settings">
        /// The connection settings to use for creating the connector at runtime.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{StatusInfo}"/> containing
        /// the connector's status information.
        /// </returns>
        Task<OperationResult<StatusInfo>> GetStatusAsync(string channelName, ConnectionSettings settings, CancellationToken cancellationToken = default);

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

        /// <summary>
        /// Receives a status update for a dynamically resolved channel, using
        /// runtime-provided connection settings.
        /// </summary>
        /// <param name="channelName">
        /// The name of the channel type to use, as registered via
        /// <c>AddConnectorType&lt;T&gt;(name)</c>.
        /// </param>
        /// <param name="settings">
        /// The connection settings to use for creating the connector at runtime.
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
        Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(string channelName, ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default);

        // ── Type-parameterized overloads ─────────────────────────────────────

        /// <summary>
        /// Sends a message through the channel connector of the specified type,
        /// resolved from the dependency injection container.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to use, as registered via
        /// <c>AddConnector&lt;TConnector&gt;()</c>.
        /// </typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{SendResult}"/> that indicates
        /// whether the send succeeded and carries the result details.
        /// </returns>
        Task<OperationResult<SendResult>> SendAsync<TConnector>(IMessage message, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector;

        /// <summary>
        /// Sends a message through a dynamically created connector of the
        /// specified type, using runtime-provided connection settings.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to create at runtime.
        /// </typeparam>
        /// <param name="settings">
        /// The connection settings to use for creating the connector.
        /// </param>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{SendResult}"/> that indicates
        /// whether the send succeeded and carries the result details.
        /// </returns>
        Task<OperationResult<SendResult>> SendAsync<TConnector>(ConnectionSettings settings, IMessage message, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector;

        /// <summary>
        /// Receives messages from the channel connector of the specified type,
        /// resolved from the dependency injection container.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to use, as registered via
        /// <c>AddConnector&lt;TConnector&gt;()</c>.
        /// </typeparam>
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
        Task<OperationResult<ReceiveResult>> ReceiveAsync<TConnector>(MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector;

        /// <summary>
        /// Receives messages from a dynamically created connector of the
        /// specified type, using runtime-provided connection settings.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to create at runtime.
        /// </typeparam>
        /// <param name="settings">
        /// The connection settings to use for creating the connector.
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
        Task<OperationResult<ReceiveResult>> ReceiveAsync<TConnector>(ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector;

        /// <summary>
        /// Gets the status of the channel connector of the specified type,
        /// resolved from the dependency injection container.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to use, as registered via
        /// <c>AddConnector&lt;TConnector&gt;()</c>.
        /// </typeparam>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{StatusInfo}"/> containing
        /// the connector's status information.
        /// </returns>
        Task<OperationResult<StatusInfo>> GetStatusAsync<TConnector>(CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector;

        /// <summary>
        /// Gets the status of a dynamically created connector of the
        /// specified type, using runtime-provided connection settings.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to create at runtime.
        /// </typeparam>
        /// <param name="settings">
        /// The connection settings to use for creating the connector.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{StatusInfo}"/> containing
        /// the connector's status information.
        /// </returns>
        Task<OperationResult<StatusInfo>> GetStatusAsync<TConnector>(ConnectionSettings settings, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector;

        /// <summary>
        /// Receives a message status update using the channel connector of the
        /// specified type, resolved from the dependency injection container.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to use, as registered via
        /// <c>AddConnector&lt;TConnector&gt;()</c>.
        /// </typeparam>
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
        Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync<TConnector>(MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector;

        /// <summary>
        /// Receives a message status update from a dynamically created connector
        /// of the specified type, using runtime-provided connection settings.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector to create at runtime.
        /// </typeparam>
        /// <param name="settings">
        /// The connection settings to use for creating the connector.
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
        Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync<TConnector>(ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector;
    }
}
