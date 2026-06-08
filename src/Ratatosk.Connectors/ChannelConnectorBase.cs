//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Polly;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Ratatosk
{
    /// <summary>
    /// Provides a base implementation for a channel connector that handles
    /// authentication, initialization, message sending, receiving, and
    /// health monitoring.
    /// </summary>
    public abstract class ChannelConnectorBase : IChannelConnector
    {
        private readonly IConnectorStateManager _stateManager;
        private AuthenticationCredential? _authenticationCredential;
        private readonly IAuthenticationManager _authenticationManager;
        private bool _autoAuthenticationAttempted;
        private readonly IMessageIdGenerator _idGenerator;
        private readonly RetryPolicyOptions? _retryPolicy;
        private RetryPolicyOptions? _effectiveRetryPolicy;
        private readonly Lazy<ResiliencePipeline<SendResult>?> _sendPipeline;
        private readonly ConnectorTelemetry _telemetry;

        /// <summary>
        /// Constructs the connector base with the given schema and optional settings.
        /// </summary>
        protected ChannelConnectorBase(
            IChannelSchema schema,
            ConnectionSettings? connectionSettings = null,
            ILogger? logger = null,
            IAuthenticationManager? authenticationManager = null,
            IMessageIdGenerator? idGenerator = null,
            IConnectorStateManager? stateManager = null)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            ConnectionSettings = connectionSettings ?? new ConnectionSettings();
            Logger = logger ?? NullLogger.Instance;
            _authenticationManager = authenticationManager ?? new AuthenticationManager(logger: NullLogger<AuthenticationManager>.Instance);
            _idGenerator = idGenerator ?? new DefaultMessageIdGenerator();
            _stateManager = stateManager ?? new ConnectorStateManager();

            _retryPolicy = ReadRetryPolicyFromSettings();
            _sendPipeline = new Lazy<ResiliencePipeline<SendResult>?>(() =>
            {
                var options = _retryPolicy ?? GetDefaultRetryPolicy();
                _effectiveRetryPolicy = options;
                return ResiliencePipelineFactory.BuildPipeline<SendResult>(options);
            });

            var telemetryOptions = ReadTelemetryFromSettings();
            _telemetry = new ConnectorTelemetry(Schema.ChannelType, ConnectorName, telemetryOptions);
        }

        /// <summary>
        /// Gets the channel schema that defines the capabilities and configuration of the connector.
        /// </summary>
        public IChannelSchema Schema { get; }

        /// <summary>
        /// Gets the connection settings used by the connector.
        /// </summary>
        public ConnectionSettings ConnectionSettings { get; }

        /// <summary>
        /// Gets the logger used to log diagnostic information.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the state manager that controls connector lifecycle state transitions.
        /// </summary>
        protected IConnectorStateManager StateManager => _stateManager;

        /// <summary>
        /// Gets the authentication manager used to handle authentication operations.
        /// </summary>
        protected IAuthenticationManager AuthenticationManager => _authenticationManager;

        /// <summary>
        /// Gets the current authentication credential, if authentication has been performed.
        /// </summary>
        protected AuthenticationCredential? AuthenticationCredential => _authenticationCredential;

        /// <summary>
        /// Gets the message identifier generator used to generate unique message identifiers.
        /// </summary>
        protected IMessageIdGenerator IdGenerator => _idGenerator;

        /// <summary>
        /// Gets the logical name of this connector instance.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <see cref="IChannelSchema.ChannelType"/>.
        /// Override to provide a scoped name when multiple connectors share the same
        /// channel type (e.g. <c>sms-twilio</c> vs <c>sms-vonage</c>).
        /// </remarks>
        public virtual string ConnectorName => Schema.ChannelType;

        /// <summary>
        /// Gets a value indicating whether this connector instance can be safely reused
        /// across multiple concurrent callers from the connector pool.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <c>true</c> (the default), <see cref="ChannelConnectorFactory{TConnector}"/>
        /// will pool and return the same instance for identical
        /// <see cref="ConnectionSettings"/> + <see cref="IChannelSchema"/> combinations.
        /// Pooled connectors <strong>must</strong> be fully re-entrant:
        /// </para>
        /// <list type="bullet">
        ///   <item>No mutable instance state that is not protected by a lock or is inherently thread-safe.</item>
        ///   <item>No per-call resources (e.g. open streams) stored as instance fields.</item>
        ///   <item>All public and protected methods safe to call from multiple threads simultaneously.</item>
        /// </list>
        /// <para>
        /// Override and return <c>false</c> if your connector holds per-call state or wraps
        /// a non-thread-safe third-party client. The factory will then create a fresh instance
        /// on every call instead of pooling.
        /// </para>
        /// </remarks>
        public virtual bool IsReusable => true;

        /// <summary>
        /// Reads the retry policy options from the connection settings, or <c>null</c>
        /// if no retry policy is configured in the settings.
        /// </summary>
        private RetryPolicyOptions? ReadRetryPolicyFromSettings()
        {
            if (!ConnectionSettings.Parameters.TryGetValue(RetrySettingsKeys.MaxAttempts, out var maxRaw))
                return null;

            var options = new RetryPolicyOptions
            {
                MaxRetryAttempts = Convert.ToInt32(maxRaw)
            };

            if (ConnectionSettings.Parameters.TryGetValue(RetrySettingsKeys.BackoffType, out var backoffRaw) && backoffRaw is string backoffStr)
            {
                if (Enum.TryParse<RetryBackoffType>(backoffStr, out var backoffType))
                    options.BackoffType = backoffType;
            }

            if (ConnectionSettings.Parameters.TryGetValue(RetrySettingsKeys.BaseDelay, out var delayRaw) && delayRaw is string delayStr)
            {
                if (TimeSpan.TryParse(delayStr, out var baseDelay))
                    options.BaseDelay = baseDelay;
            }

            if (ConnectionSettings.Parameters.TryGetValue(RetrySettingsKeys.UseJitter, out var jitterRaw))
                options.UseJitter = Convert.ToBoolean(jitterRaw);

            if (ConnectionSettings.Parameters.TryGetValue(RetrySettingsKeys.RetryableErrorCodes, out var codesRaw) && codesRaw is string codesStr && !string.IsNullOrWhiteSpace(codesStr))
            {
                foreach (var code in codesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    options.RetryableErrorCodes.Add(code);
            }

            if (ConnectionSettings.Parameters.TryGetValue(RetrySettingsKeys.EnableCircuitBreaker, out var cbRaw))
                options.EnableCircuitBreaker = Convert.ToBoolean(cbRaw);

            if (options.EnableCircuitBreaker)
            {
                if (ConnectionSettings.Parameters.TryGetValue(RetrySettingsKeys.CircuitBreakerFailureRatio, out var ratioRaw))
                    options.CircuitBreakerFailureRatio = Convert.ToDouble(ratioRaw);

                if (ConnectionSettings.Parameters.TryGetValue(RetrySettingsKeys.CircuitBreakerSamplingDuration, out var sampleRaw) && sampleRaw is string sampleStr)
                {
                    if (TimeSpan.TryParse(sampleStr, out var samplingDuration))
                        options.CircuitBreakerSamplingDuration = samplingDuration;
                }

                if (ConnectionSettings.Parameters.TryGetValue(RetrySettingsKeys.CircuitBreakerMinimumThroughput, out var throughputRaw))
                    options.CircuitBreakerMinimumThroughput = Convert.ToInt32(throughputRaw);

                if (ConnectionSettings.Parameters.TryGetValue(RetrySettingsKeys.CircuitBreakerBreakDuration, out var breakRaw) && breakRaw is string breakStr)
                {
                    if (TimeSpan.TryParse(breakStr, out var breakDuration))
                        options.CircuitBreakerBreakDuration = breakDuration;
                }
            }

            return options;
        }

        protected virtual RetryPolicyOptions? GetDefaultRetryPolicy() => null;

        private TelemetryOptions ReadTelemetryFromSettings()
        {
            var options = new TelemetryOptions();

            if (ConnectionSettings.Parameters.TryGetValue(TelemetrySettingsKeys.EnableTracing, out var tracingRaw))
                options.EnableTracing = Convert.ToBoolean(tracingRaw);

            if (ConnectionSettings.Parameters.TryGetValue(TelemetrySettingsKeys.EnableMetrics, out var metricsRaw))
                options.EnableMetrics = Convert.ToBoolean(metricsRaw);

            if (ConnectionSettings.Parameters.TryGetValue(TelemetrySettingsKeys.EnablePayloadSizeMetrics, out var payloadRaw))
                options.EnablePayloadSizeMetrics = Convert.ToBoolean(payloadRaw);

            return options;
        }

        private ResiliencePipeline<T>? BuildPipeline<T>()
        {
            if (typeof(T) == typeof(SendResult))
                return (ResiliencePipeline<T>?)(object?)_sendPipeline.Value;

            var options = _retryPolicy ?? GetDefaultRetryPolicy();
            return ResiliencePipelineFactory.BuildPipeline<T>(options);
        }

        /// <summary>
        /// Gets the current state of the connector.
        /// </summary>
        public ConnectorState State => _stateManager.Current;

        /// <summary>
        /// Transitions the connector to <paramref name="newState"/> in a thread-safe manner.
        /// </summary>
        /// <param name="newState">The new state to set for the connector.</param>
        protected void SetState(ConnectorState newState)
        {
            var oldState = State;
            _stateManager.TransitionTo(newState);
            _telemetry.RecordStateChange(oldState.ToString(), newState.ToString());
        }

        /// <summary>
        /// Validates that the connector supports the given capability.
        /// </summary>
        /// <param name="capability">The capability to validate.</param>
        /// <exception cref="MessagingException">
        /// Thrown if the connector does not support the specified capability.
        /// </exception>
        protected void ValidateCapability(ChannelCapability capability)
        {
            if (!Schema.Capabilities.HasFlag(capability))
            {
                throw new MessagingException(MessagingErrorCodes.InvalidConfiguration, MessagingErrorCodes.ErrorDomain, $"The connector does not support the '{capability}' capability.");
            }
        }

        /// <summary>
        /// Ensures the connector is initialized before performing any operation.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        protected async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (State == ConnectorState.Uninitialized)
                await InitializeAsync(cancellationToken);

            ValidateOperationalState();
        }

        /// <summary>
        /// Validates that the connector is in an operational state.
        /// </summary>
        /// <exception cref="MessagingException">
        /// Thrown if the connector is not in an operational state.
        /// </exception>
        protected void ValidateOperationalState() => _stateManager.EnsureOperational();

        /// <summary>
        /// Begins a logging scope that includes the channel type and version information.
        /// </summary>
        /// <returns>
        /// An <see cref="IDisposable"/> instance that ends the logging scope when disposed.
        /// </returns>
        protected virtual IDisposable? BeginConnectorLoggerScope()
        {
            return Logger.BeginScope(
                "[{ChannelType} v{ChannelVersion}]",
                    Schema.ChannelType,
                    Schema.Version);
        }

        /// <summary>
        /// Begins a logging scope that includes the message identifier.
        /// </summary>
        /// <param name="message">The message to create the logging scope for.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> instance that ends the logging scope when disposed.
        /// </returns>
        protected virtual IDisposable? BeginMessageLoggerScope(IMessage message)
        {
            return Logger.BeginScope("[MessageId:{MessageId}]", message.Id ?? "(unknown)");
        }

        /// <summary>
        /// Initializes the connector and attempts auto-authentication if required by the schema.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> indicating whether the initialization was successful.
        /// </returns>
        public async ValueTask<OperationResult<bool>> InitializeAsync(CancellationToken cancellationToken)
        {
            using var scope = BeginConnectorLoggerScope();
            using var activity = _telemetry.StartInitializeActivity();

            if (State != ConnectorState.Uninitialized)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Already initialized");
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

                activity?.SetStatus(ActivityStatusCode.Ok);

                return true;
            }
            catch (MessagingException ex)
            {
                Logger.LogConnectorInitializationFailed(ex);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                SetState(ConnectorState.Error);
                return OperationResult<bool>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogConnectorInitializationFailed(ex);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                SetState(ConnectorState.Error);
                return OperationResult<bool>.Fail(
                    ConnectorErrorCodes.InitializationError,
                    MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Performs connector-specific initialization logic when the connector is initialized.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        protected abstract ValueTask InitializeConnectorAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Authenticates the connector using the first matching authentication configuration
        /// found in the schema for the provided connection settings.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> indicating whether authentication succeeded.
        /// </returns>
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

        /// <summary>
        /// Refreshes the current authentication credential, or performs a new
        /// authentication if no credential exists.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> indicating whether the refresh succeeded.
        /// </returns>
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

        /// <summary>
        /// Gets the authentication header value from the current credential,
        /// based on the authentication scheme.
        /// </summary>
        /// <returns>
        /// A string containing the authentication header value, or <c>null</c> if
        /// no credential is available or the scheme is not supported.
        /// </returns>
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

        /// <summary>
        /// Gets the API key from the current authentication credential,
        /// if the scheme is <see cref="AuthenticationScheme.ApiKey"/>.
        /// </summary>
        /// <returns>
        /// The API key string, or <c>null</c> if the credential is not an API key.
        /// </returns>
        protected virtual string? GetApiKey()
        {
            return _authenticationCredential?.Scheme == AuthenticationScheme.ApiKey
                ? _authenticationCredential.Value
                : null;
        }

        /// <summary>
        /// Determines whether the connector uses anonymous authentication
        /// (no authentication configurations or all are set to <see cref="AuthenticationScheme.None"/>).
        /// </summary>
        /// <returns>
        /// <c>true</c> if the connector is anonymous; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsAnonymousConnector()
        {
            return Schema.AuthenticationConfigurations.Count == 0 ||
                   Schema.AuthenticationConfigurations.All(c => c.Scheme == AuthenticationScheme.None);
        }

        /// <summary>
        /// Tests the connection to the remote service by performing a connector-specific
        /// connection test.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> indicating whether the connection test succeeded.
        /// </returns>
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

        /// <summary>
        /// Performs connector-specific connection testing logic.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        protected abstract ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sends a message through the connector after validating it.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> containing the result of the send operation.
        /// </returns>
        public async ValueTask<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(message);
            ValidateCapability(ChannelCapability.SendMessages);
            await EnsureInitializedAsync(cancellationToken);

            _idGenerator.EnsureMessageId(message);

            using var scope = BeginConnectorLoggerScope();
            using var messageScope = BeginMessageLoggerScope(message);

            using var activity = _telemetry.StartSendActivity(
                Schema.ChannelType, message.Id);
            var sw = Stopwatch.StartNew();

            var payloadSize = _telemetry.IsPayloadSizeEnabled
                ? (int)_telemetry.MeasurePayloadSize(message)
                : 0;

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
                    sw.Stop();
                    _telemetry.RecordSendFailure(sw.ElapsedMilliseconds, "validation_failed");
                    activity?.SetStatus(ActivityStatusCode.Error, "Validation failed");

                    return OperationResult<SendResult>.ValidationFailed(
                        ConnectorErrorCodes.MessageValidationFailed,
                        MessagingErrorCodes.ErrorDomain,
                        validationErrors);
                }

                Logger.LogMessageValidationPassed(message.Id!);

                Logger.LogSendingMessage(message.Id!);

                var pipeline = BuildPipeline<SendResult>();
                SendResult result;
                var attempts = 1;

                if (pipeline != null)
                {
                    var count = 0;

                    try
                    {
                        result = await pipeline.ExecuteAsync(async ct =>
                        {
                            count++;
                            return await SendMessageCoreAsync(message, ct);
                        }, cancellationToken);

                        attempts = count;
                        Logger.LogRetrySucceeded("SendMessage", attempts);
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType().Name == "BrokenCircuitException")
                        {
                            sw.Stop();
                            _telemetry.RecordSendFailure(sw.ElapsedMilliseconds, "circuit_breaker_open");
                            activity?.SetStatus(ActivityStatusCode.Error, "Circuit breaker open");

                            Logger.LogCircuitBreakerOpened("SendMessage", _effectiveRetryPolicy?.CircuitBreakerBreakDuration ?? TimeSpan.FromSeconds(30));
                            return OperationResult<SendResult>.Fail(
                                ConnectorErrorCodes.CircuitBreakerOpen,
                                MessagingErrorCodes.ErrorDomain,
                                "The circuit breaker is open; requests are blocked until it recovers");
                        }

                        // Non-retryable ConnectorExceptions propagate to the outer handler
                        // to preserve the original error code instead of being masked as exhaustion
                        if (ex is ConnectorException connEx)
                        {
                            if (_effectiveRetryPolicy == null || !_effectiveRetryPolicy.RetryableErrorCodes.Contains(connEx.ErrorCode))
                            {
                                throw;
                            }
                        }

                        sw.Stop();
                        _telemetry.RecordSendFailure(sw.ElapsedMilliseconds, "retry_exhausted");
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                        Logger.LogRetryExhausted(_effectiveRetryPolicy?.MaxRetryAttempts ?? 3, "SendMessage", ex.Message);
                        return OperationResult<SendResult>.Fail(
                            ConnectorErrorCodes.RetryAttemptsExhausted,
                            MessagingErrorCodes.ErrorDomain,
                            $"All retry attempts exhausted: {ex.Message}");
                    }
                }
                else
                {
                    result = await SendMessageCoreAsync(message, cancellationToken);

                    sw.Stop();
                    _telemetry.RecordSendSuccess(sw.ElapsedMilliseconds, payloadSize);
                    activity?.SetStatus(ActivityStatusCode.Ok);

                    result.AdditionalData[ResultMetadataKeys.RetryAttempts] = 1;
                    Logger.LogMessageSent(result.RemoteMessageId);

                    return OperationResult<SendResult>.Success(result);
                }

                sw.Stop();
                _telemetry.RecordSendSuccess(sw.ElapsedMilliseconds, payloadSize);
                activity?.SetStatus(ActivityStatusCode.Ok);

                result.AdditionalData[ResultMetadataKeys.RetryAttempts] = attempts;
                Logger.LogMessageSent(result.RemoteMessageId);

                return OperationResult<SendResult>.Success(result);
            }
            catch (MessagingException ex)
            {
                sw.Stop();
                _telemetry.RecordSendFailure(sw.ElapsedMilliseconds, ex.ErrorCode);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                Logger.LogMessageSendFailed(message.Id!, ex);
                return OperationResult<SendResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _telemetry.RecordSendFailure(sw.ElapsedMilliseconds, "unknown");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                Logger.LogMessageSendFailed(message.Id!, ex);
                return OperationResult<SendResult>.Fail(ConnectorErrorCodes.SendMessageError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Performs the core logic of sending a message to the remote service.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// A <see cref="SendResult"/> containing the result of the send operation.
        /// </returns>
        protected abstract Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a batch of messages through the connector after validating each message.
        /// </summary>
        /// <param name="batch">The batch of messages to send.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> containing the result of the batch send operation.
        /// </returns>
        public virtual async ValueTask<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(batch);
            ValidateCapability(ChannelCapability.BulkMessaging);
            await EnsureInitializedAsync(cancellationToken);

            _idGenerator.EnsureBatchId(batch);

            using var scope = BeginConnectorLoggerScope();
            using var activity = _telemetry.StartActivity(
                MessagingSemanticConventions.OperationBatchSend,
                Schema.ChannelType);

            var sw = Stopwatch.StartNew();

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
                    sw.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error, "Batch validation failed");
                    Logger.LogBatchValidationFailed(batch.Messages.Count());

                    return OperationResult<BatchSendResult>.ValidationFailed(
                        ConnectorErrorCodes.BatchValidationFailed,
                        MessagingErrorCodes.ErrorDomain,
                        allValidationErrors);
                }

                Logger.LogSendingBatch(batch.Messages.Count());

                var result = await SendBatchCoreAsync(batch, cancellationToken);

                sw.Stop();
                _telemetry.RecordSendSuccess(sw.ElapsedMilliseconds, messageCount: batch.Messages.Count());
                activity?.SetStatus(ActivityStatusCode.Ok);

                Logger.LogBatchSent(batch.Messages.Count());

                return result;
            }
            catch (MessagingException ex)
            {
                sw.Stop();
                _telemetry.RecordSendFailure(sw.ElapsedMilliseconds, ex.ErrorCode);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogBatchSendFailed(batch.Messages.Count(), ex);
                return OperationResult<BatchSendResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _telemetry.RecordSendFailure(sw.ElapsedMilliseconds, "unknown");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogBatchSendFailed(batch.Messages.Count(), ex);
                return OperationResult<BatchSendResult>.Fail(ConnectorErrorCodes.SendBatchError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Performs the core logic of sending a batch of messages.
        /// </summary>
        /// <param name="batch">The batch of messages to send.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// A <see cref="BatchSendResult"/> containing the result of the batch send operation.
        /// </returns>
        protected virtual Task<BatchSendResult> SendBatchCoreAsync(IMessageBatch batch, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Batch sending is not supported by this connector.");
        }

        /// <summary>
        /// Gets the current status of the connector.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> containing the status information of the connector.
        /// </returns>
        public async ValueTask<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken cancellationToken)
        {
            using var scope = BeginConnectorLoggerScope();
            using var activity = _telemetry.StartStatusQueryActivity();

            try
            {
                Logger.LogReadingStatus();

                var result = await GetConnectorStatusAsync(cancellationToken);

                activity?.SetStatus(ActivityStatusCode.Ok);

                Logger.LogStatusRead();

                return result;
            }
            catch (MessagingException ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogStatusReadFailed(ex);
                return OperationResult<StatusInfo>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogStatusReadFailed(ex);
                return OperationResult<StatusInfo>.Fail(ConnectorErrorCodes.GetStatusError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Performs connector-specific logic to retrieve the current status.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// A <see cref="StatusInfo"/> containing the connector status.
        /// </returns>
        protected abstract Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the status updates for a specific message.
        /// </summary>
        /// <param name="messageId">The identifier of the message to query.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> containing the status updates for the message.
        /// </returns>
        public async ValueTask<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
            ValidateCapability(ChannelCapability.MessageStatusQuery);
            await EnsureInitializedAsync(cancellationToken);

            using var scope = BeginConnectorLoggerScope();
            using var activity = _telemetry.StartStatusQueryActivity(messageId);

            try
            {
                Logger.LogReadingMessageStatus(messageId);

                var result = await GetMessageStatusCoreAsync(messageId, cancellationToken);

                activity?.SetStatus(ActivityStatusCode.Ok);

                Logger.LogMessageStatusRead(messageId);

                return result;
            }
            catch (MessagingException ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogMessageStatusReadFailed(messageId, ex);
                return OperationResult<StatusUpdatesResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogMessageStatusReadFailed(messageId, ex);
                return OperationResult<StatusUpdatesResult>.Fail(ConnectorErrorCodes.GetMessageStatusError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Performs the core logic of retrieving message status from the remote service.
        /// </summary>
        /// <param name="messageId">The identifier of the message.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// A <see cref="StatusUpdatesResult"/> containing the status updates.
        /// </returns>
        protected virtual Task<StatusUpdatesResult> GetMessageStatusCoreAsync(string messageId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Message status querying is not supported by this connector.");
        }

        /// <summary>
        /// Validates the message against the channel schema and any additional connector rules.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable of <see cref="ValidationResult"/> objects describing validation errors.
        /// </returns>
        public virtual async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(message);

            await foreach (var result in ValidateMessageCoreAsync(message, cancellationToken))
            {
                yield return result;
            }
        }

        /// <summary>
        /// Performs the core validation logic against the channel schema.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable of <see cref="ValidationResult"/> objects describing validation errors.
        /// </returns>
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

        /// <summary>
        /// Gets a string representation of the endpoint type for use in API calls.
        /// </summary>
        /// <param name="endpoint">The endpoint to get the type for.</param>
        /// <returns>
        /// A string representing the endpoint type, or <c>null</c> if the type is not recognized.
        /// </returns>
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

        /// <summary>
        /// Determines whether the given endpoint type is supported by the connector
        /// for the specified roles (sender and/or receiver).
        /// </summary>
        /// <param name="endpointType">The endpoint type to check.</param>
        /// <param name="asSender">If <c>true</c>, checks if the type is supported as a sender.</param>
        /// <param name="asReceiver">If <c>true</c>, checks if the type is supported as a receiver.</param>
        /// <returns>
        /// <c>true</c> if the endpoint type is supported; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsEndpointTypeSupported(EndpointType endpointType, bool asSender = false, bool asReceiver = false)
        {
            return Schema.Endpoints.Any(e =>
                (e.Type == EndpointType.Any || e.Type == endpointType) &&
                (!asSender || e.CanSend) &&
                (!asReceiver || e.CanReceive));
        }

        /// <summary>
        /// Receives a status update for a message from the remote service.
        /// </summary>
        /// <param name="source">The source of the message status update.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> containing the status update result.
        /// </returns>
        public async ValueTask<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken cancellationToken)
        {
            ValidateCapability(ChannelCapability.HandleMessageState);
            await EnsureInitializedAsync(cancellationToken);

            using var scope = BeginConnectorLoggerScope();
            using var activity = _telemetry.StartActivity("receive_status", Schema.ChannelType);

            try
            {
                Logger.LogReceivingMessageStatus();

                var result = await ReceiveMessageStatusCoreAsync(source, cancellationToken);

                activity?.SetStatus(ActivityStatusCode.Ok);

                Logger.LogMessageStatusReceived(result.MessageId);

                return result;
            }
            catch (MessagingException ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogMessageStatusReceiveFailed(ex);
                return OperationResult<StatusUpdateResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogMessageStatusReceiveFailed(ex);
                return OperationResult<StatusUpdateResult>.Fail(ConnectorErrorCodes.ReceiveStatusError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Performs the core logic of receiving a message status update from the remote service.
        /// </summary>
        /// <param name="source">The source of the message status update.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// A <see cref="StatusUpdateResult"/> containing the status update.
        /// </returns>
        protected virtual Task<StatusUpdateResult> ReceiveMessageStatusCoreAsync(MessageSource source, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Status receiving is not supported by this connector.");
        }

        /// <summary>
        /// Receives messages from the remote service.
        /// </summary>
        /// <param name="source">The source to receive messages from.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> containing the received messages.
        /// </returns>
        public async ValueTask<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken cancellationToken)
        {
            ValidateCapability(ChannelCapability.ReceiveMessages);
            await EnsureInitializedAsync(cancellationToken);

            using var scope = BeginConnectorLoggerScope();

            using var activity = _telemetry.StartReceiveActivity(Schema.ChannelType);
            var sw = Stopwatch.StartNew();

            try
            {
                Logger.LogReceivingMessage();

                var result = await ReceiveMessagesCoreAsync(source, cancellationToken);

                sw.Stop();
                var messageCount = result?.Messages?.Count ?? 0;
                _telemetry.RecordReceiveSuccess(sw.ElapsedMilliseconds, messageCount);
                activity?.SetStatus(ActivityStatusCode.Ok);

                Logger.LogMessageReceived();

                return result;
            }
            catch (MessagingException ex)
            {
                sw.Stop();
                _telemetry.RecordReceiveFailure(sw.ElapsedMilliseconds, ex.ErrorCode);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                Logger.LogMessageReceiveFailed(ex);
                return OperationResult<ReceiveResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _telemetry.RecordReceiveFailure(sw.ElapsedMilliseconds, "unknown");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                Logger.LogMessageReceiveFailed(ex);
                return OperationResult<ReceiveResult>.Fail(
                    ConnectorErrorCodes.ReceiveMessagesError,
                    MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Performs the core logic of receiving messages from the remote service.
        /// </summary>
        /// <param name="source">The source to receive messages from.</param>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// A <see cref="ReceiveResult"/> containing the received messages.
        /// </returns>
        protected virtual Task<ReceiveResult> ReceiveMessagesCoreAsync(MessageSource source, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Message receiving is not supported by this connector.");
        }

        /// <summary>
        /// Gets the health status of the connector.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns an <see cref="OperationResult{T}"/> containing the health information.
        /// </returns>
        public virtual async ValueTask<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken cancellationToken)
        {
            ValidateCapability(ChannelCapability.HealthCheck);

            using var scope = BeginConnectorLoggerScope();
            using var activity = _telemetry.StartActivity(
                MessagingSemanticConventions.OperationHealthCheck,
                Schema.ChannelType);

            try
            {
                Logger.LogCheckingHealth();

                var result = await GetConnectorHealthAsync(cancellationToken);

                activity?.SetStatus(ActivityStatusCode.Ok);

                Logger.LogHealthCheckSuccessful();

                return result;
            }
            catch (MessagingException ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogHealthCheckFailed(ex);
                return OperationResult<ConnectorHealth>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Logger.LogHealthCheckFailed(ex);
                return OperationResult<ConnectorHealth>.Fail(ConnectorErrorCodes.GetHealthError, MessagingErrorCodes.ErrorDomain, ex.Message);
            }
        }

        /// <summary>
        /// Performs connector-specific logic to determine health status.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// A <see cref="ConnectorHealth"/> containing the health information.
        /// </returns>
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

        /// <summary>
        /// Shuts down the connector gracefully.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
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
                _telemetry.Dispose();
            }
        }

        /// <summary>
        /// Performs connector-specific shutdown logic.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to propagate notification that the operation should be canceled.
        /// </param>
        protected virtual Task ShutdownConnectorAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
