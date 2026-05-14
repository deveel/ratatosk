namespace Deveel.Messaging
{
    public class TwilioWhatsAppOptions : IConnectorOptions
    {
        public string? AccountSid { get; set; }

        public string? AuthToken { get; set; }

        public string? WebhookUrl { get; set; }

        public string? StatusCallback { get; set; }

        public ConnectionSettings ToConnectionSettings()
        {
            var settings = new ConnectionSettings();

            if (!string.IsNullOrWhiteSpace(AccountSid))
                settings.SetParameter(TwilioConnectionParameters.AccountSid, AccountSid);
            if (!string.IsNullOrWhiteSpace(AuthToken))
                settings.SetParameter(TwilioConnectionParameters.AuthToken, AuthToken);
            if (!string.IsNullOrWhiteSpace(WebhookUrl))
                settings.SetParameter(TwilioConnectionParameters.WebhookUrl, WebhookUrl);
            if (!string.IsNullOrWhiteSpace(StatusCallback))
                settings.SetParameter(TwilioConnectionParameters.StatusCallback, StatusCallback);

            return settings;
        }
    }
}
