# Twilio SMS Connector

Use this connector when you want SMS (and optional MMS) with Twilio while keeping the framework API.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Twilio
```

## Required settings

- `AccountSid`
- `AuthToken`

Common optional settings: `WebhookUrl`, `StatusCallback`, `MessagingServiceSid`.

## Minimal send example

```csharp
var settings = new ConnectionSettings()
    .AddParameter("AccountSid", "AC...")
    .AddParameter("AuthToken", "...");

var connector = new TwilioSmsConnector(TwilioChannelSchemas.SimpleSms, settings);
await connector.InitializeAsync(ct);

var message = new MessageBuilder()
    .WithId("sms-001")
    .WithPhoneSender("+15550001111")
    .WithPhoneReceiver("+15550002222")
    .WithTextContent("Hello from Twilio")
    .Message;

var result = await connector.SendMessageAsync(message, ct);
```

## Useful message properties

- `ValidityPeriod`
- `MaxPrice`
- `ProvideCallback`
- `SmartEncoded`

## Webhook notes

Inbound and status callbacks are form posts from Twilio. Validate request signatures before processing.

## Quick troubleshooting

- Invalid recipient: use E.164 format (`+15551234567`)
- Auth failures: recheck `AccountSid` and `AuthToken`
- Delivery/state issues: verify callback URL and Twilio logs

## Related docs

- [Twilio WhatsApp connector](twilio-whatsapp-connector.md)
- [Advanced configuration](../advanced-configuration.md)
