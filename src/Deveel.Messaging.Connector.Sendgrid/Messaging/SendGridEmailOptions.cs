namespace Deveel.Messaging
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

            return settings;
        }
    }
}
