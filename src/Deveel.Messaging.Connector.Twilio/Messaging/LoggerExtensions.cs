//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides high-performance logging extensions for Twilio SMS Connector operations using source-generated logging methods.
    /// </summary>
    internal static partial class LoggerExtensions
    {
        #region SMS Sending Logging

        [LoggerMessage(
            EventId = 4202,
            Level = LogLevel.Information,
            Message = "SMS sent successfully. MessageId: {MessageId}, TwilioSid: {TwilioSid}, Status: {Status}")]
        internal static partial void LogSmsSent(this ILogger logger, string messageId, string twilioSid, string status);

        [LoggerMessage(
            EventId = 4204,
            Level = LogLevel.Debug,
            Message = "Using messaging service SID: {MessagingServiceSid}")]
        internal static partial void LogUsingMessagingServiceSid(this ILogger logger, string messagingServiceSid);

        [LoggerMessage(
            EventId = 4205,
            Level = LogLevel.Debug,
            Message = "Using from number: {FromNumber}")]
        internal static partial void LogUsingFromNumber(this ILogger logger, string fromNumber);

        #endregion

        #region Batch Sending Logging

        [LoggerMessage(
            EventId = 4301,
            Level = LogLevel.Debug,
            Message = "Sending batch of {MessageCount} SMS messages")]
        internal static partial void LogSendingBatch(this ILogger logger, int messageCount);

        [LoggerMessage(
            EventId = 4302,
            Level = LogLevel.Information,
            Message = "SMS batch sent successfully. BatchId: {BatchId}, SuccessCount: {SuccessCount}, FailureCount: {FailureCount}")]
        internal static partial void LogBatchSent(this ILogger logger, string batchId, int successCount, int failureCount);

        [LoggerMessage(
            EventId = 4303,
            Level = LogLevel.Error,
            Message = "Failed to send SMS batch")]
        internal static partial void LogBatchSendFailed(this ILogger logger, Exception exception);

        #endregion

        #region Configuration Logging

        [LoggerMessage(
            EventId = 4701,
            Level = LogLevel.Debug,
            Message = "Using Twilio Account SID: {AccountSid}")]
        internal static partial void LogUsingAccountSid(this ILogger logger, string accountSid);

        [LoggerMessage(
            EventId = 4702,
            Level = LogLevel.Debug,
            Message = "Status callback URL configured: {StatusCallbackUrl}")]
        internal static partial void LogStatusCallbackConfigured(this ILogger logger, string statusCallbackUrl);

        [LoggerMessage(
            EventId = 4703,
            Level = LogLevel.Debug,
            Message = "Default messaging service SID: {MessagingServiceSid}")]
        internal static partial void LogDefaultMessagingServiceSid(this ILogger logger, string messagingServiceSid);

        [LoggerMessage(
            EventId = 4704,
            Level = LogLevel.Debug,
            Message = "Default from number: {FromNumber}")]
        internal static partial void LogDefaultFromNumber(this ILogger logger, string fromNumber);

        [LoggerMessage(
            EventId = 4206,
            Level = LogLevel.Warning,
            Message = "Invalid media URL format: {MediaUrl}")]
        internal static partial void LogInvalidMediaUrl(this ILogger logger, string mediaUrl, Exception exception);

        [LoggerMessage(
            EventId = 4207,
            Level = LogLevel.Debug,
            Message = "Querying status for message {MessageId}")]
        internal static partial void LogQueryingMessageStatus(this ILogger logger, string messageId);

        #endregion

        #region WhatsApp Sending Logging

        [LoggerMessage(
            EventId = 4401,
            Level = LogLevel.Information,
            Message = "WhatsApp message sent successfully. MessageSid: {MessageSid}, Status: {Status}")]
        internal static partial void LogWhatsAppSent(this ILogger logger, string messageSid, string status);

        [LoggerMessage(
            EventId = 4402,
            Level = LogLevel.Warning,
            Message = "Failed to serialize template parameters to JSON for message {MessageId}")]
        internal static partial void LogTemplateParamsSerializationFailed(this ILogger logger, string messageId, Exception exception);

        #endregion
    }
}
