namespace Deveel.Messaging
{
    /// <summary>
    /// Provides options for configuring the Facebook Messenger connector.
    /// </summary>
    public class FacebookMessengerOptions : IConnectorOptions
    {
        /// <summary>
        /// Gets or sets the page access token used to authenticate
        /// with the Facebook Messenger API.
        /// </summary>
        public string? PageAccessToken { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the Facebook page.
        /// </summary>
        public string? PageId { get; set; }

        /// <summary>
        /// Gets or sets the webhook URL for receiving messages.
        /// </summary>
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Gets or sets the verify token used to validate webhook subscriptions.
        /// </summary>
        public string? VerifyToken { get; set; }

        /// <inheritdoc/>
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
