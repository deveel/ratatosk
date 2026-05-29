# Sender Manager Sample

Demonstrates sender identity management in Ratatosk via an ASP.NET Core Web API. The sample uses `ISenderRegistry` backed by `SenderManager` and an in-memory store (`InMemorySenderStore`) to provide full CRUD over sender identities, and shows how those identities integrate with the messaging pipeline.

## Features Demonstrated

- **Sender CRUD** — create, read, update, and delete sender identities via REST endpoints
- **Polymorphic sender resolution** — the `POST /api/messages` endpoint accepts both inline endpoints and sender identity references through the `IEndpoint` interface's `[JsonDerivedType]` support
- **Sender identity resolution** — when a `SenderRef` (`$type: "senderref"`) is used, the connector resolves it at send time via `ISenderResolver` → `SenderManager` → `InMemorySenderStore`
- **Messaging pipeline integration** — messages built with `MessageBuilder.From(IEndpoint)` preserve `ISender` instances through `Build()`, and the connector calls `ResolveSenderAsync` before sending

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- (Optional) A [SendGrid](https://sendgrid.com) API key to test message sending

## Packages

The sample references the following Ratatosk packages:

| Package | Purpose |
|---------|---------|
| `Ratatosk.Abstractions` | Core types (`IEndpoint`, `Message`, `MessageBuilder`, `ISender` et al.) |
| `Ratatosk.Connector.Abstractions` | Connector interfaces (`IChannelConnector`, `IMessageIdGenerator`) |
| `Ratatosk.Connectors` | `ChannelConnectorBase` — base class that handles sender resolution in `SendMessageAsync` |
| `Ratatosk` | `MessagingBuilder`, `IMessagingClient`, `MessagingClient` — DI registration and client API |
| `Ratatosk.Senders` | `SenderManager`, `ISenderRegistry`, `SenderResolver`, `MessageBuilderExtensions.FromSender()` |
| `Ratatosk.Senders.InMemory` | `InMemorySenderStore` + `AddSenderInMemoryStore()` — in-memory `IRepository<SenderEntity>` |
| `Ratatosk.Sendgrid` | SendGrid email connector used as the sample channel |

NuGet dependencies: `Deveel.Results` (operation result types), `Microsoft.Extensions.Http`, `Microsoft.Extensions.Logging.Console`.

## Configuration

Configure SendGrid credentials in `appsettings.json` or via environment variables:

```json
{
  "SendGrid": {
    "ApiKey": "SG.your-api-key"
  }
}
```

Or using environment variables:

```bash
export SendGrid__ApiKey="SG.your-api-key"
```

The sample seeds three senders at startup (`support`, `notifications`, `sms-alerts`) so the CRUD endpoints work immediately without configuration.

## Building & Running

```bash
dotnet build
dotnet run
```

The server starts on `http://localhost:5000` by default.

## API Endpoints

### Sender Management

#### List All Senders

```http
GET /api/senders
```

#### Get Sender by ID

```http
GET /api/senders/{id}
```

#### Create Sender

```http
POST /api/senders
Content-Type: application/json

{
  "name": "marketing",
  "displayName": "Marketing Team",
  "address": "marketing@example.com",
  "endpointType": "email",
  "isActive": true
}
```

#### Update Sender

```http
PUT /api/senders/{id}
Content-Type: application/json

{
  "displayName": "Marketing Team (Updated)",
  "address": "new-marketing@example.com"
}
```

#### Delete Sender

```http
DELETE /api/senders/{id}
```

### Send a Message

The sender field accepts polymorphic endpoints. Use `System.Text.Json` derived-type discriminators:

#### Using a Sender Identity Reference (resolved at send time)

```http
POST /api/messages
Content-Type: application/json

{
  "channel": "sendgrid",
  "sender": { "$type": "senderref", "senderName": "support" },
  "receiverAddress": "user@example.com",
  "text": "Hello from the Sender Manager Sample!"
}
```

The `SenderRef` is resolved by the connector via `ISenderResolver` → `ISenderRegistry` before the message is sent.

#### Using an Inline Endpoint

```http
POST /api/messages
Content-Type: application/json

{
  "channel": "sendgrid",
  "sender": { "$type": "endpoint", "address": "direct@example.com", "type": "email" },
  "receiverAddress": "user@example.com",
  "text": "Hello from an inline sender!"
}
```

#### Using Specialised Sender Types

```http
POST /api/messages
Content-Type: application/json

{
  "channel": "sendgrid",
  "sender": { "$type": "email", "emailAddress": "noreply@example.com", "displayName": "No Reply" },
  "receiverAddress": "user@example.com",
  "text": "Hello!"
}
```

Available `$type` values: `endpoint`, `senderref`, `email`, `phone`, `alphanumeric`, `bot`.

## How It Works

1. `AddMessaging().AddClient().AddSenders()` registers all sender infrastructure including `ISenderRegistry` → `SenderManager`, `ISenderResolver` → `SenderResolver`, and the sender cache/selector/validator
2. `AddSenderInMemoryStore()` provides an in-memory `IRepository<SenderEntity>` seeded with sample senders
3. `MessageBuilder.From(request.Sender!)` sets the sender on the message. If `Sender` is a `SenderRef`, it carries a logical name instead of a concrete address
4. `IMessagingClient.SendAsync("sendgrid", message)` delegates to the SendGrid connector
5. `ChannelConnectorBase.SendMessageAsync` calls `ResolveSenderAsync` which uses `ISenderResolver` to resolve `SenderRef` instances from the registry. The resolved sender replaces the `SenderRef` on the message before validation and dispatch

This means the API never needs to resolve the sender itself — it passes a reference and lets the connector pipeline handle it naturally.
