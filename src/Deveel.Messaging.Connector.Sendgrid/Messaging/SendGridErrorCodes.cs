//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Deveel.Messaging
{
    /// <summary>
    /// Provides error codes specific to SendGrid connector operations.
    /// </summary>
    public static class SendGridErrorCodes
    {
        /// <summary>
        /// The error domain for SendGrid connector errors.
        /// </summary>
        public const string ErrorDomain = "SendGrid";

        /// <summary>
        /// Error code indicating missing SendGrid API key.
        /// </summary>

        /// <summary>
        /// Error code indicating invalid SendGrid API key or authentication failure.
        /// </summary>

        /// <summary>
        /// Error code indicating connection failed.
        /// </summary>

        /// <summary>
        /// Error code indicating message sending failed.
        /// </summary>

        /// <summary>
        /// Error code indicating invalid email address.
        /// </summary>
        public const string InvalidEmailAddress = "INVALID_EMAIL_ADDRESS";

        /// <summary>
        /// Error code indicating missing email content.
        /// </summary>
        public const string MissingEmailContent = "MISSING_EMAIL_CONTENT";

        /// <summary>
        /// Error code indicating API rate limit exceeded.
        /// </summary>

        /// <summary>
        /// Error code indicating missing sender email address.
        /// </summary>
    }
}
