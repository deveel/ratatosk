namespace Ratatosk;

/// <summary>
/// Extension methods for <see cref="IMessagingClient"/> that provide
/// the legacy flat-parameter overloads, delegating to the request-object
/// core methods.
/// </summary>
public static class MessagingClientExtensions
{
    // ── SendAsync ────────────────────────────────────────────────────────

    /// <inheritdoc cref="IMessagingClient.SendAsync(SendRequest, CancellationToken)"/>
    public static Task<OperationResult<SendResult>> SendAsync(
        this IMessagingClient client,
        string channelName, IMessage message,
        CancellationToken cancellationToken = default)
        => client.SendAsync(new SendRequest(channelName, message), cancellationToken);

    /// <inheritdoc cref="IMessagingClient.SendAsync(SendRequest, CancellationToken)"/>
    public static Task<OperationResult<SendResult>> SendAsync(
        this IMessagingClient client,
        string channelName, ConnectionSettings settings, IMessage message,
        CancellationToken cancellationToken = default)
        => client.SendAsync(
            new SendRequest(channelName, message) { ConnectionSettings = settings },
            cancellationToken);

    /// <inheritdoc cref="IMessagingClient.SendAsync(SendRequest, CancellationToken)"/>
    public static Task<OperationResult<SendResult>> SendAsync<TConnector>(
        this IMessagingClient client,
        IMessage message,
        CancellationToken cancellationToken = default)
        where TConnector : class, IChannelConnector
        => client.SendAsync(
            new SendRequest(typeof(TConnector).Name, message)
            {
                ConnectorType = typeof(TConnector)
            },
            cancellationToken);

    /// <inheritdoc cref="IMessagingClient.SendAsync(SendRequest, CancellationToken)"/>
    public static Task<OperationResult<SendResult>> SendAsync<TConnector>(
        this IMessagingClient client,
        ConnectionSettings settings, IMessage message,
        CancellationToken cancellationToken = default)
        where TConnector : class, IChannelConnector
        => client.SendAsync(
            new SendRequest(typeof(TConnector).Name, message)
            {
                ConnectionSettings = settings,
                ConnectorType = typeof(TConnector)
            },
            cancellationToken);

    // ── SendBatchAsync (new) ─────────────────────────────────────────────

    /// <inheritdoc cref="IMessagingClient.SendBatchAsync(BatchSendRequest, CancellationToken)"/>
    public static Task<OperationResult<BatchSendResult>> SendBatchAsync(
        this IMessagingClient client,
        string channelName, IMessageBatch batch,
        CancellationToken cancellationToken = default)
        => client.SendBatchAsync(new BatchSendRequest(channelName, batch), cancellationToken);

    /// <inheritdoc cref="IMessagingClient.SendBatchAsync(BatchSendRequest, CancellationToken)"/>
    public static Task<OperationResult<BatchSendResult>> SendBatchAsync(
        this IMessagingClient client,
        string channelName, ConnectionSettings settings, IMessageBatch batch,
        CancellationToken cancellationToken = default)
        => client.SendBatchAsync(
            new BatchSendRequest(channelName, batch) { ConnectionSettings = settings },
            cancellationToken);

    /// <inheritdoc cref="IMessagingClient.SendBatchAsync(BatchSendRequest, CancellationToken)"/>
    public static Task<OperationResult<BatchSendResult>> SendBatchAsync<TConnector>(
        this IMessagingClient client,
        IMessageBatch batch,
        CancellationToken cancellationToken = default)
        where TConnector : class, IChannelConnector
        => client.SendBatchAsync(
            new BatchSendRequest(typeof(TConnector).Name, batch)
            {
                ConnectorType = typeof(TConnector)
            },
            cancellationToken);

    /// <inheritdoc cref="IMessagingClient.SendBatchAsync(BatchSendRequest, CancellationToken)"/>
    public static Task<OperationResult<BatchSendResult>> SendBatchAsync<TConnector>(
        this IMessagingClient client,
        ConnectionSettings settings, IMessageBatch batch,
        CancellationToken cancellationToken = default)
        where TConnector : class, IChannelConnector
        => client.SendBatchAsync(
            new BatchSendRequest(typeof(TConnector).Name, batch)
            {
                ConnectionSettings = settings,
                ConnectorType = typeof(TConnector)
            },
            cancellationToken);

    // ── ReceiveAsync ─────────────────────────────────────────────────────

    /// <inheritdoc cref="IMessagingClient.ReceiveAsync(ReceiveRequest, CancellationToken)"/>
    public static Task<OperationResult<ReceiveResult>> ReceiveAsync(
        this IMessagingClient client,
        string channelName, MessageSource source,
        CancellationToken cancellationToken = default)
        => client.ReceiveAsync(new ReceiveRequest(channelName, source), cancellationToken);

    /// <inheritdoc cref="IMessagingClient.ReceiveAsync(ReceiveRequest, CancellationToken)"/>
    public static Task<OperationResult<ReceiveResult>> ReceiveAsync(
        this IMessagingClient client,
        string channelName, ConnectionSettings settings, MessageSource source,
        CancellationToken cancellationToken = default)
        => client.ReceiveAsync(
            new ReceiveRequest(channelName, source) { ConnectionSettings = settings },
            cancellationToken);

