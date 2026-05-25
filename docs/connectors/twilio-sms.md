# Twilio SMS Connector

Send and receive SMS (and optional MMS with media attachments) via Twilio's Programmable SMS API.

## Package

```bash
dotnet add package Ratatosk.Twilio
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

var message = new MessageBuilder()
    .WithId("sms-1")
    .FromPhone("+15550001111")
    .ToPhone("+15550002222")
    .WithText("Hello from Twilio SMS")
    .Build();

var result = await connector.SendMessageAsync(message, ct);

if (result.IsSuccess)
{
    Console.WriteLine($"Sent! Twilio SID: {result.Data?.RemoteMessageId}");
    Console.WriteLine($"Status: {result.Data?.Status}");
}
```

### SMS with media (MMS)

```csharp
var message = new MessageBuilder()
    .WithId("mms-1")
    .FromPhone("+15550001111")
    .ToPhone("+15550002222")
    .WithContent(new MediaContent(MediaType.Image, "photo.jpg",
        "https://example.com/photo.jpg"))
    .Build();
```

### SMS with custom validity

```csharp
new MessageBuilder()
    .FromPhone("+15550001111")
    .ToPhone("+15550002222")
    .WithText("Time-sensitive message")
    .WithValidityPeriod(300)     // 5 minutes
    .WithMaxPrice(0.01)          // $0.01 max
    .Build();
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

## Error codes

Twilio-specific error codes are defined in `TwilioErrorCodes` with domain `"Twilio"`.

| Code | Description |
|---|---|
| `INVALID_CONNECTION_SETTINGS` | Connection settings validation failed |
| `MISSING_FROM_NUMBER` | Sender phone number is required when MessagingServiceSid is not configured |
| `INVALID_SENDER` | Sender phone number is not in valid format |
| `INVALID_MESSAGE` | Message properties failed schema validation |
| `MISSING_CONTENT_SID` | WhatsApp template Content SID is missing |
| `INVALID_WHATSAPP_NUMBER` | WhatsApp number does not follow `whatsapp:+E164` format |
| `SEND_WHATSAPP_MESSAGE_FAILED` | WhatsApp message send failed via Twilio API |
| `WHATSAPP_STATUS_QUERY_FAILED` | WhatsApp message status query failed |
| `STATUS_QUERY_FAILED` | Message status query failed |
| `STATUS_ERROR` | Connector status retrieval failed |
| `RECEIVE_MESSAGE_FAILED` | Incoming message webhook processing failed |
| `RECEIVE_STATUS_FAILED` | Status callback webhook processing failed |

Standard `MessagingErrorCodes` are also used — see the [error codes reference](../result-types.md#error-code-tables).

### Original provider codes

Twilio API errors (`ApiException`) are mapped to framework error codes in `TwilioService.MapTwilioErrorCode()`:

| Twilio code | Mapped framework code |
|---|---|
| `21211` | `INVALID_RECIPIENT` |
| `21610` | `INVALID_RECIPIENT` |
| `21614` | `INVALID_SENDER` |
| `21408` | `INVALID_SENDER` |
| `20001` | `INVALID_MESSAGE` |
| Other | `SEND_MESSAGE_FAILED` |

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
