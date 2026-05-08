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
builder.Services.AddSingleton<FirebaseSampleSupport>();

var app = builder.Build();
app.AddSubCommand("firebase", command => command.AddCommands<FirebaseCommands>());
app.Run();

public sealed class FirebaseCommands(FirebaseSampleSupport support)
{
    [Command("schema", Description = "Show all Firebase schemas or a single schema.")]
    public void Schema([Argument(Description = "Optional schema name.")] string? name = null)
        => support.PrintSchemas(name);

    [Command("configure", Description = "Prompt for Firebase credentials and save them to the local app configuration file.")]
    public void Configure()
        => support.Configure();

    [Command("validate", Description = "Validate a sample Firebase message.")]
    public void Validate([Option('k', Description = "Sample kind: device or topic.")] string kind = "device")
        => support.PrintValidation(kind);

    [Command("send", Description = "Build and send a live Firebase notification interactively.")]
    public async Task Send()
        => await support.SendAsync();

    [Command("batch", Description = "Send a live Firebase batch to a device token.")]
    public async Task Batch()
        => await support.SendBatchAsync();

    [Command("status", Description = "Show Firebase connector status using saved credentials when available.")]
    public async Task Status()
        => await support.PrintStatusAsync();
}

public sealed class FirebaseSampleSupport(ILoggerFactory loggerFactory)
{
    private const string SectionName = "Firebase";

    private readonly SampleConfigurationStore configuration = new();
    private readonly ILoggerFactory loggerFactory = loggerFactory;

