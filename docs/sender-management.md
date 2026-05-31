# Sender Management

The sender identity system solves a common problem: **message composition should not depend on sender configuration**. When you write `new MessageBuilder().FromEmail("admin@example.com")`, you hardcode the sender address into the message — changing it requires a code deploy. The sender identity system lifts sender details out of message code and into a managed registry, where they can be created, updated, and activated independently of the application.

## Concepts

### Sender vs endpoint

A sender is an `IEndpoint` that also carries identity semantics — it represents *who* the message is from, not just an address. The framework distinguishes between:

- **Inline senders** — concrete sender details used directly on the message (`EmailSender`, `PhoneSender`, etc.). These may still be resolved against the registry to validate they match a known active identity.
- **Unresolved references** — a logical name (`SenderRef`) that the connector resolves at send time. The actual sender (address, display name, endpoint type) is looked up from the registry. `SenderRef` implements the `IUnresolvedSender` marker interface to signal that resolution is required.
- **Plain endpoints** — an `Endpoint` with a type and address. These are not resolved; they pass through as-is.

This separation lets you build and dispatch messages with a logical name like `"support"`, while the concrete "support@example.com" endpoint lives in the registry — changeable at runtime without touching message code.

### The resolution pipeline

When a message is sent, the `MessagingClient` builds a `SenderResolutionContext` and passes it to the registered `ISenderResolver`:

```
message.Sender + ConnectionSettings
  → SenderResolutionContext
  → ISenderResolver.ResolveAsync(context)
     → if IUnresolvedSender: cache check → repository lookup by name → cache result
     → if ISender: cache check → repository lookup by address+type → cache result
     → fallback: ConnectionSettings.GetDefaultSender()
  → resolved ISender replaces message.Sender
  → validate against schema
  → send
```

Resolution is automatic — the connector handles it. The API or service that builds the message never needs to query the registry.

### Repository as the source of truth

The repository persists sender entities (name, address, endpoint type, active flag). It decouples *who can send* from *how messages are built* — operations teams manage senders through CRUD, while developers reference them by name.

The repository interface `ISenderRepository<TSender>` extends Kista's `IRepository<TSender>`, inheriting all standard CRUD, pagination, and query capabilities. Custom implementations can use their own storage-bound entity types (e.g., `SenderEntity`, `DbSender`) that implement `ISender`.

### Cache for performance

Resolution results are cached with a configurable TTL (default 5 minutes). Each sender is indexed under two keys — by name and by endpoint (type + address) — so lookups are fast regardless of whether resolution started from a `SenderRef` or an inline sender. The default implementation uses `IDistributedCache` via `DistributedSenderCache`.

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

### With the messaging builder (recommended)

Sender services are registered via `SenderServiceBuilder`, which is returned by `AddSenders<TSender>()` on the `MessagingBuilder`. Storage backends provide extension methods on the builder:

```csharp
using Ratatosk.Senders;

// Registers cache, sender manager, validator, resolver,
// and configures the in-memory store — all in one fluent chain
builder.Services.AddMessaging()
    .AddClient()
    .AddSenders<SenderEntity>(cfg => cfg
        .UseInMemoryStore(seedSenders))
    .AddSendGridEmail("sendgrid", c => c.WithSettings("SendGrid"));
```

For advanced scenarios, `AddSenders<TSender>()` returns the builder directly:

```csharp
var senders = builder.Services.AddMessaging()
    .AddSenders<SenderEntity>();

senders.UseInMemoryStore(seedSenders);
senders.ConfigureCacheOptions(o => o.DefaultTtl = TimeSpan.FromMinutes(10));
```

### Standalone (no messaging)

Without a connector, the `SenderServiceBuilder` is accessible through the `MessagingBuilder`:

```csharp
using Ratatosk.Senders;

var senderBuilder = builder.Services.AddMessaging()
    .AddSenders<SenderEntity>();

senderBuilder.UseInMemoryStore();
```

### Entity Framework

```csharp
using Ratatosk.Senders;

builder.Services.AddMessaging()
    .AddClient()
    .AddSenders<DbSender>(cfg => cfg
        .UseEntityFramework(options =>
            options.UseSqlServer(connectionString)))
    .AddSendGridEmail("sendgrid", c => c.WithSettings("SendGrid"));
```

The `UseEntityFramework` extension registers `SenderDbContext`, `EntitySenderRepository`, and required logging services.

### In-memory (development/testing)

