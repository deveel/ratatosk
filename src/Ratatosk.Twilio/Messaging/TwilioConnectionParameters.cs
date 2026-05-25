namespace Ratatosk;

/// <summary>
/// Defines well-known configuration keys used by Twilio connectors.
/// </summary>
public static class TwilioConnectionParameters
{
    /// <summary>
    /// The key for the Twilio account SID.
    /// </summary>
    public const string AccountSid = "AccountSid";

    /// <summary>
    /// The key for the Twilio authentication token.
    /// </summary>
    public const string AuthToken = "AuthToken";

    /// <summary>
    /// The key for the webhook endpoint URL.
    /// </summary>
    public const string WebhookUrl = "WebhookUrl";

    /// <summary>
    /// The key for the status callback URL.
    /// </summary>
    public const string StatusCallback = "StatusCallback";

    /// <summary>
    /// The key for the message validity period.
    /// </summary>
    public const string ValidityPeriod = "ValidityPeriod";

    /// <summary>
    /// The key for the maximum delivery price.
    /// </summary>
    public const string MaxPrice = "MaxPrice";

    /// <summary>
    /// The key for the Twilio messaging service SID.
    /// </summary>
    public const string MessagingServiceSid = "MessagingServiceSid";
}
