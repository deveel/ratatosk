# Installation

The framework is split into layers so you depend only on what you use. The model layer has zero external dependencies; the connector infrastructure depends only on Microsoft.Extensions abstractions; provider connectors bring in their respective SDKs.

Choosing packages by layer:
- **Model only** (library projects that define messages but never send them): `Ratatosk.Abstractions`
- **Host application** (sends or receives messages): `Ratatosk` + the connector packages for your providers
- **Custom connector authoring** (building a new provider integration): `Ratatosk.Connector.Abstractions` + `Ratatosk.Connectors`

## Package selection

Install only what your application needs:

```bash
# Core messaging platform — DI registration, IMessagingClient facade, MessageBuilder, connector factory
#  (pulls in Abstractions + Connectors transitively)
dotnet add package Ratatosk

# Provider-specific connectors — install what you actually use
dotnet add package Ratatosk.Twilio
dotnet add package Ratatosk.Sendgrid
dotnet add package Ratatosk.Firebase
dotnet add package Ratatosk.Facebook
dotnet add package Ratatosk.Telegram

# Model only (no DI, no connectors) — needed only if you define messages in a library
dotnet add package Ratatosk.Abstractions

# Custom connector authoring — needed only if you build your own connector
dotnet add package Ratatosk.Connector.Abstractions
dotnet add package Ratatosk.Connectors

# OpenTelemetry — convenience extensions for wiring Ratatosk telemetry sources
dotnet add package Ratatosk.Extensions.OpenTelemetry
```

`Ratatosk` depends on `Ratatosk.Abstractions` and `Ratatosk.Connectors` (which bring in `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`). The `Abstractions` and `Connector.Abstractions` packages have no external dependencies beyond the .NET BCL.

## Framework targets

All packages target .NET 8, .NET 9, and .NET 10.

## Basic DI registration

Once packages are installed, the next step is wiring them into the application's dependency injection container. The entry point is `AddMessaging()` (from the `Ratatosk` package), which registers the infrastructure services (schema registry, authentication manager, `MessageBuilder` support) and returns a builder for registering connectors.

Register messaging services and one or more connectors in your startup code:

```csharp
using Ratatosk;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMessaging()
    .AddConnector<TwilioSmsConnector>()
    .AddConnector<SendGridEmailConnector>();
```

`AddMessaging()` returns a `MessagingBuilder` instance. Each `AddConnector<T>()` call registers the connector as a singleton in DI and also registers the connector's schema in the `IChannelSchemaRegistry`. Connectors are resolved as `IChannelConnector` from DI.

