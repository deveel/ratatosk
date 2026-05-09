# Twilio WhatsApp Connector

Use this connector for WhatsApp Business messaging through Twilio.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Twilio
```

## Required settings

- `AccountSid`
- `AuthToken`

Optional: `WebhookUrl`, `StatusCallback`.

## Minimal send example

```csharp
var settings = new ConnectionSettings()
    .AddParameter("AccountSid", "AC...")
    .AddParameter("AuthToken", "...");

var connector = new TwilioWhatsAppConnector(TwilioChannelSchemas.TwilioWhatsApp, settings);
await connector.InitializeAsync(ct);

var message = new MessageBuilder()
    .WithId("wa-001")
    .WithPhoneSender("whatsapp:+15550001111")
    .WithPhoneReceiver("whatsapp:+15550002222")
    .WithTextContent("Hello from WhatsApp")
    .Message;

var result = await connector.SendMessageAsync(message, ct);
```

## Useful scenarios

- Template messages for business notifications
- Media messages with captions
- Webhook handling for inbound text and interactive replies

## Practical reminders

- Keep number format as `whatsapp:+E164`
- Use approved templates where WhatsApp policy requires it
- Validate webhook authenticity before processing

## Related docs

- [Twilio SMS connector](twilio-sms-connector.md)
- [Advanced configuration](../advanced-configuration.md)
