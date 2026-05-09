# Framework Overview

`deveel.messaging` gives you a single programming model for multi-channel messaging.

Instead of coupling your app to provider SDKs, you code once against the framework abstractions and swap connectors when needed.

## Mental model

1. Build an `IMessage`
2. Send or receive through an `IChannelConnector`
3. Handle a `ConnectorResult<T>`

This stays the same for SMS, email, push, and chat connectors.

## Core building blocks

- `IMessage`: sender, receiver, content, and properties
- `EndpointType`: typed endpoint categories such as `EmailAddress`, `PhoneNumber`, `DeviceId`, `Topic`
- `IChannelSchema`: capability and validation contract for a connector
- `IChannelConnector`: connector lifecycle plus send/receive operations
- `ConnectorResult<T>`: normalized success/failure response

## Package boundaries

- `Deveel.Messaging.Abstractions`: message model and endpoints
- `Deveel.Messaging.Connector.Abstractions`: connectors, schemas, validation contracts
- `Deveel.Messaging.Connectors`: DI helpers and registry integration
- `Deveel.Messaging.Connector.*`: provider implementations

## Minimal send flow

```csharp
var message = Message.Create()
    .From(PhoneEndpoint.Create("+15550001111"))
    .To(PhoneEndpoint.Create("+15550002222"))
    .WithText("Order confirmed")
    .Build();

var result = await connector.SendMessageAsync(message, ct);

if (!result.IsSuccess)
    throw new InvalidOperationException(result.Error?.Description ?? "Send failed");
```

## What belongs where

The framework handles message contracts, schema validation, and connector consistency.

Your application handles queueing, scheduling, persistence, retries, and business workflows.

## Next reads

- [Quick start](quick-start.md)
- [Installation and setup](installation-setup.md)
- [Channel schema usage](channelschema-usage.md)
- [Connector index](connectors/README.md)


