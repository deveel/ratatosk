namespace Deveel.Messaging
{
    public static class FacebookConnectionSettingsExtensions
    {
        public static string? GetPageAccessToken(this ConnectionSettings settings)
            => settings.GetParameter<string>(FacebookConnectionParameters.PageAccessToken);

        public static string? GetPageId(this ConnectionSettings settings)
            => settings.GetParameter<string>(FacebookConnectionParameters.PageId);

        public static string? GetWebhookUrl(this ConnectionSettings settings)
            => settings.GetParameter<string>(FacebookConnectionParameters.WebhookUrl);

        public static string? GetVerifyToken(this ConnectionSettings settings)
            => settings.GetParameter<string>(FacebookConnectionParameters.VerifyToken);
    }
}
