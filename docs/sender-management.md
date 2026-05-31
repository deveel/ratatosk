# Sender Management

The sender identity system solves a common problem: **message composition should not depend on sender configuration**. When you write `new MessageBuilder().FromEmail("admin@example.com")`, you hardcode the sender address into the message — changing it requires a code deploy. The sender identity system lifts sender details out of message code and into a managed registry, where they can be created, updated, and activated independently of the application.

## Concepts

### Sender vs endpoint

A sender is an `IEndpoint` that also carries identity semantics — it represents *who* the message is from, not just an address. The framework distinguishes between:

- **Inline senders** — concrete sender details used directly on the message (`EmailSender`, `PhoneSender`, etc.). No resolution needed; the connector uses them as-is.
- **Unresolved references** — a logical name (`SenderRef`) that the connector resolves at send time. The actual sender (address, display name, endpoint type) is looked up from a registry. `SenderRef` implements the `IUnresolvedSender` marker interface to signal that resolution is required.
- **Plain endpoints** — an `Endpoint` with a type and address. These are not resolved; they pass through as-is.

This separation lets you build and dispatch messages with a logical name like `"support"`, while the concrete "support@example.com" endpoint lives in the registry — changeable at runtime without touching message code.

### The resolution pipeline

When a message carrying a `SenderRef` reaches the connector, the framework resolves it before sending:

```
SenderRef("support")
  → cache check (avoid repeated lookups)
  → repository lookup by name
  → replace SenderRef with concrete ISender on the message
  → validate against schema
  → send
```

Resolution is automatic — the connector handles it. The API or service that builds the message never needs to query the registry.

### Repository as the source of truth

The repository persists sender entities (name, address, endpoint type, active flag). It decouples *who can send* from *how messages are built* — operations teams manage senders through CRUD, while developers reference them by name.

The repository interface `ISenderRepository<TSender>` extends Kista's `IRepository<TSender>`, inheriting all standard CRUD, pagination, and query capabilities. Custom implementations can use their own storage-bound entity types (e.g., `DbSender`, `MongoSender`) that implement `ISender`.

### Cache for performance

Resolution results are cached by sender name with a configurable TTL (default 5 minutes). Frequent messages using the same `SenderRef` skip the repository lookup, reducing latency and load on the store.

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
| `Sender` | Concrete sender entity for the registry | Default entity type stored in the sender repository |

**Unresolved references** — logical name, resolved at runtime:

| Type | Meaning | Typical use case |
|------|---------|-----------------|
| `SenderRef` | Logical name resolved via repository | Decoupled message composition |

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
builder.Services.AddSenderRepository<MySenderStore, MySender>();
```

## Managing senders

### Creating senders

`SenderBuilder` provides a fluent API for constructing `Sender` instances:

```csharp
var sender = new SenderBuilder()
    .WithName("support")
    .WithDisplayName("Customer Support")
    .WithAddress("support@example.com")
    .WithEndpointType(EndpointType.EmailAddress)
    .AsActive(true)
    .Build();

await repository.AddAsync(sender);
```

Each sender carries: `Name` (logical name used in `SenderRef`), `DisplayName`, `Address`, `EndpointType`, and `IsActive`. Persistence metadata (`Id`, `CreatedAt`, `UpdatedAt`) is set by the repository on save.

### Repository CRUD

The sender repository extends `IRepository<Sender>`, providing `FindAsync`, `AddAsync`, `UpdateAsync`, `RemoveAsync`, plus sender-specific methods:

```csharp
public class SenderService
{
    private readonly ISenderRepository<Sender> _repository;

    public SenderService(ISenderRepository<Sender> repository) => _repository = repository;

    public async Task<IList<Sender>> GetAll()
        => (await _repository.FindAllAsync()).ToList();

    public Task<Sender?> FindByName(string name)
        => _repository.FindByNameAsync(name);

    public async Task<Sender> Create(string name, string address, EndpointType type)
    {
        var sender = new SenderBuilder()
            .WithName(name)
            .WithAddress(address)
            .WithEndpointType(type)
            .Build();

        await _repository.AddAsync(sender);
        return sender;
    }

    public async Task Update(Sender sender)
    {
        sender.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(sender);
    }

    public Task Delete(Sender sender)
        => _repository.RemoveAsync(sender);
}
```

### Seed data

```csharp
builder.Services.AddSenderInMemoryStore(new[]
{
    new Sender
    {
        Id = "seed-support",
        Name = "support",
        DisplayName = "Customer Support",
        Address = "support@example.com",
        EndpointType = EndpointType.EmailAddress,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    },

    new Sender
    {
        Id = "seed-sms-alerts",
        Name = "sms-alerts",
        DisplayName = "SMS Alert System",
        Address = "+15551234567",
        EndpointType = EndpointType.PhoneNumber,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    }
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
    .From(new EmailSender("noreply@example.com", name: "No Reply"))
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
                .WithEndpointType(EndpointType.EmailAddress))));
