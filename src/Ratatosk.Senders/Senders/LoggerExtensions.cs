//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Ratatosk.Senders;

static partial class LoggerExtensions
{
    [LoggerMessage(EventId = SenderLoggerEventId.SenderResolvedFromCache, Level = LogLevel.Debug, Message = "Sender '{SenderName}' resolved from cache.")]
    public static partial void LogSenderResolvedFromCache(this ILogger logger, string senderName);

    [LoggerMessage(EventId = SenderLoggerEventId.SenderNotFoundInRegistry, Level = LogLevel.Warning, Message = "Sender '{SenderName}' not found in registry.")]
    public static partial void LogSenderNotFoundInRegistry(this ILogger logger, string senderName);

    [LoggerMessage(EventId = SenderLoggerEventId.NoSenderFoundForEndpoint, Level = LogLevel.Debug, Message = "No sender found for endpoint '{Address}' (type: {EndpointType}).")]
    public static partial void LogNoSenderFoundForEndpoint(this ILogger logger, string address, EndpointType endpointType);

    [LoggerMessage(EventId = SenderLoggerEventId.SenderFoundButInactive, Level = LogLevel.Warning, Message = "Sender '{SenderName}' found but is inactive.")]
    public static partial void LogSenderFoundButInactive(this ILogger logger, string senderName);

    [LoggerMessage(EventId = SenderLoggerEventId.FailedToFindSenderByName, Level = LogLevel.Error, Message = "Failed to find sender by name '{Name}'.")]
    public static partial void LogFailedToFindSenderByName(this ILogger logger, Exception exception, string name);

    [LoggerMessage(EventId = SenderLoggerEventId.FailedToFindSenderByEndpoint, Level = LogLevel.Error, Message = "Failed to find sender by endpoint '{Address}' ({EndpointType}).")]
    public static partial void LogFailedToFindSenderByEndpoint(this ILogger logger, Exception exception, string address, EndpointType endpointType);

    [LoggerMessage(EventId = SenderLoggerEventId.SenderResolvedFromCacheByEndpoint, Level = LogLevel.Debug, Message = "Sender for endpoint '{Address}' (type: {EndpointType}) resolved from cache.")]
    public static partial void LogSenderResolvedFromCacheByEndpoint(this ILogger logger, string address, EndpointType endpointType);

    [LoggerMessage(EventId = SenderLoggerEventId.FailedToRetrieveAllActiveSenders, Level = LogLevel.Error, Message = "Failed to retrieve all active senders.")]
    public static partial void LogFailedToRetrieveAllActiveSenders(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = SenderLoggerEventId.FailedToSetActiveState, Level = LogLevel.Error, Message = "Failed to set active state for sender '{Id}'.")]
    public static partial void LogFailedToSetActiveState(this ILogger logger, Exception exception, string id);
}
