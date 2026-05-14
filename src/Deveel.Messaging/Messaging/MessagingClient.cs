using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Messaging
{
    /// <summary>
    /// Default implementation of <see cref="IMessagingClient"/> that resolves
    /// channel connectors from the dependency injection container and manages
    /// their initialization and lifecycle.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Connectors are resolved lazily on first use and cached for subsequent
    /// calls, ensuring each named channel is initialized at most once.
    /// Thread-safe resolution is guaranteed via a <see cref="SemaphoreSlim"/>
    /// guard.
    /// </para>
    /// <para>
    /// When <see cref="MessagingClientOptions.AutoInitialize"/> is <c>true</c>
    /// (default), the client automatically calls
    /// <see cref="IChannelConnector.InitializeAsync"/> on the connector before
    /// delegating the operation. If initialization fails, the error is logged
    /// and a failure result is returned.
    /// </para>
    /// </remarks>
    public class MessagingClient : IMessagingClient
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MessagingClientOptions _options;
        private readonly ILogger _logger;

        private readonly Dictionary<string, IChannelConnector> _connectors = new(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim _lock = new(1, 1);

        /// <summary>
        /// Constructs a new <see cref="MessagingClient"/> instance.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve channel connectors by name.
        /// </param>
        /// <param name="options">
        /// An optional <see cref="MessagingClientOptions"/> instance that
        /// controls auto-initialization and other client behavior. When
        /// <c>null</c>, a default options instance is used.
        /// </param>
        /// <param name="logger">
        /// An optional logger for diagnostic output. When <c>null</c>,
        /// logging is suppressed.
        /// </param>
        public MessagingClient(
            IServiceProvider serviceProvider,
            MessagingClientOptions? options = null,
            ILogger<MessagingClient>? logger = null)
        {
            _serviceProvider = serviceProvider;
            _options = options ?? new MessagingClientOptions();
            _logger = logger ?? NullLogger<MessagingClient>.Instance;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<SendResult>> SendAsync(string channelName, IMessage message, CancellationToken cancellationToken = default)
        {
            _logger.LogSendingMessage(channelName);

            var connector = await ResolveConnectorAsync(channelName, cancellationToken);
            if (connector == null)
                return OperationResult<SendResult>.Fail("CHANNEL_NOT_FOUND", channelName, $"No connector registered for channel '{channelName}'.");

            var result = await connector.SendMessageAsync(message, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageSent(channelName);
            else
                _logger.LogMessageSendFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<ReceiveResult>> ReceiveAsync(string channelName, MessageSource source, CancellationToken cancellationToken = default)
        {
            _logger.LogReceivingMessage(channelName);

            var connector = await ResolveConnectorAsync(channelName, cancellationToken);
            if (connector == null)
                return OperationResult<ReceiveResult>.Fail("CHANNEL_NOT_FOUND", channelName, $"No connector registered for channel '{channelName}'.");

            var result = await connector.ReceiveMessagesAsync(source, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageReceived(channelName);
            else
                _logger.LogMessageReceiveFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<StatusInfo>> GetStatusAsync(string channelName, CancellationToken cancellationToken = default)
        {
            _logger.LogReadingStatus(channelName);

            var connector = await ResolveConnectorAsync(channelName, cancellationToken);
            if (connector == null)
                return OperationResult<StatusInfo>.Fail("CHANNEL_NOT_FOUND", channelName, $"No connector registered for channel '{channelName}'.");

            var result = await connector.GetStatusAsync(cancellationToken);
            if (result.IsSuccess())
                _logger.LogStatusRead(channelName);
            else
                _logger.LogStatusReadFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(string channelName, MessageSource source, CancellationToken cancellationToken = default)
        {
            _logger.LogReceivingMessageStatus(channelName);

            var connector = await ResolveConnectorAsync(channelName, cancellationToken);
            if (connector == null)
                return OperationResult<StatusUpdateResult>.Fail("CHANNEL_NOT_FOUND", channelName, $"No connector registered for channel '{channelName}'.");

            var result = await connector.ReceiveMessageStatusAsync(source, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageStatusReceived(channelName);
            else
                _logger.LogMessageStatusReceiveFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <summary>
        /// Resolves a channel connector by name, with lazy initialization
        /// and thread-safe caching.
        /// </summary>
        /// <param name="channelName">
        /// The name of the channel to resolve.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// The resolved <see cref="IChannelConnector"/> instance, or
        /// <c>null</c> if no connector is registered for the given name
        /// or if auto-initialization failed.
        /// </returns>
        private async Task<IChannelConnector?> ResolveConnectorAsync(string channelName, CancellationToken cancellationToken)
        {
            if (_connectors.TryGetValue(channelName, out var existing))
                return existing;

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_connectors.TryGetValue(channelName, out existing))
                    return existing;

                _logger.LogResolvingChannel(channelName);

                var connector = _serviceProvider.GetKeyedService<IChannelConnector>(channelName);
                if (connector == null)
                {
                    _logger.LogChannelNotFound(channelName);
                    return null;
                }

                if (_options.AutoInitialize && connector.State == ConnectorState.Uninitialized)
                {
                    _logger.LogInitializingConnector(channelName);

                    var initResult = await connector.InitializeAsync(cancellationToken);
                    if (!initResult.IsSuccess())
                    {
                        _logger.LogConnectorInitializationFailed(channelName, initResult.Error?.Message);
                        return null;
                    }

                    _logger.LogConnectorInitialized(channelName);
                }

                _logger.LogChannelResolved(channelName);

                _connectors[channelName] = connector;
                return connector;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