```

Each connector type can have its own sender configuration, stored in `ISenderConfigurationRegistry`. The factory creates a per-connector `SenderResolver` instance that uses the connector-specific cache and default sender settings.

## Resolution model internals

The sender resolution pipeline is built on an extensible abstract base class, with storage-specific resolvers providing the lookup implementation.

### Abstract base class

`SenderResolverBase` implements `ISenderResolver` and defines the common resolution flow. It accepts an `IEndpoint` and dispatches through two abstract methods that subclasses must implement:

```csharp
public abstract class SenderResolverBase : ISenderResolver
{
    protected abstract ValueTask<ISender?> FindSenderByNameAsync(
        string name, CancellationToken cancellationToken);

    protected abstract ValueTask<ISender?> FindSenderByEndpointAsync(
        string address, EndpointType endpointType, CancellationToken cancellationToken);

    public async ValueTask<ISender?> ResolveSenderAsync(
        IEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        // Dispatches to FindSenderByNameAsync for IUnresolvedSender,
        // or FindSenderByEndpointAsync for concrete ISender
    }
}
```

### Two resolution paths

| Input type | Resolution path | Example |
|---|---|---|
| `IUnresolvedSender` (`SenderRef`) | Lookup by logical name, with cache | `FromSender("support")` |
| `ISender` (inline) | Lookup by address + endpoint type, no cache | `From(new EmailSender(...))` |

For **named references**, the resolver:
1. Checks the cache — returns immediately if found and not expired
2. Defers to `FindSenderByNameAsync` (the storage-specific override)
3. Rejects inactive senders (`IsActive == false`)
4. Caches the resolved `ISender` for subsequent calls

For **inline senders** (concrete `EmailSender`, `PhoneSender`, etc.), the resolver looks up the registry by address and endpoint type. This validates that the inline sender matches a known active identity — useful for governance when senders must be pre-registered.

### Active state enforcement

Both resolution paths check `IsActive` on the resolved entity. An inactive sender is rejected regardless of how it was referenced, preventing messages from being sent through decommissioned identities.

### Structured logging

Resolution events are logged via source-generated `LoggerMessage` attributes:

| Event | Level | Trigger |
|---|---|---|
| `LogSenderResolvedFromCache` | Debug | Sender found in cache |
| `LogSenderNotFoundInRegistry` | Warning | Name lookup returned null |
| `LogNoSenderFoundForEndpoint` | Debug | Endpoint lookup returned null |
| `LogSenderFoundButInactive` | Warning | Sender exists but is inactive |

## Storage model

### Generic repository pattern

`ISenderRepository<TSender>` is generic over the sender entity type, extending Kista's `IRepository<TSender>`:

```csharp
public interface ISenderRepository<TSender> : IRepository<TSender>
    where TSender : class, ISender
{
    Task<TSender?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<TSender?> FindByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken = default);
    Task<IList<TSender>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task SetActiveAsync(string id, bool isActive, CancellationToken cancellationToken = default);
}
```

Sender-specific operations (`FindByNameAsync`, `FindByEndpointAsync`, `GetAllActiveAsync`, `SetActiveAsync`) sit alongside the standard CRUD inherited from `IRepository<TSender>`.

### SenderManager — generic manager implementation

`SenderManager<TSender>` implements `ISenderRepository<TSender>` by delegating to an underlying `IRepository<TSender>`. It provides the standard entity management layer (validation, caching, system time) via Kista's `EntityManager<TSender>` base class.

The sender-specific query methods require the underlying repository to implement `IQueryableRepository<TSender>`:

```csharp
public virtual async Task<TSender?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
{
    return AsQueryable().FirstOrDefault(s => s.Name == name);
}

public virtual async Task<TSender?> FindByEndpointAsync(string address, EndpointType endpointType, CancellationToken cancellationToken = default)
{
    return AsQueryable().FirstOrDefault(s => s.Address == address && s.Type == endpointType);
}

