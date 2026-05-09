# Telegram Bot Connector

Use this connector for Telegram bot messaging with outbound sends and webhook-based inbound handling.

## Package

```bash
dotnet add package Deveel.Messaging.Connector.Telegram
```

## Required settings

- `BotToken`

Optional: `WebhookUrl`, `SecretToken`, `ParseMode`, timeout and retry parameters.

## Minimal send example

```csharp
var settings = new ConnectionSettings()
    .AddParameter("BotToken", "123456:ABC...");

var connector = new TelegramBotConnector(TelegramChannelSchemas.SimpleTelegramBot, settings);
await connector.InitializeAsync(ct);

var message = new MessageBuilder()
    .WithId("tg-001")
    .WithIdReceiver("123456789")
    .WithTextContent("Hello from Telegram")
    .Message;

var result = await connector.SendMessageAsync(message, ct);
```

## Useful features

- Rich text via `ParseMode`
- Media and document sends
- Inline keyboard payloads
- Webhook-based inbound processing

## Webhook essentials

- Set webhook URL in connector settings
- Validate `X-Telegram-Bot-Api-Secret-Token`
- Parse update payload and map to `IMessage`

## Quick troubleshooting

- Invalid chat id: verify bot access and destination type (user/group/channel)
- Bot blocked: treat as permanent recipient failure
- Message too long: split text into chunks

## Related docs

- [Advanced configuration](../advanced-configuration.md)
- [Connector implementation](../channelconnector-usage.md)
