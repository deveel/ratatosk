# Deveel Messaging Framework

[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/badge/NuGet-Available-blue)](https://www.nuget.org/) [![codecov](https://codecov.io/gh/deveel/deveel.messaging/graph/badge.svg?token=0JV12EPQOD)](https://codecov.io/gh/deveel/deveel.messaging)


A modern, extensible messaging framework for .NET that provides a unified abstraction layer for various messaging providers including SMS, email, WhatsApp, and push notifications. The framework offers strong type safety, comprehensive validation, and flexible connector architecture.

## 🚀 Motivation

Modern applications often need to send notifications through multiple channels (SMS, email, WhatsApp, push notifications, webhooks). Each provider has different APIs, authentication methods, and message formats. The Deveel Messaging Framework solves this by:

- **Unified API**: Single interface for all messaging providers
- **Type Safety**: Strongly-typed endpoints and configurations prevent runtime errors
- **Extensibility**: Easy to add new connectors and message types
- **Validation**: Built-in message and configuration validation
- **Webhook Support**: Comprehensive webhook handling for message receiving and status updates
- **Testability**: Comprehensive mocking and testing support
- **Performance**: Async/await throughout with efficient resource usage

## 📦 Core Packages

| Package | Description | NuGet |
|---------|-------------|-------|
| `Deveel.Messaging.Abstractions` | Core messaging abstractions and models | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Abstractions.svg)](https://www.nuget.org/packages/Deveel.Messaging.Abstractions/) |
| `Deveel.Messaging.Connector.Abstractions` | Base classes and interfaces for connectors | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Abstractions.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Abstractions/) |

## 🔌 Available Connectors

