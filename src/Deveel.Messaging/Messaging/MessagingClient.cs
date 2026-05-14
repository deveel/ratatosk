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
    public class MessagingClient : IMessagingClient, IDisposable, IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MessagingClientOptions _options;
        private readonly ILogger _logger;
        private readonly IChannelConnectorResolver? _resolver;
        private readonly ConnectorTypeCatalog? _catalog;

        private readonly Dictionary<string, IChannelConnector> _connectors = new(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly List<IChannelConnector> _runtimeConnectors = new();
        private readonly object _runtimeLock = new();

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
        /// <param name="resolver">
        /// An optional <see cref="IChannelConnectorResolver"/> used to
        /// resolve pre-configured connectors by name. When <c>null</c>,
        /// connectors are resolved directly from the service provider.
        /// </param>
        /// <param name="catalog">
        /// An optional <see cref="ConnectorTypeCatalog"/> used to create
        /// connectors at runtime from <see cref="ConnectionSettings"/>.
        /// </param>
        public MessagingClient(
            IServiceProvider serviceProvider,
            MessagingClientOptions? options = null,
            ILogger<MessagingClient>? logger = null,
            IChannelConnectorResolver? resolver = null,
            ConnectorTypeCatalog? catalog = null)
        {
            _serviceProvider = serviceProvider;
            _options = options ?? new MessagingClientOptions();
            _logger = logger ?? NullLogger<MessagingClient>.Instance;
            _resolver = resolver;
            _catalog = catalog;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<SendResult>> SendAsync(string channelName, IMessage message, CancellationToken cancellationToken = default)
        {
            _logger.LogSendingMessage(channelName);

            var connector = await ResolveConnectorAsync(channelName, cancellationToken);
            if (connector == null)
                return OperationResult<SendResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");

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
                return OperationResult<ReceiveResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");

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
                return OperationResult<StatusInfo>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");

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
                return OperationResult<StatusUpdateResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");

            var result = await connector.ReceiveMessageStatusAsync(source, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageStatusReceived(channelName);
            else
                _logger.LogMessageStatusReceiveFailed(channelName, result.Error?.Message);

            return result;
        }

        // ── Type-parameterized standard overloads ────────────────────────────

        /// <inheritdoc/>
        public async Task<OperationResult<SendResult>> SendAsync<TConnector>(IMessage message, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            var connector = await ResolveConnectorAsync<TConnector>(cancellationToken);
            if (connector == null)
                return OperationResult<SendResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for type '{channelName}'.");

            var result = await connector.SendMessageAsync(message, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageSent(channelName);
            else
                _logger.LogMessageSendFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<ReceiveResult>> ReceiveAsync<TConnector>(MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            var connector = await ResolveConnectorAsync<TConnector>(cancellationToken);
            if (connector == null)
                return OperationResult<ReceiveResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for type '{channelName}'.");

            var result = await connector.ReceiveMessagesAsync(source, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageReceived(channelName);
            else
                _logger.LogMessageReceiveFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<StatusInfo>> GetStatusAsync<TConnector>(CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            var connector = await ResolveConnectorAsync<TConnector>(cancellationToken);
            if (connector == null)
                return OperationResult<StatusInfo>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for type '{channelName}'.");

            var result = await connector.GetStatusAsync(cancellationToken);
            if (result.IsSuccess())
                _logger.LogStatusRead(channelName);
            else
                _logger.LogStatusReadFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync<TConnector>(MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            var connector = await ResolveConnectorAsync<TConnector>(cancellationToken);
            if (connector == null)
                return OperationResult<StatusUpdateResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for type '{channelName}'.");

            var result = await connector.ReceiveMessageStatusAsync(source, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageStatusReceived(channelName);
            else
                _logger.LogMessageStatusReceiveFailed(channelName, result.Error?.Message);

            return result;
        }

        // ── Runtime overloads ────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<OperationResult<SendResult>> SendAsync(string channelName, ConnectionSettings settings, IMessage message, CancellationToken cancellationToken = default)
        {
            _logger.LogSendingMessage(channelName);

            var connector = await CreateRuntimeConnectorAsync(channelName, settings, cancellationToken);
            if (connector == null)
                return OperationResult<SendResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for channel '{channelName}'.");

            var result = await connector.SendMessageAsync(message, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageSent(channelName);
            else
                _logger.LogMessageSendFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<ReceiveResult>> ReceiveAsync(string channelName, ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default)
        {
            _logger.LogReceivingMessage(channelName);

            var connector = await CreateRuntimeConnectorAsync(channelName, settings, cancellationToken);
            if (connector == null)
                return OperationResult<ReceiveResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for channel '{channelName}'.");

            var result = await connector.ReceiveMessagesAsync(source, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageReceived(channelName);
            else
                _logger.LogMessageReceiveFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<StatusInfo>> GetStatusAsync(string channelName, ConnectionSettings settings, CancellationToken cancellationToken = default)
        {
            _logger.LogReadingStatus(channelName);

            var connector = await CreateRuntimeConnectorAsync(channelName, settings, cancellationToken);
            if (connector == null)
                return OperationResult<StatusInfo>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for channel '{channelName}'.");

            var result = await connector.GetStatusAsync(cancellationToken);
            if (result.IsSuccess())
                _logger.LogStatusRead(channelName);
            else
                _logger.LogStatusReadFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(string channelName, ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default)
        {
            _logger.LogReceivingMessageStatus(channelName);

            var connector = await CreateRuntimeConnectorAsync(channelName, settings, cancellationToken);
            if (connector == null)
                return OperationResult<StatusUpdateResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for channel '{channelName}'.");

            var result = await connector.ReceiveMessageStatusAsync(source, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageStatusReceived(channelName);
            else
                _logger.LogMessageStatusReceiveFailed(channelName, result.Error?.Message);

            return result;
        }

        // ── Type-parameterized runtime overloads ────────────────────────────

        /// <inheritdoc/>
        public async Task<OperationResult<SendResult>> SendAsync<TConnector>(ConnectionSettings settings, IMessage message, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            var connector = await CreateRuntimeConnectorAsync<TConnector>(settings, cancellationToken);
            if (connector == null)
                return OperationResult<SendResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for '{channelName}'.");

            var result = await connector.SendMessageAsync(message, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageSent(channelName);
            else
                _logger.LogMessageSendFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<ReceiveResult>> ReceiveAsync<TConnector>(ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            var connector = await CreateRuntimeConnectorAsync<TConnector>(settings, cancellationToken);
            if (connector == null)
                return OperationResult<ReceiveResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for '{channelName}'.");

            var result = await connector.ReceiveMessagesAsync(source, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageReceived(channelName);
            else
                _logger.LogMessageReceiveFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<StatusInfo>> GetStatusAsync<TConnector>(ConnectionSettings settings, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            var connector = await CreateRuntimeConnectorAsync<TConnector>(settings, cancellationToken);
            if (connector == null)
                return OperationResult<StatusInfo>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for '{channelName}'.");

            var result = await connector.GetStatusAsync(cancellationToken);
            if (result.IsSuccess())
                _logger.LogStatusRead(channelName);
            else
                _logger.LogStatusReadFailed(channelName, result.Error?.Message);

            return result;
        }

        /// <inheritdoc/>
        public async Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync<TConnector>(ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            var connector = await CreateRuntimeConnectorAsync<TConnector>(settings, cancellationToken);
            if (connector == null)
                return OperationResult<StatusUpdateResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for '{channelName}'.");

            var result = await connector.ReceiveMessageStatusAsync(source, cancellationToken);
            if (result.IsSuccess())
                _logger.LogMessageStatusReceived(channelName);
            else
                _logger.LogMessageStatusReceiveFailed(channelName, result.Error?.Message);

            return result;
        }

        // ── Runtime connector creation ──────────────────────────────────────

        private async Task<IChannelConnector?> CreateRuntimeConnectorAsync(string channelName, ConnectionSettings settings, CancellationToken cancellationToken)
        {
            if (_catalog == null || !_catalog.TryGetEntry(channelName, out var entry))
            {
                _logger.LogChannelNotFound(channelName);
                return null;
            }

            _logger.LogResolvingChannel(channelName);

            var schema = entry!.GetSchema(_serviceProvider);
            var connector = (IChannelConnector)ActivatorUtilities.CreateInstance(
                _serviceProvider, entry.ConnectorType, schema, settings);

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

            TrackRuntimeConnector(connector);
            return connector;
        }

        private async Task<IChannelConnector?> CreateRuntimeConnectorAsync<TConnector>(ConnectionSettings settings, CancellationToken cancellationToken)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            _logger.LogResolvingChannel(channelName);

            var connectorType = typeof(TConnector);
            var schema = ConnectorSchemaHelper.DiscoverConnectorSchema(_serviceProvider, connectorType);
            var connector = (IChannelConnector)ActivatorUtilities.CreateInstance(
                _serviceProvider, connectorType, schema, settings);

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

            TrackRuntimeConnector(connector);
            return connector;
        }

        private async Task<IChannelConnector?> ResolveConnectorAsync<TConnector>(CancellationToken cancellationToken)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            if (_connectors.TryGetValue(channelName, out var existing))
                return existing;

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_connectors.TryGetValue(channelName, out existing))
                    return existing;

                _logger.LogResolvingChannel(channelName);

                var connector = _serviceProvider.GetService<TConnector>();
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

        private void TrackRuntimeConnector(IChannelConnector connector)
        {
            lock (_runtimeLock)
                _runtimeConnectors.Add(connector);
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

                var connector = _resolver != null
                    ? await _resolver.ResolveAsync(channelName, cancellationToken)
                    : _serviceProvider.GetKeyedService<IChannelConnector>(channelName);

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

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var connector in _connectors.Values)
            {
                if (connector is IDisposable disposable)
                    disposable.Dispose();
            }

            lock (_runtimeLock)
            {
                foreach (var connector in _runtimeConnectors)
                {
                    if (connector is IDisposable disposable)
                        disposable.Dispose();
                }
                _runtimeConnectors.Clear();
            }

            _lock.Dispose();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            foreach (var connector in _connectors.Values)
            {
                await connector.ShutdownAsync(default).ConfigureAwait(false);

                if (connector is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }

            IChannelConnector[] runtimeSnapshot;
            lock (_runtimeLock)
            {
                runtimeSnapshot = _runtimeConnectors.ToArray();
                _runtimeConnectors.Clear();
            }

            foreach (var connector in runtimeSnapshot)
            {
                await connector.ShutdownAsync(default).ConfigureAwait(false);

                if (connector is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }

            _lock.Dispose();
        }
    }
}
