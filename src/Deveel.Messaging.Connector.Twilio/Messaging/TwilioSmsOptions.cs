namespace Deveel.Messaging
{
    public class TwilioSmsOptions : IConnectorOptions
    {
        public string? AccountSid { get; set; }

        public string? AuthToken { get; set; }

        public string? WebhookUrl { get; set; }

        public string? StatusCallback { get; set; }

        public int? ValidityPeriod { get; set; }

        public decimal? MaxPrice { get; set; }

        public string? MessagingServiceSid { get; set; }

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
            if (ValidityPeriod.HasValue)
                settings.SetParameter(TwilioConnectionParameters.ValidityPeriod, ValidityPeriod.Value);
            if (MaxPrice.HasValue)
                settings.SetParameter(TwilioConnectionParameters.MaxPrice, MaxPrice.Value);
            if (!string.IsNullOrWhiteSpace(MessagingServiceSid))
                settings.SetParameter(TwilioConnectionParameters.MessagingServiceSid, MessagingServiceSid);

            return settings;
        }
    }
}
