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

var message = new Message()
    .WithId("tg-1")
    .WithReceiver(Endpoint.Id("123456789"))
    .WithTextContent("Hello from Telegram Bot!");

var result = await connector.SendMessageAsync(message, ct);
```

### Rich text with MarkdownV2

```csharp
new Message()
    .WithReceiver(Endpoint.Id("123456789"))
    .WithTextContent("*bold* _italic_ `code` [link](https://example.com)")
    .With("ParseMode", "MarkdownV2");
```

### Rich text with HTML

```csharp
new Message()
    .WithReceiver(Endpoint.Id("123456789"))
    .WithTextContent("<b>bold</b> <i>italic</i> <code>code</code>")
    .With("ParseMode", "HTML");
```

### Image

```csharp
new Message()
    .WithReceiver(Endpoint.Id("123456789"))
    .WithContent(new MediaContent(MediaType.Image, "photo.jpg",
        "https://example.com/photo.jpg"));
```

### Document

```csharp
new Message()
    .WithReceiver(Endpoint.Id("123456789"))
    .WithContent(new MediaContent(MediaType.Document, "report.pdf",
        null, pdfFileBytes));
```

### Image with caption

```csharp
new Message()
    .WithReceiver(Endpoint.Id("123456789"))
    .WithContent(new MediaContent(MediaType.Image, "photo.jpg",
        "https://example.com/photo.jpg"))
    .With("caption", "Check out this photo!");
```

### Location

```csharp
new Message()
    .WithReceiver(Endpoint.Id("123456789"))
    .WithContent(new LocationContent(41.9028, 12.4964)
        .WithHorizontalAccuracy(10));
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
            var reply = new Message()
                .WithId(Guid.NewGuid().ToString("n"))
                .WithReceiver(message.Sender!)
                .WithTextContent($"Echo: {text}");

            await _connector.SendMessageAsync(reply, ct);
        }
    }

    return Ok();  // Telegram expects 200 OK
}
```

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
