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
builder.Services.AddSingleton<SendGridSampleSupport>();

var app = builder.Build();
app.AddSubCommand("sendgrid", command => command.AddCommands<SendGridCommands>());
app.Run();

public sealed class SendGridCommands(SendGridSampleSupport support)
{
    [Command("schema", Description = "Show all SendGrid schemas or a single schema.")]
    public void Schema([Argument(Description = "Optional schema name.")] string? name = null)
        => support.PrintSchemas(name);

    [Command("configure", Description = "Prompt for SendGrid credentials and save them to the local app configuration file.")]
    public void Configure()
        => support.Configure();

    [Command("validate", Description = "Validate a sample SendGrid message.")]
    public void Validate([Option('k', Description = "Sample kind: html or template.")] string kind = "html")
        => support.PrintValidation(kind);

    [Command("status", Description = "Show the connector runtime status using saved credentials when available.")]
    public async Task Status()
        => support.PrintStatus("sendgrid status", await support.GetStatusAsync());

    [Command("receive", Description = "Parse a sample inbound SendGrid webhook.")]
    public async Task Receive(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: json or form.")] string mode = "json")
        => support.PrintReceiveResult($"sendgrid receive {mode}", await support.ReceiveAsync(file, mode));

    [Command("receive-status", Description = "Parse a sample SendGrid delivery event callback.")]
    public async Task ReceiveStatus(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: json or form.")] string mode = "json")
        => support.PrintStatusUpdateResult($"sendgrid receive-status {mode}", await support.ReceiveStatusAsync(file, mode));

    [Command("send", Description = "Build and send a live SendGrid message interactively.")]
    public async Task Send()
        => await support.SendAsync();
}

public sealed class SendGridSampleSupport(ILoggerFactory loggerFactory)
{
    private const string SectionName = "SendGrid";

    private readonly SampleConfigurationStore configuration = new();
    private readonly ILoggerFactory loggerFactory = loggerFactory;

    private readonly Dictionary<string, IChannelSchema> schemas = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SendGridEmail"] = SendGridChannelSchemas.SendGridEmail,
        ["SimpleEmail"] = SendGridChannelSchemas.SimpleEmail,
        ["TransactionalEmail"] = SendGridChannelSchemas.TransactionalEmail,
        ["MarketingEmail"] = SendGridChannelSchemas.MarketingEmail,
        ["TemplateEmail"] = SendGridChannelSchemas.TemplateEmail,
        ["BulkEmail"] = SendGridChannelSchemas.BulkEmail,
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
            new SampleConfigurationField("ApiKey", "SendGrid API key", IsSecret: true, IsRequired: true),
            new SampleConfigurationField("FromEmail", "Default sender email", IsRequired: true),
            new SampleConfigurationField("ToEmail", "Default recipient email", IsRequired: true),
            new SampleConfigurationField("SandboxMode", "Sandbox mode (true/false)"),
            new SampleConfigurationField("WebhookUrl", "Webhook URL"),
            new SampleConfigurationField("FromName", "Default sender name"),
            new SampleConfigurationField("ReplyTo", "Default reply-to address"),
            new SampleConfigurationField("TemplateId", "Default template ID"));
    }

    public void PrintValidation(string kind)
    {
        switch (kind.Trim().ToLowerInvariant())
        {
            case "html":
                PrintValidationResult(
                    "sendgrid validate html",
                    SendGridChannelSchemas.TransactionalEmail.ValidateMessage(CreateHtmlMessage("sender@example.com", "recipient@example.com")));
                break;
            case "template":
                PrintValidationResult(
                    "sendgrid validate template",
                    SendGridChannelSchemas.TemplateEmail.ValidateMessage(CreateTemplateMessage("sender@example.com", "recipient@example.com", "d-sample-template")));
                break;
            default:
                Console.WriteLine($"Unsupported validation kind '{kind}'. Use html or template.");
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

    public async Task<ConnectorResult<ReceiveResult>> ReceiveAsync(string? file, string mode)
    {
        var connector = CreateOfflineConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.Successful)
            return ConnectorResult<ReceiveResult>.Fail(initialize.Error!);

        return await connector.ReceiveMessagesAsync(CreateMessageSource(file, mode, DefaultInboundJson, DefaultInboundForm), CancellationToken.None);
    }

    public async Task<ConnectorResult<StatusUpdateResult>> ReceiveStatusAsync(string? file, string mode)
    {
        var connector = CreateOfflineConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.Successful)
            return ConnectorResult<StatusUpdateResult>.Fail(initialize.Error!);

        return await connector.ReceiveMessageStatusAsync(CreateMessageSource(file, mode, DefaultStatusJson, DefaultStatusForm), CancellationToken.None);
    }

