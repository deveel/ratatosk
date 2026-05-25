namespace Deveel.Messaging
{
    /// <summary>
    /// Provides extension methods for <see cref="ConnectionSettings"/> to access Facebook Messenger-specific parameters.
    /// </summary>
    public static class FacebookConnectionSettingsExtensions
    {
        /// <summary>
        /// Gets the Facebook Page Access Token from the connection settings.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The page access token, or <c>null</c> if not configured.</returns>
        public static string? GetPageAccessToken(this ConnectionSettings settings)
            => settings.GetParameter<string>(FacebookConnectionParameters.PageAccessToken);

        /// <summary>
        /// Gets the Facebook Page ID from the connection settings.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The page ID, or <c>null</c> if not configured.</returns>
        public static string? GetPageId(this ConnectionSettings settings)
            => settings.GetParameter<string>(FacebookConnectionParameters.PageId);

        /// <summary>
        /// Gets the webhook URL configured for Facebook Messenger callbacks.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The webhook URL, or <c>null</c> if not configured.</returns>
        public static string? GetWebhookUrl(this ConnectionSettings settings)
            => settings.GetParameter<string>(FacebookConnectionParameters.WebhookUrl);

        /// <summary>
        /// Gets the verify token for Facebook webhook verification.
        /// </summary>
        /// <param name="settings">The connection settings to read from.</param>
        /// <returns>The verify token, or <c>null</c> if not configured.</returns>
        public static string? GetVerifyToken(this ConnectionSettings settings)
            => settings.GetParameter<string>(FacebookConnectionParameters.VerifyToken);
    }
}
