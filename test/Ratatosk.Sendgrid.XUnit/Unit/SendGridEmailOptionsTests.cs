namespace Ratatosk.XUnit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "SendGridEmailConnector")]
    public class SendGridEmailOptionsTests
    {
        [Fact]
        public void ConnectionParameters_ShouldHaveExpectedValues()
        {
            Assert.Equal("ApiKey", SendGridConnectionParameters.ApiKey);
            Assert.Equal("SandboxMode", SendGridConnectionParameters.SandboxMode);
            Assert.Equal("WebhookUrl", SendGridConnectionParameters.WebhookUrl);
            Assert.Equal("TrackingSettings", SendGridConnectionParameters.TrackingSettings);
            Assert.Equal("DefaultFromName", SendGridConnectionParameters.DefaultFromName);
            Assert.Equal("DefaultReplyTo", SendGridConnectionParameters.DefaultReplyTo);
        }

        [Fact]
        public void ConnectionSettingsDefaults_ShouldHaveExpectedValues()
        {
            Assert.False(SendGridConnectionSettingsDefaults.SandboxMode);
            Assert.True(SendGridConnectionSettingsDefaults.TrackingSettings);
        }

        [Fact]
        public void ToConnectionSettings_ShouldSetAllProperties()
        {
            var options = new SendGridEmailOptions
            {
                ApiKey = "SG.test-key",
                SandboxMode = true,
                WebhookUrl = "https://example.com/webhook",
                TrackingSettings = false,
                DefaultFromName = "Test Sender",
                DefaultReplyTo = "reply@example.com"
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("SG.test-key", settings.GetParameter(SendGridConnectionParameters.ApiKey));
            Assert.True(settings.GetParameter<bool>(SendGridConnectionParameters.SandboxMode));
            Assert.Equal("https://example.com/webhook", settings.GetParameter(SendGridConnectionParameters.WebhookUrl));
            // TrackingSettings was set to false (not the default), so it is not stored
            Assert.Null(settings.GetParameter(SendGridConnectionParameters.TrackingSettings));
            Assert.Equal("Test Sender", settings.GetParameter(SendGridConnectionParameters.DefaultFromName));
            Assert.Equal("reply@example.com", settings.GetParameter(SendGridConnectionParameters.DefaultReplyTo));
        }

        [Fact]
        public void ToConnectionSettings_ShouldSkipEmptyProperties()
        {
            var options = new SendGridEmailOptions
            {
                ApiKey = "SG.test-key"
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("SG.test-key", settings.GetParameter(SendGridConnectionParameters.ApiKey));
            Assert.False(settings.GetParameter<bool?>(SendGridConnectionParameters.SandboxMode) ?? false);
            Assert.Null(settings.GetParameter(SendGridConnectionParameters.WebhookUrl));
            Assert.False(settings.GetParameter<bool?>(SendGridConnectionParameters.TrackingSettings) ?? false);
            Assert.Null(settings.GetParameter(SendGridConnectionParameters.DefaultFromName));
            Assert.Null(settings.GetParameter(SendGridConnectionParameters.DefaultReplyTo));
        }

        [Fact]
        public void ToConnectionSettings_ShouldReturnEmpty_WhenAllNull()
        {
            var options = new SendGridEmailOptions();
            var settings = options.ToConnectionSettings();

            Assert.Empty(settings.Parameters);
        }

        [Fact]
        public void Implements_IConnectorOptions()
        {
            var options = new SendGridEmailOptions();
            Assert.IsAssignableFrom<IConnectorOptions>(options);
        }
    }
}
