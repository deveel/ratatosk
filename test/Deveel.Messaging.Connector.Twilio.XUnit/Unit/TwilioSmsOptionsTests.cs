namespace Deveel.Messaging.XUnit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "TwilioSmsConnector")]
    public class TwilioSmsOptionsTests
    {
        [Fact]
        public void ConnectionParameters_ShouldHaveExpectedValues()
        {
            Assert.Equal("AccountSid", TwilioConnectionParameters.AccountSid);
            Assert.Equal("AuthToken", TwilioConnectionParameters.AuthToken);
            Assert.Equal("WebhookUrl", TwilioConnectionParameters.WebhookUrl);
            Assert.Equal("StatusCallback", TwilioConnectionParameters.StatusCallback);
            Assert.Equal("ValidityPeriod", TwilioConnectionParameters.ValidityPeriod);
            Assert.Equal("MaxPrice", TwilioConnectionParameters.MaxPrice);
            Assert.Equal("MessagingServiceSid", TwilioConnectionParameters.MessagingServiceSid);
        }

        [Fact]
        public void ConnectionSettingsDefaults_ShouldHaveExpectedValues()
        {
            Assert.Equal(14400, TwilioConnectionSettingsDefaults.ValidityPeriod);
        }

        [Fact]
        public void ToConnectionSettings_ShouldSetAllProperties()
        {
            var options = new TwilioSmsOptions
            {
                AccountSid = "AC123456",
                AuthToken = "auth-token-123",
                WebhookUrl = "https://example.com/webhook",
                StatusCallback = "https://example.com/callback",
                ValidityPeriod = 3600,
                MaxPrice = 0.05m,
                MessagingServiceSid = "MG789012"
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("AC123456", settings.GetParameter(TwilioConnectionParameters.AccountSid));
            Assert.Equal("auth-token-123", settings.GetParameter(TwilioConnectionParameters.AuthToken));
            Assert.Equal("https://example.com/webhook", settings.GetParameter(TwilioConnectionParameters.WebhookUrl));
            Assert.Equal("https://example.com/callback", settings.GetParameter(TwilioConnectionParameters.StatusCallback));
            Assert.Equal(3600, settings.GetParameter<int>(TwilioConnectionParameters.ValidityPeriod));
            Assert.Equal(0.05m, settings.GetParameter<decimal>(TwilioConnectionParameters.MaxPrice));
            Assert.Equal("MG789012", settings.GetParameter(TwilioConnectionParameters.MessagingServiceSid));
        }

        [Fact]
        public void ToConnectionSettings_ShouldSkipEmptyProperties()
        {
            var options = new TwilioSmsOptions
            {
                AccountSid = "AC123456",
                AuthToken = "auth-token-123"
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("AC123456", settings.GetParameter(TwilioConnectionParameters.AccountSid));
            Assert.Equal("auth-token-123", settings.GetParameter(TwilioConnectionParameters.AuthToken));
            Assert.Null(settings.GetParameter(TwilioConnectionParameters.WebhookUrl));
            Assert.Null(settings.GetParameter(TwilioConnectionParameters.StatusCallback));
            Assert.Null(settings.GetParameter(TwilioConnectionParameters.ValidityPeriod));
            Assert.Null(settings.GetParameter(TwilioConnectionParameters.MaxPrice));
            Assert.Null(settings.GetParameter(TwilioConnectionParameters.MessagingServiceSid));
        }

        [Fact]
        public void ToConnectionSettings_ShouldReturnEmpty_WhenAllNull()
        {
            var options = new TwilioSmsOptions();
            var settings = options.ToConnectionSettings();

            Assert.Empty(settings.Parameters);
        }

        [Fact]
        public void Implements_IConnectorOptions()
        {
            var options = new TwilioSmsOptions();
            Assert.IsAssignableFrom<IConnectorOptions>(options);
        }
    }
}
