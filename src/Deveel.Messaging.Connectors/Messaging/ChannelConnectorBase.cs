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
	/// <summary>
	/// Provides a base implementation of the <see cref="IChannelConnector"/> interface
	/// with common functionality for state management, capability validation, and authentication.
	/// </summary>
	/// <remarks>
	/// This abstract class handles the common concerns of connector implementations such as
	/// state transitions, capability checking, authentication management, and providing sensible
	/// default implementations for operations that may not be supported by all connectors.
	///
	/// Derived classes need to implement the abstract methods to provide connector-specific
	/// functionality while benefiting from the state management, validation logic, and
	/// authentication support provided by this base class.
	/// </remarks>
	public abstract class ChannelConnectorBase : IChannelConnector
	{
		private ConnectorState _state = ConnectorState.Uninitialized;
		private readonly object _stateLock = new object();
		private AuthenticationCredential? _authenticationCredential;
		private readonly IAuthenticationManager _authenticationManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChannelConnectorBase"/> class
		/// with the specified schema.
		/// </summary>
		/// <param name="schema">The schema describing the connector's capabilities and configuration.</param>
		/// <param name="connectionSettings">The connector configuration values used to initialize and authenticate the connector.</param>
		/// <param name="logger">A service used to log messages</param>
		/// <param name="authenticationManager">Optional authentication manager for handling authentication flows</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="schema"/> is null.</exception>
		protected ChannelConnectorBase(
            IChannelSchema schema,
            ConnectionSettings? connectionSettings = null,
            ILogger? logger = null,
            IAuthenticationManager? authenticationManager = null)
		{
			Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            ConnectionSettings = connectionSettings ?? new ConnectionSettings();
			Logger = logger ?? NullLogger.Instance; // Use a null logger if none is provided
			_authenticationManager = authenticationManager ?? new AuthenticationManager(logger: NullLogger<AuthenticationManager>.Instance);
		}

		/// <inheritdoc/>
		public IChannelSchema Schema { get; }

		/// <summary>
		/// Gets the connection settings used by this connector instance.
		/// </summary>
		public ConnectionSettings ConnectionSettings { get; }

		/// <summary>
		/// Provides a service for the connector to log messages.
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// Gets the authentication manager used by this connector.
		/// </summary>
		protected IAuthenticationManager AuthenticationManager => _authenticationManager;

		/// <summary>
		/// Gets the current authentication credential, if available.
		/// </summary>
		protected AuthenticationCredential? AuthenticationCredential => _authenticationCredential;

		/// <inheritdoc/>
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

		/// <summary>
		/// Sets the current state of the connector.
		/// </summary>
		/// <param name="newState">The new state to transition to.</param>
		protected void SetState(ConnectorState newState)
		{
			lock (_stateLock)
			{
				_state = newState;
			}
		}

		/// <summary>
		/// Validates that the connector supports the specified capability.
		/// </summary>
		/// <param name="capability">The capability to validate.</param>
		/// <exception cref="NotSupportedException">
		/// Thrown when the connector does not support the specified capability.
		/// </exception>
		protected void ValidateCapability(ChannelCapability capability)
		{
			if (!Schema.Capabilities.HasFlag(capability))
			{
				throw new NotSupportedException($"The connector does not support the '{capability}' capability.");
			}
		}

		/// <summary>
		/// Validates that the connector is in an operational state (Ready or Error).
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the connector is not in an operational state.
		/// </exception>
		protected void ValidateOperationalState()
		{
			var currentState = State;
			if (currentState == ConnectorState.Uninitialized ||
			    currentState == ConnectorState.Initializing ||
			    currentState == ConnectorState.ShuttingDown ||
			    currentState == ConnectorState.Shutdown)
			{
				throw new InvalidOperationException($"The connector is not in an operational state. Current state: {currentState}");
			}
		}

		/// <summary>
		/// Begins a logging scope that enriches log entries with connector channel information.
		/// </summary>
		/// <returns>An <see cref="IDisposable"/> used to end the scope, or <see langword="null"/> when scoping is unavailable.</returns>
		protected virtual IDisposable? BeginConnectorLoggerScope()
        {
            return Logger.BeginScope(
                "[{ChannelType} v{ChannelVersion}]",
                    Schema.ChannelType,
                    Schema.Version);
        }

		/// <summary>
		/// Begins a logging scope that enriches log entries with the message identifier.
		/// </summary>
		/// <param name="message">The message being processed.</param>
		/// <returns>An <see cref="IDisposable"/> used to end the scope, or <see langword="null"/> when scoping is unavailable.</returns>
		protected virtual IDisposable? BeginMessageLoggerScope(IMessage message)
        {
            return Logger.BeginScope("[MessageId:{MessageId}]", message.Id);
        }

		/// <inheritdoc/>
		public async Task<OperationResult<bool>> InitializeAsync(CancellationToken cancellationToken)
        {
            using var scope = BeginConnectorLoggerScope();

			if (State != ConnectorState.Uninitialized)
			{
				return OperationResult<bool>.Fail(ConnectorErrorCodes.AlreadyInitialized,
					Schema.ChannelType,
					"The connector has already been initialized.");
			}

			SetState(ConnectorState.Initializing);

            try
            {
                Logger.LogInitializingConnector();

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
					Schema.ChannelType, ex.Message);
			}
		}

		/// <summary>
		/// When overridden in a derived class, performs the actual connector initialization logic.
		/// This method can call <see cref="AuthenticateAsync(CancellationToken)"/>
		/// to handle authentication during initialization.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous initialization operation.</returns>
		protected abstract ValueTask InitializeConnectorAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Performs authentication using the connection settings and the first supported authentication configuration.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task representing the asynchronous authentication operation.</returns>
		protected async Task<OperationResult<bool>> AuthenticateAsync(CancellationToken cancellationToken = default)
		{
            using var scope = BeginConnectorLoggerScope();

            try
            {
                Logger.LogStartingAuthentication();

                // Find the first authentication configuration that is satisfied by the connection settings
                var authConfig =
                    Schema.AuthenticationConfigurations.FirstOrDefault(config =>
                        config.IsSatisfiedBy(ConnectionSettings));

                if (authConfig == null)
                {
                    Logger.LogNoAuthenticationConfigurationFound();
                    return OperationResult<bool>.Fail(
	                    ConnectorErrorCodes.AuthenticationFailed,
	                    Schema.ChannelType,
                        "No suitable authentication configuration found for the provided connection settings");
                }

                Logger.LogUsingAuthenticationConfiguration(authConfig.AuthenticationType);

                // Perform authentication
                var authResult =
                    await _authenticationManager.AuthenticateAsync(ConnectionSettings, authConfig, cancellationToken);

                if (authResult.IsSuccessful && authResult.Credential != null)
                {
                    _authenticationCredential = authResult.Credential;
                    Logger.LogAuthenticationSuccessful(authConfig.AuthenticationType);
                    return true;
                }
                else
                {
                    Logger.LogAuthenticationFailed(authConfig.AuthenticationType);
                    return OperationResult<bool>.Fail(
	                    ConnectorErrorCodes.AuthenticationFailed,
	                    Schema.ChannelType,
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
					Schema.ChannelType,
					$"Authentication error: {ex.Message}");
			}
		}

		/// <summary>
		/// Refreshes the current authentication credential if it's about to expire or has expired.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task representing the asynchronous refresh operation.</returns>
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

				// Find the authentication configuration
				var authConfig = Schema.AuthenticationConfigurations.FirstOrDefault(config =>
					config.AuthenticationType == _authenticationCredential.AuthenticationType);

				if (authConfig == null)
				{
					Logger.LogAuthenticationConfigurationNotFoundForType(_authenticationCredential.AuthenticationType);
					return await AuthenticateAsync(cancellationToken);
				}

				// Refresh the credential
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
						Schema.ChannelType,
						authResult.ErrorMessage ?? "Authentication refresh failed");
				}
			}
			catch (Exception ex)
			{
				Logger.LogAuthenticationRefreshException(ex);
				return OperationResult<bool>.Fail(
					ConnectorErrorCodes.AuthenticationFailed,
					Schema.ChannelType,
					$"Authentication refresh error: {ex.Message}");
			}
		}

		/// <summary>
		/// Gets the authentication header value for HTTP requests, if applicable.
		/// </summary>
		/// <returns>The authentication header value, or null if not applicable.</returns>
		protected virtual string? GetAuthenticationHeader()
		{
			if (_authenticationCredential == null)
				return null;

			return _authenticationCredential.AuthenticationType switch
			{
				AuthenticationType.Token => GetTokenAuthHeader(),
				AuthenticationType.Basic => GetBasicAuthHeader(),
				AuthenticationType.ApiKey => null, // API keys are usually added as custom headers or query parameters
				_ => null
			};
		}

		private string? GetTokenAuthHeader()
		{
			if (_authenticationCredential == null)
				return null;

			var tokenType = _authenticationCredential.Properties.TryGetValue("TokenType", out var type) ? type?.ToString() : "Bearer";
			return $"{tokenType} {_authenticationCredential.CredentialValue}";
		}

		private string? GetBasicAuthHeader()
		{
			if (_authenticationCredential == null)
				return null;

			return $"Basic {_authenticationCredential.CredentialValue}";
		}

		/// <summary>
		/// Gets the API key for requests, if applicable.
		/// </summary>
		/// <returns>The API key value, or null if not applicable.</returns>
		protected virtual string? GetApiKey()
		{
			return _authenticationCredential?.AuthenticationType == AuthenticationType.ApiKey
				? _authenticationCredential.CredentialValue
				: null;
		}

		/// <summary>
		/// Checks if the connector is configured for anonymous access (no authentication required).
		/// </summary>
		/// <returns>True if the connector is anonymous, false otherwise.</returns>
		protected virtual bool IsAnonymousConnector()
		{
			return Schema.AuthenticationConfigurations.Count == 0;
		}

		/// <inheritdoc/>
		public async Task<OperationResult<bool>> TestConnectionAsync(CancellationToken cancellationToken)
		{
			ValidateOperationalState();

            using var scope = BeginConnectorLoggerScope();

            try
            {
                Logger.LogTestingConnection();

                await TestConnectorConnectionAsync(cancellationToken);

                Logger.LogConnectionTestSuccessful();

                return true;
            }
            catch (ConnectorException ex)
            {
                Logger.LogConnectionTestFailed(ex);

                return OperationResult<bool>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
			catch (Exception ex)
			{
                Logger.LogConnectionTestFailed(ex);

				return OperationResult<bool>.Fail(
					ConnectorErrorCodes.ConnectionTestError, 
					Schema.ChannelType, ex.Message);
			}
		}

		/// <summary>
		/// When overridden in a derived class, performs the actual connection testing logic.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous connection test operation.</returns>
		protected abstract ValueTask TestConnectorConnectionAsync(CancellationToken cancellationToken);

		/// <inheritdoc/>
		public async Task<OperationResult<SendResult>> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(message);
			ValidateCapability(ChannelCapability.SendMessages);
			ValidateOperationalState();

            using var scope = BeginConnectorLoggerScope();
            using var messageScope = BeginMessageLoggerScope(message);

            try
            {
                Logger.LogValidatingMessage(message.Id);

                // Validate the message before sending
                var validationErrors = new List<ValidationResult>();
                await foreach (var validationResult in ValidateMessageAsync(message, cancellationToken))
                {
                    if (validationResult != ValidationResult.Success)
                    {
                        validationErrors.Add(validationResult);
                    }
                }

                // If there are validation errors, return a failure result
                if (validationErrors.Count > 0)
                {
                    Logger.LogMessageValidationFailed(message.Id, validationErrors.Count);

                    return OperationResult<SendResult>.ValidationFailed(
	                    ConnectorErrorCodes.MessageValidationFailed,
	                    Schema.ChannelType,
	                    validationErrors);
                }

                Logger.LogMessageValidationPassed(message.Id);

                Logger.LogSendingMessage(message.Id);

                var result = await SendMessageCoreAsync(message, cancellationToken);

                Logger.LogMessageSent(result.RemoteMessageId);

                return result;
            }
            catch (MessagingException ex)
            {
                Logger.LogMessageSendFailed(message.Id, ex);
                return OperationResult<SendResult>.Fail(ex.ErrorCode, ex.ErrorDomain, ex.Message);
            }
			catch (Exception ex)
			{
                Logger.LogMessageSendFailed(message.Id, ex);
				return OperationResult<SendResult>.Fail(ConnectorErrorCodes.SendMessageError, Schema.ChannelType, ex.Message);
			}
		}

		/// <summary>
		/// When overridden in a derived class, performs the actual message sending logic.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous send operation.</returns>
		protected abstract Task<SendResult> SendMessageCoreAsync(IMessage message, CancellationToken cancellationToken);

		/// <inheritdoc/>
		public virtual async Task<OperationResult<BatchSendResult>> SendBatchAsync(IMessageBatch batch, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(batch);
			ValidateCapability(ChannelCapability.BulkMessaging);
			ValidateOperationalState();

            using var scope = BeginConnectorLoggerScope();

            try
            {
                // Validate all messages in the batch before sending
                var allValidationErrors = new List<ValidationResult>();
                var messageValidationResults = new Dictionary<string, List<ValidationResult>>();

                foreach (var message in batch.Messages)
                {
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
                        messageValidationResults[message.Id] = messageErrors;
                    }
                }

                // If there are validation errors, return a failure result with details
                if (allValidationErrors.Count > 0)
                {
                    Logger.LogBatchValidationFailed(batch.Messages.Count());

                    var errorData = new Dictionary<string, object>
                    {
                        ["MessageValidationResults"] = messageValidationResults
                    };

                    return OperationResult<BatchSendResult>.ValidationFailed(
	                    ConnectorErrorCodes.BatchValidationFailed,
	                    Schema.ChannelType,
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
				return OperationResult<BatchSendResult>.Fail(ConnectorErrorCodes.SendBatchError, Schema.ChannelType, ex.Message);
			}
		}

		/// <summary>
		/// When overridden in a derived class, performs the actual batch sending logic.
		/// The default implementation throws <see cref="NotSupportedException"/>.
		/// </summary>
		/// <param name="batch">The batch of messages to send.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous batch send operation.</returns>
		protected virtual Task<BatchSendResult> SendBatchCoreAsync(IMessageBatch batch, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Batch sending is not supported by this connector.");
		}

		/// <inheritdoc/>
		public async Task<OperationResult<StatusInfo>> GetStatusAsync(CancellationToken cancellationToken)
		{
            using var scope = BeginConnectorLoggerScope();

			try
            {
                Logger.LogReadingStatus();

				var result = await GetConnectorStatusAsync(cancellationToken);

                Logger.LogStatusRead();

                return result;
            }
			catch (Exception ex)
			{
                Logger.LogStatusReadFailed(ex);
				return OperationResult<StatusInfo>.Fail(ConnectorErrorCodes.GetStatusError, Schema.ChannelType, ex.Message);
			}
		}

		/// <summary>
		/// When overridden in a derived class, retrieves the current status of the connector.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous status retrieval operation.</returns>
		protected abstract Task<StatusInfo> GetConnectorStatusAsync(CancellationToken cancellationToken);

		/// <inheritdoc/>
		public async Task<OperationResult<StatusUpdatesResult>> GetMessageStatusAsync(string messageId, CancellationToken cancellationToken)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
			ValidateCapability(ChannelCapability.MessageStatusQuery);
			ValidateOperationalState();

            using var scope = BeginConnectorLoggerScope();

			try
			{
                Logger.LogReadingMessageStatus(messageId);

				var result = await GetMessageStatusCoreAsync(messageId, cancellationToken);

                Logger.LogMessageStatusRead(messageId);

                return result;
            }
			catch (Exception ex)
			{
                Logger.LogMessageStatusReadFailed(messageId, ex);
				return OperationResult<StatusUpdatesResult>.Fail(ConnectorErrorCodes.GetMessageStatusError, Schema.ChannelType, ex.Message);
			}
		}

		/// <summary>
		/// When overridden in a derived class, retrieves the status updates for a specific message.
		/// The default implementation throws <see cref="NotSupportedException"/>.
		/// </summary>
		/// <param name="messageId">The unique identifier of the message.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous message status retrieval operation.</returns>
		protected virtual Task<StatusUpdatesResult> GetMessageStatusCoreAsync(string messageId, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Message status querying is not supported by this connector.");
		}

		/// <inheritdoc/>
		public virtual async IAsyncEnumerable<ValidationResult> ValidateMessageAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(message);

			await foreach (var result in ValidateMessageCoreAsync(message, cancellationToken))
			{
				yield return result;
			}
		}

		/// <summary>
		/// When overridden in a derived class, validates a message for sending through the connector.
		/// The default implementation performs basic validation and delegates to schema validation.
		/// </summary>
		/// <param name="message">The message to validate.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>An async enumerable of validation results.</returns>
		protected virtual async IAsyncEnumerable<ValidationResult> ValidateMessageCoreAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			// Use schema validation as the primary validation mechanism
			// This includes validation of: message ID, endpoints, content type, and message properties
			var validationResults = Schema.ValidateMessage(message);
			var hasValidationErrors = false;

			foreach (var validationResult in validationResults)
			{
				hasValidationErrors = true;
				yield return validationResult;
			}

			// If no validation errors were found, yield success
			if (!hasValidationErrors)
			{
				yield return ValidationResult.Success!;
			}

			await Task.CompletedTask; // Suppress compiler warning about not being async
		}

		/// <summary>
		/// Gets the endpoint type from an endpoint. Override this method to provide custom endpoint type extraction logic.
		/// </summary>
		/// <param name="endpoint">The endpoint to extract the type from.</param>
		/// <returns>The endpoint type string, or null if it cannot be determined.</returns>
		protected virtual string? GetEndpointType(IEndpoint endpoint)
		{
			// Convert EndpointType enum to its string representation for compatibility
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
		/// Checks if an endpoint type is supported by this connector.
		/// </summary>
		/// <param name="endpointType">The endpoint type to check.</param>
		/// <param name="asSender">Whether to check as a sender.</param>
		/// <param name="asReceiver">Whether to check as a receiver.</param>
		/// <returns>True if the endpoint type is supported in the specified role.</returns>
		protected virtual bool IsEndpointTypeSupported(EndpointType endpointType, bool asSender = false, bool asReceiver = false)
		{
			// Check if any endpoint configuration matches
			return Schema.Endpoints.Any(e =>
				(e.Type == EndpointType.Any || e.Type == endpointType) &&
				(!asSender || e.CanSend) &&
				(!asReceiver || e.CanReceive));
		}

		/// <inheritdoc/>
		public async Task<OperationResult<StatusUpdateResult>> ReceiveMessageStatusAsync(MessageSource source, CancellationToken cancellationToken)
		{
			ValidateCapability(ChannelCapability.HandleMessageState);
			ValidateOperationalState();

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
				return OperationResult<StatusUpdateResult>.Fail(ConnectorErrorCodes.ReceiveStatusError, Schema.ChannelType, ex.Message);
			}
		}

		/// <summary>
		/// When overridden in a derived class, receives status updates from the specified source.
		/// The default implementation throws <see cref="NotSupportedException"/>.
		/// </summary>
		/// <param name="source">The source from which to receive status updates.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous status receiving operation.</returns>
		protected virtual Task<StatusUpdateResult> ReceiveMessageStatusCoreAsync(MessageSource source, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Status receiving is not supported by this connector.");
		}

		/// <inheritdoc/>
		public async Task<OperationResult<ReceiveResult>> ReceiveMessagesAsync(MessageSource source, CancellationToken cancellationToken)
		{
			ValidateCapability(ChannelCapability.ReceiveMessages);
			ValidateOperationalState();

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
					Schema.ChannelType, ex.Message);
			}
		}

		/// <summary>
		/// When overridden in a derived class, receives messages from the specified source.
		/// The default implementation throws <see cref="NotSupportedException"/>.
		/// </summary>
		/// <param name="source">The source from which to receive messages.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous message receiving operation.</returns>
		protected virtual Task<ReceiveResult> ReceiveMessagesCoreAsync(MessageSource source, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Message receiving is not supported by this connector.");
		}

		/// <inheritdoc/>
		public virtual async Task<OperationResult<ConnectorHealth>> GetHealthAsync(CancellationToken cancellationToken)
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
			catch (Exception ex)
			{
                Logger.LogHealthCheckFailed(ex);
				return OperationResult<ConnectorHealth>.Fail(ConnectorErrorCodes.GetHealthError, Schema.ChannelType, ex.Message);
			}
		}

		/// <summary>
		/// When overridden in a derived class, retrieves the health information of the connector.
		/// The default implementation returns basic health information based on the current state.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous health check operation.</returns>
		protected virtual Task<ConnectorHealth> GetConnectorHealthAsync(CancellationToken cancellationToken)
		{
			var health = new ConnectorHealth
			{
				State = State,
				IsHealthy = State == ConnectorState.Ready,
				LastHealthCheck = DateTime.UtcNow,
				Uptime = TimeSpan.Zero // Derived classes should track actual uptime
			};

			if (!health.IsHealthy)
			{
				health.Issues.Add($"Connector is in {State} state");
			}

			return Task.FromResult(health);
		}

		/// <inheritdoc/>
		public async Task ShutdownAsync(CancellationToken cancellationToken)
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

		/// <summary>
		/// When overridden in a derived class, performs the actual connector shutdown logic.
		/// The default implementation does nothing.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous shutdown operation.</returns>
		protected virtual Task ShutdownConnectorAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
