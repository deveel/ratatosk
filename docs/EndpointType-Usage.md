# Endpoint Types

`EndpointType` keeps sender and receiver addresses strongly typed.

This avoids stringly-typed mistakes and makes schema validation much clearer.

## Common endpoint types

- `EmailAddress`
- `PhoneNumber`
- `Url`
- `Topic`
- `UserId`
- `ApplicationId`
- `DeviceId`
- `Id`, `Label`, `Any`

## Schema-side configuration

```csharp
var schema = new ChannelSchema("Provider", "Type", "1.0.0")
    .AllowsMessageEndpoint(EndpointType.EmailAddress, asSender: true, asReceiver: true)
    .AllowsMessageEndpoint(EndpointType.PhoneNumber, asSender: true, asReceiver: false)
    .AllowsMessageEndpoint(EndpointType.Url, asSender: false, asReceiver: true);
```

Use `AllowsAnyMessageEndpoint()` only when you really need a wildcard endpoint model.

## Message-side usage

```csharp
var message = new MessageBuilder()
    .WithEmailSender("sender@example.com")
    .WithPhoneReceiver("+15550002222")
    .WithTextContent("Hello")
    .Message;
```

You can also use generic endpoint methods when needed:

```csharp
.WithSender(EndpointType.UserId, "user-42")
.WithReceiver(EndpointType.ApplicationId, "app-main")
```

## Validation behavior

If a message endpoint type is not allowed by the schema (or direction is wrong), `ValidateMessage` returns an error before provider calls are made.

## Migration note

If old code still uses string endpoint types, migrate gradually to enum-based APIs and typed builder helpers. The end state should be enum-first everywhere.

## Related docs

- [Channel schema usage](channelschema-usage.md)
- [Message validation examples](validatemessage-usage-examples.md)
