# Twilio SMS Connector

Send and receive SMS (and optional MMS with media attachments) via Twilio's Programmable SMS API.

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

| Parameter | Type | Default | Description |
|---|---|---|---|
| `MessagingServiceSid` | `string` | — | Twilio Messaging Service SID for advanced routing |
| `WebhookUrl` | `string` | — | URL for inbound SMS and status callbacks |
| `StatusCallback` | `string` | — | URL for delivery status callbacks |
| `ValidityPeriod` | `int` | — | Message validity in seconds (14400 = 4 hours default) |
| `MaxPrice` | `decimal` | — | Maximum price per message |
| `SmartEncoded` | `bool` | — | Optimize for GSM-7 charset |

## Schema

| Property | Value |
|---|---|
| Provider | `Twilio` |
| Type | `SMS` |
| Version | `1.0.0` |
| Capabilities | `SendMessages`, `MessageStatusQuery` |
| Content types | `PlainText`, `Media` |
| Endpoints | `PhoneNumber` (send + receive) |
| Authentication | Basic (`AccountSid` + `AuthToken`) |
| Parameters | 2 required, 5 optional |

## Send examples

### Text SMS

```csharp
var settings = new ConnectionSettings()
    .SetParameter("AccountSid", "AC...")
    .SetParameter("AuthToken", "...");

var connector = new TwilioSmsConnector(TwilioChannelSchemas.SimpleSms, settings);
await connector.InitializeAsync(ct);

var message = new Message()
    .WithId("sms-1")
    .WithPhoneSender("+15550001111")
    .WithPhoneReceiver("+15550002222")
    .WithTextContent("Hello from Twilio SMS");

var result = await connector.SendMessageAsync(message, ct);

if (result.IsSuccess)
{
    Console.WriteLine($"Sent! Twilio SID: {result.Data?.RemoteMessageId}");
    Console.WriteLine($"Status: {result.Data?.Status}");
}
```

### SMS with media (MMS)

```csharp
var message = new Message()
    .WithId("mms-1")
    .WithPhoneSender("+15550001111")
    .WithPhoneReceiver("+15550002222")
    .WithContent(new MediaContent(MediaType.Image, "photo.jpg",
        "https://example.com/photo.jpg"));
```

### SMS with custom validity

```csharp
new Message()
    .WithPhoneSender("+15550001111")
    .WithPhoneReceiver("+15550002222")
    .WithTextContent("Time-sensitive message")
    .With("ValidityPeriod", 300)     // 5 minutes
    .With("MaxPrice", 0.01);         // $0.01 max
```

## Message properties

| Property | Type | Description |
|---|---|---|
| `ValidityPeriod` | `int` | Time in seconds the message is valid |
| `MaxPrice` | `decimal` | Maximum price to pay for the message |
| `ProvideCallback` | `bool` | Request a delivery callback |
| `SmartEncoded` | `bool` | Optimize for GSM-7 charset |

## Webhook handling

Twilio sends inbound messages and status callbacks as form POST requests. The `MessageSource.UrlPost()` factory handles parsing:

```csharp
[HttpPost("/webhooks/twilio/sms")]
public async Task<IActionResult> TwilioWebhook(CancellationToken ct)
{
    // Read the raw form body into a MessageSource
    using var reader = new StreamReader(Request.Body);
    var body = await reader.ReadToEndAsync();
    var source = MessageSource.UrlPost(body);

    // Process the status update
    var statusResult = await _connector.ReceiveMessageStatusAsync(source, ct);

    // Or process an inbound message
    var messageResult = await _connector.ReceiveMessagesAsync(source, ct);

    return TwiML.Empty;  // Return TwiML response
}
```

**Important**: validate `X-Twilio-Signature` before processing. See [Twilio security docs](https://www.twilio.com/docs/security).

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `INVALID_CREDENTIALS` | Wrong AccountSid/AuthToken | Check credentials in Twilio Console |
| `INVALID_RECIPIENT` | Phone number not in E.164 | Use `+` prefix and country code |
| `RATE_LIMITED` | Too many requests per second | Add delay between sends |
| `MESSAGE_TOO_LARGE` | SMS > 1600 characters | Split into multiple messages |
| Delivery not received | Recipient phone number issues | Check Twilio Console logs |
| Status callback not received | Webhook URL not reachable | Check callback URL and network |

## TwilioChannelSchemas

The connector ships with a predefined schema:

```csharp
// Simple SMS schema (send only)
TwilioChannelSchemas.SimpleSms
```