    private readonly Dictionary<string, IChannelSchema> schemas = new(StringComparer.OrdinalIgnoreCase)
    {
        ["FirebasePush"] = FirebaseChannelSchemas.FirebasePush,
        ["SimplePush"] = FirebaseChannelSchemas.SimplePush,
        ["RichPush"] = FirebaseChannelSchemas.RichPush,
        ["BulkPush"] = FirebaseChannelSchemas.BulkPush,
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
            new SampleConfigurationField("ProjectId", "Firebase project ID", IsRequired: true),
            new SampleConfigurationField("ServiceAccountKey", "Service account key JSON or path", IsSecret: true, IsRequired: true),
            new SampleConfigurationField("DeviceToken", "Default device token"),
            new SampleConfigurationField("Topic", "Default topic"),
            new SampleConfigurationField("DryRun", "Dry run (true/false)"));
    }

    public void PrintValidation(string kind)
    {
        switch (kind.Trim().ToLowerInvariant())
        {
            case "device":
                PrintValidationResult(
                    "firebase validate device",
                    FirebaseChannelSchemas.RichPush.ValidateMessage(CreateDeviceMessage("DEVICE_TOKEN")));
                break;
            case "topic":
                PrintValidationResult(
                    "firebase validate topic",
                    FirebaseChannelSchemas.BulkPush.ValidateMessage(CreateTopicMessage("daily-news")));
                break;
            default:
                Console.WriteLine($"Unsupported validation kind '{kind}'. Use device or topic.");
                break;
        }
    }

    public async Task SendAsync()
    {
        if (!HasCredentials())
        {
            Console.WriteLine("Run 'firebase configure' or set FIREBASE_PROJECT_ID and FIREBASE_SERVICE_ACCOUNT_KEY.");
            return;
        }

        var connector = CreateLiveConnector();
        PrintResult("firebase initialize", await connector.InitializeAsync(CancellationToken.None));

        var message = BuildFirebaseMessage();
        PrintSendResult("firebase send", await connector.SendMessageAsync(message, CancellationToken.None));
    }

    public async Task SendBatchAsync()
    {
        if (!HasCredentials())
        {
            Console.WriteLine("Run 'firebase configure' or set FIREBASE_PROJECT_ID and FIREBASE_SERVICE_ACCOUNT_KEY.");
            return;
        }

        var connector = CreateLiveConnector();
        PrintResult("firebase initialize", await connector.InitializeAsync(CancellationToken.None));

        var batch = BuildBatch();
        var result = await connector.SendBatchAsync(batch, CancellationToken.None);
        if (!result.Successful || result.Value is null)
        {
            PrintResult("firebase batch", result);
            return;
        }

        Console.WriteLine($"firebase batch: messages={result.Value.MessageResults.Count}");
    }

    public async Task PrintStatusAsync()
    {
        if (!HasCredentials())
        {
            Console.WriteLine("Run 'firebase configure' or set FIREBASE_PROJECT_ID and FIREBASE_SERVICE_ACCOUNT_KEY.");
            return;
        }

        var connector = CreateLiveConnector();
        PrintResult("firebase initialize", await connector.InitializeAsync(CancellationToken.None));
        PrintStatus("firebase status", await connector.GetStatusAsync(CancellationToken.None));
    }

    private bool HasCredentials()
        => HasValue("ProjectId", "FIREBASE_PROJECT_ID") &&
           HasValue("ServiceAccountKey", "FIREBASE_SERVICE_ACCOUNT_KEY");

    private bool HasValue(string key, string environmentName)
        => configuration.HasValue(SectionName, key, environmentName);

    private string GetRequired(string key, string environmentName)
        => configuration.GetRequired(SectionName, key, environmentName);

    private string? GetValue(string key, string environmentName)
        => configuration.GetValue(SectionName, key, environmentName);

    private FirebasePushConnector CreateLiveConnector()
    {
        var settings = new ConnectionSettings(FirebaseChannelSchemas.BulkPush)
            .SetParameter("ProjectId", GetRequired("ProjectId", "FIREBASE_PROJECT_ID"))
            .SetParameter("ServiceAccountKey", GetRequired("ServiceAccountKey", "FIREBASE_SERVICE_ACCOUNT_KEY"))
            .SetParameter("DryRun", configuration.GetBool(SectionName, "DryRun", "FIREBASE_DRY_RUN", true));

        return new FirebasePushConnector(
            FirebaseChannelSchemas.BulkPush,
            settings,
            logger: loggerFactory.CreateLogger<FirebasePushConnector>());
    }

    private static Message CreateDeviceMessage(string deviceToken)
        => CreateDeviceMessage(
            "firebase-device-sample",
            deviceToken,
            "Hello from the Deveel Firebase sample.",
            "Firebase sample",
            "high",
            "https://example.com/fcm.png",
            "demo",
            """{"source":"sample","channel":"firebase"}""");

    private static Message CreateDeviceMessage(
        string id,
        string deviceToken,
        string body,
        string title,
        string priority,
        string? imageUrl,
        string? tag,
        string? customData)
    {
        var properties = new Dictionary<string, MessageProperty>
        {
            ["Title"] = new("Title", title),
            ["Priority"] = new("Priority", priority)
        };

        if (!String.IsNullOrWhiteSpace(imageUrl))
            properties["ImageUrl"] = new("ImageUrl", imageUrl);
        if (!String.IsNullOrWhiteSpace(tag))
            properties["Tag"] = new("Tag", tag);
        if (!String.IsNullOrWhiteSpace(customData))
            properties["CustomData"] = new("CustomData", customData);

        return new Message
        {
            Id = id,
            Receiver = Endpoint.Device(deviceToken),
            Content = new TextContent(body),
            Properties = properties
        };
    }

    private static Message CreateTopicMessage(string topic)
        => CreateTopicMessage(
            "firebase-topic-sample",
            topic,
            "A topic notification prepared by the Firebase sample.",
            "Topic sample",
            "normal");

    private static Message CreateTopicMessage(
        string id,
        string topic,
        string body,
        string title,
        string priority)
    {
        return new Message
        {
            Id = id,
            Receiver = new Endpoint(EndpointType.Topic, topic),
            Content = new TextContent(body),
            Properties = new Dictionary<string, MessageProperty>
            {
                ["Title"] = new("Title", title),
                ["Priority"] = new("Priority", priority)
            }
        };
    }

    private Message BuildFirebaseMessage()
    {
        var kind = SampleConsolePrompts.Select(
            "Select the Firebase target type",
            ["Device", "Topic"],
            HasValue("DeviceToken", "FIREBASE_DEVICE_TOKEN") ? "Device" : "Topic");

        return kind == "Device"
            ? CreateDeviceMessage(
                SampleConsolePrompts.RequiredText("Message ID", "firebase-device-sample"),
                SampleConsolePrompts.RequiredText("Device token", GetValue("DeviceToken", "FIREBASE_DEVICE_TOKEN")),
                SampleConsolePrompts.RequiredText("Notification body", "Hello from the Deveel Firebase sample."),
                SampleConsolePrompts.RequiredText("Notification title", "Firebase sample"),
                SampleConsolePrompts.Select("Priority", ["high", "normal"], "high"),
                SampleConsolePrompts.OptionalText("Image URL", "https://example.com/fcm.png"),
                SampleConsolePrompts.OptionalText("Tag", "demo"),
                SampleConsolePrompts.OptionalText("Custom data JSON", """{"source":"sample","channel":"firebase"}"""))
            : CreateTopicMessage(
                SampleConsolePrompts.RequiredText("Message ID", "firebase-topic-sample"),
                SampleConsolePrompts.RequiredText("Topic", GetValue("Topic", "FIREBASE_TOPIC")),
                SampleConsolePrompts.RequiredText("Notification body", "A topic notification prepared by the Firebase sample."),
                SampleConsolePrompts.RequiredText("Notification title", "Topic sample"),
                SampleConsolePrompts.Select("Priority", ["normal", "high"], "normal"));
    }

    private MessageBatch BuildBatch()
    {
        var count = SampleConsolePrompts.RequiredInt("How many messages should be included in the batch?", 2);
        var batch = new MessageBatch
        {
            Id = SampleConsolePrompts.RequiredText("Batch ID", "firebase-batch-sample")
        };

        for (var i = 0; i < count; i++)
        {
            Console.WriteLine($"Message {i + 1} of {count}");
            batch.Messages.Add(BuildFirebaseMessage());
        }

        return batch;
    }

    private static MessageBatch CreateBatch(string deviceToken)
    {
        return new MessageBatch
        {
            Id = "firebase-batch-sample",
            Messages =
            {
                CreateDeviceMessage(deviceToken),
                new Message
                {
                    Id = "firebase-device-sample-2",
                    Receiver = Endpoint.Device(deviceToken),
                    Content = new TextContent("Second push notification from the Deveel Firebase batch sample."),
                    Properties = new Dictionary<string, MessageProperty>
                    {
                        ["Title"] = new("Title", "Firebase batch"),
                        ["Priority"] = new("Priority", "high")
                    }
                }
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

        Console.WriteLine($"{label}: {result.Value.Status} ({result.Value.Description})");
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

    private static string JoinOrNone(IEnumerable<string> values)
    {
        var items = values.Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
        return items.Length == 0 ? "none" : String.Join(", ", items);
    }
}
