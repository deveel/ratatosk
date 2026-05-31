namespace Ratatosk.Senders
{
    /// <summary>
    /// Provides standardized error codes for sender-related operations.
    /// </summary>
    public static class SenderErrorCodes
    {
        /// <summary>
        /// The error domain prefix for sender errors.
        /// </summary>
        public const string ErrorDomain = "sender";

        /// <summary>
        /// Indicates that the requested sender was not found.
        /// </summary>
        public const string SenderNotFound = "SENDER_NOT_FOUND";

        /// <summary>
        /// Indicates that the sender could not be created.
        /// </summary>
        public const string SenderNotCreated = "SENDER_NOT_CREATED";

        /// <summary>
        /// Indicates that the sender could not be updated.
        /// </summary>
        public const string SenderNotUpdated = "SENDER_NOT_UPDATED";

        /// <summary>
        /// Indicates that the sender could not be deleted.
        /// </summary>
        public const string SenderNotDeleted = "SENDER_NOT_DELETED";

        /// <summary>
        /// Indicates that the underlying store does not support
        /// the query operations required by the sender manager.
        /// </summary>
        public const string QueryNotSupported = "QUERY_NOT_SUPPORTED";

        /// <summary>
        /// A general error occurred during a sender operation.
        /// </summary>
        public const string SenderError = "SENDER_ERROR";
    }
}
