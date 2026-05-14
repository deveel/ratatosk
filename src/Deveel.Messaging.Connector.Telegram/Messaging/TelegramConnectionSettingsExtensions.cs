namespace Deveel.Messaging
{
    public static class TelegramConnectionSettingsExtensions
    {
        public static string? GetBotToken(this ConnectionSettings settings)
            => settings.GetParameter<string>(TelegramConnectionParameters.BotToken);

        public static string? GetWebhookUrl(this ConnectionSettings settings)
            => settings.GetParameter<string>(TelegramConnectionParameters.WebhookUrl);

        public static string? GetSecretToken(this ConnectionSettings settings)
            => settings.GetParameter<string>(TelegramConnectionParameters.SecretToken);

        public static bool? GetDisableWebPagePreview(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(TelegramConnectionParameters.DisableWebPagePreview);

        public static bool? GetDisableNotification(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(TelegramConnectionParameters.DisableNotification);

        public static string? GetParseMode(this ConnectionSettings settings)
            => settings.GetParameter<string>(TelegramConnectionParameters.ParseMode);

        public static int? GetMaxRetries(this ConnectionSettings settings)
            => settings.GetParameter<int?>(TelegramConnectionParameters.MaxRetries);

        public static int? GetTimeoutSeconds(this ConnectionSettings settings)
            => settings.GetParameter<int?>(TelegramConnectionParameters.TimeoutSeconds);

        public static int? GetMaxConnections(this ConnectionSettings settings)
            => settings.GetParameter<int?>(TelegramConnectionParameters.MaxConnections);

        public static bool? GetDropPendingUpdates(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(TelegramConnectionParameters.DropPendingUpdates);

        public static string? GetDefaultChatId(this ConnectionSettings settings)
            => settings.GetParameter<string>(TelegramConnectionParameters.DefaultChatId);
    }
}
