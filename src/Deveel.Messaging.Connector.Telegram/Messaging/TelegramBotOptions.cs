namespace Deveel.Messaging
{
    public class TelegramBotOptions : IConnectorOptions
    {
        public string? BotToken { get; set; }

        public string? WebhookUrl { get; set; }

        public string? SecretToken { get; set; }

        public bool DisableWebPagePreview { get; set; }

        public bool DisableNotification { get; set; }

        public string ParseMode { get; set; } = "Markdown";

        public int MaxRetries { get; set; } = 3;

        public int TimeoutSeconds { get; set; } = 60;

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
