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
builder.Services.AddSingleton<TwilioSampleSupport>();

var app = builder.Build();
app.AddSubCommand("twilio", command =>
{
    command.AddCommands<TwilioRootCommands>();
    command.AddSubCommand("sms", sms => sms.AddCommands<TwilioSmsCommands>());
    command.AddSubCommand("whatsapp", whatsapp => whatsapp.AddCommands<TwilioWhatsAppCommands>());
});
app.Run();

public sealed class TwilioRootCommands(TwilioSampleSupport support)
{
    [Command("schema", Description = "Show all Twilio schemas or a single schema.")]
    public void Schema([Argument(Description = "Optional schema name.")] string? name = null)
        => support.PrintSchemas(name);

    [Command("configure", Description = "Prompt for Twilio credentials and save them to the local app configuration file.")]
    public void Configure()
        => support.Configure();
}

public sealed class TwilioSmsCommands(TwilioSampleSupport support)
{
    [Command("validate", Description = "Validate a sample SMS message.")]
    public void Validate()
        => support.PrintSmsValidation();

    [Command("status", Description = "Show SMS connector runtime status using saved credentials when available.")]
    public async Task Status()
        => support.PrintStatus("twilio sms status", await support.GetSmsStatusAsync());

    [Command("receive", Description = "Parse a sample Twilio SMS webhook payload.")]
    public async Task Receive(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: form or json.")] string mode = "form")
        => support.PrintReceiveResult($"twilio sms receive {mode}", await support.ReceiveSmsAsync(file, mode));

    [Command("receive-status", Description = "Parse a sample Twilio SMS status callback.")]
    public async Task ReceiveStatus(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: form or json.")] string mode = "form")
        => support.PrintStatusUpdateResult($"twilio sms receive-status {mode}", await support.ReceiveSmsStatusAsync(file, mode));

    [Command("send", Description = "Send a live Twilio SMS message using saved or environment-based settings.")]
    public async Task Send()
        => await support.SendSmsAsync();
}

public sealed class TwilioWhatsAppCommands(TwilioSampleSupport support)
{
    [Command("validate", Description = "Validate a sample WhatsApp message.")]
    public void Validate([Option('k', Description = "Sample kind: text or template.")] string kind = "text")
        => support.PrintWhatsAppValidation(kind);

    [Command("status", Description = "Show WhatsApp connector runtime status using saved credentials when available.")]
    public async Task Status()
        => support.PrintStatus("twilio whatsapp status", await support.GetWhatsAppStatusAsync());

    [Command("receive", Description = "Parse a sample Twilio WhatsApp webhook payload.")]
    public async Task Receive(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: form or json.")] string mode = "form")
        => support.PrintReceiveResult($"twilio whatsapp receive {mode}", await support.ReceiveWhatsAppAsync(file, mode));

    [Command("receive-status", Description = "Parse a sample Twilio WhatsApp status callback.")]
    public async Task ReceiveStatus(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: form or json.")] string mode = "form")
        => support.PrintStatusUpdateResult($"twilio whatsapp receive-status {mode}", await support.ReceiveWhatsAppStatusAsync(file, mode));

    [Command("send", Description = "Build and send a live Twilio WhatsApp message interactively.")]
    public async Task Send()
        => await support.SendWhatsAppAsync();
}

public sealed class TwilioSampleSupport(ILoggerFactory loggerFactory)
{
    private const string SectionName = "Twilio";

    private readonly SampleConfigurationStore configuration = new();
    private readonly ILoggerFactory loggerFactory = loggerFactory;

    private readonly Dictionary<string, IChannelSchema> schemas = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TwilioSms"] = TwilioChannelSchemas.TwilioSms,
        ["SimpleSms"] = TwilioChannelSchemas.SimpleSms,
        ["BulkSms"] = TwilioChannelSchemas.BulkSms,
        ["TwilioWhatsApp"] = TwilioChannelSchemas.TwilioWhatsApp,
        ["SimpleWhatsApp"] = TwilioChannelSchemas.SimpleWhatsApp,
        ["WhatsAppTemplates"] = TwilioChannelSchemas.WhatsAppTemplates,
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
            new SampleConfigurationField("AccountSid", "Twilio account SID", IsRequired: true),
            new SampleConfigurationField("AuthToken", "Twilio auth token", IsSecret: true, IsRequired: true),
            new SampleConfigurationField("SmsFrom", "SMS sender number"),
            new SampleConfigurationField("SmsTo", "SMS recipient number"),
            new SampleConfigurationField("MessagingServiceSid", "Messaging service SID"),
            new SampleConfigurationField("SmsWebhookUrl", "SMS webhook URL"),
            new SampleConfigurationField("SmsStatusCallback", "SMS status callback URL"),
            new SampleConfigurationField("WhatsAppFrom", "WhatsApp sender number"),
            new SampleConfigurationField("WhatsAppTo", "WhatsApp recipient number"),
            new SampleConfigurationField("WhatsAppTemplateId", "WhatsApp template ID"),
            new SampleConfigurationField("WhatsAppWebhookUrl", "WhatsApp webhook URL"),
            new SampleConfigurationField("WhatsAppStatusCallback", "WhatsApp status callback URL"));
    }

    public void PrintSmsValidation()
        => PrintValidationResult(
            "twilio sms validate",
            TwilioChannelSchemas.TwilioSms.ValidateMessage(CreateSmsMessage("+15551234567", "+15557654321")));

    public void PrintWhatsAppValidation(string kind)
    {
        switch (kind.Trim().ToLowerInvariant())
        {
            case "text":
                PrintValidationResult(
                    "twilio whatsapp validate text",
                    TwilioChannelSchemas.SimpleWhatsApp.ValidateMessage(CreateWhatsAppTextMessage("whatsapp:+14155238886", "whatsapp:+15557654321")));
                break;
            case "template":
                PrintValidationResult(
                    "twilio whatsapp validate template",
                    TwilioChannelSchemas.WhatsAppTemplates.ValidateMessage(CreateWhatsAppTemplateMessage("whatsapp:+14155238886", "whatsapp:+15557654321", "HX_SAMPLE_TEMPLATE")));
                break;
            default:
                Console.WriteLine($"Unsupported validation kind '{kind}'. Use text or template.");
                break;
        }
    }

    public async Task<ConnectorResult<StatusInfo>> GetSmsStatusAsync()
    {
        var connector = HasCredentials()
            ? CreateLiveSmsConnector(HasValue("MessagingServiceSid", "TWILIO_MESSAGING_SERVICE_SID"))
            : CreateOfflineSmsConnector();

        await connector.InitializeAsync(CancellationToken.None);
        return await connector.GetStatusAsync(CancellationToken.None);
    }

    public async Task<ConnectorResult<StatusInfo>> GetWhatsAppStatusAsync()
    {
        var connector = HasCredentials()
            ? CreateLiveWhatsAppConnector()
            : CreateOfflineWhatsAppConnector();

        await connector.InitializeAsync(CancellationToken.None);
        return await connector.GetStatusAsync(CancellationToken.None);
    }

    public async Task<ConnectorResult<ReceiveResult>> ReceiveSmsAsync(string? file, string mode)
    {
        var connector = CreateOfflineSmsConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.Successful)
            return ConnectorResult<ReceiveResult>.Fail(initialize.Error!);

        return await connector.ReceiveMessagesAsync(CreateMessageSource(file, mode, DefaultSmsJson, DefaultSmsForm), CancellationToken.None);
    }

    public async Task<ConnectorResult<ReceiveResult>> ReceiveWhatsAppAsync(string? file, string mode)
    {
        var connector = CreateOfflineWhatsAppConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.Successful)
            return ConnectorResult<ReceiveResult>.Fail(initialize.Error!);

        return await connector.ReceiveMessagesAsync(CreateMessageSource(file, mode, DefaultWhatsAppJson, DefaultWhatsAppForm), CancellationToken.None);
    }

