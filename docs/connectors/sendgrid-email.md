# SendGrid Email Connector

Transactional and bulk email via the SendGrid v3 API.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Sendgrid
```

## Required settings

| Parameter | Type | Description |
|---|---|---|
| `ApiKey` | `string` | SendGrid API key (starts with `SG.`) |

### Optional settings

| Parameter | Type | Default | Description |
|---|---|---|---|
| `SandboxMode` | `bool` | `false` | When `true`, no email is actually sent (for testing) |
| `WebhookUrl` | `string` | — | URL for event webhooks |
| `TrackingSettings` | `string` | — | JSON with click/open tracking settings |

## Schema

| Property | Value |
|---|---|
| Provider | `SendGrid` |
| Type | `Email` |
| Version | `1.1.0` |
| Capabilities | `SendMessages`, `BulkMessaging`, `Templates`, `HealthCheck` |
| Content types | `PlainText`, `Html`, `Multipart`, `Template` |
| Endpoints | `EmailAddress` (send + receive) |
| Authentication | API Key |

## Send examples

### Plain text email

```csharp
var settings = new ConnectionSettings()
    .SetParameter("ApiKey", "SG...");

var connector = new SendGridEmailConnector(SendGridChannelSchemas.SendGridEmail, settings);
await connector.InitializeAsync(ct);

var email = new Message()
    .WithId("email-1")
    .WithEmailSender("noreply@yourdomain.com")
    .WithEmailReceiver("user@example.com")
    .WithTextContent("Welcome to the app!")
    .With("Subject", "Welcome!");

var result = await connector.SendMessageAsync(email, ct);
```

### HTML email

```csharp
new Message()
    .WithEmailSender("noreply@yourdomain.com")
    .WithEmailReceiver("user@example.com")
    .WithHtmlContent(
        "<h1>Welcome!</h1><p>Thanks for signing up, <b>{{name}}</b>.</p>")
    .With("Subject", "Welcome!");
```

### Multipart email (text + HTML)

```csharp
var multipart = new MultipartContent();
multipart.Parts.Add(new TextContent("Welcome to the app! Please verify your email."));
multipart.Parts.Add(new HtmlContent(
    "<h1>Welcome!</h1><p>Please <a href='{{link}}'>verify your email</a>.</p>"));

new Message()
    .WithEmailSender("noreply@yourdomain.com")
    .WithEmailReceiver("user@example.com")
    .WithContent(multipart)
    .With("Subject", "Verify your email");
```

### Email with attachment

```csharp
new Message()
    .WithEmailSender("noreply@yourdomain.com")
    .WithEmailReceiver("user@example.com")
    .WithHtmlContent("<p>Please find the attached report.</p>", html =>
    {
        html.Attachments.Add(new MessageAttachment(
            "report", "report.pdf", "application/pdf", base64PdfContent));
    })
    .With("Subject", "Monthly Report");
```

### Template email (SendGrid dynamic templates)

```csharp
new Message()
    .WithEmailSender("noreply@yourdomain.com")
    .WithEmailReceiver("user@example.com")
    .WithContent(new TemplateContent("d-abc123def456", new Dictionary<string, object?>
    {
        ["name"] = "Alice",
        ["verification_link"] = "https://example.com/verify?token=xyz"
    }))
    .With("Subject", "Welcome!");  // Subject can be overridden from template defaults
```

### Batch send

```csharp
var batch = new MessageBatch();
batch.Messages.Add(email1);
batch.Messages.Add(email2);
batch.Messages.Add(email3);

var result = await connector.SendBatchAsync(batch, ct);
Console.WriteLine($"Batch sent: {result.Data?.BatchId}");

if (result.Data != null)
{
    foreach (var (msgId, sendResult) in result.Data.MessageResults)
        Console.WriteLine($"  {msgId}: {sendResult.Status} ({sendResult.RemoteMessageId})");
}
```

### Sandbox mode (testing)

```csharp
new ConnectionSettings()
    .SetParameter("ApiKey", "SG...")
    .SetParameter("SandboxMode", true);
```

## Message properties

| Property | Type | Description |
|---|---|---|
| `Subject` | `string` | Email subject line (required) |
| `TrackingSettings` | `string` | JSON string for click/open tracking configuration |
| `ReplyTo` | `string` | Reply-to email address |

## Webhook handling

SendGrid posts event data (delivered, opened, clicked, bounced, spam reports) as JSON POST to your webhook URL:

```csharp
[HttpPost("/webhooks/sendgrid")]
public async Task<IActionResult> SendGridWebhook(CancellationToken ct)
{
    using var reader = new StreamReader(Request.Body);
    var body = await reader.ReadToEndAsync(ct);
    var source = MessageSource.Json(body);

    var result = await _connector.ReceiveMessageStatusAsync(source, ct);
    // Process status updates...

    return Ok();
}
```

Validate `X-Twilio-Email-Event-Webhook-Signature` header when using signed webhooks.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `INVALID_CREDENTIALS` | API key is invalid | Generate a new API key in SendGrid UI |
| `PROVIDER_VALIDATION_FAILED` | Invalid sender email | Verify sender domain (SPF/DKIM setup) |
| Delivery not happening | Sender not verified | Verify sender email or domain in SendGrid |
| Emails going to spam | Poor sender reputation | Check SendGrid reputation reports |
| Template not rendering | Template ID wrong | Verify template ID in SendGrid UI |
| Event webhooks not arriving | Webhook URL not configured | Subscribe to events in SendGrid Mail Settings |

## SendGridChannelSchemas

```csharp
// Full email schema (send, bulk, templates)
SendGridChannelSchemas.SendGridEmail
```
