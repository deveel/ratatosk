[![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Abstractions.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging.Abstractions/)
[![codecov](https://codecov.io/gh/deveel/deveel.messaging/graph/badge.svg)](https://codecov.io/gh/deveel/deveel.messaging)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4)](https://dotnet.microsoft.com/)
[![Documentation](https://img.shields.io/badge/docs-available-blue)](https://messaging.deveel.org)

# Deveel Messaging

Deveel Messaging is a .NET framework that gives you one consistent way to work with SMS, email, push notifications, and chat channels.

Instead of coding directly against each provider SDK, you build an `IMessage`, send it through an `IChannelConnector`, and handle a predictable `OperationResult<T>`. That keeps your app code stable even if providers change.

The repository currently targets `.NET 8`, `.NET 9`, and `.NET 10`.

## What this project includes

- A shared message model (`IMessage`, endpoints, content types, properties)
- Connector contracts and base classes for provider implementations
- Channel schemas to declare capabilities and validate messages/settings
- DI helpers and a channel registry for runtime connector discovery
- Ready-to-use connectors for Twilio, SendGrid, Firebase, Facebook Messenger, and Telegram

## What this project does not do

- Queueing and scheduling
- Durable storage for audit/history
- Workflow orchestration or business retry policies

Those are application-level concerns, so you can choose your own architecture.

## Packages

| Package | Purpose | NuGet |
|---|---|---|
| `Deveel.Messaging.Abstractions` | Core message and endpoint model | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Abstractions.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging.Abstractions/) |
| `Deveel.Messaging` | DI registration, `IMessagingClient` facade, `MessageBuilder`, connector factory | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging/) |
| `Deveel.Messaging.Connector.Abstractions` | Connector contracts, schemas, auth, result types | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Abstractions.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Abstractions/) |
| `Deveel.Messaging.Connectors` | `ChannelConnectorBase`, `ChannelSchema`, auth manager, schema registry | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connectors.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging.Connectors/) |
| `Deveel.Messaging.Connector.Twilio` | Twilio SMS and WhatsApp connectors | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Twilio.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Twilio/) |
| `Deveel.Messaging.Connector.Sendgrid` | SendGrid email connector | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Sendgrid.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Sendgrid/) |
| `Deveel.Messaging.Connector.Firebase` | Firebase push connector | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Firebase.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Firebase/) |
| `Deveel.Messaging.Connector.Facebook` | Facebook Messenger connector | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Facebook.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Facebook/) |
| `Deveel.Messaging.Connector.Telegram` | Telegram Bot connector | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Telegram.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Telegram/) |

## Quick example

```csharp
var message = new MessageBuilder()
    .WithId("order-confirm-123")
    .FromPhone("+15550001111")
    .ToPhone("+15550002222")
    .WithText("Hello from Deveel Messaging")
    .Build();

var result = await connector.SendMessageAsync(message, ct);

if (!result.IsSuccess())
{
    throw new InvalidOperationException(result.Error?.Message ?? "Failed to send message");
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

- [ ] **Twilio inbound messages (SMS/WhatsApp)** - Parse inbound payloads into framework messages and endpoints.
- [ ] **SendGrid inbound parse support** - Map multipart inbound emails, content, and attachments into the message model.
- [ ] **Firebase inbound messages** - Support upstream device data messages through the same receive abstractions.

### v1.0.0 - First Stable Release

Locks the public API and ships stable package releases with production-ready guarantees.

- [ ] **API freeze and compatibility enforcement** - Prevent breaking API changes without a major version bump.
- [ ] **NuGet GA release** - Publish stable `Deveel.Messaging.*` packages without prerelease suffixes.
- [ ] **Interactive content model** - Add cross-channel abstractions for buttons, quick replies, carousels, and lists.
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
