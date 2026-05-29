namespace Ratatosk
{
    /// <summary>
    /// Defines constant keys for the connection parameters used
    /// to configure the Telegram Bot connector.
    /// </summary>
    public static class TelegramConnectionParameters
    {
        /// <summary>
        /// The key for the bot token parameter.
        /// </summary>
        public const string BotToken = "BotToken";

        /// <summary>
        /// The key for the webhook URL parameter.
        /// </summary>
        public const string WebhookUrl = "WebhookUrl";

        /// <summary>
        /// The key for the secret token parameter.
        /// </summary>
        public const string SecretToken = "SecretToken";

        /// <summary>
        /// The key for the disable web page preview parameter.
        /// </summary>
        public const string DisableWebPagePreview = "DisableWebPagePreview";

        /// <summary>
        /// The key for the disable notification parameter.
        /// </summary>
        public const string DisableNotification = "DisableNotification";

        /// <summary>
        /// The key for the parse mode parameter.
        /// </summary>
        public const string ParseMode = "ParseMode";

        /// <summary>
        /// The key for the max retries parameter.
        /// </summary>
        public const string MaxRetries = "MaxRetries";

        /// <summary>
        /// The key for the timeout in seconds parameter.
        /// </summary>
        public const string TimeoutSeconds = "TimeoutSeconds";

        /// <summary>
        /// The key for the max connections parameter.
        /// </summary>
        public const string MaxConnections = "MaxConnections";

        /// <summary>
        /// The key for the drop pending updates parameter.
        /// </summary>
        public const string DropPendingUpdates = "DropPendingUpdates";

        /// <summary>
        /// The key for the default chat identifier parameter.
        /// </summary>
        public const string DefaultChatId = "DefaultChatId";
    }
}
