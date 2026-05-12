# Firebase Push Connector

Push notifications via Firebase Cloud Messaging (FCM) HTTP v1 API.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Firebase
```

## Required settings

| Parameter | Type | Description |
|---|---|---|
| `ProjectId` | `string` | Firebase project ID (from Firebase Console) |
| `ServiceAccountKey` | `string` | Full JSON content of the Firebase service account private key |

### Optional settings

| Parameter | Type | Default | Description |
|---|---|---|---|
| `DryRun` | `bool` | `false` | When `true`, validates the message without sending |

The `ServiceAccountKey` must be the complete JSON content of a Firebase service account key file, not a file path.

## Schema

| Property | Value |
|---|---|
| Provider | `Firebase` |
| Type | `Push` |
| Version | `1.0.0` |
| Capabilities | `SendMessages`, `BulkMessaging` |
| Content types | `PlainText`, `Json` |
| Endpoints | `Device` (FCM token), `Id` (topic) |
| Authentication | Firebase service account JWT |

## Send examples

### Notification to a device token

```csharp
var settings = new ConnectionSettings()
    .SetParameter("ProjectId", "my-project")
    .SetParameter("ServiceAccountKey", serviceAccountJson);

var connector = new FirebasePushConnector(FirebaseChannelSchemas.FirebasePush, settings);
await connector.InitializeAsync(ct);

var message = new Message()
    .WithId("push-1")
    .WithReceiver(Endpoint.Device("fcm-device-token-here"))
    .WithTextContent("You have a new notification")
    .With("Title", "New message");

var result = await connector.SendMessageAsync(message, ct);
```

### Notification to a topic

```csharp
new Message()
    .WithId("push-topic-1")
    .WithReceiver(Endpoint.Id("/topics/news"))
    .WithTextContent("Breaking news: ...")
    .With("Title", "News alert");
```

### Push with custom data payload

```csharp
new Message()
    .WithReceiver(Endpoint.Device("fcm-token"))
    .WithContent(new JsonContent("{\"order_id\": \"ORD-123\", \"status\": \"shipped\"}"))
    .With("Title", "Order shipped")
    .With("sound", "default")
    .With("badge", 1);
```

### Batch push to multiple devices

```csharp
var batch = new MessageBatch();
foreach (var token in deviceTokens)
{
    batch.Messages.Add(new Message()
        .WithId(Guid.NewGuid().ToString("n"))
        .WithReceiver(Endpoint.Device(token))
        .WithTextContent("You have a new message")
        .With("Title", "New message"));
}

var result = await connector.SendBatchAsync(batch, ct);
```

### Dry run (validation without sending)

```csharp
settings.SetParameter("DryRun", true);
// All sends will be validated by Firebase but not delivered
```

## Message properties

| Property | Type | Description |
|---|---|---|
| `Title` | `string` | Notification title |
| `sound` | `string` | Sound file to play (e.g., `"default"`) |
| `badge` | `int` | Badge count for the app icon |
| `image` | `string` | Image URL for rich notifications |
| `click_action` | `string` | Action to perform on notification tap |
| `priority` | `string` | `"normal"` or `"high"` |
| `ttl` | `int` | Time-to-live in seconds |

## Authentication

Firebase uses OAuth 2.0 with a JWT signed by the service account's private key. The `FirebaseServiceAccountAuthenticationProvider` handles this automatically:

1. Reads `ProjectId` and `ServiceAccountKey` from settings
2. Creates a signed JWT with the service account's private key
3. Exchanges the JWT for a Firebase OAuth 2.0 access token
4. Caches the token and refreshes it before expiry

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `INVALID_CREDENTIALS` | Wrong ProjectId or ServiceAccountKey | Verify in Firebase Console → Project Settings → Service Accounts |
| `INVALID_RECIPIENT` | Invalid FCM token | Token may be stale; request a new token from the client |
| `PROVIDER_VALIDATION_FAILED` | Payload too large | FCM limit is 4096 bytes for most payloads |
| Device not receiving | Token was unregistered | Remove token from your store; Firebase returns `UNREGISTERED` |
| Topic send failing | Topic doesn't exist | Topics are auto-created; verify the format starts with `/topics/` |
| Token expired | Token refreshed | Listen for token refresh on the client and update your store |

## FirebaseChannelSchemas

```csharp
// Push notification schema
FirebaseChannelSchemas.FirebasePush
```
