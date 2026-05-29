# Sender Management

The sender identity system solves a common problem: **message composition should not depend on sender configuration**. When you write `new MessageBuilder().FromEmail("admin@example.com")`, you hardcode the sender address into the message — changing it requires a code deploy. The sender identity system lifts sender details out of message code and into a managed registry, where they can be created, updated, and activated independently of the application.

## Concepts

### Sender vs endpoint

A sender is an `IEndpoint` that also carries identity semantics — it represents *who* the message is from, not just an address. The framework distinguishes between:

- **Inline senders** — concrete sender details used directly on the message (`EmailSender`, `PhoneSender`, etc.). No resolution needed; the connector uses them as-is.
- **Identity references** — a logical name (`SenderRef`) that the connector resolves at send time. The actual sender (address, display name, endpoint type) is looked up from a registry.

This separation lets you build and dispatch messages with a logical name like `"support"`, while the concrete "support@example.com" endpoint lives in the registry — changeable at runtime without touching message code.

### The resolution pipeline

When a message carrying a `SenderRef` reaches the connector, the framework resolves it before sending:

```
SenderRef("support")
  → cache check (avoid repeated lookups)
  → registry lookup by name
  → selector picks best match (when multiple senders share a name)
  → replace SenderRef with concrete sender on the message
  → validate against schema
  → send
```

Resolution is automatic — the connector handles it. The API or service that builds the message never needs to query the registry.

### Registry as the source of truth

The registry persists sender entities (name, address, endpoint type, active flag). It decouples *who can send* from *how messages are built* — operations teams manage senders through CRUD, while developers reference them by name.

### Cache for performance

Resolution results are cached by sender name with a configurable TTL (default 5 minutes). Frequent messages using the same `SenderRef` skip the registry lookup, reducing latency and load on the store.

### Selector for disambiguation

When multiple sender entities share the same logical name (e.g. different `EmailSender` entries for different channels), the selector picks one based on the message context — the channel, content type, and receiver endpoint.

## Sender identity model

All sender types implement a common interface and carry an `EndpointType` tag that tells the schema validator what kind of address they hold.

**Inline endpoints** — concrete sender, used directly:

| Type | Meaning | Typical use case |
|------|---------|-----------------|
| `Endpoint` | Generic address with type tag | Any channel, when no special sender type is needed |
| `EmailSender` | Email address + optional display name | Transactional email from a named sender |
| `PhoneSender` | Phone number + optional display name | SMS from a branded phone number |
| `AlphaNumericSender` | Alphanumeric ID (e.g. brand name) | SMS sender ID in regions that allow alphanumeric |
| `BotSender` | Bot identifier | Chat bot (Telegram, Facebook) |

**Identity references** — logical name, resolved at runtime:

| Type | Meaning | Typical use case |
|------|---------|-----------------|
| `SenderRef` | Logical name resolved via registry | Decoupled message composition |

## Package and registration

```bash
dotnet add package Ratatosk.Senders
```

The `Ratatosk.Senders` package extends `IServiceCollection` directly — the `Ratatosk` package and `AddMessaging()` are **not** required. Sender management works as a standalone feature.

### Standalone (no messaging)

```csharp
using Ratatosk;

builder.Services.AddSenders();
builder.Services.AddSenderInMemoryStore();
```

### With the messaging builder

```csharp
builder.Services.AddMessaging()
    .AddClient()
    .AddSenders()
    .AddSendGridEmail("sendgrid", cfg => cfg.WithSettings("SendGrid"));
```

### Storage backends

```csharp
// In-memory (development/testing):
builder.Services.AddSenderInMemoryStore(seedSenders);

// Entity Framework:
builder.Services.AddSenderEntityFrameworkStore<MyDbContext>();

// Custom:
builder.Services.AddSenderStore<MySenderStore>();
```

## Managing senders

### Creating sender entities

`SenderBuilder` provides a fluent API for constructing sender entities:

```csharp
var sender = new SenderBuilder()
    .WithName("support")
    .WithDisplayName("Customer Support")
    .WithAddress("support@example.com")
    .WithEndpointType("email")
    .AsActive(true)
    .Build();

await registry.CreateAsync(sender);
```

