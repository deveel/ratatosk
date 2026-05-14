using System.ComponentModel.DataAnnotations;
using Deveel;
using Deveel.Messaging;
using Microsoft.Extensions.Logging;

namespace TwilioSample;

public sealed class TwilioSampleSupport(ILoggerFactory loggerFactory, IMessagingClient client)
{
    private const string SectionName = "Twilio";

    private readonly SampleConfigurationStore configuration = new();
    private readonly ILoggerFactory loggerFactory = loggerFactory;
    private readonly IMessagingClient client = client;

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
        => SampleOutputHelper.PrintValidationResult(
            "twilio sms validate",
            TwilioChannelSchemas.TwilioSms.ValidateMessage(CreateSmsMessage("+15551234567", "+15557654321")));

    public void PrintWhatsAppValidation(string kind)
    {
        switch (kind.Trim().ToLowerInvariant())
        {
            case "text":
                SampleOutputHelper.PrintValidationResult(
                    "twilio whatsapp validate text",
                    TwilioChannelSchemas.SimpleWhatsApp.ValidateMessage(CreateWhatsAppTextMessage("whatsapp:+14155238886", "whatsapp:+15557654321")));
                break;
            case "template":
                SampleOutputHelper.PrintValidationResult(
                    "twilio whatsapp validate template",
                    TwilioChannelSchemas.WhatsAppTemplates.ValidateMessage(CreateWhatsAppTemplateMessage("whatsapp:+14155238886", "whatsapp:+15557654321", "HX_SAMPLE_TEMPLATE")));
                break;
            default:
                Console.WriteLine($"Unsupported validation kind '{kind}'. Use text or template.");
                break;
        }
    }

    public async Task<OperationResult<StatusInfo>> GetSmsStatusAsync()
    {
        return await client.GetStatusAsync("sms", CancellationToken.None);
    }

    public async Task<OperationResult<StatusInfo>> GetWhatsAppStatusAsync()
    {
        return await client.GetStatusAsync("whatsapp", CancellationToken.None);
    }

    public async Task<OperationResult<ReceiveResult>> ReceiveSmsAsync(string? file, string mode)
    {
        var connector = CreateOfflineSmsConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.IsSuccess())
            return OperationResult<ReceiveResult>.Fail(initialize.Error!.Code, initialize.Error!.Domain, initialize.Error!.Message);

