namespace Deveel.Messaging
{
    public static class TelegramConnectionSettingsDefaults
    {
        public const bool DisableWebPagePreview = false;
        public const bool DisableNotification = false;
        public const string ParseMode = "Markdown";
        public const int MaxRetries = 3;
        public const int TimeoutSeconds = 30;
        public const int MaxConnections = 40;
        public const bool DropPendingUpdates = false;
    }
}