public virtual async Task SetActiveAsync(string id, bool isActive, CancellationToken cancellationToken = default)
{
    var sender = await FindAsync(id, cancellationToken);
    sender.IsActive = isActive;
    await UpdateAsync(sender, cancellationToken);
}
```

This generic design lets different storage backends use their own entity types (e.g., `SenderEntity`, `DbSender`) as long as they implement `ISender`.

### SenderEntity — the default storage entity

`SenderEntity` implements `ISender` directly and is the default entity used by the in-memory store:

```csharp
public class SenderEntity : ISender
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Address { get; set; }
    public EndpointType Type { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Storage backends

Each storage backend provides its own resolver and repository, wired through `WithCustomStore`:

**In-memory** (`Ratatosk.Senders.InMemory`):
- `InMemorySenderStore` — in-memory repository backed by a concurrent dictionary
- `InMemorySenderResolver` — resolves against `ISenderRepository<SenderEntity>`
- Automatically seeds the default sender from `SenderConnectorOptions`

**Entity Framework** (`Ratatosk.Senders.EntityFramework`):
- `SenderDbContext` — EF Core context with `DbSet<DbSender>`
- `EfSenderRepository` — queries via LINQ against the context
- `EfSenderResolver` — resolves against `ISenderRepository<DbSender>`

## Management aspects

### Per-connector configuration via SenderRegistrationBuilder

`SenderRegistrationBuilder<TConnector>` provides a fluent API for configuring sender resolution per connector type, chaining back to the parent connector builder:

```csharp
builder.Services.AddMessaging()
    .AddClient()
    .AddSenders()
    .AddSendGridEmail("sendgrid", cfg => cfg
        .WithSettings("SendGrid")
        .WithSenders(s => s
            .WithDefault(d => d
                .WithName("billing")
                .WithAddress("billing@example.com")
                .WithEndpointType(EndpointType.EmailAddress))
            .WithCache(new InMemorySenderCache(TimeSpan.FromMinutes(10)))
            .WithCustomStore(services =>
            {
                // Register a custom resolver/repository for this connector
            })));
```

The builder supports:

| Method | Purpose |
|---|---|
| `WithDefault(Action<SenderBuilder>)` | Sets the fallback sender when the message has none |
| `WithCache(ISenderCache)` | Replaces the default cache for this connector |
| `WithCacheTtl(TimeSpan)` | Overrides the default cache TTL |
| `WithCustomStore(Action<IServiceCollection>)` | Registers connector-specific storage and resolver |

When `WithCustomStore` is omitted, the resolver falls back to the global `ISenderRepository<ISender>` registered via `AddSenders()`. When `WithCustomStore` is used, a connector-specific `ISenderResolver` is registered as well.

### Validation

`ISenderValidator<TSender>` extends Kista's `IEntityValidator<TSender>` to validate sender entities before persistence. The default `SenderValidator<TSender>` checks:

- `Name` is not null or whitespace
- `DisplayName` is not null or whitespace
- `EndpointType` is not `Any`
- `Address` is not null or whitespace

```csharp
services.TryAddScoped<ISenderValidator<SenderEntity>, SenderValidator<SenderEntity>>();
```

Custom validators can be registered per sender type to enforce domain-specific rules (e.g., email format validation, phone number normalization).

### Error codes

`SenderErrorCodes` provides standardized error identifiers for sender operations:

| Constant | Value | Usage |
|---|---|---|
| `ErrorDomain` | `"sender"` | Error domain prefix |
| `SenderNotFound` | `"SENDER_NOT_FOUND"` | Lookup by ID or name failed |
| `SenderNotCreated` | `"SENDER_NOT_CREATED"` | Add operation failed |
| `SenderNotUpdated` | `"SENDER_NOT_UPDATED"` | Update operation failed |
| `SenderNotDeleted` | `"SENDER_NOT_DELETED"` | Remove operation failed |
| `QueryNotSupported` | `"QUERY_NOT_SUPPORTED"` | Underlying store does not support queryable operations |
| `SenderError` | `"SENDER_ERROR"` | General sender error |

### Registration

```csharp
// Minimal registration (resolver uses global repository):
services.AddSenders();

// With custom storage and validator:
services.AddSenders();
services.AddSenderRepository<MySenderRepository, MySender>();
services.TryAddScoped<ISenderValidator<MySender>, MySenderValidator>();
```

### Management API surface

The management interface `ISenderRepository<TSender>` exposes:

```csharp
// Standard CRUD (from IRepository<TSender>):
await repository.AddAsync(sender);
await repository.UpdateAsync(sender);
await repository.RemoveAsync(sender);
var found = await repository.FindAsync(key);

// Sender-specific:
var byName = await repository.FindByNameAsync("support");
var byEndpoint = await repository.FindByEndpointAsync("admin@example.com", EndpointType.EmailAddress);
var active = await repository.GetAllActiveAsync();
await repository.SetActiveAsync("sender-id", isActive: false);
```

## End-to-end summary

1. **Register** — `services.AddSenders()` + storage backend
2. **Manage** — create/update/delete senders via `ISenderRepository<Sender>`
3. **Reference** — build messages with `MessageBuilder.FromSender(name)`
4. **Send** — the connector resolves the name, validates, and dispatches
