namespace Ratatosk
{
    /// <summary>
    /// Provides options for configuring the Twilio WhatsApp connector.
    /// </summary>
    public class TwilioWhatsAppOptions : IConnectorOptions
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

            return settings;
        }
    }
}