```csharp
using Ratatosk.Senders;

builder.Services.AddMessaging()
    .AddSenders<SenderEntity>(cfg => cfg
        .UseInMemoryStore(seedSenders));
```

`UseInMemoryStore` optionally accepts seed sender entities. Without seed data, the store starts empty.

### Custom storage

Register your own `ISenderRepository<TSender>` after calling `AddSenders<TSender>()`:

```csharp
using Ratatosk.Senders;

builder.Services.AddMessaging()
    .AddSenders<MySender>(cfg => cfg
        .ConfigureCacheOptions(o => o.DefaultTtl = TimeSpan.FromMinutes(10)));

builder.Services.AddScoped<ISenderRepository<MySender>, MySenderRepository>();
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
    .AsActive()
    .Build();

await repository.AddAsync(sender);
```

Each sender carries: `Name` (logical name used in `SenderRef`), `DisplayName`, `Address`, `EndpointType`, and `IsActive`. Persistence metadata (`Id`, `CreatedAt`, `UpdatedAt`) is set by the repository on save.

### Repository CRUD

The sender repository extends `IRepository<TSender>`, providing `FindAsync`, `AddAsync`, `UpdateAsync`, `RemoveAsync`, plus sender-specific methods:

```csharp
public class SenderService
{
    private readonly ISenderRepository<SenderEntity> _repository;

    public SenderService(ISenderRepository<SenderEntity> repository) => _repository = repository;

    public async Task<IList<SenderEntity>> GetAll()
        => (await _repository.FindAllAsync()).ToList();

    public Task<SenderEntity?> FindByName(string name)
        => _repository.FindByNameAsync(name);

    public async Task<SenderEntity> Create(string name, string address, EndpointType type)
    {
        var sender = new SenderBuilder()
            .WithName(name)
            .WithAddress(address)
            .WithEndpointType(type)
            .Build();

        await _repository.AddAsync(sender);
        return sender;
    }

    public async Task Update(SenderEntity sender)
    {
        sender.Update(displayName: "New Display Name");
        await _repository.UpdateAsync(sender);
    }

    public Task Delete(SenderEntity sender)
        => _repository.RemoveAsync(sender);
}
```

### Seed data

```csharp
var seedSenders = new[]
{
    new SenderEntity
    {
        Id = "seed-support",
        Name = "support",
        DisplayName = "Customer Support",
        Address = "support@example.com",
        Type = EndpointType.EmailAddress
    },

    new SenderEntity
    {
        Id = "seed-sms-alerts",
        Name = "sms-alerts",
        DisplayName = "SMS Alert System",
        Address = "+15551234567",
        Type = EndpointType.PhoneNumber
    }
};

seedSenders[0].Activate();
seedSenders[1].Activate();
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

## Default sender fallback

When a message has no sender set, the resolver falls back to the default sender configured in the connection settings. `ConnectionSettings.GetDefaultSender()` checks for:

1. Explicit parameters: `DefaultSenderName`, `DefaultSenderAddress`, `DefaultSenderType`
2. Channel-native `From` parameter (e.g., Twilio phone number, SendGrid email address)

```csharp
// Via connection string:
var settings = ConnectionSettings.Parse("DefaultSenderName=billing;DefaultSenderAddress=billing@example.com;DefaultSenderType=EmailAddress;ApiKey=...");

// Or via schema parameter:
schema.AddParameter("DefaultSenderName", DataType.String, isRequired: false);
schema.AddParameter("From", DataType.String, isRequired: false);
```

## Configuring resolution

### Cache TTL

Resolution results expire after the configured TTL. The default is 5 minutes:

```csharp
// Via the builder
builder.Services.AddMessaging()
    .AddSenders<SenderEntity>(cfg => cfg
        .ConfigureCacheOptions(o => o.DefaultTtl = TimeSpan.FromMinutes(15)));
```

### Custom cache implementation

Replace the default `DistributedSenderCache` with a custom `ISenderCache`:

```csharp
builder.Services.AddMessaging()
    .AddSenders<SenderEntity>(cfg => cfg
        .WithCache<MyCustomSenderCache>());
```

Or register a custom cache on the service collection before `AddSenders` to override the default (the builder uses `TryAddSingleton`):

```csharp
builder.Services.AddSingleton<ISenderCache, MyCustomSenderCache>();
builder.Services.AddMessaging()
    .AddSenders<SenderEntity>();
