# Multi-Connector Sample

This sample demonstrates how to use multiple Ratatosk connectors together in a single ASP.NET Core web application, exposing them through REST API endpoints.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- Credentials for at least one of the supported messaging services (see individual setup guides)

## Supported Channels

| Channel | Key | Connector |
|---------|-----|-----------|
| Facebook Messenger | `facebook` | Ratatosk.Facebook |
| Firebase Cloud Messaging | `firebase` | Ratatosk.Firebase |
| SendGrid Email | `sendgrid` | Ratatosk.Sendgrid |
| Telegram Bot | `telegram` | Ratatosk.Telegram |
| Twilio SMS | `sms` | Ratatosk.Twilio |
| Twilio WhatsApp | `whatsapp` | Ratatosk.Twilio |

## Connector Setup

Each connector requires its own service account / API key. See the individual sample READMEs for detailed setup instructions:

- [Firebase Push](../firebase-push/README.md)
- [SendGrid Email](../sendgrid-email/README.md)
- [Telegram Bot](../telegram-bot/README.md)
- [Facebook Messenger](../facebook-messenger/README.md)
- [Twilio SMS & WhatsApp](../twilio/README.md)

## Configuration

Configure credentials in `appsettings.json` or via environment variables. The application reads settings from the `appsettings.json` file and the `appsettings.local.json` file (optional, git-ignored).

Example `appsettings.json`:

```json
{
  "Firebase": {
    "ProjectId": "my-project",
    "ServiceAccountKey": "{...}",
    "DryRun": true
  },
  "SendGrid": {
    "ApiKey": "SG.your-api-key",
    "SandboxMode": true
  },
  "Telegram": {
    "BotToken": "123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11"
  },
  "Twilio": {
    "AccountSid": "AC1234567890123456789012345678901234",
    "AuthToken": "your-auth-token"
  },
  "Facebook": {
    "PageAccessToken": "EAAx...your-token",
    "PageId": "123456789"
  }
}
```

You can also set each value as an environment variable using the `: ` (colon) separator or double-underscore separator, for example:

```bash
export Firebase__ProjectId="my-project"
export SendGrid__ApiKey="SG.your-api-key"
```

## Building & Running

```bash
./run.sh          # quiet
./run.sh -v       # show logs
```

| Flag | Description |
|------|-------------|
| `-v`, `--verbose` | Enable console logging output (hidden by default) |

To build without running:

```bash
dotnet build
```

The API server starts on `http://localhost:5000` (configurable via `launchSettings.json` or `ASPNETCORE_URLS`).

## API Endpoints

### List Available Schemas

```http
GET /schemas
```

Returns all registered channel schemas with their capabilities, parameters, and endpoint configurations.

### Send a Message

```http
POST /{channel}/message
Content-Type: application/json

{
  "id": "msg-001",
  "sender": { "type": "email", "address": "sender@example.com" },
  "receiver": { "type": "email", "address": "recipient@example.com" },
  "content": { "type": "text", "text": "Hello from the API!" },
  "properties": {
    "subject": { "name": "Subject", "value": "API Test" }
  }
}
```

Replace `{channel}` with one of: `facebook`, `firebase`, `sendgrid`, `telegram`, `sms`, `whatsapp`.

### Receive a Status Update

```http
POST /{channel}/message/status
Content-Type: application/json

{
  "MessageSid": "SM0001",
  "MessageStatus": "delivered"
}
```

### Receive a Webhook Payload

```http
POST /{channel}/receive
Content-Type: application/json

{
  "sender": { "id": "USER-PSID" },
  "recipient": { "id": "PAGE-ID" },
  "message": { "text": "Hello!" }
}
```

### Get Channel Status

```http
GET /{channel}/status
```

Returns the connector's current operational status.
