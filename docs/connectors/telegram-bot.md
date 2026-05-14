# Telegram Bot Connector

Send and receive messages through the Telegram Bot API.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Telegram
```

## Required settings

| Parameter | Type | Description |
|---|---|---|
| `BotToken` | `string` | Telegram Bot token from BotFather (format: `123456:ABC...`) |

### Optional settings

| Parameter | Type | Default | Description |
|---|---|---|---|
| `WebhookUrl` | `string` | — | Public HTTPS URL for receiving updates |
| `SecretToken` | `string` | — | Secret token for webhook request validation |
| `ParseMode` | `string` | — | Default parse mode: `MarkdownV2`, `HTML`, or `Markdown` |

The Bot token is obtained from [@BotFather](https://t.me/BotFather) on Telegram. The webhook URL must be HTTPS. 

## Schema

| Property | Value |
|---|---|
| Provider | `Telegram` |
| Type | `Bot` |
| Version | `1.0.0` |
| Capabilities | `SendMessages`, `ReceiveMessages`, `MediaAttachments` |
| Content types | `PlainText`, `Media` |
| Endpoints | `Id` (chat ID) |
| Authentication | Token (bot token) |

## Send examples

### Text message

```csharp
var settings = new ConnectionSettings()
    .SetParameter("BotToken", "123456:ABC...");

var connector = new TelegramBotConnector(TelegramChannelSchemas.SimpleTelegramBot, settings);
await connector.InitializeAsync(ct);

var message = new MessageBuilder()
    .WithId("tg-1")
    .To(Endpoint.Id("123456789"))
    .WithText("Hello from Telegram Bot!")
    .Build();

var result = await connector.SendMessageAsync(message, ct);
```

### Rich text with MarkdownV2

```csharp
new MessageBuilder()
    .To(Endpoint.Id("123456789"))
    .WithText("*bold* _italic_ `code` [link](https://example.com)")
    .WithParseMode("MarkdownV2")
    .Build();
```

### Rich text with HTML

```csharp
new MessageBuilder()
    .To(Endpoint.Id("123456789"))
    .WithText("<b>bold</b> <i>italic</i> <code>code</code>")
    .WithParseMode("HTML")
    .Build();
```

### Image

```csharp
new MessageBuilder()
    .To(Endpoint.Id("123456789"))
    .WithContent(new MediaContent(MediaType.Image, "photo.jpg",
        "https://example.com/photo.jpg"))
    .Build();
```

### Document

```csharp
new MessageBuilder()
    .To(Endpoint.Id("123456789"))
    .WithContent(new MediaContent(MediaType.Document, "report.pdf",
        null, pdfFileBytes))
    .Build();
```

### Image with caption

```csharp
new MessageBuilder()
    .To(Endpoint.Id("123456789"))
    .WithContent(new MediaContent(MediaType.Image, "photo.jpg",
        "https://example.com/photo.jpg"))
    .WithCaption("Check out this photo!")
    .Build();
```

### Location

```csharp
new MessageBuilder()
    .To(Endpoint.Id("123456789"))
    .WithContent(new LocationContent(41.9028, 12.4964)
        .WithHorizontalAccuracy(10))
    .Build();
```

## Message properties

| Property | Type | Description |
|---|---|---|
| `ParseMode` | `string` | Text formatting: `MarkdownV2`, `HTML`, `Markdown` |
| `caption` | `string` | Caption for media messages (max 1024 chars) |
| `disable_notification` | `bool` | Send silently |
| `protect_content` | `bool` | Disable forwarding and saving |
| `reply_to_message_id` | `int` | ID of the message to reply to |
| `allow_sending_without_reply` | `bool` | Allow reply even if the original message not found |

## Webhook setup

### 1. Configure webhook

Set the webhook URL in connection settings:

```csharp
var settings = new ConnectionSettings()
    .SetParameter("BotToken", "123456:ABC...")
    .SetParameter("WebhookUrl", "https://yourdomain.com/webhooks/telegram")
    .SetParameter("SecretToken", "your-secret-token");
