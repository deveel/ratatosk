using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using System.Diagnostics;

namespace Ratatosk
{
    public class MessagingClient : IMessagingClient
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
        private readonly ClientTelemetry _clientTelemetry;

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
            _clientTelemetry = new ClientTelemetry(_options.Telemetry);
        }

        // ── Core methods ──────────────────────────────────────────────────────

        public async Task<OperationResult<SendResult>> SendAsync(SendRequest request, CancellationToken cancellationToken = default)
        {
            var channelName = request.ChannelName;
            var message = request.Message;

            _logger.LogSendingMessage(channelName);

            using var scope = BeginContextScope(request.Context);
            using var activity = _clientTelemetry.StartSendActivity(channelName, message.Id, request.Context);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync(request.ConnectorType, channelName, request.ConnectionSettings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordSendFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<SendResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");
            }

            await ResolveSenderAsync(message, connector.ConnectionSettings, cancellationToken);
            StampMessageContext(message, request.Context);

            try
            {
                var result = await connector.SendMessageAsync(message, cancellationToken);
                sw.Stop();

                if (result.IsSuccess())
                {
                    _clientTelemetry.RecordSendSuccess(sw.ElapsedMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger.LogMessageSent(channelName);
                }
                else
                {
                    _clientTelemetry.RecordSendFailure(sw.ElapsedMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Error, result.Error?.Message);
                    _logger.LogMessageSendFailed(channelName, result.Error?.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _clientTelemetry.RecordSendFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        public async Task<OperationResult<BatchSendResult>> SendBatchAsync(BatchSendRequest request, CancellationToken cancellationToken = default)
        {
            var channelName = request.ChannelName;
            var batch = request.Batch;

            _logger.LogSendingMessage(channelName);

            using var scope = BeginContextScope(request.Context);
            using var activity = _clientTelemetry.StartSendActivity(channelName, batch.Id, request.Context);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync(request.ConnectorType, channelName, request.ConnectionSettings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordSendFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<BatchSendResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");
            }

            try
            {
                var result = await connector.SendBatchAsync(batch, cancellationToken);
                sw.Stop();

                if (result.IsSuccess())
                {
                    _clientTelemetry.RecordSendSuccess(sw.ElapsedMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger.LogMessageSent(channelName);
                }
                else
                {
                    _clientTelemetry.RecordSendFailure(sw.ElapsedMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Error, result.Error?.Message);
                    _logger.LogMessageSendFailed(channelName, result.Error?.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _clientTelemetry.RecordSendFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        public async Task<OperationResult<ReceiveResult>> ReceiveAsync(ReceiveRequest request, CancellationToken cancellationToken = default)
        {
            var channelName = request.ChannelName;

            _logger.LogReceivingMessage(channelName);

            using var scope = BeginContextScope(request.Context);
            using var activity = _clientTelemetry.StartReceiveActivity(channelName, request.Context);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync(request.ConnectorType, channelName, request.ConnectionSettings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<ReceiveResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");
            }

            try
            {
                var result = await connector.ReceiveMessagesAsync(request.Source, cancellationToken);
                sw.Stop();

                if (result.IsSuccess())
                {
                    _clientTelemetry.RecordReceiveSuccess(sw.ElapsedMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger.LogMessageReceived(channelName);
                }
                else
                {
                    _clientTelemetry.RecordReceiveFailure(sw.ElapsedMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Error, result.Error?.Message);
                    _logger.LogMessageReceiveFailed(channelName, result.Error?.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        public async Task<OperationResult<StatusInfo>> GetStatusAsync(StatusRequest request, CancellationToken cancellationToken = default)
        {
            var channelName = request.ChannelName;

            _logger.LogReadingStatus(channelName);

            using var scope = BeginContextScope(request.Context);
            using var activity = _clientTelemetry.StartStatusActivity(channelName, request.Context);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync(request.ConnectorType, channelName, request.ConnectionSettings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<StatusInfo>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");
            }

            try
            {
                var result = await connector.GetStatusAsync(cancellationToken);
                sw.Stop();

                if (result.IsSuccess())
                {
                    _clientTelemetry.RecordStatusSuccess(sw.ElapsedMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger.LogStatusRead(channelName);
                }
                else
                {
                    _clientTelemetry.RecordStatusFailure(sw.ElapsedMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Error, result.Error?.Message);
                    _logger.LogStatusReadFailed(channelName, result.Error?.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _clientTelemetry.RecordStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        public async Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(ReceiveStatusRequest request, CancellationToken cancellationToken = default)
        {
            var channelName = request.ChannelName;

            _logger.LogReceivingMessageStatus(channelName);

            using var scope = BeginContextScope(request.Context);
            using var activity = _clientTelemetry.StartReceiveStatusActivity(channelName, request.Context);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync(request.ConnectorType, channelName, request.ConnectionSettings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<StatusUpdateResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");
            }

            try
            {
                var result = await connector.ReceiveMessageStatusAsync(request.Source, cancellationToken);
                sw.Stop();

                if (result.IsSuccess())
                {
                    _clientTelemetry.RecordReceiveStatusSuccess(sw.ElapsedMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger.LogMessageStatusReceived(channelName);
                }
                else
                {
                    _clientTelemetry.RecordReceiveStatusFailure(sw.ElapsedMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Error, result.Error?.Message);
                    _logger.LogMessageStatusReceiveFailed(channelName, result.Error?.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        // ── Connector resolution ──────────────────────────────────────────

        private async Task<IChannelConnector?> ResolveConnectorAsync(Type? connectorType, string channelName, ConnectionSettings? settings, CancellationToken cancellationToken)
        {
            if (connectorType != null)
            {
                if (settings != null)
                    return await CreateRuntimeConnectorAsync(connectorType, channelName, settings, cancellationToken);

                return await ResolveConnectorByTypeAsync(channelName, connectorType, cancellationToken);
            }

            if (settings != null)
                return await CreateRuntimeConnectorAsync(channelName, settings, cancellationToken);

            return await ResolveConnectorByNameAsync(channelName, cancellationToken);
        }

        private async Task<IChannelConnector?> ResolveConnectorByNameAsync(string channelName, CancellationToken cancellationToken)
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

        private async Task<IChannelConnector?> ResolveConnectorByTypeAsync(string channelName, Type connectorType, CancellationToken cancellationToken)
        {
            if (_connectors.TryGetValue(channelName, out var existing))
                return existing;

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_connectors.TryGetValue(channelName, out existing))
                    return existing;

                _logger.LogResolvingChannel(channelName);

                var connector = _serviceProvider.GetService(connectorType) as IChannelConnector;
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

        private async Task<IChannelConnector?> CreateRuntimeConnectorAsync(Type connectorType, string channelName, ConnectionSettings settings, CancellationToken cancellationToken)
        {
            _logger.LogResolvingChannel(channelName);

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

        // ── Sender resolution ────────────────────────────────────────────

        private async ValueTask ResolveSenderAsync(IMessage message, ConnectionSettings settings, CancellationToken cancellationToken)
        {
            var resolver = _serviceProvider.GetService<ISenderResolver>();
            if (resolver == null)
                return;

            var context = new SenderResolutionContext(message.Sender, settings);
            var resolved = await resolver.ResolveAsync(context, cancellationToken);

            if (resolved != null && message is Message msg)
            {
                msg.Sender = resolved;
            }
        }

        // ── Context enrichment ───────────────────────────────────────────

        private static void StampMessageContext(IMessage message, MessageContext? context)
        {
            if (context == null || context.Data.Count == 0)
                return;

            if (message is not Message msg)
                return;

            msg.Properties ??= new Dictionary<string, MessageProperty>(StringComparer.OrdinalIgnoreCase);

            foreach (var (key, value) in context.Data)
            {
                if (!string.IsNullOrWhiteSpace(key))
                    msg.Properties[key] = new MessageProperty(key, value);
            }
        }

        private IDisposable? BeginContextScope(MessageContext? context)
        {
            if (context == null || context.Data.Count == 0)
                return null;

            return _logger.BeginScope(context.Data.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value ?? "<null>"));
        }

        // ── Runtime connector tracking ───────────────────────────────────

        private void TrackRuntimeConnector(IChannelConnector connector)
        {
            lock (_runtimeLock)
                _runtimeConnectors.Add(connector);
        }

        // ── Dispose ──────────────────────────────────────────────────────

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
            _clientTelemetry.Dispose();
        }

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
            _clientTelemetry.Dispose();
        }
    }
}
