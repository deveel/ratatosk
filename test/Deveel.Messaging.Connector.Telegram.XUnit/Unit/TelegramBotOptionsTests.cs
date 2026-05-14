namespace Deveel.Messaging.XUnit
{
    [Trait("Category", "Unit")]
    [Trait("Feature", "TelegramBotConnector")]
    public class TelegramBotOptionsTests
    {
        [Fact]
        public void ConnectionParameters_ShouldHaveExpectedValues()
        {
            Assert.Equal("BotToken", TelegramConnectionParameters.BotToken);
            Assert.Equal("WebhookUrl", TelegramConnectionParameters.WebhookUrl);
            Assert.Equal("SecretToken", TelegramConnectionParameters.SecretToken);
            Assert.Equal("DisableWebPagePreview", TelegramConnectionParameters.DisableWebPagePreview);
            Assert.Equal("DisableNotification", TelegramConnectionParameters.DisableNotification);
            Assert.Equal("ParseMode", TelegramConnectionParameters.ParseMode);
            Assert.Equal("MaxRetries", TelegramConnectionParameters.MaxRetries);
            Assert.Equal("TimeoutSeconds", TelegramConnectionParameters.TimeoutSeconds);
            Assert.Equal("MaxConnections", TelegramConnectionParameters.MaxConnections);
            Assert.Equal("DropPendingUpdates", TelegramConnectionParameters.DropPendingUpdates);
            Assert.Equal("DefaultChatId", TelegramConnectionParameters.DefaultChatId);
        }

        [Fact]
        public void ConnectionSettingsDefaults_ShouldHaveExpectedValues()
        {
            Assert.False(TelegramConnectionSettingsDefaults.DisableWebPagePreview);
            Assert.False(TelegramConnectionSettingsDefaults.DisableNotification);
            Assert.Equal("Markdown", TelegramConnectionSettingsDefaults.ParseMode);
            Assert.Equal(3, TelegramConnectionSettingsDefaults.MaxRetries);
            Assert.Equal(30, TelegramConnectionSettingsDefaults.TimeoutSeconds);
            Assert.Equal(40, TelegramConnectionSettingsDefaults.MaxConnections);
            Assert.False(TelegramConnectionSettingsDefaults.DropPendingUpdates);
        }

        [Fact]
        public void ToConnectionSettings_ShouldSetAllProperties()
        {
            var options = new TelegramBotOptions
            {
                BotToken = "123:ABC-def",
                WebhookUrl = "https://example.com/webhook",
                SecretToken = "my-secret",
                DisableWebPagePreview = true,
                DisableNotification = true,
                ParseMode = "HTML",
                MaxRetries = 5,
                TimeoutSeconds = 120
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("123:ABC-def", settings.GetParameter(TelegramConnectionParameters.BotToken));
            Assert.Equal("https://example.com/webhook", settings.GetParameter(TelegramConnectionParameters.WebhookUrl));
            Assert.Equal("my-secret", settings.GetParameter(TelegramConnectionParameters.SecretToken));
            Assert.True(settings.GetParameter<bool>(TelegramConnectionParameters.DisableWebPagePreview));
            Assert.True(settings.GetParameter<bool>(TelegramConnectionParameters.DisableNotification));
            Assert.Equal("HTML", settings.GetParameter(TelegramConnectionParameters.ParseMode));
            Assert.Equal(5, settings.GetParameter<int>(TelegramConnectionParameters.MaxRetries));
            Assert.Equal(120, settings.GetParameter<int>(TelegramConnectionParameters.TimeoutSeconds));
        }

        [Fact]
        public void ToConnectionSettings_ShouldSkipDefaultValues()
        {
            var options = new TelegramBotOptions
            {
                BotToken = "123:ABC-def"
            };

            var settings = options.ToConnectionSettings();

            Assert.Equal("123:ABC-def", settings.GetParameter(TelegramConnectionParameters.BotToken));
            Assert.Null(settings.GetParameter(TelegramConnectionParameters.WebhookUrl));
            Assert.Null(settings.GetParameter(TelegramConnectionParameters.SecretToken));
            Assert.Null(settings.GetParameter(TelegramConnectionParameters.ParseMode));
            Assert.Null(settings.GetParameter(TelegramConnectionParameters.MaxRetries));
            Assert.Null(settings.GetParameter(TelegramConnectionParameters.TimeoutSeconds));
        }

        [Fact]
        public void ToConnectionSettings_ShouldReturnEmpty_WhenAllNull()
        {
            var options = new TelegramBotOptions
            {
                BotToken = null!
            };
            var settings = options.ToConnectionSettings();

            Assert.Empty(settings.Parameters);
        }

        [Fact]
        public void Implements_IConnectorOptions()
        {
            var options = new TelegramBotOptions();
            Assert.IsAssignableFrom<IConnectorOptions>(options);
        }
    }
}