Each entity carries: `Id` (auto-generated), `Name` (logical name used in `SenderRef`), `DisplayName`, `Address`, `EndpointType`, `IsActive`, `CreatedAt`, `UpdatedAt`.

### Registry CRUD

```csharp
public class SenderService
{
    private readonly ISenderRegistry _registry;

    public SenderService(ISenderRegistry registry) => _registry = registry;

    public Task<IList<SenderEntity>> GetAll() => _registry.GetAllAsync();

    public Task<SenderEntity?> FindByName(string name)
        => _registry.FindByNameAsync(name);

    public async Task<SenderEntity> Create(string name, string address, string type)
    {
        var result = await _registry.CreateAsync(
            new SenderBuilder()
                .WithName(name)
                .WithAddress(address)
                .WithEndpointType(type)
                .Build());
        return result.Value!;
    }

    public async Task Update(SenderEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _registry.UpdateAsync(entity);
    }

    public Task Delete(SenderEntity entity)
        => _registry.DeleteAsync(entity);
}
```

### Seed data

```csharp
builder.Services.AddSenderInMemoryStore(new[]
{
    new SenderBuilder()
        .WithName("support")
        .WithDisplayName("Customer Support")
        .WithAddress("support@example.com")
        .WithEndpointType("email")
        .Build(),

    new SenderBuilder()
        .WithName("sms-alerts")
        .WithDisplayName("SMS Alert System")
        .WithAddress("+15551234567")
        .WithEndpointType("phone")
        .Build()
});
```

## Using senders in messages

### With a logical name (SenderRef)

The message carries a reference — the connector resolves it:

```csharp
var message = new MessageBuilder()
    .FromSender("support")
    .ToEmail("user@example.com")
    .WithText("Hello!")
    .Build();

await client.SendAsync("sendgrid", message);
```

This is the pattern that decouples message code from sender configuration. The name `"support"` is resolved at send time.

### With an inline sender

When the sender is known at message-construction time:

```csharp
var message = new MessageBuilder()
    .From(new EmailSender("noreply@example.com", "No Reply"))
    .ToEmail("user@example.com")
    .WithText("Hello!")
    .Build();
```

### From a JSON API

```json
POST /api/messages
Content-Type: application/json

{
  "channel": "sendgrid",
  "sender": { "$type": "senderref", "senderName": "support" },
  "receiverAddress": "user@example.com",
  "text": "Hello!"
}
```

The `$type` discriminator tells `System.Text.Json` which concrete endpoint type to deserialize. See the [message model](messaging-model.md#polymorphic-json-serialization) for the full list.

## Configuring resolution

### Selector strategy

Controls which sender is chosen when the registry returns multiple entities with the same name:

| Strategy | Behaviour |
|----------|-----------|
| `FirstMatchSenderSelector` (default) | Returns the first active sender matching channel and endpoint type |
| `RoundRobinSenderSelector` | Cycles through matching senders in round-robin order |
| `ExplicitSenderSelector` | Requires an explicit sender name; fails if absent |

```csharp
builder.Services.AddSingleton<ISenderSelector, RoundRobinSenderSelector>();
```

### Cache TTL

Resolution results expire after the configured TTL:

```csharp
builder.Services.AddSingleton<ISenderCache>(
    sp => new InMemorySenderCache(TimeSpan.FromMinutes(15)));
```

## Connector-level defaults

Configure a default sender per-connector — used when a message has no sender set:

```csharp
builder.Services.AddMessaging()
    .AddClient()
    .AddSenders()
    .AddSendGridEmail("sendgrid", cfg => cfg
        .WithSettings("SendGrid")
        .WithSenders(s => s
            .WithDefault(d => d
                .WithName("default-sender")
                .WithDisplayName("Default")
                .WithAddress("default@example.com")
                .WithEndpointType("email"))));
```

## End-to-end summary

1. **Register** — `services.AddSenders()` + storage backend
2. **Manage** — create/update/delete senders in the registry via `ISenderRegistry`
3. **Reference** — build messages with `MessageBuilder.FromSender(name)`
4. **Send** — the connector resolves the name, validates, and dispatches
