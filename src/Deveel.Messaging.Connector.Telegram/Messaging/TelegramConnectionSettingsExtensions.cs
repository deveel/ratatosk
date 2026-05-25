namespace Deveel.Messaging
{
    /// <summary>
    /// Provides extension methods for <see cref="ConnectionSettings"/> to access Telegram-specific parameters.
    /// </summary>
    public static class TelegramConnectionSettingsExtensions
    {
        /// <summary>
        /// Gets the Telegram bot token from the connection settings.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The bot token, or <c>null</c> if not configured.</returns>
        public static string? GetBotToken(this ConnectionSettings settings)
            => settings.GetParameter<string>(TelegramConnectionParameters.BotToken);

        /// <summary>
        /// Gets the webhook URL configured for Telegram bot callbacks.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The webhook URL, or <c>null</c> if not configured.</returns>
        public static string? GetWebhookUrl(this ConnectionSettings settings)
            => settings.GetParameter<string>(TelegramConnectionParameters.WebhookUrl);

        /// <summary>
        /// Gets the secret token for securing Telegram webhook requests.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The secret token, or <c>null</c> if not configured.</returns>
        public static string? GetSecretToken(this ConnectionSettings settings)
            => settings.GetParameter<string>(TelegramConnectionParameters.SecretToken);

        /// <summary>
        /// Gets whether the web page preview is disabled in messages.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns><c>true</c> if disabled, <c>false</c> if enabled, or <c>null</c> if not configured.</returns>
        public static bool? GetDisableWebPagePreview(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(TelegramConnectionParameters.DisableWebPagePreview);

        /// <summary>
        /// Gets whether notifications are disabled for messages.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns><c>true</c> if disabled, <c>false</c> if enabled, or <c>null</c> if not configured.</returns>
        public static bool? GetDisableNotification(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(TelegramConnectionParameters.DisableNotification);

        /// <summary>
        /// Gets the parse mode (e.g., HTML, Markdown) for Telegram messages.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The parse mode, or <c>null</c> if not configured.</returns>
        public static string? GetParseMode(this ConnectionSettings settings)
            => settings.GetParameter<string>(TelegramConnectionParameters.ParseMode);

        /// <summary>
        /// Gets the maximum number of retries when sending messages.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The maximum retries, or <c>null</c> if not configured.</returns>
        public static int? GetMaxRetries(this ConnectionSettings settings)
            => settings.GetParameter<int?>(TelegramConnectionParameters.MaxRetries);

        /// <summary>
        /// Gets the timeout in seconds for HTTP requests.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The timeout in seconds, or <c>null</c> if not configured.</returns>
        public static int? GetTimeoutSeconds(this ConnectionSettings settings)
            => settings.GetParameter<int?>(TelegramConnectionParameters.TimeoutSeconds);

        /// <summary>
        /// Gets the maximum number of connections allowed.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The maximum connections, or <c>null</c> if not configured.</returns>
        public static int? GetMaxConnections(this ConnectionSettings settings)
            => settings.GetParameter<int?>(TelegramConnectionParameters.MaxConnections);

        /// <summary>
        /// Gets whether pending updates should be dropped when starting the bot.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns><c>true</c> to drop pending updates, or <c>null</c> if not configured.</returns>
        public static bool? GetDropPendingUpdates(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(TelegramConnectionParameters.DropPendingUpdates);

        /// <summary>
        /// Gets the default chat identifier for sending messages.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The default chat ID, or <c>null</c> if not configured.</returns>
        public static string? GetDefaultChatId(this ConnectionSettings settings)
            => settings.GetParameter<string>(TelegramConnectionParameters.DefaultChatId);
    }
}