```

## Resolution model internals

The sender resolution pipeline is built on an extensible abstract base class, with storage-specific resolvers providing the lookup implementation.

### Resolution context

`SenderResolutionContext` carries all information needed to resolve a sender at send time:

```csharp
public class SenderResolutionContext
{
    public IEndpoint? Sender { get; }         // From the message
    public ConnectionSettings Settings { get; }  // For default sender fallback
    public string? TenantId { get; }          // Optional multi-tenant identifier
}
```

### Abstract base class

`SenderResolverBase` implements `ISenderResolver` and defines the common resolution flow. It accepts a `SenderResolutionContext` and dispatches through two abstract methods that subclasses must implement:

```csharp
public abstract class SenderResolverBase : ISenderResolver
{
    protected abstract ValueTask<ISender?> ResolveByNameAsync(
        string name, CancellationToken cancellationToken);

    protected abstract ValueTask<ISender?> ResolveByEndpointAsync(
        string address, EndpointType endpointType, CancellationToken cancellationToken);

    public async ValueTask<ISender?> ResolveAsync(
        SenderResolutionContext context, CancellationToken cancellationToken = default)
    {
        // 1. If context.Sender is IUnresolvedSender → ResolveByNameAsyncCached
        // 2. If context.Sender is ISender → ResolveBySenderAsync
        // 3. Fallback → context.Settings.GetDefaultSender()
    }
}
```

### Two resolution paths

| Input type | Resolution path | Example |
|---|---|---|
| `IUnresolvedSender` (`SenderRef`) | Lookup by logical name, with cache | `FromSender("support")` |
| `ISender` (inline) | Lookup by address + endpoint type, with cache | `From(new EmailSender(...))` |
| `null` | Fallback to `ConnectionSettings.GetDefaultSender()` | No sender on message |

For **named references**, the resolver:
1. Checks the cache by name — returns immediately if found and not expired
2. Defers to `ResolveByNameAsync` (the storage-specific override)
3. Rejects inactive senders (`IsActive == false`)
4. Caches the resolved `ISender` under both name and endpoint keys

For **inline senders**, the resolver:
1. Checks the cache by endpoint (address + type) — returns immediately if found
2. Defers to `ResolveByEndpointAsync` (the storage-specific override)
3. Rejects inactive senders
4. Caches the resolved `ISender` under both name and endpoint keys

### Active state enforcement

Both resolution paths check `IsActive` on the resolved entity. An inactive sender is rejected regardless of how it was referenced, preventing messages from being sent through decommissioned identities.

### Structured logging

Resolution events are logged via source-generated `LoggerMessage` attributes with stable event IDs:

| Event ID | Event | Level | Trigger |
|---|---|---|---|
| 7001 | `LogSenderResolvedFromCache` | Debug | Sender found in cache by name |
| 7002 | `LogSenderNotFoundInRegistry` | Warning | Name lookup returned null |
| 7003 | `LogNoSenderFoundForEndpoint` | Debug | Endpoint lookup returned null |
| 7004 | `LogSenderFoundButInactive` | Warning | Sender exists but is inactive |
| 7005 | `LogFailedToFindSenderByName` | Error | Exception during name lookup |
| 7006 | `LogFailedToFindSenderByEndpoint` | Error | Exception during endpoint lookup |
| 7007 | `LogSenderResolvedFromCacheByEndpoint` | Debug | Sender found in cache by endpoint |
| 7008 | `LogFailedToRetrieveAllActiveSenders` | Error | Exception retrieving active senders |
| 7009 | `LogFailedToSetActiveState` | Error | Exception setting active state |

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
    Task SetActiveAsync(TSender sender, bool isActive, CancellationToken cancellationToken = default);
}
```

Sender-specific operations (`FindByNameAsync`, `FindByEndpointAsync`, `GetAllActiveAsync`, `SetActiveAsync`) sit alongside the standard CRUD inherited from `IRepository<TSender>`.

### SenderManager — generic manager implementation

`SenderManager<TSender>` extends Kista's `EntityManager<TSender>` and wraps an `IRepository<TSender>` to provide sender-specific management with validation, caching, and system-time support. All operations return `OperationResult<T>` for consistent error handling:

```csharp
public class SenderManager<TSender> : EntityManager<TSender>
    where TSender : class, ISender
{
    public virtual async Task<OperationResult<TSender>> FindByNameAsync(string name);
    public virtual async Task<OperationResult<TSender>> FindByEndpointAsync(string address, EndpointType endpointType);
    public virtual async Task<OperationResult<IList<TSender>>> GetAllActiveAsync();
    public virtual async Task<OperationResult> ActivateAsync(string id);
    public virtual async Task<OperationResult> DeactivateAsync(string id);
}
```