```

### 2. Handle incoming updates

```csharp
[HttpPost("/webhooks/telegram")]
public async Task<IActionResult> TelegramWebhook(CancellationToken ct)
{
    // Validate secret token
    var secretToken = Request.Headers["X-Telegram-Bot-Api-Secret-Token"].FirstOrDefault();
    if (secretToken != expectedSecretToken)
        return Unauthorized();

    // Read the update
    using var reader = new StreamReader(Request.Body);
    var body = await reader.ReadToEndAsync(ct);
    var source = MessageSource.Json(body);

    // Process inbound messages
    var result = await _connector.ReceiveMessagesAsync(source, ct);

    if (result.IsSuccess)
    {
        foreach (var message in result.Data?.Messages ?? [])
        {
            var text = (message.Content as TextContent)?.Text;
            Console.WriteLine($"Received from {message.Sender?.Address}: {text}");

            // Auto-reply
            var reply = new MessageBuilder()
                .WithId(Guid.NewGuid().ToString("n"))
                .To(message.Sender!)
                .WithText($"Echo: {text}")
                .Build();

            await _connector.SendMessageAsync(reply, ct);
        }
    }

    return Ok();  // Telegram expects 200 OK
}
```

## Error codes

Telegram-specific error codes are defined in `TelegramErrorCodes` with domain `"Telegram"`.

| Code | Description |
|---|---|
| `INVALID_CHAT_ID` | Chat ID is missing or invalid |
| `FILE_TOO_LARGE` | File exceeds Telegram size limits |
| `INVALID_MEDIA_URL` | Media URL is not valid |
| `BOT_BLOCKED` | Bot was blocked by the user |
| `CHAT_NOT_FOUND` | Chat does not exist or bot cannot access it |
| `UNAUTHORIZED` | Bot token is invalid or unauthorized |

Standard `MessagingErrorCodes` are also used — see the [error codes reference](../result-types.md#error-code-tables).

### Original provider codes

Telegram Bot API errors (`ApiRequestException`) are mapped to framework error codes via two mapping methods in `TelegramService`:

**`MapTelegramErrorCode`** (getMe, initialization):

| Telegram code | Mapped framework code |
|---|---|
| `400` | `INVALID_CREDENTIALS` |
| `401` | `UNAUTHORIZED` |
| `403` | `BOT_BLOCKED` |
| `404` | `CHAT_NOT_FOUND` |
| `429` | `UNSUPPORTED_CONTENT_TYPE` |
| Other | `UNAUTHORIZED` |

**`MapTelegramSendErrorCode`** (sendMessage, sendPhoto, etc.):

| Telegram code | Mapped framework code |
|---|---|
| `400` | `INVALID_CHAT_ID` |
| `403` | `BOT_BLOCKED` |
| `404` | `CHAT_NOT_FOUND` |
| `429` | `UNSUPPORTED_CONTENT_TYPE` |
| Other | `UNSUPPORTED_CONTENT_TYPE` |

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `INVALID_CREDENTIALS` | Wrong bot token | Re-generate token from BotFather |
| `INVALID_RECIPIENT` | Chat ID doesn't exist or bot can't message | User must `/start` the bot first |
| `PROVIDER_VALIDATION_FAILED` | Invalid message format | Check ParseMode escaping rules |
| Message too long | Exceeds 4096 characters | Split into multiple messages using `reply_to_message_id` |
| Bot blocked | User blocked the bot | Remove chat ID from your store |
| Webhook not receiving | HTTPS required | Telegram only accepts HTTPS webhook URLs |
| Webhook fails | Secret token mismatch | Verify `SecretToken` matches what Telegram sent |

## TelegramChannelSchemas

```csharp
// Simple Telegram bot schema (send only)
TelegramChannelSchemas.SimpleTelegramBot
```