| Connector | Provider | Type | Documentation | Package | Nuget |
|-----------|----------|------|---------------|---------|--------|
| **Twilio SMS** | Twilio | SMS | [📖 Guide](../docs/connectors/twilio-sms-connector.md) | `Deveel.Messaging.Connector.Twilio` | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Twilio.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Twilio/) |
| **Twilio WhatsApp** | Twilio | WhatsApp | [📖 Guide](../docs/connectors/twilio-whatsapp-connector.md) | `Deveel.Messaging.Connector.Twilio` | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Twilio.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Twilio/) |
| **Firebase FCM** | Firebase | Push | [📖 Guide](../docs/connectors/firebase-push-connector.md) | `Deveel.Messaging.Connector.Firebase` | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Firebase.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Firebase/) |
| **SendGrid Email** | SendGrid | Email | [📖 Guide](../docs/connectors/sendgrid-email-connector.md) | `Deveel.Messaging.Connector.Sendgrid` | [![NuGet](https://img.shields.io/nuget/v/Deveel.Messaging.Connector.Sendgrid.svg)](https://www.nuget.org/packages/Deveel.Messaging.Connector.Sendgrid/) |

> **📋 [Complete Connector Documentation](docs/connectors/README.md)** - Detailed installation, configuration, and usage guides for all connectors.

## 🔧 Quick Installation

### Core Framework
```bash
# Install core messaging abstractions
dotnet add package Deveel.Messaging.Abstractions

# For building custom connectors
dotnet add package Deveel.Messaging.Connector.Abstractions
```

### Connectors
Each connector has specific installation and setup instructions in its documentation:
- **📱 SMS**: [Twilio SMS Installation Guide](docs/connectors/twilio-sms-connector.md#installation)
- **💬 WhatsApp**: [Twilio WhatsApp Installation Guide](docs/connectors/twilio-whatsapp-connector.md#installation)  
- **🔔 Push**: [Firebase FCM Installation Guide](docs/connectors/firebase-push-connector.md#installation)
- **📧 Email**: [SendGrid Email Installation Guide](docs/connectors/sendgrid-email-connector.md#installation)

## 🏁 Quick Start

### 1. Choose Your Messaging Provider

Pick the connector that matches your needs from the [connector documentation](docs/connectors/README.md).

### 2. Basic Usage Pattern

All connectors follow the same pattern:

```csharp
using Deveel.Messaging;

// 1. Define a schema
var schema = new ChannelSchema("Provider", "Type", "1.0.0")
    .WithCapabilities(ChannelCapability.SendMessages)
    .AllowsMessageEndpoint(EndpointType.EmailAddress)
    .AddContentType(MessageContentType.PlainText);

// 2. Create and configure connector
var connector = new ProviderConnector(schema);
await connector.InitializeAsync(cancellationToken);

// 3. Build and send message
var message = new MessageBuilder()
    .WithId("msg-001")
    .WithEmailSender("sender@company.com")
    .WithEmailReceiver("user@example.com")
    .WithTextContent("Hello from our service!")
    .Message;

var result = await connector.SendMessageAsync(message, cancellationToken);
if (result.IsSuccess)
{
    Console.WriteLine($"Message sent: {result.Value?.MessageId}");
}
```

### 3. Provider-Specific Examples

Each connector has detailed examples in its documentation:

- **[📱 Twilio SMS Examples](docs/connectors/twilio-sms-connector.md#usage-examples)**
- **[💬 WhatsApp Business Examples](docs/connectors/twilio-whatsapp-connector.md#usage-examples)**
- **[🔔 Firebase Push Examples](docs/connectors/firebase-push-connector.md#usage-examples)**
- **[📧 SendGrid Email Examples](docs/connectors/sendgrid-email-connector.md#usage-examples)**

## 🎯 Core Features

### Strongly-Typed Endpoints
```csharp
// Type-safe endpoint creation
var emailEndpoint = Endpoint.EmailAddress("user@example.com");
var phoneEndpoint = Endpoint.PhoneNumber("+1234567890");
var deviceEndpoint = Endpoint.DeviceId("firebase-device-token");
```

### Flexible Content Types
```csharp
// Rich content support
.WithHtmlContent("<h1>Welcome!</h1>")
.WithTemplateContent("template-name", new { user = "John" })
.WithMediaContent("https://example.com/image.jpg", "image/jpeg")
```

### Webhook Integration
```csharp
// Bidirectional messaging
[HttpPost("webhook/provider")]
public async Task<IActionResult> ReceiveMessage([FromForm] Dictionary<string, string> data)
{
    var messageSource = MessageSource.FromFormData(data);
    var result = await connector.ReceiveMessagesAsync(messageSource, cancellationToken);
    return Ok();
}
```

### Error Handling
```csharp
var result = await connector.SendMessageAsync(message, cancellationToken);
if (!result.IsSuccess)
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
    // Handle specific error cases
}
```

## 📚 Documentation

### Getting Started
- **[📖 Getting Started Guide](../docs/getting-started.md)** - Step-by-step setup and first message
- **[🔌 Connector Documentation](../docs/connectors/README.md)** - Complete connector guides

### Framework Guides
- **[🏗️ Channel Schema Guide](../docs/ChannelSchema-Usage.md)** - Schema configuration
- **[⚡ Connector Implementation](../docs/ChannelConnector-Usage.md)** - Building custom connectors
- **[🎯 Endpoint Types](../docs/EndpointType-Usage.md)** - Type-safe endpoint usage
- **[🚀 Advanced Configuration](../docs/advanced-configuration.md)** - Production patterns

### Provider-Specific
- **[📱 Twilio SMS](../docs/connectors/twilio-sms-connector.md)** - SMS messaging with webhooks
- **[💬 Twilio WhatsApp](../docs/connectors/twilio-whatsapp-connector.md)** - WhatsApp Business integration
- **[🔔 Firebase FCM](../docs/connectors/firebase-push-connector.md)** - Push notifications
- **[📧 SendGrid Email](../docs/connectors/sendgrid-email-connector.md)** - Email delivery

## 🌟 Latest Features

- **🔥 Firebase Cloud Messaging** - Complete FCM connector with multicast support
- **💬 Enhanced WhatsApp Business** - Interactive elements, templates, and media
- **📞 Two-Way SMS** - Webhook support for incoming messages and status updates
- **📊 Batch Processing** - Efficient bulk operations across all connectors
- **🛡️ Health Monitoring** - Built-in connection testing and diagnostics

## 🧪 Testing

The framework includes comprehensive test suites with over 500 tests:

```bash
# Run all tests
dotnet test

# Run tests for specific areas
dotnet test test/Deveel.Messaging.Abstractions.XUnit
dotnet test test/Deveel.Messaging.Connector.Twilio.XUnit
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup
```bash
# Clone and build
git clone https://github.com/deveel/deveel.messaging.git
cd deveel.messaging
dotnet build
dotnet test
```

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

- **📖 Documentation**: [docs/README.md](docs/README.md)
- **🐛 Issues**: [GitHub Issues](https://github.com/deveel/deveel.messaging/issues)
- **💬 Discussions**: [GitHub Discussions](https://github.com/deveel/deveel.messaging/discussions)
- **📧 Email**: support@deveel.com

---

*Built with ❤️ by the Deveel team*

