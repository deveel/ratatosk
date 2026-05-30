//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Ratatosk;

static partial class LoggerExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Sender '{SenderName}' resolved from cache.")]
    public static partial void LogSenderResolvedFromCache(this ILogger logger, string senderName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Sender '{SenderName}' not found in registry.")]
    public static partial void LogSenderNotFoundInRegistry(this ILogger logger, string senderName);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "No sender found for endpoint '{Address}' (type: {EndpointType}).")]
    public static partial void LogNoSenderFoundForEndpoint(this ILogger logger, string address, EndpointType endpointType);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Sender '{SenderName}' found but is inactive.")]
    public static partial void LogSenderFoundButInactive(this ILogger logger, string senderName);
}
