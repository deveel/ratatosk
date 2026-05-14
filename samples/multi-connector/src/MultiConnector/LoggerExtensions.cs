using Microsoft.Extensions.Logging;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Message {MessageId} sent via {Channel}: local={LocalId} remote={RemoteId} status={Status}")]
    public static partial void LogMessageSent(this ILogger logger, string messageId, string channel, string localId, string remoteId, string status);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Status update received via {Channel}: id={Id} status={Status}")]
    public static partial void LogStatusUpdateReceived(this ILogger logger, string channel, string id, string status);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Received {Count} message(s) via {Channel}")]
    public static partial void LogMessagesReceived(this ILogger logger, int count, string channel);
}