The sender-specific query methods require the underlying repository to implement `ISenderRepository<TSender>`:

```csharp
// FindByNameAsync delegates to the repository:
var sender = await SenderRepository.FindByNameAsync(name, CancellationToken);
return sender is null
    ? OperationResult<TSender>.Fail(SenderErrorCodes.SenderNotFound, ...)
    : OperationResult<TSender>.Success(sender);
```

This generic design lets different storage backends use their own entity types (e.g., `SenderEntity`, `DbSender`) as long as they implement `ISender`.

### SenderEntity — the default storage entity

`SenderEntity` implements `ISender` directly and is the default entity used by the in-memory store:

```csharp
public class SenderEntity : ISender
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Address { get; set; }
    public EndpointType Type { get; set; }
    public bool IsActive { get; private set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    EndpointType IEndpoint.Type => Type;

    public void Activate()    // Sets IsActive = true, updates UpdatedAt
    public void Deactivate()  // Sets IsActive = false, updates UpdatedAt
    public void Update(string? displayName = null, string? address = null, EndpointType? type = null)
}
```

### Storage backends

Each storage backend provides its own repository and resolver, wired through the registration builder:

**In-memory** (`Ratatosk.Senders.InMemory`):
- `InMemorySenderRepository` — in-memory repository backed by a concurrent dictionary, implementing `ISenderRepository<SenderEntity>`
- `SenderResolver<SenderEntity>` — resolves against `ISenderRepository<SenderEntity>`
- Accepts seed senders via constructor injection

**Entity Framework** (`Ratatosk.Senders.EntityFramework`):
- `SenderDbContext` — EF Core context with `DbSet<DbSender>`
- `EntitySenderRepository` — queries via LINQ against the context, implementing `ISenderRepository<DbSender>`
- `DbSender` — EF entity with `string Type` property and `EndpointType` computed property
- `SenderResolver<DbSender>` — resolves against `ISenderRepository<DbSender>`

## Management aspects

### Per-connector cache configuration

Sender cache can be configured globally. `WithSenders()` is no longer available on connector builders — sender configuration is not per-connector.

### Validation

`ISenderValidator<TSender>` extends Kista's `IEntityValidator<TSender>` to validate sender entities before persistence. The default `SenderValidator<TSender>` checks:

- `Name` is not null or whitespace
- `DisplayName` is not null or whitespace
- `Type` is not `EndpointType.Any`
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
// Minimal registration with in-memory store:
services.AddMessaging()
    .AddSenders<SenderEntity>(cfg => cfg
        .UseInMemoryStore());

// With custom storage and validator:
services.AddMessaging()
    .AddSenders<MySender>(cfg => cfg
        .ConfigureCacheOptions(o => o.DefaultTtl = TimeSpan.FromMinutes(10)));

services.AddScoped<ISenderRepository<MySender>, MySenderRepository>();
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
await repository.SetActiveAsync(sender, isActive: false);
```

### Distributed cache

`DistributedSenderCache` implements `ISenderCache` using `IDistributedCache` (backed by Redis, SQL Server, or any other distributed cache provider). Each sender is stored under two keys:

- `ratatosk:sender:name:{name}` — for name-based lookups
- `ratatosk:sender:endpoint:{type}:{address}` — for endpoint-based lookups

`SetAsync` writes to both keys simultaneously with the configured TTL. Serialization uses `System.Text.Json` with `JsonSerializerDefaults.Web`.

```csharp
// Configure with Redis:
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

builder.Services.AddMessaging()
    .AddSenders<SenderEntity>(cfg => cfg
        .ConfigureCacheOptions(o => o.DefaultTtl = TimeSpan.FromMinutes(10)));
// DistributedSenderCache is registered automatically by AddSenders<TSender>()
```

## End-to-end summary

1. **Register** — `services.AddMessaging().AddSenders<TSender>(cfg => cfg.UseInMemoryStore(...))`
2. **Manage** — create/update/delete senders via `ISenderRepository<TSender>` or `SenderManager<TSender>`
3. **Reference** — build messages with `MessageBuilder.FromSender(name)`
4. **Send** — the client resolves the name via `SenderResolutionContext`, validates, and dispatches