    public async Task SendAsync()
    {
        if (!HasCredentials())
        {
            Console.WriteLine("Run 'sendgrid configure' or set SENDGRID_API_KEY.");
            return;
        }

        var connector = CreateLiveConnector();
        PrintResult("sendgrid initialize", await connector.InitializeAsync(CancellationToken.None));

        var message = BuildSendGridMessage();
        PrintSendResult("sendgrid send", await connector.SendMessageAsync(message, CancellationToken.None));
    }

    private bool HasCredentials()
        => HasValue("ApiKey", "SENDGRID_API_KEY");

    private bool HasValue(string key, string environmentName)
        => configuration.HasValue(SectionName, key, environmentName);

    private string GetRequired(string key, string environmentName)
        => configuration.GetRequired(SectionName, key, environmentName);

    private string? GetValue(string key, string environmentName)
        => configuration.GetValue(SectionName, key, environmentName);

    private SendGridEmailConnector CreateOfflineConnector()
    {
        var settings = new ConnectionSettings(SendGridChannelSchemas.SendGridEmail)
            .SetParameter("ApiKey", "sample-sendgrid-key")
            .SetParameter("SandboxMode", true);

        return new SendGridEmailConnector(
            SendGridChannelSchemas.SendGridEmail,
            settings,
            logger: loggerFactory.CreateLogger<SendGridEmailConnector>());
    }

    private SendGridEmailConnector CreateLiveConnector()
    {
        var settings = new ConnectionSettings(SendGridChannelSchemas.SendGridEmail)
            .SetParameter("ApiKey", GetRequired("ApiKey", "SENDGRID_API_KEY"))
            .SetParameter("SandboxMode", configuration.GetBool(SectionName, "SandboxMode", "SENDGRID_SANDBOX_MODE", true))
            .SetParameter("TrackingSettings", true);

        AddIfPresent(settings, "WebhookUrl", GetValue("WebhookUrl", "SENDGRID_WEBHOOK_URL"));
        AddIfPresent(settings, "DefaultFromName", GetValue("FromName", "SENDGRID_FROM_NAME"));
        AddIfPresent(settings, "DefaultReplyTo", GetValue("ReplyTo", "SENDGRID_REPLY_TO"));

        return new SendGridEmailConnector(
            SendGridChannelSchemas.SendGridEmail,
            settings,
            logger: loggerFactory.CreateLogger<SendGridEmailConnector>());
    }

    private static Message CreateHtmlMessage(string sender, string recipient)
        => CreateHtmlMessage(
            "sendgrid-html-sample",
            sender,
            recipient,
            "Deveel SendGrid sample",
            "<p>Hello from the <strong>Deveel SendGrid</strong> sample.</p>",
            "high",
            "samples,demo",
            """{"source":"sample","connector":"sendgrid"}""");

    private static Message CreateHtmlMessage(
        string id,
        string sender,
        string recipient,
        string subject,
        string htmlBody,
        string priority,
        string? categories,
        string? customArgs)
    {
        var properties = new Dictionary<string, MessageProperty>
        {
            ["Subject"] = new("Subject", subject),
            ["Priority"] = new("Priority", priority)
        };

        if (!String.IsNullOrWhiteSpace(categories))
            properties["Categories"] = new("Categories", categories);
        if (!String.IsNullOrWhiteSpace(customArgs))
            properties["CustomArgs"] = new("CustomArgs", customArgs);

        return new Message
        {
            Id = id,
            Sender = Endpoint.EmailAddress(sender),
            Receiver = Endpoint.EmailAddress(recipient),
            Content = new HtmlContent(htmlBody),
            Properties = properties
        };
    }

    private static Message CreateTemplateMessage(string sender, string recipient, string templateId)
        => CreateTemplateMessage(
            "sendgrid-template-sample",
            sender,
            recipient,
            templateId,
            "Template sample",
            "Deveel",
            "SendGrid",
            "templates,samples");

