# Sender Manager Sample

Demonstrates sender identity management in Ratatosk via an ASP.NET Core Web API. The sample uses `ISenderRepository<SenderEntity>` backed by `SenderManager<SenderEntity>` and an in-memory store (`InMemorySenderRepository`) to provide full CRUD over sender identities, and shows how those identities integrate with the messaging pipeline.

## Features Demonstrated

- **Sender CRUD** — create, read, update, and delete sender identities via REST endpoints
- **Polymorphic sender resolution** — the `POST /api/messages` endpoint accepts both inline endpoints and sender identity references through the `IEndpoint` interface's `[JsonDerivedType]` support
- **Sender identity resolution** — when a `SenderRef` (`$type: "senderref"`) is used, the connector resolves it at send time via `ISenderResolver` → `ISenderRepository<SenderEntity>` → `InMemorySenderRepository`
- **Messaging pipeline integration** — messages built with `MessageBuilder.From(IEndpoint)` preserve `ISender` and `IUnresolvedSender` instances through `Build()`, and the connector calls `ResolveSenderAsync` before sending

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- (Optional) A [SendGrid](https://sendgrid.com) API key to test message sending

## Packages

The sample references the following Ratatosk packages:

| Package | Purpose |
|---------|---------|
| `Ratatosk.Abstractions` | Core types (`IEndpoint`, `IUnresolvedSender`, `Message`, `MessageBuilder`, `ISender`, `Sender` et al.) |
| `Ratatosk.Connector.Abstractions` | Connector interfaces (`IChannelConnector`, `IMessageIdGenerator`, `ISenderResolver`) |
| `Ratatosk.Connectors` | `ChannelConnectorBase` — base class that handles sender resolution in `SendMessageAsync` |
| `Ratatosk` | `MessagingBuilder`, `IMessagingClient`, `MessagingClient` — DI registration and client API |
| `Ratatosk.Senders` | `ISenderRepository<TSender>`, `SenderManager<TSender>`, `SenderResolver`, `MessageBuilderExtensions.FromSender()` |
| `Ratatosk.Senders.InMemory` | `InMemorySenderRepository` — in-memory `ISenderRepository<SenderEntity>` |
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
  "endpointType": "EmailAddress",
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

The `SenderRef` is resolved by the connector via `ISenderResolver` → `ISenderRepository<SenderEntity>` before the message is sent.

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
  "sender": { "$type": "email", "address": "noreply@example.com", "name": "No Reply", "displayName": "No Reply", "isActive": true },
  "receiverAddress": "user@example.com",
  "text": "Hello!"
}
```

Available `$type` values: `endpoint`, `sender`, `senderref`, `email`, `phone`, `alphanumeric`, `bot`.

## How It Works

1. `AddMessaging().AddClient().AddSenders<SenderEntity>(cfg => cfg.UseInMemoryStore(seedSenders))` registers cache, `SenderManager<SenderEntity>`, `ISenderResolver`, and the seeded in-memory store.
2. `MessageBuilder.From(request.Sender!)` sets the sender on the message. If `Sender` is a `SenderRef`, it carries a logical name instead of a concrete address
3. `IMessagingClient.SendAsync("sendgrid", message)` delegates to the SendGrid connector
4. `ChannelConnectorBase.SendMessageAsync` calls `ResolveSenderAsync` which uses `ISenderResolver` to resolve `SenderRef` instances from the repository. The resolved sender replaces the `SenderRef` on the message before validation and dispatch

This means the API never needs to resolve the sender itself — it passes a reference and lets the connector pipeline handle it naturally.
