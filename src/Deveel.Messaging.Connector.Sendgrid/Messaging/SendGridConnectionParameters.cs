namespace Deveel.Messaging
{
    /// <summary>
    /// Defines constant keys for the connection parameters used
    /// to configure the SendGrid email connector.
    /// </summary>
    public static class SendGridConnectionParameters
    {
        /// <summary>
        /// The key for the API key parameter.
        /// </summary>
        public const string ApiKey = "ApiKey";

        /// <summary>
        /// The key for the sandbox mode parameter.
        /// </summary>
        public const string SandboxMode = "SandboxMode";

        /// <summary>
        /// The key for the webhook URL parameter.
        /// </summary>
        public const string WebhookUrl = "WebhookUrl";

        /// <summary>
        /// The key for the tracking settings parameter.
        /// </summary>
        public const string TrackingSettings = "TrackingSettings";

        /// <summary>
        /// The key for the default from name parameter.
        /// </summary>
        public const string DefaultFromName = "DefaultFromName";

        /// <summary>
        /// The key for the default reply-to parameter.
        /// </summary>
        public const string DefaultReplyTo = "DefaultReplyTo";
    }
}
