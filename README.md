[![NuGet](https://img.shields.io/nuget/v/Ratatosk.Abstractions.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Abstractions/)
[![codecov](https://codecov.io/gh/deveel/deveel.messaging/graph/badge.svg)](https://codecov.io/gh/deveel/deveel.messaging)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4)](https://dotnet.microsoft.com/)
[![Documentation](https://img.shields.io/badge/docs-available-blue)](https://messaging.deveel.org)

<p align="center">
  <img src="ratatosk-full-logo.png" alt="Ratatosk" width="400"/>
</p>

# Ratatosk

> **Note**
> The project was originally named **Deveel Messaging**: on the day _25.05.2026_ it has been rebranded as **Ratatosk**, after the mythological squirrel that runs up and down Yggdrasil, the World Tree, carrying messages between the eagle at the top and the dragon at the roots — a fitting symbol for a messaging framework.

Ratatosk is a .NET framework that gives you one consistent way to work with SMS, email, push notifications, and chat channels.

Instead of coding directly against each provider SDK, you build an `IMessage`, send it through an `IChannelConnector`, and handle a predictable `OperationResult<T>`. That keeps your app code stable even if providers change.

The repository currently targets `.NET 8`, `.NET 9`, and `.NET 10`.  
Current stable release: **v1.0.1** (pre-release packages available from `main` builds).

## Features

- **Unified message model** — `IMessage` with fluent `MessageBuilder`, typed endpoints, and 8 content types (text, HTML, media, binary, JSON, location, template, multipart)
- **Schema-driven validation** — every connector declares capabilities, parameters, and constraints via `IChannelSchema`; catch invalid messages before they reach the provider
- **Pluggable authentication** — API key, token, basic auth, OAuth 2.0, Firebase service account, or custom providers
- **DI-first design** — `AddMessaging()` + `AddConnector<T>()` integration with `Microsoft.Extensions.DependencyInjection`
- **Standardized results** — all operations return `OperationResult<T>` with success/failure semantics
- **Schema derivation** — derive restricted schemas from a master for feature tiers or environment-specific constraints
- **IMessagingClient facade** — disposable high-level client with lazy initialization and named channel routing
- **Extensible** — implement `ChannelConnectorBase` to add any provider; built-in logging scopes, state management, and error wrapping
- **5 ready-made connectors** — Twilio (SMS/WhatsApp), SendGrid (email), Firebase (push), Facebook Messenger, Telegram Bot
- **Sender identity management** — registry-backed senders with resolution, caching, and per-channel selector strategies

## What this project does not do

- Queueing and scheduling
- Durable storage for audit/history
- Workflow orchestration

Those are application-level concerns, so you can choose your own architecture.

## Packages

| Package | Description | NuGet |
|---|---|---|
| `Ratatosk.Abstractions` | Message model with fluent builder, typed endpoints, 8 content types, properties, batch support. Zero external dependencies. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.Abstractions.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Abstractions/) |
| `Ratatosk` | DI registration (`AddMessaging`), `IMessagingClient` facade (disposable), connector factory, service collection extensions. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk/) |
| `Ratatosk.Connector.Abstractions` | Connector contracts, schemas, authentication, validation, result types. Reference for custom connector libraries. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.Connector.Abstractions.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Connector.Abstractions/) |
| `Ratatosk.Connectors` | Abstract connector base (`ChannelConnectorBase`) with state management and error wrapping, `ChannelSchema` builder, schema registry, auth manager, connector builder API. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.Connectors.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Connectors/) |
| `Ratatosk.Twilio` | Twilio SMS, MMS, WhatsApp messaging with status callbacks and template support. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.Twilio.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Twilio/) |
| `Ratatosk.Sendgrid` | SendGrid transactional and bulk email with HTML, multipart, templates, attachments, and event webhook processing. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.Sendgrid.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Sendgrid/) |
| `Ratatosk.Firebase` | Firebase Cloud Messaging push for device tokens and topics, with batch sends and dry-run mode. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.Firebase.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Firebase/) |
| `Ratatosk.Facebook` | Facebook Messenger Page-based messaging with text, media, quick replies, and webhook inbound. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.Facebook.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Facebook/) |
| `Ratatosk.Telegram` | Telegram bot messaging with rich text, media, locations, and webhook-based updates. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.Telegram.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Telegram/) |
| `Ratatosk.Senders` | Sender identity infrastructure: `ISenderRepository<TSender>`, `ISenderResolver`, cache, per-connector configuration, `MessageBuilder.FromSender()` extension. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.Senders.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Senders/) |
| `Ratatosk.Senders.InMemory` | In-memory `IRepository<Sender>` for development and testing. | [![NuGet](https://img.shields.io/nuget/v/Ratatosk.Senders.InMemory.svg?label=NuGet)](https://www.nuget.org/packages/Ratatosk.Senders.InMemory/) |

## Quick example

```csharp
// Register in DI (Startup / Program)
services.AddMessaging()
    .AddTwilioSmsConnector(options => {
        options.AccountSid = "...";
        options.AuthToken = "...";
    });

// Resolve and send
public class NotificationService(IMessagingClient messaging) 
{
    public async Task SendAsync(CancellationToken ct)
    {
        var message = new MessageBuilder()
            .WithId("order-confirm-123")
            .FromPhone("+15550001111")
            .ToPhone("+15550002222")
            .WithText("Your order has been confirmed!")
            .Build();

        var result = await messaging.SendMessageAsync(message, ct);

        if (!result.IsSuccess())
        {
            throw new InvalidOperationException(
                result.Error?.Message ?? "Failed to send message");
        }
    }
}
```

## Documentation

If you want to go deeper, the documentation provides practical, step-by-step guidance for designing channels, wiring connectors, validating messages, and operating the framework in real projects.

Start from the docs home and follow the path that best matches what you are building: [docs/README.md](docs/README.md).

### Suggested reading paths

- **First integration** - Start with the framework concepts, then wire a minimal implementation, and finally pick a connector guide for your channel.
  ([Framework overview](docs/framework-overview.md) -> [Quickstart](docs/quickstart.md) -> [Connector index](docs/connectors/README.md))
- **Custom connector authoring** - Learn schema design first, then implement connector behaviour, then register and resolve channels at runtime.
  ([Channel schema](docs/channel-schema.md) -> [Connector implementation](docs/connector-implementation.md) -> [Installation](docs/installation.md))
- **Validation-first integration** - Model endpoints, apply validation rules, and extend validation when channel-specific constraints grow.
  ([Message model](docs/messaging-model.md) -> [Message validation](docs/message-validation.md) -> [Channel schema](docs/channel-schema.md))
- **Sender identity management** - Set up sender registries, resolution, and caching for decoupled sender configuration.
  ([Sender management](docs/sender-management.md) -> [Message model](docs/messaging-model.md#sender-identities) -> [Sender Manager sample](docs/samples/sender-manager.md))


## Roadmap

The project roadmap is tracked in detail in [ROADMAP.md](ROADMAP.md). The summary below highlights the upcoming milestones and the main features planned for each release.

### v0.4.0 - Framework Foundations

Strengthens quality gates, release automation, observability, and docs completeness across the existing connector ecosystem.

- [x] **Test coverage target (>= 80%)** - Enforce minimum line coverage per library in CI.
- [x] **CI/CD pipeline hardening** - Automate build, test, compatibility checks, signing, and NuGet publishing from release tags.
- [x] **Structured logging improvements** - Standardize event IDs, scopes, and `LoggerMessage` patterns across connectors.
- [x] **Documentation completeness** - Complete XML API docs and connector guides with consistent coverage.

### v0.5.0 - Inbound Messaging

Completes the receive-side model for current connectors to support bidirectional messaging scenarios.

- [x] **Twilio inbound messages (SMS/WhatsApp)** - Parse inbound payloads into framework messages and endpoints.
- [x] **SendGrid inbound parse support** - Map multipart inbound emails, content, and attachments into the message model.
- [x] **Firebase inbound messages** - Support upstream device data messages through the same receive abstractions.

### v1.0.0 - First Stable Release

Locks the public API and ships stable package releases with production-ready guarantees.

- [x] **API freeze and compatibility enforcement** - Prevent breaking API changes without a major version bump.
- [x] **NuGet GA release** - Publish stable `Ratatosk.*` packages without prerelease suffixes.
- [x] **Interactive content model** - Add cross-channel abstractions for buttons, quick replies, carousels, and lists.
- [ ] **Sender identity model** - Provide typed sender identities for phone, email, and bot-based channels.

### v1.1.0 and beyond - Platform Expansion

Extends resilience, observability, connectors, protocol support, and higher-level messaging capabilities.

- [ ] **Resilience and observability** - Retry/circuit-breaker policies, OpenTelemetry signals, health checks, and timeout controls.
- [ ] **New SaaS connectors** - Add Slack, Microsoft Teams, WhatsApp Business API, Viber, and LINE connectors.
- [ ] **Protocol connectors** - Add SMPP, SMTP, RCS, and direct APNs support with shared protocol base classes.
- [ ] **Content adaptation and validation** - Introduce transcoding, channel-aware fallback, segmentation, and address validation.
- [ ] **Tooling and diagnostics** - Ship connector scaffolding templates and ASP.NET Core diagnostic middleware.
- [ ] **Conversations and templates (v2.x)** - Add conversation state/correlation and provider-agnostic template modeling.

For milestone-level detail, rationale, and dependencies, see [ROADMAP.md](ROADMAP.md).

## Versioning and Releases

The project uses a simple GitHub Flow-based release model:

- `main` is the only long-lived branch
- merges to `main` produce preview packages with an `-alpha.N` suffix
- stable packages are produced only from `vX.Y.Z` tags on commits already in `main`

Package versions are calculated automatically with GitVersion, so versions should not be edited manually in project files.

## Contributing

Contributions are welcome and appreciated.

If you want to contribute:

- Open an issue to discuss bugs, features, or design changes
- Check the existing documentation and package boundaries before proposing API changes
- Submit focused pull requests with tests when behavior changes

The contribution workflow follows GitHub Flow: create a short-lived branch from `main`, open a pull request, and let CI produce preview packages from `main` after merge.

For local setup, coding conventions, and the contribution workflow, read [CONTRIBUTING.md](CONTRIBUTING.md).

## License

This project is released under the MIT License.

You can use it in personal, academic, and commercial projects, including closed-source applications, provided that the license notice is preserved.

See [LICENSE](LICENSE) for the full text and terms.

## Contributors

[![Contributors](https://contrib.rocks/image?repo=deveel/deveel.messaging)](https://github.com/deveel/deveel.messaging/graphs/contributors)
