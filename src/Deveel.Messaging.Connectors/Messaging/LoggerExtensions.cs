//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Deveel.Messaging
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
    }
}