Call `.AddClient()` on the builder to register the `IMessagingClient` facade, which handles lazy connector initialization and channel routing (see [Quickstart](quickstart.md#6-imessagingclient-facade)).

### What AddMessaging registers

- `IChannelSchemaRegistry` — singleton, aggregated view of all connector schemas
- `IAuthenticationManager` — singleton, manages authentication providers and credential caching
- `IChannelConnectorResolver` — singleton, resolves connectors by name from the DI container (registered by `AddClient`)
- `ConnectorTypeCatalog` — singleton, maps channel names to connector types for runtime resolution (registered by `AddClient` only when `AddConnectorType` is used)
- Per connector: the connector type as singleton, plus `IChannelConnector` forwarding to the same instance

## Registration strategies

The framework supports two registration strategies that can be mixed in the same application:

### Named connectors (keyed services)

Use named connectors when you need multiple instances of the same connector type with different settings — for example, a primary and fallback Twilio account.

Register multiple instances of the same connector type with different names and configurations:

```csharp
builder.Services
    .AddMessaging()
    .AddConnector<TwilioSmsConnector>("primary", cfg => cfg
        .WithSettings("Twilio:Primary"))
    .AddConnector<TwilioSmsConnector>("fallback", cfg => cfg
        .WithSettings("Twilio:Fallback"));
```

Named connectors are registered as keyed DI services. Resolve them with `[FromKeyedServices]`:

```csharp
public class NotificationService(
    [FromKeyedServices("primary")] IChannelConnector primary,
    [FromKeyedServices("fallback")] IChannelConnector fallback)
{
    public async Task SendAsync(IMessage message)
    {
        var result = await primary.SendMessageAsync(message, ct);
        if (result.IsFailure)
            result = await fallback.SendMessageAsync(message, ct);
    }
}
```

### Anonymous connectors (by type)

When you only need one instance of a connector type, register it without a name:

```csharp
builder.Services
    .AddMessaging()
    .AddConnector<TwilioSmsConnector>(cfg => cfg
        .WithSettings("Twilio"));
```

Anonymous connectors are resolved by type through the `IMessagingClient` generic overloads:

```csharp
await client.SendAsync<TwilioSmsConnector>(message, ct);
```

### Connector type registration (for runtime creation)

For scenarios where connection settings are loaded at runtime (not from configuration), register the connector type without providing settings:

```csharp
builder.Services
    .AddMessaging()
    .AddConnectorType<TwilioSmsConnector>("runtime-sms")
    .AddConnectorType<FacebookMessengerConnector>()
    .AddClient();
```

`AddConnectorType` accepts an optional name parameter. When omitted, the connector type name is used as the key. This registration:
- Registers the default `IChannelConnectorFactory<T>` for the connector type
- Adds the type to a `ConnectorTypeCatalog` used at runtime to resolve connector types by name

Runtime connectors are created on-demand using the `ConnectionSettings` provided at the call site:

```csharp
var runtimeSettings = new ConnectionSettings()
    .SetParameter("AccountSid", accountSid)
    .SetParameter("AuthToken", authToken);

await client.SendAsync("runtime-sms", runtimeSettings, message);
```

You can mix all three strategies in the same application:

```csharp
builder.Services
    .AddMessaging()
    .AddConnector<TwilioSmsConnector>("corporate", cfg => cfg
        .WithSettings("Twilio:Corporate"))
    .AddConnector<SendGridEmailConnector>(cfg => cfg
        .WithSettings("SendGrid"))
    .AddConnectorType<FacebookMessengerConnector>("runtime-fb")
    .AddClient();
```

## Configuration from appsettings.json

Hardcoding connection parameters works for prototypes but is not viable in production. The framework integrates with the .NET configuration stack: you store settings in `appsettings.json`, environment variables, or a vault, and the connector builder reads them via `WithSettings()`.

Store provider credentials and settings in configuration:

```json
{
  "Twilio": {
    "AccountSid": "AC...",
    "AuthToken": "...",
    "WebhookUrl": "https://myapp.example.com/twilio/status"
  },
  "SendGrid": {
    "ApiKey": "SG..."
  }
}
```

Wire them using the `WithSettings` method, which accepts a configuration section path:

```csharp
builder.Services
    .AddMessaging()
    .AddConnector<TwilioSmsConnector>(cfg => cfg
        .WithSettings("Twilio"))
    .AddConnector<SendGridEmailConnector>(cfg => cfg
        .WithSettings("SendGrid"));
```

### Inline settings

For hardcoded or programmatic settings, use `WithSetting`:

```csharp
cfg.WithSetting("AccountSid", "AC...")
   .WithSetting("AuthToken", "...");
```

### Connection string format

Some connectors accept a semicolon-delimited connection string:

```csharp
cfg.WithConnectionString("AccountSid=AC...;AuthToken=...");
```

## Complete startup example

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMessaging()
    .AddConnector<TwilioSmsConnector>(cfg => cfg
        .WithSettings("Twilio"))
    .AddConnector<SendGridEmailConnector>(cfg => cfg
        .WithSettings("SendGrid"))
    .AddConnector<FirebasePushConnector>(cfg => cfg
        .WithSetting("ProjectId", "my-project")
        .WithSetting("ServiceAccountKey", serviceAccountJson));

builder.Services.AddSingleton<NotificationService>();

var app = builder.Build();
app.Run();
```

## Verification

```bash
dotnet restore
dotnet build
```

No provider credentials are needed for a build-time check. Runtime initialization fails fast with clear error messages when settings are missing or invalid.

## Next

- [Quickstart](quickstart.md) — build a running example
- [Connector implementation](connector-implementation.md) — register custom connectors
- [Authentication](authentication.md) — configure authentication providers
