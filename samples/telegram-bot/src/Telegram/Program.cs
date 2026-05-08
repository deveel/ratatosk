using System.ComponentModel.DataAnnotations;
using Cocona;
using Deveel.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = CoconaApp.CreateBuilder();
builder.Services.AddLogging(logging =>
{
    logging
        .SetMinimumLevel(LogLevel.Information)
        .AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
});
builder.Services.AddSingleton<TelegramSampleSupport>();

var app = builder.Build();
app.AddSubCommand("telegram", command => command.AddCommands<TelegramCommands>());
app.Run();

public sealed class TelegramCommands(TelegramSampleSupport support)
{
    [Command("schema", Description = "Show all Telegram schemas or a single schema.")]
    public void Schema([Argument(Description = "Optional schema name.")] string? name = null)
        => support.PrintSchemas(name);

    [Command("configure", Description = "Prompt for Telegram credentials and save them to the local app configuration file.")]
    public void Configure()
        => support.Configure();

    [Command("validate", Description = "Validate a sample Telegram message.")]
    public void Validate([Option('k', Description = "Sample kind: text or location.")] string kind = "text")
        => support.PrintValidation(kind);

    [Command("status", Description = "Show Telegram connector status when saved credentials are available.")]
    public async Task Status()
        => await support.PrintStatusAsync();

    [Command("send", Description = "Build and send a live Telegram message interactively.")]
    public async Task Send()
        => await support.SendAsync();
}

public sealed class TelegramSampleSupport(ILoggerFactory loggerFactory)
{
    private const string SectionName = "Telegram";

    private readonly SampleConfigurationStore configuration = new();
    private readonly ILoggerFactory loggerFactory = loggerFactory;

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
                PrintSchema(entry.Key, entry.Value);
            }

            return;
        }

        if (!schemas.TryGetValue(name, out var schema))
        {
            Console.WriteLine($"Unknown schema '{name}'. Available schemas: {String.Join(", ", schemas.Keys)}");
            return;
        }

        PrintSchema(name, schema);
    }

    public void Configure()
    {
        configuration.ConfigureSection(
            SectionName,
            new SampleConfigurationField("BotToken", "Telegram bot token", IsSecret: true, IsRequired: true),
            new SampleConfigurationField("ChatId", "Default chat ID", IsRequired: true),
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
                PrintValidationResult(
                    "telegram validate text",
                    TelegramChannelSchemas.TelegramBot.ValidateMessage(CreateTextMessage("123456789")));
                break;
            case "location":
                PrintValidationResult(
                    "telegram validate location",
                    TelegramChannelSchemas.TelegramBot.ValidateMessage(CreateLocationMessage("123456789", 45.4642, 9.1900)));
                break;
            default:
                Console.WriteLine($"Unsupported validation kind '{kind}'. Use text or location.");
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

        var connector = CreateLiveConnector();
        PrintResult("telegram initialize", await connector.InitializeAsync(CancellationToken.None));
        PrintStatus("telegram status", await connector.GetStatusAsync(CancellationToken.None));
    }

    public async Task SendAsync()
    {
        if (!HasLiveConfiguration())
        {
            Console.WriteLine("Run 'telegram configure' or set TELEGRAM_BOT_TOKEN.");
            return;
        }

        var connector = CreateLiveConnector();
        PrintResult("telegram initialize", await connector.InitializeAsync(CancellationToken.None));

        var chatId = SampleConsolePrompts.RequiredText("Chat ID", GetValue("ChatId", "TELEGRAM_CHAT_ID"));
        var kind = SampleConsolePrompts.Select("Select the Telegram message type", ["Text", "Media", "Location"], "Text");
        var message = kind switch
        {
            "Text" => BuildTextMessage(chatId),
            "Media" => BuildMediaMessage(chatId),
            _ => BuildLocationMessage(chatId)
        };

        PrintSendResult($"telegram send {kind.ToLowerInvariant()}", await connector.SendMessageAsync(message, CancellationToken.None));
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

        AddIfPresent(settings, "WebhookUrl", GetValue("WebhookUrl", "TELEGRAM_WEBHOOK_URL"));
        AddIfPresent(settings, "SecretToken", GetValue("SecretToken", "TELEGRAM_SECRET_TOKEN"));

        return new TelegramBotConnector(
            TelegramChannelSchemas.TelegramBot,
            settings,
            logger: loggerFactory.CreateLogger<TelegramBotConnector>());
    }

    private Message BuildTextMessage(string chatId)
        => CreateTextMessage(
            SampleConsolePrompts.RequiredText("Message ID", "telegram-text-sample"),
            chatId,
            SampleConsolePrompts.RequiredText("Message text", "Hello from the <b>Deveel Telegram</b> sample."),
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

    private static double ParseDoubleOrDefault(string? value, double defaultValue)
        => Double.TryParse(
            value,
            System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : defaultValue;

    private static void PrintSchema(string name, IChannelSchema schema)
    {
        Console.WriteLine($"[{name}] {schema.DisplayName}");
        Console.WriteLine($"  Identity: {schema.GetLogicalIdentity()}");
        Console.WriteLine($"  Capabilities: {schema.Capabilities}");
        Console.WriteLine($"  Content Types: {JoinOrNone(schema.ContentTypes.Select(x => x.ToString()))}");
        Console.WriteLine($"  Parameters: {JoinOrNone(schema.Parameters.Select(x => $"{x.Name}:{x.DataType}{(x.IsRequired ? "*" : "")}"))}");
        Console.WriteLine($"  Message Properties: {JoinOrNone(schema.MessageProperties.Select(x => x.Name))}");
        Console.WriteLine();
    }

    private static void PrintValidationResult(string label, IEnumerable<ValidationResult> results)
    {
        var errors = results.ToList();
        Console.WriteLine($"{label}: {(errors.Count == 0 ? "valid" : "invalid")}");
        foreach (var error in errors)
        {
            Console.WriteLine($"  - {error.ErrorMessage}");
        }
    }

    private static void PrintResult<T>(string label, ConnectorResult<T> result)
    {
        Console.WriteLine($"{label}: {(result.Successful ? "ok" : $"{result.Error?.ErrorCode} - {result.Error?.ErrorMessage}")}");
    }

    private static void PrintStatus(string label, ConnectorResult<StatusInfo> result)
    {
        if (!result.Successful)
        {
            PrintResult(label, result);
            return;
        }

        Console.WriteLine($"{label}: {result.Value.Status}");
    }

    private static void PrintSendResult(string label, ConnectorResult<SendResult> result)
    {
        if (!result.Successful || result.Value is null)
        {
            PrintResult(label, result);
            return;
        }

        Console.WriteLine($"{label}: local={result.Value.MessageId}, remote={result.Value.RemoteMessageId}, status={result.Value.Status}");
    }

    private static void AddIfPresent(ConnectionSettings settings, string parameterName, string? value)
    {
        if (!String.IsNullOrWhiteSpace(value))
        {
            settings.SetParameter(parameterName, value);
        }
    }

    private static string JoinOrNone(IEnumerable<string> values)
    {
        var items = values.Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
        return items.Length == 0 ? "none" : String.Join(", ", items);
    }
}
