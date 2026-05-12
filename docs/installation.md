# Installation

The framework is split into layers so you depend only on what you use. The model layer has zero external dependencies; the connector infrastructure depends only on Microsoft.Extensions abstractions; provider connectors bring in their respective SDKs.

Choosing packages by layer:
- **Model only** (library projects that define messages but never send them): `Abstractions`
- **Host application** (sends or receives messages): `Abstractions` + `Connectors` + the connector packages for your providers
- **Custom connector authoring** (building a new provider integration): `Connector.Abstractions` + `Connectors`

## Package selection

Install only what your application needs:

```bash
# Core message model — needed by every project that sends/receives messages
dotnet add package Deveel.Messaging.Abstractions

# DI integration + base connector infrastructure — needed by every host application
dotnet add package Deveel.Messaging.Connectors

# Provider-specific connectors — install what you actually use
dotnet add package Deveel.Messaging.Connector.Twilio
dotnet add package Deveel.Messaging.Connector.Sendgrid
dotnet add package Deveel.Messaging.Connector.Firebase
dotnote add package Deveel.Messaging.Connector.Facebook
dotnet add package Deveel.Messaging.Connector.Telegram

# Custom connector authoring — needed only if you build your own connector
dotnet add package Deveel.Messaging.Connector.Abstractions
```

The `Abstractions` and `Connector.Abstractions` packages have no external dependencies beyond the .NET BCL. `Connectors` depends on `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`.

## Framework targets

All packages target .NET 8, .NET 9, and .NET 10.

## Basic DI registration

Once packages are installed, the next step is wiring them into the application's dependency injection container. The entry point is `AddMessaging()`, which registers the infrastructure services (schema registry, authentication manager) and returns a builder for registering connectors.

Register messaging services and one or more connectors in your startup code:

```csharp
using Deveel.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMessaging()
    .AddConnector<TwilioSmsConnector>()
    .AddConnector<SendGridEmailConnector>();
```

`AddMessaging()` returns a `MessagingBuilder` instance. Each `AddConnector<T>()` call registers the connector as a singleton in DI and also registers the connector's schema in the `IChannelSchemaRegistry`. Connectors are resolved as `IChannelConnector` from DI.

### What AddMessaging registers

- `IChannelSchemaRegistry` — singleton, aggregated view of all connector schemas
- `IAuthenticationManager` — singleton, manages authentication providers and credential caching
- Per connector: the connector type as singleton, plus `IChannelConnector` forwarding to the same instance

## Named connectors (keyed services)

A single application may need multiple instances of the same connector type with different settings — for example, a primary and fallback Twilio account, or per-tenant connector instances. Named connectors solve this by registering each instance under a distinct key.

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
