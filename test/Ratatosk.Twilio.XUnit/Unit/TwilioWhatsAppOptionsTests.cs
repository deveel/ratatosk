namespace Ratatosk.XUnit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "TwilioWhatsAppConnector")]
    public class TwilioWhatsAppOptionsTests
    {
        [Fact]
        public void ToConnectionSettings_ShouldSetAllProperties()
        {
            var options = new TwilioWhatsAppOptions
            {
                AccountSid = "AC123456",
                AuthToken = "auth-token-123",
                WebhookUrl = "https://example.com/webhook",
                StatusCallback = "https://example.com/callback"
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("AC123456", settings.GetParameter(TwilioConnectionParameters.AccountSid));
            Assert.Equal("auth-token-123", settings.GetParameter(TwilioConnectionParameters.AuthToken));
            Assert.Equal("https://example.com/webhook", settings.GetParameter(TwilioConnectionParameters.WebhookUrl));
            Assert.Equal("https://example.com/callback", settings.GetParameter(TwilioConnectionParameters.StatusCallback));
        }

        [Fact]
        public void ToConnectionSettings_ShouldSkipEmptyProperties()
        {
            var options = new TwilioWhatsAppOptions
            {
                AccountSid = "AC123456",
                AuthToken = "auth-token-123"
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("AC123456", settings.GetParameter(TwilioConnectionParameters.AccountSid));
            Assert.Equal("auth-token-123", settings.GetParameter(TwilioConnectionParameters.AuthToken));
            Assert.Null(settings.GetParameter(TwilioConnectionParameters.WebhookUrl));
            Assert.Null(settings.GetParameter(TwilioConnectionParameters.StatusCallback));
        }

        [Fact]
        public void ToConnectionSettings_ShouldReturnEmpty_WhenAllNull()
        {
            var options = new TwilioWhatsAppOptions();
            var settings = options.ToConnectionSettings();

            Assert.Empty(settings.Parameters);
        }

        [Fact]
        public void Implements_IConnectorOptions()
        {
            var options = new TwilioWhatsAppOptions();
            Assert.IsAssignableFrom<IConnectorOptions>(options);
        }
    }
}
