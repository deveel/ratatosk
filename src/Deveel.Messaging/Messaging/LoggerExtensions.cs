using Microsoft.Extensions.Logging;

namespace Deveel.Messaging
{
    internal static partial class LoggerExtensions
    {
        #region Channel Resolution

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientResolvingChannel,
            Level = LogLevel.Debug,
            Message = "Resolving connector for channel '{ChannelName}'")]
        public static partial void LogResolvingChannel(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientChannelResolved,
            Level = LogLevel.Debug,
            Message = "Connector resolved for channel '{ChannelName}'")]
        public static partial void LogChannelResolved(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientChannelNotFound,
            Level = LogLevel.Warning,
            Message = "No connector registered for channel '{ChannelName}'")]
        public static partial void LogChannelNotFound(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientInitializingConnector,
            Level = LogLevel.Information,
            Message = "Auto-initializing connector for channel '{ChannelName}'")]
        public static partial void LogInitializingConnector(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientConnectorInitialized,
            Level = LogLevel.Information,
            Message = "Connector initialized for channel '{ChannelName}'")]
        public static partial void LogConnectorInitialized(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientConnectorInitializationFailed,
            Level = LogLevel.Error,
            Message = "Failed to initialize connector for channel '{ChannelName}': {Error}")]
        public static partial void LogConnectorInitializationFailed(this ILogger logger, string channelName, string? error);

        #endregion

        #region Sending

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientSendingMessage,
            Level = LogLevel.Debug,
            Message = "Sending message through channel '{ChannelName}'")]
        public static partial void LogSendingMessage(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientMessageSent,
            Level = LogLevel.Information,
            Message = "Message sent through channel '{ChannelName}'")]
        public static partial void LogMessageSent(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientMessageSendFailed,
            Level = LogLevel.Error,
            Message = "Failed to send message through channel '{ChannelName}': {Error}")]
        public static partial void LogMessageSendFailed(this ILogger logger, string channelName, string? error);

        #endregion

        #region Receiving

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientReceivingMessage,
            Level = LogLevel.Debug,
            Message = "Receiving messages from channel '{ChannelName}'")]
        public static partial void LogReceivingMessage(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientMessageReceived,
            Level = LogLevel.Information,
            Message = "Messages received from channel '{ChannelName}'")]
        public static partial void LogMessageReceived(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientMessageReceiveFailed,
            Level = LogLevel.Error,
            Message = "Failed to receive messages from channel '{ChannelName}': {Error}")]
        public static partial void LogMessageReceiveFailed(this ILogger logger, string channelName, string? error);

        #endregion

        #region Status

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientReadingStatus,
            Level = LogLevel.Debug,
            Message = "Reading status of channel '{ChannelName}'")]
        public static partial void LogReadingStatus(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientStatusRead,
            Level = LogLevel.Information,
            Message = "Status read for channel '{ChannelName}'")]
        public static partial void LogStatusRead(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientStatusReadFailed,
            Level = LogLevel.Error,
            Message = "Failed to read status for channel '{ChannelName}': {Error}")]
        public static partial void LogStatusReadFailed(this ILogger logger, string channelName, string? error);

        #endregion

        #region Message Status

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientReceivingMessageStatus,
            Level = LogLevel.Debug,
            Message = "Receiving message status for channel '{ChannelName}'")]
        public static partial void LogReceivingMessageStatus(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientMessageStatusReceived,
            Level = LogLevel.Information,
            Message = "Message status received for channel '{ChannelName}'")]
        public static partial void LogMessageStatusReceived(this ILogger logger, string channelName);

        [LoggerMessage(
            EventId = MessagingClientLoggerEventId.ClientMessageStatusReceiveFailed,
            Level = LogLevel.Error,
            Message = "Failed to receive message status for channel '{ChannelName}': {Error}")]
        public static partial void LogMessageStatusReceiveFailed(this ILogger logger, string channelName, string? error);

        #endregion
    }
}
