namespace Ratatosk
{
    /// <summary>
    /// Provides extension methods for <see cref="ConnectionSettings"/> to access SendGrid-specific parameters.
    /// </summary>
    public static class SendGridConnectionSettingsExtensions
    {
        /// <summary>
        /// Gets the SendGrid API key from the connection settings.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The API key, or <c>null</c> if not configured.</returns>
        public static string? GetApiKey(this ConnectionSettings settings)
            => settings.GetParameter<string>(SendGridConnectionParameters.ApiKey);

        /// <summary>
        /// Gets whether sandbox mode is enabled for testing.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns><c>true</c> if sandbox mode is enabled, or <c>null</c> if not configured.</returns>
        public static bool? GetSandboxMode(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(SendGridConnectionParameters.SandboxMode);

        /// <summary>
        /// Gets the webhook URL configured for SendGrid event callbacks.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The webhook URL, or <c>null</c> if not configured.</returns>
        public static string? GetWebhookUrl(this ConnectionSettings settings)
            => settings.GetParameter<string>(SendGridConnectionParameters.WebhookUrl);

        /// <summary>
        /// Gets whether tracking settings (e.g., open tracking) are enabled.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns><c>true</c> if tracking is enabled, or <c>null</c> if not configured.</returns>
        public static bool? GetTrackingSettings(this ConnectionSettings settings)
            => settings.GetParameter<bool?>(SendGridConnectionParameters.TrackingSettings);

        /// <summary>
        /// Gets the default sender name for outgoing emails.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The default from name, or <c>null</c> if not configured.</returns>
        public static string? GetDefaultFromName(this ConnectionSettings settings)
            => settings.GetParameter<string>(SendGridConnectionParameters.DefaultFromName);

        /// <summary>
        /// Gets the default reply-to address for outgoing emails.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The default reply-to, or <c>null</c> if not configured.</returns>
        public static string? GetDefaultReplyTo(this ConnectionSettings settings)
            => settings.GetParameter<string>(SendGridConnectionParameters.DefaultReplyTo);
    }
}
