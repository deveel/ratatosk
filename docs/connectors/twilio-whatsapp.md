# Twilio WhatsApp Connector

WhatsApp Business messaging through Twilio's API for WhatsApp.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Twilio
```

## Required settings

| Parameter | Type | Description |
|---|---|---|
| `AccountSid` | `string` | Twilio account SID (starts with `AC`) |
| `AuthToken` | `string` | Twilio auth token (sensitive) |

### Optional settings

| Parameter | Type | Description |
|---|---|---|
| `WebhookUrl` | `string` | URL for inbound WhatsApp messages |
| `StatusCallback` | `string` | URL for delivery status callbacks |

## Number format

Phone numbers must use the `whatsapp:` prefix with E.164 format:

```csharp
// Sender — must be a Twilio WhatsApp-enabled number
.WithPhoneSender("whatsapp:+14155238886")   // Twilio's default WhatsApp number

// Recipient — must have opted in to receive messages from your business
.WithPhoneReceiver("whatsapp:+15550002222")
```

## Schema

| Property | Value |
|---|---|
| Provider | `Twilio` |
| Type | `WhatsApp` |
| Version | `1.0.0` |
| Capabilities | `SendMessages`, `MessageStatusQuery`, `Templates` |
| Content types | `PlainText`, `Media`, `Template` |
| Endpoints | `PhoneNumber` (send + receive) |
| Authentication | Basic (`AccountSid` + `AuthToken`) |

## Send examples

### Text message

```csharp
var settings = new ConnectionSettings()
    .SetParameter("AccountSid", "AC...")
    .SetParameter("AuthToken", "...");

var connector = new TwilioWhatsAppConnector(TwilioChannelSchemas.TwilioWhatsApp, settings);
await connector.InitializeAsync(ct);

var message = new Message()
    .WithId("wa-1")
    .WithPhoneSender("whatsapp:+14155238886")
    .WithPhoneReceiver("whatsapp:+15550002222")
    .WithTextContent("Hello from WhatsApp!");

var result = await connector.SendMessageAsync(message, ct);
```

### Template message (for business-initiated conversations)

WhatsApp requires approved templates for the first message to a user:

```csharp
var message = new Message()
    .WithId("wa-template-1")
    .WithPhoneReceiver("whatsapp:+15550002222")
    .WithContent(new TemplateContent("order_confirmed", new Dictionary<string, object?>
    {
        ["order_id"] = "ORD-123",
        ["delivery_date"] = "2026-05-15"
    }));
```

### Media message with caption

```csharp
new Message()
    .WithPhoneSender("whatsapp:+14155238886")
    .WithPhoneReceiver("whatsapp:+15550002222")
    .WithContent(new MediaContent(MediaType.Image, "product.jpg",
        "https://example.com/product.jpg"))
    .With("caption", "Check out our new product!");
```

## Message properties

| Property | Type | Description |
|---|---|---|
| `caption` | `string` | Caption for media messages |
| `ValidityPeriod` | `int` | Message validity in seconds |

## Webhook handling

Similar to Twilio SMS — inbound messages arrive as form POST with `MessageSource.UrlPost()`:

```csharp
[HttpPost("/webhooks/twilio/whatsapp")]
public async Task<IActionResult> WhatsAppWebhook(CancellationToken ct)
{
    using var reader = new StreamReader(Request.Body);
    var body = await reader.ReadAsStringAsync(ct);
    var source = MessageSource.UrlPost(body);

    var result = await _connector.ReceiveMessagesAsync(source, ct);
    return TwiML.Empty;
}
```

Validate `X-Twilio-Signature` header before processing.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `INVALID_RECIPIENT` | Number without `whatsapp:` prefix | Use `whatsapp:+E164` format |
| Template rejected | Template not approved | Verify template status in Twilio Console |
| `RATE_LIMITED` | Too many messages | WhatsApp has per-conversation limits |
| No delivery | User hasn't opted in | User must send first message or accept invite |
| Media not sent | Unsupported format | Check WhatsApp media requirements (max 64MB, specific formats) |

## TwilioChannelSchemas

```csharp
// WhatsApp messaging schema
TwilioChannelSchemas.TwilioWhatsApp
```
