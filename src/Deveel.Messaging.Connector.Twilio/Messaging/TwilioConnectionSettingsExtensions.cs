namespace Deveel.Messaging;

public static class TwilioConnectionSettingsExtensions
{
    public static string? GetAccountSid(this ConnectionSettings settings)
        => settings.GetParameter<string>(TwilioConnectionParameters.AccountSid);

    public static string? GetAuthToken(this ConnectionSettings settings)
        => settings.GetParameter<string>(TwilioConnectionParameters.AuthToken);

    public static string? GetWebhookUrl(this ConnectionSettings settings)
        => settings.GetParameter<string>(TwilioConnectionParameters.WebhookUrl);

    public static string? GetStatusCallback(this ConnectionSettings settings)
        => settings.GetParameter<string>(TwilioConnectionParameters.StatusCallback);

    public static int? GetValidityPeriod(this ConnectionSettings settings)
        => settings.GetParameter<int?>(TwilioConnectionParameters.ValidityPeriod);

    public static double? GetMaxPrice(this ConnectionSettings settings)
        => settings.GetParameter<double?>(TwilioConnectionParameters.MaxPrice);

    public static string? GetMessagingServiceSid(this ConnectionSettings settings)
        => settings.GetParameter<string>(TwilioConnectionParameters.MessagingServiceSid);
}
