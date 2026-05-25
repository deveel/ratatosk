namespace Ratatosk
{
    /// <summary>
    /// Provides options for configuring the Telegram Bot connector.
    /// </summary>
    public class TelegramBotOptions : IConnectorOptions
    {
        /// <summary>
        /// Gets or sets the bot token used to authenticate with the Telegram API.
        /// </summary>
        public string? BotToken { get; set; }

        /// <summary>
        /// Gets or sets the webhook URL for receiving updates.
        /// </summary>
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Gets or sets the secret token used to validate webhook requests.
        /// </summary>
        public string? SecretToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether web page previews
        /// should be disabled in messages.
        /// </summary>
        public bool DisableWebPagePreview { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notifications
        /// should be disabled for messages.
        /// </summary>
        public bool DisableNotification { get; set; }

        /// <summary>
        /// Gets or sets the parse mode for message text (e.g., Markdown, HTML).
        /// </summary>
        public string ParseMode { get; set; } = "Markdown";

        /// <summary>
        /// Gets or sets the maximum number of retry attempts for API calls.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the timeout in seconds for API calls.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 60;

        /// <inheritdoc/>
        public ConnectionSettings ToConnectionSettings()
        {
            var settings = new ConnectionSettings();

            if (!string.IsNullOrWhiteSpace(BotToken))
                settings.SetParameter(TelegramConnectionParameters.BotToken, BotToken);
            if (!string.IsNullOrWhiteSpace(WebhookUrl))
                settings.SetParameter(TelegramConnectionParameters.WebhookUrl, WebhookUrl);
            if (!string.IsNullOrWhiteSpace(SecretToken))
                settings.SetParameter(TelegramConnectionParameters.SecretToken, SecretToken);
            if (DisableWebPagePreview)
                settings.SetParameter(TelegramConnectionParameters.DisableWebPagePreview, true);
            if (DisableNotification)
                settings.SetParameter(TelegramConnectionParameters.DisableNotification, true);
            if (!string.Equals(ParseMode, TelegramConnectionSettingsDefaults.ParseMode))
                settings.SetParameter(TelegramConnectionParameters.ParseMode, ParseMode);
            if (MaxRetries != TelegramConnectionSettingsDefaults.MaxRetries)
                settings.SetParameter(TelegramConnectionParameters.MaxRetries, MaxRetries);
            if (TimeoutSeconds != 60)
                settings.SetParameter(TelegramConnectionParameters.TimeoutSeconds, TimeoutSeconds);

            return settings;
        }
    }
}
