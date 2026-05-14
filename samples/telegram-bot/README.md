# Telegram Bot Sample

This sample demonstrates how to use the Deveel Messaging Telegram Bot connector to send messages via the Telegram Bot API.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- A [Telegram](https://telegram.org/) account
- A Telegram Bot Token (from [@BotFather](https://t.me/botfather))

## Telegram Bot Setup

1. Open Telegram and search for [@BotFather](https://t.me/botfather).
2. Send `/newbot` and follow the prompts to create a new bot.
3. Copy the **HTTP API token** (e.g., `123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11`).
4. To test message sending, you need a **Chat ID**:
   - Send a message to your bot.
   - Visit `https://api.telegram.org/bot<YOUR_TOKEN>/getUpdates`.
   - Find your Chat ID in the JSON response (it appears as `chat.id`).
5. (Optional) To use webhooks, you need a public HTTPS endpoint (e.g., using ngrok).

## Configuration

Run the configuration wizard:

```bash
dotnet run -- telegram configure
```

You will be prompted for:

| Field | Description |
|-------|-------------|
| BotToken | Your Telegram bot token from @BotFather |
| ChatId | The chat/user/group ID to send messages to |
| WebhookUrl | (Optional) Public HTTPS URL for webhook updates |
| SecretToken | (Optional) Secret token for webhook validation |
| MediaUrl | (Optional) Default URL for media messages |
| LocationLatitude | (Optional) Default latitude for location tests |
| LocationLongitude | (Optional) Default longitude for location tests |

Alternatively, set environment variables:

```bash
export TELEGRAM_BOT_TOKEN="123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11"
export TELEGRAM_CHAT_ID="123456789"
export TELEGRAM_WEBHOOK_URL="https://example.com/webhook"
```

## Building & Running

The `run.sh` script builds the required libraries (only if missing) and runs the sample in one step:

```bash
./run.sh -- telegram <command>
```

| Flag | Description |
|------|-------------|
| `-b`, `--build-libs` | Force rebuild library dependencies even if already present |
| `-v`, `--verbose` | Enable console logging output (hidden by default) |

Examples:

```bash
./run.sh -- telegram send           # quiet, build deps only if needed
./run.sh -v -- telegram status      # show logs, build deps only if needed
./run.sh -b -- telegram configure   # force rebuild deps, quiet run
./run.sh -b -v -- telegram send     # force rebuild deps + show logs
```

To build without running:

```bash
./build-libs.sh
dotnet build
```

### Commands

| Command | Description |
|---------|-------------|
| `schema [name]` | Show all Telegram schemas or a single schema |
| `configure` | Prompt for credentials and save to local config |
| `validate -k <kind>` | Validate a sample message (`text` or `location`) |
| `status` | Show the connector runtime status |
| `send` | Build and send a live Telegram message interactively |

### Send Example

```bash
./run.sh -- telegram send
```

You will be prompted to select the message type (Text, Media, or Location) and compose the content. The text body supports multi-line input — type `!done` on a new line to finish.
