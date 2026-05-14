//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides high-performance logging extensions for SendGrid Email Connector operations using source-generated logging methods.
    /// </summary>
    internal static partial class LoggerExtensions
    {
        #region Email Sending Logging

        [LoggerMessage(
            EventId = 3204,
            Level = LogLevel.Warning,
            Message = "SendGrid API returned non-success status: {StatusCode} for message {MessageId}")]
        internal static partial void LogApiNonSuccessStatus(this ILogger logger, int statusCode, string messageId);

        #endregion

        #region Configuration Logging

        [LoggerMessage(
            EventId = 3601,
            Level = LogLevel.Debug,
            Message = "Sandbox mode is {SandboxMode}")]
        internal static partial void LogSandboxMode(this ILogger logger, bool sandboxMode);

        [LoggerMessage(
            EventId = 3602,
            Level = LogLevel.Debug,
            Message = "Tracking settings enabled: {TrackingEnabled}")]
        internal static partial void LogTrackingSettings(this ILogger logger, bool trackingEnabled);

        [LoggerMessage(
            EventId = 3603,
            Level = LogLevel.Debug,
            Message = "Webhook URL configured: {WebhookUrl}")]
        internal static partial void LogWebhookConfigured(this ILogger logger, string webhookUrl);

        [LoggerMessage(
            EventId = 3604,
            Level = LogLevel.Debug,
            Message = "Default from name set: {DefaultFromName}")]
        internal static partial void LogDefaultFromName(this ILogger logger, string defaultFromName);

        [LoggerMessage(
            EventId = 3605,
            Level = LogLevel.Debug,
            Message = "Default reply-to address set: {DefaultReplyTo}")]
        internal static partial void LogDefaultReplyTo(this ILogger logger, string defaultReplyTo);

        [LoggerMessage(
            EventId = 3201,
            Level = LogLevel.Information,
            Message = "Email message sent successfully. MessageId: {MessageId}, StatusCode: {StatusCode}")]
        internal static partial void LogEmailSent(this ILogger logger, string messageId, string statusCode);

        [LoggerMessage(
            EventId = 3202,
            Level = LogLevel.Error,
            Message = "Failed to send email. StatusCode: {StatusCode}, Error: {Error}")]
        internal static partial void LogEmailSendFailed(this ILogger logger, string statusCode, string error);

        [LoggerMessage(
            EventId = 3203,
            Level = LogLevel.Warning,
            Message = "Failed to extract message ID from SendGrid response")]
        internal static partial void LogExtractMessageIdFailed(this ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = 3205,
            Level = LogLevel.Debug,
            Message = "Querying status for message {MessageId}")]
        internal static partial void LogQueryingMessageStatus(this ILogger logger, string messageId);

        #endregion

        #region Message Building Logging

        [LoggerMessage(
            EventId = 3301,
            Level = LogLevel.Warning,
            Message = "Failed to parse CustomArgs as JSON: {CustomArgs}")]
        internal static partial void LogCustomArgsParseFailed(this ILogger logger, string customArgs, Exception exception);

        #endregion
    }
}
