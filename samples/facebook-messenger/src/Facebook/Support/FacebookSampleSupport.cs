using System.ComponentModel.DataAnnotations;
using Deveel;
using Deveel.Messaging;
using Microsoft.Extensions.Logging;

namespace Facebook;

public sealed class FacebookSampleSupport(ILoggerFactory loggerFactory, IMessagingClient client)
{
    private const string SectionName = "Facebook";

    private readonly SampleConfigurationStore configuration = new();
    private readonly ILoggerFactory loggerFactory = loggerFactory;
    private readonly IMessagingClient client = client;

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
                SampleOutputHelper.PrintValidationResult(
                    "facebook validate text",
                    FacebookChannelSchemas.FacebookMessenger.ValidateMessage(CreateTextMessage("USER-PSID")));
                break;
            case "media":
                SampleOutputHelper.PrintValidationResult(
                    "facebook validate media",
                    FacebookChannelSchemas.MediaMessenger.ValidateMessage(CreateMediaMessage("USER-PSID", "https://example.com/welcome.png")));
                break;
            default:
                Console.WriteLine($"Unsupported validation kind '{kind}'. Use text or media.");
                break;
        }
    }

    public async Task<OperationResult<StatusInfo>> GetStatusAsync()
    {
        return await client.GetStatusAsync("facebook", CancellationToken.None);
    }

    public async Task<OperationResult<ReceiveResult>> ReceiveAsync(string? file)
    {
        var connector = CreateOfflineConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.IsSuccess())
            return OperationResult<ReceiveResult>.Fail(initialize.Error!.Code, initialize.Error!.Domain, initialize.Error!.Message);

        return await connector.ReceiveMessagesAsync(
            MessageSource.Json(SampleOutputHelper.ReadFileOrDefault(file, DefaultReceivePayload)),
            CancellationToken.None);
    }

    public async Task SendAsync()
    {
        if (!HasCredentials())
        {
            Console.WriteLine("Run 'facebook configure' or set FACEBOOK_PAGE_ACCESS_TOKEN and FACEBOOK_PAGE_ID.");
            return;
        }

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

        SampleOutputHelper.PrintSendResult($"facebook send {kind.ToLowerInvariant()}", await client.SendAsync("facebook", message, CancellationToken.None));
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

        SampleOutputHelper.AddIfPresent(settings, "WebhookUrl", GetValue("WebhookUrl", "FACEBOOK_WEBHOOK_URL"));
        SampleOutputHelper.AddIfPresent(settings, "VerifyToken", GetValue("VerifyToken", "FACEBOOK_VERIFY_TOKEN"));

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
            SampleConsolePrompts.MultiLineBody("Message text", "Hello from the Deveel Facebook Messenger sample."),
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

    public void PrintStatus(string label, OperationResult<StatusInfo> result)
    {
        if (!result.IsSuccess())
        {
            SampleOutputHelper.PrintResult(label, result);
            return;
        }

        Console.WriteLine($"{label}: {result.Value.Status} ({result.Value.Description})");
    }

    public void PrintReceiveResult(string label, OperationResult<ReceiveResult> result)
    {
        if (!result.IsSuccess() || result.Value is null)
        {
            SampleOutputHelper.PrintResult(label, result);
            return;
        }

        Console.WriteLine($"{label}: messages={result.Value.Messages.Count}");
        foreach (var message in result.Value.Messages)
        {
            Console.WriteLine($"  - id={message.Id}, from={message.Sender?.Address}, to={message.Receiver?.Address}, content={message.Content?.GetType().Name}");
        }
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
