namespace Deveel.Messaging.XUnit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "MessagingClient")]
    public class FacebookMessengerOptionsTests
    {
        [Fact]
        public void ConnectionParameters_ShouldHaveExpectedValues()
        {
            Assert.Equal("PageAccessToken", FacebookConnectionParameters.PageAccessToken);
            Assert.Equal("PageId", FacebookConnectionParameters.PageId);
            Assert.Equal("WebhookUrl", FacebookConnectionParameters.WebhookUrl);
            Assert.Equal("VerifyToken", FacebookConnectionParameters.VerifyToken);
        }

        [Fact]
        public void ToConnectionSettings_ShouldSetAllProperties()
        {
            var options = new FacebookMessengerOptions
            {
                PageAccessToken = "test-token",
                PageId = "test-page-123",
                WebhookUrl = "https://example.com/webhook",
                VerifyToken = "test-verify"
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("test-token", settings.GetParameter(FacebookConnectionParameters.PageAccessToken));
            Assert.Equal("test-page-123", settings.GetParameter(FacebookConnectionParameters.PageId));
            Assert.Equal("https://example.com/webhook", settings.GetParameter(FacebookConnectionParameters.WebhookUrl));
            Assert.Equal("test-verify", settings.GetParameter(FacebookConnectionParameters.VerifyToken));
        }

        [Fact]
        public void ToConnectionSettings_ShouldSkipEmptyProperties()
        {
            var options = new FacebookMessengerOptions
            {
                PageAccessToken = "test-token",
                PageId = "test-page-123"
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("test-token", settings.GetParameter(FacebookConnectionParameters.PageAccessToken));
            Assert.Equal("test-page-123", settings.GetParameter(FacebookConnectionParameters.PageId));
            Assert.Null(settings.GetParameter(FacebookConnectionParameters.WebhookUrl));
            Assert.Null(settings.GetParameter(FacebookConnectionParameters.VerifyToken));
        }

        [Fact]
        public void ToConnectionSettings_ShouldReturnEmpty_WhenAllNull()
        {
            var options = new FacebookMessengerOptions();
            var settings = options.ToConnectionSettings();

            Assert.Empty(settings.Parameters);
        }

        [Fact]
        public void Implements_IConnectorOptions()
        {
            var options = new FacebookMessengerOptions();
            Assert.IsAssignableFrom<IConnectorOptions>(options);
        }
    }
}
