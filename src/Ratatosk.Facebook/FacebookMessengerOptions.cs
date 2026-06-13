namespace Ratatosk
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

        /// <summary>
        /// Gets or sets the timeout for send operations.
        /// </summary>
        public TimeSpan? SendTimeout { get; set; }

        /// <summary>
        /// Gets or sets the timeout for receive operations.
        /// </summary>
        public TimeSpan? ReceiveTimeout { get; set; }

        /// <summary>
        /// Gets or sets the timeout for status query operations.
        /// </summary>
        public TimeSpan? StatusQueryTimeout { get; set; }

        /// <summary>
        /// Gets or sets whether timeout errors should be retried.
        /// </summary>
        public bool? RetryOnTimeout { get; set; }

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

            if (SendTimeout.HasValue)
                settings.SetParameter(TimeoutSettingsKeys.SendTimeout, SendTimeout.Value);
            if (ReceiveTimeout.HasValue)
                settings.SetParameter(TimeoutSettingsKeys.ReceiveTimeout, ReceiveTimeout.Value);
            if (StatusQueryTimeout.HasValue)
                settings.SetParameter(TimeoutSettingsKeys.StatusQueryTimeout, StatusQueryTimeout.Value);
            if (RetryOnTimeout.HasValue)
                settings.SetParameter(TimeoutSettingsKeys.RetryOnTimeout, RetryOnTimeout.Value);

            return settings;
        }
    }
}