    private static Message CreateTemplateMessage(
        string id,
        string sender,
        string recipient,
        string templateId,
        string subject,
        string firstName,
        string connectorName,
        string? categories)
    {
        var properties = new Dictionary<string, MessageProperty>
        {
            ["Subject"] = new("Subject", subject)
        };

        if (!String.IsNullOrWhiteSpace(categories))
            properties["Categories"] = new("Categories", categories);

        return new Message
        {
            Id = id,
            Sender = Endpoint.EmailAddress(sender),
            Receiver = Endpoint.EmailAddress(recipient),
            Content = new TemplateContent(templateId, new Dictionary<string, object?>
            {
                ["firstName"] = firstName,
                ["connector"] = connectorName
            }),
            Properties = properties
        };
    }

    private Message BuildSendGridMessage()
    {
        var sender = SampleConsolePrompts.RequiredText("Sender email", GetValue("FromEmail", "SENDGRID_FROM_EMAIL"));
        var recipient = SampleConsolePrompts.RequiredText("Recipient email", GetValue("ToEmail", "SENDGRID_TO_EMAIL"));
        var kind = SampleConsolePrompts.Select(
            "Select the SendGrid message type",
            ["Html", "Template"],
            HasValue("TemplateId", "SENDGRID_TEMPLATE_ID") ? "Template" : "Html");

        return kind == "Html"
            ? CreateHtmlMessage(
                SampleConsolePrompts.RequiredText("Message ID", "sendgrid-html-sample"),
                sender,
                recipient,
                SampleConsolePrompts.RequiredText("Subject", "Deveel SendGrid sample"),
                SampleConsolePrompts.RequiredText("HTML body", "<p>Hello from the <strong>Deveel SendGrid</strong> sample.</p>"),
                SampleConsolePrompts.Select("Priority", ["high", "normal", "low"], "high"),
                SampleConsolePrompts.OptionalText("Categories", "samples,demo"),
                SampleConsolePrompts.OptionalText("Custom args JSON", """{"source":"sample","connector":"sendgrid"}"""))
            : CreateTemplateMessage(
                SampleConsolePrompts.RequiredText("Message ID", "sendgrid-template-sample"),
                sender,
                recipient,
                SampleConsolePrompts.RequiredText("Template ID", GetValue("TemplateId", "SENDGRID_TEMPLATE_ID")),
                SampleConsolePrompts.RequiredText("Subject", "Template sample"),
                SampleConsolePrompts.RequiredText("Template variable 'firstName'", "Deveel"),
                SampleConsolePrompts.RequiredText("Template variable 'connector'", "SendGrid"),
                SampleConsolePrompts.OptionalText("Categories", "templates,samples"));
    }

    private static MessageSource CreateMessageSource(string? file, string mode, string jsonPayload, string formPayload)
    {
        var payload = ReadFileOrDefault(file, mode.Equals("form", StringComparison.OrdinalIgnoreCase) ? formPayload : jsonPayload);
        return mode.Equals("form", StringComparison.OrdinalIgnoreCase)
            ? MessageSource.UrlPost(payload)
            : MessageSource.Json(payload);
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
            var subject = message.Properties is not null &&
                message.Properties.TryGetValue("Subject", out var subjectProperty)
                ? subjectProperty.Value
                : "n/a";
            Console.WriteLine($"  - id={message.Id}, from={message.Sender?.Address}, subject={subject}");
        }
    }

    public void PrintStatusUpdateResult(string label, ConnectorResult<StatusUpdateResult> result)
    {
        if (!result.Successful || result.Value is null)
        {
            PrintResult(label, result);
            return;
        }

        Console.WriteLine($"{label}: id={result.Value.MessageId}, status={result.Value.Status}");
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

    private const string DefaultInboundJson = """
        {
          "headers": {
            "from": "sender@example.com",
            "subject": "SendGrid inbound sample"
          },
          "from": "sender@example.com",
          "to": "recipient@example.com",
          "subject": "SendGrid inbound sample",
          "text": "Hello from an inbound SendGrid event"
        }
        """;

    private const string DefaultInboundForm = "from=sender%40example.com&to=recipient%40example.com&subject=SendGrid+inbound+sample&text=Hello+from+an+inbound+SendGrid+event";

    private const string DefaultStatusJson = """
        [
          {
            "email": "recipient@example.com",
            "event": "delivered",
            "timestamp": 1710000000,
            "sg_message_id": "sendgrid-sample-id"
          }
        ]
        """;

    private const string DefaultStatusForm = "event=delivered&timestamp=1710000000&sg_message_id=sendgrid-sample-id&email=recipient%40example.com";
}
