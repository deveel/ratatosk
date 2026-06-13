namespace Ratatosk
{
    /// <summary>
    /// Provides options for configuring the SendGrid email connector.
    /// </summary>
    public class SendGridEmailOptions : IConnectorOptions
    {
        /// <summary>
        /// Gets or sets the SendGrid API key.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sandbox mode is enabled.
        /// </summary>
        public bool SandboxMode { get; set; }

        /// <summary>
        /// Gets or sets the webhook URL for receiving events.
        /// </summary>
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tracking settings are enabled.
        /// </summary>
        public bool TrackingSettings { get; set; }

        /// <summary>
        /// Gets or sets the default from name for emails.
        /// </summary>
        public string? DefaultFromName { get; set; }

        /// <summary>
        /// Gets or sets the default reply-to address for emails.
        /// </summary>
        public string? DefaultReplyTo { get; set; }

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

            if (!string.IsNullOrWhiteSpace(ApiKey))
                settings.SetParameter(SendGridConnectionParameters.ApiKey, ApiKey);
            if (SandboxMode)
                settings.SetParameter(SendGridConnectionParameters.SandboxMode, true);
            if (!string.IsNullOrWhiteSpace(WebhookUrl))
                settings.SetParameter(SendGridConnectionParameters.WebhookUrl, WebhookUrl);
            if (TrackingSettings)
                settings.SetParameter(SendGridConnectionParameters.TrackingSettings, true);
            if (!string.IsNullOrWhiteSpace(DefaultFromName))
                settings.SetParameter(SendGridConnectionParameters.DefaultFromName, DefaultFromName);
            if (!string.IsNullOrWhiteSpace(DefaultReplyTo))
                settings.SetParameter(SendGridConnectionParameters.DefaultReplyTo, DefaultReplyTo);

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
