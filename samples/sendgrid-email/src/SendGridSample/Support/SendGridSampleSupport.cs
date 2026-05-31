using System.ComponentModel.DataAnnotations;
using Deveel;
using Ratatosk;
using Microsoft.Extensions.Logging;

namespace SendGridSample;

public sealed class SendGridSampleSupport(ILoggerFactory loggerFactory, IMessagingClient client)
{
    private const string SectionName = "SendGrid";

    private readonly SampleConfigurationStore configuration = new();
    private readonly ILoggerFactory loggerFactory = loggerFactory;
    private readonly IMessagingClient client = client;

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
            new SampleConfigurationField("ApiKey", "SendGrid API key", IsSecret: true, IsRequired: true),
            new SampleConfigurationField("FromEmail", "Default sender email", IsRequired: true),
            new SampleConfigurationField("ToEmail", "Default recipient email"),
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
                SampleOutputHelper.PrintValidationResult(
                    "sendgrid validate html",
                    SendGridChannelSchemas.TransactionalEmail.ValidateMessage(CreateHtmlMessage("sender@example.com", "recipient@example.com")));
                break;
            case "template":
                SampleOutputHelper.PrintValidationResult(
                    "sendgrid validate template",
                    SendGridChannelSchemas.TemplateEmail.ValidateMessage(CreateTemplateMessage("sender@example.com", "recipient@example.com", "d-sample-template")));
                break;
            default:
                Console.WriteLine($"Unsupported validation kind '{kind}'. Use html or template.");
                break;
        }
    }

    public async Task<OperationResult<StatusInfo>> GetStatusAsync()
    {
        return await client.GetStatusAsync("sendgrid", CancellationToken.None);
    }

    public async Task<OperationResult<ReceiveResult>> ReceiveAsync(string? file, string mode)
    {
        var connector = CreateOfflineConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.IsSuccess())
            return OperationResult<ReceiveResult>.Fail(initialize.Error!.Code, initialize.Error!.Domain, initialize.Error!.Message);

        return await connector.ReceiveMessagesAsync(CreateMessageSource(file, mode, DefaultInboundJson, DefaultInboundForm), CancellationToken.None);
    }

    public async Task<OperationResult<StatusUpdateResult>> ReceiveStatusAsync(string? file, string mode)
    {
        var connector = CreateOfflineConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.IsSuccess())
            return OperationResult<StatusUpdateResult>.Fail(initialize.Error!.Code, initialize.Error!.Domain, initialize.Error!.Message);

        return await connector.ReceiveMessageStatusAsync(CreateMessageSource(file, mode, DefaultStatusJson, DefaultStatusForm), CancellationToken.None);
    }

    public async Task SendAsync()
    {
        if (!HasCredentials())
        {
            Console.WriteLine("Run 'sendgrid configure' or set SENDGRID_API_KEY.");
            return;
        }

        var message = BuildSendGridMessage();
        if (message is null)
            return;

        SampleOutputHelper.PrintSendResult("sendgrid send", await client.SendAsync("sendgrid", message, CancellationToken.None));
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

        SampleOutputHelper.AddIfPresent(settings, "WebhookUrl", GetValue("WebhookUrl", "SENDGRID_WEBHOOK_URL"));
        SampleOutputHelper.AddIfPresent(settings, "DefaultFromName", GetValue("FromName", "SENDGRID_FROM_NAME"));
        SampleOutputHelper.AddIfPresent(settings, "DefaultReplyTo", GetValue("ReplyTo", "SENDGRID_REPLY_TO"));

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
            "Ratatosk SendGrid sample",
            "<p>Hello from the <strong>Ratatosk SendGrid</strong> sample.</p>",
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
            "Ratatosk",
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

    private Message? BuildSendGridMessage()
    {
        var sender = SampleConsolePrompts.RequiredText("Sender email", GetValue("FromEmail", "SENDGRID_FROM_EMAIL"));
        var recipient = SampleConsolePrompts.OptionalText("Recipient email", GetValue("ToEmail", "SENDGRID_TO_EMAIL"));

        if (String.IsNullOrWhiteSpace(recipient))
        {
            Console.WriteLine("No recipient email provided. Aborting send.");
            return null;
        }

        var kind = SampleConsolePrompts.Select(
            "Select the SendGrid message type",
            ["Html", "Template"],
            HasValue("TemplateId", "SENDGRID_TEMPLATE_ID") ? "Template" : "Html");

        return kind == "Html"
            ? CreateHtmlMessage(
                SampleConsolePrompts.RequiredText("Message ID", "sendgrid-html-sample"),
                sender,
                recipient,
                SampleConsolePrompts.RequiredText("Subject", "Ratatosk SendGrid sample"),
                SampleConsolePrompts.MultiLineBody("HTML body", "<p>Hello from the <strong>Ratatosk SendGrid</strong> sample.</p>"),
                SampleConsolePrompts.Select("Priority", ["high", "normal", "low"], "high"),
                SampleConsolePrompts.OptionalText("Categories", "samples,demo"),
                SampleConsolePrompts.OptionalText("Custom args JSON", """{"source":"sample","connector":"sendgrid"}"""))
            : CreateTemplateMessage(
                SampleConsolePrompts.RequiredText("Message ID", "sendgrid-template-sample"),
                sender,
                recipient,
                SampleConsolePrompts.RequiredText("Template ID", GetValue("TemplateId", "SENDGRID_TEMPLATE_ID")),
                SampleConsolePrompts.RequiredText("Subject", "Template sample"),
                SampleConsolePrompts.RequiredText("Template variable 'firstName'", "Ratatosk"),
                SampleConsolePrompts.RequiredText("Template variable 'connector'", "SendGrid"),
                SampleConsolePrompts.OptionalText("Categories", "templates,samples"));
    }

    private static MessageSource CreateMessageSource(string? file, string mode, string jsonPayload, string formPayload)
    {
        var payload = SampleOutputHelper.ReadFileOrDefault(file, mode.Equals("form", StringComparison.OrdinalIgnoreCase) ? formPayload : jsonPayload);
        return mode.Equals("form", StringComparison.OrdinalIgnoreCase)
            ? MessageSource.UrlPost(payload)
            : MessageSource.Json(payload);
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
            var subject = message.Properties is not null &&
                message.Properties.TryGetValue("Subject", out var subjectProperty)
                ? subjectProperty.Value
                : "n/a";
            Console.WriteLine($"  - id={message.Id}, from={message.Sender?.Address}, subject={subject}");
        }
    }

    public void PrintStatusUpdateResult(string label, OperationResult<StatusUpdateResult> result)
    {
        if (!result.IsSuccess() || result.Value is null)
        {
            SampleOutputHelper.PrintResult(label, result);
            return;
        }

        Console.WriteLine($"{label}: id={result.Value.MessageId}, status={result.Value.Status}");
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
