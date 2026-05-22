using System.ComponentModel.DataAnnotations;
using Deveel;
using Deveel.Messaging;
using Microsoft.Extensions.Logging;

namespace Telegram;

public sealed class TelegramSampleSupport(ILoggerFactory loggerFactory, IMessagingClient client)
{
    private const string SectionName = "Telegram";

    private readonly SampleConfigurationStore configuration = new();
    private readonly ILoggerFactory loggerFactory = loggerFactory;
    private readonly IMessagingClient client = client;

    private readonly Dictionary<string, IChannelSchema> schemas = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TelegramBot"] = TelegramChannelSchemas.TelegramBot,
        ["SimpleTelegramBot"] = TelegramChannelSchemas.SimpleTelegramBot,
        ["NotificationBot"] = TelegramChannelSchemas.NotificationBot,
        ["WebhookBot"] = TelegramChannelSchemas.WebhookBot,
    };

    public void PrintSchemas(string? name)
    {
        if (String.IsNullOrWhiteSpace(name))
        {
            foreach (var entry in schemas)
            {
                SampleOutputHelper.PrintSchema(entry.Key, entry.Value);
            }

            return;
        }

        if (!schemas.TryGetValue(name, out var schema))
        {
            Console.WriteLine($"Unknown schema '{name}'. Available schemas: {String.Join(", ", schemas.Keys)}");
            return;
        }

        SampleOutputHelper.PrintSchema(name, schema);
    }

    public void Configure()
    {
        configuration.ConfigureSection(
            SectionName,
            new SampleConfigurationField("BotToken", "Telegram bot token", IsSecret: true, IsRequired: true),
            new SampleConfigurationField("ChatId", "Default chat ID"),
            new SampleConfigurationField("WebhookUrl", "Webhook URL"),
            new SampleConfigurationField("SecretToken", "Webhook secret token", IsSecret: true),
            new SampleConfigurationField("MediaUrl", "Default media URL"),
            new SampleConfigurationField("LocationLatitude", "Default latitude"),
            new SampleConfigurationField("LocationLongitude", "Default longitude"));
    }

    public void PrintValidation(string kind)
    {
        switch (kind.Trim().ToLowerInvariant())
        {
            case "text":
                SampleOutputHelper.PrintValidationResult(
                    "telegram validate text",
                    TelegramChannelSchemas.TelegramBot.ValidateMessage(CreateTextMessage("123456789")));
                break;
            case "location":
                SampleOutputHelper.PrintValidationResult(
                    "telegram validate location",
                    TelegramChannelSchemas.TelegramBot.ValidateMessage(CreateLocationMessage("123456789", 45.4642, 9.1900)));
                break;
            case "button":
                SampleOutputHelper.PrintValidationResult(
                    "telegram validate button",
                    TelegramChannelSchemas.TelegramBot.ValidateMessage(CreateButtonMessage("123456789")));
                break;
            default:
                Console.WriteLine($"Unsupported validation kind '{kind}'. Use text, location, or button.");
                break;
        }
    }

    public async Task PrintStatusAsync()
    {
        if (!HasLiveConfiguration())
        {
            Console.WriteLine("Run 'telegram configure' or set TELEGRAM_BOT_TOKEN and TELEGRAM_CHAT_ID.");
            return;
        }

        var status = await client.GetStatusAsync("telegram", CancellationToken.None);
        if (!status.IsSuccess())
        {
            SampleOutputHelper.PrintResult("telegram status", status);
            return;
        }

        Console.WriteLine($"telegram status: {status.Value.Status}");
    }

    public async Task SendAsync()
    {
        if (!HasLiveConfiguration())
        {
            Console.WriteLine("Run 'telegram configure' or set TELEGRAM_BOT_TOKEN.");
            return;
        }

        var chatId = SampleConsolePrompts.OptionalText("Chat ID", GetValue("ChatId", "TELEGRAM_CHAT_ID"));

        if (String.IsNullOrWhiteSpace(chatId))
        {
            Console.WriteLine("No chat ID provided. Aborting send.");
            return;
        }

        var kind = SampleConsolePrompts.Select("Select the Telegram message type", ["Text", "Media", "Location", "Button", "Quick Reply"], "Text");
        var message = kind switch
        {
            "Text" => BuildTextMessage(chatId),
            "Media" => BuildMediaMessage(chatId),
            "Location" => BuildLocationMessage(chatId),
            "Button" => BuildButtonMessage(chatId),
            _ => BuildQuickReplyMessage(chatId)
        };

        SampleOutputHelper.PrintSendResult($"telegram send {kind.ToLowerInvariant()}", await client.SendAsync("telegram", message, CancellationToken.None));
    }

    private bool HasLiveConfiguration()
        => HasValue("BotToken", "TELEGRAM_BOT_TOKEN");

    private bool HasValue(string key, string environmentName)
        => configuration.HasValue(SectionName, key, environmentName);

    private string GetRequired(string key, string environmentName)
        => configuration.GetRequired(SectionName, key, environmentName);

    private string? GetValue(string key, string environmentName)
        => configuration.GetValue(SectionName, key, environmentName);

    private TelegramBotConnector CreateLiveConnector()
    {
        var settings = new ConnectionSettings(TelegramChannelSchemas.TelegramBot)
            .SetParameter("BotToken", GetRequired("BotToken", "TELEGRAM_BOT_TOKEN"))
            .SetParameter("ParseMode", "Html")
            .SetParameter("DisableWebPagePreview", true);

        SampleOutputHelper.AddIfPresent(settings, "WebhookUrl", GetValue("WebhookUrl", "TELEGRAM_WEBHOOK_URL"));
        SampleOutputHelper.AddIfPresent(settings, "SecretToken", GetValue("SecretToken", "TELEGRAM_SECRET_TOKEN"));

        return new TelegramBotConnector(
            TelegramChannelSchemas.TelegramBot,
            settings,
            logger: loggerFactory.CreateLogger<TelegramBotConnector>());
    }

    private Message BuildTextMessage(string chatId)
        => CreateTextMessage(
            SampleConsolePrompts.RequiredText("Message ID", "telegram-text-sample"),
            chatId,
            SampleConsolePrompts.MultiLineBody("Message text", "Hello from the <b>Deveel Telegram</b> sample."),
            SampleConsolePrompts.Select("Parse mode", ["Html", "Markdown"], "Html"),
            SampleConsolePrompts.Confirm("Disable web page preview?", true));

    private Message BuildMediaMessage(string chatId)
        => CreateMediaMessage(
            SampleConsolePrompts.RequiredText("Message ID", "telegram-media-sample"),
            chatId,
            SampleConsolePrompts.RequiredText("Media file name", "telegram-sample.png"),
            SampleConsolePrompts.RequiredText("Media URL", GetValue("MediaUrl", "TELEGRAM_MEDIA_URL") ?? "https://example.com/telegram.png"),
            SampleConsolePrompts.OptionalText("Caption", "Telegram media sample"),
            SampleConsolePrompts.Select("Parse mode", ["Markdown", "Html"], "Markdown"));

    private Message BuildLocationMessage(string chatId)
        => CreateLocationMessage(
            SampleConsolePrompts.RequiredText("Message ID", "telegram-location-sample"),
            chatId,
            SampleConsolePrompts.RequiredDouble("Latitude", ParseDoubleOrDefault(GetValue("LocationLatitude", "TELEGRAM_LOCATION_LATITUDE"), 45.4642)),
            SampleConsolePrompts.RequiredDouble("Longitude", ParseDoubleOrDefault(GetValue("LocationLongitude", "TELEGRAM_LOCATION_LONGITUDE"), 9.1900)),
            SampleConsolePrompts.RequiredInt("Live period seconds", 300));

    private static Message CreateTextMessage(string chatId)
        => CreateTextMessage("telegram-text-sample", chatId, "Hello from the <b>Deveel Telegram</b> sample.", "Html", true);

    private static Message CreateTextMessage(
        string id,
        string chatId,
        string text,
        string parseMode,
        bool disableWebPagePreview)
    {
        return new Message
        {
            Id = id,
            Receiver = Endpoint.Id(chatId),
            Content = new TextContent(text),
            Properties = new Dictionary<string, MessageProperty>
            {
                ["ParseMode"] = new("ParseMode", parseMode),
                ["DisableWebPagePreview"] = new("DisableWebPagePreview", disableWebPagePreview)
            }
        };
    }

    private static Message CreateMediaMessage(string chatId, string mediaUrl)
        => CreateMediaMessage("telegram-media-sample", chatId, "telegram-sample.png", mediaUrl, "Telegram media sample", "Markdown");

    private static Message CreateMediaMessage(
        string id,
        string chatId,
        string fileName,
        string mediaUrl,
        string? caption,
        string parseMode)
    {
        var properties = new Dictionary<string, MessageProperty>
        {
            ["ParseMode"] = new("ParseMode", parseMode)
        };

        if (!String.IsNullOrWhiteSpace(caption))
            properties["Caption"] = new("Caption", caption);

        return new Message
        {
            Id = id,
            Receiver = Endpoint.Id(chatId),
            Content = new MediaContent(MediaType.Image, fileName, mediaUrl),
            Properties = properties
        };
    }

    private static Message CreateLocationMessage(string chatId, double latitude, double longitude)
        => CreateLocationMessage("telegram-location-sample", chatId, latitude, longitude, 300);

    private static Message CreateLocationMessage(
        string id,
        string chatId,
        double latitude,
        double longitude,
        int livePeriod)
    {
        return new Message
        {
            Id = id,
            Receiver = Endpoint.Id(chatId),
            Content = new LocationContent(latitude, longitude).WithLivePeriod(livePeriod)
        };
    }

    private Message BuildButtonMessage(string chatId)
    {
        var text = SampleConsolePrompts.RequiredText("Button text", "Click here");
        var buttonType = SampleConsolePrompts.Select("Button type", ["Url", "Postback"], "Url");
        var value = buttonType == "Url"
            ? SampleConsolePrompts.RequiredText("URL", "https://example.com")
            : SampleConsolePrompts.RequiredText("Callback data", "BTN_PAYLOAD");

        return CreateButtonMessage(
            SampleConsolePrompts.RequiredText("Message ID", "telegram-button-sample"),
            chatId,
            text,
            buttonType == "Url" ? ButtonType.Url : ButtonType.Postback,
            value);
    }

    private Message BuildQuickReplyMessage(string chatId)
    {
        var title = SampleConsolePrompts.RequiredText("Quick reply title", "Yes");

        return new Message
        {
            Id = SampleConsolePrompts.RequiredText("Message ID", "telegram-quickreply-sample"),
            Receiver = Endpoint.Id(chatId),
            Content = new QuickReplyContent(title, title),
            Properties = new Dictionary<string, MessageProperty>
            {
                ["ParseMode"] = new("ParseMode", "Html")
            }
        };
    }

    private static Message CreateButtonMessage(string chatId)
        => CreateButtonMessage("telegram-button-sample", chatId, "Click here", ButtonType.Url, "https://example.com");

    private static Message CreateButtonMessage(
        string id,
        string chatId,
        string text,
        ButtonType buttonType,
        string? value)
    {
        return new Message
        {
            Id = id,
            Receiver = Endpoint.Id(chatId),
            Content = new ButtonContent(text, buttonType, value),
            Properties = new Dictionary<string, MessageProperty>
            {
                ["ParseMode"] = new("ParseMode", "Html"),
                ["DisableWebPagePreview"] = new("DisableWebPagePreview", true)
            }
        };
    }

    private static double ParseDoubleOrDefault(string? value, double defaultValue)
        => Double.TryParse(
            value,
            System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : defaultValue;
}
