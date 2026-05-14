namespace Deveel.Messaging
{
    /// <summary>
    /// Provides options for configuring the Twilio SMS connector.
    /// </summary>
    public class TwilioSmsOptions : IConnectorOptions
    {
        /// <summary>
        /// Gets or sets the Twilio account SID.
        /// </summary>
        public string? AccountSid { get; set; }

        /// <summary>
        /// Gets or sets the Twilio authentication token.
        /// </summary>
        public string? AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the webhook URL for incoming messages.
        /// </summary>
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Gets or sets the status callback URL for delivery status updates.
        /// </summary>
        public string? StatusCallback { get; set; }

        /// <summary>
        /// Gets or sets the validity period of the message in seconds.
        /// </summary>
        public int? ValidityPeriod { get; set; }

        /// <summary>
        /// Gets or sets the maximum price to spend on the message.
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Gets or sets the Messaging Service SID.
        /// </summary>
        public string? MessagingServiceSid { get; set; }

        /// <inheritdoc/>
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
