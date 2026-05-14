namespace Deveel.Messaging
{
    public class FacebookMessengerOptions : IConnectorOptions
    {
        public string? PageAccessToken { get; set; }

        public string? PageId { get; set; }

        public string? WebhookUrl { get; set; }

        public string? VerifyToken { get; set; }

        public ConnectionSettings ToConnectionSettings()
        {
            var settings = new ConnectionSettings();

            if (!string.IsNullOrWhiteSpace(PageAccessToken))
                settings.SetParameter(FacebookConnectionParameters.PageAccessToken, PageAccessToken);
            if (!string.IsNullOrWhiteSpace(PageId))
                settings.SetParameter(FacebookConnectionParameters.PageId, PageId);
            if (!string.IsNullOrWhiteSpace(WebhookUrl))
                settings.SetParameter(FacebookConnectionParameters.WebhookUrl, WebhookUrl);
            if (!string.IsNullOrWhiteSpace(VerifyToken))
                settings.SetParameter(FacebookConnectionParameters.VerifyToken, VerifyToken);

            return settings;
        }
    }
}
