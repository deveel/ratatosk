# Firebase Push Connector

Use this connector for Firebase Cloud Messaging sends to device tokens and topics.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Firebase
```

## Required settings

- `ProjectId`
- `ServiceAccountKey` (JSON content)

Optional: `DryRun` for safe pre-production checks.

## Minimal send example

```csharp
var settings = new ConnectionSettings()
    .AddParameter("ProjectId", "my-project")
    .AddParameter("ServiceAccountKey", serviceAccountJson);

var connector = new FirebasePushConnector(FirebaseChannelSchemas.FirebasePush, settings);
await connector.InitializeAsync(ct);

var message = new MessageBuilder()
    .WithId("push-001")
    .WithDeviceReceiver("fcm-device-token")
    .WithTextContent("You have a new notification")
    .WithProperty("Title", "New message")
    .Message;

var result = await connector.SendMessageAsync(message, ct);
```

## Notes

- Supports device and topic targeting
- Supports batch send patterns for campaigns
- Inbound/status behavior differs from SMS/email channels; treat this connector as primarily outbound

## Quick troubleshooting

- Auth issues: verify service account key/project pairing
- Token failures (`invalid`, `unregistered`): clean stale tokens from your store
- Payload too big: reduce custom data size

## Related docs

- [Advanced configuration](../advanced-configuration.md)
- [Channel schema usage](../channelschema-usage.md)
