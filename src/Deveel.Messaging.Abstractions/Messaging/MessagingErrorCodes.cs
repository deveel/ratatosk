//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    /// <summary>
    /// Defines standard error codes for general messaging operations
    /// that are not specific to any particular channel connector.
    /// </summary>
    /// <remarks>
    /// These error codes are used for messaging-level errors that occur
    /// outside the scope of channel connectors, such as message routing,
    /// serialization, and general configuration errors.
    /// Use connector-specific error code classes (e.g., <see cref="TwilioErrorCodes"/>)
    /// for errors that originate from a specific provider.
    /// </remarks>
    public static class MessagingErrorCodes
    {
        /// <summary>
        /// The error domain for general messaging errors.
        /// </summary>
        public const string ErrorDomain = "messaging";

        /// <summary>
        /// Indicates that a messaging operation failed due to an
        /// unspecified or unexpected error.
        /// </summary>
        public const string MessagingError = "MESSAGING_ERROR";

        /// <summary>
        /// Indicates that a message could not be routed to the
        /// intended recipient or channel.
        /// </summary>
        public const string MessageRoutingFailed = "MESSAGE_ROUTING_FAILED";

        /// <summary>
        /// Indicates that message serialization failed.
        /// </summary>
        public const string MessageSerializationFailed = "MESSAGE_SERIALIZATION_FAILED";

        /// <summary>
        /// Indicates that message deserialization failed.
        /// </summary>
        public const string MessageDeserializationFailed = "MESSAGE_DESERIALIZATION_FAILED";

        /// <summary>
        /// Indicates that a connector configuration is invalid or incomplete.
        /// </summary>
        public const string InvalidConfiguration = "INVALID_CONFIGURATION";

        /// <summary>
        /// Indicates that an unsupported message content type was encountered.
        /// </summary>
        public const string UnsupportedContentType = "UNSUPPORTED_CONTENT_TYPE";

        /// <summary>
        /// Indicates that no connector was found for the requested channel type.
        /// </summary>
        public const string ConnectorNotFound = "CONNECTOR_NOT_FOUND";

        /// <summary>
        /// Indicates that the webhook data provided is invalid or malformed.
        /// </summary>
        public const string InvalidWebhookData = "INVALID_WEBHOOK_DATA";

        /// <summary>
        /// Indicates that the recipient endpoint is invalid or missing.
        /// </summary>
        public const string InvalidRecipient = "INVALID_RECIPIENT";

        /// <summary>
        /// Indicates that required credentials are missing.
        /// </summary>
        public const string MissingCredentials = "MISSING_CREDENTIALS";

        /// <summary>
        /// Indicates that the provided credentials are invalid or expired.
        /// </summary>
        public const string InvalidCredentials = "INVALID_CREDENTIALS";

        /// <summary>
        /// Indicates that the sender endpoint is missing or not configured.
        /// </summary>
        public const string MissingSender = "MISSING_SENDER";

        /// <summary>
        /// Indicates that the message exceeds the maximum allowed length or size.
        /// </summary>
        public const string MessageTooLong = "MESSAGE_TOO_LONG";

        /// <summary>
        /// Indicates that the connection to the remote service failed.
        /// </summary>
        public const string ConnectionFailed = "CONNECTION_FAILED";

        /// <summary>
        /// Indicates that sending a message failed.
        /// </summary>
        public const string SendMessageFailed = "SEND_MESSAGE_FAILED";

        /// <summary>
        /// Indicates that the API rate limit has been exceeded.
        /// </summary>
        public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    }
}
