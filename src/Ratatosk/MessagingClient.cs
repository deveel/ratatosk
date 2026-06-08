using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using System.Diagnostics;

namespace Ratatosk
{
    /// <summary>
    /// Default implementation of <see cref="IMessagingClient"/> that resolves
    /// channel connectors from the dependency injection container and manages
    /// their initialization and lifecycle.
    /// </summary>
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

        /// <summary>
        /// Constructs a new <see cref="MessagingClient"/> instance.
        /// </summary>
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

        /// <inheritdoc/>
        public async Task<OperationResult<SendResult>> SendAsync(string channelName, IMessage message, CancellationToken cancellationToken = default)
        {
            _logger.LogSendingMessage(channelName);

            using var activity = _clientTelemetry.StartSendActivity(channelName, message.Id);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync(channelName, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordSendFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<SendResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");
            }

            await ResolveSenderAsync(message, connector.ConnectionSettings, cancellationToken);

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

        /// <inheritdoc/>
        public async Task<OperationResult<ReceiveResult>> ReceiveAsync(string channelName, MessageSource source, CancellationToken cancellationToken = default)
        {
            _logger.LogReceivingMessage(channelName);

            using var activity = _clientTelemetry.StartReceiveActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync(channelName, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<ReceiveResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");
            }

            try
            {
                var result = await connector.ReceiveMessagesAsync(source, cancellationToken);
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

        /// <inheritdoc/>
        public async Task<OperationResult<StatusInfo>> GetStatusAsync(string channelName, CancellationToken cancellationToken = default)
        {
            _logger.LogReadingStatus(channelName);

            using var activity = _clientTelemetry.StartStatusActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync(channelName, cancellationToken);
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

        /// <inheritdoc/>
        public async Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(string channelName, MessageSource source, CancellationToken cancellationToken = default)
        {
            _logger.LogReceivingMessageStatus(channelName);

            using var activity = _clientTelemetry.StartReceiveStatusActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync(channelName, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<StatusUpdateResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for channel '{channelName}'.");
            }

            try
            {
                var result = await connector.ReceiveMessageStatusAsync(source, cancellationToken);
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

        // ── Type-parameterized standard overloads ────────────────────────────

        /// <inheritdoc/>
        public async Task<OperationResult<SendResult>> SendAsync<TConnector>(IMessage message, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            using var activity = _clientTelemetry.StartSendActivity(channelName, message.Id);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync<TConnector>(cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordSendFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<SendResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for type '{channelName}'.");
            }

            await ResolveSenderAsync(message, connector.ConnectionSettings, cancellationToken);

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

        /// <inheritdoc/>
        public async Task<OperationResult<ReceiveResult>> ReceiveAsync<TConnector>(MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            using var activity = _clientTelemetry.StartReceiveActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync<TConnector>(cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<ReceiveResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for type '{channelName}'.");
            }

            try
            {
                var result = await connector.ReceiveMessagesAsync(source, cancellationToken);
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

        /// <inheritdoc/>
        public async Task<OperationResult<StatusInfo>> GetStatusAsync<TConnector>(CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            using var activity = _clientTelemetry.StartStatusActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync<TConnector>(cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<StatusInfo>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for type '{channelName}'.");
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

        /// <inheritdoc/>
        public async Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync<TConnector>(MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            using var activity = _clientTelemetry.StartReceiveStatusActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await ResolveConnectorAsync<TConnector>(cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<StatusUpdateResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector registered for type '{channelName}'.");
            }

            try
            {
                var result = await connector.ReceiveMessageStatusAsync(source, cancellationToken);
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

        // ── Runtime overloads ────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<OperationResult<SendResult>> SendAsync(string channelName, ConnectionSettings settings, IMessage message, CancellationToken cancellationToken = default)
        {
            _logger.LogSendingMessage(channelName);

            using var activity = _clientTelemetry.StartSendActivity(channelName, message.Id);
            var sw = Stopwatch.StartNew();

            var connector = await CreateRuntimeConnectorAsync(channelName, settings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordSendFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<SendResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for channel '{channelName}'.");
            }

            await ResolveSenderAsync(message, settings, cancellationToken);

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

        /// <inheritdoc/>
        public async Task<OperationResult<ReceiveResult>> ReceiveAsync(string channelName, ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default)
        {
            _logger.LogReceivingMessage(channelName);

            using var activity = _clientTelemetry.StartReceiveActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await CreateRuntimeConnectorAsync(channelName, settings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<ReceiveResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for channel '{channelName}'.");
            }

            try
            {
                var result = await connector.ReceiveMessagesAsync(source, cancellationToken);
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

        /// <inheritdoc/>
        public async Task<OperationResult<StatusInfo>> GetStatusAsync(string channelName, ConnectionSettings settings, CancellationToken cancellationToken = default)
        {
            _logger.LogReadingStatus(channelName);

            using var activity = _clientTelemetry.StartStatusActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await CreateRuntimeConnectorAsync(channelName, settings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<StatusInfo>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for channel '{channelName}'.");
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

        /// <inheritdoc/>
        public async Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(string channelName, ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default)
        {
            _logger.LogReceivingMessageStatus(channelName);

            using var activity = _clientTelemetry.StartReceiveStatusActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await CreateRuntimeConnectorAsync(channelName, settings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<StatusUpdateResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for channel '{channelName}'.");
            }

            try
            {
                var result = await connector.ReceiveMessageStatusAsync(source, cancellationToken);
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

        // ── Type-parameterized runtime overloads ────────────────────────────

        /// <inheritdoc/>
        public async Task<OperationResult<SendResult>> SendAsync<TConnector>(ConnectionSettings settings, IMessage message, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            using var activity = _clientTelemetry.StartSendActivity(channelName, message.Id);
            var sw = Stopwatch.StartNew();

            var connector = await CreateRuntimeConnectorAsync<TConnector>(settings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordSendFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<SendResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for '{channelName}'.");
            }

            await ResolveSenderAsync(message, settings, cancellationToken);

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

        /// <inheritdoc/>
        public async Task<OperationResult<ReceiveResult>> ReceiveAsync<TConnector>(ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            using var activity = _clientTelemetry.StartReceiveActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await CreateRuntimeConnectorAsync<TConnector>(settings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<ReceiveResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for '{channelName}'.");
            }

            try
            {
                var result = await connector.ReceiveMessagesAsync(source, cancellationToken);
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

        /// <inheritdoc/>
        public async Task<OperationResult<StatusInfo>> GetStatusAsync<TConnector>(ConnectionSettings settings, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            using var activity = _clientTelemetry.StartStatusActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await CreateRuntimeConnectorAsync<TConnector>(settings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<StatusInfo>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for '{channelName}'.");
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

        /// <inheritdoc/>
        public async Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync<TConnector>(ConnectionSettings settings, MessageSource source, CancellationToken cancellationToken = default)
            where TConnector : class, IChannelConnector
        {
            var channelName = typeof(TConnector).Name;

            using var activity = _clientTelemetry.StartReceiveStatusActivity(channelName);
            var sw = Stopwatch.StartNew();

            var connector = await CreateRuntimeConnectorAsync<TConnector>(settings, cancellationToken);
            if (connector == null)
            {
                sw.Stop();
                _clientTelemetry.RecordReceiveStatusFailure(sw.ElapsedMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, "Connector not found");
                return OperationResult<StatusUpdateResult>.Fail(MessagingErrorCodes.ConnectorNotFound, MessagingErrorCodes.ErrorDomain, $"No connector type registered for '{channelName}'.");
            }

            try
            {
                var result = await connector.ReceiveMessageStatusAsync(source, cancellationToken);
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

        // ── Sender resolution ────────────────────────────────────────────────

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
            _clientTelemetry.Dispose();
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
            _clientTelemetry.Dispose();
        }
    }
}
