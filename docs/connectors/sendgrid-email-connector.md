# SendGrid Email Connector

Use this connector for transactional and bulk email through SendGrid.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Sendgrid
```

## Required settings

- `ApiKey`

Common optional settings: `SandboxMode`, `WebhookUrl`, `TrackingSettings`.

## Minimal send example

```csharp
var settings = new ConnectionSettings()
    .AddParameter("ApiKey", "SG...");

var connector = new SendGridEmailConnector(SendGridChannelSchemas.SendGridEmail, settings);
await connector.InitializeAsync(ct);

var email = new MessageBuilder()
    .WithId("email-001")
    .WithEmailSender("noreply@yourdomain.com")
    .WithEmailReceiver("user@example.com")
    .WithTextContent("Welcome to the app")
    .WithProperty("Subject", "Welcome")
    .Message;

var result = await connector.SendMessageAsync(email, ct);
```

## Useful features

- Template content for dynamic emails
- Multipart content with attachments
- Batch sends for campaigns
- Event webhook processing (delivered/opened/clicked/bounced)

## Quick troubleshooting

- Auth errors: verify API key and permissions
- Delivery issues: check sender/domain verification
- Event gaps: verify webhook endpoint and event subscription

## Related docs

- [Advanced configuration](../advanced-configuration.md)
- [Channel schema usage](../channelschema-usage.md)
