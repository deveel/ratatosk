//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Ratatosk
{
    /// <summary>
    /// Provides high-performance logging extensions for Channel Connector Base operations using source-generated logging methods.
    /// </summary>
    internal static partial class LoggerExtensions
    {
        #region Authentication Logging

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.StartingAuthentication,
            Level = LogLevel.Debug,
            Message = "Starting authentication process")]
        internal static partial void LogStartingAuthentication(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.NoAuthenticationConfigurationFound,
            Level = LogLevel.Warning,
            Message = "No suitable authentication configuration found for the provided connection settings")]
        internal static partial void LogNoAuthenticationConfigurationFound(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.UsingAuthenticationConfiguration,
            Level = LogLevel.Debug,
            Message = "Using authentication configuration: {Scheme}")]
        internal static partial void LogUsingAuthenticationConfiguration(this ILogger logger, AuthenticationScheme scheme);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.AuthenticationSuccessful,
            Level = LogLevel.Information,
            Message = "Authentication successful using {Scheme}")]
        internal static partial void LogAuthenticationSuccessful(this ILogger logger, AuthenticationScheme scheme);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.AuthenticationFailed,
            Level = LogLevel.Error,
            Message = "Authentication failed using {Scheme}")]
        internal static partial void LogAuthenticationFailed(this ILogger logger, AuthenticationScheme scheme);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.AuthenticationException,
            Level = LogLevel.Error,
            Message = "Unexpected error during authentication")]
        internal static partial void LogAuthenticationException(this ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.NoCredentialToRefresh,
            Level = LogLevel.Warning,
            Message = "No authentication credential to refresh")]
        internal static partial void LogNoCredentialToRefresh(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.RefreshingAuthenticationCredential,
            Level = LogLevel.Debug,
            Message = "Refreshing authentication credential")]
        internal static partial void LogRefreshingAuthenticationCredential(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.NoAuthenticationConfigurationFoundForType,
            Level = LogLevel.Warning,
            Message = "Authentication configuration not found for credential scheme {Scheme}")]
        internal static partial void LogAuthenticationConfigurationNotFoundForType(this ILogger logger, AuthenticationScheme scheme);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.AuthenticationCredentialRefreshed,
            Level = LogLevel.Information,
            Message = "Authentication credential refreshed successfully")]
        internal static partial void LogAuthenticationCredentialRefreshed(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.AuthenticationCredentialRefreshFailed,
            Level = LogLevel.Error,
            Message = "Authentication refresh failed")]
        internal static partial void LogAuthenticationRefreshFailed(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.AuthenticationCredentialRefreshException,
            Level = LogLevel.Error,
            Message = "Unexpected error during authentication refresh")]
        internal static partial void LogAuthenticationRefreshException(this ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.AuthenticationProviderRegistered,
            Level = LogLevel.Debug,
            Message = "Registered authentication provider: {ProviderName} for {Scheme}")]
        internal static partial void LogAuthenticationProviderRegistered(this ILogger logger, string providerName, AuthenticationScheme scheme);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.AuthenticationProviderNotFound,
            Level = LogLevel.Warning,
            Message = "No authentication provider found for {Scheme}")]
        internal static partial void LogAuthenticationProviderNotFound(this ILogger logger, AuthenticationScheme scheme);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.UsingCachedCredential,
            Level = LogLevel.Debug,
            Message = "Using cached credential for {Scheme}")]
        internal static partial void LogUsingCachedCredential(this ILogger logger, AuthenticationScheme scheme);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ObtainingNewCredential,
            Level = LogLevel.Debug,
            Message = "Obtaining new credential for {Scheme}")]
        internal static partial void LogObtainingNewCredential(this ILogger logger, AuthenticationScheme scheme);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.AuthenticationFailedWithMessage,
            Level = LogLevel.Warning,
            Message = "Authentication failed for {Scheme}: {ErrorMessage}")]
        internal static partial void LogAuthenticationFailedWithMessage(this ILogger logger, AuthenticationScheme scheme, string? errorMessage);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.CacheCleared,
            Level = LogLevel.Debug,
            Message = "Authentication credential cache cleared")]
        internal static partial void LogCacheCleared(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.CredentialInvalidated,
            Level = LogLevel.Debug,
            Message = "Invalidated cached credential for {Scheme}")]
        internal static partial void LogCredentialInvalidated(this ILogger logger, AuthenticationScheme scheme);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.DefaultProvidersRegistered,
            Level = LogLevel.Debug,
            Message = "Registered default authentication providers")]
        internal static partial void LogDefaultProvidersRegistered(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.AutoAuthenticationFailed,
            Level = LogLevel.Warning,
            Message = "Auto-authentication failed during initialization: {Error}")]
        internal static partial void LogAutoAuthenticationFailed(this ILogger logger, string? error);

        #endregion

        #region Authentication Provider Logging

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.FoundCredentials,
            Level = LogLevel.Debug,
            Message = "Found {Scheme} credentials in field: {Field}")]
        internal static partial void LogFoundCredentials(this ILogger logger, AuthenticationScheme scheme, string field);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ObtainingAccessToken,
            Level = LogLevel.Debug,
            Message = "Obtaining access token using client credentials flow")]
        internal static partial void LogObtainingAccessToken(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.TokenRequestSent,
            Level = LogLevel.Debug,
            Message = "Requesting access token from {TokenEndpoint}")]
        internal static partial void LogTokenRequestSent(this ILogger logger, string tokenEndpoint);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.TokenRequestFailed,
            Level = LogLevel.Error,
            Message = "Token request failed with status {StatusCode}: {ErrorContent}")]
        internal static partial void LogTokenRequestFailed(this ILogger logger, string statusCode, string errorContent);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.MissingAccessToken,
            Level = LogLevel.Error,
            Message = "Token response does not contain access_token")]
        internal static partial void LogMissingAccessToken(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.TokenObtained,
            Level = LogLevel.Information,
            Message = "Successfully obtained access token (expires at: {ExpiresAt})")]
        internal static partial void LogTokenObtained(this ILogger logger, string? expiresAt);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.NetworkErrorDuringTokenRequest,
            Level = LogLevel.Error,
            Message = "Network error during token request")]
        internal static partial void LogNetworkErrorDuringTokenRequest(this ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.TokenRequestTimedOut,
            Level = LogLevel.Error,
            Message = "Token request timed out")]
        internal static partial void LogTokenRequestTimedOut(this ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.TokenParseFailed,
            Level = LogLevel.Error,
            Message = "Failed to parse token response")]
        internal static partial void LogTokenParseFailed(this ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.UnexpectedTokenError,
            Level = LogLevel.Error,
            Message = "Unexpected error during token request")]
        internal static partial void LogUnexpectedTokenError(this ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.RefreshingWithRefreshToken,
            Level = LogLevel.Debug,
            Message = "Refreshing access token using refresh token")]
        internal static partial void LogRefreshingWithRefreshToken(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.NoRefreshTokenAvailable,
            Level = LogLevel.Debug,
            Message = "No refresh token available, obtaining new token")]
        internal static partial void LogNoRefreshTokenAvailable(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.TokenRefreshError,
            Level = LogLevel.Error,
            Message = "Error during token refresh")]
        internal static partial void LogTokenRefreshError(this ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.TokenRefreshFailedWithStatus,
            Level = LogLevel.Error,
            Message = "Token refresh failed with status {StatusCode}: {ErrorContent}")]
        internal static partial void LogTokenRefreshFailedWithStatus(this ILogger logger, string statusCode, string errorContent);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.RetryingTokenObtainment,
            Level = LogLevel.Debug,
            Message = "Refresh token failed, attempting to obtain new token")]
        internal static partial void LogRetryingTokenObtainment(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.TokenRefreshed,
            Level = LogLevel.Information,
            Message = "Successfully refreshed access token (expires at: {ExpiresAt})")]
        internal static partial void LogTokenRefreshed(this ILogger logger, string? expiresAt);

        #endregion

        #region State Management Logging

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.StateChanged,
            Level = LogLevel.Debug,
            Message = "Connector state changed from {OldState} to {NewState}")]
        internal static partial void LogStateChanged(this ILogger logger, ConnectorState oldState, ConnectorState newState);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.InitializingConnector,
            Level = LogLevel.Information,
            Message = "Initializing connector...")]
        internal static partial void LogInitializingConnector(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ConnectorInitialized,
            Level = LogLevel.Information,
            Message = "Connector initialized successfully")]
        internal static partial void LogConnectorInitialized(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ConnectorInitializationFailed,
            Level = LogLevel.Error,
            Message = "Connector initialization failed")]
        internal static partial void LogConnectorInitializationFailed(this ILogger logger, Exception exception);

        #endregion

        #region Testing Connection Logging

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.TestingConnection,
            Level = LogLevel.Debug,
            Message = "Testing connection...")]
        internal static partial void LogTestingConnection(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ConnectionTestSuccessful,
            Level = LogLevel.Debug,
            Message = "Connection test successful")]
        internal static partial void LogConnectionTestSuccessful(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ConnectionTestFailed,
            Level = LogLevel.Warning,
            Message = "Connection test failed")]
        internal static partial void LogConnectionTestFailed(this ILogger logger, Exception ex);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ReadingStatus,
            Level = LogLevel.Debug,
            Message = "Reading connector status...")]
        internal static partial void LogReadingStatus(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.StatusRead,
            Level = LogLevel.Information,
            Message = "Connector status read")]
        internal static partial void LogStatusRead(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.StatusReadFailed,
            Level = LogLevel.Error,
            Message = "Failed to read connector status")]
        internal static partial void LogStatusReadFailed(this ILogger logger, Exception ex);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.CheckingHealth,
            Level = LogLevel.Debug,
            Message = "Checking connector health...")]
        public static partial void LogCheckingHealth(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.HealthCheckSuccessful,
            Level = LogLevel.Information,
            Message = "Connector health check was successful")]
        public static partial void LogHealthCheckSuccessful(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.HealthCheckFailed,
            Level = LogLevel.Error,
            Message = "Connector health check failed")]
        public static partial void LogHealthCheckFailed(this ILogger logger, Exception ex);

        #endregion

        #region Validation Logging

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ValidatingMessage,
            Level = LogLevel.Debug,
            Message = "Validating message {MessageId} for sending")]
        internal static partial void LogValidatingMessage(this ILogger logger, string messageId);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.MessageValidationFailed,
            Level = LogLevel.Warning,
            Message = "Message {MessageId} validation failed with {ErrorCount} error(s)")]
        internal static partial void LogMessageValidationFailed(this ILogger logger, string messageId, int errorCount);

        [LoggerMessage(
            EventId =  ConnectorLoggerEventId.MessageValidationPassed,
            Level = LogLevel.Debug,
            Message = "Message {MessageId} validation passed")]
        internal static partial void LogMessageValidationPassed(this ILogger logger, string messageId);

        [LoggerMessage(
            EventId =  ConnectorLoggerEventId.BatchValidationFailed,
            Level = LogLevel.Warning,
            Message = "Batch validation failed for {MessageCount} message(s) in the batch")]
        internal static partial void LogBatchValidationFailed(this ILogger logger, int messageCount);

        #endregion

        #region Send Message Logging

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.SendingMessage,
            Level = LogLevel.Debug,
            Message = "Sending message {MessageId}")]
        public static partial void LogSendingMessage(this ILogger logger, string messageId);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.MessageSent,
            Level = LogLevel.Information,
            Message = "Message sent successfully - RemoteMessageId: {RemoteMessageId}")]
        public static partial void LogMessageSent(this ILogger logger, string remoteMessageId);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.MessageSendFailed,
            Level = LogLevel.Error,
            Message = "Failed to send message {MessageId}")]
        public static partial void LogMessageSendFailed(this ILogger logger, string messageId, Exception exception);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.SendingBatch,
            Level = LogLevel.Debug,
            Message = "Sending batch of {MessageCount} messages")]
        public static partial void LogSendingBatch(this ILogger logger, int messageCount);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.BatchSent,
            Level = LogLevel.Information,
            Message = "Batch of {MessageCount} messages sent successfully")]
        public static partial void LogBatchSent(this ILogger logger, int messageCount);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.BatchSendFailed,
            Level = LogLevel.Error,
            Message = "Failed to send batch of {MessageCount} messages")]
        public static partial void LogBatchSendFailed(this ILogger logger, int messageCount, Exception exception);

        #endregion

        #region Receive Message Logging

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ReceivingMessage,
            Level = LogLevel.Debug,
            Message = "Receiving a message")]
        public static partial void LogReceivingMessage(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.MessageReceived,
            Level = LogLevel.Information,
            Message = "Message batch received successfully")]
        public static partial void LogMessageReceived(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.MessageReceiveFailed,
            Level = LogLevel.Error,
            Message = "Failed to receive a message")]
        public static partial void LogMessageReceiveFailed(this ILogger logger, Exception exception);
        #endregion

        #region Message Status Logging

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ReadingMessageStatus,
            Level = LogLevel.Debug,
            Message = "Reading status for message {MessageId}")]
        public static partial void LogReadingMessageStatus(this ILogger logger, string messageId);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.MessageStatusRead,
            Level = LogLevel.Debug,
            Message = "Message {MessageId} status read successfully")]
        public static partial void LogMessageStatusRead(this ILogger logger, string messageId);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.MessageStatusReadFailed,
            Level = LogLevel.Error,
            Message = "Failed to read status for message {MessageId}")]
        public static partial void LogMessageStatusReadFailed(this ILogger logger, string messageId, Exception exception);


        [LoggerMessage(
            EventId = ConnectorLoggerEventId.ReceivingMessageStatus,
            Level = LogLevel.Debug,
            Message = "Receiving status update for a message")]
        public static partial void LogReceivingMessageStatus(this ILogger logger);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.MessageStatusReceived,
            Level = LogLevel.Debug,
            Message = "Message {MessageId} status received successfully")]
        public static partial void LogMessageStatusReceived(this ILogger logger, string messageId);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.MessageStatusReceiveFailed,
            Level = LogLevel.Error,
            Message = "Failed to receive status for a message")]
        public static partial void LogMessageStatusReceiveFailed(this ILogger logger, Exception exception);

        #endregion

        #region Capability Validation Logging

        [LoggerMessage(
            EventId =  ConnectorLoggerEventId.ValidatingCapability,
            Level = LogLevel.Debug,
            Message = "Validating capability: {Capability}")]
        internal static partial void LogValidatingCapability(this ILogger logger, string capability);

        [LoggerMessage(
            EventId =  ConnectorLoggerEventId.CapabilityNotSupported,
            Level = LogLevel.Error,
            Message = "Capability {Capability} is not supported by this connector")]
        internal static partial void LogCapabilityNotSupported(this ILogger logger, string capability);

        [LoggerMessage(
            EventId =  ConnectorLoggerEventId.ValidatingOperationalState,
            Level = LogLevel.Debug,
            Message = "Validating operational state. Current state: {State}")]
        internal static partial void LogValidatingOperationalState(this ILogger logger, ConnectorState state);

        [LoggerMessage(
            EventId =  ConnectorLoggerEventId.NotInOperationalState,
            Level = LogLevel.Error,
            Message = "Connector is not in an operational state. Current state: {State}")]
        internal static partial void LogNotInOperationalState(this ILogger logger, ConnectorState state);

        #endregion

        #region Retry Policy Logging

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.RetryAttempt,
            Level = LogLevel.Debug,
            Message = "Retry attempt {AttemptNumber} of {MaxAttempts} for operation {OperationType}: {ErrorMessage}")]
        internal static partial void LogRetryAttempt(this ILogger logger, int attemptNumber, int maxAttempts, string operationType, string? errorMessage);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.RetrySucceeded,
            Level = LogLevel.Information,
            Message = "Operation {OperationType} succeeded after {AttemptCount} attempt(s)")]
        internal static partial void LogRetrySucceeded(this ILogger logger, string operationType, int attemptCount);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.RetryExhausted,
            Level = LogLevel.Error,
            Message = "All {MaxAttempts} retry attempt(s) exhausted for operation {OperationType}: {ErrorMessage}")]
        internal static partial void LogRetryExhausted(this ILogger logger, int maxAttempts, string operationType, string? errorMessage);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.CircuitBreakerOpened,
            Level = LogLevel.Warning,
            Message = "Circuit breaker opened for {OperationType} — failing fast until {BreakDuration} elapses")]
        internal static partial void LogCircuitBreakerOpened(this ILogger logger, string operationType, TimeSpan breakDuration);

        [LoggerMessage(
            EventId = ConnectorLoggerEventId.CircuitBreakerReset,
            Level = LogLevel.Information,
            Message = "Circuit breaker reset for {OperationType} —恢复正常 operation")]
        internal static partial void LogCircuitBreakerReset(this ILogger logger, string operationType);

        #endregion
    }
}