    /// <inheritdoc cref="IMessagingClient.ReceiveAsync(ReceiveRequest, CancellationToken)"/>
    public static Task<OperationResult<ReceiveResult>> ReceiveAsync<TConnector>(
        this IMessagingClient client,
        MessageSource source,
        CancellationToken cancellationToken = default)
        where TConnector : class, IChannelConnector
        => client.ReceiveAsync(
            new ReceiveRequest(typeof(TConnector).Name, source)
            {
                ConnectorType = typeof(TConnector)
            },
            cancellationToken);

    /// <inheritdoc cref="IMessagingClient.ReceiveAsync(ReceiveRequest, CancellationToken)"/>
    public static Task<OperationResult<ReceiveResult>> ReceiveAsync<TConnector>(
        this IMessagingClient client,
        ConnectionSettings settings, MessageSource source,
        CancellationToken cancellationToken = default)
        where TConnector : class, IChannelConnector
        => client.ReceiveAsync(
            new ReceiveRequest(typeof(TConnector).Name, source)
            {
                ConnectionSettings = settings,
                ConnectorType = typeof(TConnector)
            },
            cancellationToken);

    // ── GetStatusAsync ───────────────────────────────────────────────────

    /// <inheritdoc cref="IMessagingClient.GetStatusAsync(StatusRequest, CancellationToken)"/>
    public static Task<OperationResult<StatusInfo>> GetStatusAsync(
        this IMessagingClient client,
        string channelName,
        CancellationToken cancellationToken = default)
        => client.GetStatusAsync(new StatusRequest(channelName), cancellationToken);

    /// <inheritdoc cref="IMessagingClient.GetStatusAsync(StatusRequest, CancellationToken)"/>
    public static Task<OperationResult<StatusInfo>> GetStatusAsync(
        this IMessagingClient client,
        string channelName, ConnectionSettings settings,
        CancellationToken cancellationToken = default)
        => client.GetStatusAsync(
            new StatusRequest(channelName) { ConnectionSettings = settings },
            cancellationToken);

    /// <inheritdoc cref="IMessagingClient.GetStatusAsync(StatusRequest, CancellationToken)"/>
    public static Task<OperationResult<StatusInfo>> GetStatusAsync<TConnector>(
        this IMessagingClient client,
        CancellationToken cancellationToken = default)
        where TConnector : class, IChannelConnector
        => client.GetStatusAsync(
            new StatusRequest(typeof(TConnector).Name)
            {
                ConnectorType = typeof(TConnector)
            },
            cancellationToken);

    /// <inheritdoc cref="IMessagingClient.GetStatusAsync(StatusRequest, CancellationToken)"/>
    public static Task<OperationResult<StatusInfo>> GetStatusAsync<TConnector>(
        this IMessagingClient client,
        ConnectionSettings settings,
        CancellationToken cancellationToken = default)
        where TConnector : class, IChannelConnector
        => client.GetStatusAsync(
            new StatusRequest(typeof(TConnector).Name)
            {
                ConnectionSettings = settings,
                ConnectorType = typeof(TConnector)
            },
            cancellationToken);

    // ── ReceiveMessageStatusAsync ────────────────────────────────────────

    /// <inheritdoc cref="IMessagingClient.ReceiveMessageStatusAsync(ReceiveStatusRequest, CancellationToken)"/>
    public static Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(
        this IMessagingClient client,
        string channelName, MessageSource source,
        CancellationToken cancellationToken = default)
        => client.ReceiveMessageStatusAsync(new ReceiveStatusRequest(channelName, source), cancellationToken);

    /// <inheritdoc cref="IMessagingClient.ReceiveMessageStatusAsync(ReceiveStatusRequest, CancellationToken)"/>
    public static Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(
        this IMessagingClient client,
        string channelName, ConnectionSettings settings, MessageSource source,
        CancellationToken cancellationToken = default)
        => client.ReceiveMessageStatusAsync(
            new ReceiveStatusRequest(channelName, source) { ConnectionSettings = settings },
            cancellationToken);

    /// <inheritdoc cref="IMessagingClient.ReceiveMessageStatusAsync(ReceiveStatusRequest, CancellationToken)"/>
    public static Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync<TConnector>(
        this IMessagingClient client,
        MessageSource source,
        CancellationToken cancellationToken = default)
        where TConnector : class, IChannelConnector
        => client.ReceiveMessageStatusAsync(
            new ReceiveStatusRequest(typeof(TConnector).Name, source)
            {
                ConnectorType = typeof(TConnector)
            },
            cancellationToken);

    /// <inheritdoc cref="IMessagingClient.ReceiveMessageStatusAsync(ReceiveStatusRequest, CancellationToken)"/>
    public static Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync<TConnector>(
        this IMessagingClient client,
        ConnectionSettings settings, MessageSource source,
        CancellationToken cancellationToken = default)
        where TConnector : class, IChannelConnector
        => client.ReceiveMessageStatusAsync(
            new ReceiveStatusRequest(typeof(TConnector).Name, source)
            {
                ConnectionSettings = settings,
                ConnectorType = typeof(TConnector)
            },
            cancellationToken);
}
