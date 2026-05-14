# Facebook Messenger Connector

Send and receive messages through the Facebook Messenger Platform API using a Facebook Page.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Facebook
```

## Required settings

| Parameter | Type | Description |
|---|---|---|
| `PageAccessToken` | `string` | Long-lived Facebook Page access token (starts with `EAA`) |
| `PageId` | `string` | Facebook Page ID (numeric) |

### Optional settings

| Parameter | Type | Description |
|---|---|---|
| `WebhookUrl` | `string` | URL for receiving Messenger webhook events |
| `VerifyToken` | `string` | Token used for webhook verification challenge |

The `PageAccessToken` must be a long-lived token (valid ~60 days). Generate it from Facebook Developer Console → Your App → Messenger → Settings → Page Access Token.

## Schema

| Property | Value |
|---|---|
| Provider | `Facebook` |
| Type | `Messenger` |
| Version | `1.0.0` |
| Capabilities | `SendMessages`, `ReceiveMessages`, `MediaAttachments` |
| Content types | `PlainText`, `Media`, `Json` |
| Endpoints | `UserId` (PSID), `Id` (page-scoped ID) |
| Authentication | API Key (`PageAccessToken`) |

## Send examples

### Text message

```csharp
var settings = new ConnectionSettings()
    .SetParameter("PageAccessToken", "EAA...")
    .SetParameter("PageId", "1234567890");

var connector = new FacebookMessengerConnector(settings);
await connector.InitializeAsync(ct);

var message = new MessageBuilder()
    .WithId("fb-1")
    .To(Endpoint.User("facebook-user-psid"))
    .WithText("Hello from Facebook Messenger")
    .WithMessagingType("RESPONSE")
    .Build();

var result = await connector.SendMessageAsync(message, ct);
```

### Message with media (image)

```csharp
new MessageBuilder()
    .To(Endpoint.User("psid-123"))
    .WithContent(new MediaContent(MediaType.Image, "photo.jpg",
        "https://example.com/photo.jpg"))
    .Build();
```

### Message with quick replies

```csharp
new MessageBuilder()
    .To(Endpoint.User("psid-123"))
    .WithText("Choose an option:")
    .WithQuickReplies("[{\"content_type\":\"text\",\"title\":\"Yes\",\"payload\":\"YES\"},{\"content_type\":\"text\",\"title\":\"No\",\"payload\":\"NO\"}]")
    .Build();
```

## Webhook setup

Facebook Messenger uses webhooks for inbound messages and delivery events.

### 1. Configure webhook in Facebook Developer Console

- Go to your App → Messenger → Settings
- Set the callback URL to your endpoint
- Set a verify token (must match the one in your settings)

### 2. Handle verification challenge

Facebook sends a GET request with `hub.mode`, `hub.verify_token`, and `hub.challenge` parameters. The connector handles this automatically via `ReceiveMessagesAsync`.

### 3. Process inbound messages

```csharp
[HttpGet("/webhooks/facebook")]
[HttpPost("/webhooks/facebook")]
public async Task<IActionResult> FacebookWebhook(CancellationToken ct)
{
    if (Request.Method == "GET")
    {
        // Verification challenge — handled by the connector
        using var reader = new StreamReader(Request.Body);
        var query = Request.QueryString.Value!;
        var source = MessageSource.UrlPost(query);
        var result = await _connector.ReceiveMessagesAsync(source, ct);
        return Content(result.Data?.Messages.FirstOrDefault()?.Id ?? "error");
    }

    // Inbound message
    using var postReader = new StreamReader(Request.Body);
    var body = await postReader.ReadToEndAsync(ct);
    var msgSource = MessageSource.Json(body);

    // Verify signature
    var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
    if (!IsValidSignature(body, signature))
        return Unauthorized();

    var result = await _connector.ReceiveMessagesAsync(msgSource, ct);
    return Ok();
}
```

Validate `X-Hub-Signature-256` header using your Facebook app secret before processing.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `INVALID_CREDENTIALS` | Token expired or invalid | Regenerate long-lived token (valid ~60 days) |
| `INVALID_RECIPIENT` | PSID doesn't exist or page can't message user | User must have interacted with the page |
| Webhook verification fails | VerifyToken mismatch | Ensure `VerifyToken` matches Facebook Console |
| Message not delivered | User blocked the page | Remove user from your store |
| `RATE_LIMITED` | Too many messages | Facebook limits: 250 messages per page per hour (standard) |
