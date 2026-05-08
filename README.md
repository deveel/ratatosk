[![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Abstractions.svg?label=NuGet)](https://www.nuget.org/packages/Deveel.Messaging.Abstractions/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4)](https://dotnet.microsoft.com/)
[![codecov](https://codecov.io/gh/deveel/deveel.messaging/graph/badge.svg)](https://codecov.io/gh/deveel/deveel.messaging)

## Deveel Messaging Framework

The Deveel Messaging Framework is a .NET library for sending and receiving messages across multiple channels — SMS, email, push notifications, chat platforms — without tying your code to any specific provider.

It gives you the core abstractions, a connector contract, and DI integration. Provider-specific packages plug in on top. What you write against the framework stays the same whether the message goes out through Twilio, SendGrid, Firebase, or anything else.

Routing, scheduling, and queueing are out of scope — those live in your application. This framework's job is to make sure every connector looks the same and every API call has a predictable result.

---

## Motivation

Every provider has its own SDK, its own authentication dance, its own quirky payload format. When your app needs SMS *and* email *and* push, you end up gluing together three different libraries that have nothing in common.

This framework replaces all of that with a single `IChannelConnector` contract. You send an `IMessage`, you get back a typed result — whether it went through a Twilio REST call, an SMTP handshake, or an FCM payload is none of your business. Swapping or adding a provider is a one-line DI change, not a refactor.

---

## Packages

| Package | Description | NuGet |
|---|---|---|
| `Deveel.Messaging.Abstractions` | Core message model, endpoints, and content types | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Abstractions.svg)](https://www.nuget.org/packages/Deveel.Messaging.Abstractions/) |
| `Deveel.Messaging.Connector.Abstractions` | Connector contract, channel schema, and base classes | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Abstractions.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Abstractions/) |
| `Deveel.Messaging.Connectors` | DI registration and connector registry | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connectors.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connectors/) |
| `Deveel.Messaging.Connector.Twilio` | Twilio SMS and WhatsApp connector | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Twilio.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Twilio/) |
| `Deveel.Messaging.Connector.Sendgrid` | SendGrid email connector | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Sendgrid.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Sendgrid/) |
| `Deveel.Messaging.Connector.Facebook` | Facebook Messenger connector | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Facebook.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Facebook/) |
| `Deveel.Messaging.Connector.Firebase` | Firebase Cloud Messaging connector | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Firebase.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Firebase/) |
| `Deveel.Messaging.Connector.Telegram` | Telegram Bot connector | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Telegram.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Telegram/) |

---

## Usage

### Register a connector

```csharp
builder.Services
    .AddTwilioConnector(options =>
    {
        options.AccountSid = configuration["Twilio:AccountSid"];
        options.AuthToken  = configuration["Twilio:AuthToken"];
    });
```

### Send a message

```csharp
public class NotificationService(IChannelConnector connector)
{
    public async Task SendSmsAsync(string to, string text, CancellationToken ct)
    {
        var message = Message.Create()
            .From(PhoneEndpoint.Create("+15550001111"))
            .To(PhoneEndpoint.Create(to))
            .WithText(text)
            .Build();

        var result = await connector.SendMessageAsync(message, ct);

        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error.Description);
    }
}
```

### Receive messages

```csharp
var received = await connector.ReceiveMessagesAsync(ct);

foreach (var message in received.Messages)
{
    Console.WriteLine($"From: {message.From}  Body: {message.Content}");
}
```

For full configuration options, content types, and connector-specific details see the [documentation](docs/README.md).

---

## Roadmap

The framework is currently at **v0.3.1**. Here's what's coming — check [ROADMAP.md](ROADMAP.md) for the full story behind each item.

**v0.4.0 — Framework Foundations**
- [ ] Test Coverage Target (≥ 80%)
- [ ] CI/CD Pipeline Hardening
- [ ] Structured Logging Improvements
- [ ] Documentation

**v0.5.0 — Inbound Messaging**
- [ ] Twilio Inbound Messages (SMS & WhatsApp)
- [ ] SendGrid Inbound Messages (Inbound Parse)
- [ ] Firebase Inbound Messages (Data & Notification Messages)

**v1.0.0 — First Stable Release**
- [ ] API Freeze
- [ ] NuGet GA Release
- [ ] Interactive Content
- [ ] Sender Identity Model

**v1.1.0 — Resilience & Observability**
- [ ] Retry Policies
- [ ] OpenTelemetry Tracing & Metrics
- [ ] Health Checks
- [ ] Connector-Level Timeout Configuration

**v1.2.0 — New SaaS Connectors**
- [ ] Slack Connector
- [ ] Microsoft Teams Connector
- [ ] WhatsApp Business API Connector (Direct Cloud API)
- [ ] Viber Business Connector
- [ ] LINE Connector

**v1.3.0 — Protocol Connectors**
- [ ] Protocol Connector Base Classes
- [ ] SMPP Connector
- [ ] SMTP Connector
- [ ] RCS Connector
- [ ] APNs Connector (Direct)

**v1.4.0 — Content Adaptation & Transcoding**
- [ ] `IContentTranscoder` Abstraction
- [ ] Built-In Transcoders
- [ ] Channel-Aware Content Fallback
- [ ] SMS Segmentation
- [ ] Character Encoding Detection

**v1.5.0 — Address & Number Validation**
- [ ] E.164 Normalization
- [ ] HLR (Home Location Register) Lookup
- [ ] Number Portability Awareness
- [ ] Email Address Validation
- [ ] `IAddressValidator` Abstraction

**v1.6.0 — Tooling & Instrumentation**
- [ ] `dotnet new` Connector Scaffold
- [ ] ASP.NET Core Diagnostic Middleware

**v2.0.0 — Conversations**
- [ ] `IConversation` Abstraction
- [ ] Conversation State Model
- [ ] Conversation Correlation
- [ ] Multi-Channel Conversations
- [ ] Multi-Tenant Channel Registry
- [ ] xUnit Test Helpers for Conversation Flows

**v2.1.0 — Message Templates**
- [ ] `IMessageTemplate` Abstraction
- [ ] Variable Substitution Engine
- [ ] Per-Connector Template Rendering
- [ ] Template Registry

---

## Contributing

Found a bug? Have an idea for a new connector? Want to help move a milestone forward? Open an issue or a pull request on [GitHub](https://github.com/deveel/deveel.messaging) — all contributions are welcome.

Check [CONTRIBUTING.md](CONTRIBUTING.md) for how to set up the dev environment and what we look for in a PR.

---

## License

This project is licensed under the [MIT License](LICENSE).

---

## Contributors

[![Contributors](https://contrib.rocks/image?repo=deveel/deveel.messaging)](https://github.com/deveel/deveel.messaging/graphs/contributors)

