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
builder.Services.AddSingleton<FacebookSampleSupport>();

var app = builder.Build();
app.AddSubCommand("facebook", command => command.AddCommands<FacebookCommands>());
app.Run();

public sealed class FacebookCommands(FacebookSampleSupport support)
{
    [Command("schema", Description = "Show all Facebook schemas or a single schema.")]
    public void Schema([Argument(Description = "Optional schema name.")] string? name = null)
        => support.PrintSchemas(name);

    [Command("configure", Description = "Prompt for Facebook credentials and save them to the local app configuration file.")]
    public void Configure()
        => support.Configure();

    [Command("validate", Description = "Validate a sample Facebook message.")]
    public void Validate([Option('k', Description = "Sample kind: text or media.")] string kind = "text")
        => support.PrintValidation(kind);

    [Command("status", Description = "Show the connector runtime status using saved credentials when available.")]
    public async Task Status()
        => support.PrintStatus("facebook status", await support.GetStatusAsync());

    [Command("receive", Description = "Parse a sample Facebook webhook payload.")]
    public async Task Receive([Option('f', Description = "Optional JSON file path.")] string? file = null)
        => support.PrintReceiveResult("facebook receive", await support.ReceiveAsync(file));

    [Command("send", Description = "Build and send a live Facebook message interactively.")]
    public async Task Send()
        => await support.SendAsync();
}

public sealed class FacebookSampleSupport(ILoggerFactory loggerFactory)
{
    private const string SectionName = "Facebook";

    private readonly SampleConfigurationStore configuration = new();
    private readonly ILoggerFactory loggerFactory = loggerFactory;

