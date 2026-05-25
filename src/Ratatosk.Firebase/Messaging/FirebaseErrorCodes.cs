//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace Ratatosk
{
    /// <summary>
    /// Defines error codes specific to Firebase Cloud Messaging (FCM) connector operations.
    /// </summary>
    /// <remarks>
    /// This class provides Firebase-specific error codes that complement the standard
    /// connector error codes defined in <see cref="ConnectorErrorCodes"/>. These codes
    /// are used to identify specific failure scenarios related to Firebase Admin SDK integration.
    /// </remarks>
    public static class FirebaseErrorCodes
    {
        /// <summary>
        /// The error domain for Firebase Cloud Messaging connector errors.
        /// </summary>
        public const string ErrorDomain = "Firebase";

        /// <summary>
        /// Indicates that the Firebase service account key is missing or invalid.
        /// </summary>
        public const string MissingServiceAccountKey = "MISSING_SERVICE_ACCOUNT_KEY";

        /// <summary>
        /// Indicates that the Firebase project ID is missing.
        /// </summary>
        public const string MissingProjectId = "MISSING_PROJECT_ID";

        /// <summary>
        /// Indicates that Firebase initialization failed.
        /// </summary>
        public const string InitializationFailed = "INITIALIZATION_FAILED";

        /// <summary>
        /// Indicates that the connection test to Firebase failed.
        /// </summary>

        /// <summary>
        /// Indicates that sending a Firebase push message failed.
        /// </summary>

        /// <summary>
        /// Indicates that the message could not be sent because the
        /// registration token is not registered or invalid.
        /// </summary>
        public const string UnregisteredToken = "UNREGISTERED_TOKEN";

        /// <summary>
        /// Indicates that the request contains an invalid argument.
        /// </summary>
        public const string InvalidArgument = "INVALID_ARGUMENT";

        /// <summary>
        /// Indicates that the sender ID does not match the registered token.
        /// </summary>
        public const string SenderIdMismatch = "SENDER_ID_MISMATCH";

        /// <summary>
        /// Indicates that the FCM quota has been exceeded.
        /// </summary>

        /// <summary>
        /// Indicates that a third-party authentication error occurred.
        /// </summary>
        public const string ThirdPartyAuthError = "THIRD_PARTY_AUTH_ERROR";

        /// <summary>
        /// Indicates that the FCM service is temporarily unavailable.
        /// </summary>
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";

        /// <summary>
        /// Indicates that an internal error occurred in the Firebase service.
        /// </summary>
        public const string InternalError = "INTERNAL_ERROR";
    }
}
