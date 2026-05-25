//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Ratatosk
{
    /// <summary>
    /// Provides high-performance logging extensions for Firebase Push Connector operations using source-generated logging methods.
    /// </summary>
    internal static partial class LoggerExtensions
    {
        #region Initialization Logging

        [LoggerMessage(
            EventId = 1004,
            Level = LogLevel.Error,
            Message = "Failed to shut down Firebase push connector")]
        internal static partial void LogShutdownFailed(this ILogger logger, Exception exception);

        #endregion

        #region Message Sending Logging

        [LoggerMessage(
            EventId = 1201,
            Level = LogLevel.Debug,
            Message = "Sending push notification to {ReceiverAddress}")]
        internal static partial void LogSendingPushNotification(this ILogger logger, string? receiverAddress);

        #endregion

        #region Firebase Authentication Logging

        [LoggerMessage(
            EventId = 1401,
            Level = LogLevel.Debug,
            Message = "Obtaining Firebase service account credential")]
        internal static partial void LogObtainingServiceAccountCredential(this ILogger logger);

        [LoggerMessage(
            EventId = 1402,
            Level = LogLevel.Debug,
            Message = "Using service account key file")]
        internal static partial void LogUsingServiceAccountKeyFile(this ILogger logger);

        [LoggerMessage(
            EventId = 1403,
            Level = LogLevel.Debug,
            Message = "Using service account key JSON string")]
        internal static partial void LogUsingServiceAccountKeyJson(this ILogger logger);

        [LoggerMessage(
            EventId = 1404,
            Level = LogLevel.Information,
            Message = "Successfully prepared Firebase service account credential")]
        internal static partial void LogFirebaseServiceAccountCredentialPrepared(this ILogger logger);

        [LoggerMessage(
            EventId = 1405,
            Level = LogLevel.Error,
            Message = "Error preparing Firebase service account credential")]
        internal static partial void LogFirebaseServiceAccountCredentialError(this ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = 1406,
            Level = LogLevel.Debug,
            Message = "Service account credential is still valid, no refresh needed")]
        internal static partial void LogServiceAccountCredentialValid(this ILogger logger);

        [LoggerMessage(
            EventId = 1407,
            Level = LogLevel.Debug,
            Message = "Service account credential is invalid, obtaining new credential")]
        internal static partial void LogServiceAccountCredentialInvalid(this ILogger logger);

        #endregion
    }
}