    private readonly Dictionary<string, IChannelSchema> schemas = new(StringComparer.OrdinalIgnoreCase)
    {
        ["FacebookMessenger"] = FacebookChannelSchemas.FacebookMessenger,
        ["SimpleMessenger"] = FacebookChannelSchemas.SimpleMessenger,
        ["NotificationMessenger"] = FacebookChannelSchemas.NotificationMessenger,
        ["MediaMessenger"] = FacebookChannelSchemas.MediaMessenger,
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
            new SampleConfigurationField("PageAccessToken", "Facebook page access token", IsSecret: true, IsRequired: true),
            new SampleConfigurationField("PageId", "Facebook page ID", IsRequired: true),
            new SampleConfigurationField("RecipientPsid", "Default recipient PSID", IsRequired: true),
            new SampleConfigurationField("WebhookUrl", "Webhook URL"),
            new SampleConfigurationField("VerifyToken", "Webhook verify token", IsSecret: true),
            new SampleConfigurationField("MediaUrl", "Default media URL"));
    }

    public void PrintValidation(string kind)
    {
        switch (kind.Trim().ToLowerInvariant())
        {
            case "text":
                PrintValidationResult(
                    "facebook validate text",
                    FacebookChannelSchemas.FacebookMessenger.ValidateMessage(CreateTextMessage("USER-PSID")));
                break;
            case "media":
                PrintValidationResult(
                    "facebook validate media",
                    FacebookChannelSchemas.MediaMessenger.ValidateMessage(CreateMediaMessage("USER-PSID", "https://example.com/welcome.png")));
                break;
            default:
                Console.WriteLine($"Unsupported validation kind '{kind}'. Use text or media.");
                break;
        }
    }

    public async Task<ConnectorResult<StatusInfo>> GetStatusAsync()
    {
        var connector = HasCredentials()
            ? CreateLiveConnector()
            : CreateOfflineConnector();

        await connector.InitializeAsync(CancellationToken.None);
        return await connector.GetStatusAsync(CancellationToken.None);
    }

    public async Task<ConnectorResult<ReceiveResult>> ReceiveAsync(string? file)
    {
        var connector = CreateOfflineConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.Successful)
            return ConnectorResult<ReceiveResult>.Fail(initialize.Error!);

        return await connector.ReceiveMessagesAsync(
            MessageSource.Json(ReadFileOrDefault(file, DefaultReceivePayload)),
            CancellationToken.None);
    }

    public async Task SendAsync()
    {
        if (!HasCredentials())
        {
            Console.WriteLine("Run 'facebook configure' or set FACEBOOK_PAGE_ACCESS_TOKEN and FACEBOOK_PAGE_ID.");
            return;
        }

        var connector = CreateLiveConnector();
        PrintResult("facebook initialize", await connector.InitializeAsync(CancellationToken.None));

        var recipient = SampleConsolePrompts.RequiredText(
            "Recipient PSID",
            GetValue("RecipientPsid", "FACEBOOK_RECIPIENT_PSID"));
        var kind = SampleConsolePrompts.Select(
            "Select the Facebook message type",
            ["Text", "Media"],
            GetValue("MediaUrl", "FACEBOOK_MEDIA_URL") is null ? "Text" : "Media");

        var message = kind == "Text"
            ? BuildTextMessage(recipient)
            : BuildMediaMessage(recipient);

        PrintSendResult($"facebook send {kind.ToLowerInvariant()}", await connector.SendMessageAsync(message, CancellationToken.None));
    }

    private bool HasCredentials()
        => HasValue("PageAccessToken", "FACEBOOK_PAGE_ACCESS_TOKEN") &&
           HasValue("PageId", "FACEBOOK_PAGE_ID");

    private bool HasValue(string key, string environmentName)
        => configuration.HasValue(SectionName, key, environmentName);

    private string GetRequired(string key, string environmentName)
        => configuration.GetRequired(SectionName, key, environmentName);

    private string? GetValue(string key, string environmentName)
        => configuration.GetValue(SectionName, key, environmentName);

    private FacebookMessengerConnector CreateOfflineConnector()
    {
        var settings = new ConnectionSettings(FacebookChannelSchemas.FacebookMessenger)
            .SetParameter("PageAccessToken", "sample-access-token")
            .SetParameter("PageId", "sample-page-id");

        return new FacebookMessengerConnector(
            FacebookChannelSchemas.FacebookMessenger,
            settings,
            logger: loggerFactory.CreateLogger<FacebookMessengerConnector>());
    }

    private FacebookMessengerConnector CreateLiveConnector()
    {
        var settings = new ConnectionSettings(FacebookChannelSchemas.FacebookMessenger)
            .SetParameter("PageAccessToken", GetRequired("PageAccessToken", "FACEBOOK_PAGE_ACCESS_TOKEN"))
            .SetParameter("PageId", GetRequired("PageId", "FACEBOOK_PAGE_ID"));

        AddIfPresent(settings, "WebhookUrl", GetValue("WebhookUrl", "FACEBOOK_WEBHOOK_URL"));
        AddIfPresent(settings, "VerifyToken", GetValue("VerifyToken", "FACEBOOK_VERIFY_TOKEN"));

        return new FacebookMessengerConnector(
            FacebookChannelSchemas.FacebookMessenger,
            settings,
            logger: loggerFactory.CreateLogger<FacebookMessengerConnector>());
    }

    private Message BuildTextMessage(string recipientId)
    {
        var includeQuickReplies = SampleConsolePrompts.Confirm("Include quick replies?", true);
        string? quickReplies = null;
        if (includeQuickReplies)
        {
            var firstTitle = SampleConsolePrompts.RequiredText("First quick reply title", "Start");
            var firstPayload = SampleConsolePrompts.RequiredText("First quick reply payload", "START_PAYLOAD");
            var secondTitle = SampleConsolePrompts.RequiredText("Second quick reply title", "Help");
            var secondPayload = SampleConsolePrompts.RequiredText("Second quick reply payload", "HELP_PAYLOAD");
            quickReplies = $$"""
                [{"content_type":"text","title":"{{firstTitle}}","payload":"{{firstPayload}}"},{"content_type":"text","title":"{{secondTitle}}","payload":"{{secondPayload}}"}]
                """;
        }

        return CreateTextMessage(
            recipientId,
            SampleConsolePrompts.RequiredText("Message ID", "facebook-text-sample"),
            SampleConsolePrompts.RequiredText("Message text", "Hello from the Deveel Facebook Messenger sample."),
            SampleConsolePrompts.Select("Messaging type", ["RESPONSE", "UPDATE", "MESSAGE_TAG"], "RESPONSE"),
            SampleConsolePrompts.Select("Notification type", ["REGULAR", "SILENT_PUSH", "NO_PUSH"], "REGULAR"),
            quickReplies);
    }

    private Message BuildMediaMessage(string recipientId)
    {
        var defaultMediaUrl = GetValue("MediaUrl", "FACEBOOK_MEDIA_URL") ?? "https://example.com/welcome.png";
        return CreateMediaMessage(
            recipientId,
            SampleConsolePrompts.RequiredText("Message ID", "facebook-media-sample"),
            SampleConsolePrompts.RequiredText("Media file name", "welcome.png"),
            SampleConsolePrompts.RequiredText("Media URL", defaultMediaUrl),
            SampleConsolePrompts.Select("Messaging type", ["UPDATE", "RESPONSE", "MESSAGE_TAG"], "UPDATE"));
    }

    private static Message CreateTextMessage(string recipientId)
        => CreateTextMessage(
            recipientId,
            "facebook-text-sample",
            "Hello from the Deveel Facebook Messenger sample.",
            "RESPONSE",
            "REGULAR",
            """
            [{"content_type":"text","title":"Start","payload":"START_PAYLOAD"},{"content_type":"text","title":"Help","payload":"HELP_PAYLOAD"}]
            """);

    private static Message CreateTextMessage(
        string recipientId,
        string id,
        string text,
        string messagingType,
        string notificationType,
        string? quickReplies)
    {
        var properties = new Dictionary<string, MessageProperty>
        {
            ["MessagingType"] = new("MessagingType", messagingType),
            ["NotificationType"] = new("NotificationType", notificationType)
        };

        if (!String.IsNullOrWhiteSpace(quickReplies))
        {
            properties["QuickReplies"] = new("QuickReplies", quickReplies);
        }

        return new Message
        {
            Id = id,
            Receiver = Endpoint.User(recipientId),
            Content = new TextContent(text),
            Properties = properties
        };
    }

    private static Message CreateMediaMessage(string recipientId, string mediaUrl)
        => CreateMediaMessage(recipientId, "facebook-media-sample", "welcome.png", mediaUrl, "UPDATE");

    private static Message CreateMediaMessage(
        string recipientId,
        string id,
        string fileName,
        string mediaUrl,
        string messagingType)
    {
        return new Message
        {
            Id = id,
            Receiver = Endpoint.User(recipientId),
            Content = new MediaContent(MediaType.Image, fileName, mediaUrl),
            Properties = new Dictionary<string, MessageProperty>
            {
                ["MessagingType"] = new("MessagingType", messagingType)
            }
        };
    }

    private static void PrintSchema(string name, IChannelSchema schema)
    {
        Console.WriteLine($"[{name}] {schema.DisplayName}");
        Console.WriteLine($"  Identity: {schema.GetLogicalIdentity()}");
        Console.WriteLine($"  Capabilities: {schema.Capabilities}");
        Console.WriteLine($"  Content Types: {JoinOrNone(schema.ContentTypes.Select(x => x.ToString()))}");
        Console.WriteLine($"  Parameters: {JoinOrNone(schema.Parameters.Select(x => $"{x.Name}:{x.DataType}{(x.IsRequired ? "*" : "")}"))}");
        Console.WriteLine($"  Endpoints: {JoinOrNone(schema.Endpoints.Select(x => $"{x.Type}(send={x.CanSend},receive={x.CanReceive},required={x.IsRequired})"))}");
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

    public void PrintStatus(string label, ConnectorResult<StatusInfo> result)
    {
        if (!result.Successful)
        {
            PrintResult(label, result);
            return;
        }

        Console.WriteLine($"{label}: {result.Value.Status} ({result.Value.Description})");
    }

    public void PrintReceiveResult(string label, ConnectorResult<ReceiveResult> result)
    {
        if (!result.Successful || result.Value is null)
        {
            PrintResult(label, result);
            return;
        }

        Console.WriteLine($"{label}: messages={result.Value.Messages.Count}");
        foreach (var message in result.Value.Messages)
        {
            Console.WriteLine($"  - id={message.Id}, from={message.Sender?.Address}, to={message.Receiver?.Address}, content={message.Content?.GetType().Name}");
        }
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

    private static string ReadFileOrDefault(string? file, string defaultValue)
        => String.IsNullOrWhiteSpace(file) ? defaultValue : File.ReadAllText(file);

    private static string JoinOrNone(IEnumerable<string> values)
    {
        var items = values.Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
        return items.Length == 0 ? "none" : String.Join(", ", items);
    }

    private const string DefaultReceivePayload = """
        {
          "object": "page",
          "entry": [
            {
              "id": "PAGE-ID",
              "time": 1710000000,
              "messaging": [
                {
                  "sender": { "id": "USER-PSID" },
                  "recipient": { "id": "PAGE-ID" },
                  "timestamp": 1710000001,
                  "message": {
                    "mid": "m_sample",
                    "text": "Hello from Facebook webhook"
                  }
                }
              ]
            }
          ]
        }
        """;
}
