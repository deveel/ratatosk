namespace Deveel.Messaging
{
    public class SendGridEmailOptions : IConnectorOptions
    {
        public string? ApiKey { get; set; }

        public bool SandboxMode { get; set; }

        public string? WebhookUrl { get; set; }

        public bool TrackingSettings { get; set; }

        public string? DefaultFromName { get; set; }

        public string? DefaultReplyTo { get; set; }

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
