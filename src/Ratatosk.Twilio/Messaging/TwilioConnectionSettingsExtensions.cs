namespace Ratatosk;

/// <summary>
/// Provides extension methods for <see cref="ConnectionSettings"/> to access Twilio-specific parameters.
/// </summary>
public static class TwilioConnectionSettingsExtensions
{
    /// <summary>
    /// Gets the Twilio Account SID from the connection settings.
    /// </summary>
    /// <param name="settings">The connection settings to read from.</param>
    /// <returns>The Account SID, or <c>null</c> if not configured.</returns>
    public static string? GetAccountSid(this ConnectionSettings settings)
        => settings.GetParameter<string>(TwilioConnectionParameters.AccountSid);

    /// <summary>
    /// Gets the Twilio authentication token from the connection settings.
    /// </summary>
    /// <param name="settings">The connection settings to read from.</param>
    /// <returns>The authentication token, or <c>null</c> if not configured.</returns>
    public static string? GetAuthToken(this ConnectionSettings settings)
        => settings.GetParameter<string>(TwilioConnectionParameters.AuthToken);

    /// <summary>
    /// Gets the webhook URL configured for Twilio callbacks.
    /// </summary>
    /// <param name="settings">The connection settings to read from.</param>
    /// <returns>The webhook URL, or <c>null</c> if not configured.</returns>
    public static string? GetWebhookUrl(this ConnectionSettings settings)
        => settings.GetParameter<string>(TwilioConnectionParameters.WebhookUrl);

    /// <summary>
    /// Gets the status callback URL for Twilio message status updates.
    /// </summary>
    /// <param name="settings">The connection settings to read from.</param>
    /// <returns>The status callback URL, or <c>null</c> if not configured.</returns>
    public static string? GetStatusCallback(this ConnectionSettings settings)
        => settings.GetParameter<string>(TwilioConnectionParameters.StatusCallback);

    /// <summary>
    /// Gets the validity period (in seconds) for Twilio messages.
    /// </summary>
    /// <param name="settings">The connection settings to read from.</param>
    /// <returns>The validity period, or <c>null</c> if not configured.</returns>
    public static int? GetValidityPeriod(this ConnectionSettings settings)
        => settings.GetParameter<int?>(TwilioConnectionParameters.ValidityPeriod);

    /// <summary>
    /// Gets the maximum price allowed for messages.
    /// </summary>
    /// <param name="settings">The connection settings to read from.</param>
    /// <returns>The maximum price, or <c>null</c> if not configured.</returns>
    public static double? GetMaxPrice(this ConnectionSettings settings)
        => settings.GetParameter<double?>(TwilioConnectionParameters.MaxPrice);

    /// <summary>
    /// Gets the Messaging Service SID for Twilio.
    /// </summary>
    /// <param name="settings">The connection settings to read from.</param>
    /// <returns>The Messaging Service SID, or <c>null</c> if not configured.</returns>
    public static string? GetMessagingServiceSid(this ConnectionSettings settings)
        => settings.GetParameter<string>(TwilioConnectionParameters.MessagingServiceSid);
}
