namespace Deveel.Messaging
{
    public static class SendGridConnectionSettingsExtensions
    {
        public static string? GetApiKey(this ConnectionSettings settings)
            => settings.GetParameter<string>(SendGridConnectionParameters.ApiKey);

        public static bool? GetSandboxMode(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(SendGridConnectionParameters.SandboxMode);

        public static string? GetWebhookUrl(this ConnectionSettings settings)
            => settings.GetParameter<string>(SendGridConnectionParameters.WebhookUrl);

        public static bool? GetTrackingSettings(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(SendGridConnectionParameters.TrackingSettings);

        public static string? GetDefaultFromName(this ConnectionSettings settings)
            => settings.GetParameter<string>(SendGridConnectionParameters.DefaultFromName);

        public static string? GetDefaultReplyTo(this ConnectionSettings settings)
            => settings.GetParameter<string>(SendGridConnectionParameters.DefaultReplyTo);
    }
}
