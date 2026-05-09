# Facebook Messenger Connector

Use this connector for Facebook Page messaging through the Messenger Platform API.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Facebook
```

## Required settings

- `PageAccessToken`
- `PageId`

Optional webhook-related settings: `WebhookUrl`, `VerifyToken`.

## Minimal send example

```csharp
var settings = new ConnectionSettings()
    .SetParameter("PageAccessToken", "EAA...")
    .SetParameter("PageId", "1234567890");

var connector = new FacebookMessengerConnector(settings);
await connector.InitializeAsync(ct);

var message = new Message
{
    Id = Guid.NewGuid().ToString(),
    Receiver = new Endpoint(EndpointType.UserId, "facebook-user-psid"),
    Content = new TextContent("Hello from Facebook")
};

var result = await connector.SendMessageAsync(message, ct);
```

## Webhook basics

- Handle Facebook verification challenge (GET)
- Process message events (POST)
- Validate `X-Hub-Signature-256` before parsing payload

## Quick troubleshooting

- Recipient errors: verify PSID and page permissions
- Auth failures: regenerate long-lived page token
- Webhook issues: verify callback URL, verify token, and subscribed events

## Related docs

- [Advanced configuration](../advanced-configuration.md)
- [Connector implementation](../channelconnector-usage.md)
