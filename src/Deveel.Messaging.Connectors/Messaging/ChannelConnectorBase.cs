//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Deveel.Messaging
{
    public abstract class ChannelConnectorBase : IChannelConnector
    {
        private ConnectorState _state = ConnectorState.Uninitialized;
        private readonly object _stateLock = new object();
        private AuthenticationCredential? _authenticationCredential;
        private readonly IAuthenticationManager _authenticationManager;
        private bool _autoAuthenticationAttempted;
        private readonly IMessageIdGenerator _idGenerator;

        protected ChannelConnectorBase(
            IChannelSchema schema,
            ConnectionSettings? connectionSettings = null,
            ILogger? logger = null,
            IAuthenticationManager? authenticationManager = null,
            IMessageIdGenerator? idGenerator = null)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            ConnectionSettings = connectionSettings ?? new ConnectionSettings();
            Logger = logger ?? NullLogger.Instance;
            _authenticationManager = authenticationManager ?? new AuthenticationManager(logger: NullLogger<AuthenticationManager>.Instance);
            _idGenerator = idGenerator ?? new DefaultMessageIdGenerator();
        }

        public IChannelSchema Schema { get; }

        public ConnectionSettings ConnectionSettings { get; }

        protected ILogger Logger { get; }

        protected IAuthenticationManager AuthenticationManager => _authenticationManager;

        protected AuthenticationCredential? AuthenticationCredential => _authenticationCredential;

        protected IMessageIdGenerator IdGenerator => _idGenerator;

        public ConnectorState State
        {
            get
            {
                lock (_stateLock)
                {
                    return _state;
                }
            }
        }

        protected void SetState(ConnectorState newState)
        {
            lock (_stateLock)
            {
                _state = newState;
            }
        }

        protected void ValidateCapability(ChannelCapability capability)
        {
            if (!Schema.Capabilities.HasFlag(capability))
            {
                throw new MessagingException(MessagingErrorCodes.InvalidConfiguration, MessagingErrorCodes.ErrorDomain, $"The connector does not support the '{capability}' capability.");
            }
        }

        protected async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (State == ConnectorState.Uninitialized)
                await InitializeAsync(cancellationToken);

            ValidateOperationalState();
        }

        protected void ValidateOperationalState()
        {
            var currentState = State;
            if (currentState == ConnectorState.Uninitialized ||
                currentState == ConnectorState.Initializing ||
                currentState == ConnectorState.ShuttingDown ||
                currentState == ConnectorState.Shutdown)
            {
                throw new MessagingException(MessagingErrorCodes.MessagingError, MessagingErrorCodes.ErrorDomain, $"The connector is not in an operational state. Current state: {currentState}");
            }
        }

        protected virtual IDisposable? BeginConnectorLoggerScope()
        {
            return Logger.BeginScope(
                "[{ChannelType} v{ChannelVersion}]",
                    Schema.ChannelType,
                    Schema.Version);
        }

        protected virtual IDisposable? BeginMessageLoggerScope(IMessage message)
        {
            return Logger.BeginScope("[MessageId:{MessageId}]", message.Id ?? "(unknown)");
        }

        public async ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken cancellationToken)
        {
            using var scope = BeginConnectorLoggerScope();

            if (State != ConnectorState.Uninitialized)
            {
                return OperationResult<bool>.Fail(ConnectorErrorCodes.AlreadyInitialized,
                    MessagingErrorCodes.ErrorDomain,
                    "The connector has already been initialized.");
            }

            SetState(ConnectorState.Initializing);

            try
            {
                Logger.LogInitializingConnector();

                // Auto-authenticate if the schema has authentication configurations
                // and authentication hasn't been explicitly handled by the connector
                if (Schema.AuthenticationConfigurations.Any(c => c.Scheme != AuthenticationScheme.None) &&
                    !_autoAuthenticationAttempted)
                {
                    _autoAuthenticationAttempted = true;
                    var authResult = await AuthenticateAsync(cancellationToken);
                    if (!authResult.IsSuccess())
                    {
                        // Non-fatal: connector can still initialize and handle auth later
                        Logger.LogAutoAuthenticationFailed(authResult.Error?.Message);
                    }
                }

                await InitializeConnectorAsync(cancellationToken);

                Logger.LogConnectorInitialized();
                SetState(ConnectorState.Ready);

                return true;
            }
            catch (MessagingException ex)
            {
                Logger.LogConnectorInitializationFailed(ex);

                SetState(ConnectorState.Error);
                return OperationResult<bool>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogConnectorInitializationFailed(ex);

                SetState(ConnectorState.Error);
                return OperationResult<bool>.Fail(
                    ConnectorErrorCodes.InitializationError,
                    MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        protected abstract ValueTask InitializeConnectorAsync(CancellationToken cancellationToken);

        protected async Task<OperationResult<bool>> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            using var scope = BeginConnectorLoggerScope();

            try
            {
                Logger.LogStartingAuthentication();

                var authConfig =
                    Schema.AuthenticationConfigurations.FirstOrDefault(config =>
                        config.IsSatisfiedBy(ConnectionSettings));

                if (authConfig == null)
                {
                    Logger.LogNoAuthenticationConfigurationFound();
                    return OperationResult<bool>.Fail(
                        ConnectorErrorCodes.AuthenticationFailed,
                        MessagingErrorCodes.ErrorDomain,
                        "No suitable authentication configuration found for the provided connection settings");
                }

                Logger.LogUsingAuthenticationConfiguration(authConfig.Scheme);

                var authResult =
                    await _authenticationManager.AuthenticateAsync(ConnectionSettings, authConfig, cancellationToken);

                if (authResult.IsSuccessful && authResult.Credential != null)
                {
                    _authenticationCredential = authResult.Credential;
                    Logger.LogAuthenticationSuccessful(authConfig.Scheme);
                    return true;
                }
                else
                {
                    Logger.LogAuthenticationFailed(authConfig.Scheme);
                    return OperationResult<bool>.Fail(
                        ConnectorErrorCodes.AuthenticationFailed,
                        MessagingErrorCodes.ErrorDomain,
                        authResult.ErrorMessage ?? "Authentication failed");
                }
            }
            catch (MessagingException ex)
            {
                Logger.LogAuthenticationException(ex);
                return OperationResult<bool>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogAuthenticationException(ex);
                return OperationResult<bool>.Fail(
                    ConnectorErrorCodes.AuthenticationFailed,
                    MessagingErrorCodes.ErrorDomain,
                    $"Authentication error: {ex.Message}");
            }
        }

        protected async Task<OperationResult<bool>> RefreshAuthenticationAsync(CancellationToken cancellationToken = default)
        {
            using var scope = BeginConnectorLoggerScope();

            if (_authenticationCredential == null)
            {
                Logger.LogNoCredentialToRefresh();
                return await AuthenticateAsync(cancellationToken);
            }

            try
            {
                Logger.LogRefreshingAuthenticationCredential();

                var authConfig = Schema.AuthenticationConfigurations.FirstOrDefault(config =>
                    config.Scheme == _authenticationCredential.Scheme);

                if (authConfig == null)
                {
                    Logger.LogAuthenticationConfigurationNotFoundForType(_authenticationCredential.Scheme);
                    return await AuthenticateAsync(cancellationToken);
                }

                var authResult = await _authenticationManager.AuthenticateAsync(ConnectionSettings, authConfig, cancellationToken);

                if (authResult.IsSuccessful && authResult.Credential != null)
                {
                    _authenticationCredential = authResult.Credential;
                    Logger.LogAuthenticationCredentialRefreshed();
                    return true;
                }
                else
                {
                    Logger.LogAuthenticationRefreshFailed();
                    return OperationResult<bool>.Fail(
                        ConnectorErrorCodes.AuthenticationFailed,
                        MessagingErrorCodes.ErrorDomain,
                        authResult.ErrorMessage ?? "Authentication refresh failed");
                }
            }
            catch (MessagingException ex)
            {
                Logger.LogAuthenticationRefreshException(ex);
                return OperationResult<bool>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogAuthenticationRefreshException(ex);
                return OperationResult<bool>.Fail(
                    ConnectorErrorCodes.AuthenticationFailed,
                    MessagingErrorCodes.ErrorDomain,
                    $"Authentication refresh error: {ex.Message}");
            }
        }

        protected virtual string? GetAuthenticationHeader()
        {
            if (_authenticationCredential == null)
                return null;

            if (_authenticationCredential.Scheme == AuthenticationScheme.Bearer ||
                _authenticationCredential.Scheme == AuthenticationScheme.OAuthClientCredentials)
            {
                var tokenType = _authenticationCredential.Properties.TryGetValue("TokenType", out var type) ? type?.ToString() : "Bearer";
                return $"{tokenType} {_authenticationCredential.Value}";
            }

            if (_authenticationCredential.Scheme == AuthenticationScheme.Basic)
            {
                return $"Basic {_authenticationCredential.Value}";
            }

            return null;
        }

        protected virtual string? GetApiKey()
        {
            return _authenticationCredential?.Scheme == AuthenticationScheme.ApiKey
                ? _authenticationCredential.Value
                : null;
        }

        protected virtual bool IsAnonymousConnector()
        {
            return Schema.AuthenticationConfigurations.Count == 0 ||
                   Schema.AuthenticationConfigurations.All(c => c.Scheme == AuthenticationScheme.None);
        }

        public async ValueTask<OperationResult<bool>> TestConnectionAsync(CancellationToken cancellationToken)
        {
            await EnsureInitializedAsync(cancellationToken);

            using var scope = BeginConnectorLoggerScope();

            try
            {
                Logger.LogTestingConnection();

                await TestConnectorConnectionAsync(cancellationToken);

                Logger.LogConnectionTestSuccessful();

                return true;
            }
            catch (MessagingException ex)
            {
                Logger.LogConnectionTestFailed(ex);

                return OperationResult<bool>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogConnectionTestFailed(ex);

                return OperationResult<bool>.Fail(
                    ConnectorErrorCodes.ConnectionTestError,
                    MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        protected abstract ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken);

        public async ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(message);
            ValidateCapability(ChannelCapability.SendMessages);
            await EnsureInitializedAsync(cancellationToken);

            _idGenerator.EnsureMessageId(message);

            using var scope = BeginConnectorLoggerScope();
            using var messageScope = BeginMessageLoggerScope(message);

            try
            {
                Logger.LogValidatingMessage(message.Id!);

                var validationErrors = new List<ValidationResult>();
                await foreach (var validationResult in ValidateMessageAsync(message, cancellationToken))
                {
                    if (validationResult != ValidationResult.Success)
                    {
                        validationErrors.Add(validationResult);
                    }
                }

                if (validationErrors.Count > 0)
                {
                    Logger.LogMessageValidationFailed(message.Id!, validationErrors.Count);

                    return OperationResult<SendResult>.ValidationFailed(
                        ConnectorErrorCodes.MessageValidationFailed,
                        MessagingErrorCodes.ErrorDomain,
                        validationErrors);
                }

                Logger.LogMessageValidationPassed(message.Id!);

                Logger.LogSendingMessage(message.Id!);

                var result = await SendMessageCoreAsync(message, cancellationToken);

                Logger.LogMessageSent(result.RemoteMessageId);

                return result;
            }
            catch (MessagingException ex)
            {
                Logger.LogMessageSendFailed(message.Id!, ex);
                return OperationResult<SendResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogMessageSendFailed(message.Id!, ex);
                return OperationResult<SendResult>.Fail(ConnectorErrorCodes.SendMessageError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        protected abstract Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken);

        public virtual async ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(batch);
            ValidateCapability(ChannelCapability.BulkMessaging);
            await EnsureInitializedAsync(cancellationToken);

            _idGenerator.EnsureBatchId(batch);

            using var scope = BeginConnectorLoggerScope();

            try
            {
                var allValidationErrors = new List<ValidationResult>();
                var messageValidationResults = new Dictionary<string, List<ValidationResult>>();

                foreach (var message in batch.Messages)
                {
                    _idGenerator.EnsureMessageId(message);

                    var messageErrors = new List<ValidationResult>();
                    await foreach (var validationResult in ValidateMessageAsync(message, cancellationToken))
                    {
                        if (validationResult != ValidationResult.Success)
                        {
                            messageErrors.Add(validationResult);
                            allValidationErrors.Add(validationResult);
                        }
                    }

                    if (messageErrors.Count > 0)
                    {
                        messageValidationResults[message.Id!] = messageErrors;
                    }
                }

                if (allValidationErrors.Count > 0)
                {
                    Logger.LogBatchValidationFailed(batch.Messages.Count());

                    return OperationResult<BatchSendResult>.ValidationFailed(
                        ConnectorErrorCodes.BatchValidationFailed,
                        MessagingErrorCodes.ErrorDomain,
                        allValidationErrors);
                }

                Logger.LogSendingBatch(batch.Messages.Count());

                var result = await SendBatchCoreAsync(batch, cancellationToken);

                Logger.LogBatchSent(batch.Messages.Count());

                return result;
            }
            catch (MessagingException ex)
            {
                Logger.LogBatchSendFailed(batch.Messages.Count(), ex);
                return OperationResult<BatchSendResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogBatchSendFailed(batch.Messages.Count(), ex);
                return OperationResult<BatchSendResult>.Fail(ConnectorErrorCodes.SendBatchError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        protected virtual Task<BatchSendResult> SendBatchCoreAsync(IMessageBatch batch, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Batch sending is not supported by this connector.");
        }

        public async ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken cancellationToken)
        {
            using var scope = BeginConnectorLoggerScope();

            try
            {
                Logger.LogReadingStatus();

                var result = await GetConnectorStatusAsync(cancellationToken);

                Logger.LogStatusRead();

                return result;
            }
            catch (MessagingException ex)
            {
                Logger.LogStatusReadFailed(ex);
                return OperationResult<StatusInfo>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogStatusReadFailed(ex);
                return OperationResult<StatusInfo>.Fail(ConnectorErrorCodes.GetStatusError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        protected abstract Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken);

        public async ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
            ValidateCapability(ChannelCapability.MessageStatusQuery);
            await EnsureInitializedAsync(cancellationToken);

            using var scope = BeginConnectorLoggerScope();

            try
            {
                Logger.LogReadingMessageStatus(messageId);

                var result = await GetMessageStatusCoreAsync(messageId, cancellationToken);

                Logger.LogMessageStatusRead(messageId);

                return result;
            }
            catch (MessagingException ex)
            {
                Logger.LogMessageStatusReadFailed(messageId, ex);
                return OperationResult<StatusUpdatesResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogMessageStatusReadFailed(messageId, ex);
                return OperationResult<StatusUpdatesResult>.Fail(ConnectorErrorCodes.GetMessageStatusError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        protected virtual Task<StatusUpdatesResult> GetMessageStatusCoreAsync(string messageId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Message status querying is not supported by this connector.");
        }

        public virtual async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(message);

            await foreach (var result in ValidateMessageCoreAsync(message, cancellationToken))
            {
                yield return result;
            }
        }

        protected virtual async IAsyncEnumerable<ValidationResult> ValidateMessageCoreAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var validationResults = Schema.ValidateMessage(message);
            var hasValidationErrors = false;

            foreach (var validationResult in validationResults)
            {
                hasValidationErrors = true;
                yield return validationResult;
            }

            if (!hasValidationErrors)
            {
                yield return ValidationResult.Success!;
            }

            await Task.CompletedTask;
        }

        protected virtual string? GetEndpointType(IEndpoint endpoint)
        {
            return endpoint.Type switch
            {
                EndpointType.EmailAddress => "email",
                EndpointType.PhoneNumber => "phone",
                EndpointType.Url => "url",
                EndpointType.UserId => "user-id",
                EndpointType.ApplicationId => "app-id",
                EndpointType.Id => "endpoint-id",
                EndpointType.DeviceId => "device-id",
                EndpointType.Label => "label",
                EndpointType.Topic => "topic",
                EndpointType.Any => "*",
                _ => null
            };
        }

        protected virtual bool IsEndpointTypeSupported(EndpointType endpointType, bool asSender = false, bool asReceiver = false)
        {
            return Schema.Endpoints.Any(e =>
                (e.Type == EndpointType.Any || e.Type == endpointType) &&
                (!asSender || e.CanSend) &&
                (!asReceiver || e.CanReceive));
        }

        public async ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken cancellationToken)
        {
            ValidateCapability(ChannelCapability.HandleMessageState);
            await EnsureInitializedAsync(cancellationToken);

            using var scope = BeginConnectorLoggerScope();

            try
            {
                Logger.LogReceivingMessageStatus();

                var result = await ReceiveMessageStatusCoreAsync(source, cancellationToken);

                Logger.LogMessageStatusReceived(result.MessageId);

                return result;
            }
            catch (MessagingException ex)
            {
                Logger.LogMessageStatusReceiveFailed(ex);
                return OperationResult<StatusUpdateResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogMessageStatusReceiveFailed(ex);
                return OperationResult<StatusUpdateResult>.Fail(ConnectorErrorCodes.ReceiveStatusError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        protected virtual Task<StatusUpdateResult> ReceiveMessageStatusCoreAsync(MessageSource source, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Status receiving is not supported by this connector.");
        }

        public async ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken cancellationToken)
        {
            ValidateCapability(ChannelCapability.ReceiveMessages);
            await EnsureInitializedAsync(cancellationToken);

            using var scope = BeginConnectorLoggerScope();

            try
            {
                Logger.LogReceivingMessage();

                var result = await ReceiveMessagesCoreAsync(source, cancellationToken);

                Logger.LogMessageReceived();

                return result;
            }
            catch (MessagingException ex)
            {
                Logger.LogMessageReceiveFailed(ex);
                return OperationResult<ReceiveResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogMessageReceiveFailed(ex);
                return OperationResult<ReceiveResult>.Fail(
                    ConnectorErrorCodes.ReceiveMessagesError,
                    MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        protected virtual Task<ReceiveResult> ReceiveMessagesCoreAsync(MessageSource source, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Message receiving is not supported by this connector.");
        }

        public virtual async ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken cancellationToken)
        {
            ValidateCapability(ChannelCapability.HealthCheck);

            using var scope = BeginConnectorLoggerScope();

            try
            {
                Logger.LogCheckingHealth();

                var result = await GetConnectorHealthAsync(cancellationToken);

                Logger.LogHealthCheckSuccessful();

                return result;
            }
            catch (MessagingException ex)
            {
                Logger.LogHealthCheckFailed(ex);
                return OperationResult<ConnectorHealth>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogHealthCheckFailed(ex);
                return OperationResult<ConnectorHealth>.Fail(ConnectorErrorCodes.GetHealthError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        protected virtual Task<ConnectorHealth> GetConnectorHealthAsync(CancellationToken cancellationToken)
        {
            var health = new ConnectorHealth
            {
                State = State,
                IsHealthy = State == ConnectorState.Ready,
                LastHealthCheck = DateTime.UtcNow,
                Uptime = TimeSpan.Zero
            };

            if (!health.IsHealthy)
            {
                health.Issues.Add($"Connector is in {State} state");
            }

            return Task.FromResult(health);
        }

        public async ValueTask ShutdownAsync(CancellationToken cancellationToken)
        {
            using var scope = BeginConnectorLoggerScope();

            if (State == ConnectorState.Shutdown || State == ConnectorState.ShuttingDown)
            {
                return;
            }

            SetState(ConnectorState.ShuttingDown);

            try
            {
                await ShutdownConnectorAsync(cancellationToken);
            }
            finally
            {
                SetState(ConnectorState.Shutdown);
            }
        }

        protected virtual Task ShutdownConnectorAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