    public async Task<ConnectorResult<StatusUpdateResult>> ReceiveSmsStatusAsync(string? file, string mode)
    {
        var connector = CreateOfflineSmsConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.Successful)
            return ConnectorResult<StatusUpdateResult>.Fail(initialize.Error!);

        return await connector.ReceiveMessageStatusAsync(CreateMessageSource(file, mode, DefaultSmsStatusJson, DefaultSmsStatusForm), CancellationToken.None);
    }

    public async Task<ConnectorResult<StatusUpdateResult>> ReceiveWhatsAppStatusAsync(string? file, string mode)
    {
        var connector = CreateOfflineWhatsAppConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.Successful)
            return ConnectorResult<StatusUpdateResult>.Fail(initialize.Error!);

        return await connector.ReceiveMessageStatusAsync(CreateMessageSource(file, mode, DefaultWhatsAppStatusJson, DefaultWhatsAppStatusForm), CancellationToken.None);
    }

    public async Task SendSmsAsync()
    {
        if (!HasCredentials())
        {
            Console.WriteLine("Run 'twilio configure' or set TWILIO_ACCOUNT_SID and TWILIO_AUTH_TOKEN.");
            return;
        }

        var senderMode = SampleConsolePrompts.Select(
            "Select the SMS sender mode",
            ["From number", "Messaging service"],
            HasValue("MessagingServiceSid", "TWILIO_MESSAGING_SERVICE_SID") ? "Messaging service" : "From number");
        var useMessagingService = senderMode == "Messaging service";
        var connector = CreateLiveSmsConnector(useMessagingService);
        PrintResult("twilio sms initialize", await connector.InitializeAsync(CancellationToken.None));

        var sender = useMessagingService
            ? null
            : SampleConsolePrompts.RequiredText("SMS sender number", GetValue("SmsFrom", "TWILIO_SMS_FROM"));
        var recipient = SampleConsolePrompts.RequiredText("SMS recipient number", GetValue("SmsTo", "TWILIO_SMS_TO"));
        var message = CreateSmsMessage(
            SampleConsolePrompts.RequiredText("Message ID", "twilio-sms-sample"),
            sender,
            recipient,
            SampleConsolePrompts.RequiredText("SMS body", "Hello from the Deveel Twilio SMS sample."),
            SampleConsolePrompts.Confirm("Request status callback?", true),
            SampleConsolePrompts.Confirm("Enable smart encoding?", true));
        var result = await connector.SendMessageAsync(message, CancellationToken.None);
        PrintSendResult("twilio sms send", result);
    }

    public async Task SendWhatsAppAsync()
    {
        if (!HasCredentials())
        {
            Console.WriteLine("Run 'twilio configure' or set TWILIO_ACCOUNT_SID and TWILIO_AUTH_TOKEN.");
            return;
        }

        var connector = CreateLiveWhatsAppConnector();
        PrintResult("twilio whatsapp initialize", await connector.InitializeAsync(CancellationToken.None));

        var from = SampleConsolePrompts.RequiredText("WhatsApp sender number", GetValue("WhatsAppFrom", "TWILIO_WHATSAPP_FROM"));
        var to = SampleConsolePrompts.RequiredText("WhatsApp recipient number", GetValue("WhatsAppTo", "TWILIO_WHATSAPP_TO"));
        var kind = SampleConsolePrompts.Select(
            "Select the WhatsApp message type",
            ["Text", "Template"],
            HasValue("WhatsAppTemplateId", "TWILIO_WHATSAPP_TEMPLATE_ID") ? "Template" : "Text");

        var message = kind == "Text"
            ? CreateWhatsAppTextMessage(
                SampleConsolePrompts.RequiredText("Message ID", "twilio-whatsapp-sample"),
                from,
                to,
                SampleConsolePrompts.RequiredText("WhatsApp body", "Hello from the Deveel Twilio WhatsApp sample."),
                SampleConsolePrompts.Confirm("Request status callback?", true))
            : CreateWhatsAppTemplateMessage(
                SampleConsolePrompts.RequiredText("Message ID", "twilio-whatsapp-template-sample"),
                from,
                to,
                SampleConsolePrompts.RequiredText("Template ID", GetValue("WhatsAppTemplateId", "TWILIO_WHATSAPP_TEMPLATE_ID")),
                SampleConsolePrompts.RequiredText("Template parameter 1", "Deveel"),
                SampleConsolePrompts.RequiredText("Template parameter 2", "Twilio"));

        PrintSendResult($"twilio whatsapp send {kind.ToLowerInvariant()}", await connector.SendMessageAsync(message, CancellationToken.None));
    }

    private bool HasCredentials()
        => HasValue("AccountSid", "TWILIO_ACCOUNT_SID") &&
           HasValue("AuthToken", "TWILIO_AUTH_TOKEN");

    private bool HasValue(string key, string environmentName)
        => configuration.HasValue(SectionName, key, environmentName);

    private string GetRequired(string key, string environmentName)
        => configuration.GetRequired(SectionName, key, environmentName);

    private string? GetValue(string key, string environmentName)
        => configuration.GetValue(SectionName, key, environmentName);

    private TwilioSmsConnector CreateOfflineSmsConnector()
    {
        var settings = new ConnectionSettings(TwilioChannelSchemas.TwilioSms)
            .SetParameter("AccountSid", "AC_SAMPLE")
            .SetParameter("AuthToken", "sample-auth-token");

        return new TwilioSmsConnector(
            TwilioChannelSchemas.TwilioSms,
            settings,
            logger: loggerFactory.CreateLogger<TwilioSmsConnector>());
    }

    private TwilioWhatsAppConnector CreateOfflineWhatsAppConnector()
    {
        var settings = new ConnectionSettings(TwilioChannelSchemas.TwilioWhatsApp)
            .SetParameter("AccountSid", "AC_SAMPLE")
            .SetParameter("AuthToken", "sample-auth-token");

        return new TwilioWhatsAppConnector(
            TwilioChannelSchemas.TwilioWhatsApp,
            settings,
            logger: loggerFactory.CreateLogger<TwilioWhatsAppConnector>());
    }

    private TwilioSmsConnector CreateLiveSmsConnector(bool useMessagingService)
    {
        var schema = useMessagingService ? TwilioChannelSchemas.BulkSms : TwilioChannelSchemas.TwilioSms;
        var settings = new ConnectionSettings(schema)
            .SetParameter("AccountSid", GetRequired("AccountSid", "TWILIO_ACCOUNT_SID"))
            .SetParameter("AuthToken", GetRequired("AuthToken", "TWILIO_AUTH_TOKEN"));

        if (useMessagingService)
        {
            var messagingServiceSid = SampleConsolePrompts.RequiredText(
                "Messaging service SID",
                GetValue("MessagingServiceSid", "TWILIO_MESSAGING_SERVICE_SID"));
            AddIfPresent(settings, "MessagingServiceSid", messagingServiceSid);
        }

        AddIfPresent(settings, "WebhookUrl", GetValue("SmsWebhookUrl", "TWILIO_SMS_WEBHOOK_URL"));
        AddIfPresent(settings, "StatusCallback", GetValue("SmsStatusCallback", "TWILIO_SMS_STATUS_CALLBACK"));

        return new TwilioSmsConnector(
            schema,
            settings,
            logger: loggerFactory.CreateLogger<TwilioSmsConnector>());
    }

    private TwilioWhatsAppConnector CreateLiveWhatsAppConnector()
    {
        var settings = new ConnectionSettings(TwilioChannelSchemas.TwilioWhatsApp)
            .SetParameter("AccountSid", GetRequired("AccountSid", "TWILIO_ACCOUNT_SID"))
            .SetParameter("AuthToken", GetRequired("AuthToken", "TWILIO_AUTH_TOKEN"));

        AddIfPresent(settings, "WebhookUrl", GetValue("WhatsAppWebhookUrl", "TWILIO_WHATSAPP_WEBHOOK_URL"));
        AddIfPresent(settings, "StatusCallback", GetValue("WhatsAppStatusCallback", "TWILIO_WHATSAPP_STATUS_CALLBACK"));

        return new TwilioWhatsAppConnector(
            TwilioChannelSchemas.TwilioWhatsApp,
            settings,
            logger: loggerFactory.CreateLogger<TwilioWhatsAppConnector>());
    }

    private static Message CreateSmsMessage(string? sender, string recipient)
        => CreateSmsMessage("twilio-sms-sample", sender, recipient, "Hello from the Deveel Twilio SMS sample.", true, true);

    private static Message CreateSmsMessage(
        string id,
        string? sender,
        string recipient,
        string body,
        bool provideCallback,
        bool smartEncoded)
    {
        return new Message
        {
            Id = id,
            Sender = String.IsNullOrWhiteSpace(sender) ? null : Endpoint.PhoneNumber(sender),
            Receiver = Endpoint.PhoneNumber(recipient),
            Content = new TextContent(body),
            Properties = new Dictionary<string, MessageProperty>
            {
                ["ProvideCallback"] = new("ProvideCallback", provideCallback),
                ["SmartEncoded"] = new("SmartEncoded", smartEncoded)
            }
        };
    }

    private static Message CreateWhatsAppTextMessage(string sender, string recipient)
        => CreateWhatsAppTextMessage("twilio-whatsapp-sample", sender, recipient, "Hello from the Deveel Twilio WhatsApp sample.", true);

    private static Message CreateWhatsAppTextMessage(
        string id,
        string sender,
        string recipient,
        string body,
        bool provideCallback)
    {
        return new Message
        {
            Id = id,
            Sender = Endpoint.PhoneNumber(sender),
            Receiver = Endpoint.PhoneNumber(recipient),
            Content = new TextContent(body),
            Properties = new Dictionary<string, MessageProperty>
            {
                ["ProvideCallback"] = new("ProvideCallback", provideCallback)
            }
        };
    }

    private static Message CreateWhatsAppTemplateMessage(string sender, string recipient, string templateId)
        => CreateWhatsAppTemplateMessage("twilio-whatsapp-template-sample", sender, recipient, templateId, "Deveel", "Twilio");

    private static Message CreateWhatsAppTemplateMessage(
        string id,
        string sender,
        string recipient,
        string templateId,
        string parameter1,
        string parameter2)
    {
        return new Message
        {
            Id = id,
            Sender = Endpoint.PhoneNumber(sender),
            Receiver = Endpoint.PhoneNumber(recipient),
            Content = new TemplateContent(templateId, new Dictionary<string, object?>
            {
                ["1"] = parameter1,
                ["2"] = parameter2
            })
        };
    }

    private static MessageSource CreateMessageSource(string? file, string mode, string jsonPayload, string formPayload)
    {
        var payload = ReadFileOrDefault(file, mode.Equals("json", StringComparison.OrdinalIgnoreCase) ? jsonPayload : formPayload);
        return mode.Equals("json", StringComparison.OrdinalIgnoreCase)
            ? MessageSource.Json(payload)
            : MessageSource.UrlPost(payload);
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
            Console.WriteLine($"  - id={message.Id}, from={message.Sender?.Address}, to={message.Receiver?.Address}");
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

    private const string DefaultSmsForm = "MessageSid=SM0001&From=%2B15551234567&To=%2B15557654321&Body=Hello+from+an+SMS+webhook";
    private const string DefaultSmsJson = """
        {
          "MessageSid": "SM0001",
          "From": "+15551234567",
          "To": "+15557654321",
          "Body": "Hello from an SMS webhook"
        }
        """;

    private const string DefaultSmsStatusForm = "MessageSid=SM0001&MessageStatus=delivered&From=%2B15551234567&To=%2B15557654321&AccountSid=AC_SAMPLE";
    private const string DefaultSmsStatusJson = """
        {
          "MessageSid": "SM0001",
          "MessageStatus": "delivered",
          "From": "+15551234567",
          "To": "+15557654321",
          "AccountSid": "AC_SAMPLE"
        }
        """;

    private const string DefaultWhatsAppForm = "MessageSid=SM9001&From=whatsapp%3A%2B14155238886&To=whatsapp%3A%2B15557654321&Body=Hello+from+WhatsApp";
    private const string DefaultWhatsAppJson = """
        {
          "MessageSid": "SM9001",
          "From": "whatsapp:+14155238886",
          "To": "whatsapp:+15557654321",
          "Body": "Hello from WhatsApp"
        }
        """;

    private const string DefaultWhatsAppStatusForm = "MessageSid=SM9001&MessageStatus=read&From=whatsapp%3A%2B14155238886&To=whatsapp%3A%2B15557654321&ProfileName=Deveel";
    private const string DefaultWhatsAppStatusJson = """
        {
          "MessageSid": "SM9001",
          "MessageStatus": "read",
          "From": "whatsapp:+14155238886",
          "To": "whatsapp:+15557654321",
          "ProfileName": "Deveel"
        }
        """;
}