        return await connector.ReceiveMessagesAsync(CreateMessageSource(file, mode, DefaultSmsJson, DefaultSmsForm), CancellationToken.None);
    }

    public async Task<OperationResult<ReceiveResult>> ReceiveWhatsAppAsync(string? file, string mode)
    {
        var connector = CreateOfflineWhatsAppConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.IsSuccess())
            return OperationResult<ReceiveResult>.Fail(initialize.Error!.Code, initialize.Error!.Domain, initialize.Error!.Message);

        return await connector.ReceiveMessagesAsync(CreateMessageSource(file, mode, DefaultWhatsAppJson, DefaultWhatsAppForm), CancellationToken.None);
    }

    public async Task<OperationResult<StatusUpdateResult>> ReceiveSmsStatusAsync(string? file, string mode)
    {
        var connector = CreateOfflineSmsConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.IsSuccess())
            return OperationResult<StatusUpdateResult>.Fail(initialize.Error!.Code, initialize.Error!.Domain, initialize.Error!.Message);

        return await connector.ReceiveMessageStatusAsync(CreateMessageSource(file, mode, DefaultSmsStatusJson, DefaultSmsStatusForm), CancellationToken.None);
    }

    public async Task<OperationResult<StatusUpdateResult>> ReceiveWhatsAppStatusAsync(string? file, string mode)
    {
        var connector = CreateOfflineWhatsAppConnector();
        var initialize = await connector.InitializeAsync(CancellationToken.None);
        if (!initialize.IsSuccess())
            return OperationResult<StatusUpdateResult>.Fail(initialize.Error!.Code, initialize.Error!.Domain, initialize.Error!.Message);

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
        var sender = useMessagingService
            ? null
            : SampleConsolePrompts.RequiredText("SMS sender number", GetValue("SmsFrom", "TWILIO_SMS_FROM"));
        var recipient = SampleConsolePrompts.OptionalText("SMS recipient number", GetValue("SmsTo", "TWILIO_SMS_TO"));

        if (String.IsNullOrWhiteSpace(recipient))
        {
            Console.WriteLine("No SMS recipient number provided. Aborting send.");
            return;
        }

        var message = CreateSmsMessage(
            SampleConsolePrompts.RequiredText("Message ID", "twilio-sms-sample"),
            sender,
            recipient,
            SampleConsolePrompts.MultiLineBody("SMS body", "Hello from the Deveel Twilio SMS sample."),
            SampleConsolePrompts.Confirm("Request status callback?", true),
            SampleConsolePrompts.Confirm("Enable smart encoding?", true));
        var result = await client.SendAsync("sms", message, CancellationToken.None);
        SampleOutputHelper.PrintSendResult("twilio sms send", result);
    }

    public async Task SendWhatsAppAsync()
    {
        if (!HasCredentials())
        {
            Console.WriteLine("Run 'twilio configure' or set TWILIO_ACCOUNT_SID and TWILIO_AUTH_TOKEN.");
            return;
        }

        var from = SampleConsolePrompts.RequiredText("WhatsApp sender number", GetValue("WhatsAppFrom", "TWILIO_WHATSAPP_FROM"));
        var to = SampleConsolePrompts.OptionalText("WhatsApp recipient number", GetValue("WhatsAppTo", "TWILIO_WHATSAPP_TO"));

        if (String.IsNullOrWhiteSpace(to))
        {
            Console.WriteLine("No WhatsApp recipient number provided. Aborting send.");
            return;
        }

        var kind = SampleConsolePrompts.Select(
            "Select the WhatsApp message type",
            ["Text", "Template"],
            HasValue("WhatsAppTemplateId", "TWILIO_WHATSAPP_TEMPLATE_ID") ? "Template" : "Text");

        var message = kind == "Text"
            ? CreateWhatsAppTextMessage(
                SampleConsolePrompts.RequiredText("Message ID", "twilio-whatsapp-sample"),
                from,
                to,
                SampleConsolePrompts.MultiLineBody("WhatsApp body", "Hello from the Deveel Twilio WhatsApp sample."),
                SampleConsolePrompts.Confirm("Request status callback?", true))
            : CreateWhatsAppTemplateMessage(
                SampleConsolePrompts.RequiredText("Message ID", "twilio-whatsapp-template-sample"),
                from,
                to,
                SampleConsolePrompts.RequiredText("Template ID", GetValue("WhatsAppTemplateId", "TWILIO_WHATSAPP_TEMPLATE_ID")),
                SampleConsolePrompts.RequiredText("Template parameter 1", "Deveel"),
                SampleConsolePrompts.RequiredText("Template parameter 2", "Twilio"));

        SampleOutputHelper.PrintSendResult($"twilio whatsapp send {kind.ToLowerInvariant()}", await client.SendAsync("whatsapp", message, CancellationToken.None));
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
            SampleOutputHelper.AddIfPresent(settings, "MessagingServiceSid", messagingServiceSid);
        }

        SampleOutputHelper.AddIfPresent(settings, "WebhookUrl", GetValue("SmsWebhookUrl", "TWILIO_SMS_WEBHOOK_URL"));
        SampleOutputHelper.AddIfPresent(settings, "StatusCallback", GetValue("SmsStatusCallback", "TWILIO_SMS_STATUS_CALLBACK"));

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

        SampleOutputHelper.AddIfPresent(settings, "WebhookUrl", GetValue("WhatsAppWebhookUrl", "TWILIO_WHATSAPP_WEBHOOK_URL"));
        SampleOutputHelper.AddIfPresent(settings, "StatusCallback", GetValue("WhatsAppStatusCallback", "TWILIO_WHATSAPP_STATUS_CALLBACK"));

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
        var payload = SampleOutputHelper.ReadFileOrDefault(file, mode.Equals("json", StringComparison.OrdinalIgnoreCase) ? jsonPayload : formPayload);
        return mode.Equals("json", StringComparison.OrdinalIgnoreCase)
            ? MessageSource.Json(payload)
            : MessageSource.UrlPost(payload);
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
            Console.WriteLine($"  - id={message.Id}, from={message.Sender?.Address}, to={message.Receiver?.Address}");
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
