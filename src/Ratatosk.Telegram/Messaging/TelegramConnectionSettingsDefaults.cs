namespace Ratatosk
{
    /// <summary>
    /// Defines default values for the Telegram Bot connection settings.
    /// </summary>
    public static class TelegramConnectionSettingsDefaults
    {
        /// <summary>
        /// The default value for disabling web page preview.
        /// </summary>
        public const bool DisableWebPagePreview = false;

        /// <summary>
        /// The default value for disabling notification.
        /// </summary>
        public const bool DisableNotification = false;

        /// <summary>
        /// The default parse mode for message text.
        /// </summary>
        public const string ParseMode = "Markdown";

        /// <summary>
        /// The default maximum number of retry attempts.
        /// </summary>
        public const int MaxRetries = 3;

        /// <summary>
        /// The default timeout in seconds for API calls.
        /// </summary>
        public const int TimeoutSeconds = 30;

        /// <summary>
        /// The default maximum number of webhook connections.
        /// </summary>
        public const int MaxConnections = 40;

        /// <summary>
        /// The default value for dropping pending updates on startup.
        /// </summary>
        public const bool DropPendingUpdates = false;
    }
}
